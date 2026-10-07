# Geurts Game Forge Commandments Compatibility

Version **0.15.2**, Windows Unity **6000.6.3f1**. This package is now a passive Editor API adapter for existing Companion installations. God **0.29.0** owns viewing, acquisition, version checks, confirmed updates and status. Fresh installations need God only.

Update this package and God through their immutable Git releases, in either order. With older God the adapter reports the missing supported owner. New God blocks competing content work while an old Companion implementation remains installed. Package updates alone never acquire content or change saved module preferences, schema-3 consent, managed AI routes, AGENTS.md or game design.

The existing UPM ID `com.geurts.gameforge.documentation`, Editor assembly `Geurts.GameForge.Documentation.Editor`, public `DocumentationIntegration`, `Geurts.GameForge.Commandments.CommandmentsIntegration`, `BuildForgeIntegration`, and legacy window script GUID remain compatible. The adapter contains no updater, transport, automatic initializer, menu or command registrations. A restored old window offers only a handoff to God.

Use **Tools > Geurts Game Forge > Commandments** or **View Commandments** inside God. The same four-target confirmation applies to content replacement. Preserve local edits manually before accepting it; no backup, rollback or automatic migration is supplied. `Docs/GameDesign/`, `AGENTS.md`, the old `GeurtsGameForgeDocumentation/` folder and unlisted paths remain outside that action.

Custom Editor assemblies can keep this adapter while migrating to `Geurts.GameForge.God.Editor.CommandmentsIntegration` and `CommandmentsSetupIntegration`, in God's Editor assembly. Remove the adapter through UPM only after those references migrate. Its reflection forwarding introduces no God/vendor compile dependency and reports unavailable operations when God is absent. Never reference either Editor API from runtime assemblies.

Install or update from `https://github.com/Geurtsy/com.geurts.gameforge.commandments.git#v0.15.2` (the immutable release includes an exact commit pin). The source Commandments remain independent at [GeurtsGameForge_Commandments](https://github.com/Geurtsy/GeurtsGameForge_Commandments), and local Markdown is readable without God or this adapter. See the authoritative [0.41.0 migration](https://github.com/Geurtsy/GeurtsGameForge_Commandments/blob/v0.41.0/Migrations/v0.41.0.md).

The prior validation records describe historical releases; they do not assign ongoing ownership to this adapter. Current behavior is covered by `Tests/Editor/CompatibilityTests.cs` and God's transferred acquisition/consent/boundary regressions and package-transition validation.

The restored handoff retains one native status and action across reconstruction, reports an unavailable God owner inline and reuses the current God theme when available. See the [scoped 0.15.2 validation](Documentation~/AuthoringValidation.md); full rendered/physical acceptance remains unverified.

<!-- GEURTS-RELEASE-CONTEXT:BEGIN -->
```json
{
  "packageId": "com.geurts.gameforge.documentation",
  "packageVersion": "0.15.2",
  "dependencies": {}
}
```
<!-- GEURTS-RELEASE-CONTEXT:END -->
