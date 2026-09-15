<#
.SYNOPSIS
Compares the rebuilt game's boot log against the log the original game wrote.

.DESCRIPTION
Stage 5 is about whether the rebuild behaves like the original, and the original leaves exactly the
evidence needed: VaM writes every Debug.Log it makes to

    %USERPROFILE%\AppData\LocalLow\MeshedVR\VaM\output_log.txt

Boot the original game once, run RebuildGate.Play (scripts\Invoke-SmokeTest.ps1 -Method Play), and
the two logs can be lined up line by line. The point is not the exact wording - it is that the same
things happen in the same order: the asset manager comes up, the VR rig is probed, the package
scan runs, the UI initialises.

Because the gate reports through `Debug.Log`, its own block is dropped whole (from the report header
to the OK/FAILED marker) rather than line by line, so a new diagnostic added to the report can never
show up as a fake difference. Durations are normalised (`took 0.6 ms` -> `took <ms>`) because two runs
of the same code never take the same number of milliseconds.

.EXAMPLE
scripts\Compare-BootLogs.ps1
scripts\Compare-BootLogs.ps1 -Candidate artifacts\smoke-play.log
#>
[CmdletBinding()]
param(
    [string] $Reference = "$env:USERPROFILE\AppData\LocalLow\MeshedVR\VaM\output_log.txt",
    [string] $Candidate,
    [switch] $ShowAligned
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $Candidate) { $Candidate = Join-Path $root 'artifacts\smoke-play.log' }
foreach ($p in @($Reference, $Candidate)) {
    if (-not (Test-Path -LiteralPath $p)) { throw ("no such log: {0}" -f $p) }
}

# Two kinds of line describe the tool rather than the game: the editor's own start-up chatter, which
# a player build never prints, and the gate's report, which the original obviously never printed.
# Everything here is dropped from both sides. The report itself is skipped as a block in
# Read-Messages; these patterns only catch the markers and stray lines around it.
$noise = @(
    '^\s*$',
    '^ ',                                          # stack traces and the report's detail lines
    '^UnityEngine\.', '^UnityEditor\.', '^MeshVR\.', '^MVR\.', '^SuperController', '^GPUTools',
    '^Battlehub', '^System\.', '^RebuildGate', '^<', '^at ',
    '^\(Filename:',
    '^-+ RebuildGate',
    '^played:', '^errors and exceptions:', '^loaded scenes:', '^mono behaviours',
    '^SuperController\.singleton',

    # the editor's start-up and shut-down chatter
    '^Loading GUID', '^Loading Asset Database', '^Loading previous', '^Refreshing native plugins',
    '^Preloading \d+ native plugins', '^Unloading ', '^Reloading assemblies', '^Begin MonoManager',
    '^- Completed reload', '^Mono: ', '^Mono config path', '^Mono path\[', '^Initialize mono',
    '^Initializing Unity extensions', '^Initializing Unity\.PackageManager',
    '^Registering platform support', '^Register platform support module', '^Registered platform',
    '^Registered in ', '^Registering precompiled', '^Native extension for',
    "^Load scene '", '^Opening scene', '^\[Package Manager\]', '^DisplayProgressbar',
    '^Total: \d', '^System memory in use', '^Setting up \d+ worker threads',
    '^Packing sprites', '^\[\s*\d+ MB \]', '^Created GICache', '^WARNING: Shader Unsupported',
    '^Cleanup mono', '^Initialize engine version', '^GfxDevice', '^Direct3D',
    '^    Version', '^    Renderer', '^    Vendor', '^    VRAM', '^    Driver', '^    Thread',
    '^XR: OpenVR Error', '^<RI>',
    '^\[Performance\]', '^\[C:\\buildslave', '^##utp:', '^Assertion failed', '^Audio: FMOD',
    '^BatchMode:', '^Built from ', '^Checking for leaked weakptr', '^GetVirtualKey',
    '^LICENSE SYSTEM', "^OS: '", '^Successfully changed project path', '^Using monoOptions',
    '^Validating Project structure', '^Warming cache', '^AssetDatabase consistency checks',
    '^Refresh completed', '^Refresh: detecting', '^UnloadTime:', '^DisplayProgressbar',
    # The chatter a GUI editor makes and batch mode never does: -Visible runs pick these up.
    '^EditorUpdateCheck', '^Issue TrimJob', '^TrimDiskCacheJob', '^IsTimeToCheckForNewEditor',
    '^Launched and connected shader compiler', '^Opening scene ',
    '^\d+$',                                       # the bare duration that follows game arguments
    '^-[a-zA-Z]',                                  # the command line itself
    '^[A-Za-z]:[\\/]'                              # the paths on the command line
) -join '|'

function Read-Messages([string] $path) {
    $messages = New-Object System.Collections.Generic.List[string]
    $inReport = $false
    foreach ($line in (Get-Content -LiteralPath $path)) {
        # The gate prints its whole report as one multi-line message. Skip it as a block rather than
        # listing every line it contains, so adding a diagnostic to the report cannot leak into the
        # comparison as a fake difference.
        if ($line -match '^-{5} RebuildGate .*(report|inspection)') { $inReport = $true; continue }
        if ($inReport) {
            if ($line -match '^-{5} RebuildGate (OK|FAILED)') { $inReport = $false }
            continue
        }
        if ($line -match $noise) { continue }

        # Durations are the one thing that legitimately differs between two runs, so drop the numbers
        # and keep the sentence. Without this, "Scanned 9 packages in 93.9 ms" and the original's
        # "Scanned 9 packages in 50.3 ms" look like two different events instead of the same one.
        $message = $line.Trim()
        $message = $message -replace '(took|in) \d+(\.\d+)? ms', '$1 <ms>'

        # The benchmark line is nothing but averages, and none of them repeat between two runs.
        if ($message -like 'Benchmark complete.*') { $message = $message -replace '\d+(\.\d+)?', '<n>' }
        $messages.Add($message)
    }
    return $messages
}

$ref  = Read-Messages $Reference
$cand = Read-Messages $Candidate
if ($ref.Count -eq 0)  { throw ("nothing left of {0} after filtering" -f $Reference) }
if ($cand.Count -eq 0) { throw ("nothing left of {0} after filtering" -f $Candidate) }

# Greedy in-order alignment. The look-ahead window keeps a single unmatched line from destroying the
# alignment of everything after it, which is all a boot log needs.
$window = 40
$ri = 0
$added = New-Object System.Collections.Generic.List[string]
$missing = New-Object System.Collections.Generic.List[string]
$aligned = New-Object System.Collections.Generic.List[string]
$matched = 0

foreach ($message in $cand) {
    $hit = -1
    $limit = [Math]::Min($ri + $window, $ref.Count - 1)
    for ($i = $ri; $i -le $limit; $i++) {
        if ($ref[$i] -eq $message) { $hit = $i; break }
    }

    if ($hit -lt 0) {
        $added.Add($message)
        continue
    }

    for ($i = $ri; $i -lt $hit; $i++) { $missing.Add($ref[$i]) }
    $ri = $hit + 1
    $aligned.Add($message)
    $matched++
}

for ($i = $ri; $i -lt $ref.Count; $i++) { $missing.Add($ref[$i]) }

function Show-Group([string] $title, $list, [string] $sign) {
    if ($list.Count -eq 0) { return }
    Write-Host ''
    Write-Host ("{0} ({1})" -f $title, $list.Count)
    $counts = @{}
    foreach ($item in $list) { $counts[$item] = 1 + $(if ($counts.ContainsKey($item)) { $counts[$item] } else { 0 }) }
    foreach ($item in ($counts.Keys | Sort-Object)) {
        $times = if ($counts[$item] -gt 1) { " x$($counts[$item])" } else { '' }
        Write-Host ("  {0} {1}{2}" -f $sign, $item, $times)
    }
}

Write-Host ("original:  {0} messages" -f $ref.Count)
Write-Host ("rebuild:   {0} messages" -f $cand.Count)
Write-Host ("aligned in order: {0}" -f $matched)

if ($ShowAligned) {
    Write-Host ''
    Write-Host ("what both boots print, in the same order ({0})" -f $aligned.Count)
    foreach ($item in $aligned) { Write-Host ("  = {0}" -f $item) }
}

Show-Group 'printed by the rebuild, not by the original' $added '+'
Show-Group 'printed by the original, not by the rebuild' $missing '-'

if ($added.Count -eq 0 -and $missing.Count -eq 0) {
    Write-Host ''
    Write-Host 'the two boots are identical'
}
