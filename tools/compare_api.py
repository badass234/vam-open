#!/usr/bin/env python3
"""Compare the member surface of two IL listings produced by `ilspycmd -il`.

Used to verify that the rebuilt assembly still exposes the same fields and
methods as the original game assembly. Compiler-generated closure and iterator
types are ignored, since their names change on every recompilation.
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

CLASS_START_RE = re.compile(r'^\.class\s+(?P<flags>.*?)(?P<name>[\w\.`<>\+]+)\s*$')
CLASS_END_RE = re.compile(r'^\s*\}\s*//\s*end of class\s+(?P<name>\S+)\s*$')
FIELD_RE = re.compile(r'^\s*\.field\s+(?P<sig>.+)$')
GENERATED_RE = re.compile(r'[<>]|__StaticArrayInitTypeSize|EmbeddedAttribute|RefSafetyRulesAttribute')
WHITESPACE_RE = re.compile(r'\s+')

# Unity's Mono mscorlib keeps LINQ/`Func` types in System.Core while the .NET
# Framework reference assemblies place them in mscorlib. The declaring assembly
# is irrelevant to API parity, so it is stripped before comparing.
ASSEMBLY_QUALIFIER_RE = re.compile(r'\[[^\]]+\](?=[\w\.`\+])')


def normalise(signature: str) -> str:
    signature = ASSEMBLY_QUALIFIER_RE.sub('', signature)
    return WHITESPACE_RE.sub(' ', signature).strip()


def is_generated_member(signature: str) -> bool:
    """Members the compiler synthesises for closures, iterators and caches."""
    return '<' in signature or '$' in signature


MODIFIER_RE = re.compile(
    r'\b(?:public|private|protected|internal|assembly|family|famorassem|famandassem'
    r'|hidebysig|specialname|rtspecialname|newslot|final|virtual|abstract|static|instance'
    r'|cil|managed|beforefieldinit|auto|ansi|sealed)\b'
)


def strip_modifiers(signature: str) -> str:
    """Drop access/implementation modifiers so only the member identity remains."""
    return WHITESPACE_RE.sub(' ', MODIFIER_RE.sub('', signature)).strip()


def load(path: Path) -> dict[str, dict[str, set[str]]]:
    """Map each type to the set of its method and field signatures."""
    types: dict[str, dict[str, set[str]]] = {}
    stack: list[str] = []
    method_lines: list[str] | None = None

    def flush() -> None:
        nonlocal method_lines
        if method_lines is None:
            return
        signature = normalise(' '.join(line.strip() for line in method_lines))
        signature = signature.removeprefix('.method ').removesuffix(' cil managed').strip()
        if stack:
            types[stack[-1]]['method'].add(signature)
        method_lines = None

    with path.open('r', encoding='utf-8', errors='replace') as handle:
        for raw in handle:
            line = raw.rstrip('\n')

            if method_lines is not None:
                if line.strip() == '{':
                    flush()
                else:
                    method_lines.append(line)
                continue

            if line.startswith('.class '):
                match = CLASS_START_RE.match(line)
                if match:
                    stack.append(match.group('name').lstrip('.'))
                    types.setdefault(stack[-1], {'method': set(), 'field': set()})
                continue

            if 'end of class' in line:
                match = CLASS_END_RE.match(line)
                if match and stack:
                    name = match.group('name')
                    if stack[-1] == name:
                        stack.pop()
                    elif name in stack:
                        del stack[stack.index(name):]
                continue

            if not stack:
                continue

            if re.match(r'^\s*\.method\b', line):
                method_lines = [line]
                continue

            match = FIELD_RE.match(line)
            if match:
                field_signature = normalise(match.group('sig').split('//')[0])
                types[stack[-1]]['field'].add(field_signature)

    return types


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument('--original', required=True, type=Path)
    ap.add_argument('--rebuilt', required=True, type=Path)
    ap.add_argument('--show', type=int, default=10)
    ap.add_argument('--details', type=int, default=0, help='print this many absent member signatures')
    args = ap.parse_args()

    real_original = {k: v for k, v in load(args.original).items() if not GENERATED_RE.search(k)}
    real_rebuilt = {k: v for k, v in load(args.rebuilt).items() if not GENERATED_RE.search(k)}

    missing_types = sorted(set(real_original) - set(real_rebuilt))
    extra_types = sorted(set(real_rebuilt) - set(real_original))

    all_names = sorted(set(real_original) & set(real_rebuilt))

    missing_names: set[str] = set()
    extra_names: set[str] = set()
    cosmetic: list[str] = []
    broken: list[str] = []
    missing_only: set[str] = set()

    for name in all_names:
        for kind in ('method', 'field'):
            raw_missing = {s for s in real_original[name][kind] - real_rebuilt[name][kind]
                           if not is_generated_member(s)}
            raw_extra = {s for s in real_rebuilt[name][kind] - real_original[name][kind]
                         if not is_generated_member(s)}
            if not raw_missing and not raw_extra:
                continue

            rebuilt_identity = {strip_modifiers(s) for s in real_rebuilt[name][kind]}
            original_identity = {strip_modifiers(s) for s in real_original[name][kind]}

            real_missing = {s for s in raw_missing if strip_modifiers(s) not in rebuilt_identity}
            real_extra = {s for s in raw_extra if strip_modifiers(s) not in original_identity}

            if real_missing or real_extra:
                missing_names.update(real_missing)
                extra_names.update(real_extra)
                missing_only |= real_missing
                broken.append(
                    f'{name} [{kind}] absent={len(real_missing)} new={len(real_extra)}'
                )
            elif raw_missing or raw_extra:
                cosmetic.append(
                    f'{name} [{kind}] modifiers={len(raw_missing)}/{len(raw_extra)}'
                )

    print(f'types: {len(real_original)} original / {len(real_rebuilt)} rebuilt')
    print(f'types missing in rebuild: {len(missing_types)}')
    print(f'types extra in rebuild:   {len(extra_types)}')
    print(f'members absent from rebuild (identity, ignoring access modifiers): {len(missing_names)}')
    print(f'members newly present in rebuild: {len(extra_names)}')
    print(f'types where only access modifiers differ: {len(cosmetic)}')

    for label, items in (('missing type', missing_types), ('extra type', extra_types)):
        for item in items[: args.show]:
            print(f'  {label}: {item}')
        if len(items) > args.show:
            print(f'  ... {len(items) - args.show} more')

    for line in broken[: args.show]:
        print(f'  {line}')
    if len(broken) > args.show:
        print(f'  ... {len(broken) - args.show} more types with absent members')

    if args.details:
        print('\n-- absent members --')
        for signature in sorted(missing_only)[: args.details]:
            print(f'  {signature}')

    return 0


if __name__ == '__main__':
    sys.exit(main())
