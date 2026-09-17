#Requires -Version 5.1
<#
.SYNOPSIS
    Assembles VAMOpen\VaM_Rebuild from the AssetRipper export plus the verified sources.

.DESCRIPTION
    The AssetRipper export in work\ripped-core\ExportedProject is the raw material, but two
    things in it are replaced before it can become our project:

      * Assets\Scripts\Assembly-CSharp and Assembly-UnityScript - AssetRipper decompiled them
        independently; we use src\, which is parity-checked against the original assembly and
        builds clean (see docs\parity-report.md). The matching .cs.meta files from the export are
        carried across, because they hold the GUIDs the scenes and prefabs reference.
      * The other 21 decompiled assemblies - we drop them and reference the original DLLs from
        VaM_Data\Managed instead. Recompiling 4500 files of NAudio / mcs / System.Windows.Forms
        from decompiled source would be a large, pointless source of compile errors.
      * Nothing at all for the native side - AssetRipper writes no Assets\Plugins. VaM_Data\Plugins
        is copied to Assets\Plugins\x86_64, which is the same folder Unity would have copied to the
        build's Plugins directory.

    The reference set is the other half of making the sources compile. VaM's BCL is not shipped
    wholesale: Unity supplies the .NET 4.x facades itself, its own Boo.Lang is referenced by every
    UnityScript assembly, and only Mono.Cecil and System.Drawing are added back as ordinary plugins.

    See docs\rebuild-project.md for the reasoning and the known dangling references.

    Re-runnable: the target directory is rebuilt from scratch on every invocation, and the two
    things inside it that are not the export's are carried across that wipe - Assets\Editor,
    which is ours, and the StreamingAssets junction to the installation's bundles.

.EXAMPLE
    scripts\Setup-RebuildProject.ps1
    scripts\Setup-RebuildProject.ps1 -SkipNativePlugins
#>
param(
    [string]$ExportDir  = (Join-Path $PSScriptRoot '..\work\ripped-core\ExportedProject'),
    [string]$SourceDir  = (Join-Path $PSScriptRoot '..\src'),
    [string]$TargetDir  = (Join-Path $PSScriptRoot '..\VaM_Rebuild'),
    [string]$ManagedDir = (Join-Path $PSScriptRoot '..\..\VaM_Data\Managed'),
    # Also copy the BCL extras VaM shipped (System.Windows.Forms, Mono.Posix, ...). They are the
    # runtime dependencies of mcs.dll - the C# compiler DynamicCSharp drives - and not something
    # Assembly-CSharp is compiled against, so they are opt-in: they can collide with the facades
    # Unity 4.x references by itself.
    [switch]$IncludeVaMBclExtras,
    # Skip Assets\Plugins\x86_64 - the editor starts much faster without 160 MB of native
    # libraries, but any DllImport (bass, LeapC, openvr_api, ...) will fail at runtime.
    [switch]$SkipNativePlugins
)

$ErrorActionPreference = 'Stop'

foreach ($d in $ExportDir, $SourceDir, $ManagedDir) {
    if (-not (Test-Path -LiteralPath $d)) { throw "Not found: $d" }
}

$ExportDir  = (Resolve-Path -LiteralPath $ExportDir).Path
$SourceDir  = (Resolve-Path -LiteralPath $SourceDir).Path
$ManagedDir = (Resolve-Path -LiteralPath $ManagedDir).Path
$TargetDir  = [System.IO.Path]::GetFullPath($TargetDir)

# Assets\Editor is not from the export - RebuildGate, RebuildPlayer and VaMInspector are ours, and
# they are tracked. Re-runnable means the wipe has to carry them across it: without the stash a second
# run deletes the editor assembly that both gates look their entry point up in, and the compile gate
# then reports "the gate method never ran" - which reads like a broken project and is not one.
$editorStash   = Join-Path ([System.IO.Path]::GetTempPath()) ('vamopen-editor-' + [guid]::NewGuid().ToString('N'))
$stashedEditor = Join-Path $editorStash 'Editor'

# <project>\StreamingAssets is a junction to the installation's 16.47 GB of bundles, created by
# New-StreamingAssetsLink.ps1 after setup and easy to lose. It is unlinked before the wipe and
# never walked through: deleting a tree that *contains* a junction recursively deletes the files
# behind it, and those files are the user's game, not ours. The target is read back off the link,
# so no install root is needed here, and the link is re-made once the export is back in place.
$bundleLink   = Join-Path $TargetDir 'StreamingAssets'
$bundleTarget = $null

if (Test-Path -LiteralPath $TargetDir) {
    Write-Host "Removing existing $TargetDir"
    if (Test-Path -LiteralPath (Join-Path $TargetDir 'Assets\Editor')) {
        New-Item -ItemType Directory -Path $editorStash -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $TargetDir 'Assets\Editor') -Destination $stashedEditor -Recurse
        $editorMeta = Join-Path $TargetDir 'Assets\Editor.meta'
        if (Test-Path -LiteralPath $editorMeta) {
            Copy-Item -LiteralPath $editorMeta -Destination (Join-Path $editorStash 'Editor.meta')
        }
        Write-Host ("  stashed Assets\Editor ({0} files)" -f @(Get-ChildItem -LiteralPath $stashedEditor -Recurse -File).Count)
    }
    $bundleItem = Get-Item -LiteralPath $bundleLink -Force -ErrorAction SilentlyContinue
    if ($bundleItem -and ($bundleItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        $bundleTarget = @($bundleItem.Target)[0]
        [IO.Directory]::Delete($bundleLink, $false)
        Write-Host "  unlinked StreamingAssets -> $bundleTarget"
    }
    Remove-Item -LiteralPath $TargetDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TargetDir | Out-Null

# ---------------------------------------------------------------- 1. raw export

Copy-Item -LiteralPath (Join-Path $ExportDir 'Assets')         -Destination $TargetDir -Recurse
Copy-Item -LiteralPath (Join-Path $ExportDir 'Packages')       -Destination $TargetDir -Recurse
Copy-Item -LiteralPath (Join-Path $ExportDir 'ProjectSettings') -Destination $TargetDir -Recurse
Write-Host "Copied AssetRipper export"

# Back over whatever the export had there. The contents are copied rather than the directory, because
# the destination may already exist and PowerShell would then nest the source inside it.
if (Test-Path -LiteralPath $stashedEditor) {
    $editorDir = Join-Path $TargetDir 'Assets\Editor'
    New-Item -ItemType Directory -Path $editorDir -Force | Out-Null
    Get-ChildItem -LiteralPath $stashedEditor -Force | Copy-Item -Destination $editorDir -Recurse -Force
    $stashedMeta = Join-Path $editorStash 'Editor.meta'
    if (Test-Path -LiteralPath $stashedMeta) {
        Copy-Item -LiteralPath $stashedMeta -Destination (Join-Path $TargetDir 'Assets\Editor.meta') -Force
    }
    Remove-Item -LiteralPath $editorStash -Recurse -Force
    Write-Host ("restored Assets\Editor ({0} files)" -f @(Get-ChildItem -LiteralPath $editorDir -Recurse -File).Count)
}

if ($bundleTarget) {
    New-Item -ItemType Junction -Path $bundleLink -Target $bundleTarget | Out-Null
    Write-Host ("relinked StreamingAssets -> {0}" -f $bundleTarget)
}

# The export does not carry a usable scripting runtime setting, and the default is the wrong one.
# VaM shipped with the .NET 4.x runtime, and the decompiled sources are C# 6 (interpolated strings,
# expression bodied members, ?.), so with Unity's default (.NET 3.5) the whole project fails:
#
#   Assets/Scripts/Assembly-CSharp/Atom.cs(352,17): error CS1644: Feature `expression bodied
#   members' cannot be used because it is not part of the C# 4.0 language specification
#
# scriptingRuntimeVersion: 0 = .NET 3.5, 1 = .NET 4.x; apiCompatibilityLevel: 2 = .NET 2.0 Subset,
# 3 = .NET 4.6. The two have to agree.
$playerSettings = Join-Path $TargetDir 'ProjectSettings\ProjectSettings.asset'
$settingsText = [System.IO.File]::ReadAllText($playerSettings)
$settingsBefore = $settingsText
$settingsText = $settingsText -replace '(?m)^  scriptingRuntimeVersion: 0\r?$', '  scriptingRuntimeVersion: 1'
$settingsText = $settingsText -replace '(?m)^  apiCompatibilityLevel: 2\r?$', '  apiCompatibilityLevel: 3'
if ($settingsText -eq $settingsBefore) { throw "Could not set the scripting runtime version in $playerSettings" }
[System.IO.File]::WriteAllText($playerSettings, $settingsText)
Write-Host "  scripting runtime -> .NET 4.x (scriptingRuntimeVersion: 1, apiCompatibilityLevel: 3)"

# ---------------------------------------------- 2. replace the decompiled assemblies

$scriptsDir = Join-Path $TargetDir 'Assets\Scripts'

# Assemblies we compile from src\ instead of trusting AssetRipper's decompilation.
$fromSource = 'Assembly-CSharp', 'Assembly-UnityScript'

Get-ChildItem -LiteralPath $scriptsDir | Where-Object { $_.PSIsContainer } | ForEach-Object {
    if ($fromSource -notcontains $_.Name) {
        Remove-Item -LiteralPath $_.FullName -Recurse -Force
    }
}
Get-ChildItem -LiteralPath $scriptsDir -Filter '*.asmdef' -File | ForEach-Object {
    if ($fromSource -notcontains $_.BaseName) { Remove-Item -LiteralPath $_.FullName -Force }
}

# AssetRipper writes a .cs.meta for every script whose GUID is referenced by a scene, prefab or
# asset. Those GUIDs are the only thing tying a MonoBehaviour component to its class, so they have
# to survive the source swap. Both decompilers walk the assembly the same way, so the relative
# paths line up and the metas can simply be moved across - but only after checking that the .cs
# they describe really exists in our tree.
$metaStash    = Join-Path $TargetDir '.meta-stash'
$metaTotal    = 0
$metaRestored = 0
$metaOrphaned = New-Object System.Collections.Generic.List[string]

foreach ($asm in $fromSource) {
    $dest  = Join-Path $scriptsDir $asm
    $stash = Join-Path $metaStash $asm
    New-Item -ItemType Directory -Path $stash -Force | Out-Null

    Get-ChildItem -LiteralPath $dest -Recurse -Filter '*.cs.meta' -File | ForEach-Object {
        $relative = $_.FullName.Substring($dest.Length + 1)
        $copy     = Join-Path $stash $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $copy) -Force | Out-Null
        Move-Item -LiteralPath $_.FullName -Destination $copy
        $metaTotal++
    }

    Remove-Item -LiteralPath $dest -Recurse -Force
    Copy-Item -LiteralPath (Join-Path $SourceDir $asm) -Destination $dest -Recurse

    Get-ChildItem -LiteralPath $stash -Recurse -File | ForEach-Object {
        $relative = $_.FullName.Substring($stash.Length + 1)
        $script   = Join-Path $dest ($relative -replace '\.meta$', '')
        if (Test-Path -LiteralPath $script) {
            Move-Item -LiteralPath $_.FullName -Destination "$script.meta" -Force
            $metaRestored++
        } else {
            $metaOrphaned.Add("$asm\$relative")
        }
    }
    Remove-Item -LiteralPath $stash -Recurse -Force

    # .mdb/.pdb companions have no business in a Unity project. NB: -Include does not filter
    # when -LiteralPath is used, so the extension test has to be explicit.
    Get-ChildItem -LiteralPath $dest -Recurse -File |
        Where-Object { $_.Extension -eq '.mdb' -or $_.Extension -eq '.pdb' } |
        Remove-Item -Force

    $count = (Get-ChildItem -LiteralPath $dest -Recurse -Filter *.cs).Count
    Write-Host ("  {0}: {1} .cs files from src" -f $asm, $count)
}
Remove-Item -LiteralPath $metaStash -Recurse -Force

Write-Host ("  .meta restored: {0}/{1}" -f $metaRestored, $metaTotal)
if ($metaOrphaned.Count) {
    Write-Warning ("{0} .meta files describe scripts that src\ does not contain:" -f $metaOrphaned.Count)
    $metaOrphaned | Select-Object -First 20 | ForEach-Object { Write-Warning "    $_" }
}

# AssetRipper did not emit an .asmdef for Assembly-UnityScript, so its 13 files would be merged
# into the default Assembly-CSharp. The original game keeps them in a separate assembly, so the
# split is recreated explicitly - but not under the original name. "Assembly-UnityScript" is one of
# Unity's predefined (UnityScript-era) assembly names and Unity refuses to compile it:
#
#   Exception: Assembly cannot be have reserved name 'Assembly-UnityScript'
#   at EditorBuildRules.CreateTargetAssemblies [EditorBuildRules.cs:193]
#
# That exception aborts the whole target-assembly setup, so Assembly-CSharp.dll is never built and
# the project cannot run. The assembly therefore carries our own name. Verified: no type defined in
# src\Assembly-UnityScript is referenced from src\Assembly-CSharp (nor named identically), so the
# rename cannot break a reference. Predefined assemblies automatically reference asmdef
# assemblies, so the split stays transparent to the rest of the code.
$unityScriptDir = Join-Path $scriptsDir 'Assembly-UnityScript'
Get-ChildItem -LiteralPath $unityScriptDir -Filter '*.asmdef*' -File | Remove-Item -Force
@'
{
  "name": "VaMUnityScript",
  "references": [],
  "allowUnsafeCode": true
}
'@ | Set-Content -LiteralPath (Join-Path $unityScriptDir 'VaMUnityScript.asmdef') -Encoding UTF8
Write-Host "  wrote VaMUnityScript.asmdef (Unity reserves the original name)"

# ------------------------------------------------------------- 3. reference DLLs

$pluginsDir = Join-Path $TargetDir 'Assets\Plugins'
New-Item -ItemType Directory -Path $pluginsDir -Force | Out-Null

# Provided by Unity itself (engine, modules, editor).
$unityProvided = 'UnityEngine', 'UnityEditor'

# Referenced by Unity itself for every user assembly, so shipping VaM's copies as plugins as well
# would give every type they define two definitions (CS0433 / CS1703). With the .NET 4.x scripting
# runtime this is the set in MonoBleedingEdge\lib\mono\4.7.1-api; the exact reference list of a
# target is written to <project>\Temp\UnityTempFile-* on every compile.
$unityProvidedBcl = @(
    'mscorlib', 'netstandard'
    'System', 'System.Core', 'System.Xml', 'System.Xml.Linq'
    'System.Runtime.Serialization', 'System.Numerics', 'System.Numerics.Vectors'
)

# Compiled against Unity's own copy. An assembly containing UnityScript code references
# MonoBleedingEdge\lib\mono\unityscript\Boo.Lang.dll, and Unity puts that same copy into the player
# build, so VaM's Boo.Lang.dll only ever duplicated it - it kept colliding even after its meta was
# restricted to Standalone, because the auto-reference is unconditional:
#   Assets/Scripts/Assembly-UnityScript/CharacterMotor.cs(15,65): error CS0433:
#   The imported type `Boo.Lang.GenericGenerator<T>' is defined multiple times
$unityProvidedProfile = 'Boo.Lang'

# Neither is a facade Unity references: Mono.Cecil is needed by DynamicCSharp\Security, and
# System.Drawing by ImageLoaderThreaded / MaterialOptions. Unity's 4.7.1-api profile forwards
# System.Drawing.Color to an assembly it never references (CS1070 + CS0234), so VaM's copies have to
# be shipped as ordinary editor plugins.
$requiredByOurCode = 'Mono.Cecil', 'System.Drawing'

# The rest of the BCL VaM shipped - the runtime closure of mcs.dll, the C# compiler DynamicCSharp
# drives. Assembly-CSharp does not compile against any of it, and each one can collide with Unity's
# facades, so it is opt-in (-IncludeVaMBclExtras) for when a player build turns out to need it.
$bclExtras = @(
    'Accessibility', 'System.Configuration', 'System.Data', 'System.EnterpriseServices'
    'System.Security', 'System.Transactions', 'System.Windows.Forms'
    'Mono.Data.Tds', 'Mono.Posix', 'Mono.Security', 'Mono.WebBrowser'
)

# The artifacts we are rebuilding.
$rebuilt = 'Assembly-CSharp', 'Assembly-UnityScript'

$copied = New-Object System.Collections.Generic.List[string]
Get-ChildItem -LiteralPath $ManagedDir -Filter *.dll -File | Sort-Object Name | ForEach-Object {
    $name = $_.BaseName
    if ($rebuilt -contains $name) { return }
    if ($unityProvidedBcl -contains $name) { return }
    if ($unityProvidedProfile -contains $name) { return }
    if ($bclExtras -contains $name -and -not $IncludeVaMBclExtras) { return }
    if ($unityProvided | Where-Object { $name -like "$_*" }) { return }
    Copy-Item -LiteralPath $_.FullName -Destination $pluginsDir
    $copied.Add($_.Name)
}
Write-Host ("Copied {0} plugin assemblies to Assets\Plugins" -f $copied.Count)

# Native plugins and their data files. Unity copies everything under Assets\Plugins\<platform>\ into
# the build's Plugins folder, which is exactly what VaM_Data\Plugins contains - bass.dll (via
# Bass.Net), LeapC.dll (38 DllImports in Assembly-CSharp), ConvexDecompositionDll.dll, openvr_api,
# the Oculus/MR audio spatializers, and the whole ZFBrowser CEF payload including locales\.
if ($SkipNativePlugins) {
    Write-Warning 'Skipped native plugins (-SkipNativePlugins); DllImport calls will fail at runtime.'
} else {
    $nativeDir = Join-Path $ManagedDir '..\Plugins'
    if (Test-Path -LiteralPath $nativeDir) {
        $nativeDir = (Resolve-Path -LiteralPath $nativeDir).Path
        $nativeDest = Join-Path $pluginsDir 'x86_64'
        New-Item -ItemType Directory -Path $nativeDest -Force | Out-Null
        # -LiteralPath does not expand wildcards, so enumerate first.
        Get-ChildItem -LiteralPath $nativeDir -Force | Copy-Item -Destination $nativeDest -Recurse -Force
        $nativeFiles = Get-ChildItem -LiteralPath $nativeDest -Recurse -File
        $nativeMb = ($nativeFiles | Measure-Object Length -Sum).Sum / 1MB
        Write-Host ("Copied {0} native plugin files ({1:N1} MB) to Assets\Plugins\x86_64" -f $nativeFiles.Count, $nativeMb)
        if ($nativeFiles.Count -eq 0) { throw "Nothing copied from $nativeDir" }
    } else {
        Write-Warning "Native plugin folder not found: $nativeDir"
    }
}

# ------------------------------------------------------------------ 4. sanity checks

$asmdef = Get-ChildItem -LiteralPath (Join-Path $scriptsDir 'Assembly-CSharp') -Recurse -Filter *.cs |
    Measure-Object
if ($asmdef.Count -eq 0) { throw 'Assembly-CSharp is empty' }

$stillDecompiled = (Get-ChildItem -LiteralPath $scriptsDir -Directory).Name
Write-Host ("Assets\Scripts now contains: {0}" -f ($stillDecompiled -join ', '))

# Every scene and prefab is wired to its scripts through the GUIDs in .meta files, so a dangling
# reference means a silent "Missing (Mono Script)" in the editor. known_dangling_guids.txt lists
# the handful that are expected to stay undefined.
$guidCheck = Join-Path $PSScriptRoot '..\tools\check_asset_guids.py'
$python = Get-Command python -ErrorAction SilentlyContinue
if ($python -and (Test-Path -LiteralPath $guidCheck)) {
    Write-Host ''
    & $python.Source $guidCheck --project $TargetDir
    if ($LASTEXITCODE -ne 0) {
        Write-Warning 'Asset GUID check found unexpected dangling references (see above).'
    }
} else {
    Write-Warning 'Python not found; skipped the asset GUID check.'
}

# ------------------------------------------------------- 5. GPU skinning shaders
# AssetRipper exports the *ComputeBuff shaders as placeholders - a POSITION-only vertex stage and a
# flat fragment stage - and a body drawn with one of those loses both its pose and its lighting. The
# real shaders survive only as compiled DXBC in the player build, so they are regenerated over the
# placeholders here. See docs\shader-reconstruction.md.
$shaderGen = Join-Path $PSScriptRoot 'New-VaMShaders.py'
$shaderBlobs = Join-Path $PSScriptRoot '..\artifacts\shader-blobs'
if ($python -and (Test-Path -LiteralPath $shaderGen)) {
    if (Test-Path -LiteralPath $shaderBlobs) {
        Write-Host ''
        # New-VaMShaders.py reports the compute shaders it leaves pending on stderr, and this
        # script runs with ErrorActionPreference 'Stop': that note arrives as a NativeCommandError
        # and aborts setup before 'Project ready'. The exit code is the verdict, the text is just
        # text, so the note is echoed rather than allowed to stop the rebuild.
        $nativePreference = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        try {
            $shaderLog = & $python.Source $shaderGen 2>&1
            $shaderExitCode = $LASTEXITCODE
        } finally {
            $ErrorActionPreference = $nativePreference
        }
        $shaderLog | ForEach-Object { Write-Host "  $_" }
        if ($shaderExitCode -ne 0) { throw 'Shader generation failed' }
    } else {
        Write-Warning ("No extracted shader bytecode at $shaderBlobs; Assets\Shader keeps the " +
                       "AssetRipper placeholders and characters will lose their pose. Run: " +
                       'scripts\Extract-VaMShaders.py --out artifacts\shader-blobs')
    }
} else {
    Write-Warning 'Python not found; skipped the ComputeBuff shader generation.'
}

Write-Host ''
Write-Host "Project ready: $TargetDir"
Get-ChildItem -LiteralPath $TargetDir | ForEach-Object { "  $($_.Name)" }
