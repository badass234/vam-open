<#
.SYNOPSIS
Links the game data to a built player, so the player can load a scene without an install copy.

.DESCRIPTION
RebuildPlayer writes the player to artifacts\player and nothing else: the data the game loads at
runtime is not part of a Unity build. The player resolves all of it relative to its own directory,
which is why the links belong there and not in the editor project:

  * FileManager looks for AddonPackages, AddonPackagesUserPrefs, Custom and Saves by a relative path,
    that is, from the process working directory - and a player started by double-click has its own
    directory as the working directory;
  * AssetBundleManager.GetStreamingAssetsDirectory returns Application.streamingAssetsPath outside the
    editor, which is <exe>_Data\StreamingAssets, so the 16.5 GB of bundles are linked into
    VAMOpen_Data and never copied.

AddonPackages (0.84 GB) and Custom (1.1 GB) are junctions because they are read-only here - a copy
would only drift away from the install. Saves, AddonPackagesUserPrefs and prefs.json are written by
the player, so they are COPIED instead: a run of this build must not modify the installation it
borrows data from.

Two of the copies are not about files at all - they are the difference between a player that runs and
a player that runs the same game the editor does:

  * SuperController reads keyFilePath, Keys/1.21/key.json, relative to the working directory. Without
    that file the game registers only its restricted package set - the player scanned 9 of the 18
    packages the editor scans, which is a solved problem only because the log says how many it found.
    It is copied and refreshed on every run, a stale key being exactly the 9-against-18 symptom;
  * prefs.json holds renderScale, msaaLevel, glowEffects and smoothPasses. Seeding it from the
    installation is what makes a player-against-installation comparison mean anything, since the same
    scene renders differently at glowEffects High and glowEffects Low.

A built player is useless without these links, and the links are useless without an install - this
script is the "point it at your own installation" step, and it is all it takes.

.EXAMPLE
scripts\New-PlayerRuntimeLinks.ps1
.EXAMPLE
scripts\New-PlayerRuntimeLinks.ps1 -Remove
#>
[CmdletBinding()]
param(
    [string]$PlayerPath,
    [string]$InstallRoot,
    [string]$ExeName = 'VAMOpen.exe',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

# The default is resolved here and not in the parameter block: $PSScriptRoot is not filled in when the
# script is started through `powershell -File`, and a script that only works when called one way is a
# trap. $MyInvocation.MyCommand.Path is set in both cases.
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $PlayerPath) { $PlayerPath = Join-Path $root 'artifacts\player' }

# The game install is not part of this repository, so it is never hard-coded here: -InstallRoot
# wins, then $env:VAM_INSTALL, and only then the parent directory of the repository - which is
# correct when the repository has been placed inside the install, as it normally is.
if (-not $InstallRoot) { $InstallRoot = $env:VAM_INSTALL }
if (-not $InstallRoot) {
    $candidate = Split-Path -Parent $root
    if (Test-Path -LiteralPath (Join-Path $candidate 'VaM_Data\Managed\Assembly-CSharp.dll')) { $InstallRoot = $candidate }
}
if (-not $InstallRoot) {
    throw 'no install root: pass -InstallRoot or set $env:VAM_INSTALL to the Virt-a-Mate directory'
}
$InstallRoot = (Resolve-Path -LiteralPath $InstallRoot).Path

# The key file is a fallback chain for the same reason the install root is: the installation is the
# first place to look, and the editor project - which keeps its own copy, under .gitignore, because a
# key is not something to publish - is what is left when the installation has none.
$keysSource = Join-Path $InstallRoot 'Keys'
if (-not (Test-Path -LiteralPath $keysSource)) { $keysSource = Join-Path $root 'VaM_Rebuild\Keys' }

$exe = Join-Path $PlayerPath $ExeName
if (-not (Test-Path -LiteralPath $exe)) {
    if ($Remove) { Write-Host "no player at $PlayerPath - nothing to unlink"; exit 0 }
    throw "no player at $exe - build it first with scripts\Invoke-PlayerBuild.ps1"
}

# The data directory is named after the exe, the way Unity names it, so it is derived rather than
# written down twice.
$dataDirectory = Join-Path $PlayerPath ([IO.Path]::GetFileNameWithoutExtension($ExeName) + '_Data')

# Read-only data: never copied, always linked.
$links = @(
    @{ Link = Join-Path $PlayerPath 'AddonPackages'; Source = Join-Path $InstallRoot 'AddonPackages'; Label = 'var packages' }
    @{ Link = Join-Path $PlayerPath 'Custom'; Source = Join-Path $InstallRoot 'Custom'; Label = 'user content' }
    @{ Link = Join-Path $dataDirectory 'StreamingAssets'; Source = Join-Path $InstallRoot 'VaM_Data\StreamingAssets'; Label = 'asset bundles' }
)

# Data the player writes: copied, so that running the player does not modify the install.
$copies = @(
    @{ Destination = Join-Path $PlayerPath 'Saves'; Source = Join-Path $InstallRoot 'Saves'; Label = 'saved scenes' }
    @{ Destination = Join-Path $PlayerPath 'AddonPackagesUserPrefs'; Source = Join-Path $InstallRoot 'AddonPackagesUserPrefs'; Label = 'package preferences' }
    @{ Destination = Join-Path $PlayerPath 'prefs.json'; Source = Join-Path $InstallRoot 'prefs.json'; Label = 'graphics preferences' }
)

function Get-ReparseState([string]$Path) {
    $item = Get-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue
    if (-not $item) { return 'missing' }
    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { return 'junction' }
    return 'folder'
}

function Connect-Junction([string]$Link, [string]$Source, [string]$Label, [switch]$Remove) {
    $state = Get-ReparseState $Link

    if ($Remove) {
        if ($state -eq 'junction') {
            [IO.Directory]::Delete($Link, $false)
            Write-Host "junction removed: $Link ($Label in $Source left untouched)" -ForegroundColor Green
        }
        else {
            Write-Host "junction for $Label is already absent"
        }
        return
    }

    if (-not (Test-Path -LiteralPath $Source)) { throw "no directory for $Label`: $Source" }

    switch ($state) {
        'junction' {
            $current = (Get-Item -LiteralPath $Link -Force).Target
            if ($current -and ($current.TrimEnd('\') -ieq $Source.TrimEnd('\'))) {
                Write-Host "junction already set: $Link -> $current" -ForegroundColor Green
            }
            else {
                throw "junction points at $current, expected $Source (remove it with the -Remove switch)"
            }
        }
        'folder' {
            # A player that was started before its links existed creates its own empty directories
            # (FileManager does exactly that). Empty ones are removed, anything with files in it is
            # reported instead of deleted.
            $leftovers = Get-ChildItem -LiteralPath $Link -Recurse -File
            if ($leftovers.Count -ne 0) {
                $names = $leftovers | Select-Object -First 5 -ExpandProperty FullName | ForEach-Object { $_.Replace($Link, '.') }
                throw "$Link is not empty and is not a junction - I will not delete it by hand, sort it out: $($names -join ', ')"
            }
            Remove-Item -LiteralPath $Link -Recurse -Force
            New-Item -ItemType Junction -Path $Link -Target $Source | Out-Null
            Write-Host "junction created: $Link -> $Source" -ForegroundColor Green
        }
        default {
            New-Item -ItemType Junction -Path $Link -Target $Source | Out-Null
            Write-Host "junction created: $Link -> $Source" -ForegroundColor Green
        }
    }
}

function Sync-LocalCopy([string]$Source, [string]$Destination, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Source)) { return }

    # prefs.json is a single file and Saves is a tree, and both are "seed it once, leave it alone":
    # the file branch keeps the one entry list instead of a second parameter that means the same thing.
    if (-not (Get-Item -LiteralPath $Source).PSIsContainer) {
        if (Test-Path -LiteralPath $Destination) { Write-Host "$Label already in place" -ForegroundColor Green; return }
        Copy-Item -LiteralPath $Source -Destination $Destination -Force
        Write-Host "copied $Label (original untouched)" -ForegroundColor Green
        return
    }

    $root = (Get-Item -LiteralPath $Source).FullName.TrimEnd('\')
    $copied = 0

    foreach ($file in Get-ChildItem -LiteralPath $root -File -Recurse) {
        $relative = $file.FullName.Substring($root.Length + 1)
        $target = Join-Path $Destination $relative
        if (Test-Path -LiteralPath $target) { continue }

        $targetDir = Split-Path -Parent $target
        if (-not (Test-Path -LiteralPath $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }

        Copy-Item -LiteralPath $file.FullName -Destination $target
        $copied++
    }

    if ($copied -gt 0) { Write-Host "copied $Label`: $copied (original untouched)" -ForegroundColor Green }
    else { Write-Host "$Label already in place" -ForegroundColor Green }
}

# Unlike Saves, the key file is refreshed rather than seeded: it is 25 bytes, the player writes to it
# only when a key is typed into the game's own key screen, and a key that silently went stale costs
# half the packages. A junction left behind by an earlier version of this script is replaced by the
# copy - Directory.Delete on the link, never Remove-Item -Recurse on it, which would walk into the
# source it points at.
function Sync-RefreshedCopy([string]$Source, [string]$Destination, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Source)) {
        Write-Warning "no $Label at $Source - the player will run without it"
        return
    }

    if (Test-Path -LiteralPath $Destination) {
        if ((Get-ReparseState $Destination) -eq 'junction') { [IO.Directory]::Delete($Destination, $false) }
        else { Remove-Item -LiteralPath $Destination -Recurse -Force }
    }

    Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    Write-Host "refreshed $Label`: $Destination" -ForegroundColor Green
}

foreach ($entry in $links) {
    Connect-Junction -Link $entry.Link -Source $entry.Source -Label $entry.Label -Remove:$Remove
}

if ($Remove) {
    Write-Host 'the local Saves, AddonPackagesUserPrefs, prefs.json and Keys copies are left as they are (player data, not links)'
    exit 0
}

# The bundles are the one link the player cannot start without, so the manifest bundle is verified:
# a link that exists but is empty would look like a loading bug much later.
$streamingAssets = ($links | Where-Object { $_.Label -eq 'asset bundles' }).Link
if (-not (Test-Path -LiteralPath (Join-Path $streamingAssets 'StandaloneWindows64'))) {
    throw "manifest bundle StandaloneWindows64 not visible in $streamingAssets - check the installation"
}

$packages = Get-ChildItem -LiteralPath (Join-Path $PlayerPath 'AddonPackages') -File -Filter '*.var'
if ($packages.Count -eq 0) { throw 'no .var packages visible - check the installation' }

foreach ($entry in $copies) {
    Sync-LocalCopy -Source $entry.Source -Destination $entry.Destination -Label $entry.Label
}

Sync-RefreshedCopy -Source $keysSource -Destination (Join-Path $PlayerPath 'Keys') -Label 'key file'

Write-Host ''
Write-Host ("player ready: {0}" -f $exe) -ForegroundColor Green
Write-Host ("  {0} var packages, Custom and the bundles are the installation's own" -f $packages.Count)
Write-Host '  Saves, AddonPackagesUserPrefs, prefs.json and Keys are copies inside the player folder'
if (Test-Path -LiteralPath (Join-Path $PlayerPath 'Keys')) {
    Write-Host '  the key file is in place: the game should register every package it scans for' -ForegroundColor Green
}
else {
    Write-Host '  no key file: the game will register only its restricted package set (fewer than the editor scans)' -ForegroundColor Yellow
}
