#!/usr/bin/env python3
"""Scan the shipped plugin assemblies for the APIs an engine upgrade removes.

An upgrade guide names APIs that the newer engine no longer has. The project's
own sources can be grepped for them, but a plugin that ships as a `.dll` has no
source here, and a plugin is exactly what breaks first when an API goes away.
This walks the metadata tables of every assembly under a root - `TypeRef` and
`MemberRef` - so a reference to a removed API is found even when the caller is a
binary blob.

The default name list is the one `docs/unity-upgrade-audit.md` audits, taken
from Unity's 2018 LTS and 2019 LTS upgrade guides. A name matches when it is a
type's name or namespace-qualified name, or a member's name; matching is exact,
so `Network` does not match `NetworkView`.

Usage:
    python tools/audit_plugin_apis.py
    python tools/audit_plugin_apis.py --name NetworkView --name CancelQuit
    python tools/audit_plugin_apis.py --root VaM_Rebuild\\Assets\\Plugins --out artifacts\\plugin-api-audit.txt
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

import dnfile

NO_METADATA = 'no CLR metadata'

# Assemblies AssetRipper wrote under Assets\Plugins. `.pak` and `.bin` are
# managed assemblies too - AssetRipper names a few of them by content - so all
# of these extensions are candidates and the ones that do not parse are
# reported rather than silently dropped.
ASSEMBLY_SUFFIXES = ('.dll', '.pak', '.bin', '.exe')

# What the guides remove, and why the name is in the list.
GUIDE_APIS = [
    ('NetworkView', '2018.2: UnityEngine.Networking.NetworkView removed'),
    ('NetworkPlayerSurrogate', '2018.2: the legacy networking stack removed'),
    ('NetworkViewIDSurrogate', '2018.2: the legacy networking stack removed'),
    ('NetworkIdentity', '2018.2: the high level UNet API removed'),
    ('NetworkManager', '2018.2: the high level UNet API removed'),
    ('Network', '2018.2: UnityEngine.Networking.Network removed'),
    ('MasterServer', '2018.2: UnityEngine.MasterServer removed'),
    ('RPCMode', '2018.2: UnityEngine.RPCMode removed'),
    ('get_mainAsset', 'AssetBundle.mainAsset obsolete'),
    ('BuildPipeline', 'editor-only build API, replaced by BuildReport'),
    ('BuildPlayer', 'editor-only build API, replaced by BuildReport'),
    ('BuildAssetBundles', 'editor-only build API, replaced by BuildReport'),
    ('CancelQuit', 'Application.CancelQuit deprecated'),
    ('BuildHumanAvatar', 'AvatarBuilder.BuildHumanAvatar deprecated'),
    ('wasCanceled', 'TouchScreenKeyboard.wasCanceled obsolete'),
    ('TouchScreenKeyboard', 'context for wasCanceled, not itself removed'),
]


def text(value) -> str:
    """The heap item of a metadata row resolves to its string only on demand."""
    return '' if value is None else str(value)


def type_name(row) -> str:
    """`Namespace.Name` of a type's row, or '' for a row that declares no type.

    Only `TypeRef` and `TypeDef` rows carry `TypeName`/`TypeNamespace`; a
    `MemberRef` may be declared by a module or a method instead.
    """
    name = text(getattr(row, 'TypeName', None))
    if not name:
        return ''
    namespace = text(getattr(row, 'TypeNamespace', None))
    return f'{namespace}.{name}' if namespace else name


def scan(path: Path):
    """Return (types, members) referenced by one assembly.

    `types` is `(Namespace.Name, Name)` per `TypeRef` row, `members` is
    `(Name, declared-by type)` per `MemberRef` row. Returns None for a file
    that carries no managed metadata: `Assets\\Plugins` mixes managed plugins
    with the browser's native Chromium libraries, and a native library has no
    metadata tables to read.
    """
    pe = dnfile.dnPE(str(path))
    if pe.net is None:
        return None
    tables = pe.net.mdtables
    types: list[tuple[str, str]] = []
    members: list[tuple[str, str]] = []

    for row in getattr(getattr(tables, 'TypeRef', None), 'rows', []):
        name = text(getattr(row, 'TypeName', None))
        if name:
            types.append((type_name(row), name))

    for row in getattr(getattr(tables, 'MemberRef', None), 'rows', []):
        name = text(getattr(row, 'Name', None))
        if not name:
            continue
        declared_by = type_name(getattr(getattr(row, 'Class', None), 'row', None))
        members.append((name, declared_by))

    return types, members


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('--root', default=str(Path(__file__).resolve().parent.parent / 'VaM_Rebuild' / 'Assets' / 'Plugins'),
                        help='directory holding the plugin assemblies')
    parser.add_argument('--assembly', action='append', default=[],
                        help='one more assembly to scan, by path; repeatable')
    parser.add_argument('--name', action='append', default=None,
                        help='API name to look for; repeatable, defaults to the guide list')
    parser.add_argument('--examples', type=int, default=3, help='how many hits to print per name')
    parser.add_argument('--out', default=None, help='also write the report to this file')
    args = parser.parse_args()

    root = Path(args.root)
    if not root.is_dir():
        print(f'no such directory: {root}', file=sys.stderr)
        return 2

    names = args.name if args.name else [name for name, _ in GUIDE_APIS]
    reasons = dict(GUIDE_APIS)

    paths = sorted(path for path in root.rglob('*')
                   if path.is_file() and path.suffix.lower() in ASSEMBLY_SUFFIXES)
    paths.extend(Path(path) for path in args.assembly)

    scanned: dict[str, tuple[list, dict]] = {}
    native: list[str] = []
    failed: list[tuple[str, str]] = []
    for path in paths:
        try:
            result = scan(path)
        except Exception as error:  # a file that is not a PE at all
            failed.append((path.name, type(error).__name__))
            continue
        if result is None:
            native.append(path.name)
            continue
        scanned[path.name] = result

    lines: list[str] = []
    lines.append(f'root: {root}')
    if args.assembly:
        lines.append(f'extra assemblies: {", ".join(sorted(Path(path).name for path in args.assembly))}')
    lines.append(f'candidate files: {len(paths)}, managed assemblies scanned: {len(scanned)}, '
                 f'native (no CLR metadata): {len(native)}, unreadable: {len(failed)}')
    if failed:
        lines.append('unreadable: ' + ', '.join(f'{name} ({why})' for name, why in failed))
    lines.append('scanned: ' + ', '.join(sorted(scanned)))
    lines.append('')

    width = max(len(name) for name in names) + 2
    lines.append(f'{"name":<{width}} {"assemblies":>10} {"refs":>6}  what the guide removes')
    lines.append('-' * 104)

    for name in names:
        hits: list[str] = []
        for assembly, (types, members) in scanned.items():
            for member_name, declared_by in members:
                if member_name == name:
                    hits.append(f'{assembly}!{declared_by}::{member_name}' if declared_by
                                else f'{assembly}!{member_name}')
            for qualified, bare in types:
                if qualified == name or bare == name:
                    hits.append(f'{assembly}!{qualified}')
        hits = sorted(set(hits))
        assemblies = len({hit.split('!')[0] for hit in hits})
        lines.append(f'{name:<{width}} {assemblies:>10} {len(hits):>6}  {reasons.get(name, "")}')
        for example in hits[:args.examples]:
            lines.append(f'{"":<{width}} {"":>10} {"":>6}    {example}')
        if len(hits) > args.examples:
            lines.append(f'{"":<{width}} {"":>10} {"":>6}    ... and {len(hits) - args.examples} more')

    report = '\n'.join(lines)
    print(report)
    if args.out:
        Path(args.out).write_text(report + '\n', encoding='utf-8')
        print(f'\nwritten to {args.out}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
