#Requires -Version 5.1
<#
.SYNOPSIS
    Builds src\mcs into the mcs.dll that DynamicCSharp loads to compile plugins at run time.

.DESCRIPTION
    Every plugin in a VaM session is compiled while the game runs, by Mono's C# compiler driven
    through DynamicCSharp. The compiler ships as an assembly named "mcs", and the copy VaM released
    cannot be used here: it hands every optional struct parameter to ParameterBuilder.SetConstant,
    and the profile this project selects for the .NET 4.x runtime rejects a Nullable<T> default.

        ArgumentException: System.Nullable`1[System.Int32] is not a supported constant type.
          at System.Reflection.Emit.ParameterBuilder.SetConstant(Object)
          at Mono.CSharp.Parameter.ApplyAttributes(MethodBuilder, ConstructorBuilder, Int32, PredefinedAttributes)

    The exception is raised while the parameter's attributes are being emitted, so the compilation
    of that plugin aborts and the reported error list is left empty. everlaster.TittyMagic's
    ObjectHierarchyToString(Transform, int? = null, Func<Transform, string> = null) is enough to
    make every plugin in the session fail to load.

    src\mcs is the compiler's own source, decompiled from VaM_Data\Managed\mcs.dll, with two
    changes: Mono\CSharp\Parameter.cs drops a default value the run time will not store instead of
    failing the compile, and Mono\CSharp\TypeParameterInflator.cs re-makes a by-ref type around its
    inflated element (it threw NotImplementedException before, which is what an engine whose
    mscorlib carries ReadOnlySpan<T> — every Unity line from 2020.3 on — turns into an internal
    compiler error on StringBuilder.Append and int.Parse). Built from there it keeps the original
    assembly name, version and reference set. The two substitutes that were tried first both fail:
    the editor's Mono.CSharp 4.0.0.0 has the guard but its Mono.CSharp.Driver/TimeReporter/
    DynamicLoader are internal to DynamicCSharp, and MonoBleedingEdge\lib\mono\4.5\mcs.exe is a
    newer compiler whose public surface differs (SourceFile.GetDataStream,
    CompilerContext.TimeReporter, ...).

    The editor's own mcs.exe is used rather than dotnet: this step has to work with nothing but the
    Unity installation and the repository, and it is the same Mono the game itself runs on. It has to
    be a profile from the 2018-2020 lines though - the 2021 profiles and later have moved
    System.Collections.Generic.Stack<T> out of System.dll, where this compiler's source expects it, so
    the script builds with the newest installed editor that still has it there when the project's own
    editor cannot.

.EXAMPLE
    scripts\Build-McsCompiler.ps1
    scripts\Build-McsCompiler.ps1 -OutputPath VaM_Rebuild\Assets\Plugins\mcs.dll -Verbose
#>
param(
    [string]$SourceDir = (Join-Path $PSScriptRoot '..\src\mcs'),
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\VaM_Rebuild\Assets\Plugins\mcs.dll'),
    # The editor's Data folder. Defaults to the editor named in VaM_Rebuild\ProjectSettings.
    [string]$EditorDataDir,
    [string]$LogFile
)

# The one thing that decides whether a profile can build this source. Stack<T> was still in System.dll
# when the compiler was written; the profiles the 2021 editors and later ship no longer have it there
# and keep a netstandard facade for System.Collections instead, which a compiler this old does not
# follow - so it stops at "CS0246: The type or namespace name 'Stack' could not be found". The profile
# is build-time only: it changes nothing in the output but the failure, because the reference table is
# mscorlib/System/System.Core/System.Xml either way and every run time from 2020.3 on forwards the type
# to whichever assembly its own BCL keeps it in.
$profileProbeSource = @'
using System.Collections.Generic;
internal sealed class McsProfileProbe
{
    private readonly Stack<object> items = new Stack<object>();
}
'@

function Get-McsProfile {
    param([string] $EditorDataDir)

    [pscustomobject]@{
        EditorDataDir = $EditorDataDir
        Version       = Split-Path -Leaf (Split-Path -Parent (Split-Path -Parent $EditorDataDir))
        Mono          = Join-Path $EditorDataDir 'MonoBleedingEdge\bin\mono.exe'
        Compiler      = Join-Path $EditorDataDir 'MonoBleedingEdge\lib\mono\4.5\mcs.exe'
    }
}

function Test-McsProfile {
    param(
        [pscustomobject] $Profile,
        [string] $ProbeSource,
        [string] $ProbeOutput
    )

    if (-not (Test-Path -LiteralPath $Profile.Mono) -or -not (Test-Path -LiteralPath $Profile.Compiler)) {
        return $false
    }

    # The build's own reference set, because the profile that cannot resolve Stack<T> through it is
    # the profile that cannot build src\mcs either.
    $profileLib = Split-Path -Parent $Profile.Compiler
    $references = @('System', 'System.Core') |
        ForEach-Object { "-r:" + (Join-Path $profileLib "$_.dll") } |
        Where-Object { Test-Path -LiteralPath $_.Substring(3) }

    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $output = & $Profile.Mono $Profile.Compiler -target:library -sdk:4.5 -noconfig -nowarn:0169 "-out:$ProbeOutput" @references $ProbeSource 2>&1
    $ErrorActionPreference = $previousPreference

    @($output | Select-String -Pattern 'error CS').Count -eq 0
}

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

if (-not $EditorDataDir) {
    . (Join-Path $PSScriptRoot 'UnityEditor.ps1')
    $EditorDataDir = Get-VaMOpenEditorDataDir
}

foreach ($d in $SourceDir, $EditorDataDir) {
    if (-not (Test-Path -LiteralPath $d)) { throw "Not found: $d" }
}
$SourceDir     = (Resolve-Path -LiteralPath $SourceDir).Path
$EditorDataDir = (Resolve-Path -LiteralPath $EditorDataDir).Path
$OutputPath    = [System.IO.Path]::GetFullPath($OutputPath)
if (-not $LogFile) { $LogFile = Join-Path $root 'artifacts\logs\build-mcs.log' }

$objDir = Join-Path $root 'artifacts\obj\mcs'
New-Item -ItemType Directory -Path $objDir -Force | Out-Null

$probeSource = Join-Path $objDir 'mcs-profile-probe.cs'
Set-Content -LiteralPath $probeSource -Value $profileProbeSource -Encoding UTF8
$probeOutput = Join-Path $objDir 'mcs-profile-probe.dll'

$profile = Get-McsProfile -EditorDataDir $EditorDataDir
if (-not (Test-McsProfile -Profile $profile -ProbeSource $probeSource -ProbeOutput $probeOutput)) {
    $hub = Join-Path ${env:ProgramFiles} 'Unity\Hub\Editor'
    $candidateDirs = @(Get-ChildItem -LiteralPath $hub -Directory -ErrorAction SilentlyContinue |
        Sort-Object -Property Name -Descending |
        ForEach-Object { Join-Path $_.FullName 'Editor\Data' } |
        Where-Object { Test-Path -LiteralPath $_ })

    $fallback = $null
    foreach ($candidateDir in $candidateDirs) {
        $candidate = Get-McsProfile -EditorDataDir $candidateDir
        if ($candidate.Version -eq $profile.Version) { continue }
        if (Test-McsProfile -Profile $candidate -ProbeSource $probeSource -ProbeOutput $probeOutput) {
            $fallback = $candidate
            break
        }
    }

    if (-not $fallback) {
        throw ('no installed editor can build src\mcs: that needs a profile with Stack<T> still in ' +
            'System.dll, which the 2021 editors and later no longer ship. Install one of the 2018-2020 ' +
            'lines, or point -EditorDataDir at an editor that still has it.')
    }

    Write-Host ("  {0} cannot build src\mcs - its profile has no Stack<T> in System.dll; building with {1}" -f $profile.Version, $fallback.Version) -ForegroundColor Yellow
    $profile = $fallback
    $EditorDataDir = $profile.EditorDataDir
}

$mono = $profile.Mono
$mcs  = $profile.Compiler
foreach ($tool in $mono, $mcs) {
    if (-not (Test-Path -LiteralPath $tool)) { throw "not in this editor: $tool" }
}

# The compiler's own source is in IKVM, Mono and Properties, and it is the C# 7+ subset the compiler
# itself was written in - not the C# 6 the game's code is held to - so the language version is not
# constrained the way src\Directory.Build.props constrains the game assemblies.
$sources = @(Get-ChildItem -LiteralPath $SourceDir -Recurse -File -Filter '*.cs' | Sort-Object FullName)
if ($sources.Count -lt 700) { throw "src\mcs looks incomplete: $($sources.Count) .cs files" }

# Only mscorlib, System, System.Core and System.Xml end up in the built assembly's reference table,
# exactly as in VaM's own mcs.dll; the extra ones are what the source needs to see to compile (Linq,
# serialization). mscorlib is not listed: -sdk:4.5 already gives the compiler the one from the profile,
# and passing it twice makes every type in it ambiguous (warning CS1685 on System.Object).
$bcl = 'System', 'System.Core', 'System.Xml', 'System.Xml.Linq', 'System.Numerics', 'System.Runtime.Serialization'

# 800 paths do not fit on a command line, so they go into a response file. Quoted, because the
# repository can live under a path with spaces.
$responseFile = Join-Path $objDir 'sources.rsp'
$stagedDll    = Join-Path $objDir 'mcs.dll'
Remove-Item -LiteralPath $stagedDll -Force -ErrorAction SilentlyContinue

$lines = @($bcl | ForEach-Object { '-r:"{0}"' -f (Join-Path ($EditorDataDir + '\MonoBleedingEdge\lib\mono\4.5') "$_.dll") })
$lines += $sources | ForEach-Object { '"{0}"' -f $_.FullName }
[System.IO.File]::WriteAllLines($responseFile, $lines)

New-Item -ItemType Directory -Path (Split-Path -Parent $LogFile) -Force | Out-Null

$arguments = @(
    $mcs
    '-target:library'
    '-sdk:4.5'
    '-noconfig'
    '-optimize+'
    '-debug-'
    '-nowarn:0169,0414,0649'
    '-langversion:latest'
    "-out:$stagedDll"
    "@$responseFile"
)

Write-Host ("Building mcs.dll from src\mcs ({0} .cs files) ..." -f $sources.Count) -ForegroundColor Cyan

# mcs writes its warnings to stderr, and a native command's stderr is a terminating error under
# ErrorActionPreference = 'Stop' - which is exactly how a successful build of 797 files would look
# like a failure. The exit code and the log decide, not the stream.
$previousPreference = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
$output = & $mono $arguments 2>&1 | ForEach-Object { $_.ToString() }
$exitCode = $LASTEXITCODE
$ErrorActionPreference = $previousPreference

$output | Set-Content -LiteralPath $LogFile

$errors = @($output | Select-String -Pattern 'error CS')
if ($exitCode -ne 0 -or $errors.Count) {
    $output | Select-Object -First 30 | ForEach-Object { Write-Host "  $_" }
    if ($errors.Count -gt 30) { Write-Host ("  ... and {0} more" -f ($errors.Count - 30)) }
    throw "mcs build failed: exit $exitCode, $($errors.Count) error(s). Log: $LogFile"
}

# Unity only treats a file as a managed plugin if its PE header says it is a DLL. An assembly built
# with -target:library always is, but the flavour mistake has been made here once already - an
# "mcs.exe" dropped into Assets\Plugins loaded fine in every other respect and then simply was not
# referenced by the compiler, which reads as missing Mono.CSharp types instead of as a bad plugin.
$bytes = [System.IO.File]::ReadAllBytes($stagedDll)
$peOffset = [BitConverter]::ToInt32($bytes, 0x3C)
$characteristics = [BitConverter]::ToUInt16($bytes, $peOffset + 0x16)
if (-not ($characteristics -band 0x2000)) {
    throw "$stagedDll has no IMAGE_FILE_DLL flag (Characteristics 0x$('{0:X4}' -f $characteristics)) - Unity will ignore it"
}

$assembly = [System.Reflection.AssemblyName]::GetAssemblyName($stagedDll)
if ($assembly.Name -ne 'mcs') {
    throw "$stagedDll is assembly '$($assembly.Name)' - DynamicCSharp and the reference restrictions look it up as 'mcs'"
}

New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null
Copy-Item -LiteralPath $stagedDll -Destination $OutputPath -Force

Write-Host ("  mcs {0} ({1:N0} B) -> {2}" -f $assembly.Version, (Get-Item -LiteralPath $OutputPath).Length, $OutputPath) -ForegroundColor Green
Write-Host "Log: $LogFile"
