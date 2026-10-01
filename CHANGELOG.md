# Changelog

## 0.14.1 - 2026-10-01

- Align current package guide version and supported immutable installation example with the coordinated Commandments release. No behavior, API or Unity GUID changes.

## [0.14.0] - 2026-10-01

- Adopt Commandments guidance names and canonical links; preserve package/API identities, Unity metadata GUIDs and existing consumer content.

## 0.13.2 - 2026-10-01

- Restore the full Documentation Companion dashboard inside God, including manual checks and documentation/package version and update cards. Opening the embedded panel remains offline.
- Retain independent view ownership, module and operation guards, existing update confirmations and standalone opening checks. Add rendered embedded-card and confirmed-update regression coverage.

## 0.13.1 - 2026-10-01

- Rename the package display, menu, dashboard and module status to Geurts Game Forge Documentation Companion, keeping the documentation content name and update action distinct.
- Refresh restored window titles without changing their layout or starting remote checks. Preserve package identity, APIs, assemblies, asset GUIDs, saved preferences and the existing console command.
- Align isolated validation with Unity 6000.6.3f1 and require an explicit Git candidate reference instead of a local package fallback. Report distinct warnings from the copied Quantum Console source separately while retaining failures for compiler errors, build failures and first-party or unknown C# warnings.

## 0.13.0 - 2026-10-01

- Expose an independent embedded tools view for God without update controls or opening checks; preserve the standalone dashboard.

## 0.12.1 - 2026-10-01

- Correct the God module-switch minimum to the published 0.25.0 release, retaining module behavior.

## 0.12.0 - 2026-10-01

- Add visible module enable/disable checkboxes, retaining installed packages and saved settings.
- Block module changes during active work and preserve independent Documentation integration.

## 0.11.0

- Target Unity 6000.6.3f1 for the standalone Documentation Companion package.

## 0.10.3

- Resolve the documentation main commit through bounded Git HTTP reference discovery, avoiding anonymous GitHub REST API rate limits.
- Validate the service, packet framing and unique main reference before downloading the exact commit; preserve confirmation and failure safeguards.

## 0.10.2

- Support the documentation-owned FMOD ignore template 1.0.1 alongside the original 1.0.0 template, each with its exact version, line count and SHA-256.
- Preserve existing project ignore files and reject unsupported versions, modified payloads and self-declared replacement hashes.

## 0.10.1

- Remove remote package/documentation checks and automatic dashboard popups at Unity startup.
- Restored dashboards remain offline; deliberate menu openings and manual refresh still check updates.
- Preserve active package requests through script reload.

## 0.10.0

- Add an optional host integration for explicitly opted-in automatic documentation updates when God opens. Reuse the existing metadata, validation and four-target replacement pipeline.
- Recheck saved consent and Editor/host state after download and before replacement. Turning off or closing God stops pending replacement; automatic failures report through status without modal dialogs or automatic retry.
- Standalone startup remains metadata-only and manual Update keeps its cancel-default confirmation. No God dependency is added.

## [0.9.2] - 2026-09-26

- Open a new Documentation dashboard at a larger, resizable initial size, preferring 1000 × 760 Editor points fitted to the main Editor area where space permits, with its 540 × 560 supported minimum taking precedence.
- Preserve the geometry and constraints of an already open or docked dashboard when opening it again.
- Regenerate the shared theme from God's canonical source without adding a God dependency or changing documentation checks, updates or confirmation behavior.

## [0.9.1] - 2026-09-23

- Apply the shared dark sci-fi and green Editor theme to the Odin dashboard, dependency guidance, Package Manager extension and all three confirmation windows.
- Preserve warning and error colours, cancel-default confirmation behaviour, explicit/deferred actions and the companion's independence from God.
- Generate the companion's theme from God's canonical Editor source without adding a God/UPM package dependency; required licensed Odin/QC assemblies remain separate prerequisites. Temporary IMGUI styling is restored after every draw and generated resources are released when the window closes.
- Keep long confirmation paths and dependency guidance scrollable without hiding the action controls.

## [0.9.0] - Unreleased

- Expose the existing documentation status, checks, confirmed content update and window through a public optional Editor integration API for Game Forge God.
- Keep the companion independent: no reverse dependency, duplicated updater or changed confirmation targets.
- Reserve confirmed updates before delayed execution and accept disposable host-operation guards so documentation and package changes cannot overlap.
- Cover optional API checks, cancelled updates, queued-work guards and unchanged project boundaries in integration tests.

## [0.8.1] - 2026-09-23

- Recognize the approved `.gitignore` with LF, CRLF or CR newlines as already installed, preserving its original bytes and timestamp without a confirmation or rewrite.
- Share the same strict UTF-8, newline-only comparison between the installer and Build Forge completion checks. Changed rules, comments, ordering, whitespace, BOMs or terminal newlines remain differences.
- Cover existing newline variants, substantive differences and missing-file installation in the installer regression tests.

## [0.8.0] - 2026-09-23

- Expose a documented Editor API for Build Forge to validate setup completion and invoke the existing documentation-owned Git ignore and Codex guide installers.
- Consolidate setup in Build Forge when its setup window is available; remove duplicate top-level install menu entries and retain the Documentation window's standalone installer buttons otherwise.
- Create `.gitignore` only when missing. Preserve differing custom files and their timestamps, and leave identical files unchanged.
- Keep Codex guide folder selection, protected destinations and cancel-default overwrite confirmation; compare exact final guide bytes for checklist completion.
- Align the disposable validation project with the installed Unity 6000.3.24f1 patch.

## [0.7.0] - 2026-09-08

- Add Install Codex guide with a user-selected destination, direct documentation entry point, and cancel-default overwrite confirmation.
- Read the sole guide template from the installed documentation technique.
- Adopt documentation contract schema 2.0.0: updates replace documentation and three Copilot routes; Codex guides are installed separately. Update this package before installing documentation 0.12.0.

## 0.6.1 - 2026-09-08

- Give Check for updates its own labelled Odin box above the update cards, with a larger full-width button and the automatic-check explanation inside the section.
- Include Unity metadata for the existing validation-evidence folder and file.

## 0.6.0 - 2026-09-08

- Give Odin Inspector and Quantum Console dependency cards the same green ready, orange missing, blue checking and red error palette as package/documentation update cards, in the dashboard and Unity Package Manager extension.
- Add owned-asset download/import access through Unity's My Assets view and interactive import of a locally downloaded licensed .unitypackage. Unity retains its ownership checks, import review and download/import progress.
- Report blocked and failed installation actions clearly; defer both actions until UI drawing finishes. Readiness refreshes during Unity import/compilation and after script reload; it does not claim vendor-version currency or ownership.

## 0.5.3 - 2026-09-08

- Label Odin Inspector and Quantum Console as required external dependencies in Unity Package Manager's package details.
- Show green dependency boxes marked Installed and ready when the required tool types are loaded and Unity has no script compilation errors. Refresh status during compilation/import and show missing or unavailable tools without a green box.
- Include the same explicit requirements in the package description so they remain readable before package scripts compile. Commercial assets remain separate imports, with no invented registry dependencies.

## 0.5.2 - 2026-09-08

- Fix the dashboard confirmation path: defer actions until Odin has finished drawing, collect confirmation in a UI Toolkit dialog, then start the approved documentation update after the modal window closes.
- Report a blocked update attempt instead of silently discarding it when Unity or another operation is busy.
- Show installed and available Git revision identifiers alongside versions, and explain same-version revision changes or missing installation records explicitly.
- Cover real dashboard clicks, confirmation acceptance/cancellation, file replacement, commit persistence, and a fresh up-to-date result with an Editor integration test.

## 0.5.1 - 2026-09-08

- Require the actual Odin Inspector and Quantum Console assemblies under the current Geurts Game Forge package contract. Both commercial tools remain separately installed and are never bundled.
- Add the read-only `GeurtsGameForge.Documentation.Status` Quantum Console command in the Editor assembly.
- Declare the Unity 6000.3 baseline. Preserve the existing documentation dashboard and its confirmation/update flows; no God dependency is introduced.

## 0.5.0 - 2026-09-08

- Automatically check package and documentation Git versions on window open; add a manual Check for updates action for both.
- Show installed and available versions in separate update cards, with orange highlighting when Git commits differ, even if version numbers match.
- Read bounded version metadata at the resolved commit; retain configured package repositories, branches, tags, pins, and subfolders.
- Show download byte progress plus animated activity bars and detailed checking, extraction, validation, installation, and failure status.
- Keep checks read-only, coalesce overlapping checks, and report unavailable remote versions explicitly.

## 0.4.0 - 2026-09-08

- Added Update package from Git inside the dashboard, with installed version, configured source, progress, success, and failure feedback.
- Refresh the package's existing Git reference through Unity's package client, preserving branches and pins without opening Package Manager or replacing local development checkouts.
- Retain the active package request across script reloads and block overlapping documentation operations.
- Added a Dependencies area with mandatory Odin Inspector status and installation guidance. Missing Odin blocks documentation tools, including the standalone .gitignore action.

## 0.3.0 - 2026-09-08

- Rebuilt the documentation dashboard with Odin Inspector, a clear status card, grouped actions, and collapsible source and managed-file details.
- Added missing-Odin installation guidance while preserving package compilation and the existing confirmation flows.
- Added validation against a locally installed Odin copy; Odin remains separately licensed and is never bundled.

## 0.2.0 - 2026-09-08

- Added Install Geurts .gitignore to the Tools menu and documentation window.
- Read the approved payload from the installed GeurtsGameForgeDocumentation package and validate its manifest version, markers, UTF-8, line count, and checksum before writing the project-root .gitignore.
- Require a cancel-default warning that the existing .gitignore and custom rules will be overwritten and lost.

## 0.1.1 - 2026-09-06

- Use GitHub's compact branch-reference metadata endpoint for bounded startup and Update checks.

## 0.1.0 - 2026-09-06

- Added startup metadata checks for the authoritative documentation repository.
- Added a Unity Editor window with one confirmed documentation update action.
- Added direct replacement of the managed documentation folder and the four contract-declared AI routes.
- Added package and Editor tests for the managed boundary and failure preservation.
