// IMPORTANT: This script must comply with GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using NUnit.Framework;
using UnityEngine;

namespace Geurts.GameForge.Documentation.Tests
{
    public sealed class DocumentationEmbeddingTests
    {
        [Test] public void EmbeddedFactoryCreatesIndependentToolsWithoutAnOpeningCheck()
        {
            int checks = 0;
            var type = typeof(DocumentationIntegration).Assembly.GetType("Geurts.GameForge.Documentation.DocumentationUpdaterWindow");
            var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
            var check = type.GetField("OpenCheckForTests", flags);
            check.SetValue(null, new System.Func<System.Threading.Tasks.Task>(() => { checks++; return System.Threading.Tasks.Task.CompletedTask; }));
            var standalone = ScriptableObject.CreateInstance(type);
            var embedded = DocumentationIntegration.CreateEmbeddedWindow(_ => {});
            var embeddedField = type.GetField("EmbeddedInGod", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            try
            {
                Assert.That(embedded, Is.Not.SameAs(standalone));
                Assert.That(embeddedField.GetValue(embedded), Is.True);
                Assert.That(embeddedField.GetValue(standalone), Is.False);
                Assert.That(checks, Is.Zero);
                Assert.That(DocumentationIntegration.IsBusy, Is.False);
            }
            finally { Object.DestroyImmediate(embedded); Object.DestroyImmediate(standalone); check.SetValue(null, null); }
        }
    }
}
