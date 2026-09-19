<#
.SYNOPSIS
Checks an installed Unity editor before a version hop spends a run on it.

.DESCRIPTION
Some Unity patches past the end of a public LTS line are extended-LTS (xLTS) builds, and an xLTS build
is entitlement-gated: on a Personal licence it prints

    [Licensing::Module] Error: 'com.unity.editor.access.xlts' was not found.
    This build of Unity 2021 is part of an Extended LTS release, which requires either a valid Unity
    Industry or Unity Enterprise license.
    Application will terminate with return code 198

and exits **before the project is opened** - ProjectVersion.txt is not advanced, nothing under Assets\
is touched, no Temp\UnityLockfile is left behind. That is indistinguishable from a silent failure unless
the run's log is read, so it costs a run and tells nothing. The entitlement is visible without launching
the editor at all, in the install's metadata.hub.json, and that is what this script reads instead.

An install with no metadata.hub.json at all is not an xLTS install: Unity Hub writes that file, and an
editor installed straight from the official installer has none (the 2020.3.49f1 install here is one).

The same pass reports the facts a hop needs before it starts, so that a missing one is found now rather
than by a compiler failure later:

  packages      Editor\Data\Resources\PackageManager\Editor\manifest.json - each entry carries the
                version and minimumVersion THIS build ships and recommends, which is the authority for
                what a hop receives. A release-notes page describes a different patch line.
  mcs.exe       Editor\Data\MonoBleedingEdge\lib\mono\4.5\mcs.exe - the Mono compiler the self-built
                runtime compiler plugin (Assets\Plugins\mcs.dll) is built with. Withdrawn in 2020.2 for
                Boo only, so the 4.5 profile is expected to survive; this script proves it for the build
                in hand instead of assuming it.
  standalone    Editor\Data\PlaybackEngines\windowsstandalonesupport - the player build gate needs it,
                and the Mono variations inside it are what scriptingBackend: {} requires.

.PARAMETER Version
Editor version folder name under Unity\Hub\Editor, e.g. 2021.3.45f2. Defaults to the version named in
VaM_Rebuild\ProjectSettings\ProjectVersion.txt, which is the same source the gate scripts read.

.PARAMETER EditorDir
The editor folder itself (the one holding Unity.exe), for an install in a non-standard location. Wins
over -Version.

.PARAMETER Quiet
Print only the verdict line.

.EXAMPLE
.\Test-UnityEditorUsable.ps1
.EXAMPLE
.\Test-UnityEditorUsable.ps1 -Version 2021.3.45f2
.EXAMPLE
.\Test-UnityEditorUsable.ps1 -Quiet
if ($LASTEXITCODE -eq 0) { 'safe to hop' }

.OUTPUTS
The report lines, then a `verdict` line. The exit code carries the answer, because an xLTS install is a
refusal to run rather than a value: **0** usable, **1** blocked, not installed, or anything else. Gate on
the exit code. `-Quiet` prints the verdict line alone, and it is still text - a blocked verdict is a
non-empty string, so do not test it for truthiness, test `$LASTEXITCODE`.
#>

[CmdletBinding()]
param(
    [string] $Version,
    [string] $EditorDir,
    [switch] $Quiet
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'UnityEditor.ps1')

if (-not $EditorDir) {
    if (-not $Version) { $Version = Get-VaMOpenEditorVersion }
    $EditorDir = Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$Version\Editor"
}
if (-not $Version) { $Version = Split-Path -Leaf (Split-Path -Parent $EditorDir) }

$installRoot = Split-Path -Parent $EditorDir
$unityExe = Join-Path $EditorDir 'Unity.exe'
$dataDir = Join-Path $EditorDir 'Data'
$hubMetadata = Join-Path $installRoot 'metadata.hub.json'
$packageManifest = Join-Path $dataDir 'Resources\PackageManager\Editor\manifest.json'
$mcsExe = Join-Path $dataDir 'MonoBleedingEdge\lib\mono\4.5\mcs.exe'
$playbackEngine = Join-Path $dataDir 'PlaybackEngines\windowsstandalonesupport'

$report = New-Object System.Collections.Generic.List[string]
function Add-Report([string] $Key, [string] $Value) { $report.Add(("{0,-13} {1}" -f $Key, $Value)) }

Add-Report 'editor' $Version
Add-Report 'path' $EditorDir

if (-not (Test-Path -LiteralPath $unityExe)) {
    Add-Report 'installed' 'False'
    Add-Report 'verdict' "NOT INSTALLED - $unityExe does not exist"
    if (-not $Quiet) { $report | ForEach-Object { Write-Output $_ } }
    else { Write-Output $report[$report.Count - 1] }
    exit 1
}

Add-Report 'installed' 'True'
Add-Report 'product' ([System.Diagnostics.FileVersionInfo]::GetVersionInfo($unityExe).ProductVersion)

# ConvertFrom-Json is known to fail on Unity's own metadata in this repository (modules.json carries
# duplicate keys), so the one field this check exists for is read by regex when the parse does not hold.
$productName = ''
$entitlements = @()
$entitlementKnown = $false

if (Test-Path -LiteralPath $hubMetadata) {
    $raw = Get-Content -LiteralPath $hubMetadata -Raw
    $parsed = $null
    try { $parsed = $raw | ConvertFrom-Json } catch { $parsed = $null }
    if ($parsed) {
        if ($parsed.productName) { $productName = $parsed.productName }
        if ($parsed.entitlements) { $entitlements = @($parsed.entitlements) }
        $entitlementKnown = $true
    }
    else {
        $name = [regex]::Match($raw, '"productName"\s*:\s*"([^"]*)"')
        if ($name.Success) { $productName = $name.Groups[1].Value }
        $list = [regex]::Match($raw, '"entitlements"\s*:\s*\[([^\]]*)\]')
        if ($list.Success) {
            $entitlements = @([regex]::Matches($list.Groups[1].Value, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
            $entitlementKnown = $true
        }
    }
}

Add-Report 'productName' $(if ($productName) { $productName } else { '(none)' })
Add-Report 'hub metadata' $(if (Test-Path -LiteralPath $hubMetadata) { 'yes' } else { 'no' })
Add-Report 'entitlements' $(if (-not $entitlementKnown) { 'n/a' } elseif ($entitlements.Count) { $entitlements -join ', ' } else { '(none)' })

if (Test-Path -LiteralPath $packageManifest) {
    $manifestRaw = Get-Content -LiteralPath $packageManifest -Raw
    $count = ([regex]::Matches($manifestRaw, '"version"\s*:')).Count
    Add-Report 'packages' "$count version entries in Editor\Data\Resources\PackageManager\Editor\manifest.json"
}
else {
    Add-Report 'packages' 'MISSING - no Editor\Data\Resources\PackageManager\Editor\manifest.json'
}

Add-Report 'mcs.exe' $(if (Test-Path -LiteralPath $mcsExe) { 'yes - Mono 4.5 profile' } else { 'MISSING - the runtime compiler build has no compiler' })
Add-Report 'standalone' $(if (Test-Path -LiteralPath $playbackEngine) { 'yes - windowsstandalonesupport' } else { 'MISSING - the player build gate cannot run' })

$usable = $true
if ($entitlementKnown -and ($entitlements -contains 'XLTS')) {
    $usable = $false
    Add-Report 'verdict' 'BLOCKED - xLTS entitlement: needs Unity Industry or Unity Enterprise, and a Personal licence exits 198 before the project is opened'
}
elseif ($entitlementKnown) {
    Add-Report 'verdict' 'USABLE - no xLTS entitlement on this build'
}
else {
    Add-Report 'verdict' 'USABLE - no entitlement metadata (an installer-made install, not an xLTS one)'
}

if (-not $Quiet) { $report | ForEach-Object { Write-Output $_ } }
else { Write-Output $report[$report.Count - 1] }

if ($usable) { exit 0 } else { exit 1 }
