<#
.SYNOPSIS
Repacks VRAdultFun's E-Motion addon with a one-line fix for its static-initializer crash.

.DESCRIPTION
E-Motion seeds one of its fields from a static initializer, and that field is the only thing in
the whole 12 202-line file that calls a Unity API from one:

    Custom/Scripts/E-Motion/Scripts/EmotionEngine.cs:763
    private static float breathRate = Random.Range(0.2f, 0.3f);

Unity forbids that. The check is in the engine's own scripting layer, and it fires before the
instance constructor runs, so AddComponent never gets a component back:

    UnityException: Range is not allowed to be called from a MonoBehaviour constructor (or instance
    field initializer), call it in Awake or Start instead. Called from MonoBehaviour 'EmotionEngine'
    on game object 'plugin#1temp'.
      VRAdultFun.EmotionEngine..cctor()
      DynamicCSharp.ScriptType.CreateBehaviourInstance(ScriptType.cs:105)
      MVRPluginManager.CreateScriptController(MVRPluginManager.cs:440)

and because a throwing type initializer is cached by the runtime, the type stays poisoned and every
later touch re-raises:

    Rethrow as TypeInitializationException: The type initializer for 'VRAdultFun.EmotionEngine' threw

EmotionEngine is `partial` across 34 of the package's 36 script entries, so its single .cctor merges
every static initializer of all of them - the crash is one line among many, not a load-order accident.

The field is dead. Measured over the extracted file: `breathRate` occurs exactly once, at its own
declaration, and no reader exists anywhere in the package. So the minimal fix keeps the intent - a
fixed low breathing rate - and drops the engine call:

    private static float breathRate = 0.25f;

0.25f is the midpoint of the range the original asked for, so even a future reader that appears sees
the same value the original would have produced on average. Moving the call into Awake or Start would
also satisfy the engine, but it would add a field write and a second thing to keep in order against
the other 33 partial files, to serve a value nothing reads.

The package ships source rather than a DLL - DynamicCSharp compiles the .cs at load, which is why the
crash stack carries an MVID and IL offsets instead of a file name - so the fix is a repack, not a
patch to an assembly. The output is a plain zip with the same entries in the same order; every entry
is copied as raw bytes and only the script is rewritten, which the script proves by re-reading what
it wrote and hashing every entry on both sides.

Two things decide whether the repacked package is the one the game actually loads, and the Decal Maker
delivery measured both of them rather than assuming them:

  * A version-qualified url addresses exactly that version. `FileManager.GetPackage`
    (FileManager.cs:1560) looks the "Name.Version" up in the exact-uid dictionary; only the
    `.latest` and `.minNN` spellings consult the package group. Scenes and plugin urls here are
    written in the version-qualified form, so a scene holding `VRAdultFun.E-Motion.4:` keeps
    loading 4 while a `VRAdultFun.E-Motion.5` sits beside it - there is no "newest wins" for that
    spelling.

  * A package that ships .cs must have been confirmed once. `MVRPluginManager` calls
    `mvrp.UserConfirm()` and returns without loading anything when the plugin's package has no
    answer yet (MVRPluginManager.cs:753), and a package's answer is the file
    `AddonPackagesUserPrefs\<Uid>.prefs` holding `"pluginsAlwaysEnabled" : "true"`
    (`VarPackage.LoadUserPrefs`, VarPackage.cs:314). That folder name is relative
    (`FileManager.cs:52`), so it resolves against the process working directory: the install root
    for VaM.exe, and the project folder for a game run started by scripts\Invoke-ManualPlay.ps1.
    Measured on this install, across all three roots the working directory can be: the project folder
    (`VaM_Rebuild\AddonPackagesUserPrefs`) holds `VRAdultFun.E-Motion.4.prefs`, the player folder
    (`artifacts\player\AddonPackagesUserPrefs`) holds it too, both 125 B and both reading
    `"pluginsAlwaysEnabled" : "true"`, and the install root holds only the new revision's file. So `.4`
    is already confirmed for a run started from this project, and it is the **new** revision that needs
    its own answer written or it will prompt and not load. (An earlier version of this note said there
    was no `.4` prefs file at all; that read one root instead of three.)

So writing the package is only half of the delivery, and this script does both halves: it writes
the package, and it writes the confirmation the package needs (skip it with -NoConfirm; an
existing prefs file is never overwritten, because a recorded denial is a decision and not a gap).

The default is a new revision that leaves the shipped package byte-for-byte untouched, and that is the
route this project takes, because the package belongs to its author: the delivery is the new revision,
and moving a scene onto it is the scene owner's own edit (in game by re-adding the plugin, or in the
scene json by replacing the version token). -InPlace is kept as a tested capability for an install
owner who wants every existing scene fixed without a scene-side edit, and it is not our route.

This is a local patch of a third-party package: attribution and the author's terms travel with any
redistribution, and nothing here is an upstream release. This change was not offered upstream and no
response from the author is claimed.

.EXAMPLE
.\scripts\New-E-MotionPatch.ps1 -Verify
Reports the defect site, the entry census and the confirmation state without writing anything.
.EXAMPLE
.\scripts\New-E-MotionPatch.ps1
Writes <install>\AddonPackages\VRAdultFun.E-Motion.5.var plus its .prefs confirmation, and verifies
the package entry by entry. A scene only uses it once that scene names 5.
.EXAMPLE
.\scripts\New-E-MotionPatch.ps1 -InPlace
Rewrites VRAdultFun.E-Motion.4.var itself, keeping the original as
VRAdultFun.E-Motion.4.var.original. This is the variant that fixes scenes which already name 4,
with no scene-side edit.
.EXAMPLE
.\scripts\New-E-MotionPatch.ps1 -Remove
Deletes the generated package (and the .prefs this script wrote), which is the whole rollback.
With -InPlace it restores the .original instead.
#>
[CmdletBinding()]
param(
    [string]$PackagePath,
    [int]$Version = 5,
    [string]$OutputPath,
    [string]$InstallRoot,
    [string]$BackupPath,
    [switch]$InPlace,
    [switch]$NoConfirm,
    [switch]$Verify,
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

# The game install is not part of this repository, so it is never hard-coded here: -InstallRoot
# wins, then $env:VAM_INSTALL, and only then the parent directory of the repository.
if (-not $InstallRoot) { $InstallRoot = $env:VAM_INSTALL }
if (-not $InstallRoot) {
    $candidate = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    if (Test-Path -LiteralPath (Join-Path $candidate 'VaM_Data\Managed\Assembly-CSharp.dll')) { $InstallRoot = $candidate }
}
if (-not $InstallRoot) {
    throw 'no install root: pass -InstallRoot or set $env:VAM_INSTALL to the Virt-a-Mate directory'
}
$InstallRoot = (Resolve-Path -LiteralPath $InstallRoot).Path

if (-not $PackagePath) { $PackagePath = Join-Path $InstallRoot 'AddonPackages\VRAdultFun.E-Motion.4.var' }

if (-not (Test-Path -LiteralPath $PackagePath)) { throw "package not found: $PackagePath" }
$PackagePath = (Resolve-Path -LiteralPath $PackagePath).Path

if (-not $OutputPath) {
    $OutputPath = if ($InPlace) { $PackagePath } else { Join-Path $InstallRoot ('AddonPackages\VRAdultFun.E-Motion.{0}.var' -f $Version) }
}
$OutputPath = [IO.Path]::GetFullPath($OutputPath)

if ($InPlace -and $OutputPath -ne $PackagePath) { throw '-InPlace replaces the shipped package: pass no -OutputPath, or pass -PackagePath itself' }
if (-not $InPlace -and $OutputPath -eq $PackagePath) {
    throw 'refusing to write over the shipped package: write a new revision, or pass -InPlace to replace it (a backup is taken first)'
}
if (-not $BackupPath) { $BackupPath = $PackagePath + '.original' }
$BackupPath = [IO.Path]::GetFullPath($BackupPath)

# The confirmation side of the delivery. A package's answer lives at
# AddonPackagesUserPrefs\<Uid>.prefs, and that folder name is relative, so it is resolved from
# each working directory the game can be started in: the install root (VaM.exe) and, when it
# exists, the project folder (scripts\Invoke-ManualPlay.ps1). Bytes taken from a prefs file VaM
# itself wrote, so the shape is the game's and not a guess.
$packageUid = [IO.Path]::GetFileNameWithoutExtension($OutputPath)
$editorPrefsRoot = Join-Path (Split-Path -Parent $PSScriptRoot) 'VaM_Rebuild\AddonPackagesUserPrefs'
$prefsRoots = @((Join-Path $InstallRoot 'AddonPackagesUserPrefs'))
if (Test-Path -LiteralPath $editorPrefsRoot) { $prefsRoots += $editorPrefsRoot }
$prefsRoots = @($prefsRoots | Select-Object -Unique)
$confirmText = "{ `n   `"pluginsAlwaysEnabled`" : `"true`", `n   `"pluginsAlwaysDisabled`" : `"false`", `n   `"ignoreMissingDependencyErrors`" : `"false`"`n}"

$scriptEntry = 'Custom/Scripts/E-Motion/Scripts/EmotionEngine.cs'
$defect = 'Random.Range(0.2f, 0.3f)'
$patch = '0.25f'

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-EntrySha([System.IO.Compression.ZipArchive]$Zip, [string]$Name) {
    $entry = $Zip.GetEntry($Name)
    if (-not $entry) { return $null }
    $stream = $entry.Open()
    try {
        $sha = [Security.Cryptography.SHA256]::Create()
        try { (($sha.ComputeHash($stream) | ForEach-Object { $_.ToString('x2') }) -join '') }
        finally { $sha.Dispose() }
    }
    finally { $stream.Dispose() }
}

if ($Remove) {
    if ($InPlace) {
        if (Test-Path -LiteralPath $BackupPath) {
            [IO.File]::Copy($BackupPath, $PackagePath, $true)
            Remove-Item -LiteralPath $BackupPath -Force
            Write-Host ("restored : {0}" -f $PackagePath)
            Write-Host ("           the shipped bytes came back from {0}" -f $BackupPath)
        }
        else {
            Write-Host ("nothing  : no backup at {0} - -InPlace has not been applied" -f $BackupPath)
        }
    }
    elseif (Test-Path -LiteralPath $OutputPath) {
        Remove-Item -LiteralPath $OutputPath -Force
        Write-Host ("removed  : {0}" -f $OutputPath)
        # Only a prefs file byte-identical to the one this script writes is removed. An answer the
        # game itself recorded says more than this does, and is not touched.
        foreach ($root in $prefsRoots) {
            $file = Join-Path $root ($packageUid + '.prefs')
            if ((Test-Path -LiteralPath $file) -and ([IO.File]::ReadAllText($file) -eq $confirmText)) {
                Remove-Item -LiteralPath $file -Force
                Write-Host ("removed  : {0}" -f $file)
            }
        }
    }
    else {
        Write-Host ("absent   : {0} (nothing to remove)" -f $OutputPath)
    }
    return
}

Write-Host ("mode     : {0}" -f $(if ($InPlace) { 'in place - the shipped package is rewritten, the original is saved beside it' } else { 'new revision - the shipped package is left alone' }))

$source = [IO.Compression.ZipFile]::OpenRead($PackagePath)
try {
    $entry = $source.GetEntry($scriptEntry)
    if (-not $entry) { throw "entry '$scriptEntry' is not in $PackagePath" }
    $stream = $entry.Open()
    try {
        $reader = New-Object IO.StreamReader($stream)
        try { $text = $reader.ReadToEnd() }
        finally { $reader.Dispose() }
    }
    finally { $stream.Dispose() }

    $found = [regex]::Matches($text, [regex]::Escape($defect)).Count
    $lines = ($text -split "`n").Count
    $readers = [regex]::Matches($text, 'breathRate').Count

    Write-Host ("package  : {0}" -f $PackagePath)
    Write-Host ("           {0} entries, {1:N0} bytes" -f $source.Entries.Count, (Get-Item -LiteralPath $PackagePath).Length)
    Write-Host ("script   : {0}" -f $scriptEntry)
    Write-Host ("           {0:N0} bytes, {1:N0} lines" -f $entry.Length, $lines)
    Write-Host ("defect   : {0}" -f $defect)
    Write-Host ("patch    : {0}" -f $patch)
    Write-Host ("found    : {0} occurrence(s)" -f $found)
    # The fix is a constant only because nothing reads the field. If that ever stops being true the
    # replacement still compiles, so the count is printed rather than trusted silently.
    Write-Host ("deadness : 'breathRate' appears {0} time(s) in this file - 1 means the declaration is its only use" -f $readers)

    if ($found -ne 1) {
        throw "expected exactly one occurrence of the defect and found $found - this is not the package version this script patches"
    }

    if ($Verify) {
        foreach ($root in $prefsRoots) {
            $file = Join-Path $root ($packageUid + '.prefs')
            if (Test-Path -LiteralPath $file) {
                $enabled = ([IO.File]::ReadAllText($file) -match '"pluginsAlwaysEnabled"\s*:\s*"true"')
                Write-Host ("confirm  : {0} - pluginsAlwaysEnabled {1}" -f $file, $enabled)
            }
            else {
                Write-Host ("confirm  : {0} is absent - the game will ask before loading this package" -f $file)
            }
        }
        Write-Host 'verify   : nothing written'
        return
    }

    $patched = $text.Replace($defect, $patch)

    # Built beside the target and installed only after every check passes, so a failure cannot
    # leave a half-written package where the game would find it - which matters in -InPlace mode,
    # where the target is the file the game is already loading.
    $temp = $OutputPath + '.tmp'
    if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Force }
    $target = [IO.Compression.ZipFile]::Open($temp, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($e in $source.Entries) {
            $new = $target.CreateEntry($e.FullName, [IO.Compression.CompressionLevel]::Optimal)
            # CreateEntry stamps the moment of the repack. Carrying the source's timestamp over
            # keeps the repack closer to a byte-faithful copy and makes this script reproducible:
            # two runs of it produce the same file.
            $new.LastWriteTime = $e.LastWriteTime
            $in = $e.Open()
            try {
                $out = $new.Open()
                try {
                    if ($e.FullName -eq $scriptEntry) {
                        $writer = New-Object IO.StreamWriter($out, (New-Object Text.UTF8Encoding($false)))
                        try { $writer.Write($patched) }
                        finally { $writer.Flush() }
                    }
                    else {
                        $in.CopyTo($out)
                    }
                }
                finally { $out.Dispose() }
            }
            finally { $in.Dispose() }
        }
    }
    finally { $target.Dispose() }
}
finally { $source.Dispose() }

# The rebuild is proven rather than assumed: every entry is hashed on both sides, so the only
# difference the report can show is the one expression this script exists to change. Unlike the
# Decal Maker patch this replacement is not length-neutral (26 characters become 5), so the check
# is the rewritten line itself plus the byte delta it implies, not a same-size comparison.
$source = [IO.Compression.ZipFile]::OpenRead($PackagePath)
$result = [IO.Compression.ZipFile]::OpenRead($temp)
try {
    $sourceNames = @($source.Entries | ForEach-Object { $_.FullName })
    $resultNames = @($result.Entries | ForEach-Object { $_.FullName })
    $sameSet = ($sourceNames.Count -eq $resultNames.Count) -and -not (Compare-Object $sourceNames $resultNames)

    $changed = @()
    foreach ($name in $sourceNames) {
        if ((Get-EntrySha $source $name) -ne (Get-EntrySha $result $name)) { $changed += $name }
    }

    $expected = [Security.Cryptography.SHA256]::Create()
    $scriptSha = try { (($expected.ComputeHash([Text.Encoding]::UTF8.GetBytes($patched)) | ForEach-Object { $_.ToString('x2') }) -join '') }
    finally { $expected.Dispose() }

    Write-Host ''
    Write-Host ("built    : {0:N0} bytes" -f (Get-Item -LiteralPath $temp).Length)
    Write-Host ("entries  : {0} source, {1} written, same names and order: {2}" -f $sourceNames.Count, $resultNames.Count, $sameSet)
    Write-Host ("changed  : {0}" -f (($changed -join ', ') -replace '^$', 'none'))

    if (-not $sameSet) { throw 'the written package does not hold the same entries as the source' }
    if ($changed.Count -ne 1 -or $changed[0] -ne $scriptEntry) { throw "expected exactly one changed entry ('$scriptEntry') and got: $($changed -join ', ')" }
    if ((Get-EntrySha $result $scriptEntry) -ne $scriptSha) { throw 'the written script does not hash to the patched text' }

    # Re-read the written line from the written package and show the repair in the engine's own terms.
    $reread = $result.GetEntry($scriptEntry)
    $rs = $reread.Open()
    try {
        $rr = New-Object IO.StreamReader($rs)
        try { $written = $rr.ReadToEnd() }
        finally { $rr.Dispose() }
    }
    finally { $rs.Dispose() }
    $newLines = $written -split "`n"
    $site = -1
    for ($i = 0; $i -lt $newLines.Count; $i++) { if ($newLines[$i] -match 'breathRate') { $site = $i + 1; break } }

    Write-Host ("written  : line {0} reads: {1}" -f $site, $newLines[$site - 1].Trim())
    Write-Host ("delta    : {0:N0} bytes shorter, {1:N0} -> {2:N0} lines (the line count is unchanged)" -f ($entry.Length - $reread.Length), $lines, $newLines.Count)
    if ($written -match 'Random\.Range\(0\.2f, 0\.3f\)') { throw 'the defect is still present in the written script' }
    Write-Host 'verified : one entry rewritten, the other entries byte-identical'
}
finally {
    $source.Dispose()
    $result.Dispose()
}

if ($InPlace) {
    if (Test-Path -LiteralPath $BackupPath) { throw "a backup already exists at $BackupPath - move it away before patching again" }
    [IO.File]::Copy($PackagePath, $BackupPath, $false)
    Write-Host ("backup   : {0}" -f $BackupPath)
}

[IO.File]::Copy($temp, $OutputPath, $true)
Remove-Item -LiteralPath $temp -Force

Write-Host ''
Write-Host ("written  : {0}" -f $OutputPath)
Write-Host ("           {0:N0} bytes" -f (Get-Item -LiteralPath $OutputPath).Length)

if ($NoConfirm) {
    Write-Host 'confirm  : skipped (-NoConfirm) - the package will not load until its prefs answers "pluginsAlwaysEnabled"'
}
else {
    foreach ($root in $prefsRoots) {
        $file = Join-Path $root ($packageUid + '.prefs')
        if (Test-Path -LiteralPath $file) {
            $state = if ([IO.File]::ReadAllText($file) -match '"pluginsAlwaysEnabled"\s*:\s*"true"') { 'enabled' } else { 'NOT enabled - a recorded answer is never overwritten, change it in game if you want this package to load' }
            Write-Host ("confirm  : existing {0} - {1}" -f $file, $state)
        }
        else {
            if (-not (Test-Path -LiteralPath $root)) { $null = New-Item -ItemType Directory -Path $root -Force }
            [IO.File]::WriteAllText($file, $confirmText, (New-Object Text.UTF8Encoding($false)))
            Write-Host ("confirm  : wrote {0} ({1} bytes)" -f $file, (Get-Item -LiteralPath $file).Length)
        }
    }
}

Write-Host ''
if ($InPlace) {
    Write-Host ("rollback : .\scripts\New-E-MotionPatch.ps1 -InPlace -Remove   (restores the .original over {0})" -f (Split-Path -Leaf $OutputPath))
}
else {
    Write-Host ("rollback : .\scripts\New-E-MotionPatch.ps1 -Remove   (deletes {0})" -f (Split-Path -Leaf $OutputPath))
    Write-Host ("note     : a scene only loads this revision if it names {0}; a scene holding the older version keeps using it" -f $packageUid)
}
