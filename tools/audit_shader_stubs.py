#!/usr/bin/env python3
"""Inventory the shader names the project still answers with an AssetRipper placeholder.

AssetRipper cannot see the DXBC a shipped Unity game compiles its shaders from, so for every
family it cannot reconstruct it writes a placeholder that compiles, reports itself supported and
draws far less than the original.  scripts/New-VaMShaders.py overwrites that placeholder for each
family it can reconstruct; everything else is still a stub.

The inventory is per *name*, not per file, because one name can be defined twice -- AssetRipper
wrote a placeholder wherever it found the shader asset, and the reconstructing generator writes to
Assets/Shader, so a name can hold a placeholder in Resources and a reconstruction in Shader at the
same time.  Unity compiles both and Shader.Find resolves one of them arbitrarily, which is a defect
in its own right, so the script reports the multiplicity as well.

A placeholder is only a hazard where a name is *resolved*: `Shader.Find` answers a placeholder as
readily as it answers a reconstruction, so where a name is looked up rather than reached by GUID, a
placeholder is a wrong shader where a missing file is no shader at all - and the reconstruction the
generator wrote for that name cannot be seen by a caller that looks the *shipped* name up.

"Nothing points at" therefore has to mean nothing at all, and that took three readings to get right:
an asset reference (a GUID), a `Fallback "name"` inside another shader, and a string literal in the
decompiled source (a `Shader.Find` caller). A name that only the first reading misses is still a name
the project uses. The project holds 71 placeholders: 25 by the first reading, 30 by the second or
third (16 `Fallback` declarations over 6 `Marmoset` names, 24 `Shader.Find` names - the post FX stack,
`NGSS_Directional`, `Custom/Discard`, the `Oculus/*` ones), and 16 by none. The 45 that nothing
referenced *by GUID* were once deleted on the strength of the first reading alone, and 30 of those 45
were in the second or third group; they are back, and this script is what says so.

Usage:
    python tools/audit_shader_stubs.py --project VaM_Rebuild
    python tools/audit_shader_stubs.py --project VaM_Rebuild --references 0
"""

from __future__ import annotations

import argparse
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from check_asset_guids import collect_referenced  # noqa: E402

REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

MARKER = "//DummyShaderTextExporter"
NAME_RE = re.compile(r'^\s*Shader\s+"([^"]+)"', re.MULTILINE)
FALLBACK_RE = re.compile(r'^\s*Fallback\s+"([^"]+)"', re.MULTILINE)
GUID_RE = re.compile(rb"^guid:\s*([0-9a-fA-F]{32})\s*$", re.MULTILINE)


def project_shader_names(repo: str) -> set[str]:
    """The names the generator wrote, as carried by VamProjectShaders.cs."""
    path = os.path.join(repo, "VaM_Rebuild", "Assets", "Scripts", "Assembly-CSharp",
                        "MeshVR", "VamProjectShaders.cs")
    if not os.path.isfile(path):
        return set()
    with open(path, "r", encoding="utf-8") as handle:
        return {m for m in re.findall(r'"([^"]+)"', handle.read()) if "/" in m}


def shader_guid(path: str) -> str | None:
    meta = path + ".meta"
    if not os.path.isfile(meta):
        return None
    with open(meta, "rb") as handle:
        match = GUID_RE.search(handle.read(4096))
    return match.group(1).decode("ascii").lower() if match else None


def inventory(assets: str) -> dict[str, list[tuple[str, bool]]]:
    """name -> [(path, is_placeholder)], for every .shader in the project."""
    names: dict[str, list[tuple[str, bool]]] = {}
    for root, _dirs, files in os.walk(assets):
        for file_name in files:
            if not file_name.endswith(".shader"):
                continue
            path = os.path.join(root, file_name)
            try:
                with open(path, "r", encoding="utf-8", errors="replace") as handle:
                    text = handle.read()
            except OSError:
                continue
            match = NAME_RE.search(text)
            if not match:
                continue
            names.setdefault(match.group(1), []).append((path, MARKER in text))
    return names


def fallback_names(assets: str) -> dict[str, set[str]]:
    """name -> the project shaders that name it in a Fallback declaration."""
    named: dict[str, set[str]] = {}
    for root, _dirs, files in os.walk(assets):
        for file_name in files:
            if not file_name.endswith(".shader"):
                continue
            path = os.path.join(root, file_name)
            try:
                with open(path, "r", encoding="utf-8", errors="replace") as handle:
                    text = handle.read()
            except OSError:
                continue
            for name in FALLBACK_RE.findall(text):
                named.setdefault(name, set()).add(path)
    return named


def source_names(root: str, candidates: set[str]) -> dict[str, set[str]]:
    """name -> the .cs files that carry it as a string literal (a Shader.Find caller)."""
    named: dict[str, set[str]] = {name: set() for name in candidates}
    if not os.path.isdir(root):
        return named
    quoted = {name: f'"{name}"' for name in candidates}
    for dirpath, _dirs, files in os.walk(root):
        for file_name in files:
            if not file_name.endswith(".cs"):
                continue
            path = os.path.join(dirpath, file_name)
            try:
                with open(path, "r", encoding="utf-8", errors="replace") as handle:
                    text = handle.read()
            except OSError:
                continue
            for name, needle in quoted.items():
                if needle in text:
                    named[name].add(path)
    return named


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project", default="VaM_Rebuild", help="Unity project directory")
    parser.add_argument("--repo", default=REPO, help="repository root")
    parser.add_argument("--references", type=int, default=2,
                        help="how many referencing assets to name per stub (0 for none)")
    args = parser.parse_args()

    project = os.path.join(args.repo, args.project)
    assets = os.path.join(project, "Assets")
    if not os.path.isdir(assets):
        print(f"Not a Unity project (no Assets dir): {args.project}", file=sys.stderr)
        return 2

    names = inventory(assets)
    rebuilt = project_shader_names(args.repo)
    _referenced, sources = collect_referenced(assets)

    placeholder_only = {n for n, files in names.items() if all(p for _path, p in files)}
    duplicated = {n: files for n, files in names.items() if len(files) > 1}

    print(f"{len(names)} shader names, {len(placeholder_only)} of them placeholder-only, "
          f"{len(rebuilt)} names reconstructed by the generator")
    if duplicated:
        print(f"{len(duplicated)} names are defined more than once (a placeholder beside a "
              f"reconstruction, or AssetRipper exporting one asset twice):")
        for name in sorted(duplicated):
            files = duplicated[name]
            kinds = ", ".join(("placeholder" if p else "reconstruction") for _path, p in files)
            print(f"  {name:<55} {len(files)}x  {kinds}")
    print()

    fallbacks = fallback_names(assets)
    code = source_names(os.path.join(args.repo, "src", "Assembly-CSharp"), placeholder_only)

    dangling = []
    for name in sorted(placeholder_only):
        files = names[name]
        where: set[str] = set()
        for path, _p in files:
            guid = shader_guid(path)
            if guid:
                where |= {w for w in sources.get(guid, ()) if not w.endswith(".meta")}
        dangling.append((name, files, sorted(where)))

    referenced = [item for item in dangling if item[2]]
    print(f"PLACEHOLDER-ONLY NAMES REFERENCED BY AN ASSET ({len(referenced)}):")
    for name, files, where in referenced:
        print(f"  {name:<55} {len(where)} reference(s)")
        for path, _p in files:
            print(f"        defined by {os.path.relpath(path, project)}")
        if args.references:
            for w in where[: args.references]:
                print(f"        <- {os.path.relpath(w, project)}")
            if len(where) > args.references:
                print(f"        ... and {len(where) - args.references} more")
    print()

    by_name = [item for item in dangling
               if not item[2] and (fallbacks.get(item[0]) or code[item[0]])]
    print(f"PLACEHOLDER-ONLY NAMES ONLY REACHED BY NAME ({len(by_name)}):")
    for name, _files, _where in by_name:
        reasons = []
        if fallbacks.get(name):
            reasons.append(f"{len(fallbacks[name])} Fallback declaration(s)")
        if code[name]:
            reasons.append(f"{len(code[name])} string literal(s) in the decompiled source")
        print(f"  {name:<55} {', '.join(reasons)}")
        if args.references:
            for path in sorted(fallbacks.get(name, ())):
                print(f"        Fallback in {os.path.relpath(path, project)}")
            for path in sorted(code[name])[: args.references]:
                print(f"        the name as a string in {os.path.relpath(path, args.repo)}")
    print()

    unreached = [item for item in dangling
                 if not item[2] and not fallbacks.get(item[0]) and not code[item[0]]]
    print(f"PLACEHOLDER-ONLY NAMES NOTHING IN THE PROJECT REACHES ({len(unreached)}):")
    for name, _files, _where in unreached:
        print(f"  {name}")
    print()

    print(f"{len(placeholder_only)} placeholder-only names: {len(referenced)} referenced by an "
          f"asset, {len(by_name)} reached only by name, {len(unreached)} reached by nothing")
    return 0


if __name__ == "__main__":
    sys.exit(main())
