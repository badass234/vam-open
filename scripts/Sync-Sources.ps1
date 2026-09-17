#Requires -Version 5.1
<#
.SYNOPSIS
Copies the edited sources from src\ into the Unity project, then says whether the project is ahead of
the compiled assembly.

.DESCRIPTION
src\Assembly-CSharp is where the code is edited; VaM_Rebuild\Assets\Scripts\Assembly-CSharp is what
Unity compiles. Nothing links the two, so an edit that is not copied here is an edit the gate and the
player build never see - and because both report on whatever state the project happens to be in, a
stale project produces a green gate and a player without the change. That is the failure this script
exists to make impossible: it mirrors the tree, then it compares src against the project byte for byte
and refuses to report success unless every file matches.

Only *.cs crosses over. The .cs.meta files in the project carry the GUIDs that scenes and prefabs use
to find a MonoBehaviour, and they come from AssetRipper, not from src\, so they are never written or
deleted here - a script that vanishes without its meta keeps its GUID reserved, which is harmless,
while a meta that vanishes with it would break every reference to that class.

The last two lines are the point of the run: the newest source time against the assembly's, so a run
that compiled nothing can be told from a run that compiled everything.

.EXAMPLE
scripts\Sync-Sources.ps1

.EXAMPLE
scripts\Sync-Sources.ps1 -ProjectPath D:\VaM_Rebuild
#>
[CmdletBinding()]
param(
    [string] $ProjectPath,
    [string] $SourcePath,
    [switch] $WhatIf
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ProjectPath) { $ProjectPath = Join-Path $root '..\VaM_Rebuild' }
if (-not $SourcePath)  { $SourcePath  = Join-Path $root '..\src' }

if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath = Join-Path $root $ProjectPath }
if (-not [System.IO.Path]::IsPathRooted($SourcePath))  { $SourcePath  = Join-Path $root $SourcePath }
$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$SourcePath  = (Resolve-Path -LiteralPath $SourcePath).Path

# The two assemblies the project compiles from src\. Assembly-UnityScript keeps its generated folder
# name; the reserved name is why its output is VaMUnityScript.dll.
$assemblies = 'Assembly-CSharp', 'Assembly-UnityScript'

function Get-RelativePath([string] $Base, [string] $Full) {
    return $Full.Substring($Base.Length + 1)
}

$copied = 0
$removed = 0
$kept = 0
$metaOrphans = New-Object System.Collections.Generic.List[string]
$newestSource = $null
$newestPath = $null

foreach ($assembly in $assemblies) {
    $sourceDir = Join-Path $SourcePath $assembly
    $targetDir = Join-Path $ProjectPath ('Assets\Scripts\' + $assembly)

    if (-not (Test-Path -LiteralPath $sourceDir)) { throw "no sources at $sourceDir" }
    if (-not (Test-Path -LiteralPath $targetDir)) { throw "the project has no $assembly under Assets\Scripts - run Setup-RebuildProject.ps1 first" }

    $sources = @(Get-ChildItem -LiteralPath $sourceDir -Recurse -Filter '*.cs' -File)
    $targets = @(Get-ChildItem -LiteralPath $targetDir -Recurse -Filter '*.cs' -File)
    $kept = 0

    $wanted = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($file in $sources) { [void]$wanted.Add((Get-RelativePath $sourceDir $file.FullName)) }

    foreach ($file in $sources) {
        $relative = Get-RelativePath $sourceDir $file.FullName
        $dest = Join-Path $targetDir $relative
        if ($file.LastWriteTime -gt $newestSource) { $newestSource = $file.LastWriteTime; $newestPath = $relative }

        $same = $false
        if (Test-Path -LiteralPath $dest) {
            # Length first: it is a cheap way to skip the comparison for a file that has clearly moved on.
            $existing = Get-Item -LiteralPath $dest
            $same = ($existing.Length -eq $file.Length) -and
                    ([System.Linq.Enumerable]::SequenceEqual(
                        [System.IO.File]::ReadAllBytes($file.FullName),
                        [System.IO.File]::ReadAllBytes($dest)))
        }

        if ($same) { $kept++; continue }
        if ($WhatIf) { Write-Host ("  would copy {0}" -f $relative); $copied++; continue }

        $parent = Split-Path -Parent $dest
        if (-not (Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
        Copy-Item -LiteralPath $file.FullName -Destination $dest -Force
        $copied++
    }

    foreach ($file in $targets) {
        $relative = Get-RelativePath $targetDir $file.FullName
        if ($wanted.Contains($relative)) { continue }
        if ($WhatIf) { Write-Host ("  would delete {0}" -f $relative); $removed++; continue }
        # The script is gone from src\, so it must not stay behind and compile. Its .meta is left alone
        # so no GUID that a scene already references is released.
        Remove-Item -LiteralPath $file.FullName -Force
        if (Test-Path -LiteralPath "$($file.FullName).meta") { $metaOrphans.Add($relative) }
        $removed++
    }

    Write-Host ("{0}: {1} source(s), {2} unchanged" -f $assembly, $sources.Count, $kept)
}

if ($WhatIf) {
    Write-Host ("would copy {0}, would delete {1}" -f $copied, $removed)
    exit 0
}

Write-Host ("copied {0}, deleted {1}, already identical {2}" -f $copied, $removed, $kept)

# The check that matters: what the project holds must equal what src\ holds, file for file. A mismatch
# here means the gate and the player build are about to report on code that is not the code on disk.
$mismatches = New-Object System.Collections.Generic.List[string]
$verified = 0
foreach ($assembly in $assemblies) {
    $sourceDir = Join-Path $SourcePath $assembly
    $targetDir = Join-Path $ProjectPath ('Assets\Scripts\' + $assembly)
    foreach ($file in Get-ChildItem -LiteralPath $sourceDir -Recurse -Filter '*.cs' -File) {
        $relative = Get-RelativePath $sourceDir $file.FullName
        $dest = Join-Path $targetDir $relative
        if (-not (Test-Path -LiteralPath $dest)) { $mismatches.Add("$assembly\$relative (missing in the project)"); continue }
        $existing = Get-Item -LiteralPath $dest
        $equal = ($existing.Length -eq $file.Length) -and
                 ([System.Linq.Enumerable]::SequenceEqual(
                     [System.IO.File]::ReadAllBytes($file.FullName),
                     [System.IO.File]::ReadAllBytes($dest)))
        if ($equal) { $verified++ } else { $mismatches.Add("$assembly\$relative") }
    }
}

Write-Host ''
if ($mismatches.Count -gt 0) {
    Write-Host ("verdict: FAILED - {0} file(s) still differ from src after the copy" -f $mismatches.Count) -ForegroundColor Red
    $mismatches | Select-Object -First 20 | ForEach-Object { Write-Host ("  " + $_) }
    exit 1
}
Write-Host ("verdict: OK - {0} source(s) in the project are identical to src" -f $verified)

if ($metaOrphans.Count -gt 0) {
    Write-Host ("note: {0} deleted script(s) left a .cs.meta behind, keeping their GUID reserved" -f $metaOrphans.Count)
}

# A gate or a build only proves something about the code if the assembly was compiled after the last
# edit. These two numbers are how that is checked afterwards, or blamed for a missing change.
$assemblyDll = Join-Path $ProjectPath 'Library\ScriptAssemblies\Assembly-CSharp.dll'
if ($newestSource) {
    Write-Host ("newest source: {0} ({1:yyyy-MM-dd HH:mm:ss})" -f $newestPath, $newestSource)
} else {
    Write-Host 'newest source: none found'
}
if (Test-Path -LiteralPath $assemblyDll) {
    $dll = Get-Item -LiteralPath $assemblyDll
    $state = if ($newestSource -and $dll.LastWriteTime -lt $newestSource) { 'STALE - the assembly predates the sources, recompile before trusting any result' } else { 'newer than every source, so a green gate is about these sources' }
    Write-Host ("Assembly-CSharp.dll: {0:yyyy-MM-dd HH:mm:ss} - {1}" -f $dll.LastWriteTime, $state)
} else {
    Write-Host 'Assembly-CSharp.dll: not built yet'
}
