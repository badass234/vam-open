<#
.SYNOPSIS
Points VaM's plugin compiler at the engine assemblies this project's Unity actually ships.

.DESCRIPTION
DynamicCSharp - the layer that compiles the user's scenes, presets and custom scripts at runtime -
does not scan the Managed folder. It compiles against the list of bare file names in the shipped
Assets\Resources\DynamicCSharp_Settings.asset, and Mono's compiler treats a name it cannot resolve as
a fatal error for that compilation:

    [CS6]: Metadata file `UnityEngine.Timeline.dll' could not be found
    Compile of MacGruber.Life.12:/Custom/Scripts/MacGruber/Life/MacGruber_Life.cslist failed.

So every entry in that list has to exist next to mscorlib in <player>_Data\Managed. The game was built
with Unity 2018.1, where Timeline was an engine module shipped as UnityEngine.Timeline.dll (plus a
5,632-byte UnityEngine.TimelineModule.dll); this project is on 2019.4, where Timeline is the package
com.unity.timeline and the same API arrives as Unity.Timeline.dll. The installation's own Managed
folder satisfies all 27 names; ours was missing exactly those two, and every plugin whose compilation
failed in the log named them.

The list is data, not code, so the hop is a two-line edit - kept in this script because
Assets\Resources is generated from the export and not tracked, and a rebuild would undo a manual edit.
-Verify only reports, and is the measurement to re-run after an engine hop:

    scripts\Update-PluginCompilerReferences.ps1 -Verify

.EXAMPLE
.\scripts\Update-PluginCompilerReferences.ps1
.EXAMPLE
.\scripts\Update-PluginCompilerReferences.ps1 -Verify
#>
[CmdletBinding()]
param(
    [string]$ProjectPath,
    [string]$PlayerPath,
    [string]$ManagedDir,
    [switch]$Verify
)

$ErrorActionPreference = 'Stop'

# The defaults are resolved here and not in the parameter block: $PSScriptRoot is empty while the
# parameter defaults are evaluated, so it would resolve every path against the current directory and
# rewrite the wrong file. $MyInvocation.MyCommand.Path is set however the script is started.
$scripts = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo    = Split-Path -Parent $scripts

if (-not $ProjectPath) { $ProjectPath = Join-Path $repo 'VaM_Rebuild' }
if (-not $PlayerPath) { $PlayerPath = Join-Path $repo 'artifacts\player' }

# Engine assemblies the shipped list names that this engine does not ship under that name. An empty
# replacement drops the entry: 2019.4 has no Timeline engine module at all, the package is the whole
# thing. Keep this table narrow - it is an adaptation of two names, not a filter that hides missing
# references.
$replacements = [ordered]@{
    'UnityEngine.Timeline.dll'       = 'Unity.Timeline.dll'
    'UnityEngine.TimelineModule.dll' = ''
}

$settings = Join-Path $ProjectPath 'Assets\Resources\DynamicCSharp_Settings.asset'
if (-not (Test-Path -LiteralPath $settings)) { throw "no plugin compiler settings at $settings" }

function Get-CompilerReferences([string]$Path) {
    $raw = Get-Content -LiteralPath $Path -Raw
    $inBlock = $false
    $names = @()
    foreach ($line in ($raw -split "\r?\n")) {
        if ($line -match '^[ \t]*assemblyReferences:[ \t]*$') { $inBlock = $true; continue }
        if ($inBlock) {
            if ($line -match '^[ \t]*-[ \t]*(\S+)[ \t]*$') { $names += $Matches[1]; continue }
            if ($line -match '\S') { break }
        }
    }
    , $names
}

if ($Verify) {
    if (-not $ManagedDir) {
        $dataDir = @(Get-ChildItem -LiteralPath $PlayerPath -Directory -Filter '*_Data' -ErrorAction SilentlyContinue)
        if ($dataDir.Count -eq 0) { throw "no <player>_Data folder under $PlayerPath - build the player first" }
        $ManagedDir = Join-Path $dataDir[0].FullName 'Managed'
    }
    if (-not (Test-Path -LiteralPath $ManagedDir)) { throw "no Managed folder at $ManagedDir" }

    $names = Get-CompilerReferences $settings
    $missing = @($names | Where-Object { -not (Test-Path -LiteralPath (Join-Path $ManagedDir $_)) })

    Write-Host ("references in the shipped settings: {0}" -f $names.Count)
    Write-Host ("managed assemblies in the player:  {0}" -f @(Get-ChildItem -LiteralPath $ManagedDir -Filter *.dll).Count)
    if ($missing.Count -eq 0) {
        Write-Host 'every reference resolves in the player' -ForegroundColor Green
        exit 0
    }
    Write-Warning ("{0} reference(s) are named but not built into the player:" -f $missing.Count)
    $missing | ForEach-Object { Write-Warning "  $_" }
    Write-Host ''
    Write-Host 'each of them fails every plugin compilation that asks for it; run this script without' -ForegroundColor Yellow
    Write-Host '-Verify to apply the known engine-name adaptations, or investigate the engine - it may' -ForegroundColor Yellow
    Write-Host 'have dropped the API or moved it into a package that is not installed.' -ForegroundColor Yellow
    exit 1
}

# The adaptation is only valid when the engine really provides the replacement, so the package is
# checked before anything is written: a silently edited list would look like a fix and compile
# nothing.
$manifest = Join-Path $ProjectPath 'Packages\manifest.json'
if (-not (Test-Path -LiteralPath $manifest)) { throw "no package manifest at $manifest" }
if ((Get-Content -LiteralPath $manifest -Raw) -notmatch '"com\.unity\.timeline"') {
    throw ("$manifest does not declare com.unity.timeline, so the engine ships no assembly under " +
           "either Timeline name and plugins that use Timeline cannot compile. Add it, then re-run.")
}

$bytes = [IO.File]::ReadAllBytes($settings)
$hasBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)

$raw = Get-Content -LiteralPath $settings -Raw
$eol = if ($raw -match "`r`n") { "`r`n" } else { "`n" }
$lines = $raw -split "\r?\n"
$inBlock = $false
$changed = @()
$kept = 0
for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if (-not $inBlock) {
        if ($line -match '^[ \t]*assemblyReferences:[ \t]*$') { $inBlock = $true }
        continue
    }
    if ($line -notmatch '^[ \t]*-[ \t]*(\S+)[ \t]*$') { if ($line -match '\S') { $inBlock = $false }; continue }

    $name = $Matches[1]
    if (-not $replacements.Contains($name)) { $kept++; continue }

    $replacement = $replacements[$name]
    if (-not $replacement) {
        $lines[$i] = "!DROP"
        $changed += "  $name -> dropped (this engine has no such assembly)"
        continue
    }
    $lines[$i] = $line -replace [regex]::Escape($name), $replacement
    $changed += "  $name -> $replacement"
}

if ($changed.Count -eq 0) {
    Write-Host ("already adapted: {0} references, none of them stale" -f $kept) -ForegroundColor Green
    exit 0
}

$out = @($lines | Where-Object { $_ -ne '!DROP' })
[IO.File]::WriteAllText($settings, ($out -join $eol), (New-Object Text.UTF8Encoding $hasBom))

Write-Host ("adapted the plugin compiler's reference list in {0}" -f $settings) -ForegroundColor Green
$changed | ForEach-Object { Write-Host $_ }
Write-Host ("  {0} other references left alone" -f $kept)
Write-Host ''
Write-Host 'next: rebuild the player (scripts\Invoke-PlayerBuild.ps1), then check with -Verify'
