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
# Two sources, read in this order and deduplicated by name.  The engine data
# files carry the shaders Unity instantiates by name; z_sha is the bundle VaM
# ships its own copies of them in, and it is the only source for some of the
# plain twins -- the engine data files do not carry Custom/Hair/* at all.
# Extract-VaMShaders.py writes both.
CONTRACT_SOURCES = (
    REPO / "artifacts" / "shader-blobs",
    REPO / "artifacts" / "zsha-blobs",
)
CONTRACTS = CONTRACT_SOURCES[0]
CGINC_SRC = REPO / "shader-src" / "VamGpuSkinning.cginc"
CGINC_DST = REPO / "VaM_Rebuild" / "Assets" / "VaMShaders" / "VamGpuSkinning.cginc"
SHADER_DIR = REPO / "VaM_Rebuild" / "Assets" / "Shader"
# relative to the generated .shader files, which live in Assets/Shader
CGINC_INCLUDE = "../VaMShaders/VamGpuSkinning.cginc"
# The runtime half of the same rule.  A material the game loaded out of a
# bundle keeps the bundle's own copy of its family even when this project
# defines one of the same name, and VamShaderProvider.UseProjectShader moves it
# across -- but only for a name on this list.  The list is written here rather
# than kept by hand because it changes with every shader that is ported, and
# because a name on it that is *not* really reconstructed is worse than a
# missing one: Shader.Find answers with the 71 AssetRipper placeholders the
# project still holds just as happily, and a material moved onto one of those
# loses the shading it came with (GPUTools/MeshedVR/HairOpt among them, which
# the hair's optimised path does use).
PROJECT_SHADER_LIST = REPO / "src" / "Assembly-CSharp" / "MeshVR" / "VamProjectShaders.cs"
# The same file where it is compiled from. The generator runs after the project has been assembled
# (its output is the .shader files, which the assembly step wipes), so the copy src\ holds is the
# one a checkout starts from and this is the one a later run has to keep in step with it.
PROJECT_SHADER_LIST_DST = (REPO / "VaM_Rebuild" / "Assets" / "Scripts" / "Assembly-CSharp"
                           / "MeshVR" / "VamProjectShaders.cs")

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
#
# The hair's plain twins are listed for the same reason, and their absence was
# the last thing that kept the bundle in the hair path.  VaM does not look them
# up through VamShaderProvider at all: the hair is CPU-skinned and drawn with
# Graphics.DrawMesh, so DAZHairMesh's material carries the plain name, and only
# the z_sha bundle could resolve it.  Their contracts come from z_sha alone.
MESH_FAMILY_PREFIXES = ("Custom/Subsurface/", "Custom/Hair/")

# Declared by Unity's own include files, so the generated shader must not
# declare them a second time.  UnityLightingCommon.cginc declares _SpecColor.
UNITY_DECLARED = {"_SpecColor"}

# Shading models this project has no reconstruction of.  Their "*ComputeBuff"
# variants belong to the family's own source, not to the body one, so shading
# them with VamGpuSkinning.cginc would render the whole Marmoset IBL set with
# the skin library.  They are left pending instead: the game ships them verbatim
# in the z_sha bundle, and VamShaderProvider reads them from there.
#
# The hair used to be listed here too.  It is not a different model: the same
# library shades it, through the three knobs the hair alone declares -- the uv
# window, the per-pass vertex offset a thicken pass adds, and the cutoffs under
# its own names (_Cutoff1/_Cutoff2) -- all of which VamGpuSkinning.cginc now
# carries (see docs/shader-reconstruction.md).
UNTRANSCRIBED_FAMILIES = ("Marmoset/",)

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


# The four names a VaM shader may give its alpha cutoff to.  A two-layer family
# gives its second opaque layer _Pass1Cutoff rather than _Cutoff, and
# Custom/Hair/MainAlternate gives each of its two layers _Cutoff1 and _Cutoff2.
CUTOFF_NAMES = ("_Cutoff", "_Pass1Cutoff", "_Cutoff1", "_Cutoff2")


def stage_globals(pas: dict, stage: str) -> set:
    """The $Globals names a pass's stage actually reads.

    A vec4 property owns a constant-buffer slot whether or not the program loads
    it, so neither the property list nor the render state can say which of a
    shader's several cutoffs one pass tests.  The reflection can: a slot the
    program never reads is not listed.  Unity compiles one program per keyword
    set and per stage; the stereo variants are dropped because they read the same
    slots as the mono ones.
    """
    found = set()
    for variant in pas["stages"].get(stage) or []:
        if "STEREO" in "".join(variant.get("keywords", [])):
            continue
        for buffer in variant.get("constant_buffers", []):
            if buffer["name"] == "$Globals":
                found |= {v["name"] for v in buffer["vectors"]}
    return found


def cutoff_for(pas: dict):
    """Which alpha cutoff this pass tests against, or None if it tests none.

    Read off the pass's own fragment program rather than inferred from its
    position.  Inference gets the common case right but not the interesting
    ones: the layer-0 passes of two-layer families are transparent, so counting
    opaque forward-base passes to find the second layer misses it and hands both
    of Custom/Hair/MainSeparateAlphaLayer3's layers _Cutoff, and MainAlternate's
    second layer comes out as _Cutoff1.  Reading the program has no such
    counterexample -- no pass in artifacts/shader-blobs loads more than one of
    the four names, and the keyword variants of a pass agree on which one.
    """
    cutoffs = sorted(stage_globals(pas, "progFragment") & set(CUTOFF_NAMES))
    if len(cutoffs) > 1:
        raise ValueError(f"pass {pass_tags(pas).get('LIGHTMODE') or '<untagged>'} "
                         f"reads more than one cutoff: {cutoffs}")
    return cutoffs[0] if cutoffs else None


UV_WINDOW_NAMES = ("_uvXMin", "_uvXMax", "_uvYMin", "_uvYMax")

# The maps this project transforms per map, i.e. the ones a uv window cannot
# serve: it hands every map the same uv.
PER_MAP_TRANSFORMED = ("_BumpMap", "_DecalTex")


def uv_window(props: list) -> bool:
    """Whether this shader samples every one of its maps through a uv window.

    Five of the hair families declare _uvXMin/_uvXMax/_uvYMin/_uvYMax and have
    their fragment program remap the single uv their vertex stage interpolated --
    which their vertex stage produced with _MainTex_ST and, unlike the other
    families, with no other map's transform.  The reconstruction applies that
    remap in the vertex stage instead (an affine remap and the interpolation
    commute), so it is only equivalent while the uv the original interpolates is
    _MainTex's and the shader has no second transformed map.
    """
    declared = {p["name"] for p in props}
    present = [n for n in UV_WINDOW_NAMES if n in declared]
    if not present:
        return False
    if len(present) != len(UV_WINDOW_NAMES):
        raise ValueError(f"declares {sorted(present)} and not all of "
                         f"{list(UV_WINDOW_NAMES)}; the window needs all four")
    extra = sorted(declared & set(PER_MAP_TRANSFORMED))
    if extra:
        raise ValueError(f"declares a uv window and {extra}; the window gives every "
                         "map the same uv and cannot transform others")
    return True


# Not every pass shades.  A mask pass's original fragment program samples
# _MainTex, keeps the texel's alpha, writes zero to every colour channel and
# reads nothing else, so the shared model has no input to work with and must not
# be used for it -- but the property list cannot tell such a pass from the
# diffuse IBL families, which are equally short of *properties*, and the render
# state cannot tell it from any other transparent pass.  What can tell it apart
# is what its own fragment program reads: a program that shades reads many
# constants, a mask reads at most the colour tint and the alpha adjustment.  The
# blend that hides the mask (its zeroes land, attenuated by the alpha) is why
# this has to be measured rather than guessed; see docs/verification.md.
MASK_LIGHTMODES = ("FORWARDBASE", "FORWARDADD")
MASK_ONLY_GLOBALS = {"_Color", "_AlphaAdjust"}

# One mask pass of one family samples _MainTex at a constant uv rather than at
# the interpolated one, which only its disassembly shows.  Read out of the
# shipped programs (see docs/verification.md); the value is the set of pass
# indices that do it.
MASK_CONSTANT_UV = {
    "Custom/Subsurface/AlphaMaskComputeBuff": {1},
}


def mask_only(pas: dict) -> bool:
    """Whether this pass's original fragment is the alpha-mask program."""
    if pass_lightmode(pas) not in MASK_LIGHTMODES:
        return False
    return stage_globals(pas, "progFragment") <= MASK_ONLY_GLOBALS


# The two blend factors that read the fragment's alpha: SrcAlpha and the
# saturating form, which is SrcAlpha capped at one.
SRC_ALPHA_BLENDS = (5, 9)


def add_keeps_alpha(pas: dict) -> bool:
    """Whether an additive pass fills the alpha slot with the surface's alpha.

    Most additive programs write a constant `1.0` there -- the whole
    `Blend One One` family, whose blend cannot see the value anyway.  The ones
    that blend with a SrcAlpha factor write the alpha the base pass writes,
    `saturate(texelA * _Color.a + _AlphaAdjust)`, because their blend reads it:
    `Cornea` carries `_Color.a = 0`, and writing `1.0` there lights the whole
    lens up in the material's own colour instead of leaving it untouched.  The
    blend state separates the two groups exactly -- every SrcAlpha-factored
    additive program ends in `mad_sat o0.w, ...` while the others end in
    `mov o0.w, l(1.000000)` (work/pass-alpha-all.txt lists all of them).
    """
    return int(pas["state"]["rtBlend0"]["srcBlend"]["val"]) in SRC_ALPHA_BLENDS


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


def emit_pass(pas: dict, lightmode: str, props: list, family: str,
              shader_name: str, pass_index: int) -> str:
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
        body.append("// bound without a tangent buffer (the hair and eye meshes)")
        body.append("#define VAM_NO_TANGENTS")
    cutoff = cutoff_for(pas)
    if cutoff:
        body.append("#define VAM_PASS_CUTOFF " + cutoff)
    # Two hair families thicken the strands before they are lit: the vertex stage
    # moves each vertex along its own normal, in object space and before the
    # object transform, which is why it is a vertex-stage term and not a
    # scale on the position.  Only the passes whose vertex program reads the
    # offset get it, and each of the two families binds it in three of its
    # passes -- forward, add and shadow of the layer it thickens -- and not in
    # the others (see docs/shader-reconstruction.md).
    if "_VertexNormalOffset" in stage_globals(pas, "progVertex"):
        if "_VertexNormalOffset" not in names:
            raise ValueError("the vertex program offsets along _VertexNormalOffset "
                             "but the shader declares no such property")
        body.append("#define VAM_PASS_VERTEX_OFFSET")
    if uv_window(props):
        # One uv, remapped by the window, is what every map samples -- so the
        # original's vertex stage must transform _MainTex and nothing else.
        vertex_st = sorted(n for n in stage_globals(pas, "progVertex") if n.endswith("_ST"))
        if vertex_st != ["_MainTex_ST"]:
            raise ValueError(f"pass {lightmode or '<untagged>'} lists a uv window but its "
                             f"vertex program transforms {vertex_st}; the reconstruction "
                             "windows _MainTex's uv and samples every map there")
        body.append("// every map samples one windowed uv (see VAM_UV_WINDOW)")
        body.append("#define VAM_UV_WINDOWED")
    if mask_only(pas):
        # The alpha a mask pass keeps is its own decision: most multiply the
        # texel by the colour tint and add _AlphaAdjust, the separately-alpha
        # ones keep the texel untouched.  Both are read from the program.
        body.append("// alpha mask only: no shading inputs (see MASK_ONLY_GLOBALS)")
        body.append("#define VAM_MASK_ONLY")
        if pass_index in MASK_CONSTANT_UV.get(shader_name, ()):
            body.append("#define VAM_MASK_CONSTANT_UV float2(1.0, 0.0)")
        frag_globals = stage_globals(pas, "progFragment")
        if "_Color" in frag_globals:
            body.append("#define VAM_MASK_COLOR")
        if "_AlphaAdjust" in frag_globals:
            body.append("#define VAM_MASK_ADJUST")
        frag = "VamFragmentMask"
    if frag == "VamFragmentAdd" and add_keeps_alpha(pas):
        # The blend reads the alpha (see add_keeps_alpha).
        body.append("#define VAM_ADD_ALPHA_SURFACE")
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


def classify(name: str, sub: dict, names: set):
    """Which vertex path a contract's shader needs, or None if it is pending.

    "skin"  -- reads the compute buffers, skinned by DAZSkinV2
    "mesh"  -- plain twin of one of those shaders, drawn the ordinary way
    None    -- left pending: another shading model, or no reconstruction here
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
    # A tessellated one is out of reach as well: the hull and domain pair this
    # project reconstructs reads the compute buffers, so only the skinned family
    # has a tessellation path here.  Those shaders keep the bundle's copy.
    for pas in sub["passes"]:
        if pass_lightmode(pas) in SKIP_LIGHTMODES:
            continue
        if pass_buffers(pas) & {"verts", "normals", "tangents"}:
            return None
        if pass_tessellates(pas):
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
        f"// Program contracts: {blob_dir}/",
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
        lines.append(emit_pass(pas, lm, props, family, name, pass_index))
        lines.append("")
        passes += 1
        if pass_tessellates(pas):
            tess_passes += 1
    lines.append("\t}")
    if contract.get("fallback"):
        lines.append(f'\tFallback "{contract["fallback"]}"')
    lines.append("}")
    return "\n".join(lines) + "\n", passes, tess_passes


def emit_project_shader_list(names) -> str:
    """The C# file that names every family this run reconstructed (see PROJECT_SHADER_LIST)."""
    out = [
        "// Generated by scripts\\New-VaMShaders.py -- do not edit.",
        "//",
        "// The families this project reconstructs from the shipped shader bytecode. A material the",
        "// game loads out of a bundle keeps the bundle's own copy of its shader even when this",
        "// project defines one of the same name, and VamShaderProvider.UseProjectShader moves it",
        "// across -- but only for a name in here. Shader.Find answers with the AssetRipper",
        "// placeholders in Assets/Shader just as happily as with a reconstruction, so the list is",
        "// what keeps a material that at least shades from being moved onto a stub that does not.",
        "",
        "using System;",
        "using System.Collections.Generic;",
        "",
        "namespace MeshVR",
        "{",
        "\tpublic static class VamProjectShaders",
        "\t{",
        "\t\tprivate static readonly string[] all =",
        "\t\t{",
    ]
    out += [f'\t\t\t"{name}",' for name in sorted(names)]
    out += [
        "\t\t};",
        "",
        "\t\tprivate static readonly HashSet<string> lookup =",
        "\t\t\tnew HashSet<string>(all, StringComparer.Ordinal);",
        "",
        "\t\tpublic static bool Defines(string shaderName)",
        "\t\t{",
        "\t\t\treturn shaderName != null && lookup.Contains(shaderName);",
        "\t\t}",
        "\t}",
        "}",
    ]
    return "\n".join(out) + "\n"


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
    seen = set()
    for source in CONTRACT_SOURCES:
        if not source.is_dir():
            continue
        for path in sorted(source.glob("*/contract.json")):
            data = json.loads(path.read_text(encoding="utf-8"))
            if data["name"] in seen:
                continue
            seen.add(data["name"])
            found.append((f"{source.relative_to(REPO).as_posix()}/{path.parent.name}", data))
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
    written = 0
    passes = 0
    tess_passes = 0
    written_names = []
    for blob_dir, data, family in contracts:
        text, n, t = emit_shader(data, blob_dir, skipped, family)
        target = SHADER_DIR / (data["name"].replace("/", "_") + ".shader")
        if not n:
            # Not one of this family's passes reads the compute buffers, so classify() guessed a
            # vertex path it does not have -- the two Custom/Debug*ComputeBuff families.  A file
            # here would shadow the shipped shader with an empty one, which is worse than being
            # pending, so the name is left pending and any file a previous run wrote is removed.
            print(f"warning: {data['name']} produced no passes, left pending", file=sys.stderr)
            pending.append(data["name"])
            if not args.list and target.exists():
                target.unlink()
            continue
        if args.list:
            written += 1
            print(f"{family:<5} {data['name']:<58} "
                  f"passes={n} props={len(data['properties']):>2}"
                  f"{f'  tess={t}' if t else ''}")
            continue
        target.write_text(text, encoding="utf-8")
        written += 1
        shaders += 1
        passes += n
        tess_passes += t
        written_names.append(data["name"])

    if args.list:
        print(f"\n{written} reconstructed, {len(pending)} pending (no shading model yet):")
        for name in pending:
            print(f"      {name}")
        print(f"\n{written} shaders, {len(skipped)} passes skipped")
        return 0

    SHADER_DIR.mkdir(parents=True, exist_ok=True)
    CGINC_DST.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(CGINC_SRC, CGINC_DST)
    PROJECT_SHADER_LIST.parent.mkdir(parents=True, exist_ok=True)
    PROJECT_SHADER_LIST.write_text(emit_project_shader_list(written_names), encoding="utf-8")
    print(f"wrote   {PROJECT_SHADER_LIST.relative_to(REPO)}")
    if PROJECT_SHADER_LIST_DST.parent.is_dir():
        shutil.copyfile(PROJECT_SHADER_LIST, PROJECT_SHADER_LIST_DST)
        print(f"copied  {PROJECT_SHADER_LIST.relative_to(REPO)} -> "
              f"{PROJECT_SHADER_LIST_DST.relative_to(REPO)}")
    else:
        print("note: the project is not assembled, so only src\\ was updated")

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
