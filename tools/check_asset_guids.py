#!/usr/bin/env python3
"""Check that every asset GUID referenced by the project is actually defined by a .meta file.

Unity resolves script and asset references through 32-hex-digit GUIDs stored in .meta files.
When an exported project is re-arranged by hand (sources swapped, decompiled assemblies replaced
by DLLs) the .meta files are what keep the scenes and prefabs wired up, so a dangling GUID is a
silent broken reference that only shows up in the editor as "Missing (Mono Script)".

AssetRipper writes .meta files lazily - only for assets whose GUID is referenced somewhere.
That makes "referenced but undefined" a strong signal of a real break.

Some references are known to stay undefined and are listed in known_dangling_guids.txt, so the
check can be used as a gate: only an unknown dangling GUID fails the run.

Usage:
    python tools/check_asset_guids.py --project VAMOpen/VaM_Rebuild
    python tools/check_asset_guids.py --project VAMOpen/VaM_Rebuild --list 40
"""

from __future__ import annotations

import argparse
import collections
import os
import re
import sys

GUID_RE = re.compile(rb"guid:\s*([0-9a-fA-F]{32})")

# Extensions whose contents hold GUID references.
SCAN_EXTENSIONS = {
    ".unity", ".prefab", ".asset", ".controller", ".overrideController", ".mat",
    ".anim", ".physicsMaterial2D", ".physicMaterial", ".playable", ".mixer",
    ".renderTexture", ".meta", ".guiskin", ".fontsettings", ".spriteatlas",
    ".mask", ".shadervariants", ".lighting", ".cubemap", ".flare", ".shader",
}


def collect_defined(assets_dir: str) -> dict[str, str]:
    """guid -> path of the asset the .meta describes."""
    defined: dict[str, str] = {}
    for root, _dirs, files in os.walk(assets_dir):
        for name in files:
            if not name.endswith(".meta"):
                continue
            path = os.path.join(root, name)
            try:
                with open(path, "rb") as handle:
                    match = GUID_RE.search(handle.read(4096))
            except OSError:
                continue
            if match:
                guid = match.group(1).decode("ascii").lower()
                # Strip the trailing ".meta" to get the asset the GUID belongs to.
                defined[guid] = path[: -len(".meta")]
    return defined


def collect_referenced(assets_dir: str) -> tuple[set[str], dict[str, set[str]]]:
    """All GUIDs referenced from asset files, and where each one is referenced."""
    referenced: dict[str, set[str]] = collections.defaultdict(set)
    for root, _dirs, files in os.walk(assets_dir):
        for name in files:
            if name.endswith(".meta"):
                continue
            ext = os.path.splitext(name)[1].lower()
            if ext not in SCAN_EXTENSIONS:
                continue
            path = os.path.join(root, name)
            try:
                with open(path, "rb") as handle:
                    blob = handle.read()
            except OSError:
                continue
            for guid in GUID_RE.findall(blob):
                referenced[guid.decode("ascii").lower()].add(path)
    return set(referenced), referenced


def load_baseline(path: str) -> set[str]:
    """GUIDs that are allowed to remain undefined. '#' starts a comment."""
    if not path or not os.path.isfile(path):
        return set()
    allowed: set[str] = set()
    with open(path, "r", encoding="utf-8") as handle:
        for line in handle:
            line = line.split("#", 1)[0].strip()
            if line:
                allowed.add(line.lower())
    return allowed


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project", required=True, help="Unity project directory")
    parser.add_argument("--list", type=int, default=15,
                        help="how many dangling GUIDs to print")
    parser.add_argument("--baseline", default=os.path.join(
                            os.path.dirname(os.path.abspath(__file__)),
                            "known_dangling_guids.txt"),
                        help="file listing GUIDs that may stay undefined")
    parser.add_argument("--no-baseline", action="store_true",
                        help="ignore the baseline file and fail on any dangling GUID")
    args = parser.parse_args()

    assets = os.path.join(args.project, "Assets")
    if not os.path.isdir(assets):
        print(f"Not a Unity project (no Assets dir): {args.project}", file=sys.stderr)
        return 2

    defined = collect_defined(assets)
    referenced, sources = collect_referenced(assets)

    allowed = set() if args.no_baseline else load_baseline(args.baseline)
    dangling = sorted(referenced - set(defined))
    unexpected = [g for g in dangling if g not in allowed]
    ignored = sorted(set(dangling) - set(unexpected))
    unused = len(set(defined) - referenced)

    print(f"defined GUIDs   : {len(defined)}")
    print(f"referenced GUIDs: {len(referenced)}")
    print(f"unused .meta    : {unused}")
    print(f"dangling refs   : {len(dangling)} (expected {len(ignored)}, unexpected {len(unexpected)})")

    def describe(guid: str) -> str:
        where = sorted(sources[guid])
        shown = ", ".join(os.path.relpath(p, args.project) for p in where[:3])
        if len(where) > 3:
            shown += f" (+{len(where) - 3} more)"
        return f"  {guid}  <- {shown}"

    for title, group in (("Expected dangling GUIDs", ignored), ("UNEXPECTED dangling GUIDs", unexpected)):
        if not group:
            continue
        print()
        print(f"{title} ({len(group)}):")
        for guid in group[: args.list]:
            print(describe(guid))
        if len(group) > args.list:
            print(f"  ... and {len(group) - args.list} more")

    return 1 if unexpected else 0


if __name__ == "__main__":
    sys.exit(main())
