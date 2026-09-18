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

The run deliberately omits -quit for -Method Play. Play mode needs the editor alive, so the gate method
exits the editor itself once it is done; -quit would shut everything down before the first frame. The
other two methods never enter play mode, so there -quit costs nothing and is what guarantees the run
ends even in a state where the gate method cannot exit the editor.

A run always ends in a verdict, never in an exception. An editor that does not exit within -TimeoutSec
is stopped and its log read anyway - the report the gate already wrote is what decides. An editor that
*does* die in flight leaves no verdict at all, and that is reported as exactly that rather than as a
compile failure; docs\verification.md, failure mode B. A failure
caused by the built assemblies losing Assembly-CSharp.dll (the cascading
`CS0246: SuperController / MeshVR / Battlehub` state) is repaired once by
scripts\Repair-ScriptAssemblies.ps1 and the run is repeated; see docs\verification.md.

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
    [string] $UnityExe,
    [int]    $TimeoutSec  = 1800
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $root 'UnityEditor.ps1')
if (-not $UnityExe) { $UnityExe = Get-VaMOpenUnityExe }
$repo = Split-Path -Parent $root
if (-not $ProjectPath) { $ProjectPath = Join-Path $repo 'VaM_Rebuild' }
if (-not $LogFile)     { $LogFile     = Join-Path $repo ("artifacts\smoke-{0}.log" -f $Method.ToLower()) }

if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath = Join-Path $repo $ProjectPath }
if (-not [System.IO.Path]::IsPathRooted($LogFile))     { $LogFile     = Join-Path $repo $LogFile }
$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$LogFile = [System.IO.Path]::GetFullPath($LogFile)
New-Item -ItemType Directory -Path (Split-Path -Parent $LogFile) -Force | Out-Null

# The game reads its content from the process working directory, which in the editor is the project
# directory: AddonPackages and Custom have to be junctions to the installation, not the empty folders
# the AssetRipper export leaves behind. With empty folders FileManager scans 0 packages and a scene
# named by a package ("MeshedVR.DemoScenes.2:/Saves/...") cannot resolve, so SuperController.Load()
# returns without a word and the run reports "requested=True, taken=False, refused=False" - the
# verdict of a run that never loaded anything. Setup-RebuildProject.ps1 re-makes the links itself,
# but a project assembled before that fix keeps the failure, and it costs half an hour to find.
foreach ($link in 'AddonPackages', 'Custom') {
    $linkPath = Join-Path $ProjectPath $link
    $item = Get-Item -LiteralPath $linkPath -Force -ErrorAction SilentlyContinue
    if (-not $item -or -not ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw ("$linkPath is not a junction to the installation: the game would scan 0 packages and " +
               'every scene load would be dropped in silence. Run: scripts\New-RuntimeDataLinks.ps1')
    }
}

# The gate writes its report both into the log and beside it, so both have to start out empty - a
# stale report would otherwise be read as this run's result when the run fails to start at all.
# [System.IO.Path]::ChangeExtension($path, $null) keeps the trailing dot on PowerShell 5.1, which
# would name a file the gate never writes; the sibling name is derived explicitly instead.
function Get-SiblingPath {
    param([string] $Path, [string] $Suffix)
    return Join-Path (Split-Path -Parent $Path) ([System.IO.Path]::GetFileNameWithoutExtension($Path) + $Suffix)
}
$reportFile = Get-SiblingPath -Path $LogFile -Suffix '.report.txt'
foreach ($stale in @($LogFile, $reportFile)) {
    if (Test-Path -LiteralPath $stale) { Remove-Item -LiteralPath $stale -Force }
}

# Play mode needs a real graphics device; the scene walk does not, and staying headless keeps it
# cheap. -Headless forces the cheap path for both.
$windowed = ($Method -eq 'Play') -and -not $Headless

# The Unity command line for one run. -logFile is a parameter because a failed run may be repeated
# with its log kept aside, so that no attempt can inherit the verdict of the one before it.
function Get-GateArguments {
    param([string] $RunLog)

    # -batchmode is what makes a run unobservable: it creates no window at all, so the game renders
    # offscreen and there is nothing to watch. -Visible drops it - the editor opens normally, the gate
    # still exits it when the run is over, so a visible run does not have to be closed by hand.
    $arguments = @(
        '-projectPath', $ProjectPath,
        '-logFile', $RunLog,
        '-executeMethod', "RebuildGate.$Method",
        '-acceptSoftwareTermsForThisRunOnly'
    )
    if (-not $Visible) { $arguments = @('-batchmode') + $arguments }
    if (-not $windowed -and -not $Visible) { $arguments += '-nographics' }
    if ($Method -eq 'Play') {
        # Play mode needs the editor alive, so -quit is deliberately absent there and the gate method
        # exits the editor itself once the run is over.
        $arguments += @('-smokeSeconds', $Seconds)
        if ($Scene) {
            # The name goes through the game's own addressing, so it can contain spaces
            # (".../VR Breast Play.json") and dots - quote it for CreateProcess.
            $arguments += @('-smokeWarmup', $WarmupSeconds, '-smokeScene', ('"{0}"' -f $Scene))
        }
    } else {
        # Neither of the other two methods enters play mode, so -quit costs nothing and is what
        # guarantees the run ends even in a state where the gate method cannot exit the editor.
        $arguments += '-quit'
    }
    return $arguments
}

# One run and the verdict it wrote, printed as it is read. A run is a function because a failed one
# may be repeated exactly once, after the script-assembly state is repaired - docs\verification.md.
function Invoke-SmokeRun {
    param([string] $RunLog)

    if (Test-Path -LiteralPath $RunLog) { Remove-Item -LiteralPath $RunLog -Force }
    $runReport = Get-SiblingPath -Path $RunLog -Suffix '.report.txt'
    if (Test-Path -LiteralPath $runReport) { Remove-Item -LiteralPath $runReport -Force }

    Write-Host ("Running RebuildGate.{0} on {1} (timeout {2} s)" -f $Method, $ProjectPath, $TimeoutSec)
    if ($Visible) { Write-Host 'Visible run: a Unity editor window will open and the game will render in its Game view.' }
    if ($Scene)   { Write-Host ("Scene to load after {0} s: {1}" -f $WarmupSeconds, $Scene) }

    $process = Start-Process -FilePath $UnityExe -ArgumentList (Get-GateArguments -RunLog $RunLog) -WorkingDirectory $ProjectPath -PassThru
    $finished = $process.WaitForExit($TimeoutSec * 1000)
    if (-not $finished) {
        # An editor that will not exit is not the same thing as a failed run: the report it already
        # wrote is still on disk, so it is stopped and its output judged like any other instead of
        # being thrown away.
        Write-Host ("the editor did not exit within {0} s - stopping it and reading what it wrote" -f $TimeoutSec)
        $process.Kill()
        $process.WaitForExit(30000) | Out-Null
    }

    $lines = if (Test-Path -LiteralPath $RunLog) { @(Get-Content -LiteralPath $RunLog) } else { @() }

    # A run tears play mode down before it exits, and a log still being flushed at that moment loses its
    # tail - one run reached 20799 frames and its log had no report at all. The gate writes the same
    # report to a sidecar file for exactly that case, so a run is read from there when the log is short.
    if (-not (Test-Path -LiteralPath $RunLog) -or
        -not (Select-String -LiteralPath $RunLog -Pattern '----- RebuildGate (OK|FAILED) -----' -Quiet)) {
        if (Test-Path -LiteralPath $runReport) {
            Write-Host ("the log has no verdict - reading the report file {0}" -f $runReport)
            $lines = @(Get-Content -LiteralPath $runReport)
        }
    }
# Every report opens with a header of its own; the version form is matched loosely because it carries
# the Unity version. The verdict line looks similar but never starts with a digit.
$header = '----- RebuildGate scene inspection -----', '----- RebuildGate play report -----', '----- RebuildGate 20*'
$start = -1
$verdictLine = -1
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($header -contains $lines[$i] -or $lines[$i] -like $header[2]) { $start = $i }
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
    # A run that dies in flight leaves no verdict and no report, and the log's last lines are then the
    # game's own output. The gate's phase lines are what tells that apart from a project that never
    # started at all; reading the first as the second sends the reader hunting for CS errors that are
    # not there (artifacts\smoke-play-2019-long.log, 2026-09-18).
    Write-Host ''
    $phase = $lines | Where-Object { $_ -like '----- RebuildGate *' } | Select-Object -Last 1
    if ($phase) {
        Write-Host 'the editor died mid-run - the gate never wrote its report:'
        Write-Host ("  last phase: {0}" -f $phase.Trim())
        $lastLine = $lines | Where-Object { $_.Trim() -ne '' } | Select-Object -Last 1
        Write-Host ("  last log line: {0}" -f $lastLine.Trim())
    } else {
        Write-Host 'the gate method never ran - the project did not compile or start:'
        Select-String -LiteralPath $RunLog -Pattern ': error CS|Exception:|Compilation failed' |
            Select-Object -First 40 | ForEach-Object { $_.Line.Trim() }
    }
}

Write-Host ''
Write-Host ("log: {0} ({1} lines)" -f $RunLog, $lines.Count)
$ok = $false
if ($verdictLine -ge 0 -and ($start -lt 0 -or $verdictLine -gt $start)) {
    $ok = $lines[$verdictLine] -like '*RebuildGate OK*'
}
return [pscustomobject]@{ Ok = $ok; TimedOut = (-not $finished) }
}


$result = Invoke-SmokeRun -RunLog $LogFile

# A failed run has one recoverable cause: the built assemblies lost Assembly-CSharp.dll while the asset
# database still calls the scripts unchanged, so the editor compiles only the editor assembly and every
# use of a type from the game assembly reports as missing. The state is repaired once and the run is
# repeated; a second failure is real and stands. The decision belongs to the repair script, which
# refuses a log whose errors point anywhere but Assets\Editor\RebuildGate.cs.
if (-not $result.Ok) {
    $repair = Join-Path $root 'Repair-ScriptAssemblies.ps1'
    & $repair -ProjectPath $ProjectPath -LogFile $LogFile
    if ($LASTEXITCODE -eq 0) {
        $previous = Get-SiblingPath -Path $LogFile -Suffix '.attempt1.log'
        Move-Item -LiteralPath $LogFile -Destination $previous -Force
        if (Test-Path -LiteralPath $reportFile) {
            Move-Item -LiteralPath $reportFile -Destination (Get-SiblingPath -Path $reportFile -Suffix '.attempt1.txt') -Force
        }
        Write-Host ("the failed run is kept as {0}" -f $previous)
        $result = Invoke-SmokeRun -RunLog $LogFile
    } elseif ($LASTEXITCODE -ne 3) {
        Write-Host ("Repair-ScriptAssemblies.ps1 exited with code {0}" -f $LASTEXITCODE)
    }
}

if ($result.TimedOut) {
    Write-Host 'note: the editor did not exit on its own - it was stopped, and the verdict comes from the report it wrote before that'
}
if ($result.Ok) { exit 0 }
Write-Host 'verdict: FAILED'
exit 1
