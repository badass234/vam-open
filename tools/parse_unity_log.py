#!/usr/bin/env python3
"""Parse a Unity log (Editor.log / compilation log) into an error report — stage 4.

Why: on this machine Unity 2018.1.9f2 can only be licensed through Hub in GUI mode, so
the list of compilation errors comes from the log text the editor writes rather than from
a batch run.

Categories (heuristic by error code, see CATEGORIES):
  syntax       — decompiler artifacts: broken syntax, missing using directives
  missing-assembly — type/namespace not found: no assembly or reference
  unity-api    — the type exists, but the member/signature does not match 2018.1
  other        — everything else (handled manually)

Usage:
  python tools\\parse_unity_log.py                       # Editor.log by default
  python tools\\parse_unity_log.py path\\to\\log.log
  python tools\\parse_unity_log.py --out artifacts\\compile-errors.md --json artifacts\\compile-errors.json
  python tools\\parse_unity_log.py --fail-on-errors      # exit code 1 if there are errors (gate)
"""
from __future__ import annotations

import argparse
import json
import os
import re
import sys
from collections import Counter
from pathlib import Path

DEFAULT_LOG = Path(os.environ.get("LOCALAPPDATA", "")) / "Unity" / "Editor" / "Editor.log"

CS_LOC = re.compile(
    r"^(?P<file>[A-Za-z0-9_\-./\\:]+\.(?:cs|js|shader|hlsl|cginc|compute))"
    r"\((?P<line>\d+),(?P<col>\d+)\):\s*error\s+(?P<code>CS\d+):\s*(?P<msg>.*)$"
)
CS_PLAIN = re.compile(
    r"^(?P<file>[A-Za-z0-9_\-./\\:]+\.cs):\s*error\s+(?P<code>CS\d+):\s*(?P<msg>.*)$"
)
CS_NOFILE = re.compile(r"^(?:.*?:\s*)?error\s+(?P<code>CS\d+):\s*(?P<msg>.*)$")
WARN_LOC = re.compile(
    r"^(?P<file>[A-Za-z0-9_\-./\\:]+\.cs)\((?P<line>\d+),(?P<col>\d+)\):\s*warning\s+(?P<code>CS\d+):"
)
UNITY_ERR = re.compile(
    r"^(?P<file>[A-Za-z0-9_\-./\\:]+\.(?:shader|hlsl|cginc|compute|asmdef|asset|json))"
    r"(?:\((?P<line>\d+)\))?:\s*error[:\s](?P<msg>.*)$"
)
SHADER_ERR = re.compile(r"^Shader error in '(?P<shader>[^']+)':\s*(?P<msg>.*)$")

CATEGORIES = {
    "syntax": {
        "CS1002", "CS1003", "CS1010", "CS1022", "CS1026", "CS1031", "CS1039", "CS1041",
        "CS1513", "CS1519", "CS1520", "CS1525", "CS1585", "CS1586", "CS0106", "CS0116",
        "CS1522", "CS1528", "CS1547", "CS1023", "CS1035", "CS1036", "CS1028", "CS1029",
    },
    "missing-assembly": {
        "CS0246", "CS0234", "CS0012", "CS1069", "CS0518", "CS0400", "CS0006", "CS2001",
        "CS1705", "CS1701", "CS0426",
    },
}


def categorize(code: str) -> str:
    if code in CATEGORIES["syntax"]:
        return "syntax"
    if code in CATEGORIES["missing-assembly"]:
        return "missing-assembly"
    if code.startswith("CS"):
        return "unity-api"
    return "other"


def parse(log_path: Path) -> dict:
    errors: dict[tuple, dict] = {}
    warn_codes: Counter = Counter()
    shader_errors: Counter = Counter()
    total_lines = 0

    with log_path.open("r", encoding="utf-8", errors="replace") as fh:
        for raw in fh:
            total_lines += 1
            line = raw.rstrip("\r\n")
            if not line:
                continue

            if "error CS" in line:
                m = CS_LOC.match(line) or CS_PLAIN.match(line)
                if m:
                    rec = {
                        "file": m.group("file").replace("\\", "/"),
                        "line": int(m.group("line")),
                        "col": int(m.group("col")),
                        "code": m.group("code"),
                        "message": m.group("msg").strip(),
                    }
                else:
                    m = CS_NOFILE.match(line)
                    if not m:
                        continue
                    rec = {
                        "file": "<no location>",
                        "line": 0,
                        "col": 0,
                        "code": m.group("code"),
                        "message": m.group("msg").strip(),
                    }
                rec["category"] = categorize(rec["code"])
                key = (rec["file"], rec["line"], rec["col"], rec["code"], rec["message"])
                errors[key] = rec
            elif "warning CS" in line:
                m = WARN_LOC.match(line)
                if m:
                    warn_codes[m.group("code")] += 1
            else:
                m = SHADER_ERR.match(line)
                if m:
                    shader_errors[m.group("shader")] += 1
                    continue
                m = UNITY_ERR.match(line)
                if m and "/Assets/" in line:
                    rec = {
                        "file": m.group("file").replace("\\", "/"),
                        "line": int(m.group("line") or 0),
                        "col": 0,
                        "code": "ASSET",
                        "message": m.group("msg").strip(),
                        "category": "other",
                    }
                    key = (rec["file"], rec["line"], 0, "ASSET", rec["message"])
                    errors[key] = rec

    recs = sorted(
        errors.values(),
        key=lambda r: (r["file"], r["line"], r["col"], r["code"]),
    )
    return {
        "log": str(log_path),
        "log_lines": total_lines,
        "error_count": len(recs),
        "errors": recs,
        "by_category": dict(Counter(r["category"] for r in recs)),
        "by_code": dict(Counter(r["code"] for r in recs).most_common()),
        "by_file": dict(Counter(r["file"] for r in recs).most_common()),
        "warning_codes": dict(warn_codes.most_common()),
        "shader_errors": dict(shader_errors.most_common()),
    }


def render_markdown(rep: dict, top: int) -> str:
    out = [
        "# Unity compilation errors",
        "",
        f"- Log: `{rep['log']}` ({rep['log_lines']} lines)",
        f"- Unique errors: **{rep['error_count']}**",
    ]
    if rep["by_category"]:
        out.append("- By category: " + ", ".join(
            f"`{k}` = {v}" for k, v in sorted(rep["by_category"].items(), key=lambda kv: -kv[1])
        ))
    out.append("")

    out.append("## Top error codes")
    out.append("")
    out.append("| Code | Category | Count | Message |")
    out.append("|---|---|---|---|")
    first_msg: dict[str, str] = {}
    for e in rep["errors"]:
        first_msg.setdefault(e["code"], e["message"])
    for code, count in list(rep["by_code"].items())[:top]:
        msg = first_msg.get(code, "")
        if len(msg) > 90:
            msg = msg[:87] + "..."
        out.append(f"| {code} | {categorize(code)} | {count} | {msg} |")
    out.append("")

    out.append(f"## Files with the most errors (top {top})")
    out.append("")
    out.append("| File | Errors |")
    out.append("|---|---|")
    for path, count in list(rep["by_file"].items())[:top]:
        out.append(f"| `{path}` | {count} |")
    out.append("")

    if rep["shader_errors"]:
        out.append("## Shader errors")
        out.append("")
        for name, count in rep["shader_errors"].items():
            out.append(f"- `{name}` — {count}")
        out.append("")

    if rep["warning_codes"]:
        out.append("## Warnings (codes)")
        out.append("")
        out.append(", ".join(f"`{c}`={n}" for c, n in rep["warning_codes"].items()))
        out.append("")

    out.append("## Full list")
    out.append("")
    for e in rep["errors"]:
        loc = f"{e['file']}({e['line']},{e['col']})" if e["line"] else e["file"]
        out.append(f"- `{loc}` **{e['code']}** [{e['category']}] {e['message']}")
    out.append("")
    return "\n".join(out)


def main() -> int:
    ap = argparse.ArgumentParser(description="Parse a Unity log for compilation errors")
    ap.add_argument("log", nargs="?", type=Path, default=DEFAULT_LOG)
    ap.add_argument("--out", type=Path, help="markdown report")
    ap.add_argument("--json", type=Path, help="machine-readable report")
    ap.add_argument("--top", type=int, default=25)
    ap.add_argument("--fail-on-errors", action="store_true")
    args = ap.parse_args()

    if not args.log.is_file():
        print(f"log not found: {args.log}", file=sys.stderr)
        return 2

    rep = parse(args.log)
    print(f"log: {rep['log']} ({rep['log_lines']} lines)")
    print(f"unique errors: {rep['error_count']}")
    for cat, count in sorted(rep["by_category"].items(), key=lambda kv: -kv[1]):
        print(f"  {cat:<18} {count}")
    if rep["by_code"]:
        print("top codes: " + ", ".join(f"{c}={n}" for c, n in list(rep["by_code"].items())[:10]))
    if rep["shader_errors"]:
        print(f"shader errors: {sum(rep['shader_errors'].values())}")

    if args.out:
        args.out.parent.mkdir(parents=True, exist_ok=True)
        args.out.write_text(render_markdown(rep, args.top), encoding="utf-8")
        print(f"report: {args.out}")
    if args.json:
        args.json.parent.mkdir(parents=True, exist_ok=True)
        args.json.write_text(json.dumps(rep, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"json : {args.json}")

    if args.fail_on_errors and rep["error_count"]:
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
