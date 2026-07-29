---
name: code-guideline-check
description: "Review C# files against this project's coding conventions (Assets/Scripts/CodeGuidelines.md) and Clean Architecture layer rules (Assets/Scripts/DesignPhilosophy.md). Use after writing or editing any C# file under Assets/Scripts or Assets/Editor, or whenever the user asks for a style review, architecture review, layer-dependency check, or general code review of C# changes. Report violations with file:line references, not just prose."
---

# Code Guideline Check

Review C# source against two project documents:

- [Assets/Scripts/CodeGuidelines.md](../../../Assets/Scripts/CodeGuidelines.md) — naming, ordering, comments, formatting
- [Assets/Scripts/DesignPhilosophy.md](../../../Assets/Scripts/DesignPhilosophy.md) — Clean Architecture layers and class-role conventions

Read both files first (they're short) before reviewing — don't rely on the summary below, it can drift out of date.

## Scope

Only review files the user actually touched (recently edited/added), not the whole codebase, unless explicitly asked for a full sweep. Skip generated files, `*.designer.cs`, and third-party code under `Assets/Plugins` or similar.

## What to check (CodeGuidelines.md)

1. **File encoding**: `.cs`/`.md`/`.txt` must be UTF-8.
2. **Member order** inside a class/struct — check top-to-bottom against this exact sequence: constructor → events → public properties → interface properties → public constants → public methods → public interface methods → public enum → public class → public struct → private constants → `[SerializeField]` fields → private fields → Unity lifecycle methods (Awake/Start/Update/...) → `Handle`-prefixed event handlers → protected/virtual methods → private methods → internal/extracted helper methods → private enum → private class → private struct → debug members.
3. **XML doc summaries**: every method needs one; public properties/events need one. One-line summary format is `/// <summary> comment </summary>` (single line, spaces inside). Multi-line/class/method summaries use the indented multi-line form. Comments are in Japanese and end with `。`.
4. **Inline comments**: method bodies should have comments where the logic isn't self-evident.
5. **`[SerializeField]` fields**: must carry a `[Tooltip]`.
6. **Naming**: fields = `_camelCase`; classes/properties/methods = `PascalCase`; parameters = `camelCase`; constants = `UPPER_SNAKE_CASE`; events prefixed `On`; event handler methods suffixed `Handler`; interfaces prefixed `I`; methods start with a verb; `bool` properties start with `Is`/`Has`; uxml/uss files use lower-chain-case; namespaces mirror the layer folder path with numeric prefixes stripped (e.g. `1.Domain/InGame/Skill` → `KillChord.Runtime.Domain.InGame.Skill`).
7. **`using` directives**: grouped at the top of the file.
8. **Encapsulation**: fields exposed via properties, not raw public fields; mutation goes through an intent-revealing method (`SetXxx`/`RecordXxx`), never `public set`.
9. **Explicit access modifiers** everywhere.
10. **No magic numbers** — extract to named constants.
11. **Braces always present**, even for one-line blocks.
12. **One public type per file**, filename matches the type name (nested private types are the exception).
13. **Logging**: `Debug.LogError`/`Debug.LogWarning` messages formatted as `[{nameof(ClassName)}] メッセージ`; pass `this` as the second arg when called from a `MonoBehaviour`.
14. **ScriptableObject data containers** (`[CreateAssetMenu]`): external code should only read via properties backed by `[SerializeField]`, not mutate.

## What to check (DesignPhilosophy.md)

1. **Layer dependency direction**: Domain (no deps) → Application (Domain) → Adaptor (Domain, Application) → View (Adaptor) → InfraStructure (Domain, Application, View) → Composition (all). A lower layer must never reference a higher one. Cross-module references are only allowed from the Adaptor layer, resolved by Composition via DI.
2. **Unity dependency rule**: pure layers (Domain/Application/Adaptor) may `using UnityEngine` for value types (Vector3 etc.) but must not inherit `MonoBehaviour` or depend on the Unity lifecycle.
3. **Class-role conventions** — flag mismatches between a class's apparent purpose and its layer/shape:
   - Domain `Entity`: mutable reference type, `class`, public props read-only, changes via intent-revealing methods (e.g. `ChangeValue`).
   - Domain `ValueObject`: immutable, `readonly struct`, implements `IEquatable<T>`/`IComparable<T>` with operator overloads where relevant.
   - Application `Factory`: builds Application/Domain instances via the Factory pattern.
   - Application `IRepository`: abstract only in Application; concrete implementation lives in InfraStructure.
   - Adaptor `Presenter`: Query side (View reads).
   - Adaptor `Controller`: Command side (View writes / triggers).
   - Adaptor `State`: persistent data state.
   - Adaptor `DTO`: `readonly ref struct`, carries data from Adaptor to ViewModel.
   - Adaptor `Registory`: pairs Domain objects with their View counterparts.
   - View `ViewModel`: holds display data, ReactiveProperty-style, methods take DTOs `in`.
   - View `Signal`: event bus, methods take DTOs `in`.
   - View `Spawner`: builds View objects via Factory pattern.
   - View `Config`: `ScriptableObject`, view-only settings (use InfraStructure `Asset` instead if the data affects domain logic).
   - InfraStructure `Asset`: `ScriptableObject` data-entry class; if the type has variants, use an abstract base (`XxxAssetBase`) with `[SerializeReference, SubclassSelector]` and an `abstract Create()` that maps to the Domain type.
   - InfraStructure `Repository`: implements the Application-layer `IRepository`.
   - Composition `Initializer`: extends `InGameInitializationModuleBase` / `OutGameInitializationModuleBase` / `PersistentInitializationModuleBase`; lifecycle is `Init()` → `ResourceLoadAsync(CancellationToken)` → `Build()` → `Ready()`, run in `Order` ascending by `InitializationCoordinator<TModule>`; `Shutdown()` runs in reverse registration order.
   - Composition `Container`: registers services via `ServiceLocator.RegisterInstance`; other modules fetch via `ServiceLocator.TryGetInstance<T>()` — cross-module wiring goes through this, not constructor injection.
   - Composition `Debugger`: editor-only functionality.
   - Async return types: Composition module lifecycle methods return `Awaitable`/`Awaitable<T>`; Application/Adaptor use cases and scene transitions return `Task<T>`/`ValueTask<T>`; `Awaitable` in Application is allowed only where it bridges to a Composition-layer caller.

## Output format

Report as a list of findings, each with `file:line`, the rule violated, and a one-line fix suggestion. Group by file. If a file is fully compliant, say so briefly instead of omitting it — the user needs to know the check actually ran.

Don't rewrite files unless the user asks you to fix the issues — default to reporting.
