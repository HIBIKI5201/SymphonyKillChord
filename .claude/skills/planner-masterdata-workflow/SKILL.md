---
name: planner-masterdata-workflow
description: "Add, edit, or troubleshoot planner-facing master data via the SourceDataProvider system (Assets/Editor/Scripts/SourceDataProvider) and the Planner Master Data window. Use whenever the user wants to add a new master data type/entry, register a ScriptableObject with Addressables for planner editing, work with DataID/CollectionKey/hash IDs, debug 'not registered in SourceDataProvider' or hash-mismatch warnings, or extend the Planner Master Data window's pages. Also covers adding a brand-new SourceData collection type from scratch."
---

# Planner Master Data Workflow

This project lets non-programmer planners edit master data (ScriptableObjects) through a
custom EditorWindow instead of the raw Inspector. The system has three moving parts:

1. **SourceDataProviderSettings** (`Assets/Editor/Scripts/SourceDataProvider/SourceDataProviderSettings.Editor.cs`) — the registry: which Addressable ScriptableObjects ("SourceAssets") exist, and which array/List fields on them ("collections") are planner-editable.
2. **PlannerMasterDataEditorSettings** (`.../PlannerMasterDataEditorSettings.cs`) — groups SourceAssets/collections into sidebar "Pages" for the window.
3. **PlannerMasterDataWindow** (`.../PlannerMasterDataWindow.cs`) — the actual EditorWindow UI, opened via the `EditorWindowPathConst.PLANNER_MASTER_DATA_WINDOW_PATH` menu item.

Read the four files above before making changes — this doc summarizes the wiring, but the source is the ground truth.

## Core concepts

- **SourceAsset**: an Addressable-registered `ScriptableObject` (e.g. `Player`, `StageTreeAsset`, `CharacterDefinitionRepository`). Identified by its **Addressable key** (the address string in Addressables Groups).
- **Collection**: a `List<T>`/array field inside a SourceAsset that holds individually-addable/removable items (e.g. `StageAsset` items inside `StageTreeAsset._stageAssets`). Identified by a **CollectionKey** string, mapped to `(SourceAssetAddressableKey, PropertyPath, AssetCreationDirectory)` via `SourceDataProviderSettings.SourceCollectionMapping`.
- **DataID** (`Assets/Scripts/Runtime/0.Utility/Identity/DataID.cs`): the value type used on individual data entries. Holds a human-readable string `_id` plus a baked-in int hash (`DataIDHasher.Compute(collectionKey, id)`), rendered via `DataIDPropertyDrawer`. A field of type `DataID` marked with `[SourceDataCollection(CollectionKey)]` is either an *authoring* field (defines a new ID inside its own collection) or a *reference* field (picks an existing ID from another collection) — the drawer tells them apart via `SourceDataProviderRepositoryResolver.IsAuthoringProperty`.
- **SourceDataAddressAttribute**: put on a `string` field to get a dropdown of registered SourceAsset Addressable keys instead of typing the address by hand (rendered by `SourceDataAddressSelectorDrawer`).

## Task: add a new item to an existing collection

This is the common case — planner wants a new Stage, Skill, Character, etc.

1. Open the window: `EditorWindowPathConst.PLANNER_MASTER_DATA_WINDOW_PATH` menu item, or call `PlannerMasterDataWindow.TryGetOrOpenWindow`.
2. Pick the Page containing the target collection, switch to the "Collections" tab, select the CollectionKey.
3. Click **"データを追加"** (`DrawCollectionCommands` → `ShowCollectionCreationMenu`). This either:
   - creates a new `ScriptableObject` asset under `SourceCollectionMapping.AssetCreationDirectory` (if the collection holds asset references), or
   - adds an inline struct/class element directly into the array (if the collection holds plain serialized data).
4. Fill in the new item's fields, including any `DataID` field — type a unique human-readable ID; the hash auto-computes on change. `DataIDPropertyDrawer` shows a warning if the hash is stale or the ID collides with an existing one (`DataIDCollisionDetector`).
5. Assets save automatically on edit via `SaveAssetIfDirty` (dirty-marks + `AssetDatabase.SaveAssetIfDirty`) — no manual save step needed.

To remove an item from a collection (not delete the underlying asset), use **"Collectionから外す"** — it only unlinks it from the array via `Undo.RecordObject` + `DeleteArrayElementAtIndex`, it does not delete the asset file.

## Task: register a brand-new SourceAsset/collection type

When a new master data ScriptableObject type is introduced and needs to appear in the planner window:

1. Make sure the ScriptableObject asset is registered in Addressables with a stable address (see `Assets/AddressableAssetsData`). `SourceDataProviderSettings.RefreshSourceAssetsFromAddressables()` (called on window `OnEnable` and via the window's "Refresh" button) auto-discovers any Addressable entry whose main asset type is a `ScriptableObject` and appends it as a `SourceAssetMapping` — so simply Addressable-registering the asset already makes it show up as a SourceAsset.
2. If the asset has a `List<T>`/array field planners should manage as a collection, register it explicitly: open **Project Settings → KillChord → Source Data Provider** (`SettingsService.OpenProjectSettings("Project/KillChord/Source Data Provider")`, backed by `SourceDataProviderSettingsProvider.cs`) and add a `SourceCollectionMapping` entry with the CollectionKey, the SourceAsset's Addressable key, the field's SerializedProperty path, and (if the element type is a ScriptableObject) an `AssetCreationDirectory`.
3. Add the new SourceAsset key and/or CollectionKey to a `PageDefinition` so it's reachable from the sidebar: **Project Settings → KillChord → Planner Master Data** (`SettingsService.OpenProjectSettings("Project/KillChord/Planner Master Data")`, backed by `PlannerMasterDataEditorSettingsProvider.cs`, data model in `PlannerMasterDataEditorSettings.cs`). Either add to an existing `PageDefinition`'s `SourceAssetAddressableKeys`/`CollectionCategories`, or create a new page.
4. On the data class itself, mark the ID field with `[SourceDataCollection("YourCollectionKey")]` (namespace `KillChord.Runtime.Utility.Identity`) so `DataIDPropertyDrawer` and `SourceDataAddressSelectorDrawer` know how to resolve it.
5. If you need a custom preview panel in the detail pane (like `PlannerEnemyStatusPreview`/`PlannerEnemyWavePreview`/`PlannerStageTreeGraphRenderer`), follow those existing previewer classes as a pattern and wire them into `DrawSourceAssetPreview`/`DrawCollectionPreview` in `PlannerMasterDataWindow.cs` — but this is source-code work, confirm with the user before editing `PlannerMasterDataWindow.cs` itself since it's a shared, sizeable file.

No Addressables build/rebuild step is required just to make new entries visible in the planner window — the window reads directly from the Addressable Groups asset via `AddressableAssetSettingsDefaultObject.Settings`. An Addressables build is only needed when shipping (see the `unity-autobuilder` skill / CI).

## Task: debug ID/collision/hash warnings

`DataIDPropertyDrawer` shows inline warnings (`BuildWarning`) for:
- **"SourceDataCollection属性が設定されていません。"** — the field is missing `[SourceDataCollection(...)]`.
- **"SourceDataProviderにCollectionKeyが登録されていません。"** — the CollectionKey has no `SourceCollectionMapping` in Source Data Provider project settings; register it (see step 2 above).
- **"ハッシュが未更新です。"** — the stored hash doesn't match `DataIDHasher.Compute(collectionKey, id)`; re-trigger the field's change check (e.g. retype the ID) to recompute, or use the `DataIDRebuildMenu` editor menu if present for a bulk rebuild.
- **"選択したIDがcollectionへ登録されていません。"** — a reference field points at an ID that no longer exists in its source collection.

Duplicate/colliding IDs within one CollectionKey are flagged by `DataIDCollisionDetector.FindWarning` on the authoring field.

## Navigating programmatically

`PlannerMasterDataWindow.NavigateToSourceAsset(addressableKey)` and `NavigateToCollectionItem(collectionKey, dataId)` jump the window to a specific asset/item — these are what the "Planner" jump buttons in `DataIDPropertyDrawer`/`SourceDataAddressSelectorDrawer` call. Reuse them rather than reimplementing navigation if building new tooling.
