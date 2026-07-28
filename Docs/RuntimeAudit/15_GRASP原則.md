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

`ServiceLocator` の参照229箇所のうち227箇所が `6.Composition` 内に収まっており、
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
| `Camera.main` 6箇所 | カメラ差し替え演出と競合しうる |
