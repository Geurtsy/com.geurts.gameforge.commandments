// IMPORTANT: This script must comply with GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsFolderStructureTechnique.md.

using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
#if !ODIN_INSPECTOR
using UnityEngine.UIElements;
#endif

namespace Geurts.GameForge.Documentation.Tests
{
    internal sealed class DocumentationUpdaterWindowTests
    {
        /// <summary>Open, repaint and reopen the real window without invoking either destructive action.</summary>
        [UnityTest]
        public IEnumerator DashboardOpensAndReopensWithTheInstalledInspector()
        {
            int checks = 0;
            System.Type dashboardType = typeof(DocumentationUpdateService).Assembly.GetType(
                "Geurts.GameForge.Documentation.DocumentationUpdaterWindow", true);
            var openCheck = dashboardType.GetField("OpenCheckForTests",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            System.Func<System.Threading.Tasks.Task> check = () => { checks++; return System.Threading.Tasks.Task.CompletedTask; };
            openCheck.SetValue(null, check);
            try
            {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                // Resolve through the core assembly so the test assembly needs no optional DLL reference.
                System.Type windowType = typeof(DocumentationUpdateService).Assembly.GetType(
                    "Geurts.GameForge.Documentation.DocumentationUpdaterWindow", true);
                EditorWindow window = (EditorWindow)ScriptableObject.CreateInstance(windowType);
                try
                {
                    window.Show();
                    window.Repaint();
                    yield return null;
                    yield return null;
                    Rect restoredPosition = window.position;
                    Vector2 restoredMinimum = window.minSize;
                    Vector2 restoredMaximum = window.maxSize;
                    var lifecycleFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                    windowType.GetMethod("OnDisable", lifecycleFlags).Invoke(window, null);
                    window.titleContent = new GUIContent("Geurts Documentation");
                    windowType.GetMethod("OnEnable", lifecycleFlags).Invoke(window, null);
                    yield return null;
                    Assert.That(window.titleContent.text, Is.EqualTo("Geurts Documentation Companion"));
                    Assert.That(window.position, Is.EqualTo(restoredPosition), "Restoring the title must preserve window geometry.");
                    Assert.That(window.minSize, Is.EqualTo(restoredMinimum));
                    Assert.That(window.maxSize, Is.EqualTo(restoredMaximum));
                    Assert.That(checks, Is.EqualTo(attempt), "Restoring/enabling a window must not check remote updates.");
                    windowType.GetMethod("ShowWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, null);
                    double deadline = EditorApplication.timeSinceStartup + 3;
                    while (checks < attempt + 1 && EditorApplication.timeSinceStartup < deadline) yield return null;
                    Assert.That(checks, Is.EqualTo(attempt + 1), "An explicit menu opening still requests both checks once.");
#if ODIN_INSPECTOR
                    Assert.That(windowType.BaseType.FullName,
                        Is.EqualTo("Sirenix.OdinInspector.Editor.OdinEditorWindow"));
#else
                    HelpBox notice = window.rootVisualElement.Q<HelpBox>();
                    Assert.That(notice, Is.Not.Null);
                    Assert.That(notice.text, Does.Contain("Odin Inspector is required"));
                    Assert.That(notice.messageType, Is.EqualTo(HelpBoxMessageType.Error));
                    Assert.That(window.rootVisualElement.Q<Label>("odin-status").text, Does.Contain("Missing (required)"));
                    Assert.That(window.rootVisualElement.Query<Button>().ToList(), Has.Count.EqualTo(5));
#endif
                    LogAssert.NoUnexpectedReceived();
                }
                finally
                {
                    window.Close();
                }
            }
            }
            finally { openCheck.SetValue(null, null); }
        }
    }
}
