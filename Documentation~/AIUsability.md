# AI usability in Geurts Game Forge Commandments Compatibility

Release 0.15.2. `ForgeCapabilities.json` is this release's machine-readable declaration. Read installed package metadata and discovered owner providers separately; declaration alone never proves readiness. Planned features remain unavailable. Consumers update manually from Git.

This independent package declares metadata without acquiring a God dependency. Its existing owner APIs remain authoritative; God automation operations are not advertised.

`ForgeProjectReport.Capture()` in God reads owned assets and loaded scenes. Pass an explicit target collection to restrict scope. It never opens other scenes, runs deep checks, repairs assets or reads game-design files. Object/property identity remains unknown when it cannot be resolved. Metadata/owner-running observations do not certify runtime behavior.

## Supported operations

No God automation provider is supplied by this independent package.

## Compile-checked example

The source below is copied into the isolated Unity Editor fixture and compiled before release.

<!-- GEURTS-EXAMPLE: Tools~/Examples/AutomationExample.cs -->
```csharp
// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using System.Linq;
using UnityEditor.PackageManager;

/// <summary>Independent release-metadata example; no God or optional peer assembly is required.</summary>
public static class GeurtsGameForgeCommandmentsCompatibilityAutomationExample
{
    /// <summary>Reads actual installed package metadata without installation or remote access.</summary>
    public static string InstalledVersion() => PackageInfo.GetAllRegisteredPackages().SingleOrDefault(value => value.name == "com.geurts.gameforge.documentation")?.version ?? "unknown";
}
```

## Release validation

Run `Tools~/Validate-Release.ps1 -RequireUnityEvidence` before publication. The check rejects stale identity/version/dependencies, unsupported operation claims, unclassified documentation, broken local links and source fingerprints that do not match Unity evidence. `ReleaseDocumentation.json` identifies current guides and historical records. See `AIUsabilityValidation.json` for actual results and limits; a plan is not validation evidence.

<!-- GEURTS-RELEASE-CONTEXT:BEGIN -->
```json
{
  "packageId": "com.geurts.gameforge.documentation",
  "packageVersion": "0.15.2",
  "dependencies": {}
}
```
<!-- GEURTS-RELEASE-CONTEXT:END -->
