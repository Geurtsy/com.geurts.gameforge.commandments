// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace Geurts.GameForge.Documentation
{
    internal interface IDocumentationTransport
    {
        Task<string> ResolveHeadCommitAsync(CancellationToken cancellationToken);

        Task<string> ReadVersionAsync(string commit, CancellationToken cancellationToken);

        Task<DocumentationDownload> DownloadCommitAsync(
            string commit,
            string workingDirectory,
            CancellationToken cancellationToken, Action<UpdateProgress> progress = null);
    }

    internal sealed class GitHubDocumentationTransport : IDocumentationTransport, IDisposable
    {
        private readonly HttpClient _client;

        internal GitHubDocumentationTransport(HttpMessageHandler handler = null)
        {
            _client = handler == null ? new HttpClient() : new HttpClient(handler);
            _client.Timeout = TimeSpan.FromSeconds(90);
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(
                DocumentationPackageConstants.PackageName);
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/x-git-upload-pack-advertisement");
            _client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
        }

        /// <summary>Resolves main through bounded Git reference discovery without the GitHub REST API.</summary>
        public async Task<string> ResolveHeadCommitAsync(CancellationToken cancellationToken)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(
                       HttpMethod.Get,
                       DocumentationPackageConstants.HeadCommitAdvertisementUrl))
            using (HttpResponseMessage response = await _client.SendAsync(
                       request,
                       HttpCompletionOption.ResponseHeadersRead,
                       cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentType?.MediaType != "application/x-git-upload-pack-advertisement")
                    throw new InvalidDataException("The documentation source did not return a Git reference advertisement.");
                byte[] bytes = await ReadBoundedAsync(
                    response,
                    DocumentationPackageConstants.MetadataLimitBytes,
                    cancellationToken);
                return ParseHeadCommitMetadata(bytes);
            }
        }

        /// <summary>Reads only the documentation version from the manifest at the resolved commit.</summary>
        public async Task<string> ReadVersionAsync(string commit, CancellationToken cancellationToken)
        {
            using (var metadata = new GitVersionMetadata())
            {
                string manifest = await metadata.ReadTextAsync(
                    "https://raw.githubusercontent.com/Geurtsy/GeurtsGameForge_Commandments/" +
                    Uri.EscapeDataString(commit) + "/" + GitVersionMetadata.ManifestPath, cancellationToken);
                return GitVersionMetadata.ParseDocumentationVersion(manifest);
            }
        }

        public async Task<DocumentationDownload> DownloadCommitAsync(
            string commit,
            string workingDirectory,
            CancellationToken cancellationToken, Action<UpdateProgress> progress = null)
        {
            if (string.IsNullOrWhiteSpace(commit) ||
                !Regex.IsMatch(commit, "^[0-9a-f]{40}$", RegexOptions.CultureInvariant))
            {
                throw new ArgumentException("A resolved 40-character commit identity is required.", nameof(commit));
            }

            Directory.CreateDirectory(workingDirectory);
            string archivePath = Path.Combine(workingDirectory, "documentation.zip");
            string extractionRoot = Path.Combine(workingDirectory, "extracted");
            Directory.CreateDirectory(extractionRoot);

            string archiveUrl = string.Format(
                DocumentationPackageConstants.ArchiveUrlFormat,
                Uri.EscapeDataString(commit));

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, archiveUrl))
            using (HttpResponseMessage response = await _client.SendAsync(
                       request,
                       HttpCompletionOption.ResponseHeadersRead,
                       cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength.HasValue &&
                    response.Content.Headers.ContentLength.Value > DocumentationPackageConstants.ArchiveLimitBytes)
                {
                    throw new InvalidDataException("The documentation archive exceeds the download limit.");
                }

                using (Stream input = await response.Content.ReadAsStreamAsync())
                using (FileStream output = new FileStream(
                           archivePath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None,
                           81920,
                           true))
                {
                    await CopyBoundedAsync(
                        input,
                        output,
                        DocumentationPackageConstants.ArchiveLimitBytes,
                        cancellationToken,
                        total =>
                        {
                            long? length = response.Content.Headers.ContentLength;
                            string amount = (total / 1048576f).ToString("0.0") + " MB";
                            string expected = length.HasValue ? " of " + (length.Value / 1048576f).ToString("0.0") + " MB" : " (total size not supplied)";
                            progress?.Invoke(new UpdateProgress("Downloading documentation: " + amount + expected,
                                length.GetValueOrDefault() > 0 ? (float)total / length.Value : -1f));
                        });
                }
            }

            progress?.Invoke(new UpdateProgress("Extracting the downloaded documentation archive..."));
            string candidateRoot = ExtractCommitArchive(archivePath, extractionRoot, commit);
            return new DocumentationDownload(workingDirectory, candidateRoot);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        internal static string ParseHeadCommitMetadata(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (bytes.Length > DocumentationPackageConstants.MetadataLimitBytes)
                throw new InvalidDataException("The Git advertisement exceeds the metadata limit.");
            int offset = 0;
            if (ReadGitPacket(bytes, ref offset)?.TrimEnd('\n') != "# service=git-upload-pack" ||
                ReadGitPacket(bytes, ref offset) != null)
                throw new InvalidDataException("The Git advertisement has an invalid service header.");
            string commit = null;
            bool firstReference = true;
            while (true)
            {
                string packet = ReadGitPacket(bytes, ref offset);
                if (packet == null) break;
                if (!packet.EndsWith("\n", StringComparison.Ordinal))
                    throw new InvalidDataException("The Git reference record is incomplete.");
                string record = packet.Substring(0, packet.Length - 1);
                int capabilities = record.IndexOf('\0');
                if (firstReference != (capabilities >= 0))
                    throw new InvalidDataException("The Git reference capabilities are misplaced.");
                firstReference = false;
                if (capabilities >= 0) record = record.Substring(0, capabilities);
                if (record.Length < 42 || record[40] != ' ' ||
                    !Regex.IsMatch(record.Substring(0, 40), "^[0-9a-fA-F]{40}$", RegexOptions.CultureInvariant) ||
                    record.Substring(41).IndexOfAny(new[] { ' ', '\r', '\n', '\0' }) >= 0)
                    throw new InvalidDataException("The Git reference record is invalid.");
                if (record.Substring(41) == "refs/heads/" + DocumentationPackageConstants.RepositoryBranch)
                {
                    if (commit != null || record.Substring(0, 40) == new string('0', 40))
                        throw new InvalidDataException("The documentation main reference is ambiguous or invalid.");
                    commit = record.Substring(0, 40).ToLowerInvariant();
                }
            }
            if (offset != bytes.Length || commit == null)
                throw new InvalidDataException("The Git advertisement has no unique main commit or contains trailing data.");
            return commit;
        }

        private static string ReadGitPacket(byte[] bytes, ref int offset)
        {
            if (bytes.Length - offset < 4 ||
                !int.TryParse(Encoding.ASCII.GetString(bytes, offset, 4), NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture, out int length))
                throw new InvalidDataException("The Git packet length is invalid.");
            offset += 4;
            if (length == 0) return null;
            if (length < 5 || length > 65520 || length - 4 > bytes.Length - offset)
                throw new InvalidDataException("The Git packet is truncated or invalid.");
            string packet = new UTF8Encoding(false, true).GetString(bytes, offset, length - 4);
            offset += length - 4;
            return packet;
        }

        internal static async Task<byte[]> ReadBoundedAsync(
            HttpResponseMessage response,
            long limit,
            CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentLength.HasValue &&
                response.Content.Headers.ContentLength.Value > limit)
            {
                throw new InvalidDataException("The remote metadata response exceeds the allowed size.");
            }

            using (Stream input = await response.Content.ReadAsStreamAsync())
            using (MemoryStream output = new MemoryStream())
            {
                await CopyBoundedAsync(input, output, limit, cancellationToken);
                return output.ToArray();
            }
        }

        private static async Task CopyBoundedAsync(
            Stream input,
            Stream output,
            long limit,
            CancellationToken cancellationToken, Action<long> progress = null)
        {
            byte[] buffer = new byte[81920];
            long total = 0;
            while (true)
            {
                int read = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (read == 0)
                {
                    return;
                }

                total += read;
                if (total > limit)
                {
                    throw new InvalidDataException("The remote response exceeds the allowed size.");
                }

                await output.WriteAsync(buffer, 0, read, cancellationToken);
                progress?.Invoke(total);
            }
        }

        internal static string ExtractCommitArchive(string archivePath, string extractionRoot, string commit)
        {
            string expectedWrapper = "GeurtsGameForge_Commandments-" + commit;
            long extractedBytes = 0;
            bool foundFile = false;

            using (FileStream archiveStream = File.OpenRead(archivePath))
            using (ZipArchive archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, false))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string archiveName = entry.FullName.Replace('\\', '/');
                    if (string.IsNullOrWhiteSpace(archiveName))
                    {
                        continue;
                    }

                    bool isDirectory = archiveName.EndsWith("/", StringComparison.Ordinal);
                    string normalizedArchiveName = isDirectory
                        ? archiveName.TrimEnd('/')
                        : archiveName;
                    string[] segments = normalizedArchiveName.Split('/');
                    if (segments.Length < 1 ||
                        !string.Equals(segments[0], expectedWrapper, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException("The archive is not the resolved documentation commit.");
                    }

                    if (IsSymbolicLink(entry))
                    {
                        throw new InvalidDataException("The documentation archive contains a symbolic link.");
                    }

                    if (segments.Length == 1)
                    {
                        if (!isDirectory)
                        {
                            throw new InvalidDataException("The archive contains a file in place of its wrapper directory.");
                        }

                        continue;
                    }

                    string relativePath = string.Join("/", segments, 1, segments.Length - 1);

                    string target = DocumentationFileOperations.GetSafeFullPath(extractionRoot, relativePath);
                    if (isDirectory)
                    {
                        Directory.CreateDirectory(target);
                        continue;
                    }

                    extractedBytes += entry.Length;
                    if (extractedBytes > DocumentationPackageConstants.ExtractedLimitBytes)
                    {
                        throw new InvalidDataException("The extracted documentation exceeds the allowed size.");
                    }

                    string parent = Path.GetDirectoryName(target);
                    if (!string.IsNullOrEmpty(parent))
                    {
                        Directory.CreateDirectory(parent);
                    }

                    if (File.Exists(target) || Directory.Exists(target))
                    {
                        throw new InvalidDataException("The documentation archive contains a duplicate path.");
                    }

                    using (Stream input = entry.Open())
                    using (FileStream output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        input.CopyTo(output);
                    }

                    foundFile = true;
                }
            }

            if (!foundFile)
            {
                throw new InvalidDataException("The documentation archive contains no files.");
            }

            DocumentationFileOperations.EnsureRegularTree(extractionRoot);
            return extractionRoot;
        }

        private static bool IsSymbolicLink(ZipArchiveEntry entry)
        {
            const int unixFileTypeMask = 0xF000;
            const int unixSymbolicLink = 0xA000;
            int unixMode = (entry.ExternalAttributes >> 16) & unixFileTypeMask;
            return unixMode == unixSymbolicLink;
        }

    }
}
