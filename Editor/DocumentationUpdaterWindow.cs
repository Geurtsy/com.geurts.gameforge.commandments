// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
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
            rootVisualElement.Add(new HelpBox("Commandments tools now belong to Game Forge God. This restored compatibility window starts no work.", HelpBoxMessageType.Info));
            rootVisualElement.Add(new Button(DocumentationIntegration.OpenWindow) { text = "Open Commandments in God" });
        }
    }
}
