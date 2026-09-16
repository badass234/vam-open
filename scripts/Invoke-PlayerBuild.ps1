<#
.SYNOPSIS
Builds the standalone player (VAMOpen.exe) in Unity batch mode and reports what came out.

.DESCRIPTION
Runs `Unity.exe -batchmode -nographics -quit -executeMethod RebuildPlayer.Build`. The method is
RebuildPlayer, not RebuildGate: the gate compiles the project and stops, this one compiles the player.

Two things are worth knowing before reading the result:

  * A player build fails on the same errors the gate fails on, plus one the gate never sees - code that
    is only valid in the editor. Anything the runtime scripts touch inside UnityEditor without being
    fenced off by #if UNITY_EDITOR compiles for the editor and not for the player, and the build is
    where that shows up first.
  * The build compiles and links, it does not assemble a runnable product. The player needs the game's
    data next to it, which RebuildPlayer deliberately does not copy; scripts\New-PlayerRuntimeLinks.ps1
    links it in, and without it the player finds no bundles and no scene to load.

The verdict comes from the marker RebuildPlayer logs, not from the exit code: an editor that never
reached the method exits with the same code as a build that failed.

.EXAMPLE
scripts\Invoke-PlayerBuild.ps1
scripts\Invoke-PlayerBuild.ps1 -TimeoutSec 7200
#>
[CmdletBinding()]
param(
    [string] $ProjectPath,
    [string] $LogFile,
    [string] $UnityExe   = (Join-Path ${env:ProgramFiles} 'Unity\Hub\Editor\2018.1.9f2\Editor\Unity.exe'),
    [int]    $TimeoutSec = 3600
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ProjectPath) { $ProjectPath = Join-Path $root '..\VaM_Rebuild' }
if (-not $LogFile)     { $LogFile     = Join-Path $root '..\artifacts\player-build.log' }

if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath = Join-Path $root $ProjectPath }
if (-not [System.IO.Path]::IsPathRooted($LogFile))     { $LogFile     = Join-Path $root $LogFile }
$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$LogFile = [System.IO.Path]::GetFullPath($LogFile)
New-Item -ItemType Directory -Path (Split-Path -Parent $LogFile) -Force | Out-Null
if (Test-Path -LiteralPath $LogFile) { Remove-Item -LiteralPath $LogFile -Force }

# A second editor on the same project is refused by Unity itself, and the refused run writes no
# markers at all - which is indistinguishable from a project that failed to compile. The lock file the
# open editor holds is checked first, so the run can say which of the two happened.
$lock = Join-Path $ProjectPath 'Temp\UnityLockfile'
if (Test-Path -LiteralPath $lock) {
    Write-Host 'the project is open in the editor (Temp\UnityLockfile exists) - close it and run this again' -ForegroundColor Yellow
    exit 2
}

$arguments = @(
    '-batchmode', '-nographics', '-quit',
    '-projectPath', $ProjectPath,
    '-logFile', $LogFile,
    '-executeMethod', 'RebuildPlayer.Build',
    '-acceptSoftwareTermsForThisRunOnly'
)

Write-Host ("Building the player from {0}" -f $ProjectPath)
Write-Host ("timeout {0} s, log {1}" -f $TimeoutSec, $LogFile)
$process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -PassThru
$finished = $process.WaitForExit($TimeoutSec * 1000)
if (-not $finished) {
    Write-Host ("the editor did not exit within {0} s - stopping it and reading what it wrote" -f $TimeoutSec)
    $process.Kill()
    $process.WaitForExit(30000) | Out-Null
}

$lines = @()
if (Test-Path -LiteralPath $LogFile) { $lines = @(Get-Content -LiteralPath $LogFile) }

$failures = @($lines | Where-Object { $_ -like '*----- RebuildPlayer FAILED*' })
$success  = @($lines | Where-Object { $_ -like '*----- RebuildPlayer OK*' })
$errors   = @($lines | Where-Object { $_ -like '*: error CS*' })
$shader   = @($lines | Where-Object { $_ -like '*Shader error*' })

Select-String -LiteralPath $LogFile -Pattern '----- RebuildPlayer|  project:|  output:|  scenes:|  player:|  runtime data|: error CS|Shader error' |
    Select-Object -Last 40 | ForEach-Object { $_.Line.Trim() }

Write-Host ''
Write-Host 'verdict from the log, not from the exit code:'
if ($failures.Count -gt 0) {
    Write-Host ("verdict: FAILED - {0}" -f $failures[-1].Trim())
    if ($errors.Count -gt 0) { Write-Host ("  {0} compile error lines" -f $errors.Count) }
    exit 1
}
if ($success.Count -eq 0) {
    Write-Host 'verdict: FAILED - the build method never ran, so the project or the editor did not get there'
    if ($errors.Count -gt 0) { Write-Host ("  {0} compile error lines" -f $errors.Count) }
    Write-Host 'error breakdown: run scripts\Invoke-CompileGate.ps1 to group the compiler output'
    exit 1
}

$exe = Join-Path $root '..\artifacts\player\VAMOpen.exe'
$exe = [System.IO.Path]::GetFullPath($exe)
if (Test-Path -LiteralPath $exe) {
    $player = [System.IO.Path]::GetDirectoryName($exe)
    $size = (Get-ChildItem -LiteralPath $player -Recurse -File | Measure-Object -Property Length -Sum).Sum
    Write-Host ("verdict: OK - {0} ({1:N2} GB)" -f $exe, ($size / 1GB))
    Write-Host 'next: scripts\New-PlayerRuntimeLinks.ps1, then start the exe'
} else {
    Write-Host ("verdict: OK by the log, but no player at {0}" -f $exe)
    exit 1
}
if ($shader.Count -gt 0) { Write-Host ("note: {0} shader error lines in the log" -f $shader.Count) -ForegroundColor Yellow }
