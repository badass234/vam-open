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

Five of the compute-buffer shaders -- the "*TessMapped*" skin shaders -- also
ship a hull and a domain program on every pass, forward and shadow alike.  Both
are reconstructed as well, and a pass that carries them is emitted with the
hull/domain pragmas at Shader Model 5 (the shipped programs are SM5.0), which is
what lets the material's density map subdivide the body.  See the tessellation
block in VamGpuSkinning.cginc.

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
  * Where the contract carries a hull and a domain program for a pass, both are
    reconstructed from the same bytecode and the pass declares them (VAM_TESS,
    target 5.0).  A pass whose contract carries only one of the two is a hard
    error rather than a silent downgrade: that pair is what makes the body the
    silhouette its material was authored for.

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

# Shading models this project has no reconstruction of.  Their "*ComputeBuff"
# variants belong to the family's own source, not to the body one, so shading
# them with VamGpuSkinning.cginc would render hair and the whole Marmoset IBL
# set with the skin library.  They are left pending instead: the game ships them
# verbatim in the z_sha bundle, and VamShaderProvider reads them from there.
UNTRANSCRIBED_FAMILIES = ("Custom/Hair/", "Marmoset/")

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
    # Unity's ColorWriteMask is Alpha=1, Blue=2, Green=4, Red=8 (All=15), so 14
    # -- what the game's SeparatelyAlpha passes serialise -- means RGB.
    if int(blend.get("colMask", {}).get("val", 15)) != 15:
        mask = int(blend["colMask"]["val"])
        out.append("ColorMask " + "".join(c for b, c in ((8, "R"), (4, "G"), (2, "B"), (1, "A"))
                                          if mask & b))
    if int(st.get("alphaToMask", {}).get("val", 0)):
        out.append("AlphaToMask On")
    if st.get("offsetFactor", {}).get("val", 0) or st.get("offsetUnits", {}).get("val", 0):
        out.append(f"Offset {num(st['offsetFactor']['val'])}, {num(st['offsetUnits']['val'])}")
    return out


# Passes whose original program discards although the render state says the pass
# neither writes depth nor is opaque.  The original shader sources were written
# per family, and these cut their alpha in a pass that a state-only reading
# would draw with no cutoff at all.  They were read out of the shipped programs
# (see docs/Development.md); no state tells them apart from the hair layer-0
# passes, which must not clip.  Keyed by shader name and the contract's own pass
# index, so which ones are reconstructable does not change the answer.
CUTOFF_IN_TRANSPARENT_PASS = {
    "Custom/Hair/ScalpComputeBuff": {0},
    "Custom/Hair/ScalpSeparateAlphaComputeBuff": {0},
    "Custom/Hair/MainSeparateAlphaLayerScalpComputeBuff": {1},
    "Custom/Hair/MainAlternateComputeBuff": {3},
    "Custom/Subsurface/TransparentCutoutSeparateAlpha": {0},
    "Custom/Subsurface/TransparentCutoutSeparateAlphaComputeBuff": {0},
    "Custom/Subsurface/TransparentGlossNoCullComputeBuff": {0},
}


def cutoff_order(opaque_fb_index: int):
    """The cutoffs to look for, most likely first, for the n-th opaque pass.

    A two-layer shader tests its second opaque layer against _Pass1Cutoff rather
    than _Cutoff, so the order flips once an opaque forward-base pass has been
    seen.
    """
    if opaque_fb_index == 0:
        return ("_Cutoff", "_Pass1Cutoff", "_Cutoff1", "_Cutoff2")
    return ("_Pass1Cutoff", "_Cutoff", "_Cutoff2", "_Cutoff1")


def cutoff_for(pas: dict, lightmode: str, names: set, opaque_fb_index: int,
               shader_name: str, pass_index: int):
    """Which alpha cutoff this pass tests against, if any.

    Two things decide it.  The property list says whether the pass has a cutoff
    at all, and -- for a shader with two opaque layers -- which of the two it
    is: the second layer's pass carries _Pass1Cutoff, so the index of the opaque
    forward-base pass picks the order to look in.  (_Cutoff1/_Cutoff2 are the
    same idea under the names Custom/Hair/MainAlternate* uses.)

    Whether the pass tests it is a property of the original source, not of the
    render state: the shipped programs cut their alpha in some transparent
    passes and not in others that look identical from the state alone.  Reading
    the state gets every pass that draws depth or that adds light right; the
    transparent passes it cannot call are listed in CUTOFF_IN_TRANSPARENT_PASS.
    """
    if not {"_Cutoff", "_Pass1Cutoff", "_Cutoff1", "_Cutoff2"} & names:
        return None
    cutoff = next((c for c in cutoff_order(opaque_fb_index) if c in names), None)
    if cutoff is None:
        return None
    if lightmode == "SHADOWCASTER":
        return cutoff
    if pass_index in CUTOFF_IN_TRANSPARENT_PASS.get(shader_name, ()):
        return cutoff
    if pas["state"]["zWrite"]["val"] or lightmode == "FORWARDADD":
        return cutoff
    return None


def pass_tessellates(pas: dict) -> bool:
    """Whether the contract carries a hull/domain pair for this pass.

    Both stages belong to one decision: their arithmetic is shared (the hull
    produces the factors, the domain interpolates against them and finishes in
    the varyings the fragment stage reads), so a contract offering only one of
    the two is reported rather than silently rendered without either.
    """
    stages = pas.get("stages", {})
    hull = bool(stages.get("progHull"))
    domain = bool(stages.get("progDomain"))
    if hull != domain:
        raise ValueError(
            f"pass {pass_tags(pas).get('LIGHTMODE') or '<untagged>'} has "
            f"progHull={hull} but progDomain={domain}; tessellation is a pair")
    return hull


def entry_points(lightmode: str, tess: bool = False):
    """The cginc entry points and keyword set matching an original LightMode."""
    if lightmode == "FORWARDBASE":
        pairs = ("VamVertex", "VamFragment", ["multi_compile_fwdbase"])
    elif lightmode == "FORWARDADD":
        pairs = ("VamVertex", "VamFragmentAdd", ["multi_compile_fwdadd_fullshadows"])
    elif lightmode == "SHADOWCASTER":
        pairs = ("VamShadowVertex", "VamShadowFragment", ["multi_compile_shadowcaster"])
    else:
        pairs = ("VamVertex", "VamFragment", ["multi_compile_fwdbase"])
    if not tess:
        return pairs
    # A tessellated pass replaces the vertex stage with three: the control point
    # program, the hull that subdivides it, and -- on the far side of the
    # subdivision -- a domain that finishes either the varyings or the
    # shadow-caster clip position.
    vert, frag, keywords = pairs
    tess_vert = "VamTessVertex"
    tess_domain = "VamTessShadowDomain" if lightmode == "SHADOWCASTER" else "VamTessDomain"
    return (tess_vert, frag, keywords, tess_domain)


def emit_pass(pas: dict, lightmode: str, props: list, opaque_fb_index: int,
              family: str, shader_name: str, pass_index: int) -> str:
    bufs = pass_buffers(pas)
    if family == "skin" and not {"verts", "normals"} <= bufs:
        raise ValueError(f"pass {lightmode or '<untagged>'} binds {sorted(bufs)}, "
                         "not the compute buffers -- it cannot be skinned")
    if family == "mesh" and bufs:
        raise ValueError(f"pass {lightmode or '<untagged>'} binds {sorted(bufs)}; "
                         "a plain shader's pass reads the vertex attributes")
    names = {p["name"] for p in props}
    tess = pass_tessellates(pas)
    if tess and family != "skin":
        raise ValueError(f"pass {lightmode or '<untagged>'} is tessellated but is not "
                         "skinned; the reconstructed domain reads the compute buffers")
    if tess:
        # The far side of the subdivision runs in the shader model the original
        # hull and domain programs were compiled for.
        entry = entry_points(lightmode, tess=True)
        vert, frag, keywords, tess_domain = entry
    else:
        vert, frag, keywords = entry_points(lightmode)

    body = []
    body.append("#pragma target 5.0" if tess else "#pragma target 4.0")
    if tess:
        body.append("#define VAM_TESS")
    if family == "mesh":
        body.append("// drawn the ordinary way: the vertex stage transforms the mesh")
        body.append("#define VAM_MESH_SKIN")
    elif "tangents" not in bufs:
        body.append("// hair meshes are bound without a tangent buffer")
        body.append("#define VAM_NO_TANGENTS")
    cutoff = cutoff_for(pas, lightmode, names, opaque_fb_index, shader_name, pass_index)
    if cutoff:
        body.append("#define VAM_PASS_CUTOFF " + cutoff)
    body.append(f"#pragma vertex {vert}")
    if tess:
        body.append(f"#pragma hull {TESS_HULL}")
        body.append(f"#pragma domain {tess_domain}")
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

# The hull's patch-constant function has one implementation for every light mode:
# the subdivision does not depend on the light.  Its control point program is the
# vertex stage, so it is named by #pragma vertex.
TESS_HULL = "VamTessHull"


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
        if name.startswith(UNTRANSCRIBED_FAMILIES):
            return None
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
    tess = any(pass_tessellates(pas) for pas in sub["passes"]
               if pass_lightmode(pas) not in SKIP_LIGHTMODES)

    lines = [
        BANNER,
        f"// {name}",
        "//",
        "// Reconstructed from the game's own DXBC by scripts/New-VaMShaders.py.",
        f"// Program contracts: artifacts/shader-blobs/{blob_dir}/",
        f"// Shading model:     shader-src/{library}",
        f"// Vertex stage:      {'GPU skinning (compute buffers)' if skin else 'mesh attributes'}",
        f"// Tessellation:      {'hull + domain (Shader Model 5.0)' if tess else 'none'}",
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
    tess_passes = 0
    opaque_fb = 0
    for pass_index, pas in enumerate(sub["passes"]):
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
        lines.append(emit_pass(pas, lm, props, opaque_fb, family, name, pass_index))
        lines.append("")
        passes += 1
        if pass_tessellates(pas):
            tess_passes += 1
        if lm == "FORWARDBASE" and is_opaque_pass(pas):
            opaque_fb += 1
    lines.append("\t}")
    if contract.get("fallback"):
        lines.append(f'\tFallback "{contract["fallback"]}"')
    lines.append("}")
    return "\n".join(lines) + "\n", passes, tess_passes


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
    tess_passes = 0
    for blob_dir, data, family in contracts:
        text, n, t = emit_shader(data, blob_dir, skipped, family)
        if args.list:
            print(f"{family:<5} {data['name']:<58} "
                  f"passes={n} props={len(data['properties']):>2}"
                  f"{f'  tess={t}' if t else ''}")
            continue
        if not n:
            print(f"warning: {data['name']} produced no passes", file=sys.stderr)
        target = SHADER_DIR / (data["name"].replace("/", "_") + ".shader")
        target.write_text(text, encoding="utf-8")
        shaders += 1
        passes += n
        tess_passes += t

    if args.list:
        print(f"\n{len(contracts)} reconstructed, {len(pending)} pending (no shading model yet):")
        for name in pending:
            print(f"      {name}")
        print(f"\n{len(contracts)} shaders, {len(skipped)} passes skipped")
        return 0

    SHADER_DIR.mkdir(parents=True, exist_ok=True)
    CGINC_DST.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(CGINC_SRC, CGINC_DST)

    print(f"wrote {shaders} shaders ({passes} passes, {tess_passes} tessellated) to "
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
