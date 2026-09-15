#Requires -Version 5.1
<#
.SYNOPSIS
    Builds the decompiled VaM assemblies against the game's own Unity 2018.1 DLLs.

.DESCRIPTION
    Runs `dotnet build` for the requested project, tees the output to
    artifacts/logs/build.log and prints a per-error-code summary, which is the
    fastest way to see which class of problem to attack next.
#>
param(
    [string]$Project = 'Assembly-CSharp',
    [switch]$RegenerateReferences,
    [switch]$SummaryOnly
)

$ErrorActionPreference = 'Stop'

$root      = Split-Path -Parent $PSScriptRoot
$dotnet    = Join-Path $root '.tools\dotnet\dotnet.exe'
$logDir    = Join-Path $root 'artifacts\logs'
$logFile   = Join-Path $logDir "build-$Project.log"
$projectFile = Join-Path $root "src\$Project\$Project.csproj"

if (-not (Test-Path -LiteralPath $dotnet))      { throw "dotnet not found at $dotnet" }
if (-not (Test-Path -LiteralPath $projectFile)) { throw "project not found at $projectFile" }

New-Item -ItemType Directory -Force -Path $logDir | Out-Null

if ($RegenerateReferences) {
    & (Join-Path $PSScriptRoot 'Update-UnityReferences.ps1') | Out-Null
}

$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'

if (-not $SummaryOnly) {
    Write-Host "Building $Project ..." -ForegroundColor Cyan
    & $dotnet build $projectFile --nologo -v q 2>&1 |
        Tee-Object -FilePath $logFile | Out-Null
    $exitCode = $LASTEXITCODE
} else {
    $exitCode = if (Test-Path -LiteralPath $logFile) { 0 } else { throw "no log at $logFile" }
}

$lines = @(Get-Content -LiteralPath $logFile)

$succeeded = $lines -match '^\s*Build succeeded\.'
$failed    = $lines -match '^\s*Build FAILED\.'

$errors = $lines | Select-String -Pattern 'error (CS\d+|MSB\d+)' -AllMatches |
    ForEach-Object { $_.Matches | ForEach-Object { $_.Groups[1].Value } }

Write-Host ''
if ($succeeded) {
    Write-Host "BUILD SUCCEEDED" -ForegroundColor Green
} elseif ($failed -or $errors.Count -gt 0) {
    Write-Host "BUILD FAILED - $($errors.Count) error(s)" -ForegroundColor Red
} else {
    Write-Host "UNKNOWN RESULT (exit $exitCode)" -ForegroundColor Yellow
}

if ($errors.Count -gt 0) {
    Write-Host ''
    Write-Host 'Errors by code:' -ForegroundColor Cyan
    $errors | Group-Object | Sort-Object Count -Descending |
        Select-Object Count, Name | Format-Table -AutoSize | Out-String | Write-Host

    Write-Host 'Errors by file (top 20):' -ForegroundColor Cyan
    $lines | Select-String -Pattern 'error (CS\d+|MSB\d+)' |
        ForEach-Object { ($_ -split '\(')[0] } |
        Group-Object | Sort-Object Count -Descending | Select-Object -First 20 Count, Name |
        Format-Table -AutoSize | Out-String | Write-Host
}

Write-Host "Log: $logFile"
exit $exitCode
