// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Geurts.GameForge.Documentation.Tests
{
    /// <summary>Exercises Compatibility behavior with isolated inputs and owned fixture cleanup.</summary>
    public sealed class CompatibilityTests
    {
        /// <summary>Asserts restored Handoff Reconstruction Retains One Status And Reachable Owner Action using isolated fixture state.</summary>
        [Test] public void RestoredHandoffReconstructionRetainsOneStatusAndReachableOwnerAction()
        {
            var window = ScriptableObject.CreateInstance<DocumentationUpdaterWindow>();
            try
            {
                var build = typeof(DocumentationUpdaterWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic);
                build.Invoke(window, null); build.Invoke(window, null);
                Assert.That(window.rootVisualElement.Query<HelpBox>().ToList().Count, Is.EqualTo(1));
                Assert.That(window.rootVisualElement.Query<Button>().ToList().Count, Is.EqualTo(1));
                var open = window.rootVisualElement.Q<Button>("compatibility_open_owner");
                Assert.That(open.enabledSelf, Is.EqualTo(GodForwarder.Content != null));
                Assert.That(open.GetFirstAncestorOfType<ScrollView>(), Is.Null); Assert.That(open.GetFirstAncestorOfType<Foldout>(), Is.Null);
                Assert.That(window.rootVisualElement.Q<HelpBox>("compatibility_owner_status").text, Does.StartWith(GodForwarder.Content == null ? "Unavailable" : "Information"));
                if (GodForwarder.Content != null) Assert.That(window.rootVisualElement.ClassListContains("geurts-forge"), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }
        /// <summary>Asserts adapter Has No Menu Registrations Or Automatic Initializers Or Updater using isolated fixture state.</summary>
        [Test] public void AdapterHasNoMenuRegistrationsOrAutomaticInitializersOrUpdater()
        {
            var assembly = typeof(DocumentationIntegration).Assembly;
            Assert.That(DocumentationIntegration.AdapterProtocolVersion, Is.EqualTo(1));
            Assert.That(assembly.GetTypes().Any(t => t.Name == "DocumentationUpdaterController" || t.Name == "GitHubDocumentationTransport"), Is.False);
            foreach(var type in assembly.GetTypes())
            {
                Assert.That(type.GetCustomAttributes(typeof(InitializeOnLoadAttribute), false), Is.Empty);
                foreach(var method in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                {
                    Assert.That(method.GetCustomAttributes(typeof(MenuItem), false), Is.Empty);
                    Assert.That(method.GetCustomAttributes(typeof(InitializeOnLoadMethodAttribute), false), Is.Empty);
                }
            }
        }
        /// <summary>Asserts legacy Consent Never Invokes Its Callback using isolated fixture state.</summary>
        [Test] public void LegacyConsentNeverInvokesItsCallback()
        {
            bool called = false;
            Assert.Throws<InvalidOperationException>(() => DocumentationIntegration.UpdateDocumentationAutomaticallyAsync(() => { called=true; return true; }));
            Assert.Throws<InvalidOperationException>(() => DocumentationIntegration.UpdateCommandmentsAutomaticallyAsync(() => { called = true; return true; }));
            Assert.Throws<InvalidOperationException>(() => Geurts.GameForge.Commandments.CommandmentsIntegration.UpdateCommandmentsAutomaticallyAsync(() => { called = true; return true; }));
            Assert.That(called, Is.False);
        }
        /// <summary>Asserts both Public Namespaces Share The God Owner Or The Same Missing Owner Explanation using isolated fixture state.</summary>
        [Test] public void BothPublicNamespacesShareTheGodOwnerOrTheSameMissingOwnerExplanation()
        {
            Assert.That(Geurts.GameForge.Commandments.CommandmentsIntegration.StatusMessage, Is.EqualTo(DocumentationIntegration.StatusMessage));
            Assert.That(Geurts.GameForge.Commandments.CommandmentsIntegration.IsBusy, Is.EqualTo(DocumentationIntegration.IsBusy));
            if (GodForwarder.Content == null)
            {
                using (DocumentationIntegration.RegisterOperationGuard(() => "Blocked"))
                    Assert.That(DocumentationIntegration.IsBusy, Is.False, "Old God must initialize and remain able to update packages.");
                Assert.That(DocumentationIntegration.ModuleToggleUnavailableReason, Does.Contain("God"));
                Assert.That(DocumentationIntegration.ActionUnavailableReason, Does.Contain("God"));
                Assert.Throws<InvalidOperationException>(() => DocumentationIntegration.CheckForUpdatesAsync());
            }
            else
            {
                Assert.That(GodForwarder.Content.GetProperty("OwnerPackageId"), Is.Null, "Owner identity is a public constant, not mutable state.");
                Assert.That((string)GodForwarder.Content.GetField("OwnerPackageId").GetValue(null), Is.EqualTo("com.geurts.gameforge.god"));
                var view = DocumentationIntegration.CreateEmbeddedWindow(null);
                try { Assert.That(view.GetType().Assembly.GetName().Name, Is.EqualTo("Geurts.GameForge.God.Editor")); }
                finally { UnityEngine.Object.DestroyImmediate(view); }
            }
        }
    }
}
