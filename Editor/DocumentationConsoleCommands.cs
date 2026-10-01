// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using QFSW.QC;
using UnityEngine.Scripting;

namespace Geurts.GameForge.Documentation
{
    /// <summary>Read-only editor status exposed through the required Quantum Console integration.</summary>
    internal static class DocumentationConsoleCommands
    {
        [Command("GeurtsGameForge.Commandments.Status", "Shows current documentation and package update status in the Unity Editor."), Preserve]
        [Command("GeurtsGameForge.Documentation.Status", "Compatibility alias for GeurtsGameForge.Commandments.Status."), Preserve]
        internal static string ReadStatus()
        {
            return DocumentationPackageConstants.DisplayName + "\nCommandments: " + DocumentationUpdaterController.StatusMessage +
                   "\nPackage: " + PackageSelfUpdater.instance.StatusMessage;
        }
    }
}
