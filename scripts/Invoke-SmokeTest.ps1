<#
.SYNOPSIS
Runs the rebuilt game in the editor, in batch mode, and reports what it printed.

.DESCRIPTION
Two gates read the project from the outside:

  * -Method InspectScene opens the boot scene and counts the components that no longer resolve.
    A decompiled-and-recompiled assembly keeps class and field names, so every serialized
    MonoBehaviour reference should still bind; each one that does not appears as a null component.
    Runs headless.
  * -Method Play actually starts the game for -Seconds and then reports. The report ends with the
    state of SuperController, the singleton the whole of VaM hangs off, so the question it answers
    is "how far does the boot get" rather than "is the game finished".

-Method Play needs a real graphics device and is therefore windowed by default: VaM's cloth, hair
and collider systems are compute-shader systems, and under -nographics every shader reports "All
passes removed" and ComputeShader.FindKernel returns -1, so the run dies in
GPUCollidersManager.FixedUpdate before it can report anything. Pass -Headless to force the old
behaviour when all you want to know is whether the scene even starts.

The run deliberately omits -quit. Play mode needs the editor alive, so the gate method exits the
editor itself once it is done; -quit would shut everything down before the first frame.

Batch mode creates no window at all, so the game only renders offscreen and there is nothing to
watch. Pass -Visible to drop -batchmode and open the editor normally: the game renders in its Game
view, which the gate brings to the front. Everything else - the elapsed time, the report, the
self-exit - behaves the same, so a visible run is also the way to see the rebuild with your own eyes.

-Scene loads a scene on top of the boot, -WarmupSeconds after the first frame, through
SuperController.Load - the same call the in-game file browser makes. That replaces clicking through
the UI with a repeatable run: the report then covers the boot plus the scene, and the verdict fails
if any of the scene's own content could not be resolved - "X is missing" when FileManager cannot see
the file at all, "Not ready for load" when a hair or clothing item cannot resolve its store path. The
name is the game's addressing, not a filesystem path:

  * "MeshedVR.BonusScenes.9:/Saves/scene/..." for a scene inside a .var package
  * "Saves/scene/MeshedVR/default.json" for a scene in the install

.EXAMPLE
scripts\Invoke-SmokeTest.ps1 -Method InspectScene
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 30
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 120 -Visible
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 120 -Scene "Saves/scene/MeshedVR/default.json"
#>
[CmdletBinding()]
param(
    [ValidateSet('Report', 'InspectScene', 'Play')]
    [string] $Method      = 'Play',
    [int]    $Seconds     = 30,
    [string] $Scene,
    [int]    $WarmupSeconds = 15,
    [switch] $Headless,
    [switch] $Visible,
    [string] $ProjectPath,
    [string] $LogFile,
    [string] $UnityExe    = (Join-Path ${env:ProgramFiles} 'Unity\Hub\Editor\2018.1.9f2\Editor\Unity.exe'),
    [int]    $TimeoutSec  = 1800
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Split-Path -Parent $root
if (-not $ProjectPath) { $ProjectPath = Join-Path $repo 'VaM_Rebuild' }
if (-not $LogFile)     { $LogFile     = Join-Path $repo ("artifacts\smoke-{0}.log" -f $Method.ToLower()) }

if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath = Join-Path $repo $ProjectPath }
if (-not [System.IO.Path]::IsPathRooted($LogFile))     { $LogFile     = Join-Path $repo $LogFile }
$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$LogFile = [System.IO.Path]::GetFullPath($LogFile)
New-Item -ItemType Directory -Path (Split-Path -Parent $LogFile) -Force | Out-Null

# The gate writes its report both into the log and beside it, so both have to start out empty - a
# stale report would otherwise be read as this run's result when the run fails to start at all.
$reportFile = [System.IO.Path]::ChangeExtension($LogFile, $null) + '.report.txt'
foreach ($stale in @($LogFile, $reportFile)) {
    if (Test-Path -LiteralPath $stale) { Remove-Item -LiteralPath $stale -Force }
}

# Play mode needs a real graphics device; the scene walk does not, and staying headless keeps it
# cheap. -Headless forces the cheap path for both.
$windowed = ($Method -eq 'Play') -and -not $Headless

# -batchmode is what makes a run unobservable: it creates no window at all, so the game renders
# offscreen and there is nothing to watch. -Visible drops it - the editor opens normally, the gate
# still exits it when the run is over, so a visible run does not have to be closed by hand.
$arguments = @(
    '-projectPath', $ProjectPath,
    '-logFile', $LogFile,
    '-executeMethod', "RebuildGate.$Method",
    '-acceptSoftwareTermsForThisRunOnly'
)
if (-not $Visible) { $arguments = @('-batchmode') + $arguments }
if (-not $windowed -and -not $Visible) { $arguments += '-nographics' }
if ($Method -eq 'Play') {
    $arguments += @('-smokeSeconds', $Seconds)
    if ($Scene) {
        # The name goes through the game's own addressing, so it can contain spaces
        # (".../VR Breast Play.json") and dots - quote it for CreateProcess.
        $arguments += @('-smokeWarmup', $WarmupSeconds, '-smokeScene', ('"{0}"' -f $Scene))
    }
}

Write-Host ("Running RebuildGate.{0} on {1} (timeout {2} s)" -f $Method, $ProjectPath, $TimeoutSec)
if ($Visible) { Write-Host 'Visible run: a Unity editor window will open and the game will render in its Game view.' }
if ($Scene)   { Write-Host ("Scene to load after {0} s: {1}" -f $WarmupSeconds, $Scene) }
$process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -WorkingDirectory $ProjectPath -PassThru
if (-not $process.WaitForExit($TimeoutSec * 1000)) {
    $process.Kill()
    throw ("Unity did not finish within {0} s - see {1}" -f $TimeoutSec, $LogFile)
}

$lines = Get-Content -LiteralPath $LogFile

# A run tears play mode down before it exits, and a log still being flushed at that moment loses its
# tail - one run reached 20799 frames and its log had no report at all. The gate writes the same
# report to a sidecar file for exactly that case, so a run is read from there when the log is short.
if (-not (Select-String -LiteralPath $LogFile -Pattern '----- RebuildGate (OK|FAILED) -----' -Quiet)) {
    if (Test-Path -LiteralPath $reportFile) {
        Write-Host ("the log has no verdict - reading the report file {0}" -f $reportFile)
        $lines = @(Get-Content -LiteralPath $reportFile)
    }
}
$header = '----- RebuildGate scene inspection -----', '----- RebuildGate play report -----'
$start = -1
$verdictLine = -1
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($header -contains $lines[$i]) { $start = $i }
    if ($lines[$i] -like '*----- RebuildGate OK -----*' -or $lines[$i] -like '*----- RebuildGate FAILED -----*') { $verdictLine = $i }
}

# The report goes through Debug.Log, so it has to be read out of the log. It is a block of plain
# lines; the first line that looks like the editor's own output ends it - the stack trace and the
# "(Filename: ...)" footer both do.
if ($start -ge 0) {
    Write-Host ''
    for ($i = $start; $i -lt $lines.Count -and $i -lt ($start + 120); $i++) {
        $line = $lines[$i]
        if ($line -like '(Filename:*' -or $line -like 'UnityEngine.*' -or $line -like 'RebuildGate:*' -or $line -like 'Cleanup mono*') { break }
        Write-Host $line
    }
} else {
    Write-Host ''
    Write-Host 'the gate method never ran - the project did not compile or start:'
    Select-String -LiteralPath $LogFile -Pattern ': error CS|Exception:|Compilation failed' |
        Select-Object -First 40 | ForEach-Object { $_.Line.Trim() }
}

Write-Host ''
Write-Host ("log: {0} ({1} lines)" -f $LogFile, $lines.Count)
$ok = $false
if ($verdictLine -ge 0 -and ($start -lt 0 -or $verdictLine -gt $start)) {
    $ok = $lines[$verdictLine] -like '*RebuildGate OK*'
}
if ($ok) { exit 0 }
Write-Host 'verdict: FAILED'
exit 1
