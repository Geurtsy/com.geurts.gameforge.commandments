// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
namespace Geurts.GameForge.Documentation
{
    /// <summary>Compatibility forwarding for the separate, explicitly selected God setup helpers.</summary>
    public static class BuildForgeIntegration
    {
        /// <summary>Reports whether the God setup owner is unavailable or busy.</summary>
        public static bool IsBusy => GodForwarder.Content == null || DocumentationIntegration.ActionUnavailableReason != null;
        /// <summary>Delegates LoadGitIgnore to God with the same validation and side effects.</summary>
        /// <param name="projectRoot">The selected Unity project root.</param>
        /// <returns>The existing God helper result.</returns>
        public static byte[] LoadGitIgnore(string projectRoot) => (byte[])GodForwarder.Invoke(true, "LoadGitIgnore", projectRoot);
        /// <summary>Delegates LoadCodexGuide to God with the same validation and side effects.</summary>
        /// <param name="projectRoot">The selected Unity project root.</param>
        /// <returns>The existing God helper result.</returns>
        public static byte[] LoadCodexGuide(string projectRoot) => (byte[])GodForwarder.Invoke(true, "LoadCodexGuide", projectRoot);
        /// <summary>Delegates IsCodexGuideInstalled to God with the same validation and side effects.</summary>
        /// <param name="projectRoot">The selected Unity project root.</param>
        /// <param name="target">The explicitly selected guide path.</param>
        /// <returns>The existing God helper result.</returns>
        public static bool IsCodexGuideInstalled(string projectRoot, string target) => (bool)GodForwarder.Invoke(true, "IsCodexGuideInstalled", projectRoot, target);
        /// <summary>Delegates IsGitIgnoreInstalled to God with the same validation and side effects.</summary>
        /// <param name="projectRoot">The selected Unity project root.</param>
        /// <returns>The existing God helper result.</returns>
        public static bool IsGitIgnoreInstalled(string projectRoot) => (bool)GodForwarder.Invoke(true, "IsGitIgnoreInstalled", projectRoot);
        /// <summary>Delegates InstallCodexGuide to God with the same validation and side effects.</summary>
        /// <param name="projectRoot">The selected Unity project root.</param>
        /// <param name="target">The explicitly selected guide path.</param>
        /// <returns>The existing God helper result.</returns>
        public static bool InstallCodexGuide(string projectRoot, string target) => (bool)GodForwarder.Invoke(true, "InstallCodexGuide", projectRoot, target);
        /// <summary>Delegates InstallGitIgnore to God with the same validation and side effects.</summary>
        /// <param name="projectRoot">The selected Unity project root.</param>
        /// <returns>The existing God helper result.</returns>
        public static bool InstallGitIgnore(string projectRoot) => (bool)GodForwarder.Invoke(true, "InstallGitIgnore", projectRoot);
    }
}
