<#
.SYNOPSIS
    Judges the plugin-compiler token repair (commit 9674a1f) from Unity player logs,
    counting hits per log BLOCK rather than per raw line.

.DESCRIPTION
    Commit 9674a1f repairs ReadMethodToken in
        src/Assembly-CSharp/DynamicCSharp/Compiler/McsDriver.cs
    so that it prefers MemberInfo.MetadataToken and only falls back to
    GetToken().Token inside catch (InvalidOperationException).

    The acceptance evidence for that repair is a PAIR of logs: a pre-fix control
    against a post-fix candidate. This script reproduces the coordinator's
    readings mechanically, so nobody has to hand-run Select-String again.

    WHAT IS COUNTED (per block, never per line)
      [1] Token exception block    "[CS]: System.InvalidOperationException" plus the
                                   IL-frame chain printed beneath it, counted as ONE
                                   block; the plain copy and the "[Error]"-prefixed
                                   copy of the same message are the same block.
      [2] Token frame hits         the "get_MetadataToken" symbols inside those
                                   frames (the actual signature of the defect).
      [3] Repair marker            "[DynamicCSharp] resolved N method override
                                   declaration(s) ..." emitted by
                                   RepairMethodOverrideDeclarations (McsDriver.cs:403),
                                   only inside "if (repaired > 0)".
      [4] Failed-compile block     the case-sensitive "Compile of <plugin> failed.
                                   Errors:" header plus everything up to the next
                                   message. The error list under that header is
                                   EMPTY BY DESIGN (MVRPluginManager.cs:796-808 filters
                                   every "[CS]" entry out of it), so an empty list is
                                   correct behaviour and is reported as such.
      [5] [CS584] family           "[CS584]" internal-compiler-error blocks.
      [6] Defect B (SEPARATE)      "System.NotSupportedException" +
                                   "TypeBuilderInstantiation" +
                                   "ResolveOnTypeBuilderInst". This is a DIFFERENT,
                                   still-open defect. It is reported as its own named
                                   block and NEVER folded into the token verdict.

    COUNTING TRAPS THIS SCRIPT HONOURS
      * Select-String is case-insensitive unless -CaseSensitive is passed.
        "Compile of " (the failure header) must not be confused with
        "Exception during compile of <plugin>:". Every match here is case-sensitive.
      * Each failure is logged twice: once plain, once with an "[Error]" prefix.
        The two copies form ONE block and are counted once.
      * The "failed. Errors:" body is empty on purpose (see [4] above).
      * A raw count of marker strings can RISE after a fix, because benign blocks
        are matches too. Totals are therefore reported per block and never
        presented as a regression on their own.

.PARAMETER Control
    Path to the pre-fix control log. Use together with -Candidate.

.PARAMETER Candidate
    Path to the post-fix candidate log. Use together with -Control.

.PARAMETER Log
    Path to a single log to judge against the post-fix expectation:
    the token markers must be 0 and the repair marker must be present.
    A log that carries the defect but not the repair reports a "PRE-FIX reading"
    verdict, which is the correct verdict for a control run and is DIFFERENT
    from "the fix regressed" (a log with neither defect nor repair, e.g. a build
    that never reached the repaired code path).

.PARAMETER BaselineControl
    Optional pre-fix control log. In -Log mode it adds a "movement" line showing
    the per-family reading of that control next to this log's reading. It never
    affects the verdict, which judges -Log on its own. A missing or unreadable
    baseline is bad input (exit 2).

.PARAMETER InstallRoot
    VaM install root. Resolution order:
    -InstallRoot, then $env:VAM_INSTALL, then the repository's parent directory.
    Only used to resolve relative log paths; this script reads logs only.

.PARAMETER Verify
    Report-only. Accepted for house style; this script never mutates anything, so
    it is the default behaviour and -Verify only makes that explicit.

.INPUTS
    None. This script does not accept pipeline input.

.OUTPUTS
    None. All report text goes to stdout (Write-Host) so that $LASTEXITCODE stays
    the single gate for callers.

.EXAMPLE
    powershell -File scripts\Test-PluginCompilerRepair.ps1 `
        -Control artifacts\manual-play.log `
        -Candidate artifacts\compiler-fix-ladyclown.log

    Prints the side-by-side block census for the known pair. Expected reading:
    exit 0 (token markers 2 -> 0, repair marker absent -> present).

.EXAMPLE
    powershell -File scripts\Test-PluginCompilerRepair.ps1 -Log artifacts\manual-play.log

    Judges the pre-fix control alone. Expected reading: exit 1 with an
    explicit "PRE-FIX reading" verdict.

.EXAMPLE
    powershell -File scripts\Test-PluginCompilerRepair.ps1 -Candidate x.log -Control x.log

    Both paths resolve to the same file: exit 2 (bad input).

.NOTES
    Exit codes (gate on $LASTEXITCODE, never on truthiness):
      0  the expected reading is met.
         Pair mode: token markers 2 -> 0 and the repair marker appeared.
         Log mode : token markers 0 and the repair marker present.
      1  the expected reading is NOT met. The verdict text says which kind of
         failure it is ("PRE-FIX reading", "INCONCLUSIVE", "REGRESSED").
      2  bad input: missing path, unreadable path, empty log, or the same file
         passed as both -Control and -Candidate.

    Dependency-free: no modules outside the project's own tools, no Python.
    Idempotent: reads logs, writes to stdout, touches nothing.
#>

[CmdletBinding()]
param(
    [string]$Control,
    [string]$Candidate,
    [string]$Log,
    [string]$BaselineControl,
    [string]$InstallRoot,
    [switch]$Verify
)

$ErrorActionPreference = 'Stop'

# --- Expectations of the 9674a1f acceptance reading -------------------------
$RepairCommit   = '9674a1f'
$ExpectedToken  = 0      # post-fix token-marker blocks
$ExpectedFrames = 0      # post-fix get_MetadataToken frame hits
$ExpectedMarker = 1      # repair marker present (count > 0)
$ControlTokens  = 2      # pre-fix control reading
$ControlFrames  = 2      # pre-fix control reading
$ControlCompletes = 2    # pre-fix control "Compile of" headers (context)
$DefectBBaseline  = 0    # defect B is not exercised by the control
$DefectBCandidate = 2    # defect B reading in the post-fix candidate

# --- Literal anchors (all matched case-sensitively) -------------------------
$RxTokenException = '\[CS\]: System\.InvalidOperationException'
$RxFrameSymbol    = 'get_MetadataToken'
# Gate-report excerpts prefix their lines with "MM-DD HH:MM:SS "; plain Unity logs
# do not. The optional prefix is accepted everywhere so both spellings read alike.
$RxStamp          = '^\s*(?:\d{2}-\d{2} \d{2}:\d{2}:\d{2}\s+)?'
$RxRepairMarker   = $RxStamp + '\[DynamicCSharp\] resolved \d+ method override declaration'
$RxRepairWarning  = $RxStamp + '\[DynamicCSharp\] \d+ method override declaration'
$RxFailedHeader   = 'failed\. Errors:$'
$RxCompileOf      = 'Compile of '
$RxQuietCompile   = 'Exception during compile of '
$RxCs584          = '\[CS584\]'
$RxErrorPrefix    = $RxStamp + '\[Error\]\s+'
$RxCustom         = $RxStamp + 'Custom:\[CS\]'
$RxBlank          = '^\s*$'
$RxFilenameTag    = '\(Filename: '
$RxErrorCount     = 'errors and exceptions:'
$RxBareCs         = '\[CS\]'

# Defect B (separate, still-open defect; never folded into the token verdict)
$RxDefectBNotSupported = 'System\.NotSupportedException'
$RxDefectBTypeBuilder  = 'TypeBuilderInstantiation'
$RxDefectBResolve      = 'ResolveOnTypeBuilderInst'

$Script:Problems = @()

function Add-Problem {
    param([string]$Text)
    $Script:Problems += $Text
    Write-Host ("  [!] {0}" -f $Text) -ForegroundColor Red
}

function Resolve-LogPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) { return $null }
    $p = $Path
    if ([System.IO.Path]::IsPathRooted($p)) { return $p }
    if (Test-Path -LiteralPath $p -PathType Leaf) { return (Get-Item -LiteralPath $p).FullName }
    foreach ($base in @($Script:Root, $Script:InstallRoot)) {
        if ([string]::IsNullOrWhiteSpace($base)) { continue }
        $cand = Join-Path $base $p
        if (Test-Path -LiteralPath $cand -PathType Leaf) { return $cand }
    }
    return (Join-Path $Script:Root $p)
}

function Read-PcLog {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "log not found: $Path"
    }
    try {
        $item = Get-Item -LiteralPath $Path
        $lines = @(Get-Content -LiteralPath $Path)
    } catch {
        throw "log could not be read: $Path ($($_.Exception.Message))"
    }
    if ($null -eq $lines -or $lines.Count -eq 0) {
        throw "log is empty: $Path"
    }
    return [pscustomobject]@{
        Path  = $item.FullName
        Name  = $item.Name
        Bytes = $item.Length
        Stamp = $item.LastWriteTime.ToString('MM-dd HH:mm:ss')
        Lines = $lines
        Count = $lines.Count
    }
}
# --- Region (block) detection -----------------------------------------------

# A line that STARTS a new log block. Deliberately case-sensitive. The optional
# "[Error] "/"[Exception] " prefix is the duplicated twin of a message, and a
# bare "[Error] " line (gate final report) is its own entry.
$RxEntryPrefix = $RxStamp + '\[(Error|Exception)\]\s'
$RxEntryAnchor = $RxStamp + '(\[(Error|Exception)\]\s+)?(\[CS\]|\[CS584\]|Compile of |Exception during compile of |Custom:\[CS\])'

# A line that CONTINUES the block opened by the previous entry: stack frames and
# the Unity bookkeeping lines. Anything else ends the block.
$RxContBlank    = '^\s*$'
$RxContFilename = '\(Filename: '
$RxContManaged  = '^\s*at '
$RxContInner    = '^\s*--- '
$RxContFrame    = '^\s*[A-Za-z_][\w./+<>]*:[^\s(]+ \(.*\)(?:\s*\(at [^)]*\))?\s*$'
$RxStackDelim   = $RxStamp + '(\[(Error|Exception)\]\s+)?UnityEngine\.StackTraceUtility:ExtractStackTrace'

function Test-PcEntryStart {
    param([string]$Line)
    if ($Line -cmatch $RxEntryPrefix) { return $true }
    if ($Line -cmatch $RxRepairMarker) { return $true }
    if ($Line -cmatch $RxRepairWarning) { return $true }
    return ($Line -cmatch $RxEntryAnchor)
}

function Test-PcContinuation {
    param([string]$Line)
    if ($Line -cmatch $RxContBlank)    { return $true }
    if ($Line -cmatch $RxContFilename) { return $true }
    if ($Line -cmatch $RxContManaged)  { return $true }
    if ($Line -cmatch $RxContInner)    { return $true }
    if ($Line -cmatch $RxContFrame)    { return $true }
    return $false
}

# Classify a block by its header line, after stripping the "[Error] " twin prefix.
function Get-PcBlockKind {
    param([string]$Header)

    $t = $Header -replace $RxStamp, ''
    $t = $t -replace '^\[(Error|Exception)\]\s+', ''
    if ($t -cmatch '^\[CS\]:\s*System\.InvalidOperationException') { return 'token' }
    if ($t -cmatch '^\[CS\]:\s*System\.NotSupportedException')    { return 'notsupported' }
    if ($t -cmatch '^\[CS584\]')                                   { return 'cs584' }
    if ($t -cmatch '^Compile of ')                                 { return 'complete' }
    if ($t -cmatch '^Exception during compile of ')                { return 'quiet' }
    if ($t -cmatch '^\[CS\]')                                      { return 'cs' }
    if ($t -cmatch '^\[DynamicCSharp\] resolved \d+ method override declaration') { return 'marker' }
    if ($t -cmatch '^\[DynamicCSharp\] \d+ method override declaration')          { return 'warn' }
    return 'other'
}
function Get-PcBlocks {
    param([object]$Log)

    $lines = $Log.Lines
    $n = $Log.Count
    $blocks = New-Object System.Collections.ArrayList
    $i = 0
    while ($i -lt $n) {
        if (-not (Test-PcEntryStart ([string]$lines[$i]))) { $i++; continue }
        $j = $i + 1
        while ($j -lt $n) {
            $next = [string]$lines[$j]
            if (Test-PcEntryStart $next) { break }
            if (-not (Test-PcContinuation $next)) { break }
            $j++
        }
        $header = [string]$lines[$i]
        $null = $blocks.Add([pscustomobject]@{
            Start    = $i
            End      = $j - 1
            Line     = $i + 1
            Header   = $header
            Kind     = (Get-PcBlockKind $header)
            Prefixed = ($header -match $RxEntryPrefix)
            Message  = (($header -replace '^\s*', '') -replace $RxErrorPrefix, '').TrimEnd()
        })
        $i = $j
    }
    return $blocks
}

# The gate's final report re-lists the run's messages with an "[Error] " prefix.
# Those copies are the SAME block as their plain original, so they are merged
# here and never counted a second time.
function Merge-PcTwins {
    param([object]$Blocks, [object]$Log)

    $lines = $Log.Lines
    $open = @{}
    $twins = 0
    foreach ($b in $Blocks) {
        if ($b.Prefixed) {
            $key = $b.Message
            if ($open.ContainsKey($key) -and $open[$key].Count -gt 0) {
                $owner = $open[$key][$open[$key].Count - 1]
                $open[$key].RemoveAt($open[$key].Count - 1)
                Add-Member -InputObject $b -NotePropertyName 'TwinOf' -NotePropertyValue $owner.Line -Force
                $twins++
            }
        } else {
            $key = $b.Message
            if (-not $open.ContainsKey($key)) { $open[$key] = New-Object System.Collections.ArrayList }
            $null = $open[$key].Add($b)
        }
    }
    return $twins
}

function Get-PcSymbolHits {
    param([object]$Blocks, [object]$Log, [string]$Pattern)

    $lines = $Log.Lines
    $span = 0
    $inside = 0
    foreach ($b in $Blocks) {
        for ($k = $b.Start; $k -le $b.End; $k++) {
            $line = [string]$lines[$k]
            if ($line -cmatch $Pattern) {
                $span++
                if ($line -match $RxEntryPrefix) { $inside++ }
            }
        }
    }
    return [pscustomobject]@{ Span = $span; Prefixed = $inside }
}
function Count-PcLines {
    param([object]$Log, [string]$Pattern)
    $c = 0
    foreach ($line in $Log.Lines) { if ([string]$line -cmatch $Pattern) { $c++ } }
    return $c
}

function Get-PcCensus {
    param([object]$Log)

    $blocks = Get-PcBlocks -Log $Log
    $twins  = Merge-PcTwins -Blocks $blocks -Log $Log
    $live   = @($blocks | Where-Object { -not $_.TwinOf })

    $tokenBlocks = @($live | Where-Object { $_.Kind -eq 'token' })
    $markerBlocks = @($live | Where-Object { $_.Kind -eq 'marker' })
    $warnBlocks  = @($live | Where-Object { $_.Kind -eq 'warn' })
    $completeBlocks = @($live | Where-Object { $_.Kind -eq 'complete' })
    $cs584Blocks = @($live | Where-Object { $_.Kind -eq 'cs584' })
    $quietBlocks = @($live | Where-Object { $_.Kind -eq 'quiet' })
    $otherCs     = @($live | Where-Object { $_.Kind -eq 'cs' })
    $defectB     = @($live | Where-Object { $_.Kind -eq 'notsupported' })

    # Defect B is also recognised by its frame symbols, in case the header shape changes.
    $defectBSymbol = @($live | Where-Object {
        ($_.Header -cmatch $RxDefectBTypeBuilder) -or ($_.Header -cmatch $RxDefectBResolve)
    })
    foreach ($b in $defectBSymbol) {
        if (-not ($defectB -contains $b)) { $defectB += $b }
    }

    # Empty-by-design check for every "failed. Errors:" header.
    $emptyBody = 0
    $nonEmpty = New-Object System.Collections.ArrayList
    foreach ($b in $completeBlocks) {
        $hasBody = $false
        if ($b.End -gt $b.Start) {
            $nxt = [string]$Log.Lines[$b.Start + 1]
            if (-not ($nxt -cmatch $RxStackDelim)) { $hasBody = $true }
        }
        if ($hasBody) {
            $body = @()
            for ($k = $b.Start + 1; $k -le $b.End; $k++) { $body += ([string]$Log.Lines[$k]).TrimEnd() }
            if ($body.Count -gt 0) { $null = $nonEmpty.Add([pscustomobject]@{ Line = $b.Line; Body = ($body -join ' | ') }) }
            else { $emptyBody++ }
        } else { $emptyBody++ }
    }

    $tokenFrames = 0
    foreach ($b in $tokenBlocks) {
        for ($k = $b.Start; $k -le $b.End; $k++) { if ([string]$Log.Lines[$k] -cmatch $RxFrameSymbol) { $tokenFrames++ } }
    }

    $refPlugin = 'MacGruber.Life.12:'
    $refBlocks = @($completeBlocks | Where-Object { $_.Message -clike ("Compile of " + $refPlugin + "*") })

    # "[CS]"-anchored lines that belong to none of the named families above.
    $tokenRaw  = Count-PcLines $Log $RxTokenException
    $nsRaw     = Count-PcLines $Log $RxDefectBNotSupported
    $otherCsRaw = (Count-PcLines $Log $RxBareCs) - $tokenRaw - $nsRaw
    if ($otherCsRaw -lt 0) { $otherCsRaw = 0 }

    $families = @(
        [pscustomobject]@{ Id = '[1]'; Name = 'token exception block  ([CS]: System.InvalidOperationException)'; Live = $tokenBlocks.Count; Raw = $tokenRaw },
        [pscustomobject]@{ Id = '[2]'; Name = 'IL frame hits  (get_MetadataToken)'; Live = $tokenFrames; Raw = (Count-PcLines $Log $RxFrameSymbol) },
        [pscustomobject]@{ Id = '[3]'; Name = 'repair marker  ([DynamicCSharp] resolved N method override ...)'; Live = $markerBlocks.Count; Raw = (Count-PcLines $Log $RxRepairMarker) },
        [pscustomobject]@{ Id = '[4]'; Name = 'failed-compile header  (Compile of <plugin> failed. Errors:)'; Live = $completeBlocks.Count; Raw = (Count-PcLines $Log $RxCompileOf) },
        [pscustomobject]@{ Id = '[5]'; Name = '[CS584] block  (internal compiler error, by-ref compiler)'; Live = $cs584Blocks.Count; Raw = (Count-PcLines $Log $RxCs584) },
        [pscustomobject]@{ Id = '[6]'; Name = 'DEFECT B block  (System.NotSupportedException / TypeBuilderInstantiation)'; Live = $defectB.Count; Raw = (Count-PcLines $Log $RxDefectBNotSupported) },
        [pscustomobject]@{ Id = '[7]'; Name = 'quiet compile failure  (Exception during compile of <plugin>:)'; Live = $quietBlocks.Count; Raw = (Count-PcLines $Log $RxQuietCompile) },
        [pscustomobject]@{ Id = '[8]'; Name = 'other [CS] block  (benign / not a token defect)'; Live = $otherCs.Count; Raw = $otherCsRaw }
    )

    $totalLive = 0
    $totalRaw  = 0
    foreach ($f in $families) { $totalLive += $f.Live; $totalRaw += $f.Raw }

    return [pscustomobject]@{
        Log            = $Log
        Blocks         = $blocks
        Live           = $live
        Twins          = $twins
        TokenBlocks    = $tokenBlocks
        TokenFrames    = $tokenFrames
        MarkerBlocks   = $markerBlocks
        WarnBlocks     = $warnBlocks
        CompleteBlocks = $completeBlocks
        Cs584Blocks    = $cs584Blocks
        QuietBlocks    = $quietBlocks
        OtherCs        = $otherCs
        DefectB        = $defectB
        EmptyBody      = $emptyBody
        NonEmptyBody   = $nonEmpty
        RefBlocks      = $refBlocks
        Families       = $families
        TotalLive      = $totalLive
        TotalRaw       = $totalRaw
    }
}
# --- Reporting ---------------------------------------------------------------

function Write-PcLogInfo {
    param([string]$Label, [object]$Census)
    $log = $Census.Log
    Write-Host ("{0,-10} {1,-30} {2,7} lines  {3}  {4}" -f $Label, $log.Name, $log.Count, $log.Stamp, $log.Path)
}

function Write-PcFamilyTable {
    param([object]$Census)

    Write-Host ("  {0,-4} {1,-62} {2,7} {3,9}" -f 'id', 'block family', 'blocks', 'raw hits')
    foreach ($f in $Census.Families) {
        $name = $f.Name
        if ($name.Length -gt 62) { $name = $name.Substring(0, 62) }
        Write-Host ("  {0,-4} {1,-62} {2,7} {3,9}" -f $f.Id, $name, $f.Live, $f.Raw)
    }
    Write-Host ("  {0,-4} {1,-62} {2,7} {3,9}" -f '', 'TOTAL of families [1]-[8] (not a whole-file count)', $Census.TotalLive, $Census.TotalRaw)
    Write-Host ("       plain/[Error] twin copies merged into their original block: {0}" -f $Census.Twins)
    Write-Host ("       blocks outside families [1]-[8] are not censused; a rising raw-hits column is not a regression.")
}

function Format-PcLineList {
    param([object]$Blocks)
    $b = @($Blocks)
    if ($b.Count -eq 0) { return 'none' }
    $parts = @()
    foreach ($x in $b) { $parts += ('line {0}' -f $x.Line) }
    return ($parts -join ', ')
}

function Get-PcCompletePlugin {
    param([string]$Message)
    return (($Message -replace '^Compile of ', '') -replace ' failed\. Errors:$', '')
}

function Get-PcQuietPlugin {
    param([string]$Message)
    $t = $Message -replace '^Exception during compile of ', ''
    $i = $t.IndexOf(':')
    if ($i -gt 0) { $t = $t.Substring(0, $i) }
    return $t
}

function Write-PcDetail {
    param([object]$Census)

    Write-Host ("  [1] token exception blocks ..... {0}" -f (Format-PcLineList $Census.TokenBlocks))
    Write-Host ("  [2] get_MetadataToken frames ... {0}" -f $Census.TokenFrames)

    if ($Census.MarkerBlocks.Count -gt 0) {
        $m = $Census.MarkerBlocks[0]
        Write-Host ("  [3] repair marker .............. PRESENT  line {0}  ""{1}""" -f $m.Line, $m.Message)
        if ($Census.MarkerBlocks.Count -gt 1) {
            Write-Host ("      (further marker blocks: {0})" -f (Format-PcLineList @($Census.MarkerBlocks[1..($Census.MarkerBlocks.Count - 1)])))
        }
    } else {
        Write-Host "  [3] repair marker .............. ABSENT"
    }
    if ($Census.WarnBlocks.Count -gt 0) {
        Write-Host ("      unresolved-declaration warning blocks: {0}" -f (Format-PcLineList $Census.WarnBlocks))
    }

    if ($Census.CompleteBlocks.Count -eq 0) {
        Write-Host "  [4] failed-compile headers ..... none"
    } else {
        Write-Host ("  [4] failed-compile headers ..... {0}" -f (Format-PcLineList $Census.CompleteBlocks))
        foreach ($b in $Census.CompleteBlocks) {
            Write-Host ("      line {0}  {1}" -f $b.Line, (Get-PcCompletePlugin $b.Message))
        }
        Write-Host ("      error list under those headers: {0} empty, {1} carrying text" -f $Census.EmptyBody, $Census.NonEmptyBody.Count)
        if ($Census.EmptyBody -gt 0) {
            Write-Host "      -> an empty list is CORRECT here: MVRPluginManager.cs:798 writes the header,"
            Write-Host "         :802 skips every entry starting with ""[CS]"", so [CS]/[CS584] entries never"
            Write-Host "         reach the list. Empty does not mean the log lost data."
        }
        foreach ($ne in $Census.NonEmptyBody) {
            Write-Host ("      -> line {0} DOES carry list text (non-""[CS]"" entries survive the filter): {1}" -f $ne.Line, $ne.Body)
        }
    }

    Write-Host ("  [5] [CS584] blocks ............. {0}" -f (Format-PcLineList $Census.Cs584Blocks))

    if ($Census.DefectB.Count -eq 0) {
        Write-Host "  [6] DEFECT B blocks ............ none"
    } else {
        Write-Host ("  [6] DEFECT B blocks ............ {0}  (SEPARATE, still-open defect)" -f (Format-PcLineList $Census.DefectB))
        $ns  = Count-PcLines $Census.Log $RxDefectBNotSupported
        $tb  = Count-PcLines $Census.Log $RxDefectBTypeBuilder
        $rov = Count-PcLines $Census.Log $RxDefectBResolve
        Write-Host ("      raw hits: NotSupportedException {0}, TypeBuilderInstantiation {1}, ResolveOnTypeBuilderInst {2}" -f $ns, $tb, $rov)
    }

    if ($Census.QuietBlocks.Count -eq 0) {
        Write-Host "  [7] quiet compile failures ..... none"
    } else {
        Write-Host ("  [7] quiet compile failures ..... {0}" -f (Format-PcLineList $Census.QuietBlocks))
        foreach ($b in $Census.QuietBlocks) {
            Write-Host ("      line {0}  {1}" -f $b.Line, (Get-PcQuietPlugin $b.Message))
        }
        Write-Host "      -> ""Exception during compile of <plugin>:"" is NOT the ""Compile of <plugin> failed."" header;"
        Write-Host "         a case-insensitive grep for ""compile of"" merges the two and inflates the count."
    }

    Write-Host ("  [8] other [CS] blocks .......... {0}" -f (Format-PcLineList $Census.OtherCs))
}
# --- Verdicts ----------------------------------------------------------------

function Get-PcSingleVerdict {
    param([object]$Census)

    $tokens = $Census.TokenBlocks.Count
    $frames = $Census.TokenFrames
    $marker = $Census.MarkerBlocks.Count

    if ($tokens -eq 0 -and $frames -eq 0 -and $marker -gt 0) {
        return [pscustomobject]@{
            Code  = 0
            Label = 'PASS - POST-FIX reading'
            Text  = 'token blocks 0 and the repair marker is present: this run entered the repaired path and the defect did not reproduce.'
        }
    }
    if ($tokens -gt 0 -and $marker -gt 0) {
        return [pscustomobject]@{
            Code  = 1
            Label = 'FAIL - REGRESSED'
            Text  = 'the repair marker is present, so the repaired path DID run, yet the token defect is still logged: the fix regressed or is incomplete.'
        }
    }
    if ($tokens -gt 0) {
        return [pscustomobject]@{
            Code  = 1
            Label = 'FAIL - PRE-FIX reading'
            Text  = 'the token defect is present and the repair marker is absent. This is the correct verdict for a PRE-FIX CONTROL log; as a candidate it means the build predates 9674a1f.'
        }
    }
    return [pscustomobject]@{
        Code  = 1
        Label = 'FAIL - INDETERMINATE (repair path never entered)'
        Text  = 'no token defect but also no "[DynamicCSharp] resolved" marker: the log neither confirms nor refutes the repair, because RepairMethodOverrideDeclarations never reported work. This is NOT the same as "the fix regressed".'
    }
}

function Write-PcPairVerdict {
    param([object]$ControlCensus, [object]$CandidateCensus)

    $ct = $ControlCensus.TokenBlocks.Count
    $cf = $ControlCensus.TokenFrames
    $cm = $ControlCensus.MarkerBlocks.Count
    $kt = $CandidateCensus.TokenBlocks.Count
    $kf = $CandidateCensus.TokenFrames
    $km = $CandidateCensus.MarkerBlocks.Count

    Write-Host ''
    Write-Host '--- Verdict -------------------------------------------------------------------------'
    Write-Host ("  expectation   post-fix: token blocks 0, get_MetadataToken frames 0, repair marker present")
    Write-Host ("  control       token blocks {0} ({1}), frames {2}, marker {3}" -f `
        $ct, (Format-PcLineList $ControlCensus.TokenBlocks), $cf, $(if ($cm -gt 0) { 'PRESENT' } else { 'ABSENT' }))
    Write-Host ("  candidate     token blocks {0} ({1}), frames {2}, marker {3}" -f `
        $kt, (Format-PcLineList $CandidateCensus.TokenBlocks), $kf, $(if ($km -gt 0) { 'PRESENT' } else { 'ABSENT' }))
    Write-Host ("  defect B      control {0} block(s), candidate {1} block(s) ({2}) - reported separately, never folded into this verdict" -f `
        $ControlCensus.DefectB.Count, $CandidateCensus.DefectB.Count, (Format-PcLineList $CandidateCensus.DefectB))
    Write-Host ("  twins merged  plain/[Error] duplicates: control {0}, candidate {1}" -f $ControlCensus.Twins, $CandidateCensus.Twins)
    $c584 = $ControlCensus.Cs584Blocks.Count
    $k584 = $CandidateCensus.Cs584Blocks.Count
    $cref = $ControlCensus.RefBlocks.Count
    $kref = $CandidateCensus.RefBlocks.Count

    Write-Host ("  by-ref context [CS584] control {0} -> candidate {1}; [4]-headers for MacGruber.Life.12 control {2} -> candidate {3}" -f `
        $c584, $k584, $cref, $kref)
    Write-Host ("  moved         token {0} -> {1}; [CS584] {2} -> {3}; MacGruber.Life.12 failure headers {4} -> {5}" -f `
        $ct, $kt, $c584, $k584, $cref, $kref)

    # A pair is only meaningful for the defect families its CONTROL actually
    # exhibits; the other families are reported, never silently averaged in.
    $exercised = @()
    if ($ct -gt 0 -or $cf -gt 0) { $exercised += 'token' }
    if ($c584 -gt 0) { $exercised += '[CS584]' }
    if ($cref -gt 0) { $exercised += 'by-ref failure headers' }
    if ($exercised.Count -gt 0 -and $exercised -notcontains 'token') {
        Write-Host '  note          the control does not exhibit the token defect, so this pair is judged on the families it does exercise;'
        Write-Host '                the token reading then rests on the candidate alone (plus the repair marker).'
    }

    if ($cm -gt 0 -and $kt -gt 0 -and $km -le 0) {
        Write-Host '  hint          the control carries the repair marker while the candidate carries the token defect:'
        Write-Host '                the two logs look transposed (-Control and -Candidate swapped).'
    }

    $reasons = @()
    if (($ct -gt 0 -or $cf -gt 0) -and ($kt -gt 0 -or $kf -gt 0)) {
        $reasons += 'the token defect is still present in the candidate'
    }
    if ($c584 -gt 0 -and $k584 -gt 0) {
        $reasons += ('the [CS584] by-ref family is still present in the candidate ({0} block(s))' -f $k584)
    }
    if ($cref -gt 0 -and $kref -gt 0) {
        $reasons += ('failed-compile headers are still present in the candidate ({0} block(s))' -f $kref)
    }
    if ($km -le 0) {
        $reasons += 'the repair marker is absent from the candidate, so the repaired path never ran'
    }

    $code = 0
    $label = 'PASS'
    $text = ''

    if ($exercised.Count -eq 0) {
        $code = 1; $label = 'FAIL - INCONCLUSIVE CONTROL'
        $text = 'the control exhibits none of the token, [CS584] or by-ref families, so it cannot demonstrate that the candidate removed anything.'
    } elseif ($reasons.Count -gt 0) {
        $code = 1
        if ($km -le 0 -and ($kt -gt 0 -or $k584 -gt 0 -or $kref -gt 0)) {
            $label = 'FAIL - PRE-FIX reading of the candidate'
        } elseif ($kt -gt 0 -or $k584 -gt 0 -or $kref -gt 0) {
            $label = 'FAIL - REGRESSED'
        } else {
            $label = 'FAIL - INDETERMINATE (repair path never entered)'
        }
        $text = ($reasons -join '; ') + '.'
    } else {
        $code = 0; $label = 'PASS'
        $text = ('every defect family the control exercises now reads 0 in the candidate ({0}), and the repair marker is present.' -f ($exercised -join ', '))
    }

    Write-Host ("  RESULT        {0} - {1}" -f $label, $text)
    Write-Host ''
    return $code
}
function Write-PcSingleVerdict {
    param([object]$Census)

    $v = Get-PcSingleVerdict -Census $Census
    $reading = 'NO REPAIR EVIDENCE'
    if ($Census.TokenBlocks.Count -gt 0) { $reading = 'PRE-FIX (token defect present)' }
    elseif ($Census.MarkerBlocks.Count -gt 0) { $reading = 'POST-FIX (repair marker present)' }
    elseif ($Census.Cs584Blocks.Count -gt 0) { $reading = 'PRE-FIX-like ([CS584] present, no repair marker)' }

    Write-Host ''
    Write-Host '--- Verdict -------------------------------------------------------------------------'
    Write-Host ("  log           {0}" -f $Census.Log.Path)
    Write-Host ("  reading       {0} ({1} line(s))" -f $reading, $Census.Log.Count)
    Write-Host ("  expectation   post-fix: token blocks 0, get_MetadataToken frames 0, repair marker present")
    Write-Host ("  found         token blocks {0} ({1}), frames {2}, marker {3}" -f `
        $Census.TokenBlocks.Count, (Format-PcLineList $Census.TokenBlocks), $Census.TokenFrames, `
        $(if ($Census.MarkerBlocks.Count -gt 0) { 'PRESENT' } else { 'ABSENT' }))
    Write-Host ("  defect B      {0} block(s) ({1}) - separate, still-open defect, never folded into this verdict" -f `
        $Census.DefectB.Count, (Format-PcLineList $Census.DefectB))
    Write-Host ("  twins merged  plain/[Error] duplicates: {0}" -f $Census.Twins)
    Write-Host ("  RESULT        {0} - {1}" -f $v.Label, $v.Text)
    Write-Host ''
    return $v.Code
}

# --- Main --------------------------------------------------------------------

if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    $Script:Root = Split-Path -Parent $PSScriptRoot
} else {
    $Script:Root = (Get-Location).ProviderPath
}

if ([string]::IsNullOrWhiteSpace($InstallRoot)) { $InstallRoot = $env:VAM_INSTALL }
if ([string]::IsNullOrWhiteSpace($InstallRoot)) { $InstallRoot = Split-Path -Parent $Script:Root }
$Script:InstallRoot = $InstallRoot

function Exit-PcBadInput {
    param([string]$Text)
    Write-Host ''
    Write-Host ("INPUT ERROR: {0}" -f $Text) -ForegroundColor Red
    Write-Host "exit 2 - bad input"
    exit 2
}

$hasPair = (-not [string]::IsNullOrWhiteSpace($Control)) -or (-not [string]::IsNullOrWhiteSpace($Candidate))
$hasBoth = (-not [string]::IsNullOrWhiteSpace($Control)) -and (-not [string]::IsNullOrWhiteSpace($Candidate))
$hasSingle = -not [string]::IsNullOrWhiteSpace($Log)

if ($hasSingle -and $hasPair) {
    Exit-PcBadInput '-Log cannot be combined with -Control/-Candidate; pick one mode.'
}
if (-not $hasSingle -and -not $hasPair) {
    Exit-PcBadInput 'nothing to judge: pass -Log <file> or -Control <file> -Candidate <file>.'
}
if ($hasPair -and -not $hasBoth) {
    Exit-PcBadInput '-Control and -Candidate must be passed together.'
}

Write-Host ('=' * 88)
Write-Host ' Plugin-compiler token repair acceptance - commit 9674a1f (McsDriver.ReadMethodToken)'
Write-Host ' Block-based census: hits are grouped into log BLOCKS. Raw line counts are shown'
Write-Host ' only to make the difference visible; the verdict never uses them.'
Write-Host ('=' * 88)
Write-Host ("repo root     {0}" -f $Script:Root)
Write-Host ("install root  {0}" -f $Script:InstallRoot)
if ($Verify) { Write-Host 'mode          report only (-Verify); this script never mutates anything' }

if ($hasSingle) {
    Write-Host 'mode          single log (-Log)'
    $path = Resolve-LogPath $Log
    try { $census = Get-PcCensus -Log (Read-PcLog -Path $path) }
    catch { Exit-PcBadInput $_.Exception.Message }

    $baseCensus = $null
    if (-not [string]::IsNullOrWhiteSpace($BaselineControl)) {
        $basePath = Resolve-LogPath $BaselineControl
        try { $baseCensus = Get-PcCensus -Log (Read-PcLog -Path $basePath) }
        catch { Exit-PcBadInput ("-BaselineControl: {0}" -f $_.Exception.Message) }
    }

    Write-Host ''
    Write-PcLogInfo 'LOG' $census
    Write-Host ''
    Write-PcFamilyTable $census
    Write-Host ''
    Write-Host '--- Per-block detail ---------------------------------------------------------------'
    Write-PcDetail $census
    if ($null -ne $baseCensus) {
        Write-Host ''
        Write-Host '--- Movement against -BaselineControl ----------------------------------------------'
        Write-Host ("  baseline      {0}" -f $baseCensus.Log.Path)
        Write-Host ("  moved         token {0} -> {1}; frames {2} -> {3}; marker {4} -> {5}; [CS584] {6} -> {7}" -f `
            $baseCensus.TokenBlocks.Count, $census.TokenBlocks.Count, `
            $baseCensus.TokenFrames, $census.TokenFrames, `
            $(if ($baseCensus.MarkerBlocks.Count -gt 0) { 'PRESENT' } else { 'ABSENT' }), `
            $(if ($census.MarkerBlocks.Count -gt 0) { 'PRESENT' } else { 'ABSENT' }), `
            $baseCensus.Cs584Blocks.Count, $census.Cs584Blocks.Count)
        Write-Host ("  defect B      baseline {0} -> log {1} (reported separately, never folded into the verdict)" -f `
            $baseCensus.DefectB.Count, $census.DefectB.Count)
        Write-Host '  note          this line is context only; the verdict below judges this log on its own reading.'
    }
    $code = Write-PcSingleVerdict $census
    exit $code
}

Write-Host 'mode          log pair (-Control vs -Candidate)'
$controlPath   = Resolve-LogPath $Control
$candidatePath = Resolve-LogPath $Candidate

$controlFull = $null
$candidateFull = $null
try { $controlFull = (Get-Item -LiteralPath $controlPath).FullName } catch { $controlFull = $controlPath }
try { $candidateFull = (Get-Item -LiteralPath $candidatePath).FullName } catch { $candidateFull = $candidatePath }
if ([string]::Equals($controlFull, $candidateFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    Exit-PcBadInput ("-Control and -Candidate resolve to the same file: {0}" -f $controlFull)
}

try {
    $controlCensus   = Get-PcCensus -Log (Read-PcLog -Path $controlPath)
    $candidateCensus = Get-PcCensus -Log (Read-PcLog -Path $candidatePath)
} catch {
    Exit-PcBadInput $_.Exception.Message
}

Write-Host ''
Write-PcLogInfo 'CONTROL' $controlCensus
Write-Host ''
Write-PcFamilyTable $controlCensus
Write-Host ''
Write-Host '--- Per-block detail (control) -----------------------------------------------------'
Write-PcDetail $controlCensus
Write-Host ''
Write-PcLogInfo 'CANDIDATE' $candidateCensus
Write-Host ''
Write-PcFamilyTable $candidateCensus
Write-Host ''
Write-Host '--- Per-block detail (candidate) ---------------------------------------------------'
Write-PcDetail $candidateCensus

$code = Write-PcPairVerdict -ControlCensus $controlCensus -CandidateCensus $candidateCensus
Write-Host ("  exit code     {0}" -f $code)
exit $code
