---
name: unity-autobuilder
description: "Run or troubleshoot local player builds via this project's AutoBuilder tooling (Assets/Editor/Scripts/AutoBuilder), and explain how local builds relate to the BuildAndRelease.yml CI workflow. Use when the user wants to build the game locally (Master or Develop), asks about BuildProfiles/AutoBuilderSettings, wants to reproduce or debug a CI build failure locally, or asks how the automated GitHub Actions release build works."
---

# Unity AutoBuilder

AutoBuilder runs one or more Unity `BuildProfile`s back-to-back and writes each to its own
output subfolder. It has three code paths sharing the same core logic
(`AutoBuildExecuter.CreateBuildPlayerOptions` / `GetExtension`):

- `AutoBuildWindow` — a menu-driven EditorWindow for one-off local builds.
- `AutoBuildExecuter` — the domain-reload-safe runner the window uses (survives script recompiles mid-build-sequence via `SessionState`).
- `AutoBuilder.RunFromCli` — the batch-mode entry point CI calls directly, no window/domain-reload dance needed since CI runs Unity once and exits.

Read `Assets/Editor/Scripts/AutoBuilder/AutoBuilder.cs`, `AutoBuildExecuter.cs`, `AutoBuildWindow.cs`, and `AutoBuilderSettings.cs` before making changes — this doc summarizes them but the source is authoritative.

## Running a build from the Editor (window)

1. Open the window: menu item at `ToolConst.TOOLS_PATH + "AutoBuilder"` (`AutoBuildWindow.ShowWindow`).
2. Click one of the four slot buttons — **"Release Master Build"**, **"Release Development Build"**, **"Demo Master Build"**, **"Demo Development Build"**. Each button is replaced by an error HelpBox if that slot's output path (`AutoBuilderSettings.<Variant><Mode>Path`) isn't set or doesn't end in a slash, or its profile list is empty/has nulls/duplicates.
3. This calls `AutoBuildExecuter.Run(path, profiles)`, which builds every `BuildProfile` registered in that slot in order, each to `path/<ProfileName>/<ProductName><ext>`. Before activating a Demo profile it calls `GameDataVariantProfiles.SynchronizeDemoScenes`, which overwrites the profile's scene list with the global Build Settings list plus `Assets/Level/Scenes/Demo/DemoEnd.unity` enabled. If a sequence is already running (`AutoBuildExecuter.IsRunning`), all buttons are disabled — only one sequence runs at a time.
4. The sequence survives domain reloads between profiles: state lives in `SessionState` (`BuildSession`), errors/exceptions/asserts logged during the run are captured and replayed to the Console after a reload wipes it (`PendingLogReplay`), and the build profile active before the run started is restored afterward regardless of success/failure.
5. A dialog reports success/failure at the end (skipped in batch mode).

`AutoBuilderSettings` (`ProjectSettings/KillChord/AutoBuilderSettings.asset`, editable under Project Settings > KillChord > AutoBuilder) has one slot per game data variant × build mode: `ReleaseMasterPath`/`ReleaseMasterBuildProfiles`, `ReleaseDevelopPath`/`ReleaseDevelopBuildProfiles`, `DemoMasterPath`/`DemoMasterBuildProfiles`, `DemoDevelopPath`/`DemoDevelopBuildProfiles`. The Release fields were renamed from `MasterPath`/`MasterBuildProfiles`/`DevelopPath`/`DevelopBuildProfiles` (`[FormerlySerializedAs]` keeps old data loading).

## Build Profiles and game data variant

- Profiles live in `Assets/Settings/Build Profiles/` and are named `<Variant>_<Mode>_<Platform>`: `Release_Dev_Windows`, `Release_Master_Windows`, `Demo_Dev_Windows`, `Demo_Master_Windows`, and the same for `Android` / `macOS` / `iOS` (16 in total).
- The **Build Profile is the source of truth for the variant**: a profile is Demo if its own Scripting Defines contain `KILLCHORD_DEMO` (`GameDataVariantProfiles.GetVariant`). The name is for humans only. Nothing writes `KILLCHORD_DEMO` into global Player Settings anymore.
- Addressables `GameData.Release` / `GameData.Demo` IncludeInBuild follows the active profile: `GameDataVariantBuildSettings.ApplyActiveProfile` runs after every domain reload, and `GameDataVariantBuildPreprocessor` re-applies it for the active profile right before a player build.
- Switch the active profile from the main toolbar dropdown next to the Play button (`KillChord.Editor.Build.BuildProfileToolbar`). The menu groups profiles by `_` as `Variant/Mode/Platform` and asks for confirmation when the platform changes.
- `GameDataVariantEditorState` (EditorPrefs) is now only the Planner Master Data Window's view selection; it does not affect builds.

## Running a build from the command line (reproducing CI locally)

CI invokes:
```
Unity.exe -batchmode -projectPath <repo> -executeMethod KillChord.Editor.AutoBuilder.AutoBuilder.RunFromCli -gameDataVariant <release|demo> -buildMode <Development|Master> [-selectedProfiles Windows,Android] -logFile unity_build.log
```
To reproduce a CI build failure locally, run the same command from a shell with your local Unity Editor executable. Notes:
- `-gameDataVariant` is required (`release` or `demo`) and `-buildMode` (`Development` or `Master`) picks the mode inside that variant; omitting `-buildMode` builds both modes of the variant. Together they select exactly one `AutoBuilderSettings` slot.
- Every profile in the selected slot must match the slot's variant (Demo slot ⇔ `KILLCHORD_DEMO` in the profile's defines); otherwise the run fails before building.
- `-selectedProfiles` filters by (normalized, partial) profile name within the slot, so passing platform names like `Windows` is unambiguous.
- Output directory: `AutoBuilder.ExecuteBuildForProfile` reads the `UNITY_BUILD_OUTPUT_DIR` environment variable if set (CI sets it to `Builds/<BuildMode>`), otherwise defaults to `<project>/../Builds`. Set this env var before running locally if you want to match CI's layout exactly.
- Unlike the window/`AutoBuildExecuter` path, `RunFromCli` does NOT persist state across domain reloads — it assumes a single Unity process that runs straight through and calls `EditorApplication.Exit(exitCode)` when done (exit code 0 = all profiles succeeded, 1 = at least one failed or an exception was thrown).
- Read `unity_build.log` (or the console) for `[AutoBuilder] Building profile: ...`, `[Success] ... : N bytes`, `[Failed] ... : <BuildResult>` lines to find which profile failed.

## Relationship to `.github/workflows/BuildAndRelease.yml`

The CI workflow (`build-and-release` job, self-hosted runner) does, in order: checks whether a release for the target branch's tag prefix (`dev-`/`release-`) was published within the last 3 days (skips the run if so, unless manually triggered) → determines `BuildMode` from the PR base branch (`develop` → `Development`/`dev-`, `main` → `Master`/`release-`) → downloads and installs `.unitypackage` dependencies from a Google Drive-hosted zip → restores NuGet packages via `NuGetForUnity.Cli` → runs the CLI command above (`AutoBuilder.RunFromCli`) with a 90-minute timeout → zips the build output → creates a GitHub Release tagged `<prefix>-v1.0.<counter>` with `Builds.zip` attached, incrementing a repo variable counter.

Triggers: PR merged into `develop` or `main`, or manual `workflow_dispatch` with a `build_mode` choice. It does not run on every push — only on merge or manual dispatch.

If a local `RunFromCli` reproduction succeeds but CI fails, suspect environment differences first: missing/stale `.unitypackage` dependencies (CI fetches them fresh from Google Drive every run), NuGet restore state, or the `UNITY_BUILD_OUTPUT_DIR`/`UNITY_EXE_PATH` secrets — not the build logic itself.
