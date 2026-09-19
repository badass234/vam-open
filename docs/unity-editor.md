# Activating the Unity editor

> **Status: resolved — read from here.** The editor is activated: the user launched 2018.1.9f2
> directly once and signed in through its own GUI, so `scripts\Activate-UnityLicense.ps1` was not
> needed. Import, compilation and batch `-executeMethod` have worked since then. **No hop has
> touched the licence** - 2018.4 accepted `Unity_lic.ulf` as it was, with only
> `Initiating legacy licensing module` and `Next license update check is after ...` in its log, and
> 2019.4 and 2020.3 import, compile and run `-executeMethod` on the same file, so nothing here had to be
> redone for any of them. Everything below is the breakdown of the failure causes and the
> fallback paths: it is needed only if the license breaks again. The one thing measured to break it is
> Unity Hub's `updateLicenses` reissuing the ULF - **and only the 2018.1 editor cared.** Hub was
> launched again on 2026-09-18, the file *was* reissued at 21:41:28, and 2019.4 and 2020.3 went on
> importing, compiling and running `-executeMethod` on the reissued file, so the caution further down
> is about the old legacy validator rather than about this project.

## Why

Unity 2018.1.9f2 (Windows) reads the file license `C:\ProgramData\Unity\Unity_lic.ulf`
through the **legacy validator**. On this machine Hub 3.21 kept a license in that file which
the validator accepted: a launch from Hub on 15.09 at 11:50 imported the project
(`%USERPROFILE%\My project\Library` was created, the log has no license error lines),
at 11:58:12 Hub ran `updateLicenses: Updating ULF licenses` and **reissued** the ULF —
after that everything that tries to launch the editor runs into activation:

| Launch method | Result |
|---|---|
| `-batchmode -nographics -quit` | `BatchMode: Unity has not been activated with a valid License` (`WinEditorMain.cpp:885`), then `DisplayProgressbar: Unity license` and `Cancelling DisplayDialog: Failed to activate/update license` |
| `+ -acceptSoftwareTermsForThisRunOnly` | the same (the terms are not the issue) |
| GUI directly | login window (the log shows loading `cdn.login.unity.com`), import does not start |
| GUI with `-licensingIpc LicenseClient-<user>` | the process hangs, the log does not grow |
| launch from Hub (12:17, 12:18) | the log is only the 906 B header, no import |
| `-createManualActivationFile` (ULF removed) | `.alf` is not created: the license check runs earlier |
| `-username <fake> -password <fake>` | **the network request went out**: `UnityConnectLoginRequest: Failed to login`, `HTTP error code 401` on `https://core.cloud.unity3d.com/api/login` |

The last row is the main point: the CLI activation mechanism in 2018.1 is alive, it just gets a 401
on wrong credentials. So **correct credentials produce a valid legacy license** and bring back
both batch and GUI.

The reason the ULF from Hub does not work: Hub issues a license for the modern
`Unity.Licensing.Client` (which accepts it: `Found 1 entitlements`, `996447-UnityPersonal`,
`statusCode 200 "SameMachine"`), while the 2018.1 legacy validator checks `MachineBindings`
in its own way. Hence the official resolution: activate the old editor **with the editor itself**.

## How to activate

```powershell
# will prompt for the Unity ID email and password (the password is entered hidden)
.\scripts\Activate-UnityLicense.ps1
```

The script runs one batch pass that activates the license and imports
`VaM_Rebuild` along the way (this immediately gives an import log for stage 4). The credentials
are passed only to the editor process, they are not written to files or to history.

Limitations worth knowing in advance:
- **Two-factor authentication breaks CLI login** — with 2FA enabled this path will not work,
  use manual activation (below).
- The password is visible in the process list for a short time — this is a requirement of the editor itself.
- If the account has a Plus/Pro serial, add `-Serial <serial>`.

## Alternative: manual (offline) activation

Works if CLI login is unavailable (2FA) or Unity does not hand out Personal through the CLI:

1. Launch the GUI editor without license arguments and wait for the license window.
2. In the window choose manual activation and save the request — this produces an `.alf` with
   **machine bindings as 2018.1 itself describes them**.
3. Upload the `.alf` to <https://license.unity3d.com/manual>, choose Unity Personal,
   download the `.ulf`.
4. Load the `.ulf` in the license window (or `Unity.exe -batchmode -nographics -quit -manualLicenseFile <file>.ulf`).

The result is the same: a legacy-valid `C:\ProgramData\Unity\Unity_lic.ulf`.

## What to do after (re)activation

1. Make sure `C:\ProgramData\Unity\Unity_lic.ulf` was updated (modification time).
2. Run the compilation gate — it also checks that batch works at all:

```powershell
scripts\Invoke-CompileGate.ps1
```

The verdict is the `----- RebuildGate OK -----` marker in the log, not the return code. Error breakdown:
`tools\parse_unity_log.py` (invoked by the script itself).

3. **A reissued license only matters to the 2018.1 editor.** Hub's `updateLicenses` rewrites
   `Unity_lic.ulf`; the legacy validator in 2018.1 is the one that rejects the rewrite, and 2018.4,
   2019.4 and 2020.3 were measured to keep importing and compiling on it. Launch the editor directly
   (`-batchmode` or `-projectPath`) when you can, and re-run `Activate-UnityLicense.ps1` only if the
   old editor starts asking again.

## An extended-LTS patch is a patch this license cannot run

Unity ships some patches past the end of a public LTS line as **extended-LTS (xLTS)** builds, and an xLTS
build is entitlement-gated: it needs Unity Industry or Unity Enterprise. On a Personal license it refuses
before it opens anything, which reads like a silent failure rather than a licensing one:

```text
[Licensing::Module] Error: 'com.unity.editor.access.xlts' was not found.
This build of Unity 2021 is part of an Extended LTS release, which requires either a valid Unity
Industry or Unity Enterprise license.
Application will terminate with return code 198
```

Measured on this machine with `2021.3.58f1`: **return code 198**, with `ProjectVersion.txt` left at its
previous value, nothing under `Assets\` touched and no `Temp\UnityLockfile` created, so the project was
never opened - the log is kept at `artifacts\logs\hop4-apiupdate.log`. `2022.3.76f1`
(`Unity 2022.3 Extended LTS`) carries the same gate; `6000.6.2f1` does not (`"entitlements":[]`).

The entitlement is readable without launching the editor, out of the version folder Hub watches:

```powershell
scripts\Test-UnityEditorUsable.ps1                    # the version ProjectVersion.txt names
scripts\Test-UnityEditorUsable.ps1 -Version 2022.3.76f1
```

It reports the build's product version and entitlements, the package versions its own
`Editor\Data\Resources\PackageManager\Editor\manifest.json` ships and recommends, whether the Mono 4.5
profile still carries `mcs.exe` (the runtime compiler plugin is built with it) and whether the Windows
standalone playback engine is present, then a verdict. **The exit code is the answer** - 0 usable, 1
blocked or not installed - because a blocked verdict is still non-empty text. An install made by the
official installer rather than by Hub has no `metadata.hub.json` at all, and that is not the same thing
as being entitled: the `2020.3.49f1` install here is one of those.

Which patch of an LTS line is the usable one is therefore **not** the newest one. `2021.3.45f2` is the
last public patch of 2021 LTS, and `2021.3.58f1` is the xLTS patch after it.

## Verified facts about the environment

- Editor: `%ProgramFiles%\Unity\Hub\Editor\2018.1.9f2\Editor\Unity.exe`, FileVersion
  `2018.1.9.10931241` — matches the game's `VaM_Data\UnityPlayer.dll`. That is the editor the *game*
  was built with. The project itself is built by `2021.3.45f2`
  (`%ProgramFiles%\Unity\Hub\Editor\2021.3.45f2\Editor\Unity.exe`), which is the version
  `VaM_Rebuild\ProjectSettings\ProjectVersion.txt` names and the one every script launches
  (the hops before it, `2020.3.49f1` and `2019.4.41f2`, and the first one, `2018.4.36f1`, are all
  still installed alongside it). `2021.3.58f1` and `2022.3.76f1` are also installed and are **not**
  usable here - they are extended-LTS, see above.
- Unity 6000.6.0f1 is installed alongside — not suitable for this project (different API, different serialization).
- Hub CLI (`Unity Hub.exe -- --headless`) only supports `editors`, `install-path`, `install`,
  `install-modules` — there is no "open project" command, `unityhub://` only opens OAuth login.
  The Hub REST API on `::39000` requires a token (all requests — 401).
- `-licensingIpc` is parsed in 2018.1, but does not grant a license: the Hub↔editor link appeared
  in 2019.2+. That is why the way it was at 11:50 no longer reproduces.
- Breakdown of compilation errors from the log: `tools\parse_unity_log.py` (categories `syntax`,
  `missing-assembly`, `unity-api`, `other`, deduplication, markdown + json).
- The raw logs of all probes are in `artifacts\licensing\` (`lic-credprobe.log` — proof of the HTTP 401
  from `core.cloud.unity3d.com/api/login`, `unity-gui*.log`, `unity-batch-*.log`, `alf*.log`).
  License backup — `artifacts\backup\Unity_lic.ulf.bak`.
