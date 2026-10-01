[CmdletBinding()]
param(
    [Parameter()]
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe",

    [Parameter()]
    [string]$ProjectPath,

    [Parameter()]
    [string]$PackageReference,

    [Parameter()]
    [string]$DocumentationPath,

    [Parameter()]
    [string]$OdinPath,

    [Parameter()]
    [string]$QuantumConsolePath,

    [Parameter()]
    [switch]$StaticOnly
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = Join-Path $repositoryRoot "work~\UnityValidation"
}
$ProjectPath = [System.IO.Path]::GetFullPath($ProjectPath)
$validationRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "work~"))
$validationPrefix = $validationRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
if (-not $ProjectPath.StartsWith($validationPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "The disposable Unity project must remain under $validationRoot"
}

$package = Get-Content -LiteralPath (Join-Path $repositoryRoot "package.json") -Raw | ConvertFrom-Json
if ($package.name -ne "com.geurts.gameforge.documentation") {
    throw "package.json has the wrong package identity."
}
if ($null -ne $package.dependencies -and @($package.dependencies.PSObject.Properties).Count -ne 0) {
    throw "The package must not declare Unity package dependencies."
}

$productionAssembly = Get-Content -LiteralPath (Join-Path $repositoryRoot "Editor\Geurts.GameForge.Documentation.Editor.asmdef") -Raw | ConvertFrom-Json
if (@($productionAssembly.includePlatforms).Count -ne 1 -or $productionAssembly.includePlatforms[0] -ne "Editor") {
    throw "The production assembly must be Editor-only."
}
if (@($productionAssembly.references).Count -ne 0 -or @($productionAssembly.precompiledReferences).Count -ne 0) {
    throw "The passive adapter must not acquire God or vendor assembly references."
}
foreach ($source in Get-ChildItem -LiteralPath (Join-Path $repositoryRoot 'Editor') -File -Filter '*.cs') {
    if ([IO.File]::ReadAllText($source.FullName) -match '\[(?:MenuItem|InitializeOnLoad|InitializeOnLoadMethod|Command)(?:\(|\])') {
        throw "The passive adapter must not register menus, commands or automatic work."
    }
}
if (@($productionAssembly.defineConstraints).Count -ne 1 -or $productionAssembly.defineConstraints[0] -ne "UNITY_EDITOR_WIN") {
    throw "The production assembly must be limited to Windows Editor hosts."
}

$batchFiles = @(Get-ChildItem -LiteralPath $repositoryRoot -Recurse -File -Filter "*.bat" | Where-Object {
    $_.FullName -notmatch '[\\/](?:work~|old-validation~|outputs|\.git)[\\/]'
})
if ($batchFiles.Count -ne 0) {
    throw "External batch launchers are outside this package's scope."
}
if (Test-Path -LiteralPath (Join-Path $repositoryRoot "GeurtsTechniques")) {
    throw "The Unity package must not embed the authoritative documentation payload."
}
if (Test-Path -LiteralPath (Join-Path $repositoryRoot "GeurtsGameForgeCommandments")) {
    throw "The Unity package must not embed an installed documentation copy."
}

$forbiddenDependencyPattern = '(?i)com\.gameforge\.intelligence|brick.?manager|gameforge.?god'
$dependencyFiles = @(
    (Join-Path $repositoryRoot "package.json"),
    (Join-Path $repositoryRoot "Editor\Geurts.GameForge.Documentation.Editor.asmdef")
)
foreach ($dependencyFile in $dependencyFiles) {
    if ((Get-Content -LiteralPath $dependencyFile -Raw) -match $forbiddenDependencyPattern) {
        throw "A forbidden package dependency appears in $dependencyFile."
    }
}

if ($StaticOnly) {
    [pscustomobject]@{
        Package = $package.name
        PackageVersion = $package.version
        StaticValidation = "Passed"
    }
    return
}

if ([string]::IsNullOrWhiteSpace($PackageReference) -or
    $PackageReference -notmatch '^(?:https://|ssh://|git://|git@)[^\s]+\.git(?:\?path=[^#\s]+)?#[^#\s]+$') {
    throw "Provide an explicit Git PackageReference with a pushed revision, for example https://github.com/Geurtsy/com.geurts.gameforge.commandments.git#<commit>. Local file or embedded packages are not supported."
}

if (-not (Test-Path -LiteralPath $UnityPath -PathType Leaf)) {
    throw "Unity Editor was not found at: $UnityPath"
}
if (Test-Path -LiteralPath $ProjectPath) {
    Remove-Item -LiteralPath $ProjectPath -Recurse -Force
}
New-Item -ItemType Directory -Path (Join-Path $ProjectPath "Assets") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $ProjectPath "Packages") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $ProjectPath "ProjectSettings") -Force | Out-Null

$manifest = [ordered]@{
    dependencies = [ordered]@{
        "com.geurts.gameforge.documentation" = $PackageReference
        "com.unity.test-framework" = "1.8.0"
        "com.unity.inputsystem" = "1.20.0"
        "com.unity.ugui" = "2.6.0"
        "com.unity.modules.audio" = "1.0.0"
        "com.unity.modules.animation" = "1.0.0"
        "com.unity.modules.screencapture" = "1.0.0"
        "com.unity.modules.physics" = "1.0.0"
        "com.unity.modules.physics2d" = "1.0.0"
        "com.unity.modules.imgui" = "1.0.0"
        "com.unity.modules.jsonserialize" = "1.0.0"
        "com.unity.modules.ui" = "1.0.0"
        "com.unity.modules.unitywebrequest" = "1.0.0"
        "com.unity.modules.imageconversion" = "1.0.0"
    }
    testables = @("com.geurts.gameforge.documentation")
}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $ProjectPath "Packages\manifest.json") -Encoding UTF8

$quantumDestination = Join-Path $ProjectPath "Assets/Plugins/QFSW/Quantum Console"
if (-not [string]::IsNullOrWhiteSpace($QuantumConsolePath)) {
    New-Item -ItemType Directory -Path $quantumDestination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $QuantumConsolePath 'Source') -Destination $quantumDestination -Recurse
}

# Use a locally licensed Odin installation only in the disposable test project.
if (-not [string]::IsNullOrWhiteSpace($OdinPath)) {
    if (-not (Test-Path -LiteralPath (Join-Path $OdinPath "Assemblies\Sirenix.OdinInspector.Editor.dll") -PathType Leaf)) {
        throw "OdinPath must point to the Sirenix folder of an installed Odin Inspector copy."
    }
    $odinDestination = Join-Path $ProjectPath "Assets\Plugins\Sirenix"
    New-Item -ItemType Directory -Path $odinDestination -Force | Out-Null
    # Do not import project-specific optional modules (for example Unity.Mathematics).
    foreach ($entry in @("Assemblies", "Odin Inspector/Assets", "Odin Inspector/Config")) {
        $entryDestination = Join-Path $odinDestination $entry
        New-Item -ItemType Directory -Path (Split-Path -Parent $entryDestination) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $OdinPath $entry) -Destination $entryDestination -Recurse
        $metaPath = Join-Path $OdinPath ($entry + ".meta")
        if (Test-Path -LiteralPath $metaPath) {
            Copy-Item -LiteralPath $metaPath -Destination ($entryDestination + ".meta")
        }
    }
    # The first batch compile must see Odin before its interactive installer has run.
    '-define:ODIN_INSPECTOR' | Set-Content -LiteralPath (Join-Path $ProjectPath "Assets\csc.rsp") -Encoding ASCII
}

# Use real documentation as an external integration fixture, never as a bundled template.
if (-not [string]::IsNullOrWhiteSpace($DocumentationPath)) {
    $fixtureRoot = Join-Path $ProjectPath "GeurtsGameForgeCommandments"
    foreach ($relativePath in @("GeurtsTechniqueManifest.md", "AI_READ_FIRST.md", "GeurtsTechniques/GeurtsGitIgnoreTechnique.md", "GeurtsTechniques/GeurtsAgentTechnique.md")) {
        $sourcePath = Join-Path $DocumentationPath $relativePath
        $fixturePath = Join-Path $fixtureRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $fixturePath) -Force | Out-Null
        Copy-Item -LiteralPath $sourcePath -Destination $fixturePath
    }
}
@"
m_EditorVersion: 6000.6.3f1
"@ | Set-Content -LiteralPath (Join-Path $ProjectPath "ProjectSettings\ProjectVersion.txt") -Encoding UTF8

$resultPath = Join-Path $ProjectPath "TestResults.xml"
$logPath = Join-Path $ProjectPath "UnityValidation.log"
$arguments = @(
    "-runTests",
    "-batchmode",
    "-projectPath", $ProjectPath,
    "-testPlatform", "EditMode",
    "-testResults", $resultPath,
    "-logFile", $logPath
)

$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -WindowStyle Hidden
if ($process.ExitCode -ne 0) {
    if (Test-Path -LiteralPath $logPath) {
        Get-Content -LiteralPath $logPath -Tail 160 | Write-Host
    }
    throw "Unity validation failed with exit code $($process.ExitCode)."
}
if (-not (Test-Path -LiteralPath $resultPath -PathType Leaf)) {
    if (Test-Path -LiteralPath $logPath) {
        Get-Content -LiteralPath $logPath -Tail 160 | Write-Host
    }
    throw "Unity did not produce EditMode test results."
}

[xml]$results = Get-Content -LiteralPath $resultPath -Raw
$testRun = $results.'test-run'
if ($null -eq $testRun -or [int]$testRun.failed -ne 0 -or [int]$testRun.passed -lt 1 -or
    (-not [string]::IsNullOrWhiteSpace($DocumentationPath) -and [int]$testRun.skipped -ne 0)) {
    throw "Unity EditMode tests did not pass."
}

$log = Get-Content -LiteralPath $logPath -Raw
# Only this copied vendor source has known provenance; malformed or other warning sources fail.
$quantumSourcePrefix = [System.IO.Path]::GetFullPath((Join-Path $quantumDestination 'Source')).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
$thirdPartyWarnings = @()
$blockingWarnings = @()
foreach ($warningMatch in [regex]::Matches($log, '(?im)^[^\r\n]*\bwarning\s+CS\d+\b[^\r\n]*')) {
    $warning = $warningMatch.Value.Trim()
    $source = [regex]::Match($warning, '^(?<file>.+?)(?<diagnostic>\(\d+(?:,\d+)?\):\s*warning\s+CS\d+\s*:.*)$')
    $knownVendor = $false
    if ($source.Success) {
        try {
            $warningPath = $source.Groups['file'].Value.Replace('/', '\')
            if (-not [System.IO.Path]::IsPathRooted($warningPath)) { $warningPath = Join-Path $ProjectPath $warningPath }
            $warningPath = [System.IO.Path]::GetFullPath($warningPath)
            $knownVendor = $warningPath.StartsWith($quantumSourcePrefix, [System.StringComparison]::OrdinalIgnoreCase)
            if ($knownVendor) {
                $warning = $warningPath.Substring($ProjectPath.Length).TrimStart('\', '/').Replace('\', '/') + $source.Groups['diagnostic'].Value
            }
        }
        catch { $knownVendor = $false }
    }
    if ($knownVendor) { $thirdPartyWarnings += $warning }
    else { $blockingWarnings += $warning }
}
$thirdPartyWarnings = @($thirdPartyWarnings | Sort-Object -Unique)
$blockingWarnings = @($blockingWarnings | Sort-Object -Unique)
foreach ($warning in $thirdPartyWarnings) { Write-Warning "Third-party Quantum Console: $warning" }
foreach ($warning in $blockingWarnings) { Write-Warning "First-party or unknown source: $warning" }
if ($log -match '(?im)\berror\s+[A-Z]+\d+\b|(?:^|:)\s*(?:fatal\s+)?error\s*:' -or
    $log -match '(?i)\b(?:Tundra\s+build\s+failed|compilation\s+failed|scripts\s+have\s+compiler\s+errors|build\s+failed|build\s+completed\s+with\s+a\s+result\s+of\s+[''"]?failed)\b' -or
    $blockingWarnings.Count -gt 0) {
    throw "Unity reported a compiler error, build failure, or first-party/unknown C# warning. See $logPath"
}

[pscustomobject]@{
    Package = $package.name
    PackageVersion = $package.version
    PackageReference = $PackageReference
    Odin = -not [string]::IsNullOrWhiteSpace($OdinPath)
    QuantumConsole = -not [string]::IsNullOrWhiteSpace($QuantumConsolePath)
    Unity = "6000.6.3f1"
    Passed = [int]$testRun.passed
    Failed = [int]$testRun.failed
    Skipped = [int]$testRun.skipped
    ThirdPartyWarningCount = $thirdPartyWarnings.Count
    ThirdPartyWarnings = $thirdPartyWarnings
    ResultPath = $resultPath
    LogPath = $logPath
}
