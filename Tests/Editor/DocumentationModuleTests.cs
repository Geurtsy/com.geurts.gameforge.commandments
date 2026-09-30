// IMPORTANT: This script must comply with GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Geurts.GameForge.Documentation.Tests
{
    public sealed class DocumentationModuleTests
    {
        private bool _hadPreference, _original;
        [SetUp] public void SetUp()
        {
            _hadPreference = EditorPrefs.HasKey(DocumentationModule.PreferenceKey);
            _original = DocumentationIntegration.ModuleEnabled;
            DocumentationModule.SetEnabled(true);
        }
        [TearDown] public void TearDown()
        {
            if (_hadPreference) DocumentationModule.SetEnabled(_original);
            else EditorPrefs.DeleteKey(DocumentationModule.PreferenceKey);
        }
        [Test] public void PreferenceIsProjectSpecificAndEnablingStartsNoWork()
        {
            Assert.That(DocumentationModule.KeyForProject("C:/Forge/One"), Is.Not.EqualTo(DocumentationModule.KeyForProject("C:/Forge/Two")));
            DocumentationIntegration.SetModuleEnabled(false);
            Assert.That(DocumentationIntegration.ModuleEnabled, Is.False);
            Assert.That(EditorPrefs.GetBool(DocumentationModule.PreferenceKey, true), Is.False);
            Assert.That(DocumentationIntegration.StatusMessage, Does.Contain("off"));
            DocumentationIntegration.SetModuleEnabled(true);
            Assert.That(DocumentationIntegration.IsBusy, Is.False);
        }
        [Test] public async Task DisabledModuleDoesNotCheckOrConfirmAnUpdate()
        {
            var original = DocumentationUpdaterController.Service;
            var transport = new CountingTransport();
            int confirmations = 0;
            string root = Path.Combine(Path.GetTempPath(), "GeurtsDocumentationModule-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            DocumentationUpdaterController.Service = new DocumentationUpdateService(root, transport);
            DocumentationUpdaterController.ConfirmForTests = () => { confirmations++; return true; };
            try
            {
                DocumentationIntegration.SetModuleEnabled(false);
                await DocumentationIntegration.CheckForUpdatesAsync();
                await DocumentationIntegration.UpdateDocumentationAutomaticallyAsync(() => true);
                LogAssert.Expect(LogType.Warning, "[Geurts Documentation Companion] Documentation update could not start: Enable the Documentation Companion module first. Try Update again when Unity is ready.");
                DocumentationIntegration.UpdateDocumentation();
                Assert.That(confirmations + transport.Calls, Is.Zero);
                Assert.Throws<InvalidOperationException>(() => BuildForgeIntegration.InstallGitIgnore(root));
                Assert.That(Directory.GetFileSystemEntries(root), Is.Empty);
            }
            finally
            {
                DocumentationUpdaterController.Service = original; DocumentationUpdaterController.ConfirmForTests = null;
                Assert.That(Path.GetFullPath(root).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase), Is.True);
                Directory.Delete(root, true);
            }
        }
        [Test] public void ConflictingWorkBlocksPreferenceChange()
        {
            using (DocumentationIntegration.RegisterOperationGuard(() => "work in progress"))
                Assert.Throws<InvalidOperationException>(() => DocumentationIntegration.SetModuleEnabled(false));
            Assert.That(DocumentationIntegration.ModuleEnabled, Is.True);
        }
        private sealed class CountingTransport : IDocumentationTransport
        {
            internal int Calls;
            public Task<string> ResolveHeadCommitAsync(CancellationToken token) { Calls++; return Task.FromResult(new string('a', 40)); }
            public Task<string> ReadVersionAsync(string commit, CancellationToken token) { Calls++; return Task.FromResult("0.35.0"); }
            public Task<DocumentationDownload> DownloadCommitAsync(string commit, string directory, CancellationToken token, Action<UpdateProgress> progress)
            { Calls++; throw new InvalidOperationException("Disabled module must not download."); }
        }
    }
}
