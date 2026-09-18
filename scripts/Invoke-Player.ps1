<#
.SYNOPSIS
Starts the built player from its own folder, and reports what the run did.

.DESCRIPTION
The player resolves everything it loads relative to the process working directory: FileManager looks
for AddonPackages, AddonPackagesUserPrefs, Custom and Saves by a relative path, CacheManager creates
Cache there, and SuperController reads Keys/1.21/key.json, prefs.json and the version file the same
way. Windows gives a process started from Explorer the folder it lives in, so double-clicking the exe
is correct by construction - but a shell that happens to sit somewhere else (an elevated one starts
in C:\Windows\system32) silently strips the player of all of it: the main menu comes up with no
content, every relative path fails with UnauthorizedAccessException, and nothing on screen says why.

That is not a hypothesis, and it was not the crash it looked like. Both logs the player wrote under
2019.4 read

    UnauthorizedAccessException: Access to the path "C:\WINDOWS\system32\Custom" is denied

never printed "Scanned <n> packages" and never loaded a scene, because that is how they were started;
neither wrote a crash dump, so neither crashed at all. docs\verification.md, "The player has to be
started from its own folder", carries the reading in full.

This script removes the trap. It verifies the runtime links (New-PlayerRuntimeLinks.ps1, idempotent),
refuses to start while another player is running - two of them would share one log file and one Cache
- and starts the exe with -WorkingDirectory set to the player folder, so what is on screen is the game
and not a shell accident.

With -Seconds the run is also watched: the player is stopped after that long and its log is read back.
That is what makes a crash reproducible instead of remembered - the scene it was loading, the last
message before the log ends, the exceptions, and any Windows crash dump written while it ran.

The log is the player's own, %USERPROFILE%\AppData\LocalLow\<company>\<product>\Player.log, with the
company and product read out of ProjectSettings.asset so that a run by hand stays comparable with the
reference logs; -LogFile overrides where the player writes it.

Exit codes: 0 the run looked healthy, 1 the call or the player folder is wrong, 2 the run showed the
wrong-folder symptom or left a crash dump. Run without -Seconds nothing is judged - the player is
started detached and the script returns.

.EXAMPLE
scripts\Invoke-Player.ps1
.EXAMPLE
scripts\Invoke-Player.ps1 -Seconds 120
.EXAMPLE
scripts\Invoke-Player.ps1 -Seconds 180 -PlayerArguments '-vrmode none'
#>
[CmdletBinding()]
param(
    [string]   $PlayerPath,
    [string]   $ExeName = 'VAMOpen.exe',
    [int]      $Seconds = 0,
    [string[]] $PlayerArguments,
    [string]   $LogFile,
    [string]   $InstallRoot,
    [switch]   $SkipLinks,
    [switch]   $Force
)

$ErrorActionPreference = 'Stop'

$scripts = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo    = Split-Path -Parent $scripts

if (-not $PlayerPath) { $PlayerPath = Join-Path $repo 'artifacts\player' }
if (-not [IO.Path]::IsPathRooted($PlayerPath)) { $PlayerPath = Join-Path $repo $PlayerPath }
if (-not (Test-Path -LiteralPath $PlayerPath)) {
    throw "no player folder at $PlayerPath - build it first with scripts\Invoke-PlayerBuild.ps1"
}

$exe = Join-Path $PlayerPath $ExeName
if (-not (Test-Path -LiteralPath $exe)) {
    throw "no player at $exe - build it first with scripts\Invoke-PlayerBuild.ps1"
}

$processName = [IO.Path]::GetFileNameWithoutExtension($ExeName)

# One player at a time. Unity rotates Player.log to Player-prev.log on start and both instances share
# the same Cache directory, so a second one costs the log of the first - which is the whole point of
# running the player from here.
$running = @(Get-Process -Name $processName -ErrorAction SilentlyContinue)
if ($running.Count -gt 0) {
    if (-not $Force) {
        throw ("{0} player process(es) are already running; close them or pass -Force to stop them" -f $running.Count)
    }

    Write-Host ("stopping {0} player process(es)" -f $running.Count)
    $running | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
    Start-Sleep -Seconds 3
}

if (-not $SkipLinks) {
    Write-Host 'verifying the runtime links the player loads its data through'
    $linkArguments = @{ PlayerPath = $PlayerPath; ExeName = $ExeName }
    if ($InstallRoot) { $linkArguments.InstallRoot = $InstallRoot }
    & (Join-Path $scripts 'New-PlayerRuntimeLinks.ps1') @linkArguments
    Write-Host ''
}

# The log path is the player's own and not a choice: a run by hand has to be comparable with the
# reference logs, so the company and product names are read out of the project rather than written
# down here a second time.
if (-not $LogFile) {
    $company = 'MeshedVR'
    $product = 'VaM'
    $projectSettings = Join-Path $repo 'VaM_Rebuild\ProjectSettings\ProjectSettings.asset'
    if (Test-Path -LiteralPath $projectSettings) {
        $settingsText = Get-Content -LiteralPath $projectSettings -Raw
        if ($settingsText -match '(?m)^\s*companyName:\s*(\S+)\s*$') { $company = $Matches[1] }
        if ($settingsText -match '(?m)^\s*productName:\s*(\S+)\s*$') { $product = $Matches[1] }
    }
    $LogFile = Join-Path $env:USERPROFILE ("AppData\LocalLow\{0}\{1}\Player.log" -f $company, $product)
}

if (-not (Test-Path -LiteralPath (Split-Path -Parent $LogFile))) {
    Write-Warning "no log folder at $(Split-Path -Parent $LogFile) - the player has never run on this machine"
}

$arguments = @()
if ($PlayerArguments) { $arguments += $PlayerArguments }
if ($LogFile) { $arguments += @('-logFile', ('"{0}"' -f $LogFile)) }

$startArguments = @{
    FilePath         = $exe
    WorkingDirectory = $PlayerPath
    PassThru         = $true
}
if ($arguments.Count -gt 0) { $startArguments.ArgumentList = $arguments }

Write-Host '----- starting the player -----'
Write-Host ("exe:          {0}" -f $exe)
Write-Host ("working dir:  {0}" -f $PlayerPath)
Write-Host ("log:          {0}" -f $LogFile)

$startedAt = Get-Date
$process = Start-Process @startArguments
Write-Host ("pid:          {0}" -f $process.Id)

if ($Seconds -le 0) {
    Write-Host ''
    Write-Host 'the player is running; it is not watched and nothing is judged' -ForegroundColor Green
    Write-Host 'stop it from the game, or close the window, when the testing is done'
    exit 0
}

# A crash dump is the machine's, not the player's: Windows writes them into LocalDumps when the
# registry asks for it, and one newer than the start of this run is the only hard evidence a crash
# leaves behind. A dump that predates the run belongs to somebody else's session.
function Get-NewCrashDumps([datetime]$Since) {
    $folder = Join-Path $env:LOCALAPPDATA 'CrashDumps'
    if (-not (Test-Path -LiteralPath $folder)) { return @() }
    @(Get-ChildItem -LiteralPath $folder -Filter "$processName.*.dmp" -ErrorAction SilentlyContinue |
        Where-Object { $_.LastWriteTime -ge $Since })
}

Write-Host ''
Write-Host ("watching for {0} s" -f $Seconds)
$deadline = (Get-Date).AddSeconds($Seconds)
while (-not $process.HasExited -and (Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 2
    $process.Refresh()
}

$exited = $process.HasExited
$elapsed = [int]((Get-Date) - $startedAt).TotalSeconds

if ($exited) {
    $how = ("exited on its own after {0} s (exit code {1})" -f $elapsed, $process.ExitCode)
}
else {
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
    $how = ("stopped by this script after {0} s" -f $elapsed)
}

# Unity rotates the log on start, so everything in it belongs to the run that has just ended - no
# need to remember an offset, and Player-prev.log is still there if the previous run has to be read.
$lines = @()
if (Test-Path -LiteralPath $LogFile) { $lines = @(Get-Content -LiteralPath $LogFile -ErrorAction SilentlyContinue) }

$scanned  = @($lines | Select-String -Pattern 'Scanned \d+ packages' | Select-Object -Last 1)
$loads    = @($lines | Select-String -Pattern '^\s*(Load|Unload) ' | Select-Object -Last 4)
$denied   = @($lines | Select-String -Pattern 'UnauthorizedAccessException')
$excepted = @($lines | Select-String -Pattern 'Exception')
$lastLine = @($lines | Where-Object { $_.Trim() } | Select-Object -Last 1)
$dumps    = @(Get-NewCrashDumps $startedAt)

Write-Host ''
Write-Host '----- player run -----'
Write-Host ("exe:          {0}" -f $exe)
Write-Host ("working dir:  {0}" -f $PlayerPath)
Write-Host ("log:          {0}" -f $LogFile)
Write-Host ("state:        {0}" -f $how)

if ($lines.Count -eq 0) {
    Write-Host 'log:          no log was written at all' -ForegroundColor Red
}
else {
    if ($scanned.Count -gt 0) { Write-Host ("packages:     {0}" -f $scanned[0].Line.Trim()) -ForegroundColor Green }
    else { Write-Host 'packages:     no "Scanned <n> packages" in the log' -ForegroundColor Yellow }

    foreach ($load in $loads) { Write-Host ("scene:        {0}" -f $load.Line.Trim()) }

    Write-Host ("exceptions:   {0} line(s) ({1} of them UnauthorizedAccessException)" -f $excepted.Count, $denied.Count)
    if ($lastLine.Count -gt 0) { Write-Host ("last message: {0}" -f $lastLine[0].Trim()) }
}

if ($dumps.Count -gt 0) {
    foreach ($dump in $dumps) {
        Write-Host ("crash dump:   {0} ({1:N1} MB, {2:HH:mm:ss})" -f $dump.FullName, ($dump.Length / 1MB), $dump.LastWriteTime) -ForegroundColor Red
    }
}

$exitCode = 0
if ($dumps.Count -gt 0) {
    Write-Host 'verdict:      the player crashed; the dump above is the evidence' -ForegroundColor Red
    $exitCode = 2
}
elseif ($lines.Count -eq 0) {
    Write-Host 'verdict:      the player wrote no log - it did not get as far as initializing' -ForegroundColor Red
    $exitCode = 2
}
elseif ($denied.Count -gt 0) {
    Write-Host 'verdict:      the player could not reach its data - check the working directory above' -ForegroundColor Red
    $exitCode = 2
}
elseif ($scanned.Count -gt 0) {
    Write-Host 'verdict:      the player found its data and scanned its packages' -ForegroundColor Green
}
else {
    Write-Host 'verdict:      the player started but never scanned packages - read the log' -ForegroundColor Yellow
}

exit $exitCode
