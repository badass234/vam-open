#!/usr/bin/env python3
"""Rewrite ILSpy's C# 7 'ref local' artifacts back into plain C# 6 code.

ILSpy renders `array[i] = value;` as

    ref Vector3 reference = ref array[i];
    reference = value;

which is valid only in C# 7.0+. Virtually all occurrences are the immediately
followed by a single assignment, so the alias can simply be inlined. Anything
that does not match that shape is reported instead of being rewritten, so the
transform never guesses.
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

DECL_RE = re.compile(
    r'^(?P<indent>[ \t]*)ref[ \t]+(?P<type>[\w\.<>\[\],\?]+)[ \t]+'
    r'(?P<name>\w+)[ \t]*=[ \t]*ref[ \t]+(?P<lvalue>.+?);[ \t]*$'
)
ASSIGN_RE = r'^(?P<indent>[ \t]*){name}[ \t]*=[ \t]*(?P<value>.+?);[ \t]*$'


def fix_file(path: Path, check: bool) -> tuple[int, list[str]]:
    original = path.read_text(encoding='utf-8-sig')
    lines = original.split('\n')
    out: list[str] = []
    warnings: list[str] = []
    fixed = 0

    i = 0
    while i < len(lines):
        line = lines[i]
        m = DECL_RE.match(line)
        if not m:
            out.append(line)
            i += 1
            continue

        indent = m.group('indent')
        name = m.group('name')
        lvalue = m.group('lvalue')
        nxt = lines[i + 1] if i + 1 < len(lines) else ''
        am = re.match(ASSIGN_RE.format(name=re.escape(name)), nxt)

        if am:
            out.append(f'{indent}{lvalue} = {am.group("value")};')
            fixed += 1
            i += 2
            continue

        # A multi-line right-hand side (typically an object initializer) starts the
        # same way but only terminates further down the file.
        head = re.match(rf'^(?P<indent>[ \t]*){re.escape(name)}[ \t]*=[ \t]*(?P<value>.*)$', nxt)
        if head and not head.group('value').rstrip().endswith(';'):
            merged = [f'{indent}{lvalue} = {head.group("value")}']
            j = i + 2
            closed = False
            while j < len(lines):
                merged.append(lines[j])
                if lines[j].rstrip().endswith(';'):
                    closed = True
                    j += 1
                    break
                j += 1
            if closed:
                out.extend(merged)
                fixed += 1
                i = j
                continue

        # The alias is read (or used twice) after declaration: not safe to inline
        # mechanically, so report it for a hand-written fix.
        if re.search(rf'\b{re.escape(name)}\b', nxt):
            warnings.append(f'{path.name}:{i + 1}: alias "{name}" used beyond a simple assignment -> {line.strip()}')
        else:
            warnings.append(f'{path.name}:{i + 1}: unexpected ref-local shape -> {line.strip()}')
        out.append(line)
        i += 1

    if fixed and not check:
        path.write_text('\n'.join(out), encoding='utf-8-sig')
    return fixed, warnings


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument('root', type=Path)
    ap.add_argument('--check', action='store_true', help='report only, do not write')
    args = ap.parse_args()

    total = 0
    all_warnings: list[str] = []
    files_changed = 0

    for path in sorted(args.root.rglob('*.cs')):
        fixed, warnings = fix_file(path, args.check)
        if fixed:
            total += fixed
            files_changed += 1
        all_warnings.extend(warnings)

    verb = 'would inline' if args.check else 'inlined'
    print(f'{verb} {total} ref locals across {files_changed} files')
    if all_warnings:
        print(f'{len(all_warnings)} site(s) need manual attention:')
        for w in all_warnings[:60]:
            print('  ' + w)
        if len(all_warnings) > 60:
            print(f'  ... and {len(all_warnings) - 60} more')
    return 0


if __name__ == '__main__':
    sys.exit(main())
