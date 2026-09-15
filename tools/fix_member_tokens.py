#!/usr/bin/env python3
"""Replace ILSpy's unresolved `ldtoken` placeholders with the real member.

When the IL carries an `ldtoken` for a property accessor that ILSpy cannot bind,
it emits

    (MethodInfo)MethodBase.GetMethodFromHandle((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/)

C# has no syntax for a member token, so the fix is to look the same accessor up
by reflection. The real member name is read straight from the IL of the type, so
nothing is guessed.
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

PLACEHOLDER = (
    '(MethodInfo)MethodBase.GetMethodFromHandle('
    '(RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/)'
)

# ilspycmd can print IL for a whole assembly but not for a single type, so the
# members are recovered from a pre-dumped assembly IL listing.
CLASS_START_RE = re.compile(r'^\.class\s+(?P<flags>.*?)(?P<name>[\w\.`<>\+]+)\s*$')
CLASS_END_RE = re.compile(r'^\s*\}\s*//\s*end of class\s+(?P<name>\S+)\s*$')
LDTOKEN_RE = re.compile(
    r'\s*ldtoken\s+method\s+.*?\s+(?P<type>\[[^\]]+\][\w\.\+]+)::(?P<method>get_\w+|set_\w+)\s*\('
)
NAMESPACE_RE = re.compile(r'^namespace\s+([\w\.]+)', re.MULTILINE)
TYPE_RE = re.compile(
    r'^\s*(?:public|internal)\s+(?:sealed\s+|abstract\s+|static\s+)*(?:class|struct)\s+(\w+)',
    re.MULTILINE,
)


def strip_assembly_qualifier(type_name: str) -> str:
    return re.sub(r'^\[[^\]]+\]', '', type_name).replace('+', '.')


def accessor_expression(declaring_type: str, method: str) -> str | None:
    prefix, _, member = method.partition('_')
    if prefix == 'get':
        return f'typeof({declaring_type}).GetProperty("{member}").GetGetMethod()'
    if prefix == 'set':
        return f'typeof({declaring_type}).GetProperty("{member}").GetSetMethod()'
    return None


def load_tokens(il_path: Path) -> dict[str, list[str]]:
    """Map every type to the ordered member tokens found inside its methods."""
    tokens: dict[str, list[str]] = {}
    stack: list[str] = []

    with il_path.open('r', encoding='utf-8', errors='replace') as handle:
        for line in handle:
            if line.startswith('.class '):
                match = CLASS_START_RE.match(line.rstrip('\n'))
                if match:
                    stack.append(strip_assembly_qualifier(match.group('name').lstrip('.')))
                continue
            if 'end of class' in line:
                match = CLASS_END_RE.match(line.rstrip('\n'))
                if match and stack:
                    name = strip_assembly_qualifier(match.group('name'))
                    if stack[-1] == name:
                        stack.pop()
                    elif name in stack:
                        del stack[stack.index(name):]
                continue
            if 'ldtoken' not in line or not stack:
                continue
            match = LDTOKEN_RE.search(line.rstrip('\n'))
            if not match:
                continue
            expr = accessor_expression(
                strip_assembly_qualifier(match.group('type')), match.group('method')
            )
            tokens.setdefault(stack[-1], []).append(expr)

    return tokens


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument('--il', required=True, type=Path, help='assembly IL listing dumped by ilspycmd -il')
    ap.add_argument('--root', required=True, type=Path)
    ap.add_argument('--check', action='store_true')
    args = ap.parse_args()

    tokens = load_tokens(args.il)
    total_replaced = 0
    problems: list[str] = []

    for path in sorted(args.root.rglob('*.cs')):
        text = path.read_text(encoding='utf-8-sig')
        count = text.count(PLACEHOLDER)
        if not count:
            continue

        ns = NAMESPACE_RE.search(text)
        typ = TYPE_RE.search(text)
        if not ns or not typ:
            problems.append(f'{path.name}: cannot determine namespace/type')
            continue
        fqn = f'{ns.group(1)}.{typ.group(1)}'
        exprs = tokens.get(fqn, [])

        if len(exprs) != count:
            problems.append(
                f'{path.name}: {count} placeholder(s) in source but {len(exprs)} member token(s) in IL of {fqn}'
            )
            continue
        if any(e is None for e in exprs):
            problems.append(f'{path.name}: unsupported member kind in {fqn}')
            continue

        for expr in exprs:
            text = text.replace(PLACEHOLDER, expr, 1)

        if not args.check:
            path.write_text(text, encoding='utf-8-sig')
        total_replaced += count
        print(f'{path.name}: {count} -> {", ".join(exprs)}')

    verb = 'would replace' if args.check else 'replaced'
    print(f'{verb} {total_replaced} member token placeholder(s)')
    for p in problems:
        print('  ! ' + p)
    return 0


if __name__ == '__main__':
    sys.exit(main())
