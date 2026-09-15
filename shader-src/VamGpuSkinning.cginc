// =============================================================================
//  VaM GPU-skinning shader library                     (VAMOpen, stage 5)
// =============================================================================
//  Virt-a-Mate never lets Unity skin its characters.  DAZSkinV2 runs the
//  skinning as a compute shader and then draws the result with
//      Graphics.DrawMesh(mesh, identity, material, ...)
//  binding the compute output as structured buffers.  The vertices inside
//  those buffers are already in WORLD space and the character pose lives
//  entirely in them -- which is why the body has to be shaded by one of the
//  "*ComputeBuff" shaders: a stock shader has no way to read the buffers and
//  therefore draws the mesh in its bind pose (the T-pose seen in-game).
//
//  Bound by name from DAZSkinV2.DrawMeshGPU:
//      verts     StructuredBuffer<float3>  stride 12   world-space positions
//      normals   StructuredBuffer<float3>  stride 12   world-space normals
//      tangents  StructuredBuffer<float4>  stride 16   world-space tangents
//                                                      (body meshes only)
//
//  The same source also produces VaM's plain twins of these shaders -- the
//  ones *without* the "ComputeBuff" suffix, which a mesh drawn the ordinary
//  way has to use.  Those passes bind no structured buffers at all; the
//  generated shader defines VAM_MESH_SKIN and VaM's own vertex data supplies
//  the position, normal and tangent instead.  Everything from vam_v2f down is
//  shared: the two vertex programs pack their varyings identically and the
//  fragment programs are byte-identical (only the shader model differs).
//
//  VaM ships these shaders only as compiled DXBC, so this file is a
//  reconstruction from that bytecode (see scripts/Extract-VaMShaders.py and
//  docs/shader-reconstruction.md).  The lighting model follows the decompiled
//  body pixel shader; the generated .shader declares the per-material
//  uniforms and defines VAM_HAS_<property> for every one of them, so a shader
//  that declares fewer properties still compiles and behaves as if the
//  missing ones held their documented defaults.
//
//  The lighting is a 1:1 port of the original pixel shader, decoded register by
//  register from the shipped DXBC (docs/shader-reconstruction.md covers the
//  extraction and the disassembly the decode was read from).  The remaining
//  deviations from the bytecode are deliberate and listed here:
//    * lightmap indirection is not transcribed: the 3-D LUT lookup, the
//      probe-volume path and Unity's baked-occlusion dot product are replaced
//      by Unity's own lightmap/screen-space shadow macros, which the rebuilt
//      project drives instead;
//    * the per-vertex emissive term the original adds as `albedo * TEXCOORD6`
//      is omitted -- the vertex program hard-codes that interpolator to zero,
//      so it contributes nothing;
//    * the additive pass keeps a plain Lambert diffuse: its DXBC blob (a
//      separate program for point/spot lights) has not been transcribed yet.
//  Everything that decides the pose, the silhouette, the surface response and
//  the base shading -- the structured-buffer skinning, the normals, the
//  Fresnel/highlight/reflection curves, the SH ambient, the shadow coordinates
//  and the alpha cutoff -- is a direct transcription.
// =============================================================================

#ifndef VAM_GPU_SKINNING_INCLUDED
#define VAM_GPU_SKINNING_INCLUDED

#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
#include "AutoLight.cginc"

// AutoLight's SHADOW_COORDS is left undefined for the bare SHADOWS_DEPTH variant of
// multi_compile_fwdadd_fullshadows -- it covers DEPTH+SPOT, CUBE, SCREEN and "no shadows",
// but not DEPTH on its own -- and UNITY_SHADOW_COORDS forwards straight to it, so a shader
// that declares its shadow coordinate through the macro fails to compile for that variant
// ("unrecognized identifier 'SHADOW_COORDS'").  The variant never reads the coordinate
// back, but the declaration still has to exist, so it is supplied here.
#ifndef SHADOW_COORDS
#define SHADOW_COORDS(idx1) unityShadowCoord4 _ShadowCoord : TEXCOORD##idx1;
#endif

// -----------------------------------------------------------------------------
//  Skinning input: the compute buffers
//
//  Every generated shader defines VAM_NO_TANGENTS for the passes whose original
//  program did not bind a tangent buffer (all hair meshes, and the debug
//  shader) -- declaring an unbound SRV there would read garbage.  The plain
//  (VAM_MESH_SKIN) shaders bind nothing, so they declare nothing.
// -----------------------------------------------------------------------------
#ifndef VAM_MESH_SKIN
#ifndef VAM_NO_TANGENTS
StructuredBuffer<float3> verts    : register(t0);
StructuredBuffer<float3> normals  : register(t1);
StructuredBuffer<float4> tangents : register(t2);
#else
StructuredBuffer<float3> verts   : register(t0);
StructuredBuffer<float3> normals : register(t1);
#endif
#endif

// -----------------------------------------------------------------------------
//  Uniforms published by VaM's own sky/IBL code
//
//  Declared here as plain globals because that is what the original shaders
//  do -- none of them appear in a ShaderLab property list.  They are pushed
//  with Shader.SetGlobal* by Sky.ApplyGlobally() (src/mset/Sky.cs) and, for
//  skin materials, by Sky.ApplyToMaterial().  An unset matrix arrives as all
//  zeroes, which VamSkyDir() guards against.
// -----------------------------------------------------------------------------
float4x4 _SkyMatrix;        // world -> sky space (reflection / probe lookup)
float4   _ExposureIBL;      // .x SH+probe scale  .y reflection scale  .w IBL
float4   _ExposureLM;       // .x scale for Unity's built-in probe ambient
float    _BlendWeightIBL;

float4 _SH0;  float4 _SH1;  float4 _SH2;
float4 _SH3;  float4 _SH4;  float4 _SH5;
float4 _SH6;  float4 _SH7;  float4 _SH8;

// -----------------------------------------------------------------------------
//  Per-property fallbacks
//
//  Each block resolves to the real uniform when the generated shader declared
//  it, and to the documented default when it did not.  Range() properties
//  carry their default in the second value of the contract's defaults array;
//  the values below are copied from the union of all 51 shader definitions.
// -----------------------------------------------------------------------------
#ifdef VAM_HAS__Color
    #define VAM_Color _Color
#else
    #define VAM_Color float4(1,1,1,1)
#endif

#ifdef VAM_HAS__SpecColor
    #define VAM_SpecColor _SpecColor
#else
    #define VAM_SpecColor float4(1,1,1,1)
#endif

#ifdef VAM_HAS__SpecInt
    #define VAM_SpecInt _SpecInt
#else
    #define VAM_SpecInt 1.0
#endif

#ifdef VAM_HAS__Shininess
    #define VAM_Shininess _Shininess
#else
    #define VAM_Shininess 4.0
#endif

#ifdef VAM_HAS__Fresnel
    #define VAM_Fresnel _Fresnel
#else
    #define VAM_Fresnel 0.0
#endif

#ifdef VAM_HAS__DiffOffset
    #define VAM_DiffOffset _DiffOffset
#else
    #define VAM_DiffOffset 0.0
#endif

#ifdef VAM_HAS__SpecOffset
    #define VAM_SpecOffset _SpecOffset
#else
    #define VAM_SpecOffset 0.0
#endif

#ifdef VAM_HAS__GlossOffset
    #define VAM_GlossOffset _GlossOffset
#else
    #define VAM_GlossOffset 0.0
#endif

// Bumpiness: the decompiled shader multiplies the perturbed normal's z term
// by these, so a shader without _BumpMap must disable the perturbation
// entirely rather than fall back to a non-zero value.
#ifdef VAM_HAS__DiffuseBumpiness
    #define VAM_DiffuseBumpiness _DiffuseBumpiness
#else
    #define VAM_DiffuseBumpiness 0.0
#endif

#ifdef VAM_HAS__SpecularBumpiness
    #define VAM_SpecularBumpiness _SpecularBumpiness
#else
    #define VAM_SpecularBumpiness 0.0
#endif

#ifdef VAM_HAS__SubdermisColor
    #define VAM_SubdermisColor _SubdermisColor
#else
    #define VAM_SubdermisColor float4(0,0,0,0)
#endif

// _IBLFilter scales the entire indirect term (VaM's "IBL filter" knob).  Zero
// is the neutral value, so a shader that does not declare it keeps the
// unfiltered look rather than losing its ambient.
#ifdef VAM_HAS__IBLFilter
    #define VAM_IBLFilter _IBLFilter
#else
    #define VAM_IBLFilter 0.0
#endif

#ifdef VAM_HAS__AlphaAdjust
    #define VAM_AlphaAdjust _AlphaAdjust
#else
    #define VAM_AlphaAdjust 0.0
#endif

#ifdef VAM_HAS__Cutoff
    #define VAM_Cutoff _Cutoff
#else
    #define VAM_Cutoff 0.001
#endif

// Diffuse texture: the only property present in every *ComputeBuff shader
// except Custom/DebugNormalsComputeBuff, which has no properties at all.
// Each texture also provides its UV accessor, because a shader that does not
// declare the property has no <name>_ST uniform for TRANSFORM_TEX to use.
#ifdef VAM_HAS__MainTex
    #define VAM_SAMPLE_DIFFUSE(uv) tex2D(_MainTex, uv)
    #define VAM_UV_MAIN(uv) TRANSFORM_TEX(uv, _MainTex)
#else
    #define VAM_SAMPLE_DIFFUSE(uv) float4(1,1,1,1)
    #define VAM_UV_MAIN(uv) uv
#endif

#ifdef VAM_HAS__SpecTex
    #define VAM_SAMPLE_SPEC(uv) tex2D(_SpecTex, uv).rgb
    #define VAM_UV_SPEC(uv) TRANSFORM_TEX(uv, _SpecTex)
#else
    #define VAM_SAMPLE_SPEC(uv) float3(1,1,1)
    #define VAM_UV_SPEC(uv) uv
#endif

#ifdef VAM_HAS__GlossTex
    #define VAM_SAMPLE_GLOSS(uv) tex2D(_GlossTex, uv).r
    #define VAM_UV_GLOSS(uv) TRANSFORM_TEX(uv, _GlossTex)
#else
    #define VAM_SAMPLE_GLOSS(uv) 1.0
    #define VAM_UV_GLOSS(uv) uv
#endif

#ifdef VAM_HAS__BumpMap
    #define VAM_SAMPLE_BUMP(uv) tex2D(_BumpMap, uv)
    #define VAM_UV_BUMP(uv) TRANSFORM_TEX(uv, _BumpMap)
#else
    #define VAM_SAMPLE_BUMP(uv) float4(0.5,0.5,1,1)
    #define VAM_UV_BUMP(uv) uv
#endif

#ifdef VAM_HAS__DecalTex
    #define VAM_SAMPLE_DECAL(uv) tex2D(_DecalTex, uv)
    #define VAM_UV_DECAL(uv) TRANSFORM_TEX(uv, _DecalTex)
#else
    #define VAM_SAMPLE_DECAL(uv) float4(0,0,0,0)
    #define VAM_UV_DECAL(uv) uv
#endif

#ifdef VAM_HAS__SpecCubeIBL
    #define VAM_SAMPLE_IBL(dir, mip) texCUBElod(_SpecCubeIBL, float4(dir, mip))
#else
    #define VAM_SAMPLE_IBL(dir, mip) float4(0,0,0,1)
#endif

// -----------------------------------------------------------------------------
//  Vertex stage
//
//  The DXBC input signature is POSITION0 (w carries a per-vertex object-space
//  offset weight), TANGENT0, NORMAL0, TEXCOORD0, SV_VertexID.  In the
//  compute-buffer path the vertex data in the mesh is *only* used for those
//  extras and the skinning comes from the buffers; in the plain path it is the
//  vertex data itself that is transformed.
// -----------------------------------------------------------------------------
struct vam_appdata {
    float4 vertex  : POSITION;
    float3 normal  : NORMAL;
    float4 tangent : TANGENT;
    float4 uv0     : TEXCOORD0;
    uint   vid     : SV_VertexID;
};

struct vam_v2f {
    float4 pos     : SV_POSITION;
    float2 uvMain  : TEXCOORD0;
    float2 uvSpec  : TEXCOORD1;
    float2 uvGloss : TEXCOORD2;
    float2 uvBump  : TEXCOORD3;
    float2 uvDecal : TEXCOORD4;
    float3 posWS   : TEXCOORD5;
    float3 nrmWS   : TEXCOORD6;
    float3 tanWS   : TEXCOORD7;
    float3 bitWS   : TEXCOORD8;
    UNITY_SHADOW_COORDS(9)
};

struct vam_skinned {
    float3 pos;
    float3 nrm;
    float3 tan;
    float  tanSign;
};

// Rotate a direction into VaM's sky space.  _SkyMatrix is a global that is
// only valid while a Sky atom is applying itself; when it has never been set
// Unity hands us an all-zero matrix, and normalising the product would poison
// the shading with NaNs.  Falling back to the untransformed direction keeps
// reflections sane in that case.
float3 VamSkyDir(float3 dir) {
    float3 r = mul((float3x3)_SkyMatrix, dir);
    return dot(r, r) > 1e-8 ? normalize(r) : dir;
}

// VaM's own ambient, carried in _SH0.._SH8 as nine packed coefficients.  The
// light-probe *storage* convention leaves their semantic order unpublished, so
// this is not an interpretation: it is the basis the original pixel shader
// evaluates, read back from its bytecode.  `n` is the normal in sky space --
// the same frame the reflection cube is sampled in -- which is why the caller
// rotates it with _SkyMatrix first.
float3 VamSkySH1(float3 n) {
    return _SH1.rgb * n.y + _SH2.rgb * n.z + _SH3.rgb * n.x;
}

float3 VamSkySH2(float3 n) {
    return _SH4.rgb * (n.x * n.y)
         + _SH5.rgb * (n.z * n.y)
         + _SH7.rgb * (n.x * n.z)
         + _SH6.rgb * (3.0 * n.z * n.z - 1.0)
         + _SH8.rgb * (n.x * n.x - n.y * n.y);
}

vam_skinned VamSkin(vam_appdata v) {
    vam_skinned s;
    float3 nrmWS, tanWS;
    float  tanSign = 1.0;

#ifdef VAM_MESH_SKIN
    // Plain path.  The original computes the clip position from the plain
    // object-to-world transform and the pixel-facing world position with the
    // translation column scaled by POSITION.w -- the same per-vertex offset
    // term the compute-buffer path adds to the buffer's world position.  For
    // the w=1 meshes Unity actually feeds, both are the same point.
    s.pos = mul(unity_ObjectToWorld, v.vertex).xyz;
    nrmWS = UnityObjectToWorldNormal(v.normal);
    tanWS = UnityObjectToWorldDir(v.tangent.xyz);
    tanSign = v.tangent.w * unity_WorldTransformParams.w;
#else
    // World-space position straight out of the compute buffer, plus the
    // per-vertex object-space offset the mesh's POSITION.w asks for.  The
    // original adds it scaled by the translation column of the object matrix.
    float3 posOS = verts[v.vid] + unity_ObjectToWorld._m03_m13_m23 * v.vertex.w;
    float4 posWS = mul(unity_ObjectToWorld, float4(posOS, 1.0));
    s.pos = posWS.xyz;

    nrmWS = UnityObjectToWorldNormal(normals[v.vid]);

#ifndef VAM_NO_TANGENTS
    float4 tanIn = tangents[v.vid];
    tanWS = UnityObjectToWorldDir(tanIn.xyz);
    tanSign = tanIn.w * unity_WorldTransformParams.w;
#else
    // Hair meshes are drawn with only the position and normal buffers bound.
    // The tangent is only needed for the bump perturbation, which a shader
    // without _BumpMap disables anyway, so any unit vector will do.
    tanWS = float3(1, 0, 0);
#endif
#endif

    // Gram-Schmidt: in the compute-buffer path the buffers are skinned
    // independently, so the tangent is not guaranteed to stay orthogonal to
    // the normal.
    tanWS = normalize(tanWS - nrmWS * dot(nrmWS, tanWS));

    s.nrm = nrmWS;
    s.tan = tanWS;
    s.tanSign = tanSign;
    return s;
}

vam_v2f VamVertex(vam_appdata v) {
    vam_v2f o = (vam_v2f)0;
    vam_skinned s = VamSkin(v);

    o.pos     = mul(UNITY_MATRIX_VP, float4(s.pos, 1.0));
    o.posWS   = s.pos;
    o.nrmWS   = s.nrm;
    o.tanWS   = s.tan;
    o.bitWS   = cross(s.nrm, s.tan) * s.tanSign;

    float2 uv = v.uv0.xy;
    o.uvMain  = VAM_UV_MAIN(uv);
    o.uvSpec  = VAM_UV_SPEC(uv);
    o.uvGloss = VAM_UV_GLOSS(uv);
    o.uvBump  = VAM_UV_BUMP(uv);
    o.uvDecal = VAM_UV_DECAL(uv);

    // Light coordinates are not transferred: AutoLight's 5.6+ helpers derive
    // them in the fragment shader from the world position, which is exactly
    // what the compute buffers give us.  This keeps the macros honest where
    // TRANSFER_SHADOW would have used the unskinned v.vertex.
    UNITY_TRANSFER_SHADOW(o, float2(0, 0));

    return o;
}

// -----------------------------------------------------------------------------
//  Pixel stage
//
//  Transcribed from the decompiled body pixel shader: albedo with the detail
// layer composited in, two independently perturbed normals (the specular one
// drives the Fresnel, the reflection and the ambient, the diffuse one drives the
// direct light), a bounded Fresnel, one gloss value that drives the reflection
// mip and the highlight exponent, a two-source SH ambient (VaM's own sky
// coefficients plus Unity's probe), a subdermis-wrapped diffuse and, on top, the
// IBL filter and exposure that VaM's sky controller publishes.
// -----------------------------------------------------------------------------
struct vam_surface {
    float3 albedo;
    float3 nrmDiff;
    float3 nrmSpec;
    float3 spec;
    float  gloss;
    float  alpha;
};

// The original evaluates the Fresnel against the *tangent-space* specular normal
// but dots it with a *world-space* vector built from the TBN rows
// (T*V.x + N*V.y + B*V.z).  The two rotations therefore do not cancel: the
// expression works out to nrmSpec . (R . R . V_toCamera), which is exact for a
// character aligned with the world axes and slightly asymmetric otherwise.  It
// is reproduced here rather than "fixed", because it is what the shipped shader
// does.
float VamFresnelNormal(vam_v2f i, float3 nrmSpec, float3 toCamera) {
    float3 rows = i.tanWS * toCamera.x + i.nrmWS * toCamera.y + i.bitWS * toCamera.z;
    rows = normalize(rows);
    return saturate(dot(nrmSpec, i.tanWS * rows.x + i.nrmWS * rows.y + i.bitWS * rows.z));
}

float VamFresnelTerm(float ndv) {
    // A quadratic curve in _Fresnel through 1 -> f0 -> f0^3, then the highlight
    // is stopped from blowing out at grazing angles by bounding _SpecInt
    // through a square root (0.922444 is a literal from the original).
    float f0 = 1.0 - saturate(ndv);
    float fr = saturate(VAM_Fresnel);
    float fres = lerp(lerp(1.0, f0, fr), lerp(f0, f0 * f0 * f0, fr), fr);
    fres = fres * 0.95 + 0.05;
    float bound = sqrt(saturate(fres * VAM_SpecInt));
    return (fres * VAM_SpecInt - bound) * 0.922444 + bound;
}

vam_surface VamSurface(vam_v2f i) {
    vam_surface s;

    float4 diffuseTex = VAM_SAMPLE_DIFFUSE(i.uvMain);
    float3 albedo = saturate(diffuseTex.rgb + VAM_DiffOffset) * VAM_Color.rgb;

    // The detail layer is composited with its own alpha, premultiplied:
    // albedo*(1-a) + detail*a.
    float4 decal = VAM_SAMPLE_DECAL(i.uvDecal);
    albedo = lerp(albedo, decal.rgb, decal.a);

    // The bump map's alpha masks the X slope only -- a quirk of VaM's original
    // shader -- so only that component is scaled by it.
    float4 bumpTex = VAM_SAMPLE_BUMP(i.uvBump);
    float2 bumpXY = float2(bumpTex.x * bumpTex.a, bumpTex.y) * 2.0 - 1.0;
    float3 nTS = float3(bumpXY, sqrt(saturate(1.0 - dot(bumpXY, bumpXY))));
    // Bumpiness interpolates the tangent-space normal away from flat: 0 leaves
    // the surface unperturbed, 1 applies the map in full.
    float3 nFlat = float3(0.0, 0.0, 1.0);
    float3 n0 = nTS - nFlat;
    float3 nDiffTS = normalize(nFlat + VAM_DiffuseBumpiness * n0);
    float3 nSpecTS = normalize(nFlat + VAM_SpecularBumpiness * n0);

    float3x3 tbn = float3x3(i.tanWS, i.bitWS, i.nrmWS);
    s.nrmDiff = normalize(mul(nDiffTS, tbn));
    s.nrmSpec = normalize(mul(nSpecTS, tbn));

    s.albedo = albedo;
    s.spec   = saturate(VAM_SAMPLE_SPEC(i.uvSpec) + VAM_SpecOffset) * VAM_SpecColor.rgb;
    s.gloss  = saturate(VAM_SAMPLE_GLOSS(i.uvGloss) + VAM_GlossOffset);

    float alpha = VAM_Color.a * diffuseTex.a;
#ifdef VAM_HAS__AlphaTex
    // _AlphaAdjust is a signed offset that shifts the whole alpha mask, so it
    // is added rather than multiplied.
    alpha += tex2D(_AlphaTex, i.uvMain).r + VAM_AlphaAdjust;
#endif
    s.alpha = saturate(alpha);
    return s;
}

// The response to one light: a subdermis-wrapped diffuse plus a Blinn-Phong
// highlight that shares the gloss exponent.  The wrap bleeds the terminator
// around the body (that is what makes VaM skin look translucent) and the
// (0.5 + 0.5 * N.L)^2 lobe keeps the lit side from flattening out; the highlight
// is faded out at the terminator so grazing light cannot produce one.
float3 VamLightResponse(vam_surface s, float3 V, float3 L, float3 subdermis,
                        float fres, float exponent, float specScale) {
    // The light dirs are the *diffuse* normal's here, unlike the indirect terms.
    float ndl = saturate(dot(s.nrmDiff, L));

    float3 wrap = subdermis * 0.5;
    float3 inv = 1.0 - wrap;
    float3 diffuse = 2.0 * (0.5 + 0.5 * ndl) * (0.5 + 0.5 * ndl)
                   * inv * saturate(ndl * inv + wrap) * s.albedo;

    // V points from the camera to the surface, so the half vector is L - V.
    float ndh = saturate(dot(s.nrmSpec, normalize(L - V)));
    float3 highlight = pow(ndh, exponent) * min(ndl * 10.0, 1.0) * 0.5
                     * s.spec * fres * specScale;

    return diffuse + highlight;
}

// Everything gloss drives.  A single value, 2g - g^2, is what VaM's material
// really curves: it picks the reflection mip, and it grows the Blinn-Phong
// exponent *exponentially* with _Shininess -- which is what lets VaM's 0..10
// range produce specular that tight.  specScale normalises the highlight so its
// energy tracks the exponent instead of collapsing as the lobe narrows.
void VamGlossTerms(float gloss, out float mip, out float exponent,
                   out float specScale) {
    float gg = gloss * (2.0 - gloss);
    mip = 8.0 - gg - VAM_Shininess * gg;
    exponent = exp(gg * (1.0 + VAM_Shininess));
    specScale = exponent * 0.159155 + 0.318310;
}

float3 VamShade(vam_v2f i, vam_surface s, float3 V) {
    // ---- Fresnel -------------------------------------------------------------
    float fres = VamFresnelTerm(VamFresnelNormal(i, s.nrmSpec, -V));

    // ---- Gloss ---------------------------------------------------------------
    float mip, exponent, specScale;
    VamGlossTerms(s.gloss, mip, exponent, specScale);

    float3 specTerm = s.spec * fres;

    // ---- Image-based reflection ---------------------------------------------
    // The cube's alpha carries a mip weight that keeps the reflection from
    // bubbling up in the most polished areas; the original fits a cubic to it.
    float3 R = VamSkyDir(reflect(V, s.nrmSpec));
    float4 ibl = VAM_SAMPLE_IBL(R, max(mip, 0.0));
    float polish = dot(float3(0.465336, 25.012255, 49.174381),
                       float3(ibl.w, ibl.w * ibl.w, ibl.w * ibl.w * ibl.w));
    float3 reflection = ibl.rgb * polish * specTerm * specScale * _ExposureIBL.y;

    // ---- Ambient -------------------------------------------------------------
    // Two sources: VaM's own sky SH, evaluated in sky space, and Unity's
    // light-probe SH, evaluated in world space.  Each carries its own exposure.
    float3 nSky = VamSkyDir(s.nrmSpec);
    float4 probe = float4(s.nrmSpec, 0.0);
    float3 acc = _SH0.rgb * _ExposureIBL.x
               + float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) * _ExposureLM.x;
    float3 shL1 = VamSkySH1(nSky) * _ExposureIBL.x
                + float3(dot(unity_SHAr.xyz, s.nrmSpec),
                         dot(unity_SHAg.xyz, s.nrmSpec),
                         dot(unity_SHAb.xyz, s.nrmSpec)) * _ExposureLM.x;
    float3 shL2 = VamSkySH2(nSky) * _ExposureIBL.x
                + SHEvalLinearL2(probe) * _ExposureLM.x;

    // _SubdermisColor warps the two SH bands rather than tinting the result,
    // which is what makes VaM skin glow red in the shaded areas.  The pair of
    // curves is evaluated per channel against the skin's sub-surface colour.
    float3 sub = VAM_SubdermisColor.rgb;
    float3 c13 = 1.0 - sub * 0.333300;
    float3 c34 = 1.0 - sub * 0.750000;
    float3 k1 = sub * (c13 * c13 - c13) + c13;
    float3 k2 = sub * (c34 * c34 - c34) + c34;
    float3 ambient = abs(shL2 * k2 + shL1 * k1 + acc);

    // ---- Main directional light ---------------------------------------------
    // Unity's 5.6+ helper derives the light and shadow coordinates from the
    // world position, which is what the compute buffers hand us.
    UNITY_LIGHT_ATTENUATION(atten, i, i.posWS);
    float3 direct = VamLightResponse(s, V, normalize(_WorldSpaceLightPos0.xyz),
                                     sub, fres, exponent, specScale)
                  * atten * _LightColor0.rgb;

    // ---- Composite -----------------------------------------------------------
    // _IBLFilter scales the whole indirect term, including how much albedo
    // feeds into it, and _ExposureIBL.w is a master exposure over the result.
    float indirect = 1.0 - VAM_IBLFilter;
    float3 iblPass = reflection * indirect + ambient * (indirect * s.albedo);
    return (iblPass + direct) * _ExposureIBL.w;
}

fixed4 VamFragment(vam_v2f i) : SV_Target {
    vam_surface s = VamSurface(i);
    float3 V = normalize(i.posWS - _WorldSpaceCameraPos);
    float3 col = VamShade(i, s, V);

#ifdef VAM_PASS_CUTOFF
    clip(s.alpha - VAM_PASS_CUTOFF);
#endif

    return fixed4(col, s.alpha);
}

// Additive pass for every light after the main one.  Same surface and the same
// per-light response as the base pass -- only the light's own colour and
// attenuation come from the current Unity light.  The indirect terms are skipped
// so the ambient is not added once per light.  (The original's additive blob is
// a separate DXBC program that has not been transcribed yet, so its response is
// derived from the base pass rather than being a transcription.)
fixed4 VamFragmentAdd(vam_v2f i) : SV_Target {
    vam_surface s = VamSurface(i);
    float3 V = normalize(i.posWS - _WorldSpaceCameraPos);

    float fres = VamFresnelTerm(VamFresnelNormal(i, s.nrmSpec, -V));
    float mip, exponent, specScale;
    VamGlossTerms(s.gloss, mip, exponent, specScale);

    UNITY_LIGHT_ATTENUATION(atten, i, i.posWS);
    float3 L = normalize(_WorldSpaceLightPos0.xyz);

    float3 col = VamLightResponse(s, V, L, VAM_SubdermisColor.rgb, fres, exponent,
                                  specScale);
    col = col * _LightColor0.rgb * atten * _ExposureIBL.w;
    return fixed4(col, s.alpha);
}

// -----------------------------------------------------------------------------
//  Shadow caster
//
//  TRANSFER_SHADOW_CASTER_NOPOS would use the unskinned v.vertex, so the shadow
//  position is built here from the skinned position instead.  VamSkin already
//  returns world space, and UnityClipSpaceShadowCasterPos(vertex, normal) would
//  push that through unity_ObjectToWorld a second time, so its normal-offset
//  half is reproduced below rather than called.
// -----------------------------------------------------------------------------
struct vam_shadow_v2f {
    V2F_SHADOW_CASTER_NOPOS
    UNITY_POSITION(pos);
};

vam_shadow_v2f VamShadowVertex(vam_appdata v) {
    vam_shadow_v2f o = (vam_shadow_v2f)0;
    vam_skinned s = VamSkin(v);

    float4 wPos = float4(s.pos, 1.0);
    if (unity_LightShadowBias.z != 0.0) {
        float3 wLight = normalize(UnityWorldSpaceLightDir(wPos.xyz));
        float shadowCos = dot(s.nrm, wLight);
        float shadowSine = sqrt(1.0 - shadowCos * shadowCos);
        wPos.xyz -= s.nrm * (unity_LightShadowBias.z * shadowSine);
    }

    // Only the legacy cubemap path carries the light-space vector in the varying;
    // where SHADOWS_CUBE_IN_DEPTH_TEX is available Unity writes depth directly and
    // V2F_SHADOW_CASTER_NOPOS has no vec member to fill.
#if defined(SHADOWS_CUBE) && !defined(SHADOWS_CUBE_IN_DEPTH_TEX)
    o.vec = wPos.xyz - _LightPositionRange.xyz;
    o.pos = mul(UNITY_MATRIX_VP, wPos);
#else
    o.pos = UnityApplyLinearShadowBias(mul(UNITY_MATRIX_VP, wPos));
#endif
    return o;
}

float4 VamShadowFragment(vam_shadow_v2f i) : SV_Target {
    SHADOW_CASTER_FRAGMENT(i)
}

#endif // VAM_GPU_SKINNING_INCLUDED
