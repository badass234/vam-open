<#
.SYNOPSIS
Links the game data to the editor project: .var packages and package preferences.

.DESCRIPTION
FileManager looks for AddonPackages/AddonPackagesUserPrefs by a relative path, that is,
from the process working directory. In the built game that is the install root, so it
finds 18 .var packages. In the editor the working directory is the project directory, and
FileManager simply creates EMPTY AddonPackages/AddonPackagesUserPrefs there and reports
"Scanned 0 packages". Hence the log mismatch: "Package changes detected" versus
"No package changes detected", the extra vamX check and the missing benchmark.

The same goes for user content. A scene refers to its items by names such as
"Custom/Hair/Female/RenVR/RenVR/Simone (REN).vam", and FileManager looks for them from the
install root. While Custom was an empty folder in the project, the scene loaded WITHOUT
hair: "Hair item ... is missing", and the character silently lost its hairstyle. Hence:

  * AddonPackages — 0.84 GB of read-only data, a junction;
  * Custom — 1.1 GB of user content (hair, clothing, atoms, plugins), also a junction:
    a copy here would only drift away from the install;
  * AddonPackagesUserPrefs and Saves are WRITTEN by the editor (package preferences and
    saved scenes land there), so the originals' contents are COPIED into the project
    rather than linked — so that a build does not modify the install.

.EXAMPLE
.\scripts\New-RuntimeDataLinks.ps1
.EXAMPLE
.\scripts\New-RuntimeDataLinks.ps1 -Remove
#>
[CmdletBinding()]
param(
    [string]$ProjectPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'VaM_Rebuild'),
    [string]$InstallRoot,
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

# The game install is not part of this repository, so it is never hard-coded here: -InstallRoot
# wins, then $env:VAM_INSTALL, and only then the parent directory of the repository - which is
# correct when the repository has been placed inside the install, as it normally is.
if (-not $InstallRoot) { $InstallRoot = $env:VAM_INSTALL }
if (-not $InstallRoot) {
    $candidate = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    if (Test-Path -LiteralPath (Join-Path $candidate 'VaM_Data\Managed\Assembly-CSharp.dll')) { $InstallRoot = $candidate }
}
if (-not $InstallRoot) {
    throw 'no install root: pass -InstallRoot or set $env:VAM_INSTALL to the Virt-a-Mate directory'
}
$InstallRoot = (Resolve-Path -LiteralPath $InstallRoot).Path

if (-not (Test-Path $ProjectPath)) { throw "project not found: $ProjectPath" }

$packagesLink  = Join-Path $ProjectPath 'AddonPackages'
$prefsLocal    = Join-Path $ProjectPath 'AddonPackagesUserPrefs'
$packagesSrc   = Join-Path $InstallRoot 'AddonPackages'
$prefsSrc      = Join-Path $InstallRoot 'AddonPackagesUserPrefs'
$customLink    = Join-Path $ProjectPath 'Custom'
$customSrc     = Join-Path $InstallRoot 'Custom'
$savesLocal    = Join-Path $ProjectPath 'Saves'
$savesSrc      = Join-Path $InstallRoot 'Saves'

function Get-ReparseState([string]$Path) {
    $item = Get-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue
    if (-not $item) { return 'missing' }
    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { return 'junction' }
    return 'folder'
}

# The project directory is the only place where the game can see these links, so an
# empty leftover directory from a previous run is removed silently, while a non-empty
# one has to be sorted out by hand.
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
            # An empty directory is an artifact of a previous run: the game was stopped in the
            # middle of creating its own Custom structure, and there are no files in it. We
            # remove only what is genuinely empty — directories without files included.
            $leftovers = Get-ChildItem -LiteralPath $Link -Recurse -File
            if ($leftovers.Count -ne 0) {
                $names = $leftovers | Select-Object -First 5 -ExpandProperty FullName | ForEach-Object { $_.Replace($Link, '.') }
                throw "$Link is not empty and is not a junction — I will not delete it by hand, sort it out: $($names -join ', ')"
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

# The editor's local edits are not overwritten: only files that do not exist in the
# project yet are copied. The relative path is preserved: the game looks up scenes by
# names of the form "Saves/scene/MeshedVR/default.json", and it will not find a flat file
# in the root of Saves.
function Sync-LocalCopy([string]$Source, [string]$Destination, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Source)) { return }

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

# ── AddonPackages: junction ──────────────────────────────────────────────────
Connect-Junction -Link $packagesLink -Source $packagesSrc -Label 'var packages' -Remove:$Remove

if (-not $Remove) {
    $vars = Get-ChildItem -LiteralPath $packagesLink -File -Filter '*.var'
    $size = ($vars | Measure-Object -Property Length -Sum).Sum
    Write-Host ("visible packages: {0}, size: {1} GB" -f $vars.Count, [math]::Round($size / 1GB, 2))
    if ($vars.Count -eq 0) { throw 'no .var packages visible — check the installation' }
}

# ── Custom: junction ─────────────────────────────────────────────────────────
# Without this link a scene loads without the user items: the engine reports
# "Hair item Custom/Hair/Female/.../Simone (REN).vam is missing" and the character is left
# without a hairstyle — silently, because the atom is still created.
Connect-Junction -Link $customLink -Source $customSrc -Label 'user content' -Remove:$Remove

if (-not $Remove) {
    $items = Get-ChildItem -LiteralPath $customLink -Recurse -File
    Write-Host ("visible user files: {0}" -f $items.Count)
    if ($items.Count -eq 0) { Write-Host 'WARNING: Custom is empty — scenes will lose hair, clothing and atoms from Custom' -ForegroundColor Yellow }
}

# ── AddonPackagesUserPrefs and Saves: copy ───────────────────────────────────
if ($Remove) {
    Write-Host 'local AddonPackagesUserPrefs and Saves left as they are (this is project data, not links)'
}
else {
    Sync-LocalCopy -Source $prefsSrc -Destination $prefsLocal -Label 'package preferences'
    Sync-LocalCopy -Source $savesSrc -Destination $savesLocal -Label 'saved scenes'
}

Write-Host ''
Write-Host 'Check: run scripts\Invoke-SmokeTest.ps1 -Method InspectScene.'
