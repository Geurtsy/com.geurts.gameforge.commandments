// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Geurts.GameForge.Documentation
{
    internal sealed class DocumentationUpdaterWindow : EditorWindow
    {
        private void CreateGUI()
        {
            titleContent = new GUIContent("Commandments moved to God");
            var root = rootVisualElement; root.Clear();
            // The passive adapter has no God or vendor compile dependency and owns no alternate theme.
            var theme = Type.GetType("Geurts.GameForge.God.Editor.ForgeEditorTheme, Geurts.GameForge.God.Editor", false);
            theme?.GetMethod("ApplyToolkit", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { root });
            bool available = GodForwarder.Content != null;
            root.Add(new HelpBox(available ? "Information · Commandments tools now belong to Game Forge God. This restored compatibility window starts no work." : "Unavailable · " + GodForwarder.MissingMessage, HelpBoxMessageType.Info) { name = "compatibility_owner_status" });
            var open = new Button(DocumentationIntegration.OpenWindow) { text = "Open Commandments in God", name = "compatibility_open_owner" };
            open.SetEnabled(available); root.Add(open);
        }
    }
}
