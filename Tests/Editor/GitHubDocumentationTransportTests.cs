// IMPORTANT: This script must comply with GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsFolderStructureTechnique.md.

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Geurts.GameForge.Documentation.Tests
{
    internal sealed class GitHubDocumentationTransportTests
    {
        private string temporaryRoot;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(
                Path.GetTempPath(),
                "GeurtsDocumentationArchiveTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            DocumentationFileOperations.DeleteDirectoryBestEffort(temporaryRoot);
        }

        private static string Packet(string text) => (Encoding.UTF8.GetByteCount(text) + 4).ToString("x4") + text;

        private static string Advertisement(string commit) =>
            Packet("# service=git-upload-pack\n") + "0000" +
            Packet(new string('b', 40) + " HEAD\0multi_ack symref=HEAD:refs/heads/main\n") +
            Packet(commit + " refs/heads/main\n") + "0000";

        [Test]
        public void GitReferenceDiscoverySelectsMainRatherThanHeadOrAnotherBranch()
        {
            string commit = new string('A', 40);
            string data = Advertisement(commit);
            data = data.Substring(0, data.Length - 4) + Packet(new string('c', 40) + " refs/heads/feature/é\n") + "0000";
            Assert.That(GitHubDocumentationTransport.ParseHeadCommitMetadata(Encoding.UTF8.GetBytes(data)),
                Is.EqualTo(commit.ToLowerInvariant()));
            Assert.That(DocumentationPackageConstants.HeadCommitAdvertisementUrl,
                Is.EqualTo(DocumentationPackageConstants.RepositoryUrl + "/info/refs?service=git-upload-pack"));
        }

        [TestCase("truncated")]
        [TestCase("trailing")]
        [TestCase("duplicate")]
        [TestCase("missing")]
        [TestCase("zero")]
        [TestCase("invalidsha")]
        [TestCase("service")]
        [TestCase("length")]
        [TestCase("capabilities")]
        [TestCase("oversized")]
        public void InvalidGitAdvertisementsCannotYieldAnInstallCommit(string fault)
        {
            string commit = new string('a', 40);
            string data = Advertisement(commit);
            switch (fault)
            {
                case "truncated": data = data.Substring(0, data.Length - 1); break;
                case "trailing": data += "extra"; break;
                case "duplicate": data = data.Substring(0, data.Length - 4) + Packet(commit + " refs/heads/main\n") + "0000"; break;
                case "missing": data = data.Replace("refs/heads/main\n", "refs/heads/else\n"); break;
                case "zero": data = Advertisement(new string('0', 40)); break;
                case "invalidsha": data = Advertisement(new string('g', 40)); break;
                case "service": data = data.Replace("git-upload-pack", "git-receive-pak"); break;
                case "length": data = "zzzz" + data.Substring(4); break;
                case "capabilities": data = data.Replace('\0', ' '); break;
                case "oversized": data = new string('a', DocumentationPackageConstants.MetadataLimitBytes + 1); break;
            }
            Assert.Throws<InvalidDataException>(() => GitHubDocumentationTransport.ParseHeadCommitMetadata(Encoding.UTF8.GetBytes(data)));
        }

        [Test]
        public async Task ResolutionUsesOnlyGitDiscoveryAndPropagatesCancellation()
        {
            using (var handler = new FixtureHandler(Advertisement(new string('a', 40))))
            using (var transport = new GitHubDocumentationTransport(handler))
            using (var cancellation = new CancellationTokenSource())
            {
                Assert.That(await transport.ResolveHeadCommitAsync(cancellation.Token), Is.EqualTo(new string('a', 40)));
                Assert.That(handler.Url, Is.EqualTo(DocumentationPackageConstants.HeadCommitAdvertisementUrl));
                Assert.That(handler.RequestCount, Is.EqualTo(1));
                cancellation.Cancel();
                Assert.CatchAsync<OperationCanceledException>(async () => await transport.ResolveHeadCommitAsync(cancellation.Token));
            }
        }

        [TestCase(403, "application/x-git-upload-pack-advertisement")]
        [TestCase(200, "text/html")]
        public void HttpFailureOrUnexpectedContentCannotYieldACommit(int status, string mediaType)
        {
            using (var handler = new FixtureHandler(Advertisement(new string('a', 40)), status, mediaType))
            using (var transport = new GitHubDocumentationTransport(handler))
            {
                Assert.CatchAsync<Exception>(async () => await transport.ResolveHeadCommitAsync(CancellationToken.None));
                Assert.That(handler.RequestCount, Is.EqualTo(1), "No automatic retry is permitted.");
            }
        }

        private sealed class FixtureHandler : HttpMessageHandler
        {
            private readonly string _body;
            private readonly int _status;
            private readonly string _mediaType;
            internal string Url { get; private set; }
            internal int RequestCount { get; private set; }
            internal FixtureHandler(string body, int status = 200, string mediaType = "application/x-git-upload-pack-advertisement")
            { _body = body; _status = status; _mediaType = mediaType; }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                Url = request.RequestUri.AbsoluteUri;
                RequestCount++;
                var response = new HttpResponseMessage((HttpStatusCode)_status) { Content = new StringContent(_body, Encoding.UTF8, _mediaType) };
                return Task.FromResult(response);
            }
        }

        [Test]
        public void ExtractCommitArchiveAcceptsGitHubDirectoryEntries()
        {
            string commit = new string('a', 40);
            string wrapper = "GeurtsGameForge_Documentation-" + commit;
            string archivePath = Path.Combine(temporaryRoot, "documentation.zip");
            string extractionRoot = Path.Combine(temporaryRoot, "extracted");
            Directory.CreateDirectory(extractionRoot);

            using (FileStream archiveStream = File.Create(archivePath))
            using (ZipArchive archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false))
            {
                archive.CreateEntry(wrapper + "/");
                archive.CreateEntry(wrapper + "/GeurtsTechniques/");
                ZipArchiveEntry contract = archive.CreateEntry(
                    wrapper + "/GeurtsTechniques/GeurtsDocumentationCompanionContract.json");
                using (StreamWriter writer = new StreamWriter(contract.Open()))
                {
                    writer.Write("{}");
                }
            }

            string candidate = GitHubDocumentationTransport.ExtractCommitArchive(
                archivePath,
                extractionRoot,
                commit);

            Assert.That(candidate, Is.EqualTo(extractionRoot));
            Assert.That(
                File.ReadAllText(Path.Combine(
                    extractionRoot,
                    "GeurtsTechniques",
                    "GeurtsDocumentationCompanionContract.json")),
                Is.EqualTo("{}"));
        }

        [Test]
        public void ExtractCommitArchiveRejectsTraversalBeforeWritingOutsideExtractionRoot()
        {
            string commit = new string('b', 40);
            string wrapper = "GeurtsGameForge_Documentation-" + commit;
            string archivePath = Path.Combine(temporaryRoot, "documentation.zip");
            string extractionRoot = Path.Combine(temporaryRoot, "extracted");
            string escapedPath = Path.Combine(temporaryRoot, "escaped.txt");
            Directory.CreateDirectory(extractionRoot);

            using (FileStream archiveStream = File.Create(archivePath))
            using (ZipArchive archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false))
            {
                ZipArchiveEntry traversal = archive.CreateEntry(wrapper + "/../escaped.txt");
                using (StreamWriter writer = new StreamWriter(traversal.Open()))
                {
                    writer.Write("must not escape");
                }
            }

            Assert.Throws<InvalidDataException>(() =>
                GitHubDocumentationTransport.ExtractCommitArchive(
                    archivePath,
                    extractionRoot,
                    commit));
            Assert.That(File.Exists(escapedPath), Is.False);
        }
    }
}
