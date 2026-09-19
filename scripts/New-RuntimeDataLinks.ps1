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

prefs.json is the fifth of these and the only one that changes the render rather than what
loads, and version is the sixth and the only one that changes what the content believes: see
SuperController.SyncVersion, which reads <working directory>\version and derives the
VAM_<major>_<minor>_<patch>_<build> defines from it. While the project has no such file the editor
reports the imitate version and gates content as 1.20 - which matches nothing and shows up nowhere
until a package takes a version branch. The game reads it relative to its working directory and applies it in
UserPreferences.RestorePreferences, so a built player runs the installation's preset while the
editor runs the project's own: two copies, two presets, two renders of the same scene.
pixelLightCount 4 against 2 is how many lights reach the pixel stage at all, and smoothPasses 4
against 2 is how many times DAZSkinV2 Laplacian-smooths the body before it rebuilds the normals —
which is the difference between skin and skin that looks lightly oiled. The graphics keys are
therefore matched to the installation by default and printed one by one. Pass
-MatchGraphicsPrefs:$false to only report the difference.

whitelist_domains.json is the seventh and the narrowest of these: it is what makes the built-in browser
work at all in the editor. UserPreferences declares the two paths as bare relative names —
"whitelist_domains.json" and "whitelist_domains_user.json" (UserPreferences.cs:567) — so both are
resolved against the process working directory, which in the editor is the project directory, and
VRWebBrowser refuses every http(s) address that is not on the list. The list is the user's own and is
seeded from the installation.

.EXAMPLE
.\scripts\New-RuntimeDataLinks.ps1
.EXAMPLE
.\scripts\New-RuntimeDataLinks.ps1 -MatchGraphicsPrefs:$false
.EXAMPLE
.\scripts\New-RuntimeDataLinks.ps1 -Remove
#>
[CmdletBinding()]
param(
    [string]$ProjectPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'VaM_Rebuild'),
    [string]$InstallRoot,
    [bool]$MatchGraphicsPrefs = $true,
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

# The graphics keys of a QualityLevel, in the order UserPreferences declares them. They are the
# whole of what a preset is: everything else in prefs.json is input, HUD and plugin settings.
$graphicsKeys = @('renderScale', 'msaaLevel', 'pixelLightCount', 'shaderLOD', 'smoothPasses',
                  'mirrorReflections', 'realtimeReflectionProbes', 'softBodyPhysics', 'glowEffects')

# The value as it stands in the file, quotes included, so that a key can be replaced without
# knowing whether it is written as a number, a string or a bool. The game writes all of them as
# strings, and SimpleJSON parses the file either way.
function Get-PrefToken([string]$Text, [string]$Key) {
    $match = [regex]::Match($Text, '"' + [regex]::Escape($Key) + '"\s*:\s*("[^"]*"|[^,}\r\n]+)')
    if ($match.Success) { return $match.Groups[1].Value.Trim() }
    return $null
}

# Rewrites one key in place. SimpleJSON does not care about formatting, but the file is the game's
# own output, so it keeps its shape instead of being re-serialized: a diff of this file should show
# the values that changed and nothing else.
function Set-PrefToken([string]$Text, [string]$Key, [string]$Token) {
    $pattern = '"' + [regex]::Escape($Key) + '"(\s*:\s*)("[^"]*"|[^,}\r\n]+)'
    return [regex]::Replace($Text, $pattern, ('"' + $Key + '"${1}' + $Token))
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

# ── prefs.json: the graphics preset the editor runs at ───────────────────────
# Two copies of the same file, read by the same code, and the one the editor finds is its own. That
# is the first thing an editor-against-installation look comparison has to remove: while the two
# presets differ, a sheen that is the preset and a sheen that is the shading are the same picture on
# screen, and no amount of reading the shader can tell them apart.
$installPrefs = Join-Path $InstallRoot 'prefs.json'
$projectPrefs = Join-Path $ProjectPath 'prefs.json'

if ($Remove) {
    Write-Host "the editor's prefs.json is left as it is (the editor owns it, like Saves)"
}
elseif (-not (Test-Path -LiteralPath $installPrefs)) {
    Write-Host "WARNING: the installation has no prefs.json ($installPrefs) - no preset to match" -ForegroundColor Yellow
}
elseif (-not (Test-Path -LiteralPath $projectPrefs)) {
    Copy-Item -LiteralPath $installPrefs -Destination $projectPrefs
    Write-Host "copied prefs.json from the installation: the editor now runs the installation's preset" -ForegroundColor Green
}
else {
    $projectText = [IO.File]::ReadAllText($projectPrefs)
    $installText = [IO.File]::ReadAllText($installPrefs)
    $changed = @()

    Write-Host 'graphics preset (installation -> editor project):'
    foreach ($key in $graphicsKeys) {
        $wanted = Get-PrefToken -Text $installText -Key $key
        if (-not $wanted) { continue }

        $current = Get-PrefToken -Text $projectText -Key $key
        if (-not $current) {
            Write-Host ("  {0,-24} {1,-6} {2}" -f $key, $wanted.Trim('"'), 'not in the editor file - the game would fall back to its own default') -ForegroundColor Yellow
            continue
        }

        if ($current -eq $wanted) {
            Write-Host ("  {0,-24} {1,-6} {2}" -f $key, $current.Trim('"'), 'same')
            continue
        }

        if ($MatchGraphicsPrefs) {
            $projectText = Set-PrefToken -Text $projectText -Key $key -Token $wanted
            $changed += $key
            Write-Host ("  {0,-24} {1,-6} {2}" -f $key, $wanted.Trim('"'), ("was " + $current.Trim('"'))) -ForegroundColor Green
        }
        else {
            Write-Host ("  {0,-24} {1,-6} {2}" -f $key, $current.Trim('"'), ("the installation has " + $wanted.Trim('"'))) -ForegroundColor Yellow
        }
    }

    if ($changed.Count -eq 0) {
        Write-Host 'the editor already runs the installation preset' -ForegroundColor Green
    }
    elseif ($MatchGraphicsPrefs) {
        [IO.File]::WriteAllText($projectPrefs, $projectText)
        Write-Host ("editor prefs.json: matched {0} key(s) to the installation ({1})" -f $changed.Count, ($changed -join ', ')) -ForegroundColor Green
    }
    else {
        Write-Host ("the editor runs a different preset in {0} key(s): {1} - drop -MatchGraphicsPrefs:`$false to match them" -f $changed.Count, ($changed -join ', ')) -ForegroundColor Yellow
    }
}

# ── version: what the build reports about itself ──────────────────────────
# The sixth copy, and the one that changes no file of the game's: SuperController.SyncVersion reads
# <working directory>\version, and both the version line and the VAM_* defines the content branches
# on come from it. Seeded from the installation and refreshed when the installation changes.
$installVersion = Join-Path $InstallRoot 'version'
$projectVersion = Join-Path $ProjectPath 'version'

if ($Remove) {
    Write-Host "the editor's version file is left as it is (like prefs.json)"
}
elseif (-not (Test-Path -LiteralPath $installVersion)) {
    Write-Host "WARNING: the installation has no version file ($installVersion) - the editor will report the imitate version" -ForegroundColor Yellow
}
elseif ((Test-Path -LiteralPath $projectVersion) -and ((Get-FileHash -LiteralPath $installVersion).Hash -eq (Get-FileHash -LiteralPath $projectVersion).Hash)) {
    Write-Host 'the editor already reports the installation version' -ForegroundColor Green
}
else {
    Copy-Item -LiteralPath $installVersion -Destination $projectVersion -Force
    Write-Host 'copied the version file: the editor now reports the installation version' -ForegroundColor Green
}

# ── whitelist_domains.json: which hosts the built-in browser may open ────────
# The seventh copy, and the one the browser refuses to work without. UserPreferences reads the two
# names as relative paths (UserPreferences.cs:567 -> :4144), so the editor only sees a whitelist that
# sits in the project directory; with no file there the set is empty and CheckWhitelistDomain says no to
# every address but about:blank, which is what "Attempted to load browser URL ... which is not on
# whitelist" reports. Seeded from the installation, and refreshed when the installation's own file
# changes, because the list belongs to the user and not to this repository.
$whitelistNames = @('whitelist_domains.json', 'whitelist_domains_user.json')

foreach ($name in $whitelistNames) {
    $installWhitelist = Join-Path $InstallRoot $name
    $projectWhitelist = Join-Path $ProjectPath $name

    if ($Remove) {
        Write-Host "the editor's $name is left as it is (the game reads it, like prefs.json)"
        continue
    }

    if (-not (Test-Path -LiteralPath $installWhitelist)) {
        # Perfectly normal for the _user file, and a warning only for the main one.
        if ($name -eq 'whitelist_domains.json') {
            Write-Host "WARNING: the installation has no $name ($installWhitelist) - the browser will refuse every address" -ForegroundColor Yellow
        }
        continue
    }

    if ((Test-Path -LiteralPath $projectWhitelist) -and ((Get-FileHash -LiteralPath $installWhitelist).Hash -eq (Get-FileHash -LiteralPath $projectWhitelist).Hash)) {
        Write-Host "$name already matches the installation" -ForegroundColor Green
        continue
    }

    Copy-Item -LiteralPath $installWhitelist -Destination $projectWhitelist -Force
    Write-Host "copied $name`: the built-in browser can open the addresses the installation allows" -ForegroundColor Green
}

Write-Host ''
Write-Host 'Check: run scripts\Invoke-SmokeTest.ps1 -Method InspectScene.'
