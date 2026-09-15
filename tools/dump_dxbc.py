#!/usr/bin/env python3
"""Disassemble the compiled shader programs extracted from the player build.

``Extract-VaMShaders.py`` writes out the DXBC of every program VaM shipped, but
its contract.json keeps only the reflection - the resource bindings Unity
recorded.  The instructions, the vertex input signature and the arithmetic live
in the blob itself, and ``fxc /dumpbin`` reads them back.

This is how a reconstruction gets a source of truth: the disassembly says which
vertex attributes a pass consumes (so the HLSL signature is not a guess), which
constant buffer slot each uniform landed in, and how many instructions the
original spent where.  It is the study tool for the shader work, the counterpart
of ``check_shaders.py``, which compiles what we wrote.

    python tools/dump_dxbc.py Skybox                 # summary of every program
    python tools/dump_dxbc.py Skybox --asm           # write the disassembly out
    python tools/dump_dxbc.py Hair --stage vertex --asm --out artifacts/hair

Stereo variants are skipped unless ``--all-variants`` is given: the project runs
desktop, and a stereo variant is the same program with a different eye matrix.
"""
from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
BLOBS = REPO / "artifacts" / "shader-blobs"
DEFAULT_OUT = REPO / "artifacts" / "dxbc-asm"

PLATFORM = "kShaderCompPlatformD3D11"
STAGE_TO_TYPE = {
    "vertex": "Vertex",
    "fragment": "Pixel",
    "geometry": "Geometry",
    "hull": "Hull",
    "domain": "Domain",
}
STEREO = ("STEREO_CUBEMAP_RENDER_ON", "UNITY_SINGLE_PASS_STEREO")


def find_fxc():
    """The newest fxc.exe the Windows SDK left behind, or None."""
    kits = Path(os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")) / \
        "Windows Kits" / "10" / "bin"
    found = sorted(kits.glob("*/x64/fxc.exe")) if kits.is_dir() else []
    return found[-1] if found else None


def load_contracts(blobs: Path, pattern: str):
    """Every contract whose shader name matches, as (blob dir name, contract)."""
    wanted = pattern.lower()
    out = []
    for path in sorted(blobs.glob("*/contract.json")):
        contract = json.loads(path.read_text(encoding="utf-8"))
        if wanted in (contract.get("name") or "").lower():
            out.append((path.parent.name, contract))
    return out


def disassemble(fxc: Path, dxbc: Path):
    """fxc /dumpbin of one blob, or None when it is not accepted as one."""
    proc = subprocess.run([str(fxc), "/nologo", "/dumpbin", str(dxbc)],
                          capture_output=True, text=True, errors="replace")
    if proc.returncode != 0:
        return None
    return proc.stdout.replace("\r\n", "\n")


def signature(text: str):
    """The input signature, as (name, register) pairs."""
    block = re.search(r"// Input signature:(.*?)\n\n", text, re.S)
    if not block:
        return []
    out = []
    for line in block.group(1).splitlines():
        parts = line.split("//")
        if len(parts) < 2:
            continue
        fields = parts[1].split()
        if len(fields) >= 3 and not fields[0].startswith("-"):
            out.append((fields[0], fields[3] if len(fields) > 3 else ""))
    return out


def counts(text: str):
    """Instruction count and the resources a program declares."""
    instructions = sum(
        1 for line in text.splitlines()
        if re.match(r"^\s*[a-z_][\w.]*\s", line) and not line.strip().startswith("//")
    )
    cbs = sorted(set(re.findall(r"dcl_constantbuffer CB(\d+)", text)))
    samplers = sorted(set(re.findall(r"dcl_sampler s(\d+)", text)))
    srvs = sorted(set(re.findall(r"dcl_resource_structured t(\d+)", text)))
    return instructions, cbs, samplers, srvs


def main(argv):
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("pattern", help="substring of the shader name")
    parser.add_argument("--blobs", default=str(BLOBS))
    parser.add_argument("--out", default=str(DEFAULT_OUT))
    parser.add_argument("--fxc", default=None)
    parser.add_argument("--stage", default=None, choices=sorted(STAGE_TO_TYPE),
                        help="only this stage (default: all)")
    parser.add_argument("--keyword", default=None,
                        help="only programs whose keyword list contains this token")
    parser.add_argument("--asm", action="store_true", help="write the disassembly out")
    parser.add_argument("--all-variants", action="store_true",
                        help="include the stereo variants")
    args = parser.parse_args(argv[1:])

    fxc = Path(args.fxc) if args.fxc else find_fxc()
    if fxc is None or not Path(fxc).is_file():
        print("fxc.exe not found - install the Windows SDK or pass --fxc")
        return 2

    blobs = Path(args.blobs)
    contracts = load_contracts(blobs, args.pattern)
    if not contracts:
        print(f"no shader name matched {args.pattern!r} under {blobs}")
        return 2

    wanted_type = STAGE_TO_TYPE.get(args.stage) if args.stage else None
    total = failed = 0
    for blob_dir, contract in contracts:
        print(f"\n== {contract['name']}  ({blob_dir})")
        for entry in contract.get("programs", []):
            if entry.get("platform") != PLATFORM or not entry.get("is_dxbc"):
                continue
            if wanted_type and wanted_type not in entry["program_type"]:
                continue
            keywords = entry.get("header_keywords") or []
            if args.keyword and args.keyword not in keywords:
                continue
            if not args.all_variants and any(k in keywords for k in STEREO):
                continue
            text = disassemble(fxc, blobs / entry["file"])
            total += 1
            if text is None:
                failed += 1
                print(f"   ! {entry['file']} -- fxc would not read the blob")
                continue
            instructions, cbs, samplers, srvs = counts(text)
            sig = " ".join(f"{n}-{r}" for n, r in signature(text))
            print(f"   {entry['blob_index']:>3} {entry['program_type']:<12} "
                  f"instr={instructions:>4} cb={','.join(cbs):<10} "
                  f"s={len(samplers)} t={len(srvs)}  [{','.join(keywords) or 'none'}]")
            if sig:
                print(f"       in: {sig}")
            if args.asm:
                out_dir = Path(args.out) / blob_dir
                out_dir.mkdir(parents=True, exist_ok=True)
                (out_dir / (Path(entry["file"]).name + ".asm")).write_text(
                    text, encoding="utf-8")

    print(f"\n{total - failed}/{total} blobs disassembled")
    if args.asm:
        print(f"written to {Path(args.out)}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
