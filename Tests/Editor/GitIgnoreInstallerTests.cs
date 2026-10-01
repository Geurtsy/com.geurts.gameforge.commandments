// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Geurts.GameForge.Documentation.Tests
{
    internal sealed class GitIgnoreInstallerTests
    {
        private string _projectRoot;
        private string _target;
        private string _documentPath;
        private string _manifestPath;
        private static readonly UTF8Encoding _utf8 = new UTF8Encoding(false);

        [SetUp]
        public void SetUp()
        {
            _projectRoot = Path.Combine(Path.GetTempPath(), "GeurtsGitIgnoreTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_projectRoot);
            _target = Path.Combine(_projectRoot, ".gitignore");
            string documentation = Path.Combine(_projectRoot, DocumentationPackageConstants.ManagedDocumentationDirectory);
            _documentPath = Path.Combine(documentation, GitIgnoreTemplateReader.TechniquePath);
            _manifestPath = Path.Combine(documentation, "GeurtsTechniqueManifest.md");
        }

        [TearDown]
        public void TearDown()
        {
            DocumentationFileOperations.DeleteDirectoryBestEffort(_projectRoot);
        }

        [Test]
        public void DifferingExistingFileIsPreservedWithoutConfirmation()
        {
            CopyDocumentationFixture();
            WriteExistingTarget();
            byte[] before = File.ReadAllBytes(_target);
            DateTime timestamp = File.GetLastWriteTimeUtc(_target);
            string warning = null;

            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(_projectRoot, message =>
            {
                warning = message;
                return false;
            }), Is.False);

            Assert.That(warning, Is.Null);
            Assert.That(File.ReadAllBytes(_target), Is.EqualTo(before));
            Assert.That(File.GetLastWriteTimeUtc(_target), Is.EqualTo(timestamp));
            Assert.That(BuildForgeIntegration.IsGitIgnoreInstalled(_projectRoot), Is.False);
        }

        [TestCase("LF")]
        [TestCase("CRLF")]
        [TestCase("CR")]
        [TestCase("mixed")]
        public void ApprovedTextWithDifferentNewlinesIsCompleteWithoutRewriting(string variant)
        {
            CopyDocumentationFixture();
            string payload = _utf8.GetString(BuildForgeIntegration.LoadGitIgnore(_projectRoot));
            string newline = variant == "CRLF" ? "\r\n" : variant == "CR" ? "\r" : "\n";
            string contents = variant == "mixed" ? payload.Replace("\n#", "\r\n#") : payload.Replace("\n", newline);
            File.WriteAllBytes(_target, _utf8.GetBytes(contents));
            File.SetLastWriteTimeUtc(_target, new DateTime(2024, 1, 2, 3, 4, 6, DateTimeKind.Utc));
            byte[] before = File.ReadAllBytes(_target);
            DateTime timestamp = File.GetLastWriteTimeUtc(_target);

            Assert.That(BuildForgeIntegration.IsGitIgnoreInstalled(_projectRoot), Is.True);
            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(_projectRoot,
                _ => throw new Exception("Equivalent existing text needs no confirmation.")), Is.True);
            Assert.That(File.ReadAllBytes(_target), Is.EqualTo(before));
            Assert.That(File.GetLastWriteTimeUtc(_target), Is.EqualTo(timestamp));
        }

        [TestCase("rule")]
        [TestCase("comment")]
        [TestCase("order")]
        [TestCase("whitespace")]
        [TestCase("missing-terminal-newline")]
        [TestCase("extra-terminal-newline")]
        [TestCase("bom")]
        [TestCase("invalid-utf8")]
        [TestCase("utf16")]
        public void NewlineComparisonDoesNotAcceptOrRewriteOtherDifferences(string difference)
        {
            CopyDocumentationFixture();
            string payload = _utf8.GetString(BuildForgeIntegration.LoadGitIgnore(_projectRoot));
            string contents = payload;
            switch (difference)
            {
                case "rule": contents = payload.Replace(".geurts/", "private/"); break;
                case "comment": contents = payload.Replace("# Siegefall / Geurts Unity Project .gitignore", "# Custom rules"); break;
                case "order": contents = payload.Replace(".utmp/\n/[Ll]ibrary/", "/[Ll]ibrary/\n.utmp/"); break;
                case "whitespace": contents = payload.Replace(".geurts/", ".geurts/ "); break;
                case "missing-terminal-newline": contents = payload.TrimEnd('\n'); break;
                case "extra-terminal-newline": contents = payload + "\n"; break;
                case "bom": contents = "\uFEFF" + payload; break;
            }
            byte[] before = difference == "invalid-utf8" ? new byte[] { 0xc3, 0x28 } :
                difference == "utf16" ? Encoding.Unicode.GetBytes(contents) : _utf8.GetBytes(contents);
            File.WriteAllBytes(_target, before);
            File.SetLastWriteTimeUtc(_target, new DateTime(2024, 1, 2, 3, 4, 6, DateTimeKind.Utc));
            DateTime timestamp = File.GetLastWriteTimeUtc(_target);

            Assert.That(BuildForgeIntegration.IsGitIgnoreInstalled(_projectRoot), Is.False);
            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(_projectRoot,
                _ => throw new Exception("Different existing content must be preserved without confirmation.")), Is.False);
            Assert.That(File.ReadAllBytes(_target), Is.EqualTo(before));
            Assert.That(File.GetLastWriteTimeUtc(_target), Is.EqualTo(timestamp));
        }

        [Test]
        public void CancelDoesNotRequireDocumentationOrCreateAMissingTarget()
        {
            string missingProject = Path.Combine(_projectRoot, "does-not-exist");
            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(missingProject, _ => false), Is.False);
            Assert.That(Directory.Exists(missingProject), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingDocumentationDoesNotCreateOrOverwriteTarget(bool existing)
        {
            if (existing)
            {
                WriteExistingTarget();
            }

            Assert.That(() => GitIgnoreInstaller.InstallWithConfirmation(_projectRoot, _ => true),
                Throws.TypeOf<InvalidDataException>().With.Message.Contains("Update Geurts Game Forge Commandments"));
            Assert.That(File.Exists(_target), Is.EqualTo(existing));
            if (existing)
            {
                Assert.That(File.ReadAllText(_target), Is.EqualTo("# custom rules\r\nkeep-private/\r\n"));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ConfirmedInstallUsesTheActualDocumentationPayload(bool crlfWithBom)
        {
            CopyDocumentationFixture();
            foreach (string file in new[] { _documentPath, _manifestPath })
            {
                string text = File.ReadAllText(file).Replace("\r\n", "\n");
                if (crlfWithBom)
                {
                    text = text.Replace("\n", "\r\n");
                }
                File.WriteAllText(file, text, new UTF8Encoding(crlfWithBom));
            }
            string sentinel = Path.Combine(_projectRoot, "unrelated.txt");
            File.WriteAllText(sentinel, "user content");
            byte[] documentBefore = File.ReadAllBytes(_documentPath);
            byte[] manifestBefore = File.ReadAllBytes(_manifestPath);

            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(_projectRoot, _ => true), Is.True);

            byte[] installed = File.ReadAllBytes(_target);
            using (SHA256 sha256 = SHA256.Create())
            {
                Assert.That(BitConverter.ToString(sha256.ComputeHash(installed)).Replace("-", "").ToLowerInvariant(),
                    Is.EqualTo("4d9408d141d128225e02615c7e72031d53153f5dfcc9427305a6c651689008d4"));
            }
            string payload = _utf8.GetString(installed);
            Assert.That(payload.Count(character => character == '\n'), Is.EqualTo(404));
            Assert.That(payload, Does.Not.Contain("\r").And.Not.Contain("GEURTS-GITIGNORE-BEGIN").And.Not.Contain("```"));
            Assert.That(installed.Take(3), Is.Not.EqualTo(new byte[] { 0xef, 0xbb, 0xbf }));
            Assert.That(File.ReadAllText(sentinel), Is.EqualTo("user content"));
            Assert.That(File.ReadAllBytes(_documentPath), Is.EqualTo(documentBefore));
            Assert.That(File.ReadAllBytes(_manifestPath), Is.EqualTo(manifestBefore));
            Assert.That(BuildForgeIntegration.LoadGitIgnore(_projectRoot), Is.EqualTo(installed));
            Assert.That(BuildForgeIntegration.IsGitIgnoreInstalled(_projectRoot), Is.True);
            DateTime timestamp = File.GetLastWriteTimeUtc(_target);
            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(_projectRoot,
                _ => throw new Exception("An identical existing file needs no confirmation.")), Is.True);
            Assert.That(File.ReadAllBytes(_target), Is.EqualTo(installed));
            Assert.That(File.GetLastWriteTimeUtc(_target), Is.EqualTo(timestamp));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LegacyTemplateStillInstallsOrMatchesAnExistingOriginal(bool existing)
        {
            CopyDocumentationFixture();
            byte[] legacy = UseLegacyDocumentationFixture();
            Assert.That(ComputeHash(legacy),
                Is.EqualTo("7223a9449718942d3a5cad00cf4d4e0dee9c89eb64951541fa4ebfb803acb45b"));
            byte[] expected = existing ? _utf8.GetBytes(_utf8.GetString(legacy).Replace("\n", "\r\n")) : legacy;
            if (existing)
            {
                File.WriteAllBytes(_target, expected);
                File.SetLastWriteTimeUtc(_target, new DateTime(2024, 1, 2, 3, 4, 6, DateTimeKind.Utc));
            }
            DateTime timestamp = existing ? File.GetLastWriteTimeUtc(_target) : default;

            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(_projectRoot, _ =>
            {
                Assert.That(existing, Is.False, "Matching legacy text needs no confirmation.");
                return true;
            }), Is.True);

            Assert.That(File.ReadAllBytes(_target), Is.EqualTo(expected));
            if (existing) Assert.That(File.GetLastWriteTimeUtc(_target), Is.EqualTo(timestamp));
            Assert.That(BuildForgeIntegration.IsGitIgnoreInstalled(_projectRoot), Is.True);
        }

        [Test]
        public void UpdatingDocumentationDoesNotReplaceAnExistingLegacyIgnoreFile()
        {
            CopyDocumentationFixture();
            byte[] legacy = _utf8.GetBytes(string.Join("\n",
                _utf8.GetString(BuildForgeIntegration.LoadGitIgnore(_projectRoot)).Split('\n').Take(376)) + "\n");
            File.WriteAllBytes(_target, legacy);
            File.SetLastWriteTimeUtc(_target, new DateTime(2024, 1, 2, 3, 4, 6, DateTimeKind.Utc));
            DateTime timestamp = File.GetLastWriteTimeUtc(_target);

            Assert.That(BuildForgeIntegration.IsGitIgnoreInstalled(_projectRoot), Is.False);
            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(_projectRoot,
                _ => throw new Exception("A changed template must preserve the existing file without confirmation.")), Is.False);
            Assert.That(File.ReadAllBytes(_target), Is.EqualTo(legacy));
            Assert.That(File.GetLastWriteTimeUtc(_target), Is.EqualTo(timestamp));
        }

        [TestCase("payload")]
        [TestCase("duplicate-marker")]
        [TestCase("fence")]
        [TestCase("manifest-version")]
        [TestCase("technique-version")]
        [TestCase("unsupported-version")]
        [TestCase("legacy-version-with-current-payload")]
        [TestCase("self-declared-hash")]
        [TestCase("invalid-utf8")]
        public void InvalidDocumentationPreservesExistingTargetBeforeReplacement(string fault)
        {
            CopyDocumentationFixture();
            WriteExistingTarget();
            string document = File.ReadAllText(_documentPath);
            switch (fault)
            {
                case "payload":
                    document = document.Replace(".geurts/", "changed-rule/");
                    break;
                case "duplicate-marker":
                    document += "\n<!-- GEURTS-GITIGNORE-END -->\n";
                    break;
                case "fence":
                    document = document.Replace("```gitignore", "```text");
                    break;
                case "manifest-version":
                    File.WriteAllText(_manifestPath, File.ReadAllText(_manifestPath).Replace(
                        "`GeurtsTechniques/GeurtsGitIgnoreTechnique.md` | 1.0.1",
                        "`GeurtsTechniques/GeurtsGitIgnoreTechnique.md` | 9.0.0"), _utf8);
                    break;
                case "technique-version":
                    document = document.Replace("**Version:** 1.0.1", "**Version:** 9.0.0");
                    break;
                case "unsupported-version":
                case "legacy-version-with-current-payload":
                    string version = fault == "unsupported-version" ? "1.0.2" : "1.0.0";
                    document = document.Replace("1.0.1", version);
                    File.WriteAllText(_manifestPath, File.ReadAllText(_manifestPath).Replace(
                        "`GeurtsTechniques/GeurtsGitIgnoreTechnique.md` | 1.0.1",
                        "`GeurtsTechniques/GeurtsGitIgnoreTechnique.md` | " + version), _utf8);
                    if (fault == "legacy-version-with-current-payload")
                        document = document.Replace(GitIgnoreTemplateReader.TemplateSha256,
                            "7223a9449718942d3a5cad00cf4d4e0dee9c89eb64951541fa4ebfb803acb45b");
                    break;
                case "self-declared-hash":
                    byte[] altered = _utf8.GetBytes(_utf8.GetString(BuildForgeIntegration.LoadGitIgnore(_projectRoot))
                        .Replace(".geurts/", "changed-rule/"));
                    document = document.Replace(".geurts/", "changed-rule/")
                        .Replace(GitIgnoreTemplateReader.TemplateSha256, ComputeHash(altered));
                    break;
            }
            File.WriteAllText(_documentPath, document, _utf8);
            if (fault == "invalid-utf8")
            {
                File.WriteAllBytes(_documentPath, new byte[] { 0xc3, 0x28 });
            }
            byte[] before = File.ReadAllBytes(_target);
            DateTime timestamp = File.GetLastWriteTimeUtc(_target);

            Assert.That(() => GitIgnoreInstaller.InstallWithConfirmation(_projectRoot, _ => true), Throws.Exception);

            Assert.That(File.ReadAllBytes(_target), Is.EqualTo(before));
            Assert.That(File.GetLastWriteTimeUtc(_target), Is.EqualTo(timestamp));
        }

        [Test]
        public void DirectoryAtTargetIsPreserved()
        {
            CopyDocumentationFixture();
            Directory.CreateDirectory(_target);
            string sentinel = Path.Combine(_target, "keep.txt");
            File.WriteAllText(sentinel, "preserve");

            Assert.Throws<IOException>(() => GitIgnoreInstaller.InstallWithConfirmation(_projectRoot, _ => true));
            Assert.That(File.ReadAllText(sentinel), Is.EqualTo("preserve"));
        }

        [Test]
        public void ExistingHardLinkedTargetAndOtherFileAreBothPreserved()
        {
            CopyDocumentationFixture();
            string otherFile = Path.Combine(_projectRoot, "other-file.txt");
            File.WriteAllText(otherFile, "unrelated original");
            Assert.That(CreateHardLink(_target, otherFile, IntPtr.Zero), Is.True,
                "Could not create hard-link fixture: " + Marshal.GetLastWin32Error());

            Assert.That(GitIgnoreInstaller.InstallWithConfirmation(_projectRoot, _ => true), Is.False);
            Assert.That(File.ReadAllText(otherFile), Is.EqualTo("unrelated original"));
            Assert.That(File.ReadAllText(_target), Is.EqualTo("unrelated original"));
        }

        [Test]
        public void TargetCreatedDuringConfirmationIsPreserved()
        {
            CopyDocumentationFixture();
            Assert.Throws<IOException>(() => GitIgnoreInstaller.InstallWithConfirmation(_projectRoot, message =>
            {
                Assert.That(message, Does.Contain(_target).And.Contain("preserved unchanged"));
                WriteExistingTarget();
                return true;
            }));
            Assert.That(File.ReadAllText(_target), Is.EqualTo("# custom rules\r\nkeep-private/\r\n"));
        }

        [Test]
        public void MissingFileRemainsIncompleteUntilVerified()
        {
            CopyDocumentationFixture();
            Assert.That(BuildForgeIntegration.IsGitIgnoreInstalled(_projectRoot), Is.False);
            Assert.That(File.Exists(_target), Is.False);
        }

        private void WriteExistingTarget()
        {
            File.WriteAllText(_target, "# custom rules\r\nkeep-private/\r\n", _utf8);
            File.SetLastWriteTimeUtc(_target, new DateTime(2024, 1, 2, 3, 4, 6, DateTimeKind.Utc));
        }

        private byte[] UseLegacyDocumentationFixture()
        {
            string current = _utf8.GetString(BuildForgeIntegration.LoadGitIgnore(_projectRoot));
            string legacy = string.Join("\n", current.Split('\n').Take(376)) + "\n";
            string document = File.ReadAllText(_documentPath).Replace("\r\n", "\n")
                .Replace(current, legacy).Replace("1.0.1", "1.0.0")
                .Replace(GitIgnoreTemplateReader.TemplateSha256,
                    "7223a9449718942d3a5cad00cf4d4e0dee9c89eb64951541fa4ebfb803acb45b")
                .Replace("`404`", "`376`")
                .Replace("1af49cb7916167b1dad80b762ba5e545fc727f609970a268069027f997658216",
                    "c8412a38435bccd89f1fefb855da3c24680612c27609341e64dcbaf99c0f88ab");
            File.WriteAllText(_documentPath, document, _utf8);
            File.WriteAllText(_manifestPath, File.ReadAllText(_manifestPath).Replace(
                "`GeurtsTechniques/GeurtsGitIgnoreTechnique.md` | 1.0.1",
                "`GeurtsTechniques/GeurtsGitIgnoreTechnique.md` | 1.0.0"), _utf8);
            return _utf8.GetBytes(legacy);
        }

        private static string ComputeHash(byte[] bytes)
        {
            using (SHA256 sha256 = SHA256.Create())
                return BitConverter.ToString(sha256.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private void CopyDocumentationFixture()
        {
            string source = Path.Combine(Path.GetDirectoryName(Application.dataPath),
                DocumentationPackageConstants.ManagedDocumentationDirectory);
            if (!File.Exists(Path.Combine(source, GitIgnoreTemplateReader.TechniquePath)))
            {
                Assert.Ignore("The integration fixture requires installed documentation. " +
                              "Run Tools/ValidatePackage.ps1 with -DocumentationPath pointing to GeurtsGameForgeCommandments.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_documentPath));
            File.Copy(Path.Combine(source, GitIgnoreTemplateReader.TechniquePath), _documentPath);
            File.Copy(Path.Combine(source, "GeurtsTechniqueManifest.md"), _manifestPath);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateHardLinkW")]
        private static extern bool CreateHardLink(string fileName, string existingFileName, IntPtr securityAttributes);
    }
}
