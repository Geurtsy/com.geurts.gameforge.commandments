// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using System.Linq;
using UnityEditor.PackageManager;

/// <summary>Independent release-metadata example; no God or optional peer assembly is required.</summary>
public static class GeurtsGameForgeCommandmentsCompatibilityAutomationExample
{
    /// <summary>Reads actual installed package metadata without installation or remote access.</summary>
    public static string InstalledVersion() => PackageInfo.GetAllRegisteredPackages().SingleOrDefault(value => value.name == "com.geurts.gameforge.documentation")?.version ?? "unknown";
}
