# 15. GRASP原則

GRASP（General Responsibility Assignment Software Patterns）は
**「その責務を、どのクラスに割り当てるべきか」**を判断するための9原則。

| 原則 | 本プロジェクトの評価 |
| --- | --- |
| Information Expert（情報エキスパート） | 一部違反 |
| Creator（生成者） | 一部違反 |
| Controller（コントローラ） | 名前の濫用あり |
| Low Coupling（疎結合） | 概ね良好、局所的に違反 |
| High Cohesion（高凝集） | Composition層で違反 |
| Polymorphism（多態性） | 概ね良好 |
| Pure Fabrication（純粋人工物） | **良好** |
| Indirection（間接化） | 過剰な箇所あり |
| Protected Variations（変動からの保護） | 概ね良好、2箇所違反 |

---

## Information Expert（情報エキスパート）

> 責務は、その遂行に必要な情報を持つクラスに割り当てる。

### 【良い例】CharacterEntity

**場所**: [CharacterEntity.cs](../../Assets/Scripts/Runtime/1.Domain/InGame/Character/CharacterEntity.cs)（162行）

```csharp
public void ChangeBaseDamage(Damage newDamage)
public void SetDamage(Damage damage)
public void TakeDamage(Damage damage)
public void Heal(Health healAmount)
public void SetInvincible(bool isInvincible)
public void Reset()
```

HPを持つエンティティが、HPの増減・無敵判定・リセットを自分で担っている。
**貧血ドメインモデルに陥っていない**、模範的な実装。

### 【違反】AttackIntervalEntity が自分の状態遷移を持っていない

**場所**: [AttackIntervalEntity.cs](../../Assets/Scripts/Runtime/1.Domain/InGame/Battle/AttackIntervalEntity.cs)（32行）

```csharp
public void UpdateAttackState(bool isAttacking)   // 唯一のメソッド
```

`IsAttacking` と `Interval` という情報を持ちながら、
**「攻撃してから Interval 経過したら IsAttacking を false にする」というルールは
外部の `AttackIntervalEvaluator`（Application層）が持っている**。

```csharp
// 2.Application/InGame/Battle/AttackIntervalEvaluator.cs:50-60
_attackIntervalEntity.UpdateAttackState(true);
await UniTask.Delay((int)(duration * 1000f));
if (attackId == _currentIntervalId)
{
    _attackIntervalEntity.UpdateAttackState(false);
}
```

その結果、`_currentIntervalId` による多重実行防止という**Entityの一貫性に関わる知識**が
Evaluator側に漏れ出している。Entityは単なるboolの入れ物になっており、
`UpdateAttackState(true)` を誰でも好きなタイミングで呼べてしまう。

**修正方針**: 硬直の開始・経過判定をEntity側へ寄せる。
時間はEntityが持たず、`Tick(float deltaTime)` または
「攻撃開始時刻」を受け取って `IsAttacking` を算出する形にすれば、
非同期待機そのものが不要になる（[11. 非同期処理](11_非同期処理の不純点.md) の指摘も同時に解消）。

---

## Creator（生成者）

> オブジェクトの生成責務は、それを集約する／使用する／初期化データを持つクラスに割り当てる。

### 【違反】Adaptor層がグローバル状態を生成・登録している

**場所**: [BattleSortieSelectionService.cs:43-67](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/StageSelect/BattleSortieSelectionService.cs)

```csharp
private static SelectedBattleStageState ResolveSelectedBattleStageState()
{
    if (ServiceLocator.TryGetInstance(out SelectedBattleStageState state)) { return state; }

    state = new SelectedBattleStageState();
    ServiceLocator.RegisterInstance(state);      // ← Adaptorがコンテナへ登録
    ...
}
```

「無ければ作って登録する」という遅延初期化を、**Composition Root ではなく
Adaptor層が担っている**。結果として、Composition層が把握していないインスタンスが
コンテナに入る（誰がいつ作ったか追跡できない）。

**修正方針**: 生成と登録は `6.Composition` の初期化フェーズへ移し、
`BattleSortieSelectionService` はコンストラクタで受け取るだけにする。

### 【良い例】EnemyLifeCycle

[EnemyLifeCycle.cs:153-165](../../Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs) は
`EnemyAIController` / `HealthHudViewModel` / `EnemyHealthHudPresenter` / `TransformTargetable` を
生成しているが、これらは**すべてこの敵個体に専有される**オブジェクトであり、
Creatorの「集約するクラスが生成する」に合致している。

---

## Controller（コントローラ）

> システムイベントを受け取る責務は、専用のコントローラに割り当てる。

`3.Adaptor` の命名分布は以下。

| サフィックス | 件数 |
| --- | --- |
| **Controller** | **41** |
| Presenter | 25 |
| State | 13 |
| Facade | 5 |
| Service | 1 |
| Applicator / Sequencer / Gate | 各1 |

`Controller` が41件と突出しており、**役割の異なるクラスに同じ名前が付いている**。

| クラス | 実際の役割 |
| --- | --- |
| `ScreenController` | UseCaseへの委譲（GRASP的にController） |
| `EnemyAIController` | 敵AIの状態機械（Controllerというより Coordinator） |
| `PlayerAttackController` | 攻撃判定と結果算出（実質 Service / Pipeline） |
| `SkillExecutionController` | スキル実行ポリシーの適用（実質 Policy 適用者） |
| `MissionProgressRecorderController` | 7つのイベントを購読して記録（実質 Observer / Recorder） |

**修正方針**: 「入力/イベントを受けて他へ委譲する」ものだけ `Controller` とし、
それ以外は役割に応じた名前（`Coordinator` / `Recorder` / `Policy` / `Pipeline`）へ改める。
名前が役割を表さないと、新規メンバーがどこに何を書くべきか判断できなくなる。

---

## Low Coupling（疎結合）

### 【良好】層間の結合

`ServiceLocator` を参照する49ファイル中47ファイルが `6.Composition` 内に収まっており、
`asmdef` による参照制約も機能している。**これは高く評価すべき**。

### 【違反】デメテルの法則違反によるチェーン結合

```csharp
// 3.Adaptor/InGame/Result/StageResultPresenter.cs:66
_missionRuntimeService.MissionProgress.ElapsedTime.Value

// 3.Adaptor/InGame/Battle/PlayerAttackController.cs:203
_battleState.Attacker.CombatSpec.GetAttackDefinitionByBeatType(...)

// 6.Composition/InGame/Bootstrap/SceneDependencyInitializationModule.cs:88
_container.InputComposition.GetInputMapController.EnableOnly(...)
```

詳細は [17. デメテルの法則とCQS](17_デメテルの法則とCQS.md) を参照。

---

## High Cohesion（高凝集）

### 【違反】Composition層の巨大クラス

| ファイル | 行数 | 問題 |
| --- | --- | --- |
| [StageSelectInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs) | **945** | DI組み立て + UIレイアウト計算 |
| [EnemyLifeCycle.cs](../../Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs) | 709 | 生成 + 初期化 + プール + 死亡演出 + イベント配線 |
| [SkillTreeInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs) | 554 | DI + ノード配置 |
| [ScreenInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/Screen/ScreenInitializer.cs) | 505 | DI + 8つのUIイベントハンドラ |

`StageSelectInitializer` は `BuildNodeCenterMap` / `BuildConnectionElements` /
`BuildNodeElements` / `UpdateStageMapVerticalAlignment` という
**View層の責務であるレイアウト計算**を抱えている。

長大メソッドも同様。

| メソッド | 行数 |
| --- | --- |
| `ScreenInitializer.Initialize()` | **151** |
| `StageSelectInitializer.Initialize()` | 133 |
| `TitleSceneInitializer.Build()` | 132 |
| `PlayerInitializer.Initialize()` | 130 |

**修正方針**: レイアウト構築をView層へ、イベントハンドラ群を別クラスへ切り出す。

---

## Polymorphism（多態性）

### 【良好】

- `IOneShotVisualEffect` の6実装（Particle / Sound / VFXGraph / Animator 等）— 正当なStrategy
- Mission系 `ConditionAsset` 群の `SerializeReference` によるポリモーフィズム
- `IScreenView` / `ScreenViewBase` — レジストリ経由で多態的に使用

### 【要検討】enumに対するswitchが残る箇所

| 場所 | switch対象 | 判定 |
| --- | --- | --- |
| [SkillExecutionController.cs:99](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillExecutionController.cs) | `TARGET_REJECT_POLICY`（**const**） | **削除すべき**（[14. KISS/YAGNI](14_KISS原則とYAGNI.md) 参照） |
| [EnemyWaveSpawnerController.cs:75](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyWaveSpawnerController.cs) | `EnemyType` | 敵種別が増えるたび修正が必要（OCP違反） |
| [MissionEvaluationItemView.cs:24](../../Assets/Scripts/Runtime/4.View/InGame/Mission/MissionEvaluationItemView.cs) | `DisplayState` | ルックアップテーブル化が適切（多態化は過剰） |
| [HUDEnemyHealthView.cs:30](../../Assets/Scripts/Runtime/4.View/InGame/UI/HUDEnemyHealthView.cs) | `LockOnDisplayState` | 表示切替のみ。現状で問題なし |

**重要**: すべてのswitchを多態化すべきではない。
表示切替のような**単純なマッピングはswitchやテーブルの方が読みやすい**。
振る舞いが分岐ごとに大きく異なる `EnemyWaveSpawnerController` のみ検討対象。

---

## Pure Fabrication（純粋人工物）

> ドメイン概念に対応しないが、凝集度を保つために作るクラス。

### 【良好】

- `ModuleContainer` 系10クラス — DI組み立て結果をまとめる人工物として妥当
  （ただし `record` 化でボイラープレート削減可、[10](10_コード重複と過剰抽象化.md) 参照）
- `CatalogRepositoryBase<TId, TDefinition, TEntry>` — 3リポジトリで共有される良い抽象化
- `StageRankCalculator` — ランク算出という計算責務の切り出し
- `CameraFollowCalculator` / `CameraLockOnRotationCalculator` 等の Calculation 群 —
  カメラ計算をViewから分離しており、テスト可能性も高い

Pure Fabricationの適用は本プロジェクトの**最も優れている点**の一つ。

---

## Indirection（間接化）

### 【過剰】

シナリオパイプラインの4層構造（EventHandler → Facade → Presenter → ViewSink）は、
間接化のコストが利益を上回っている。詳細は [14. KISS/YAGNI](14_KISS原則とYAGNI.md) 参照。

### 【適切】

`IEnemyWaveDefinitionRepository` 等のDIP境界インターフェースは、
実装が1件でも**層の独立性を守る**という明確な利益がある。

---

## Protected Variations（変動からの保護）

> 変化しやすい点をインターフェースで包み、他への波及を防ぐ。

### 【良好】

- Repository群による永続化方式の隠蔽
- `IMusicSyncService` によるCRIWARE依存の隔離

### 【違反】静的アクセスが保護を破っている

| 場所 | 内容 |
| --- | --- |
| [RhythmJustService.cs:11](../../Assets/Scripts/Runtime/2.Application/InGame/Music/RhythmJustService.cs) | 唯一の静的シングルトン。差し替え不能 |
| [SkillCrosshairProgressController.cs:57](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillUI/SkillCrosshairProgressController.cs) | `static` メソッド内で `ServiceLocator` を直接参照 |
| `EventBus<T>` | 静的クラス。購読解除漏れが[01](01_イベント購読解除漏れ.md)で問題化 |
| `Camera.main` 7箇所 | カメラ差し替え演出と競合しうる |

---

# 付録: 該当箇所の全列挙

## 付録A. `3.Adaptor` の `*Controller`（全41ファイル）

うち3件はインターフェース定義なので、**具象クラスは38件**。
役割ごとに分類した。改名は影響範囲が大きいため、
**まず新規クラスから適切な名前を使う**運用を推奨。

### A-1. GRASP的に正しい Controller（入力・イベントを受けて委譲）— 8件

| ファイル | 役割 |
| --- | --- |
| [3.Adaptor/OutGame/Screen/ScreenController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Screen/ScreenController.cs) | UseCaseへの委譲 |
| [3.Adaptor/OutGame/SkillBuild/SkillBuildController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillBuild/SkillBuildController.cs) | 編成操作の受付 |
| [3.Adaptor/OutGame/SkillTree/SkillTreeController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/SkillTreeController.cs) | ツリー操作の受付 |
| [3.Adaptor/OutGame/StageSelect/StageSelectController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/StageSelect/StageSelectController.cs) | ステージ選択の受付 |
| [3.Adaptor/OutGame/Sortie/OutGameSortieController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Sortie/OutGameSortieController.cs) | 出撃操作の受付 |
| [3.Adaptor/OutGame/Title/TitleStartController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Title/TitleStartController.cs) | 開始操作の受付 |
| [3.Adaptor/OutGame/Scenario/ScenarioInputController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/ScenarioInputController.cs) | シナリオ入力の受付 |
| [3.Adaptor/InGame/Sequence/ReturnToTitleController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Sequence/ReturnToTitleController.cs) | タイトル復帰の受付 |

### A-2. 実質 Coordinator / State Machine — 6件

「入力を受けて委譲」ではなく、状態遷移を駆動している。

| ファイル | 推奨名 |
| --- | --- |
| [3.Adaptor/InGame/Enemy/EnemyAIController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyAIController.cs) | `EnemyAICoordinator` |
| [3.Adaptor/InGame/Enemy/Boss/BossAIController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/Boss/BossAIController.cs) | `BossAICoordinator` |
| [3.Adaptor/InGame/Enemy/ShellController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/ShellController.cs) | `ShellCoordinator` |
| [3.Adaptor/InGame/Enemy/EnemyWaveSpawnerController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyWaveSpawnerController.cs) | `EnemyWaveSpawner`（`EnemyType` switchは[16. OCP](16_SOLID原則.md)参照） |
| [3.Adaptor/InGame/Mission/MissionEventController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/MissionEventController.cs) | `MissionEventCoordinator` |
| [3.Adaptor/InGame/Music/MusicSyncController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Music/MusicSyncController.cs) | `MusicSyncCoordinator` |

### A-3. 実質 Service / Pipeline（計算・判定が主）— 9件

| ファイル | 推奨名 |
| --- | --- |
| [3.Adaptor/InGame/Battle/PlayerAttackController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs) | `AttackPipeline`（CQS違反も併発。[17](17_デメテルの法則とCQS.md)） |
| [3.Adaptor/InGame/Skill/SkillExecutionController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillExecutionController.cs) | `SkillExecutionPolicy`（死んだswitchあり。[14](14_KISS原則とYAGNI.md)） |
| [3.Adaptor/InGame/Skill/SkillAttackController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillAttackController.cs) | `SkillAttackService` |
| [3.Adaptor/InGame/Skill/SkillController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillController.cs) | `SkillService` |
| [3.Adaptor/InGame/Enemy/EnemyRaycastDetectController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyRaycastDetectController.cs) | `EnemyRaycastDetector` |
| [3.Adaptor/InGame/Enemy/NearestAttackPositionSearchController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/NearestAttackPositionSearchController.cs) | `NearestAttackPositionSearcher` |
| [3.Adaptor/InGame/Enemy/EnemyArtilleryAttackController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyArtilleryAttackController.cs) | `EnemyArtilleryAttacker` |
| [3.Adaptor/InGame/Enemy/EnemyInfantryAttackController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyInfantryAttackController.cs) | `EnemyInfantryAttacker` |
| [3.Adaptor/InGame/Enemy/Boss/EnemyTripleShotAttackController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/Boss/EnemyTripleShotAttackController.cs) | `EnemyTripleShotAttacker` |

### A-4. 実質 Observer / Recorder（購読して記録）— 5件

| ファイル | 推奨名 | 備考 |
| --- | --- | --- |
| [3.Adaptor/InGame/Mission/MissionProgressRecorderController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/MissionProgressRecorderController.cs) | `MissionProgressRecorder` | 7イベントを購読 |
| [3.Adaptor/InGame/Mission/MissionWaveController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/MissionWaveController.cs) | `MissionWaveObserver` | |
| [3.Adaptor/InGame/Mission/MissionStepPopupController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/MissionStepPopupController.cs) | `MissionStepPopupPresenter` | |
| [3.Adaptor/InGame/Mission/MissionPlayerBuffController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/MissionPlayerBuffController.cs) | `MissionPlayerBuffApplier` | バフ配線未完了（[差分分析A-10](../仕様書と実装の差分分析_2026-07-27.md)） |
| [3.Adaptor/Persistent/Input/RecordController.cs](../../Assets/Scripts/Runtime/3.Adaptor/Persistent/Input/RecordController.cs) | `InputRecorder` | |

### A-5. その他（10件）

| ファイル | 備考 |
| --- | --- |
| [3.Adaptor/InGame/Target/TargetSystemController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Target/TargetSystemController.cs) | `ITargetSystemViewModel` 14メンバ問題の実装元（[16](16_SOLID原則.md)） |
| [3.Adaptor/InGame/Player/PlayerController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Player/PlayerController.cs) | `ref`+`out` のCQS違反（[17](17_デメテルの法則とCQS.md)） |
| [3.Adaptor/InGame/Result/StageResultController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Result/StageResultController.cs) | |
| [3.Adaptor/InGame/Music/EquipmentBgmController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Music/EquipmentBgmController.cs) | |
| [3.Adaptor/InGame/Skill/SkillUI/SkillCrosshairProgressController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillUI/SkillCrosshairProgressController.cs) | **`ServiceLocator` 直接参照**（レイヤー違反。[08](08_緊密結合とレイヤー違反.md)） |
| [3.Adaptor/InGame/Skill/SkillUI/SkillGuideProgressController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillUI/SkillGuideProgressController.cs) | |
| [3.Adaptor/InGame/Skill/SkillUI/SkillInputProgressController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillUI/SkillInputProgressController.cs) | |
| [3.Adaptor/InGame/Mission/OutGameMissionSelectController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/OutGameMissionSelectController.cs) | InGame配下にOutGame名。配置が不自然 |
| [3.Adaptor/Persistent/Load/LoadingScreenController.cs](../../Assets/Scripts/Runtime/3.Adaptor/Persistent/Load/LoadingScreenController.cs) | |
| [3.Adaptor/Persistent/SceneManagement/SceneTransitionController.cs](../../Assets/Scripts/Runtime/3.Adaptor/Persistent/SceneManagement/SceneTransitionController.cs) | |

### A-6. インターフェース定義（3件・改名対象外）

| ファイル |
| --- |
| [3.Adaptor/InGame/Enemy/IEnemyAttackController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/IEnemyAttackController.cs) |
| [3.Adaptor/InGame/Player/IPlayerController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Player/IPlayerController.cs) |
| [3.Adaptor/OutGame/Screen/IScreenController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Screen/IScreenController.cs) |

**集計**: 38の具象Controllerのうち、GRASP的に正しい Controller は **8件（21%）**。
残り30件は他の役割を `Controller` の名前で表している。

## 付録B. Information Expert の検証結果

| クラス | 保持する情報 | 振る舞い | 判定 |
| --- | --- | --- | --- |
| [1.Domain/InGame/Character/CharacterEntity.cs](../../Assets/Scripts/Runtime/1.Domain/InGame/Character/CharacterEntity.cs) | HP・無敵状態 | `TakeDamage` / `Heal` / `SetInvincible` / `Reset` / `ChangeBaseDamage` / `SetDamage` | **良好** |
| [1.Domain/InGame/Battle/AttackIntervalEntity.cs](../../Assets/Scripts/Runtime/1.Domain/InGame/Battle/AttackIntervalEntity.cs) | `IsAttacking` / `Interval` | `UpdateAttackState(bool)` のみ | **違反**（遷移ロジックが[AttackIntervalEvaluator.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Battle/AttackIntervalEvaluator.cs)側にある） |
| [1.Domain/OutGame/SkillTree/Entity/SkillTreeStatusEntity.cs](../../Assets/Scripts/Runtime/1.Domain/OutGame/SkillTree/Entity/SkillTreeStatusEntity.cs) | 解放済ノード・ポイント | `ModifyPoint` / `AddUnlockedNode` / `AddUnlockedSkillIds` | 良好（ただし加算経路が未実装。[差分分析A-1](../仕様書と実装の差分分析_2026-07-27.md)） |
| [1.Domain/Persistent/Savedata/TutorialData.cs](../../Assets/Scripts/Runtime/1.Domain/Persistent/Savedata/TutorialData.cs) | 完了フラグ | `Complete()` | 良好 |
| [1.Domain/OutGame/SkillBuild/SkillBuildDefinition.cs](../../Assets/Scripts/Runtime/1.Domain/OutGame/SkillBuild/SkillBuildDefinition.cs) | 編成スロット | `EnsureSlotCount` / `SlotCount` | 良好 |

## 付録C. Pure Fabrication の良い適用例（削除・統合しないこと）

| ファイル | 役割 |
| --- | --- |
| [4.View/InGame/Camera/Calculation/CameraFollowCalculator.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/Calculation/CameraFollowCalculator.cs) | 追従計算。※補間式のフレームレート依存は[07](07_ライブラリ未活用.md)参照 |
| [4.View/InGame/Camera/Calculation/CameraLockOnRotationCalculator.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/Calculation/CameraLockOnRotationCalculator.cs) | ロックオン回転計算 |
| [4.View/InGame/Camera/Calculation/CameraFreeLookRotationCalculator.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/Calculation/CameraFreeLookRotationCalculator.cs) | フリールック回転計算 |
| [4.View/InGame/Camera/Calculation/CameraLookAtRotationCalculator.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/Calculation/CameraLookAtRotationCalculator.cs) | 注視回転計算 |
| [4.View/InGame/Camera/Calculation/CameraLockOnRangeChecker.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/Calculation/CameraLockOnRangeChecker.cs) | 視野内判定 |
| [4.View/InGame/Camera/Calculation/CameraLockOnBreakTracker.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/Calculation/CameraLockOnBreakTracker.cs) | ロックオン解除判定 |
| [5.InfraStructure/OutGame/Scenario/CatalogRepositoryBase.cs](../../Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/CatalogRepositoryBase.cs) | 3リポジトリで共有される辞書キャッシュ基底 |
| `6.Composition/**/\*ModuleContainer.cs`（10件） | DI組み立て結果の集約。`record`化推奨（[10](10_コード重複と過剰抽象化.md)） |

カメラ計算6クラスは `CameraSystemView`（674行）からロジックを分離したもので、
本プロジェクトで最も成功した責務分離。**引数15個問題（[17](17_デメテルの法則とCQS.md)）の
解決時も、この6クラスは維持したまま `CameraCalculatorSet` に束ねるだけでよい**。
