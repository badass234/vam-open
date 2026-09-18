<#
.SYNOPSIS
Writes the solution and project files an IDE opens for VaM_Rebuild.

.DESCRIPTION
There is no .sln in this repository and there is not meant to be one: a solution describes which
assemblies Unity compiles, and only the editor knows that. So the editor is asked for it instead.

RebuildGate.SyncSolution calls the installed code editor integration - the one behind
Preferences > External Tools, supplied by com.unity.ide.rider, com.unity.ide.visualstudio or
com.unity.ide.vscode - which writes VaM_Rebuild.sln plus one .csproj per assembly into the project
root, next to Assets. Without such a package Unity registers its own DefaultExternalCodeEditor,
whose SyncAll() writes nothing at all; the gate then falls back to the built-in Visual Studio
generator, and if the project root is still empty it fails instead of reporting an empty success,
naming what to install.

The generated files are gitignored (VaM_Rebuild/*.sln, VaM_Rebuild/*.csproj): they are build
output, which changes with the editor and the package set, not source.

A second editor on the same project only opens a "project is already open" dialog, so the script
refuses while an editor is running and stops it only when -Force is passed. Unity generates the same
files from its own window as well (Assets > Open C# Project, or the External Tools preference
dialog), which is the way to go when the editor is already open and the session matters.

.EXAMPLE
scripts\Invoke-SyncSolution.ps1
.EXAMPLE
scripts\Invoke-SyncSolution.ps1 -Force
#>
[CmdletBinding()]
param(
    [string] $ProjectPath,
    [string] $LogFile,
    [string] $UnityExe,
    [int]    $TimeoutSec = 900,
    [switch] $Force
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $root 'UnityEditor.ps1')
if (-not $UnityExe) { $UnityExe = Get-VaMOpenUnityExe }
$repo = Split-Path -Parent $root
if (-not $ProjectPath) { $ProjectPath = Join-Path $repo 'VaM_Rebuild' }
if (-not $LogFile)     { $LogFile     = Join-Path $repo 'artifacts\sync-solution.log' }

if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath = Join-Path $repo $ProjectPath }
if (-not [System.IO.Path]::IsPathRooted($LogFile))     { $LogFile     = Join-Path $repo $LogFile }
if (-not (Test-Path -LiteralPath $ProjectPath))        { throw "no Unity project at $ProjectPath" }
if (-not (Test-Path -LiteralPath $UnityExe))           { throw "no Unity editor at $UnityExe" }
$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
New-Item -ItemType Directory -Path (Split-Path -Parent $LogFile) -Force | Out-Null
$LogFile = [System.IO.Path]::GetFullPath($LogFile)
if (Test-Path -LiteralPath $LogFile) { Remove-Item -LiteralPath $LogFile -Force }

$lockFile = Join-Path $ProjectPath 'Temp\UnityLockfile'
$editorProcs = @(Get-Process -Name Unity -ErrorAction SilentlyContinue)
if ($editorProcs.Count -gt 0) {
    if (-not $Force) {
        throw ("{0} Unity editor process(es) are running; close them, generate the solution from the editor itself, or pass -Force to stop them" -f $editorProcs.Count)
    }

    Write-Host ("stopping {0} Unity editor process(es)" -f $editorProcs.Count)
    $editorProcs | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
    Start-Sleep -Seconds 5
}

if (Test-Path -LiteralPath $lockFile) {
    Remove-Item -LiteralPath $lockFile -Force
    Write-Host 'removed a stale Temp\UnityLockfile'
}

$arguments = @(
    '-batchmode', '-nographics', '-quit',
    '-projectPath', $ProjectPath,
    '-logFile', $LogFile,
    '-executeMethod', 'RebuildGate.SyncSolution',
    '-acceptSoftwareTermsForThisRunOnly'
)

Write-Host ("Generating the solution on {0} (timeout {1} s)" -f $ProjectPath, $TimeoutSec)
$process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -PassThru
$finished = $process.WaitForExit($TimeoutSec * 1000)
if (-not $finished) {
    Write-Host ("the editor did not exit within {0} s - stopping it and reading what it wrote" -f $TimeoutSec)
    $process.Kill()
    $process.WaitForExit(30000) | Out-Null
}

# The verdict comes from the marker the method logs, not from the exit code: it can only appear once
# every script assembly compiled and the editor loaded the one the method lives in.
$verdict = $null
if (Test-Path -LiteralPath $LogFile) {
    $lines = @(Get-Content -LiteralPath $LogFile)
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        if ($lines[$i] -like '*----- RebuildGate*-----*') {
            $verdict = if ($lines[$i] -like '*RebuildGate OK*') { 'OK' } else { 'FAILED' }
            break
        }
    }

    Select-String -LiteralPath $LogFile -Pattern '----- RebuildGate|code editor:|fallback|SyncAll\(\) threw|nothing was generated|: error CS' |
        Select-Object -First 40 | ForEach-Object { $_.Line.Trim() }
}

Write-Host ''
Write-Host 'generated files in the project root:'
$generated = @(Get-ChildItem -LiteralPath $ProjectPath -File |
    Where-Object { $_.Name -like '*.sln' -or $_.Name -like '*.csproj' } |
    Sort-Object Name)
foreach ($file in $generated) {
    Write-Host ("  {0}  {1:N0} B  {2:yyyy-MM-dd HH:mm:ss}" -f $file.Name, $file.Length, $file.LastWriteTime)
}
if ($generated.Count -eq 0) { Write-Host '  (none)' }

if ($null -eq $verdict) {
    Write-Host 'verdict: FAILED - the gate method never ran, so the project did not compile'
    exit 1
}
Write-Host ("verdict: {0}" -f $verdict)
if ($generated.Count -eq 0) { exit 1 }
if ($verdict -ne 'OK') { exit 1 }
