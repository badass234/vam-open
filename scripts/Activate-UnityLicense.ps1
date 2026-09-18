<#
.SYNOPSIS
One-off activation of the project's Unity editor from the command line.

.DESCRIPTION
The editor is whatever VaM_Rebuild\ProjectSettings\ProjectVersion.txt names (see
scripts\UnityEditor.ps1), so an engine hop needs no edit here. The 2018.1.9f2 editor rejects
the ULF that Hub reissued on 15.09 at 11:58 (details and evidence in docs\unity-editor.md).
Activating with the editor itself hands back a legacy-valid license, and batch mode along with it.

The script performs a single batch run: the editor logs in to a Unity ID, activates the
license and imports the project along the way (that is, the same run also produces the log
for analysing compilation errors).

.EXAMPLE
.\scripts\Activate-UnityLicense.ps1
.EXAMPLE
.\scripts\Activate-UnityLicense.ps1 -Username user@example.com -Serial XX-XXXX-XXXX-XXXX-XXXX-XXXX
#>
[CmdletBinding()]
param(
    [string]$Username,
    [string]$Serial,
    [string]$UnityExe,
    [string]$ProjectPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'VaM_Rebuild'),
    [string]$LogFile,
    [switch]$NonInteractive
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'UnityEditor.ps1')
if (-not $UnityExe) { $UnityExe = Get-VaMOpenUnityExe -ProjectPath $ProjectPath }

$ulf = Join-Path $env:ProgramData 'Unity\Unity_lic.ulf'

if (-not (Test-Path $UnityExe)) { throw "editor not found: $UnityExe" }
if (-not (Test-Path $ProjectPath)) { throw "project not found: $ProjectPath" }
if (-not $LogFile) {
    $LogFile = Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts\activate-unity.log'
}
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogFile) | Out-Null
Remove-Item $LogFile -ErrorAction SilentlyContinue

if (-not $Username) {
    if ($NonInteractive) { throw '-Username is required' }
    $Username = Read-Host 'Unity ID (email)'
}
if (-not $NonInteractive) {
    Write-Host 'The password is entered hidden and is passed only to the editor process.' -ForegroundColor Yellow
}

$secure = Read-Host -AsSecureString "Unity ID password for $Username"
$bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
$plain = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)

$before = if (Test-Path $ulf) { (Get-Item $ulf).LastWriteTime } else { $null }

$unityArgs = @(
    '-batchmode', '-nographics', '-quit',
    '-projectPath', $ProjectPath,
    '-logFile', $LogFile,
    '-acceptSoftwareTermsForThisRunOnly',
    '-username', $Username,
    '-password', $plain
)
if ($Serial) { $unityArgs += @('-serial', $Serial) }

Write-Host "activating: $UnityExe (project: $ProjectPath)" -ForegroundColor Cyan
$sw = [Diagnostics.Stopwatch]::StartNew()
try {
    $proc = Start-Process -FilePath $UnityExe -ArgumentList $unityArgs -PassThru -Wait
}
finally {
    $plain = $null
    [GC]::Collect()
}
$sw.Stop()

$exitCode = $proc.ExitCode
$lines = if (Test-Path $LogFile) { Get-Content $LogFile } else { @() }
$licenseLines = $lines | Where-Object {
    $_ -match 'LICENSE SYSTEM|not been activated|login|Login|license|License|Activat|activat'
}

Write-Host "`n--- license lines ---" -ForegroundColor Cyan
if ($licenseLines) { $licenseLines | ForEach-Object { Write-Host $_ } } else { Write-Host '(none)' }

$after = if (Test-Path $ulf) { (Get-Item $ulf).LastWriteTime } else { $null }
$notActivated = [bool]($lines -match 'has not been activated with a valid License')

Write-Host "`n--- summary ---" -ForegroundColor Cyan
Write-Host "editor exit code     : $exitCode"
Write-Host "elapsed time         : $([int]$sw.Elapsed.TotalSeconds) s"
Write-Host "log                  : $LogFile ($((Get-Item $LogFile -ErrorAction SilentlyContinue).Length) bytes)"
Write-Host "ULF before / after   : $before / $after"

if ($notActivated) {
    Write-Host 'RESULT: the license was not activated.' -ForegroundColor Red
    Write-Host 'Check the credentials; with 2FA enabled the CLI login does not work — use manual activation (docs\unity-editor.md).' -ForegroundColor Red
    exit 1
}
if ($after -and $after -ne $before) {
    Write-Host 'RESULT: ULF updated — the license is activated.' -ForegroundColor Green
} elseif ($exitCode -eq 0) {
    Write-Host 'RESULT: the editor finished without a license error; ULF was not rewritten (the license was already valid).' -ForegroundColor Green
}
Write-Host "Next: python tools\parse_unity_log.py `"$LogFile`" --out artifacts\compile-errors.md" -ForegroundColor Cyan
exit 0
