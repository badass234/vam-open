#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Rebuild Virt-a-Mate's shaders from their extracted DXBC.

Two families are produced, and they differ only in where the vertex stage gets
its data:

  * The "*ComputeBuff" shaders.  VaM skins its characters on the GPU: DAZSkinV2
    runs the skinning as a compute shader and then draws the mesh with the
    result bound as structured buffers (verts/normals/tangents, see
    shader-src/VamGpuSkinning.cginc).  A shader that cannot read those buffers
    draws the mesh in its bind pose -- the T-pose that hangs motionless in the
    middle of the scene while the animation plays on the invisible skeleton.
    Exactly 51 of the game's shaders can read them, they are the "*ComputeBuff"
    family, and the AssetRipper stubs shipped in VaM_Rebuild/Assets/Shader are
    not among them: AssetRipper has no access to the DXBC and emits a
    placeholder that samples the mesh in bind pose, unlit.

  * Their plain twins -- the same shaders without the suffix, for meshes drawn
    the ordinary way.  VaM generates both from one source, so the vertex
    programs pack their varyings identically and the fragment programs are
    byte-identical; the plain passes bind no structured buffers and take the
    position, normal and tangent from the vertex attributes instead.  Those
    shaders therefore reuse the same cginc with VAM_MESH_SKIN defined.

This script reads the per-shader contracts produced by Extract-VaMShaders.py
(artifacts/shader-blobs/<name>/contract.json) and writes real ShaderLab files
that forward to the reconstruction in shader-src/VamGpuSkinning.cginc:

  * Properties are restored from the contract (name, inspector label, type,
    range and default), in the original order.
  * Render state (cull, z-write, z-test, blend, alpha-to-mask, offset) is
    restored per pass.
  * Every pass keeps its original LightMode, which the contract carries in
    state.m_Tags; forward-base passes get VamFragment, forward-additive passes
    VamFragmentAdd, shadow-caster passes the shadow-caster path.
  * META, DEFERRED and the deferred pre-passes are dropped: this project
    renders forward only, and those passes need a rendering setup (lightmap
    baking, a G-buffer) that the rebuilt project does not have.
  * Tessellation stages are dropped: the hull/domain programs of the
    *TessMapped* variants are not reconstructed, so those passes render with
    plain vertex skinning, exactly like the *TessMappedFixed* variants that
    VaM ships side by side with them.

Shaders whose fragment program is a *different* model are left alone: they are
reported as pending and keep whatever AssetRipper wrote.  Reconstructing them
means another shading library, not another entry in this table.

The cginc is copied into the project because Unity can only include files from
inside Assets, but the repository tracks it once, under shader-src.

Usage:
    python scripts/New-VaMShaders.py             # regenerate the .shader files
    python scripts/New-VaMShaders.py --list      # inventory, write nothing
"""
from __future__ import annotations

import argparse
import json
import shutil
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
CONTRACTS = REPO / "artifacts" / "shader-blobs"
CGINC_SRC = REPO / "shader-src" / "VamGpuSkinning.cginc"
CGINC_DST = REPO / "VaM_Rebuild" / "Assets" / "VaMShaders" / "VamGpuSkinning.cginc"
SHADER_DIR = REPO / "VaM_Rebuild" / "Assets" / "Shader"
# relative to the generated .shader files, which live in Assets/Shader
CGINC_INCLUDE = "../VaMShaders/VamGpuSkinning.cginc"

CULL = {0: "Off", 1: "Front", 2: "Back"}
ZTEST = {0: "Off", 1: "Never", 2: "Less", 3: "Equal", 4: "LEqual",
         5: "GEqual", 6: "Greater", 7: "NotEqual", 8: "Always"}
BLEND = {0: "Zero", 1: "One", 2: "DstColor", 3: "SrcColor", 4: "OneMinusDstColor",
         5: "SrcAlpha", 6: "OneMinusSrcColor", 7: "DstAlpha", 8: "OneMinusDstAlpha",
         9: "SrcAlphaSaturate", 10: "OneMinusSrcAlpha"}

# Passes this project does not use (forward rendering only, no lightmap baking).
SKIP_LIGHTMODES = {"META", "DEFERRED", "PREPASSBASE", "PREPASSFINAL"}

# Plain (non-"*ComputeBuff") shaders that are the ordinary-mesh twins of the
# compute-buffer family.  VaM generates both from one source, so the vertex
# programs pack their varyings identically and the fragment programs are
# byte-identical -- only the shader model differs -- which is what lets the
# shading model in VamGpuSkinning.cginc serve them unchanged.  Every pair under
# these prefixes was checked by disassembling both fragment programs (see
# docs/shader-reconstruction.md); the two conditions below stand in for that
# check here, so that a plain shader of a *different* model -- one with no
# compute-buffer twin at all, like Custom/Subsurface/EmissiveGlow -- is left
# alone instead of being quietly shaded by the wrong library.
MESH_FAMILY_PREFIXES = ("Custom/Subsurface/",)

# Declared by Unity's own include files, so the generated shader must not
# declare them a second time.  UnityLightingCommon.cginc declares _SpecColor.
UNITY_DECLARED = {"_SpecColor"}

def num(v: float) -> str:
    """Shortest ShaderLab spelling of a contract default."""
    return f"{v:g}"


def property_line(p: dict) -> str:
    """One line of the Properties block, as the original shader had it."""
    name, label, kind = p["name"], p["description"], p["type"]
    d = p["defaults"]
    if kind == 0:
        return f'{name} ("{label}", Vector) = ({num(d[0])},{num(d[1])},{num(d[2])},{num(d[3])})'
    if kind == 2:
        return f'{name} ("{label}", Float) = {num(d[0])}'
    if kind == 3:
        return f'{name} ("{label}", Range({num(d[1])}, {num(d[2])})) = {num(d[0])}'
    if kind == 4:
        cube = p["texture"]["m_TexDim"] == 4
        default = p["texture"]["m_DefaultName"] or ("black" if cube else "white")
        return f'{name} ("{label}", {"Cube" if cube else "2D"}) = "{default}" {{}}'
    raise ValueError(f"unknown property type {kind} for {name}")


def uniform_line(p: dict) -> str:
    """The HLSL declaration of a property (Unity declares none of them itself)."""
    name, kind = p["name"], p["type"]
    if name in UNITY_DECLARED:
        return ""
    if kind == 0:
        return f"float4 {name};"
    if kind in (2, 3):
        return f"float {name};"
    if kind == 4:
        if p["texture"]["m_TexDim"] == 4:
            return f"samplerCUBE {name};"
        # Unity fills <name>_ST for every texture property, but the shader has
        # to declare it before TRANSFORM_TEX can use it.
        return f"sampler2D {name};\nfloat4 {name}_ST;"
    raise ValueError(f"unknown property type {kind} for {name}")


def pass_tags(pas: dict) -> dict:
    """The original pass tags, e.g. LIGHTMODE=FORWARDBASE, QUEUE=Geometry-1."""
    return {k.upper(): v for k, v in pas["state"].get("m_Tags", {}).get("tags", [])}


def pass_lightmode(pas: dict) -> str:
    return pass_tags(pas).get("LIGHTMODE", "").upper()


def pass_buffers(pas: dict) -> set:
    return {b["name"] for stage in pas["stages"].values()
            for variant in stage for b in variant.get("buffers", [])}


def render_state(pas: dict) -> list:
    """ShaderLab commands for one pass, restored from its serialised state."""
    st = pas["state"]
    out = []
    cull = CULL.get(int(st["culling"]["val"]), "Back")
    if cull != "Back":
        out.append(f"Cull {cull}")
    ztest = ZTEST.get(int(st["zTest"]["val"]), "LEqual")
    if ztest != "LEqual":
        out.append(f"ZTest {ztest}")
    out.append(f"ZWrite {'On' if st['zWrite']['val'] else 'Off'}")
    blend = st["rtBlend0"]
    src = BLEND.get(int(blend["srcBlend"]["val"]), "One")
    dst = BLEND.get(int(blend["destBlend"]["val"]), "Zero")
    if (src, dst) != ("One", "Zero"):
        out.append(f"Blend {src} {dst}")
    if int(blend.get("colMask", {}).get("val", 15)) != 15:
        mask = int(blend["colMask"]["val"])
        out.append("ColorMask " + "".join(c for b, c in ((1, "R"), (2, "G"), (4, "B"), (8, "A"))
                                          if mask & b))
    if int(st.get("alphaToMask", {}).get("val", 0)):
        out.append("AlphaToMask On")
    if st.get("offsetFactor", {}).get("val", 0) or st.get("offsetUnits", {}).get("val", 0):
        out.append(f"Offset {num(st['offsetFactor']['val'])}, {num(st['offsetUnits']['val'])}")
    return out


def cutoff_for(pas: dict, lightmode: str, names: set, opaque_fb_index: int):
    """Which alpha cutoff this pass tests against, if any.

    A shader with two opaque layers carries _Pass1Cutoff for the second one,
    which is why the index of the opaque forward-base pass matters.
    """
    if lightmode == "FORWARDADD":
        return None
    z_opaque = bool(pas["state"]["zWrite"]["val"])
    blend = pas["state"]["rtBlend0"]
    blend_opaque = (int(blend["srcBlend"]["val"]), int(blend["destBlend"]["val"])) == (1, 0)
    if lightmode == "SHADOWCASTER":
        for cand in ("_Cutoff", "_Pass1Cutoff"):
            if cand in names:
                return cand
        return None
    if not (z_opaque and blend_opaque):
        return None
    order = ("_Cutoff", "_Pass1Cutoff") if opaque_fb_index == 0 else ("_Pass1Cutoff", "_Cutoff")
    for cand in order:
        if cand in names:
            return cand
    return None


def entry_points(lightmode: str):
    """The cginc entry points and keyword set matching an original LightMode."""
    if lightmode == "FORWARDBASE":
        return "VamVertex", "VamFragment", ["multi_compile_fwdbase"]
    if lightmode == "FORWARDADD":
        return "VamVertex", "VamFragmentAdd", ["multi_compile_fwdadd_fullshadows"]
    if lightmode == "SHADOWCASTER":
        return "VamShadowVertex", "VamShadowFragment", ["multi_compile_shadowcaster"]
    return "VamVertex", "VamFragment", ["multi_compile_fwdbase"]


def emit_pass(pas: dict, lightmode: str, props: list, opaque_fb_index: int,
              family: str) -> str:
    bufs = pass_buffers(pas)
    if family == "skin" and not {"verts", "normals"} <= bufs:
        raise ValueError(f"pass {lightmode or '<untagged>'} binds {sorted(bufs)}, "
                         "not the compute buffers -- it cannot be skinned")
    if family == "mesh" and bufs:
        raise ValueError(f"pass {lightmode or '<untagged>'} binds {sorted(bufs)}; "
                         "a plain shader's pass reads the vertex attributes")
    names = {p["name"] for p in props}
    vert, frag, keywords = entry_points(lightmode)

    body = []
    body.append("#pragma target 4.0")
    if family == "mesh":
        body.append("// drawn the ordinary way: the vertex stage transforms the mesh")
        body.append("#define VAM_MESH_SKIN")
    elif "tangents" not in bufs:
        body.append("// hair meshes are bound without a tangent buffer")
        body.append("#define VAM_NO_TANGENTS")
    cutoff = cutoff_for(pas, lightmode, names, opaque_fb_index)
    if cutoff:
        body.append("#define VAM_PASS_CUTOFF " + cutoff)
    body.append(f"#pragma vertex {vert}")
    body.append(f"#pragma fragment {frag}")
    for kw in keywords:
        body.append(f"#pragma {kw}")
    body.append("")
    for p in props:
        line = uniform_line(p)
        if line:
            body.extend(line.split("\n"))
    body.append("")
    body.extend(f"#define VAM_HAS_{p['name']}" for p in props)
    body.append(f'#include "{CGINC_INCLUDE}"')

    out = ["\t\tPass", "\t\t{"]
    lm = pass_tags(pas).get("LIGHTMODE")
    if lm:
        out.append(f'\t\t\tTags {{ "LightMode"="{lm}" }}')
    for line in render_state(pas):
        out.append("\t\t\t" + line)
    out.append("\t\t\tCGPROGRAM")
    out.extend("\t\t\t" + line for line in body)
    out.append("\t\t\tENDCG")
    out.append("\t\t}")
    return "\n".join(out)


TAG_CASE = {"QUEUE": "Queue", "RENDERTYPE": "RenderType"}
BANNER = "// " + "-" * 78


def is_opaque_pass(pas: dict) -> bool:
    blend = pas["state"]["rtBlend0"]
    return (bool(pas["state"]["zWrite"]["val"])
            and (int(blend["srcBlend"]["val"]), int(blend["destBlend"]["val"])) == (1, 0))


def classify(name: str, sub: dict, names: set):
    """Which vertex path a contract's shader needs, or None if it is pending.

    "skin"  -- reads the compute buffers, skinned by DAZSkinV2
    "mesh"  -- plain twin of one of those shaders, drawn the ordinary way
    None    -- a different shading model, not reconstructed by this script
    """
    if name.endswith("ComputeBuff"):
        return "skin"
    if not name.startswith(MESH_FAMILY_PREFIXES):
        return None
    if name + "ComputeBuff" not in names:
        return None
    # A plain twin must not bind the compute buffers in any pass this project
    # renders -- if it did, its fragment program would be a different model.
    for pas in sub["passes"]:
        if pass_lightmode(pas) in SKIP_LIGHTMODES:
            continue
        if pass_buffers(pas) & {"verts", "normals", "tangents"}:
            return None
    return "mesh"


def emit_shader(contract: dict, blob_dir: str, skipped: list, family: str):
    """The whole ShaderLab file for one shader."""
    name = contract["name"]
    sub = contract["subshaders"][0]
    props = contract["properties"]
    skin = family == "skin"
    library = "VamGpuSkinning.cginc"

    lines = [
        BANNER,
        f"// {name}",
        "//",
        "// Reconstructed from the game's own DXBC by scripts/New-VaMShaders.py.",
        f"// Program contracts: artifacts/shader-blobs/{blob_dir}/",
        f"// Shading model:     shader-src/{library}",
        f"// Vertex stage:      {'GPU skinning (compute buffers)' if skin else 'mesh attributes'}",
        BANNER,
        f'Shader "{name}" {{',
    ]
    if props:
        lines.append("\tProperties {")
        lines.extend("\t\t" + property_line(p) for p in props)
        lines.append("\t}")
        lines.append("")
    lines.append("\tSubShader {")
    tags = sub.get("tags") or []
    if tags:
        rendered = " ".join(f'"{TAG_CASE.get(k.upper(), k)}"="{v}"' for k, v in tags)
        lines.append(f"\t\tTags {{ {rendered} }}")
    lines.append(f'\t\tLOD {sub["lod"]}')
    lines.append("")

    passes = 0
    opaque_fb = 0
    for pas in sub["passes"]:
        lm = pass_lightmode(pas)
        if lm in SKIP_LIGHTMODES:
            skipped.append((name, lm, "rendering path not used by this project"))
            continue
        bufs = pass_buffers(pas)
        if skin and not {"verts", "normals"} <= bufs:
            skipped.append((name, lm or "<untagged>", "pass does not read the compute buffers"))
            continue
        if not skin and bufs:
            skipped.append((name, lm or "<untagged>", "pass reads the compute buffers"))
            continue
        lines.append(emit_pass(pas, lm, props, opaque_fb, family))
        lines.append("")
        passes += 1
        if lm == "FORWARDBASE" and is_opaque_pass(pas):
            opaque_fb += 1
    lines.append("\t}")
    if contract.get("fallback"):
        lines.append(f'\tFallback "{contract["fallback"]}"')
    lines.append("}")
    return "\n".join(lines) + "\n", passes


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description="Rebuild VaM's shaders.")
    parser.add_argument("--list", action="store_true",
                        help="print the inventory instead of writing files")
    args = parser.parse_args(argv)

    if not CONTRACTS.is_dir():
        print(f"error: {CONTRACTS} is missing -- run scripts/Extract-VaMShaders.py first",
              file=sys.stderr)
        return 2
    if not CGINC_SRC.is_file():
        print(f"error: {CGINC_SRC} is missing", file=sys.stderr)
        return 2

    contracts = []
    pending = []
    found = []
    for path in sorted(CONTRACTS.glob("*/contract.json")):
        data = json.loads(path.read_text(encoding="utf-8"))
        found.append((path.parent.name, data))
    names = {data["name"] for _, data in found}

    for blob_dir, data in found:
        family = classify(data["name"], data["subshaders"][0], names)
        if family:
            contracts.append((blob_dir, data, family))
        else:
            pending.append(data["name"])
    if not contracts:
        print("error: no reconstructable contracts found", file=sys.stderr)
        return 2

    skipped = []
    shaders = 0
    passes = 0
    for blob_dir, data, family in contracts:
        text, n = emit_shader(data, blob_dir, skipped, family)
        if args.list:
            print(f"{family:<5} {data['name']:<58} "
                  f"passes={n} props={len(data['properties']):>2}")
            continue
        if not n:
            print(f"warning: {data['name']} produced no passes", file=sys.stderr)
        target = SHADER_DIR / (data["name"].replace("/", "_") + ".shader")
        target.write_text(text, encoding="utf-8")
        shaders += 1
        passes += n

    if args.list:
        print(f"\n{len(contracts)} reconstructed, {len(pending)} pending (no shading model yet):")
        for name in pending:
            print(f"      {name}")
        print(f"\n{len(contracts)} shaders, {len(skipped)} passes skipped")
        return 0

    SHADER_DIR.mkdir(parents=True, exist_ok=True)
    CGINC_DST.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(CGINC_SRC, CGINC_DST)

    print(f"wrote {shaders} shaders ({passes} passes) to "
          f"{SHADER_DIR.relative_to(REPO)}")
    print(f"copied  {CGINC_SRC.relative_to(REPO)} -> {CGINC_DST.relative_to(REPO)}")
    for name, lightmode, why in skipped:
        print(f"skipped {lightmode:<13} {name}  ({why})")
    if pending:
        print(f"\n{len(pending)} of VaM's shaders are still AssetRipper stubs "
              "(different shading model, not built here):")
        for name in pending:
            print(f"  {name}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
