# Augur 自動生成テスト計画 (2026-09-06)

- 生成: `augur tests plan --repo SymphonyKillChord --analysis <PrDiffReview> --no-impact`
- 入力: `anatomia pr-review --base HEAD~20` (Assets/Scripts 44ファイル変更)
- planId: `plan_01M1VR4ZQP8NNJXJENGNPGT7VY` / headSha: `0a624fa2f` / status: **ready**
- **targets 53 件採用 / 162 件 quota で不採用 / blockers 0**
- brief は `test-briefs.json` (author=session)。テスト本文はセッション側で書く前提
- **1 target が 2 つのビジネスドメインに属することがある** (10 件)。下の一覧では該当ドメイン両方に現れるため、ドメイン別の合計は targets 総数より多くなる

## quota の消化

| ビジネスドメイン | max | 既存 active | 本計画 planned |
|---|---|---|---|
| (unowned) | 12 | 0 | 12 |
| Guidance, Feedback and Recovery | 10 | 0 | 10 |
| Mission Evaluation and Result | 10 | 0 | 8 |
| Musical Time and Adaptive Arrangement | 16 | 0 | 1 |
| Narrative and Game Flow | 10 | 0 | 10 |
| Persistence and Player Settings | 16 | 0 | 6 |
| Progression, Research and Loadout | 16 | 0 | 16 |

## 採用された target

### Progression, Research and Loadout (16)

| 優先度 | 種別 | 対象シンボル | 実装ファイル | テスト置き先 | runtime |
|---|---|---|---|---|---|
| high | assurance | `Initialize` | `6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs:214` | `EditMode/…/6.Composition/OutGame/SkillTree/SkillTreeInitializerTests.cs` | ✓ |
| high | assurance | `Initialize` | `6.Composition/OutGame/SkillBuild/SkillBuildInitializer.cs:136` | `EditMode/…/6.Composition/OutGame/SkillBuild/SkillBuildInitializerTests.cs` | ✓ |
| medium | assurance | `RegisterScreenGeometryCallback` | `4.View/OutGame/SkillTree/SkillTreeViewportView.cs:406` | `EditMode/…/4.View/OutGame/SkillTree/SkillTreeViewportViewTests.cs` |  |
| medium | assurance | `SaveSkillUnlockData` | `2.Application/OutGame/SkillTree/SkillTreeService.cs:175` | `EditMode/…/2.Application/OutGame/SkillTree/SkillTreeServiceTests.cs` |  |
| medium | assurance | `PrepareFocusAfterLayout` | `4.View/OutGame/SkillTree/SkillTreeViewportView.cs:188` | `EditMode/…/4.View/OutGame/SkillTree/SkillTreeViewportViewTests.cs` |  |
| medium | assurance | `IsFinite` | `4.View/OutGame/SkillTree/SkillTreeViewportView.cs:398` | `EditMode/…/4.View/OutGame/SkillTree/SkillTreeViewportViewTests.cs` | ✓ |
| medium | assurance | `TryGetNearestLockedNodeIdsFromStart` | `2.Application/OutGame/SkillTree/SkillTreeService.cs:73` | `EditMode/…/2.Application/OutGame/SkillTree/SkillTreeServiceTests.cs` |  |
| medium | assurance | `Dispose` | `4.View/OutGame/SkillBuild/SkillElementControllerEquipController.cs:94` | `EditMode/…/4.View/OutGame/SkillBuild/SkillElementControllerEquipControllerTests.cs` | ✓ |
| medium | assurance | `TryBuildStageMapElements` | `6.Composition/OutGame/StageSelect/StageSelectInitializer.cs:550` | `EditMode/…/6.Composition/OutGame/StageSelect/StageSelectInitializerTests.cs` |  |
| medium | assurance | `IsElementVisible` | `4.View/OutGame/SkillTree/SkillTreeViewportView.cs:356` | `EditMode/…/4.View/OutGame/SkillTree/SkillTreeViewportViewTests.cs` |  |
| medium | assurance | `Subscribe` | `6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs:547` | `EditMode/…/6.Composition/OutGame/SkillTree/SkillTreeInitializerTests.cs` | ✓ |
| medium | assurance | `ResetSkillTreeAsync` | `2.Application/OutGame/SkillTree/SkillTreeService.cs:251` | `EditMode/…/2.Application/OutGame/SkillTree/SkillTreeServiceTests.cs` |  |
| medium | assurance | `SetupSkillElement` | `6.Composition/OutGame/SkillBuild/SkillBuildInitializer.cs:211` | `EditMode/…/6.Composition/OutGame/SkillBuild/SkillBuildInitializerTests.cs` |  |
| medium | assurance | `ExecutePendingNodeTransitionAsync` | `6.Composition/OutGame/StageSelect/StageSelectInitializer.cs:955` | `EditMode/…/6.Composition/OutGame/StageSelect/StageSelectInitializerTests.cs` |  |
| medium | assurance | `Push` | `3.Adaptor/OutGame/SkillTree/SkillTreeFocusPresenter.cs:31` | `EditMode/…/3.Adaptor/OutGame/SkillTree/SkillTreeFocusPresenterTests.cs` | ✓ |
| low | assurance | `HandleSkillTreeScreenShownHandler` | `6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs:673` | `EditMode/…/6.Composition/OutGame/SkillTree/SkillTreeInitializerTests.cs` |  |

### (unowned) (12)

| 優先度 | 種別 | 対象シンボル | 実装ファイル | テスト置き先 | runtime |
|---|---|---|---|---|---|
| high | assurance | `Disable` | `Assets/Settings/Input/KillChordInputActioMap.cs:1809` | `Assets/Tests/EditMode/Assets/Settings/Input/KillChordInputActioMapTests.cs` | ✓ |
| high | assurance | `Disable` | `Assets/Settings/Input/KillChordInputActioMap.cs:1626` | `Assets/Tests/EditMode/Assets/Settings/Input/KillChordInputActioMapTests.cs` | ✓ |
| high | assurance | `Enable` | `Assets/Settings/Input/KillChordInputActioMap.cs:1624` | `Assets/Tests/EditMode/Assets/Settings/Input/KillChordInputActioMapTests.cs` | ✓ |
| high | assurance | `Disable` | `Assets/Settings/Input/KillChordInputActioMap.cs:1489` | `Assets/Tests/EditMode/Assets/Settings/Input/KillChordInputActioMapTests.cs` | ✓ |
| high | assurance | `Enable` | `Assets/Settings/Input/KillChordInputActioMap.cs:1487` | `Assets/Tests/EditMode/Assets/Settings/Input/KillChordInputActioMapTests.cs` | ✓ |
| high | assurance | `Disable` | `Assets/Settings/Input/KillChordInputActioMap.cs:1382` | `Assets/Tests/EditMode/Assets/Settings/Input/KillChordInputActioMapTests.cs` | ✓ |
| high | assurance | `Initialize` | `4.View/InGame/Camera/CameraSystemView.cs:45` | `EditMode/…/4.View/InGame/Camera/CameraSystemViewTests.cs` | ✓ |
| high | assurance | `Enable` | `Assets/Settings/Input/KillChordInputActioMap.cs:1380` | `Assets/Tests/EditMode/Assets/Settings/Input/KillChordInputActioMapTests.cs` | ✓ |
| high | assurance | `Enable` | `Assets/Settings/Input/KillChordInputActioMap.cs:1807` | `Assets/Tests/EditMode/Assets/Settings/Input/KillChordInputActioMapTests.cs` | ✓ |
| medium | assurance | `ExcludeFromNavigation` | `4.View/OutGame/Navigation/UINavigationExtensions.cs:144` | `EditMode/…/4.View/OutGame/Navigation/UINavigationExtensionsTests.cs` | ✓ |
| medium | assurance | `EnableSubmitAsClick` | `4.View/OutGame/Navigation/UINavigationExtensions.cs:72` | `EditMode/…/4.View/OutGame/Navigation/UINavigationExtensionsTests.cs` | ✓ |
| medium | assurance | `Activate` | `4.View/OutGame/Navigation/ModalNavigationScope.cs:26` | `EditMode/…/4.View/OutGame/Navigation/ModalNavigationScopeTests.cs` | ✓ |

### Narrative and Game Flow (10)

| 優先度 | 種別 | 対象シンボル | 実装ファイル | テスト置き先 | runtime |
|---|---|---|---|---|---|
| medium | assurance | `Show` | `6.Composition/OutGame/Title/TitleScreenViewRegistry.cs:41` | `EditMode/…/6.Composition/OutGame/Title/TitleScreenViewRegistryTests.cs` | ✓ |
| medium | assurance | `Show` | `4.View/OutGame/Screen/ScreenViewBase.cs:46` | `EditMode/…/4.View/OutGame/Screen/ScreenViewBaseTests.cs` | ✓ |
| medium | assurance | `Dispose` | `4.View/OutGame/Screen/ScreenViewBase.cs:104` | `EditMode/…/4.View/OutGame/Screen/ScreenViewBaseTests.cs` | ✓ |
| medium | assurance | `Show` | `4.View/OutGame/Screen/SettingScreenView.cs:35` | `EditMode/…/4.View/OutGame/Screen/SettingScreenViewTests.cs` | ✓ |
| medium | assurance | `Dispose` | `4.View/OutGame/Title/OptionsScreenView.cs:28` | `EditMode/…/4.View/OutGame/Title/OptionsScreenViewTests.cs` | ✓ |
| medium | assurance | `Dispose` | `4.View/OutGame/Title/TitleSceneView.cs:60` | `EditMode/…/4.View/OutGame/Title/TitleSceneViewTests.cs` | ✓ |
| medium | assurance | `Dispose` | `6.Composition/OutGame/Title/TitleScreenViewRegistry.cs:127` | `EditMode/…/6.Composition/OutGame/Title/TitleScreenViewRegistryTests.cs` | ✓ |
| medium | assurance | `Hide` | `6.Composition/OutGame/Screen/ScreenViewRegistry.cs:66` | `EditMode/…/6.Composition/OutGame/Screen/ScreenViewRegistryTests.cs` | ✓ |
| medium | assurance | `Dispose` | `6.Composition/OutGame/Screen/ScreenViewRegistry.cs:115` | `EditMode/…/6.Composition/OutGame/Screen/ScreenViewRegistryTests.cs` | ✓ |
| medium | assurance | `Show` | `6.Composition/OutGame/Screen/ScreenViewRegistry.cs:39` | `EditMode/…/6.Composition/OutGame/Screen/ScreenViewRegistryTests.cs` | ✓ |

### Guidance, Feedback and Recovery (10)

| 優先度 | 種別 | 対象シンボル | 実装ファイル | テスト置き先 | runtime |
|---|---|---|---|---|---|
| medium | assurance | `RegisterScreenGeometryCallback` | `4.View/OutGame/SkillTree/SkillTreeViewportView.cs:406` | `EditMode/…/4.View/OutGame/SkillTree/SkillTreeViewportViewTests.cs` |  |
| medium | assurance | `Show` | `4.View/OutGame/Screen/ScreenViewBase.cs:46` | `EditMode/…/4.View/OutGame/Screen/ScreenViewBaseTests.cs` | ✓ |
| medium | assurance | `PrepareFocusAfterLayout` | `4.View/OutGame/SkillTree/SkillTreeViewportView.cs:188` | `EditMode/…/4.View/OutGame/SkillTree/SkillTreeViewportViewTests.cs` |  |
| medium | assurance | `Dispose` | `4.View/OutGame/Screen/ScreenViewBase.cs:104` | `EditMode/…/4.View/OutGame/Screen/ScreenViewBaseTests.cs` | ✓ |
| medium | assurance | `IsFinite` | `4.View/OutGame/SkillTree/SkillTreeViewportView.cs:398` | `EditMode/…/4.View/OutGame/SkillTree/SkillTreeViewportViewTests.cs` | ✓ |
| medium | assurance | `Show` | `4.View/OutGame/Screen/SettingScreenView.cs:35` | `EditMode/…/4.View/OutGame/Screen/SettingScreenViewTests.cs` | ✓ |
| medium | assurance | `Dispose` | `4.View/OutGame/SkillBuild/SkillElementControllerEquipController.cs:94` | `EditMode/…/4.View/OutGame/SkillBuild/SkillElementControllerEquipControllerTests.cs` | ✓ |
| medium | assurance | `Dispose` | `4.View/OutGame/Title/OptionsScreenView.cs:28` | `EditMode/…/4.View/OutGame/Title/OptionsScreenViewTests.cs` | ✓ |
| medium | assurance | `IsElementVisible` | `4.View/OutGame/SkillTree/SkillTreeViewportView.cs:356` | `EditMode/…/4.View/OutGame/SkillTree/SkillTreeViewportViewTests.cs` |  |
| medium | assurance | `Dispose` | `4.View/OutGame/Title/TitleSceneView.cs:60` | `EditMode/…/4.View/OutGame/Title/TitleSceneViewTests.cs` | ✓ |

### Mission Evaluation and Result (8)

| 優先度 | 種別 | 対象シンボル | 実装ファイル | テスト置き先 | runtime |
|---|---|---|---|---|---|
| medium | assurance | `Show` | `4.View/InGame/Result/StageResultView.cs:39` | `EditMode/…/4.View/InGame/Result/StageResultViewTests.cs` | ✓ |
| medium | assurance | `Hide` | `4.View/InGame/Result/StageResultView.cs:64` | `EditMode/…/4.View/InGame/Result/StageResultViewTests.cs` | ✓ |
| low | assurance | `SelectButton` | `4.View/InGame/Result/StageResultView.cs:604` | `EditMode/…/4.View/InGame/Result/StageResultViewTests.cs` |  |
| low | assurance | `RebuildSubMissionItems` | `4.View/InGame/Result/StageResultView.cs:541` | `EditMode/…/4.View/InGame/Result/StageResultViewTests.cs` |  |
| low | assurance | `RestoreInteraction` | `4.View/InGame/Result/StageResultView.cs:592` | `EditMode/…/4.View/InGame/Result/StageResultViewTests.cs` |  |
| low | assurance | `OnRetryButtonClicked` | `4.View/InGame/Result/StageResultView.cs:114` | `EditMode/…/4.View/InGame/Result/StageResultViewTests.cs` |  |
| low | assurance | `ClearSelection` | `4.View/InGame/Result/StageResultView.cs:620` | `EditMode/…/4.View/InGame/Result/StageResultViewTests.cs` |  |
| low | assurance | `OnCompleteButtonClicked` | `4.View/InGame/Result/StageResultView.cs:82` | `EditMode/…/4.View/InGame/Result/StageResultViewTests.cs` |  |

### Persistence and Player Settings (6)

| 優先度 | 種別 | 対象シンボル | 実装ファイル | テスト置き先 | runtime |
|---|---|---|---|---|---|
| medium | assurance | `Dispose` | `4.View/OutGame/Setting/SettingCategoryView.cs:39` | `EditMode/…/4.View/OutGame/Setting/SettingCategoryViewTests.cs` | ✓ |
| low | assurance | `RegisterCallbacks` | `4.View/OutGame/Setting/SettingCategoryView.cs:68` | `EditMode/…/4.View/OutGame/Setting/SettingCategoryViewTests.cs` |  |
| low | assurance | `HandleSystemCategoryClickedHandler` | `4.View/OutGame/Setting/SettingCategoryView.cs:87` | `EditMode/…/4.View/OutGame/Setting/SettingCategoryViewTests.cs` |  |
| low | assurance | `Require` | `4.View/OutGame/Setting/SettingCategoryView.cs:128` | `EditMode/…/4.View/OutGame/Setting/SettingCategoryViewTests.cs` |  |
| low | assurance | `SettingCategoryView` | `4.View/OutGame/Setting/SettingCategoryView.cs:14` | `EditMode/…/4.View/OutGame/Setting/SettingCategoryViewTests.cs` |  |
| low | assurance | `HandleSoundCategoryClickedHandler` | `4.View/OutGame/Setting/SettingCategoryView.cs:78` | `EditMode/…/4.View/OutGame/Setting/SettingCategoryViewTests.cs` |  |

### Musical Time and Adaptive Arrangement (1)

| 優先度 | 種別 | 対象シンボル | 実装ファイル | テスト置き先 | runtime |
|---|---|---|---|---|---|
| medium | assurance | `Initialize` | `4.View/Persistent/Music/MusicPlayer.cs:33` | `EditMode/…/4.View/Persistent/Music/MusicPlayerTests.cs` | ✓ |

> テスト置き先は `.augur/tests.config.json` の `layout.newTestPath` (既定 EditMode) から決まる。
> **PlayMode / Performance で回すケースは、生成後に `Assets/Tests/PlayMode/` 配下へ移す**
> ([machine-test-runner-design.md](../../../test/machine-test-runner-design.md) §3)。
