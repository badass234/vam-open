#!/usr/bin/env python3
"""Compile the generated VaM shaders with the DirectX shader compiler.

Unity's feedback loop costs minutes per iteration; fxc answers the same question
about HLSL syntax, overload resolution and register types in milliseconds.  This
script lifts each CGPROGRAM block out of ``VaM_Rebuild\\Assets\\Shader``, feeds it
to fxc with the profile its pragmas ask for, and prints whatever fxc rejects.

ShaderLab itself is invisible to fxc - a pass that names a missing entry point or
sets a bad render state still "compiles" - so this is a pre-flight for the code
inside the blocks, not a substitute for a Unity run.

    python tools/check_shaders.py                # every pass of every shader
    python tools/check_shaders.py GlossNMCull    # only shaders whose name matches

Every pass is compiled once per entry point and once per keyword set in
KEYWORD_SETS.  That matters: AutoLight and the shader library both branch on the
light and shadow keywords, and compiling with a single keyword set is exactly how
a variant Unity builds anyway goes unchecked.

A pass that declares a hull and a domain is compiled as a tessellation pipeline:
its control-point program at vs_5_0, the hull at hs_5_0, the domain at ds_5_0 and
its fragment stage at ps_5_0, all with SHADER_TARGET 50 to match Unity's own
preamble for those passes.  Its four stages are compiled under every keyword set
the pass declares, exactly as Unity builds them, and the whole pipeline of a
shadow-caster pass - which reads no lighting keyword - is compiled once.

Requires the Windows SDK's fxc.exe and Unity 2018.1.9f2's CGIncludes; both are
located automatically, or pass --fxc / --unity.
"""
from __future__ import annotations

import argparse
import os
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
SHADERS = REPO / "VaM_Rebuild" / "Assets" / "Shader"
CGINC = REPO / "shader-src" / "VamGpuSkinning.cginc"
OUT = REPO / "artifacts" / "hlsl" / "out"

DEFAULT_UNITY = Path(os.environ.get("ProgramFiles", r"C:\Program Files")) / \
    "Unity" / "Hub" / "Editor" / "2018.1.9f2" / "Editor"

# Unity's shader compiler defines these before it parses a source file, and
# passes /Gec so that the unqualified `half` globals in its own includes are
# accepted at SM4.0 (fxc otherwise rejects them with X3650).  A tessellated pass
# is built for Shader Model 5, which is what the original hull and domain
# programs were compiled for and what Unity's own preamble sets for them.
PLATFORM = "#define SHADER_API_D3D11 1\n#define SHADER_TARGET {target}\n"

TARGET_TESS = 50
TARGET_PLAIN = 40

# Keyword sets that read differently in HLSLSupport/AutoLight/UnityCG and in the
# VaM library.  The bare SHADOWS_DEPTH set is not a real light - it is the one
# combination multi_compile_fwdadd_fullshadows produces that AutoLight.cginc does
# not cover, and the one that used to break the Unity build.
KEYWORD_SETS = [
    "DIRECTIONAL",
    "DIRECTIONAL SHADOWS_SCREEN",
    "DIRECTIONAL SHADOWS_SCREEN SHADOWS_SOFT",
    "DIRECTIONAL LIGHTPROBE_SH VERTEXLIGHT_ON",
    "LIGHTMAP_ON SHADOWS_SHADOWMASK",
    "SHADOWS_DEPTH",
    "SPOT SHADOWS_DEPTH SHADOWS_SOFT",
    "POINT SHADOWS_CUBE",
    "POINT_COOKIE",
    "POINT_COOKIE SHADOWS_CUBE",
    "DIRECTIONAL_COOKIE",
]

# The shadow-caster pass has no lighting keywords of its own.
UNKEYED_ENTRIES = {"VamShadowVertex"}


def find_fxc():
    """The newest fxc.exe the Windows SDK left behind, or None."""
    kits = Path(os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")) / \
        "Windows Kits" / "10" / "bin"
    found = sorted(kits.glob("*/x64/fxc.exe")) if kits.is_dir() else []
    return found[-1] if found else None


def find_cgincludes(unity):
    """CGIncludes for a Unity editor given either its directory or Unity.exe."""
    root = Path(unity)
    if root.is_file():
        root = root.parent
    candidates = [root / "Data" / "CGIncludes", root]
    for candidate in candidates:
        if (candidate / "UnityCG.cginc").is_file():
            return candidate
    return None


def split_blocks(text):
    for index, block in enumerate(re.findall(r"CGPROGRAM(.*?)ENDCG", text, re.S)):
        yield index, block


def entry_point(block, kind):
    match = re.search(r"#pragma\s+" + kind + r"\s+(\w+)", block)
    return match.group(1)


def fragment_entries(block):
    """Both fragment entry points, whichever one this pass declares."""
    entry = entry_point(block, "fragment")
    if entry == "VamFragment":
        return [entry, "VamFragmentAdd"]
    if entry == "VamFragmentAdd":
        return [entry, "VamFragment"]
    return [entry]


def keyword_defines(keywords):
    return "".join(f"#define {token} 1\n" for token in keywords.split())


def has_pragma(block, kind):
    return re.search(r"#pragma\s+" + kind + r"\s+\w+", block) is not None


def compile_block(fxc, cgincludes, name, index, entry, profile, source, keywords, slot):
    OUT.mkdir(parents=True, exist_ok=True)
    path = OUT / f"{name}.{index}.{entry}.{slot}.hlsl"
    # pragmas are ShaderLab's, not fxc's; it only warns about them
    source = re.sub(r"^\s*#pragma.*$", "", source, flags=re.M)
    target = TARGET_TESS if profile.endswith("_5_0") else TARGET_PLAIN
    path.write_text(PLATFORM.format(target=target) + keyword_defines(keywords) + source,
                    encoding="utf-8")
    command = [str(fxc), "/nologo", "/Gec", "/T", profile, "/E", entry,
               "/I", str(cgincludes), "/Fo", str(path.with_suffix(".dxbc")), str(path)]
    proc = subprocess.run(command, capture_output=True, text=True)
    return proc.returncode, (proc.stdout + proc.stderr).strip()


def is_unkeyed(block, entry, tess):
    """Whether a stage of a pass is built once instead of once per lighting keyword.

    A shadow-caster pass is built once per ``multi_compile_shadowcaster`` variant
    and no stage of it reads the lighting keywords, so the whole tessellation
    pipeline of one is compiled unkeyed.  Every other stage keeps the per-entry
    rule this gate has always used: the shadow vertex program is unkeyed while the
    fragment stage of the same pass is still compiled under every keyword set.
    """
    return entry in UNKEYED_ENTRIES or (tess and "#pragma multi_compile_shadowcaster" in block)


def main(argv):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("pattern", nargs="?", default="", help="substring of the shader file name")
    parser.add_argument("--fxc", default=None, help="path to fxc.exe (default: the Windows SDK's)")
    parser.add_argument("--unity", default=str(DEFAULT_UNITY),
                        help="Unity 2018.1.9f2 editor directory or Unity.exe")
    args = parser.parse_args(argv[1:])

    fxc = Path(args.fxc) if args.fxc else find_fxc()
    cgincludes = find_cgincludes(args.unity)
    if fxc is None or not Path(fxc).is_file():
        print("fxc.exe not found - install the Windows SDK or pass --fxc")
        return 2
    if cgincludes is None:
        print(f"no CGIncludes under {args.unity} - pass --unity")
        return 2

    shaders = sorted(path for path in SHADERS.glob("*.shader") if args.pattern in path.name)
    if not shaders:
        print(f"no shaders matched {args.pattern!r} under {SHADERS}")
        return 2

    programs = failures = 0
    for path in shaders:
        # point the include at the tracked original, so a stale copy in the
        # project cannot hide a mistake in shader-src
        text = path.read_text(encoding="utf-8").replace(
            "../VaMShaders/VamGpuSkinning.cginc", str(CGINC))
        for index, block in split_blocks(text):
            tess = has_pragma(block, "hull")
            if tess:
                # The whole tessellation pipeline: the control-point program, the
                # hull, the domain, and the fragment stage the domain feeds.
                entries = [
                    (entry_point(block, "vertex"), "vs_5_0"),
                    (entry_point(block, "hull"), "hs_5_0"),
                    (entry_point(block, "domain"), "ds_5_0"),
                ]
                entries += [(entry, "ps_5_0") for entry in fragment_entries(block)]
            else:
                entries = [(entry_point(block, "vertex"), "vs_4_0")]
                entries += [(entry, "ps_4_0") for entry in fragment_entries(block)]
            for entry, profile in entries:
                source = block
                if profile.startswith("ps_"):
                    source = re.sub(r"(#pragma\s+fragment\s+)\w+", r"\g<1>" + entry, block)
                sets = [""] if is_unkeyed(block, entry, tess) else KEYWORD_SETS
                for slot, keywords in enumerate(sets):
                    programs += 1
                    code, output = compile_block(fxc, cgincludes, path.stem, index,
                                                 entry, profile, source, keywords, slot)
                    if code != 0:
                        failures += 1
                        print(f"\n=== {path.name} pass {index} "
                              f"({entry}, {profile}, {keywords or 'no keywords'}) ===")
                        print(output)
    print(f"\n{programs - failures}/{programs} programs compiled, {failures} failed")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
