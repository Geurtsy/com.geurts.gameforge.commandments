// IMPORTANT: This script must comply with GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeDocumentation/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Geurts.GameForge.Documentation
{
    /// <summary>Independent per-project preference; no God dependency or managed-documentation write.</summary>
    internal static class DocumentationModule
    {
        internal static string PreferenceKey => KeyForProject(Path.GetDirectoryName(Application.dataPath));
        internal static string KeyForProject(string root) => "GeurtsGameForge.Documentation.ModuleEnabled." +
            Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
        internal static bool Enabled => EditorPrefs.GetBool(PreferenceKey, true);
        internal static event Action Changed;
        internal static void SetEnabled(bool enabled)
        {
            EditorPrefs.SetBool(PreferenceKey, enabled);
            Changed?.Invoke();
        }
    }
}
