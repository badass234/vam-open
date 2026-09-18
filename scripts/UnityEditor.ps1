<#
.SYNOPSIS
Finds the editor that the gate scripts should run.

.DESCRIPTION
The editor version is read from VaM_Rebuild\ProjectSettings\ProjectVersion.txt - the file Unity itself
rewrites when a project is opened with another editor. That is the single place a version hop changes
(see plan.md, "The engine migration"), instead of five hardcoded editor paths that would otherwise
keep pointing at the version that was current when they were written.

Dot-source this file, then use the functions:

    . (Join-Path $root 'UnityEditor.ps1')
    if (-not $UnityExe) { $UnityExe = Get-VaMOpenUnityExe }

.EXAMPLE
Get-VaMOpenUnityExe
.EXAMPLE
Get-VaMOpenEditorVersion -ProjectPath C:\Games\VaM_Updater\VAMOpen\VaM_Rebuild
#>

function Get-VaMOpenProjectPath {
    param([string] $ProjectPath)
    if (-not $ProjectPath) {
        $ProjectPath = Join-Path (Split-Path -Parent $PSScriptRoot) 'VaM_Rebuild'
    }
    if (-not (Test-Path -LiteralPath $ProjectPath)) { throw "project not found: $ProjectPath" }
    (Resolve-Path -LiteralPath $ProjectPath).Path
}

function Get-VaMOpenEditorVersion {
    param([string] $ProjectPath)

    $ProjectPath = Get-VaMOpenProjectPath -ProjectPath $ProjectPath
    $versionFile = Join-Path $ProjectPath 'ProjectSettings\ProjectVersion.txt'
    if (-not (Test-Path -LiteralPath $versionFile)) { throw "editor version file not found: $versionFile" }

    $match = Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersion:\s*(\S+)' | Select-Object -First 1
    if (-not $match) { throw "no m_EditorVersion line in $versionFile" }
    $match.Matches[0].Groups[1].Value
}

function Get-VaMOpenEditorDir {
    param(
        [string] $Version,
        [string] $ProjectPath
    )

    if (-not $Version) { $Version = Get-VaMOpenEditorVersion -ProjectPath $ProjectPath }
    $dir = Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$Version\Editor"
    if (-not (Test-Path -LiteralPath $dir)) {
        throw "Unity $Version is not installed under $dir - install it with the Hub, or pass -UnityExe explicitly"
    }
    $dir
}

function Get-VaMOpenUnityExe {
    param(
        [string] $Version,
        [string] $ProjectPath
    )

    Join-Path (Get-VaMOpenEditorDir -Version $Version -ProjectPath $ProjectPath) 'Unity.exe'
}

function Get-VaMOpenEditorDataDir {
    param(
        [string] $Version,
        [string] $ProjectPath
    )

    Join-Path (Get-VaMOpenEditorDir -Version $Version -ProjectPath $ProjectPath) 'Data'
}
