<#
.SYNOPSIS
Repairs the editor's script compilation state that makes a batch gate fail with cascading errors.

.DESCRIPTION
Unity decides whether to compile a script assembly from state it keeps in `Library`: the assembly
files in `Library\ScriptAssemblies`, the asset database and the recorded timestamps of the sources.
That state can disagree with the sources on disk - `Library\ScriptAssemblies` loses
`Assembly-CSharp.dll` while the database still believes every script is unchanged. The next batch run
then compiles only `Assembly-CSharp-Editor.dll`, whose compiler command line has no
`-r:Library/ScriptAssemblies/Assembly-CSharp.dll`, so `Assets\Editor\RebuildGate.cs` dies with a wall
of cascading errors:

    Assets/Editor/RebuildGate.cs(300,13): error CS0246: The type or namespace name 'SuperController' could not be found

Nothing is wrong with those sources: the assembly that declares `SuperController`, `MeshVR` and
`Battlehub` is simply not in the reference list. This script performs the recovery that used to be done
by hand:

  * it deletes the built assemblies in `Library\ScriptAssemblies`, so they have to be built again;
  * it moves the timestamp of every source under `Assets\Scripts` forward, so the editor treats the
    scripts as changed and schedules a full recompilation instead of trusting its own cache.

Nothing else is touched: no project setting, and no source content - a timestamp is all that changes.

**When it repairs, and when it refuses.** With `-LogFile` the script also requires the evidence that
the failed run failed *this* way, because a genuine compile error inside `Assembly-CSharp` produces the
same missing reference. There are two signatures, because the editor reacts differently depending on
whether it decides to recompile:

  * it recompiles only the editor assembly, and `Assets\Editor\RebuildGate.cs` dies with a wall of
    `CS0246: The type or namespace name 'SuperController' / 'MeshVR' / 'Battlehub' could not be found`;
  * it recompiles nothing and simply cannot load the editor assembly at all -
    `Unloading broken assembly Library/ScriptAssemblies/Assembly-CSharp-Editor.dll`, followed by
    `executeMethod class 'RebuildGate' could not be found` and no compiler output whatsoever.

Both are accepted, and both are rejected the moment the log contains a single compile error anywhere
but `Assets\Editor\RebuildGate.cs`: then `Assembly-CSharp.dll` is missing for a real reason and
repeating the run would only cost time.

The exit code is the answer, so a caller can act on it:

| Code | Meaning | What the caller should do |
|---|---|---|
| 0 | the state was present and is repaired | run the same gate once more |
| 3 | not that state | trust the run's own verdict |
| 1 | the project could not be read | fix the invocation |

.EXAMPLE
scripts\Repair-ScriptAssemblies.ps1 -LogFile artifacts\compile-gate.log
#>
[CmdletBinding()]
param(
    [string] $ProjectPath,
    [string] $LogFile
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Split-Path -Parent $root
if (-not $ProjectPath) { $ProjectPath = Join-Path $repo 'VaM_Rebuild' }

# Relative paths are resolved against the repository root, not against the process directory:
# PowerShell's Set-Location does not move [Environment]::CurrentDirectory.
if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath = Join-Path $repo $ProjectPath }
if ($LogFile -and -not [System.IO.Path]::IsPathRooted($LogFile)) { $LogFile = Join-Path $repo $LogFile }

if (-not (Test-Path -LiteralPath $ProjectPath)) { throw ("no project at {0}" -f $ProjectPath) }
$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path

$scriptDir = Join-Path $ProjectPath 'Library\ScriptAssemblies'
$sourcesDir = Join-Path $ProjectPath 'Assets\Scripts'
$gameAssembly = Join-Path $scriptDir 'Assembly-CSharp.dll'
$editorAssembly = Join-Path $scriptDir 'Assembly-CSharp-Editor.dll'

if (-not (Test-Path -LiteralPath (Join-Path $ProjectPath 'Assets'))) {
    throw ("{0} does not look like a Unity project - no Assets folder" -f $ProjectPath)
}

# The state itself: the editor assembly is there, the assembly it has to reference is not. A project
# that has never been compiled has neither, and is not repaired.
if (Test-Path -LiteralPath $gameAssembly) {
    Write-Host 'Assembly-CSharp.dll is present - the script assemblies have nothing to repair'
    exit 3
}
if (-not (Test-Path -LiteralPath $editorAssembly)) {
    Write-Host 'Assembly-CSharp-Editor.dll is missing too - the project has simply not been compiled yet'
    exit 3
}

# The evidence, when a log is given. A real error in src\ produces the same cascade, so the log has to
# name only the editor script and the types that live in the missing assembly.
if ($LogFile) {
    if (-not (Test-Path -LiteralPath $LogFile)) {
        Write-Host ("no log at {0} - nothing to read the failure from" -f $LogFile)
        exit 3
    }
    $errors = @(Select-String -LiteralPath $LogFile -Pattern ': error CS')
    $cascade = @($errors | Where-Object { $_.Line -match "error CS0246: The type or namespace name '(SuperController|MeshVR|Battlehub)" })
    $elsewhere = @($errors | Where-Object { $_.Line -notlike '*RebuildGate.cs*' })
    # The other way the same state shows itself: the editor assembly is present but cannot load, so
    # nothing is compiled at all and the gate method is simply not found.
    $orphaned = @(Select-String -LiteralPath $LogFile -Pattern 'Unloading broken assembly .*Assembly-CSharp-Editor\.dll')
    Write-Host ("compile errors in the log: {0}, of them cascading: {1}, in any other file: {2}, broken editor assembly: {3}" -f $errors.Count, $cascade.Count, $elsewhere.Count, $orphaned.Count)
    if ($elsewhere.Count -gt 0) {
        Write-Host 'the log has compile errors outside Assets\Editor\RebuildGate.cs - Assembly-CSharp really failed to build, no repair'
        exit 3
    }
    if ($cascade.Count -lt 3 -and -not ($orphaned.Count -gt 0 -and $errors.Count -eq 0)) {
        Write-Host 'the run did not fail on the missing Assembly-CSharp reference - no repair'
        exit 3
    }
}

$removed = 0
if (Test-Path -LiteralPath $scriptDir) {
    foreach ($file in Get-ChildItem -LiteralPath $scriptDir -File) {
        Remove-Item -LiteralPath $file.FullName -Force
        $removed++
    }
}
Write-Host ("deleted {0} file(s) from Library\ScriptAssemblies" -f $removed)

# Only the timestamp moves - the bytes are left exactly as the decompiler wrote them. The editor
# compares a source against the timestamp it recorded, so this is what schedules the recompilation.
$stamp = Get-Date
$touched = 0
foreach ($source in Get-ChildItem -LiteralPath $sourcesDir -Recurse -File -Filter *.cs) {
    $source.LastWriteTime = $stamp
    $touched++
}
if ($touched -eq 0) {
    Write-Host 'no sources under Assets\Scripts to touch - the project cannot be recompiled'
    exit 1
}
Write-Host ("moved the timestamp of {0} source(s) under Assets\Scripts" -f $touched)
Write-Host 'repaired - run the same gate once more'
exit 0
