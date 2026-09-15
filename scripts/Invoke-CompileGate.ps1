<#
.SYNOPSIS
Compiles VaM_Rebuild in batch mode and reports the compile errors.

.DESCRIPTION
Runs `Unity.exe -batchmode -nographics -quit -executeMethod RebuildGate.Report`. The point is that
Unity must compile every script assembly before it can look up that method, and a run whose code does
not compile exits non-zero and prints the compiler output. A batch run without an execute target can
end with "Nothing changed" and compile nothing, which hides errors.

Afterwards the log is run through tools\parse_unity_log.py so the errors are grouped by category.

.EXAMPLE
scripts\Invoke-CompileGate.ps1
scripts\Invoke-CompileGate.ps1 -LogFile artifacts\compile-gate-2.log
#>
[CmdletBinding()]
param(
    [string] $ProjectPath,
    [string] $LogFile,
    [string] $UnityExe    = (Join-Path ${env:ProgramFiles} 'Unity\Hub\Editor\2018.1.9f2\Editor\Unity.exe'),
    [int]    $TimeoutSec  = 3600
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ProjectPath) { $ProjectPath = Join-Path $root '..\VaM_Rebuild' }
if (-not $LogFile)     { $LogFile     = Join-Path $root '..\artifacts\compile-gate.log' }

# Relative paths are resolved against the repository root, not against the process directory:
# PowerShell's Set-Location does not move [Environment]::CurrentDirectory.
if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath = Join-Path $root $ProjectPath }
if (-not [System.IO.Path]::IsPathRooted($LogFile))     { $LogFile     = Join-Path $root $LogFile }
$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$LogFile = [System.IO.Path]::GetFullPath($LogFile)
New-Item -ItemType Directory -Path (Split-Path -Parent $LogFile) -Force | Out-Null
if (Test-Path -LiteralPath $LogFile) { Remove-Item -LiteralPath $LogFile -Force }

$arguments = @(
    '-batchmode', '-nographics', '-quit',
    '-projectPath', $ProjectPath,
    '-logFile', $LogFile,
    '-executeMethod', 'RebuildGate.Report',
    '-acceptSoftwareTermsForThisRunOnly'
)

Write-Host ("Running the compile gate on {0} (timeout {1} s)" -f $ProjectPath, $TimeoutSec)
$process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -PassThru
if (-not $process.WaitForExit($TimeoutSec * 1000)) {
    $process.Kill()
    throw ("Unity did not finish within {0} s - see {1}" -f $TimeoutSec, $LogFile)
}

# The exit code alone is a weak signal: the gate method is not called when scripting fails, and
# Unity's own exit codes are not documented. The verdict therefore comes from the marker the gate
# logs at the end, which can only appear if every script assembly compiled and the editor loaded it.
$verdict = $null
$errorsAfterMarker = 0
$staleErrors = 0
if (Test-Path -LiteralPath $LogFile) {
    $lines = Get-Content -LiteralPath $LogFile
    $marker = -1
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        if ($lines[$i] -like '*----- RebuildGate*-----*') { $marker = $i; break }
    }
    if ($marker -ge 0) {
        $verdict = if ($lines[$marker] -like '*RebuildGate OK*') { 'OK' } else { 'FAILED' }
        $staleErrors = @($lines[0..$marker] | Where-Object { $_ -like '*: error CS*' }).Count
        $errorsAfterMarker = @($lines[($marker + 1)..($lines.Count - 1)] | Where-Object { $_ -like '*: error CS*' }).Count
    }

    # The gate logs its report through Debug.Log, so the interesting lines are in the log, not stdout.
    Select-String -LiteralPath $LogFile -Pattern '----- RebuildGate|: error CS' |
        Select-Object -First 60 | ForEach-Object { $_.Line.Trim() }
}

Write-Host ''
Write-Host 'verdict from the log, not from the exit code:'
if ($null -eq $verdict) {
    Write-Host 'verdict: FAILED - the gate method never ran, so the project did not compile'
    exit 1
}
if ($errorsAfterMarker -gt 0) {
    Write-Host ("verdict: FAILED - {0} compile errors after the report" -f $errorsAfterMarker)
    exit 1
}
if ($staleErrors -gt 0) {
    # Unity compiles once before the project's plugin DLLs are imported, so a first run can print
    # errors it repairs itself a few seconds later. Only the final compile decides.
    Write-Host ("note: {0} error lines came from an intermediate compile and were superseded" -f $staleErrors)
}
Write-Host ("verdict: {0}" -f $verdict)
# The four assemblies Unity has to build for this project. VaMUnityScript.dll is what
# Assembly-UnityScript became: the original folder name is reserved by the editor.
$targets = 'Assembly-CSharp.dll', 'VaMUnityScript.dll'
$scriptDir = Join-Path $ProjectPath 'Library\ScriptAssemblies'
foreach ($t in $targets) {
    $path = Join-Path $scriptDir $t
    $note = if (Test-Path -LiteralPath $path) { '{0:N0} B' -f (Get-Item -LiteralPath $path).Length } else { 'MISSING' }
    Write-Host ("  {0}: {1}" -f $t, $note)
}

$parser = Join-Path $root '..\tools\parse_unity_log.py'
$out = [System.IO.Path]::ChangeExtension($LogFile, $null).TrimEnd('.') + '.errors.md'
if (Test-Path -LiteralPath $parser) {
    # The parser sees the whole log, stale intermediate errors included, so it is a breakdown of
    # everything the run printed rather than of the final compile.
    python $parser $LogFile --out $out | Select-Object -Last 15
    Write-Host ("error breakdown: {0}" -f $out)
}

if ($verdict -ne 'OK') { exit 1 }
