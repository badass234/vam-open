#!/usr/bin/env python3
"""Find the asset bundle that carries a MonoBehaviour class, and dump what surrounds it.

VaM ships its UI as asset bundles under `VaM_Data/StreamingAssets`. That matters for the rebuild:
the panels are not in the repository, so a control cannot be added to one by editing a prefab - the
panel has to be reached at runtime instead. Before writing that code it is worth knowing which
bundle holds the panel, what the panel GameObject is called and which controls sit beside the ones
already wired up.

This is the tool that answers those questions without launching Unity.

    python tools/probe_ui_bundle.py --class UserPreferences --bundle z_cor z_ui1
    python tools/probe_ui_bundle.py --tree CoreControl --bundle z_cor --depth 3
    python tools/probe_ui_bundle.py --fields UserPreferences --bundle z_cor
"""

from __future__ import annotations

import argparse
import collections
import os
import sys

import UnityPy


def class_name(obj) -> str | None:
    """The class a MonoBehaviour was compiled from, or None when it cannot be read."""
    try:
        return obj.read().m_Script.read().m_ClassName
    except Exception:
        return None


def target_id(ptr) -> int:
    """Path id of a PPtr, which is a dict when it came from a type tree and an object otherwise."""
    if isinstance(ptr, dict):
        return ptr.get("m_PathID", 0)
    return getattr(ptr, "m_PathID", 0)


class Bundle:
    """One loaded bundle, with the lookups a hierarchy dump needs."""

    def __init__(self, path: str):
        self.path = path
        self.env = UnityPy.load(path)
        self.objects = list(self.env.objects)
        self.by_id = {obj.path_id: obj for obj in self.objects}
        self.names: dict[int, str] = {}

    def name_of(self, ptr) -> str:
        """The name of the GameObject a PPtr points at, or of its owner if it is a component."""
        path_id = target_id(ptr)
        if path_id in self.names:
            return self.names[path_id]
        target = self.by_id.get(path_id)
        if target is None:
            return "?"
        try:
            path = target.read().m_GameObject
        except Exception:
            try:
                return target.read().m_Name
            except Exception:
                return "?"
        if target_id(path) != 0:
            return self.name_of(path)
        try:
            return target.read().m_Name
        except Exception:
            return "?"

    def label(self, ptr) -> str:
        target = self.by_id.get(target_id(ptr))
        if target is None:
            return "<missing>"
        kind = target.type.name
        if kind == "GameObject":
            return f"'{self.name_of(ptr)}'"
        if kind == "MonoBehaviour":
            return f"{class_name(target)}@{self.name_of(ptr)}"
        if kind in ("Transform", "RectTransform"):
            return f"{kind}@{self.name_of(ptr)}"
        return kind

    def fields(self, obj) -> dict:
        try:
            return obj.read_typetree()
        except Exception as error:
            return {"!typetree": f"{type(error).__name__}: {error}"}


def walk(bundle: Bundle, transform_ptr, depth: int, lines: list[str], max_depth: int) -> int:
    """Append one GameObject and its children to `lines`; returns how many were appended."""
    transform = bundle.by_id.get(target_id(transform_ptr))
    if transform is None:
        return 0

    game_object = bundle.by_id.get(target_id(transform.read().m_GameObject))
    data = game_object.read()
    parts = [bundle_component(bundle, component) for component in data.m_Component]
    lines.append(f"{'  ' * depth}{data.m_Name} [{'on' if data.m_IsActive else 'off'}] {', '.join(parts)}")
    printed = 1

    if depth >= max_depth:
        return printed

    for child in transform.read().m_Children:
        printed += walk(bundle, child, depth + 1, lines, max_depth)
    return printed


def component_pptr(pair):
    """The PPtr inside a GameObject's m_Component entry, which is a ComponentPair on 2018."""
    if hasattr(pair, "component"):
        return pair.component
    if isinstance(pair, dict) and "component" in pair:
        return pair["component"]
    return pair


def bundle_component(bundle: Bundle, ptr) -> str:
    target = bundle.by_id.get(target_id(component_pptr(ptr)))
    if target is None:
        return "<missing>"
    if target.type.name == "MonoBehaviour":
        return class_name(target) or "MonoBehaviour?"
    return target.type.name


def dump(path: str, wanted: str, limit: int) -> int:
    bundle = Bundle(path)
    print(f"== {os.path.basename(path)}")

    hits = []
    unreadable = 0
    kinds: collections.Counter[str] = collections.Counter()
    for obj in bundle.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        name = class_name(obj)
        if name is None:
            unreadable += 1
            continue
        kinds[name] += 1
        if wanted.lower() in name.lower():
            hits.append((name, obj))

    print(f"   objects: {len(bundle.objects)}, MonoBehaviour matching '{wanted}': {len(hits)}")
    if not hits:
        print(f"   (MonoBehaviours whose script could not be read: {unreadable})")
        for name, count in kinds.most_common(10):
            print(f"     {count:6d}  {name}")

    for name, obj in hits[:limit]:
        print(f"   - {name} pathID={obj.path_id} on '{bundle.name_of(obj.read().m_GameObject)}'")
    return len(hits)


def show_tree(bundle: Bundle, root: str, max_depth: int) -> None:
    roots = [obj for obj in bundle.objects
             if obj.type.name == "GameObject" and getattr(obj.read(), "m_Name", None) == root]
    if not roots:
        print(f"   no GameObject named '{root}'")
        return

    for obj in roots:
        lines: list[str] = []
        count = walk(bundle, obj.read().m_Transform, 0, lines, max_depth)
        print(f"   GameObject '{root}' at pathID={obj.path_id}, {count} objects shown "
              f"(depth {max_depth})")
        print("\n".join(lines))


def show_fields(bundle: Bundle, wanted: str, limit: int) -> None:
    shown = 0
    for obj in bundle.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        name = class_name(obj) or ""
        if wanted.lower() not in name.lower():
            continue

        print(f"   fields of {name} on '{bundle.name_of(obj.read().m_GameObject)}':")
        for key, value in bundle.fields(obj).items():
            if isinstance(value, dict) and "m_PathID" in value:
                print(f"     {key} = {bundle.label(value)}")
            elif isinstance(value, (bool, int, float, str)) or value is None:
                print(f"     {key} = {value}")
        shown += 1
        if shown >= limit:
            return


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--class", dest="wanted", help="MonoBehaviour class (substring match)")
    parser.add_argument("--tree", help="dump the GameObject hierarchy under this name")
    parser.add_argument("--fields", help="print the serialized references of this class")
    parser.add_argument("--depth", type=int, default=4, help="depth for --tree")
    parser.add_argument("--bundle", nargs="*", default=[], help="bundle files under StreamingAssets")
    parser.add_argument("--bundle-dir", default=r"C:\Games\VaM_Updater\VaM_Data\StreamingAssets")
    parser.add_argument("--all", action="store_true", help="open every bundle in bundle-dir")
    parser.add_argument("--limit", type=int, default=10)
    args = parser.parse_args()

    bundles = list(args.bundle)
    if args.all:
        bundles = sorted(name for name in os.listdir(args.bundle_dir)
                         if name != "StandaloneWindows64" and not name.endswith(".manifest"))

    total = 0
    for name in bundles:
        path = name if os.path.isabs(name) else os.path.join(args.bundle_dir, name)
        if not os.path.isfile(path):
            print(f"   missing: {path}", file=sys.stderr)
            continue
        if args.wanted:
            total += dump(path, args.wanted, args.limit)

    if args.tree or args.fields:
        for name in bundles:
            path = name if os.path.isabs(name) else os.path.join(args.bundle_dir, name)
            if not os.path.isfile(path):
                continue
            bundle = Bundle(path)
            if args.tree:
                print(f"== {os.path.basename(path)}")
                show_tree(bundle, args.tree, args.depth)
            if args.fields:
                print(f"== {os.path.basename(path)}")
                show_fields(bundle, args.fields, args.limit)

    if args.wanted:
        print(f"total matching MonoBehaviours: {total}")
        return 0 if total else 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
