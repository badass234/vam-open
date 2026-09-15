<#
.SYNOPSIS
Creates (or removes) a junction to the game's asset-bundle directory.

.DESCRIPTION
In the built game VaM reads bundles from <game>_Data\StreamingAssets. In the Unity
editor Application.streamingAssetsPath points inside Assets, so AssetBundleManager
(see GetStreamingAssetsDirectory) uses <project>\StreamingAssets in the editor.
That directory lives OUTSIDE Assets, so Unity does not import it and 16.5 GB of bundles
never reach the AssetDatabase.

.EXAMPLE
.\scripts\New-StreamingAssetsLink.ps1
.EXAMPLE
.\scripts\New-StreamingAssetsLink.ps1 -Remove
#>
[CmdletBinding()]
param(
    [string]$ProjectPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'VaM_Rebuild'),
    [string]$InstallRoot,
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

# The game install is not part of this repository, so it is never hard-coded here: -InstallRoot
# wins, then $env:VAM_INSTALL, and only then the parent directory of the repository - which is
# correct when the repository has been placed inside the install, as it normally is.
if (-not $InstallRoot) { $InstallRoot = $env:VAM_INSTALL }
if (-not $InstallRoot) {
    $candidate = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    if (Test-Path -LiteralPath (Join-Path $candidate 'VaM_Data\Managed\Assembly-CSharp.dll')) { $InstallRoot = $candidate }
}
if (-not $InstallRoot) {
    throw 'no install root: pass -InstallRoot or set $env:VAM_INSTALL to the Virt-a-Mate directory'
}
$InstallRoot = (Resolve-Path -LiteralPath $InstallRoot).Path

$link = Join-Path $ProjectPath 'StreamingAssets'
$target = Join-Path $InstallRoot 'VaM_Data\StreamingAssets'

if (-not (Test-Path $ProjectPath)) { throw "project not found: $ProjectPath" }
if (-not (Test-Path $target)) { throw "no bundle directory: $target" }

$existing = Get-Item -LiteralPath $link -Force -ErrorAction SilentlyContinue

if ($Remove) {
    if (-not $existing) { Write-Host 'junction is already absent'; exit 0 }
    if (-not ($existing.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "$link exists and is not a junction — I will not delete it by hand"
    }
    [IO.Directory]::Delete($link, $false)
    Write-Host "junction removed: $link (bundles in $target left untouched)" -ForegroundColor Green
    exit 0
}

if ($existing) {
    if ($existing.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        $current = (Get-Item -LiteralPath $link -Force).Target
        if ($current -and ($current.TrimEnd('\') -ieq $target.TrimEnd('\'))) {
            Write-Host "junction already set: $link -> $current" -ForegroundColor Green
            exit 0
        }
        throw "junction points at $current, expected $target (remove it with the -Remove switch)"
    }
    throw "$link exists and is not a junction"
}

New-Item -ItemType Junction -Path $link -Target $target | Out-Null

$files = Get-ChildItem -LiteralPath $link -File
$manifest = Test-Path (Join-Path $link 'StandaloneWindows64')
$total = ($files | Measure-Object -Property Length -Sum).Sum

Write-Host "junction created: $link -> $target" -ForegroundColor Green
Write-Host ("visible files: {0}, size: {1} GB" -f $files.Count, [math]::Round($total / 1GB, 2))
if (-not $manifest) { throw 'manifest bundle StandaloneWindows64 not visible — check the installation' }
Write-Host 'StandaloneWindows64 manifest is in place' -ForegroundColor Green
Write-Host 'Unity ignores this folder: it is outside Assets, there will be no 16 GB import.'