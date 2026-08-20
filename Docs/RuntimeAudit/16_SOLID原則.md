# 16. SOLID原則

| 原則 | 評価 | 主な指摘 |
| --- | --- | --- |
| **S** 単一責任 | 違反あり | Composition層の巨大クラス・長大メソッド |
| **O** 開放閉鎖 | 一部違反 | `EnemyType` switch、定数switch |
| **L** リスコフ置換 | 要注意 | 既定でtrueを返す基底クラス |
| **I** インターフェース分離 | **概ね良好** | 例外は `ITargetSystemViewModel` |
| **D** 依存性逆転 | **良好** | 例外は Adaptor の2箇所 |

---

## S — 単一責任の原則（SRP）

### 【違反】400行超のファイルが21個

全件は [付録A](#付録a-400行超のファイル全21件) を参照。特に責務が多いのは以下。

| ファイル | 行数 | 抱えている責務 |
| --- | --- | --- |
| [StageSelectInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs) | **945** | DI組み立て / UIレイアウト計算 / イベント配線 / 遷移制御 |
| [PlayerView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Player/PlayerView.cs) | 842 | 入力 / 移動 / 回避 / 攻撃回転 / アニメ連携 |
| [ScenarioRepository.cs](../../Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs) | 823 | CSV読込 / 文字列パース / 6種のswitch分岐 |
| [EnemyLifeCycle.cs](../../Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs) | 709 | 生成 / 初期化 / プール / 死亡演出 / イベント配線 |
| [ACLikeRhythmGuideView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Music/ACLikeRhythmGuideView.cs) | 689 | GUI生成 / モーション / Vignette生成 / ゾーン計算 |
| [CameraSystemView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs) | 674 | 入力 / ロックオン / 追従 / 障害物回避 / 外部制御 |

### 【違反】60行超のメソッドが17個

全件は [付録B](#付録b-60行超のメソッド全17件) を参照。上位は以下。

| メソッド | 行数 |
| --- | --- |
| `ScreenInitializer.Initialize()` | **151** |
| `StageSelectInitializer.Initialize()` | 133 |
| `TitleSceneInitializer.Build()` | 132 |
| `PlayerInitializer.Initialize()` | 130 |
| `SequenceInitializationModule.Ready()` | 96 |
| `IngameComposition.Start()` | 92 |
| `Skill_07.Execute()` | 83 |

`ACLikeRhythmGuideView` は特に分かりやすい例で、
**実行時にGameObjectとVolumeProfileを動的生成している**。

```csharp
// line 429  ジャストタイミングマーカーのGameObject生成
GameObject markerObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));

// line 459-476  Vignette用Volumeの動的生成
GameObject volumeObject = new GameObject("JustTimingVignetteVolume", typeof(Volume));
_vignetteProfile = ScriptableObject.CreateInstance<VolumeProfile>();
_justTimingVignette = _vignetteProfile.Add<Vignette>(true);

// line 618-643  ビートブロックのGameObject生成ループ
GameObject leftBeat = new GameObject($"LeftBeat_{i}", typeof(RectTransform), typeof(Image));
```

「リズムガイドを表示する」クラスが「UIを構築する」「ポストプロセスを構築する」責務まで
持っており、`OnDestroy`（238-281行）が**44行かけて後始末**しているのがその代償。

**修正方針**: `BeatGuiBuilder`（GUI構築）と `JustTimingVignetteController`（ポストプロセス）を
切り出す。それぞれが自分の後始末を持てば `OnDestroy` も分割される。

---

## O — 開放閉鎖の原則（OCP）

### 【違反】敵種別のswitch

**場所**: [EnemyWaveSpawnerController.cs:75](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyWaveSpawnerController.cs)

```csharp
switch (waveDefinition.Details[i].EnemyType)
```

敵種別を追加するたびにこのswitchの修正が必要。
既に `EnemyInfantrySpawner` / `EnemyArtillerySpawner` という
種別ごとのSpawnerクラスが存在するため、**`Dictionary<EnemyType, IEnemySpawner>` への
登録制に変えれば、新種別追加時にこのファイルを触らずに済む**。

### 【違反】文字列パースの多段switch

**場所**: [ScenarioRepository.cs:217, 297, 337, 483, 599, 647](../../Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs)

```csharp
switch (type.Trim().ToLowerInvariant())
switch (triggerType.ToLowerInvariant())
switch (onTriggerType.ToLowerInvariant())
```

6箇所のswitchで文字列からシナリオコマンドを解決している。
コマンド追加のたびに823行のファイルを開いて該当switchを探す必要がある。
`ToLowerInvariant()` は**呼び出しごとに文字列を確保する**点も問題
（[04. GC Alloc](04_GCAllocとメモリリーク.md) の観点）。

**修正方針**: `Dictionary<string, Func<...>>` によるパーサ登録テーブルへ移行し、
キーは `StringComparer.OrdinalIgnoreCase` で比較する（`ToLowerInvariant` 不要）。

### 【違反】定数に対するswitch

[SkillExecutionController.cs:99](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillExecutionController.cs) は
拡張点を用意したつもりで**実際には拡張不能**（`private const`）。
詳細は [14. KISS/YAGNI](14_KISS原則とYAGNI.md) 参照。

---

## L — リスコフの置換原則（LSP）

### 【要注意】既定でtrueを返す基底クラス

**場所**:
[OutGameInitializationModuleBase.cs:9-60](../../Assets/Scripts/Runtime/6.Composition/OutGame/Bootstrap/OutGameInitializationModuleBase.cs)、
[PersistentInitializationModuleBase.cs:9-60](../../Assets/Scripts/Runtime/6.Composition/Persistent/Bootstrap/PersistentInitializationModuleBase.cs)

全メソッドが `true` またはno-opを返す実装になっている。
`Init` / `Build` / `Ready` の戻り値 `bool` は**「初期化に成功したか」**を意味するため、
派生クラスがオーバーライドを忘れると**「何もしていないのに成功を報告する」**。

これはLSPの「派生型は基底型の事後条件を弱めてはならない」に抵触する。
初期化フェーズが黙って通過し、後段でnull参照として顕在化する。

さらに `InGameInitializationModuleBase` は**存在しない**という不整合もある
（[10. コード重複](10_コード重複と過剰抽象化.md) 参照）。

**修正方針**: 基底を `abstract` にして派生クラスに実装を強制するか、
既定を「未実装」を意味する値にする。少なくとも
「オーバーライドされていない」ことをログで検知できるようにする。

### 【良好】ReusableParticleSystemView 階層

[ParticleSystemPoolView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Character/ParticleSystemPoolView.cs) と
`ParticleSystemRingBufferView` は `ReusableParticleSystemView` の
`Play()` / `StopAll()` を実装しており、呼び出し側はどちらでも同じ契約で扱える。
プール方式（ObjectPool / リングバッファ）の違いが利用側に漏れていない。

---

## I — インターフェース分離の原則（ISP）

### 【良好】インターフェースは総じて小さい

135のインターフェースファイルのうち、メンバ6個以上のものはごく少数。
`IFadeViewSink` や `IDamageable` のように**1〜2メソッドの小さな契約**が主体で、
ISPの観点では非常に良い状態。

### 【違反】ITargetSystemViewModel が14メンバ

**場所**: [ITargetSystemViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Target/ITargetSystemViewModel.cs)（105行）

```csharp
void RegisterTarget(ITargetableViewModel targetable);          // 登録
void UnregisterTarget(ITargetableViewModel targetable);        // 登録
bool TryGetCurrentTarget(out ITargetableViewModel targetable); // 照会
bool TryGetCurrentTargetId(out Guid targetId);                 // 照会
bool TryGetCurrentTargetPosition(out Vector3 result);          // 照会
bool TryGetCurrentCandidate(out ITargetableViewModel target);  // 照会
bool TryGetCurrentCandidateId(out Guid targetId);              // 照会
bool TryGetCurrentCandidatePosition(out Vector3 result);       // 照会
ITargetableViewModel[] GetRegisteredTargetsSnapshot();         // 照会
void ChangeTarget(in Vector3 playerPosition, in Vector3 dir);  // 操作
void UpdateCandidate(in Vector3 playerPosition, in Vector3 dir);// 操作
bool TrySwitchTarget(in Vector3 playerPosition, in Vector3 dir);// 操作
bool TrySetCurrentTarget(Guid targetId);                       // 操作
void ClearTarget();                                            // 操作
```

**3つの異なる関心事が1つの契約に同居**している。

| 関心事 | 利用者 |
| --- | --- |
| ターゲット登録 | `EnemyLifeCycle`（登録/解除のみ必要） |
| 現在ターゲットの照会 | `HUDEnemyHealthPresenter`、`ReticleHudPresenter`、`SkillCrosshairProgressController` |
| ターゲット操作 | `CameraSystemView`（入力に応じた切り替え） |

現状、HUD表示のためだけに `ClearTarget()` や `RegisterTarget()` まで
見えてしまっており、誤用の余地がある。

なお `GetRegisteredTargetsSnapshot()` は**名前どおり配列を新規確保する**可能性が高く、
毎フレーム呼ばれると [04. GC Alloc](04_GCAllocとメモリリーク.md) の対象になる。要確認。

**修正方針**: 3つに分割する。

```csharp
public interface ITargetRegistry { RegisterTarget / UnregisterTarget }
public interface ITargetQuery    { TryGetCurrent* / GetRegisteredTargetsSnapshot }
public interface ITargetCommand  { ChangeTarget / UpdateCandidate / TrySwitchTarget / TrySetCurrentTarget / ClearTarget }
```

`TargetingSystem` が3つとも実装すればよく、利用側は必要な契約だけを受け取る。
`CameraSystemView.Initialize` の引数15個問題
（[17. デメテルの法則とCQS](17_デメテルの法則とCQS.md) 参照）も、
現在5つのデリゲートで渡しているターゲット操作を `ITargetCommand` 1つに置き換えれば
大幅に改善する。

---

## D — 依存性逆転の原則（DIP）

### 【良好】層構造とasmdefによる強制

`asmdef` の参照設定により、内側の層が外側を参照できないようになっている。

```
KillChord.Domain      → Utility のみ
KillChord.Application → Utility, Domain
KillChord.Adaptor     → Utility, Domain, Application
```

Repository群も Application 層がインターフェースを定義し、
InfraStructure 層が実装する形（依存の逆転）が正しく実現されている。
`ServiceLocator` を参照する49ファイル中47ファイルが `6.Composition` 内という数字がそれを裏付ける。

### 【違反】Adaptor層の2箇所

| 場所 | 内容 |
| --- | --- |
| [SkillCrosshairProgressController.cs:57](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillUI/SkillCrosshairProgressController.cs) | `static` メソッド内で `ServiceLocator` を直接参照（具象への依存） |
| [BattleSortieSelectionService.cs:45,51,61,67](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/StageSelect/BattleSortieSelectionService.cs) | 取得に加え `RegisterInstance` まで実行 |

詳細は [08. 緊密結合とレイヤー違反](08_緊密結合とレイヤー違反.md) 参照。

### 【違反】View層が自力で依存を探しに行く

```csharp
// 4.View/Persistent/Voice/VoiceSource.cs:65
_volumeRegistryView ??= FindAnyObjectByType<PersistentAudioVolumeRegistryView>();

// 4.View/InGame/Enemy/EnemySpawnPositionSearcher.cs:19
_positionPairs = GameObject.FindObjectsByType<SpawnPositionPair>(FindObjectsSortMode.None);
```

`Camera.main` への直接依存7箇所も同種の問題。

---

# 付録: 該当箇所の全列挙

## 付録A. 400行超のファイル（全21件）

行数の降順。パスは `Assets/Scripts/Runtime/` 相対。

| # | 行数 | ファイル |
| --- | --- | --- |
| 1 | 945 | [6.Composition/OutGame/StageSelect/StageSelectInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs) |
| 2 | 842 | [4.View/InGame/Player/PlayerView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Player/PlayerView.cs) |
| 3 | 823 | [5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs](../../Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs) |
| 4 | 709 | [6.Composition/InGame/Enemy/EnemyLifeCycle.cs](../../Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs) |
| 5 | 689 | [4.View/InGame/Music/ACLikeRhythmGuideView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Music/ACLikeRhythmGuideView.cs) |
| 6 | 674 | [4.View/InGame/Camera/CameraSystemView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs) |
| 7 | 673 | [0.Utility/Collections/PriorityQueue.cs](../../Assets/Scripts/Runtime/0.Utility/Collections/PriorityQueue.cs) ※BCL移植のため対象外 |
| 8 | 571 | [4.View/InGame/Enemy/EnemyMoveView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyMoveView.cs) |
| 9 | 554 | [6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs) |
| 10 | 529 | [6.Composition/InGame/Player/PlayerInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/InGame/Player/PlayerInitializer.cs) |
| 11 | 505 | [6.Composition/OutGame/Screen/ScreenInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/Screen/ScreenInitializer.cs) |
| 12 | 500 | [4.View/OutGame/Scenario/ScenarioView.cs](../../Assets/Scripts/Runtime/4.View/OutGame/Scenario/ScenarioView.cs) |
| 13 | 494 | [6.Composition/OutGame/Title/TitleSceneInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/Title/TitleSceneInitializer.cs) |
| 14 | 472 | [4.View/InGame/Camera/StageStartCameraView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/StageStartCameraView.cs) |
| 15 | 446 | [6.Composition/OutGame/Scenario/ScenarioCom.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/Scenario/ScenarioCom.cs) |
| 16 | 444 | [1.Domain/OutGame/StageSelect/StageTree.cs](../../Assets/Scripts/Runtime/1.Domain/OutGame/StageSelect/StageTree.cs) |
| 17 | 435 | [5.InfraStructure/Persistent/SceneManagement/SceneTransitionService.cs](../../Assets/Scripts/Runtime/5.InfraStructure/Persistent/SceneManagement/SceneTransitionService.cs) |
| 18 | 413 | [4.View/InGame/Result/StageResultView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Result/StageResultView.cs) |
| 19 | 411 | [4.View/Persistent/Input/PlayerInputView.cs](../../Assets/Scripts/Runtime/4.View/Persistent/Input/PlayerInputView.cs) |
| 20 | 403 | [4.View/InGame/Enemy/EnemyRaycastDetectView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyRaycastDetectView.cs) |
| 21 | 401 | [6.Composition/InGame/Sequence/SequenceInitializationModule.cs](../../Assets/Scripts/Runtime/6.Composition/InGame/Sequence/SequenceInitializationModule.cs) |

`6.Composition` が9件と最多。DI組み立てにイベント配線・UIレイアウトが混入しているのが主因。

## 付録B. 60行超のメソッド（全17件）

`ファイル:開始行` 形式。行数はメソッド全体の長さ。

| # | 行数 | 場所 | シグネチャ |
| --- | --- | --- | --- |
| 1 | 151 | [ScreenInitializer.cs:97](../../Assets/Scripts/Runtime/6.Composition/OutGame/Screen/ScreenInitializer.cs) | `private bool Initialize()` |
| 2 | 133 | [StageSelectInitializer.cs:260](../../Assets/Scripts/Runtime/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs) | `private bool Initialize()` |
| 3 | 132 | [TitleSceneInitializer.cs:98](../../Assets/Scripts/Runtime/6.Composition/OutGame/Title/TitleSceneInitializer.cs) | `public override bool Build()` |
| 4 | 130 | [PlayerInitializer.cs:174](../../Assets/Scripts/Runtime/6.Composition/InGame/Player/PlayerInitializer.cs) | `public void Initialize(InputComposition, SkillController)` |
| 5 | 96 | [SequenceInitializationModule.cs:75](../../Assets/Scripts/Runtime/6.Composition/InGame/Sequence/SequenceInitializationModule.cs) | `public override bool Ready()` |
| 6 | 92 | [IngameComposition.cs:28](../../Assets/Scripts/Runtime/6.Composition/InGame/Bootstrap/IngameComposition.cs) | `private async void Start()` |
| 7 | 83 | [Skill_07.cs:32](../../Assets/Scripts/Runtime/2.Application/Player/SkillEffect/Skill_07.cs) | `public override void Execute(in SkillEffectContext)` |
| 8 | 82 | [SkillTreeInitializer.cs:212](../../Assets/Scripts/Runtime/6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs) | `private bool Initialize()` |
| 9 | 79 | [ScenarioRepository.cs:62](../../Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs) | `private static ScenarioDefinition ParseNormalizedCsv(string[])` |
| 10 | 71 | [EquipmentBgmInitializer.cs:37](../../Assets/Scripts/Runtime/6.Composition/InGame/Music/EquipmentBgmInitializer.cs) | `public override bool Build()` |
| 11 | 69 | [SceneTransitionInitializer.cs:41](../../Assets/Scripts/Runtime/6.Composition/Persistent/SceneManagement/SceneTransitionInitializer.cs) | `public override bool Build()` |
| 12 | 67 | [EnemyInitializer.cs:48](../../Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyInitializer.cs) | `public override async Awaitable<bool> ResourceLoadAsync(CancellationToken)` |
| 13 | 65 | [InGameMissionInitializer.cs:106](../../Assets/Scripts/Runtime/6.Composition/InGame/Mission/InGameMissionInitializer.cs) | `public bool TryInitialize(out MissionRuntimeService)` |
| 14 | 64 | [SkillBuildInitializer.cs:142](../../Assets/Scripts/Runtime/6.Composition/OutGame/SkillBuild/SkillBuildInitializer.cs) | `private bool Initialize()` |
| 15 | 64 | [ScenarioCom.cs:100](../../Assets/Scripts/Runtime/6.Composition/OutGame/Scenario/ScenarioCom.cs) | `public override bool Build()` |
| 16 | 64 | [ScenarioRepository.cs:146](../../Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs) | `private static ScenarioDefinition ParseAuthoringCsv(string[])` |
| 17 | 62 | [MissionDefinitionAsset.cs:21](../../Assets/Scripts/Runtime/5.InfraStructure/InGame/Mission/MissionDefinitionAsset.cs) | `public MissionDefinition Create()` |

17件中12件が `6.Composition`。`ScenarioRepository` の2つのCSVパーサ（#9・#16）は
構造もよく似ており、[13. DRY原則](13_DRY原則.md) の対象でもある。

## 付録C. ISP検査で「問題なし」と判定した大きめのインターフェース

`ITargetSystemViewModel` 以外はいずれも役割が単一で分割不要と判断した。

| ファイル | 判定 |
| --- | --- |
| [3.Adaptor/InGame/Target/ITargetSystemViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Target/ITargetSystemViewModel.cs)（14メンバ） | **要分割**（本文参照） |
| [2.Application/InGame/Music/IMusicSyncService.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Music/IMusicSyncService.cs) | 音楽同期という単一の関心事 |
| [2.Application/Persistent/SceneManagement/ISceneTransitionService.cs](../../Assets/Scripts/Runtime/2.Application/Persistent/SceneManagement/ISceneTransitionService.cs) | シーン遷移の一連の操作 |
| [3.Adaptor/OutGame/Screen/IScreenController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Screen/IScreenController.cs) | 画面表示操作。ただし1行委譲は[13. DRY](13_DRY原則.md)参照 |
| [6.Composition/Bootstrap/IInitializationModule.cs](../../Assets/Scripts/Runtime/6.Composition/Bootstrap/IInitializationModule.cs) | 初期化フェーズ契約 |
| [3.Adaptor/InGame/Animaiton/ICharacterAnimationSignal.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Animaiton/ICharacterAnimationSignal.cs) | アニメ通知。※フォルダ名綴り誤り([18](18_命名一貫性と可読性.md)) |
