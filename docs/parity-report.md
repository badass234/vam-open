# Parity report for the rebuilt assembly

Check: `src/` builds into `artifacts/bin/Assembly-CSharp/Debug/net472/Assembly-CSharp.dll`
(6 064 128 bytes, original ≈ 5.8 MB) and is compared against the original
`VaM_Data/Managed/Assembly-CSharp.dll` by type list and member signatures.

Reproduction:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Build.ps1 -Project Assembly-CSharp
python tools\compare_api.py --original artifacts\il\Assembly-CSharp.il ^
                            --rebuilt  artifacts\il\Assembly-CSharp.rebuilt.il --show 8 --details 25
```

## Result

| Metric | Original | Rebuild |
|---|---|---|
| Types (excluding compiler-generated) | 2753 | 2753 |
| Missing types | — | 0 |
| Extra types | — | 0 |
| Compilation errors | — | 0 |
| Warnings | — | 100 (CS0618 ×74, CS0219 ×20, CS0067 ×4, CS1717 ×2) |

The full type list matches. The signature differences are 13 "missing" and 20 "new"
members out of ~100 000 members, and all are explainable:

1. **Deliberate fixes to code incompatible with C# 6.** `ICanvasElement.get_transform()`,
   `IsDestroyed()` and `IBoxSelectable.get_transform()` were explicit implementations
   of interface methods; the decompiler emitted them as methods, while the C# 6 compiler
   requires properties — they became explicit interface properties (same members, different form).
2. **Empty static constructors.** In the original, 30 types have a `.cctor`
   with a body of exactly `IL_0000: ret` (output of Unity's old Mono compiler). Roslyn
   does not create them. The body is empty, there is no observable effect.
3. **Modifier normalization in the checker itself.** `newslot`, `final`,
   `runtime managed` on delegates stay in the signature and give false mismatches.

## What this proves and what it does not

Proven: the type structure and the member set are preserved, and the code compiles for
`net472` / C# 6 against the game's "native" Unity assemblies.

Not proven at this step: the correctness of method bodies and of the "script ↔ scene
object" links. That is verified at the launch and original-comparison stages
(see [`docs/verification.md`](verification.md)).

## Verification tools

- `tools/fix_ref_locals.py` — ILSpy's `ref` locals (C# 7) → C# 6: 486 places in 85 files.
- `tools/fix_member_tokens.py` — restoring members by `ldtoken` from the IL: 15 places.
- `tools/compare_api.py` — comparing the signatures of the original and the rebuild via the IL dump.
- `artifacts/il/Assembly-CSharp.il` — the original's IL dump (60 MB), needed because
  `ilspycmd -il` ignores `-t` and always prints the whole assembly.
