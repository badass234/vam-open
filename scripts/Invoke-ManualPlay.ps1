<#
.SYNOPSIS
Opens the rebuilt game in the editor's play mode and leaves it there for testing by hand.

.DESCRIPTION
Invoke-SmokeTest.ps1 answers "how far does the boot get" and then tears itself down: it runs for
-Seconds, writes a report and exits the editor. That is the wrong shape for looking at the render,
because the interesting part of the look work - does a seam move, does the scalp sit on the head,
does the cloth shade the right way - is something a person does, by eye, in a scene they can drive.

This script is the other half of that: RebuildGate.ManualPlay boots the game, waits for the game's
own boot scene to finish loading, requests the scene given by -Scene through the same
SuperController.Load call the in-game file browser makes, and then stops. No deadline, no report
file, no EditorApplication.Exit - the editor stays in play mode until it is stopped by hand, and
the gate disarms itself when it is.

-Scene defaults to the game's own boot scene, Saves/scene/MeshedVR/default.json, which is the scene
to use: it is the one the original game itself boots into, and it loads with nothing missing. Other
scenes are for the gates that audit them, not for eyeballing - opening several scenes in one editor
session has crashed the player.

The editor is started detached, so this script returns as soon as the process is up.

.EXAMPLE
scripts\Invoke-ManualPlay.ps1
scripts\Invoke-ManualPlay.ps1 -WarmupSeconds 30
scripts\Invoke-ManualPlay.ps1 -Scene "MeshedVR.DemoScenes.2:/Saves/scene/MeshedVR/DemoScenes/Cyber/CyberDemoAlt.json" -Force
#>
[CmdletBinding()]
param(
    [string] $Scene         = 'Saves/scene/MeshedVR/default.json',
    [int]    $WarmupSeconds = 20,
    [string] $ProjectPath,
    [string] $LogFile,
    [string] $UnityExe      = (Join-Path ${env:ProgramFiles} 'Unity\Hub\Editor\2018.1.9f2\Editor\Unity.exe'),
    [switch] $Force
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Split-Path -Parent $root
if (-not $ProjectPath) { $ProjectPath = Join-Path $repo 'VaM_Rebuild' }
if (-not $LogFile)     { $LogFile     = Join-Path $repo 'artifacts\manual-play.log' }

if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath = Join-Path $repo $ProjectPath }
if (-not [System.IO.Path]::IsPathRooted($LogFile))     { $LogFile     = Join-Path $repo $LogFile }
if (-not (Test-Path -LiteralPath $ProjectPath))        { throw "no Unity project at $ProjectPath" }
if (-not (Test-Path -LiteralPath $UnityExe))           { throw "no Unity editor at $UnityExe" }
New-Item -ItemType Directory -Path (Split-Path -Parent $LogFile) -Force | Out-Null
$LogFile = [System.IO.Path]::GetFullPath($LogFile)

# One editor per project. A second instance opens a "project is already open" dialog and the run
# never starts, so the script refuses instead of leaving that dialog on screen - and removes the
# lockfile Unity leaves behind when an editor is killed, which is what would block the next start.
$lockFile = Join-Path $ProjectPath 'Temp\UnityLockfile'
$editorProcs = @(Get-Process -Name Unity -ErrorAction SilentlyContinue)
if ($editorProcs.Count -gt 0) {
    if (-not $Force) {
        throw ("{0} Unity editor process(es) are running; close them or pass -Force to stop them" -f $editorProcs.Count)
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
    '-projectPath', $ProjectPath,
    '-logFile', $LogFile,
    '-executeMethod', 'RebuildGate.ManualPlay',
    '-smokeWarmup', $WarmupSeconds,
    '-acceptSoftwareTermsForThisRunOnly'
)
if ($Scene) {
    # The name goes through the game's own addressing, so it can contain spaces and dots - quote it.
    $arguments += @('-smokeScene', ('"{0}"' -f $Scene))
}

Write-Host ("Starting the editor on {0}" -f $ProjectPath)
Write-Host ("Scene to load after {0} s: {1}" -f $WarmupSeconds, $Scene)
Write-Host 'The editor enters play mode by itself; it has no deadline and writes no report.'
$process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -WorkingDirectory $ProjectPath -PassThru
Write-Host ("Editor PID {0}, log {1}" -f $process.Id, $LogFile)
Write-Host 'Stop play mode (or close the editor) when the testing is done.'
