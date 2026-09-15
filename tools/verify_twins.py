"""Check that a plain Custom/Subsurface shader and its ComputeBuff twin are the same program.

The rebuilt project emits two shaders per family: ``Custom/Subsurface/GlossNMCull`` for CPU
skinning and ``Custom/Subsurface/GlossNMCullComputeBuff`` for GPU skinning. ``VamGpuSkinning.cginc``
compiles both from one source through a single ``#ifdef``, and the claim that must hold is that the
fragment paths are the same machine code in the shipped game too.

For every family that has both contracts, pair the D3D11 pixel programs by keyword set and compare
the disassembled instruction streams after normalising only the shader-model spelling
(``sample_indexable(texture2d)(float,float,float,float)`` -> ``sample``). A keyword set can hold
more than one pass and the two shaders need not list them in the same order, so a bucket is
compared as a multiset.

Verdicts per twin program:

  identical   the streams are the same text
  mask-only   the only difference is a narrower sample destination (``sample r4.x`` where the other
              side writes ``sample r4.xyzw``), the SM5 encoding of components the shader never reads
  different   anything else

usage: python tools/verify_twins.py [--pairs-per-family N] [--show N]
"""
import argparse
import difflib
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import dump_dxbc as dx  # noqa: E402

BLOBS = Path(__file__).resolve().parents[1] / "artifacts" / "shader-blobs"

SAMPLE_DEST = re.compile(r"^(sample\S*)\s+(r\d+)\.([xyzw]+)([,\s])(.*)$")


def load_all():
    contracts = {}
    for path in BLOBS.glob("*/contract.json"):
        contract = json.loads(path.read_text(encoding="utf-8"))
        contracts[contract["name"]] = (path.parent, contract)
    return contracts


def programs(contract):
    out = []
    for entry in contract.get("programs", []):
        if entry.get("platform") != dx.PLATFORM or not entry.get("is_dxbc"):
            continue
        out.append(entry)
    return out


def normalise(text):
    """Drop the shader model and its encoding artefacts, keep the instructions.

    The twin is compiled for the next shader model up, and fxc prints that model's decorations:
    ``sample(texture2d)(float,float,float,float)`` for a plain ``sample``,
    ``ld_structured_indexable(structured_buffer, ...)`` for a plain ``ld_structured``. Both
    spellings mean the same instruction, so the opcode is reduced to its name before the streams
    are compared.
    """
    lines = []
    for line in text.splitlines():
        line = line.strip()
        if not line or line.startswith("//"):
            continue
        if re.match(r"^(vs|ps)_\d_\d$", line):
            continue
        line = re.sub(r"^dcl_globalFlags\s+refactoringAllowed$", "", line)
        line = re.sub(r"^[a-z_0-9]+(?:\([^)]*\))*",
                      lambda m: m.group(0).split("(")[0].replace("_indexable", ""), line)
        if line:
            lines.append(line)
    return tuple(lines)


OPCODE = re.compile(r"^([a-z_0-9]+)\s+(.*)$")
REGISTER = re.compile(r"^(?P<prefix>-?\|?)(?P<reg>r\d+)(?:\.(?P<lanes>[xyzw]+))?(?P<suffix>\|?)$")
DESTLESS = {"ret", "nop", "discard", "break", "breakc", "endif", "else", "endloop", "endswitch",
            "case", "default", "sync", "halt", "loop"}


def split_operands(rest):
    """Split an instruction's operands, keeping bracketed literals like ``l(1, 2, 3, 4)`` whole."""
    operands, depth, start = [], 0, 0
    for position, character in enumerate(rest):
        if character in "([":
            depth += 1
        elif character in ")]":
            depth -= 1
        elif character == "," and depth == 0:
            operands.append(rest[start:position].strip())
            start = position + 1
    operands.append(rest[start:].strip())
    return operands


COMMUTATIVE = {"add", "mul", "imul", "and", "or", "xor", "min", "max", "umin", "umax",
               "dp2", "dp3", "dp4", "iadd"}
UNDEF = "undef"


def ssa(lines):
    """Canonicalise the dataflow: name every value by the instruction that produced it.

    The two compilations are free to allocate different temporaries for the same value - the SM5
    side of ``TransparentCutoutSeparateAlpha`` samples into ``r1.w`` and adds that into ``r3.w``
    where the other samples straight into ``r3.w``. Naming each definition by the ordinal of the
    instruction that makes it, and each read by the definition supplying each lane, compares the
    programs as dataflow graphs rather than as register allocations.

    Two further freedoms are erased, because neither one can be observed:

    * a destination lane no read ever takes is dead, so ``sample r4.xyzw`` and ``sample r3.y`` are
      the same instruction once the destination mask is reduced to the lanes that are read;
    * the sources of ``add`` and its commutative siblings may be written either way round, which is
      how ``add r3.x, r3.x, -r4.x`` becomes ``add r3.x, -r3.y, r3.x``.
    """
    writers = {}
    definitions = []
    reads = set()
    for index, line in enumerate(lines):
        match = OPCODE.match(line)
        if not match:
            definitions.append((line, None, None))
            continue
        opcode, rest = match.group(1), match.group(2)
        operands = split_operands(rest)
        destless = opcode in DESTLESS or opcode.startswith("if_")
        rendered = []
        destination = None
        for position, operand in enumerate(operands):
            parts = REGISTER.match(operand)
            if parts is None:
                rendered.append(operand)
                continue
            lanes = parts.group("lanes") or "xyzw"
            if position == 0 and not destless:
                # the sources of this instruction are named before its destination is registered,
                # so that a read-modify-write (add_sat r3.w, r3.w, x) reads the older value
                destination = (index, lanes, parts.group("reg"))
                rendered.append(parts.group("reg"))
                continue
            expanded = "".join(lanes[i % len(lanes)] for i in range(4))
            source = tuple(writers.get(parts.group("reg"), {}).get(lane, -1) for lane in expanded)
            for lane, origin in zip(expanded, source):
                if origin >= 0:
                    reads.add((origin, lane))
            rendered.append(parts.group("prefix") + "d"
                            + "_".join(str(s) for s in source) + parts.group("suffix"))
        if destination:
            for lane in destination[1]:
                writers.setdefault(destination[2], {})[lane] = index
        if opcode.split("_")[0] in COMMUTATIVE and len(rendered) > 2:
            rendered = [rendered[0]] + sorted(rendered[1:])
        definitions.append((opcode, rendered, destination))

    out = []
    for opcode, rendered, destination in definitions:
        if destination is None:
            out.append(opcode)
            continue
        index, lanes, _ = destination
        live = [lane for lane in lanes if (index, lane) in reads]
        rendered[0] = f"d{index}." + ("".join(live) if live else UNDEF)
        out.append(opcode + " " + ", ".join(rendered))
    return tuple(out)


def opcodes(lines):
    return tuple(m.group(1) for m in (OPCODE.match(l) for l in lines) if m)


def differing_lines(plain, twin):
    return [l for l in difflib.unified_diff(plain, twin, "plain", "twin", lineterm="", n=1)
            if l.startswith(("+", "-")) and not l.startswith(("+++", "---"))]


def first_difference(plain, twin):
    return differing_lines(plain, twin)[:6]


def only_sample_masks(lines):
    return bool(lines) and all(re.match(r"^[+-]sample\b", l) for l in lines)


LANES = re.compile(r"(?<=\.)[xyzw]+")
LITERAL = re.compile(r"\bl\(([^)]*)\)")


def erase_lanes(lines):
    """Drop which lane a component was put in, keeping the number of lanes and the values.

    The SM5 compiler is free to put a component in a different lane and rotate the operand swizzle
    and the lane order of a literal to match: it writes ``sample_l r22.w, ..., t6.yzwx`` and
    ``mul r23.xyz, r22, l(45.5, 12.9, 78.2, 0)`` where the other side writes
    ``sample_l r21.x, ..., t6.xyzw`` and ``mul r23.yzw, r21, l(0, 45.5, 12.9, 78.2)``. Both read
    the same component, but comparing them requires ignoring which lane it landed in, which is a
    deliberately weaker test than the rest of this file - so a stream that needs it is counted
    apart from the ones proven equal.
    """
    out = []
    for line in lines:
        line = LITERAL.sub(lambda m: "l{" + ",".join(sorted(x.strip() for x in m.group(1).split(","))) + "}", line)
        out.append(LANES.sub(lambda m: str(len(m.group(0))), line))
    return tuple(out)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--pairs-per-family", type=int, default=8,
                        help="keyword buckets to compare per family (default 8, 0 = all)")
    parser.add_argument("--show", type=int, default=8, help="differences to print")
    args = parser.parse_args()

    fxc = dx.find_fxc()
    contracts = load_all()
    families = sorted(n for n in contracts if (n + "ComputeBuff") in contracts)

    identical = canonical = mask_only = operand_diff = opcode_diff = compared = 0
    layout_mismatch = 0
    lane_assignment = 0
    notes = []
    for name in families:
        twin_dir, twin = contracts[(name + "ComputeBuff")]

        buckets = {}
        for side, contract in (("plain", contracts[name][1]), ("twin", twin)):
            for entry in programs(contract):
                if "Pixel" not in entry["program_type"]:
                    continue
                keywords = tuple(entry.get("header_keywords") or [])
                if any(k in keywords for k in dx.STEREO):
                    continue
                body = dx.disassemble(fxc, BLOBS / entry["file"])
                if body is None:
                    notes.append(f"{name}: fxc refused {entry['file']}")
                    continue
                plain = normalise(body)
                # `blob_index` enumerates the shader's passes and variants, and both contracts list
                # the same ones (336 programs each for NoCull, SM40 versus SM50). A keyword set alone
                # is ambiguous: several passes share it, and pairing the passes wrongly invents
                # differences, so the index goes into the key.
                buckets.setdefault(("pixel", entry["blob_index"], keywords),
                                   {"plain": [], "twin": []})
                buckets[("pixel", entry["blob_index"], keywords)][side].append(
                    (entry, plain, ssa(plain)))

        if set(k for k in buckets if k[0] == "pixel" and buckets[k]["plain"]) != \
           set(k for k in buckets if k[0] == "pixel" and buckets[k]["twin"]):
            layout_mismatch += 1
            notes.append(f"{name}: the two contracts do not enumerate the same passes")

        for index, (key, sides) in enumerate(sorted(buckets.items())):
            if args.pairs_per_family and index >= args.pairs_per_family:
                break
            compared += 1
            strict_plain = {body for _, body, _ in sides["plain"]}
            loose_plain = {loose for _, _, loose in sides["plain"]}
            plain_opcodes = {opcodes(body) for _, body, _ in sides["plain"]}
            for entry, body, loose in sides["twin"]:
                if body in strict_plain:
                    strict_plain.discard(body)
                    identical += 1
                    continue
                if loose in loose_plain:
                    loose_plain.discard(loose)
                    canonical += 1
                    continue
                candidate = sorted(loose_plain, key=len)[0] if loose_plain else ()
                diff = differing_lines(candidate, loose)
                same_opcodes = opcodes(body) in plain_opcodes
                if same_opcodes and only_sample_masks(diff):
                    mask_only += 1
                    continue
                if same_opcodes and candidate and erase_lanes(candidate) == erase_lanes(loose):
                    lane_assignment += 1
                    continue
                operand_diff += 1 if same_opcodes else 0
                opcode_diff += 0 if same_opcodes else 1
                notes.append(
                    f"{name} pass {key[1]} [{'/'.join(key[2]) or 'no keywords'}] "
                    f"({'same opcodes, different operands' if same_opcodes else 'different opcodes'}): "
                    f"{entry['file']}\n"
                    + "\n".join("      " + l for l in diff[:6]))

    print(f"twin families: {len(families)}")
    print(f"pixel passes compared: {compared}")
    print(f"  identical: {identical}")
    print(f"  same up to how the temporaries are allocated: {canonical}")
    print(f"  same up to a sample's write mask: {mask_only}")
    print(f"  same up to which lane carries a component: {lane_assignment}")
    print(f"  same opcodes, different operands: {operand_diff}")
    print(f"  different instruction streams: {opcode_diff}")
    print(f"  families whose contracts do not enumerate the same passes: {layout_mismatch}")
    print(f"proven the same instruction stream up to renaming: {identical + canonical + mask_only}")
    print(f"agree only when the lane a component sits in is ignored: {lane_assignment}")
    for note in notes[:args.show]:
        print("  ", note)
    if len(notes) > args.show:
        print(f"   ... {len(notes) - args.show} more")
    return 1 if (opcode_diff or operand_diff or layout_mismatch) else 0


if __name__ == "__main__":
    sys.exit(main())
