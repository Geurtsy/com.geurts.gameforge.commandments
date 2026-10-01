// IMPORTANT: This script must comply with GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Geurts.GameForge.Documentation.Tests
{
    public sealed class DocumentationEmbeddingTests
    {
        [UnityTest]
        public IEnumerator FullDashboardRendersInsideInstalledGodAndScrollsBelowPinnedBack()
        {
            var godType = System.Type.GetType("Geurts.GameForge.God.Editor.BrickManagerWindow, Geurts.GameForge.God.Editor");
            if (godType == null) Assert.Ignore("Optional God integration requires God 0.26.0 or later in this validation project.");
            var instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var companionType = typeof(DocumentationIntegration).Assembly.GetType("Geurts.GameForge.Documentation.DocumentationUpdaterWindow");
            var check = companionType.GetField("OpenCheckForTests", BindingFlags.Static | BindingFlags.NonPublic);
            int checks = 0;
            check.SetValue(null, new System.Func<System.Threading.Tasks.Task>(() => { checks++; return System.Threading.Tasks.Task.CompletedTask; }));
            var standalone = (EditorWindow)ScriptableObject.CreateInstance(companionType);
            var god = (EditorWindow)ScriptableObject.CreateInstance(godType);
            var entryType = godType.Assembly.GetType("Geurts.GameForge.God.Editor.BrickEntry");
            var entry = System.Activator.CreateInstance(entryType);
            entryType.GetField("Installed", instance).SetValue(entry,
                UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().First(package => package.name == "com.geurts.gameforge.documentation"));
            EditorWindow child = null;
            var original = new Rect(80, 80, 650, 900);
            try
            {
                standalone.ShowUtility(); standalone.position = original;
                god.Show();
                godType.GetMethod("OpenBrickPanel", instance).Invoke(god, new[] { entry });
                child = (EditorWindow)godType.GetProperty("EmbeddedWindowForTests", instance).GetValue(god);
                Assert.That(child, Is.Not.SameAs(standalone));
                Assert.That(typeof(EditorWindow).GetField("m_Parent", instance).GetValue(child), Is.Null, "The embedded view must have no native window.");
                foreach (float width in new[] { 1000f, 560f })
                {
                    god.position = new Rect(40, 40, width, 760);
                    for (int frame = 0; frame < 30; frame++) { god.Repaint(); yield return null; }
                    Assert.That(godType.GetProperty("EmbeddedDrawingReadyForTests", instance).GetValue(god), Is.True);
                    Assert.That(godType.GetProperty("EmbeddedDrawFailureForTests", instance).GetValue(god), Is.Null);
                    var documentation = (Rect)companionType.GetProperty("DocumentationUpdateButtonRect", instance).GetValue(child);
                    var package = (Rect)companionType.GetProperty("PackageUpdateButtonRect", instance).GetValue(child);
                    Assert.That(documentation.width, Is.GreaterThan(100), "God must render the documentation update card.");
                    Assert.That(package.width, Is.GreaterThan(100), "God must render the package update card.");
                    Assert.That(package.y, Is.GreaterThan(documentation.y));
                    var back = god.rootVisualElement.Q<Button>("brick_panel_back");
                    var scroll = god.rootVisualElement.Q<ScrollView>("brick_panel_scroll");
                    Assert.That(back, Is.Not.Null); Assert.That(scroll, Is.Not.Null);
                    float backY = back.worldBound.y;
                    foreach (var offset in new[] { 0f, Mathf.Max(0, documentation.y - 300), Mathf.Max(0, package.y - 300), 10000f })
                    {
                        scroll.scrollOffset = new Vector2(0, offset);
                        for (int frame = 0; frame < 8; frame++) { god.Repaint(); yield return null; }
                        Assert.That(back.worldBound.y, Is.EqualTo(backY), "Back must remain pinned while all dashboard sections scroll.");
                        CaptureRenderedBody(god, width + "-" + (int)offset);
                    }
                    Assert.That(scroll.scrollOffset.y, Is.GreaterThan(0), "The full dashboard must remain reachable by scrolling.");
                    Assert.That(standalone.position, Is.EqualTo(original));
                    Assert.That(checks, Is.Zero, "Selecting, resizing and scrolling the panel must start no network checks.");
                    LogAssert.NoUnexpectedReceived();
                }
                godType.GetMethod("BackToGod", instance).Invoke(god, null);
                Assert.That(child == null, Is.True, "Back must dispose only the owned view.");
                Assert.That(standalone != null, Is.True);
            }
            finally { god.Close(); standalone.Close(); check.SetValue(null, null); }
        }

        private static void CaptureRenderedBody(EditorWindow window, string name)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var parent = typeof(EditorWindow).GetField("m_Parent", flags).GetValue(window);
            var render = new RenderTexture((int)window.position.width, (int)window.position.height, 0);
            var previous = RenderTexture.active;
            Texture2D pixels = null;
            try
            {
                render.Create();
                parent.GetType().GetMethod("GrabPixels", flags).Invoke(parent, new object[] { render, new Rect(0, 0, window.position.width, window.position.height) });
                RenderTexture.active = render;
                pixels = new Texture2D(render.width, render.height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0); pixels.Apply();
                var colors = pixels.GetPixels32(); int foreground = 0;
                for (int y = 100; y < render.height - 100; y += 2)
                    for (int x = 10; x < render.width - 10; x += 2)
                    {
                        var color = colors[y * render.width + x];
                        if (color.r > 100 && color.g > 100 && color.b > 100) foreground++;
                    }
                Assert.That(foreground, Is.GreaterThan(100), "God must render actual dashboard content below Back.");
                string evidence = System.Environment.GetEnvironmentVariable("GEURTS_COMPANION_EMBEDDED_EVIDENCE");
                if (!string.IsNullOrEmpty(evidence))
                {
                    Directory.CreateDirectory(evidence);
                    File.WriteAllBytes(Path.Combine(evidence, name + ".png"), pixels.EncodeToPNG());
                }
            }
            finally
            {
                RenderTexture.active = previous;
                if (pixels) Object.DestroyImmediate(pixels);
                Object.DestroyImmediate(render);
            }
        }

        [UnityTest]
        public IEnumerator EmbeddedDashboardRendersBothUpdateCardsWithoutAnOpeningCheck()
        {
            int checks = 0;
            var type = typeof(DocumentationIntegration).Assembly.GetType("Geurts.GameForge.Documentation.DocumentationUpdaterWindow");
            var instance = BindingFlags.Instance | BindingFlags.NonPublic;
            var check = type.GetField("OpenCheckForTests", BindingFlags.Static | BindingFlags.NonPublic);
            check.SetValue(null, new System.Func<System.Threading.Tasks.Task>(() => { checks++; return System.Threading.Tasks.Task.CompletedTask; }));
            var standalone = (EditorWindow)ScriptableObject.CreateInstance(type);
            var embedded = DocumentationIntegration.CreateEmbeddedWindow(_ => {});
            var original = new Rect(80, 80, 650, 900);
            try
            {
                standalone.ShowUtility(); standalone.position = original;
                embedded.ShowUtility();
                foreach (float width in new[] { 1000f, 560f })
                {
                    embedded.position = new Rect(100, 100, width, 900);
                    for (int frame = 0; frame < 12; frame++) { embedded.Repaint(); yield return null; }
                    var documentation = (Rect)type.GetProperty("DocumentationUpdateButtonRect", instance).GetValue(embedded);
                    var package = (Rect)type.GetProperty("PackageUpdateButtonRect", instance).GetValue(embedded);
                    Assert.That(documentation.width, Is.GreaterThan(100), "The embedded dashboard must draw its documentation update card.");
                    Assert.That(package.width, Is.GreaterThan(100), "The embedded dashboard must draw its package update card.");
                    Assert.That(package.y, Is.GreaterThan(documentation.y), "Both independent cards must be present in the dashboard.");
                    Assert.That(documentation.xMax, Is.LessThanOrEqualTo(width));
                    Assert.That(package.xMax, Is.LessThanOrEqualTo(width));
                    Assert.That(checks, Is.Zero, "Creating, displaying and resizing the embedded dashboard must remain offline.");
                    Assert.That(standalone.position, Is.EqualTo(original), "The host-owned view must not change an existing standalone window.");
                    LogAssert.NoUnexpectedReceived();
                }
            }
            finally { embedded.Close(); standalone.Close(); check.SetValue(null, null); }
        }

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
