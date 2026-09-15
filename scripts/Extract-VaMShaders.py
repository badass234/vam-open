#!/usr/bin/env python3
"""Extract compiled shader programs and their ShaderLab contract from a Unity player build.

A player build keeps no HLSL.  What it keeps is

  * the ShaderLab contract - properties, subshader tags, pass render state, the
    keyword table, and per-variant reflection (textures, constant buffers, and
    the *structured buffers* a variant reads), all in ``m_ParsedForm``;
  * the compiled GPU programs themselves - for a Direct3D11 build, DXBC
    containers - in the compressed program blob.

This script writes both, so a shader can be studied and rebuilt from what the
engine really ran instead of from a placeholder.

Blob layout (Unity 5.5 and up, ``compressedBlob``): every shader object holds,
per graphics platform, an LZ4-compressed buffer that begins with a program
count followed by one entry per program; each entry points at that program's
serialised header (version, program type, keyword list, code).  Entry order *is*
``m_BlobIndex`` order, which is what ties a program back to the pass, the stage
and the keyword set that select it.

Usage:
    python scripts/Extract-VaMShaders.py --out <dir> [--only <regex>] [files...]

Without explicit files it scans the named data files of the install's player-data
directory, resolved as ``%VAM_INSTALL%\VaM_Data`` or, when that variable is unset, as
``VaM_Data`` next to the repository - the usual layout, with the repository inside the
installation.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from pathlib import Path

import UnityPy
from UnityPy.enums import ShaderCompilerPlatform, ShaderGpuProgramType
from UnityPy.helpers import CompressionHelper
from UnityPy.streams import EndianBinaryReader

REPO = Path(__file__).resolve().parents[1]
DEFAULT_DATA_DIR = str(Path(os.environ.get("VAM_INSTALL") or REPO.parent) / "VaM_Data")
DEFAULT_DATA_FILES = ("globalgamemanagers.assets", "resources.assets", "sharedassets7.assets")

# Blob layout markers: the first field of a serialised program header is the
# Unity version that wrote it.
BLOB_VERSION_KEYWORDS = 201608170
BLOB_VERSION_LOCAL_KEYWORDS = 201806140
BLOB_VERSION_LOCAL_KEYWORDS_END = 202012090
BLOB_TABLE_ENTRY_12 = (2019, 3)

STAGES = ("progVertex", "progFragment", "progGeometry", "progHull", "progDomain")


def sanitize(name: str) -> str:
    return re.sub(r"[^A-Za-z0-9_.-]+", "_", name).strip("_") or "unnamed"


def enum_name(enum, value) -> str:
    try:
        return enum(value).name
    except ValueError:
        return f"Unknown{value}"


def entry_value(array, index):
    """UnityPy returns the per-platform tables either flat or nested."""
    item = array[index]
    return item[0] if isinstance(item, (list, tuple)) else item


class Program:
    """One serialised GPU program: the keyword set selecting it, and its DXBC.

    The header is laid out as version, program type, twelve bytes of GPU
    requirements, one further word, the keyword count, the keywords as aligned
    strings, then a length-prefixed code array.  The code array itself starts
    with a short GPU-specific prologue; the DXBC container follows it, and its
    own header carries the container's length.
    """

    def __init__(self, reader: EndianBinaryReader):
        self.version = reader.read_int()
        self.program_type = reader.read_int()
        reader.Position += 12
        if self.version >= BLOB_VERSION_KEYWORDS:
            reader.Position += 4
        keyword_count = reader.read_int()
        self.keywords = [reader.read_aligned_string() for _ in range(keyword_count)]
        if BLOB_VERSION_LOCAL_KEYWORDS <= self.version < BLOB_VERSION_LOCAL_KEYWORDS_END:
            local_count = reader.read_int()
            self.local_keywords = [reader.read_aligned_string() for _ in range(local_count)]
        else:
            self.local_keywords = []
        self.payload = bytes(reader.read_bytes(reader.read_int()))
        reader.align_stream()

        self.code = b""
        self.prologue = self.payload
        start = self.payload.find(b"DXBC")
        if start != -1 and start + 28 <= len(self.payload):
            size = int.from_bytes(self.payload[start + 24 : start + 28], "little")
            if start + size <= len(self.payload):
                self.prologue = self.payload[:start]
                self.code = self.payload[start : start + size]

    @property
    def is_dxbc(self) -> bool:
        return len(self.code) > 0


class PlatformPrograms:
    """Every program stored for one graphics platform."""

    def __init__(self, blob: bytes, version: tuple):
        reader = EndianBinaryReader(blob, endian="<")
        count = reader.read_int()
        entry_size = 12 if tuple(version) >= BLOB_TABLE_ENTRY_12 else 8
        self.programs: list[Program | None] = []
        for index in range(count):
            reader.Position = 4 + index * entry_size
            offset = reader.read_int()
            reader.Position = offset
            try:
                self.programs.append(Program(reader))
            except Exception:
                self.programs.append(None)


def platform_blob(typetree: dict, index: int) -> bytes:
    compressed = bytes(typetree.get("compressedBlob") or b"")
    offset = entry_value(typetree["offsets"], index)
    compressed_size = entry_value(typetree["compressedLengths"], index)
    decompressed_size = entry_value(typetree["decompressedLengths"], index)
    return CompressionHelper.decompress_lz4(
        compressed[offset : offset + compressed_size], decompressed_size
    )


def resolve_names(pass_data: dict) -> dict[int, str]:
    """The pass-level index -> name table that reflection entries refer to."""
    return {index: name for name, index in (pass_data.get("m_NameIndices") or [])}


def resolve(entry: dict, names: dict[int, str]) -> str:
    return names.get(entry.get("m_NameIndex", -1), f"<{entry.get('m_NameIndex')}>")


def describe_program(program: dict, names: dict[int, str]) -> dict:
    """Turn one reflection record into the contract a rebuild has to satisfy."""
    return {
        "blob_index": program.get("m_BlobIndex"),
        "gpu_program_type": enum_name(ShaderGpuProgramType, program.get("m_GpuProgramType")),
        "hardware_tier": program.get("m_ShaderHardwareTier"),
        "keywords": [names.get(k, f"<{k}>") for k in program.get("m_KeywordIndices") or []],
        "buffers": [
            {"name": resolve(b, names), "index": b.get("m_Index")}
            for b in program.get("m_BufferParams") or []
        ],
        "textures": [
            {
                "name": resolve(t, names),
                "index": t.get("m_Index"),
                "sampler": t.get("m_SamplerIndex"),
                "dim": t.get("m_Dim"),
            }
            for t in program.get("m_TextureParams") or []
        ],
        "constant_buffers": [
            {
                "name": resolve(cb, names),
                "matrices": [
                    {"name": resolve(m, names), "index": m.get("m_Index"), "rows": m.get("m_RowCount")}
                    for m in cb.get("m_MatrixParams") or []
                ],
                "vectors": [
                    {"name": resolve(v, names), "index": v.get("m_Index"), "size": v.get("m_ArraySize")}
                    for v in cb.get("m_VectorParams") or []
                ],
            }
            for cb in program.get("m_ConstantBuffers") or []
        ],
        "constant_buffer_bindings": [
            {"name": resolve(b, names), "index": b.get("m_Index")}
            for b in program.get("m_ConstantBufferBindings") or []
        ],
        "samplers": [
            {"name": resolve(s, names), "index": s.get("m_Index")}
            for s in program.get("m_Samplers") or []
        ],
        "shader_requirements": program.get("m_ShaderRequirements"),
    }


def describe_shader(parsed: dict) -> dict:
    subshaders = []
    for subshader in parsed.get("m_SubShaders") or []:
        passes = []
        for pass_data in subshader.get("m_Passes") or []:
            names = resolve_names(pass_data)
            stages = {}
            for stage in STAGES:
                programs = (pass_data.get(stage) or {}).get("m_SubPrograms") or []
                if programs:
                    stages[stage] = [describe_program(p, names) for p in programs]
            passes.append(
                {
                    "name": pass_data.get("m_Name") or "",
                    "tags": (pass_data.get("m_Tags") or {}).get("tags") or [],
                    "state": pass_data.get("m_State") or {},
                    "program_mask": pass_data.get("m_ProgramMask"),
                    "stages": stages,
                    "keyword_table": sorted(
                        ({"index": i, "name": n} for n, i in (pass_data.get("m_NameIndices") or [])),
                        key=lambda k: k["index"],
                    ),
                }
            )
        subshaders.append(
            {
                "lod": subshader.get("m_LOD"),
                "tags": (subshader.get("m_Tags") or {}).get("tags") or [],
                "passes": passes,
            }
        )
    return {
        "name": parsed.get("m_Name") or "",
        "fallback": parsed.get("m_FallbackName") or "",
        "custom_editor": parsed.get("m_CustomEditorName") or "",
        "dependencies": parsed.get("m_Dependencies") or [],
        "properties": [
            {
                "name": p.get("m_Name"),
                "description": p.get("m_Description"),
                "type": p.get("m_Type"),
                "flags": p.get("m_Flags"),
                "defaults": [p.get(f"m_DefValue[{i}]") for i in range(4)],
                "texture": p.get("m_DefTexture"),
                "attributes": p.get("m_Attributes"),
            }
            for p in (parsed.get("m_PropInfo") or {}).get("m_Props") or []
        ],
        "subshaders": subshaders,
    }


def extract_shader(obj, out_dir: str, source: str) -> dict:
    typetree = obj.read_typetree()
    parsed = typetree.get("m_ParsedForm") or {}
    name = parsed.get("m_Name") or ""
    script = typetree.get("m_Script") or ""
    shader_dir = os.path.join(out_dir, sanitize(name))
    os.makedirs(shader_dir, exist_ok=True)

    contract = describe_shader(parsed)
    contract.update(
        {
            "source": source,
            "path_id": obj.path_id,
            "object_version": str(obj.version),
            "script_chars": len(script),
            "script": script,
            "programs": [],
        }
    )

    platforms = list(typetree.get("platforms") or [])
    for platform_index in range(len(platforms)):
        if platform_index >= len(typetree.get("compressedLengths") or []):
            # m_Shader.platforms can be longer than the tables; stop where they end.
            break
        platform_name = enum_name(ShaderCompilerPlatform, platforms[platform_index])
        try:
            blob = platform_blob(typetree, platform_index)
            programs = PlatformPrograms(blob, obj.version).programs
        except Exception as error:
            contract["programs"].append({"platform": platform_name, "error": str(error)})
            continue

        for index, program in enumerate(programs):
            if program is None:
                contract["programs"].append(
                    {"platform": platform_name, "blob_index": index, "error": "unparsable"}
                )
                continue
            program_type = enum_name(ShaderGpuProgramType, program.program_type)
            stem = sanitize(f"{platform_name}_{index:03d}_{program_type}")
            if program.keywords:
                stem += "_" + sanitize("-".join(sorted(program.keywords)))[:48]
            entry = {
                "platform": platform_name,
                "blob_index": index,
                "program_type": program_type,
                "header_version": program.version,
                "header_keywords": sorted(program.keywords),
                "payload_size": len(program.payload),
                "prologue_size": len(program.prologue),
                "code_size": len(program.code),
                "is_dxbc": program.is_dxbc,
                "file": f"{sanitize(name)}/{stem}.dxbc",
            }
            with open(os.path.join(out_dir, entry["file"]), "wb") as handle:
                handle.write(program.code)
            contract["programs"].append(entry)

    with open(os.path.join(shader_dir, "contract.json"), "w", encoding="utf-8") as handle:
        json.dump(contract, handle, indent=1, ensure_ascii=False, default=str)
    return contract


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("files", nargs="*", help="Unity data files (default: the installation's)")
    parser.add_argument("--data-dir", default=DEFAULT_DATA_DIR,
                        help="player-data directory (default: %(default)s)")
    parser.add_argument("--out", required=True)
    parser.add_argument("--only", default=None, help="regex; extract only shaders whose name matches")
    args = parser.parse_args()

    files = args.files or [os.path.join(args.data_dir, name) for name in DEFAULT_DATA_FILES]
    only = re.compile(args.only) if args.only else None

    os.makedirs(args.out, exist_ok=True)
    manifest = []
    for path in files:
        if not os.path.exists(path):
            print(f"missing: {path}", file=sys.stderr)
            continue
        env = UnityPy.load(path)
        source = os.path.basename(path)
        found = 0
        for obj in env.objects:
            if obj.type.name != "Shader":
                continue
            try:
                typetree_name = (obj.read_typetree().get("m_ParsedForm") or {}).get("m_Name") or ""
            except Exception:
                typetree_name = ""
            if only and not only.search(typetree_name):
                continue
            try:
                contract = extract_shader(obj, args.out, source)
            except Exception as error:
                contract = {"name": typetree_name, "source": source, "path_id": obj.path_id, "error": str(error)}
                print(f"  ! {typetree_name}: {error}", file=sys.stderr)
            manifest.append(contract)
            found += 1
            programs = contract.get("programs", [])
            dxbc = sum(1 for p in programs if p.get("is_dxbc"))
            bad = sum(1 for p in programs if p.get("error"))
            print(f"  {contract['name']}: {len(programs)} programs, {dxbc} DXBC, {bad} failed")
        print(f"{source}: {found} shaders")

    with open(os.path.join(args.out, "manifest.json"), "w", encoding="utf-8") as handle:
        json.dump(manifest, handle, indent=1, ensure_ascii=False, default=str)

    total_dxbc = sum(1 for r in manifest for p in r.get("programs", []) if p.get("is_dxbc"))
    print(f"total: {len(manifest)} shaders, {total_dxbc} DXBC programs -> {args.out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
