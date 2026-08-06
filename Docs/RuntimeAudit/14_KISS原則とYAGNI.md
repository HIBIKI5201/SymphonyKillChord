# 14. KISS原則とYAGNI

- **KISS**（Keep It Simple, Stupid）— 必要以上に複雑にしない
- **YAGNI**（You Aren't Gonna Need It）— 今使わない拡張性は作らない

この2つは「投機的一般化（Speculative Generality）」という同じ症状に収束する。

---

## 【重大・KISS/YAGNI】定数に対するswitchで2分岐が死んでいる

**場所**: [SkillExecutionController.cs:16, 97-113](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillExecutionController.cs)

```csharp
/// <summary> 対象不成立時のポリシーです。 </summary>
private const SkillExecutionFailurePolicy TARGET_REJECT_POLICY
    = SkillExecutionFailurePolicy.ResetProgressOnly;   // ← private const

...

private void ApplyFailurePolicy(float now)
{
    switch (TARGET_REJECT_POLICY)                      // ← 定数に対するswitch
    {
        case SkillExecutionFailurePolicy.KeepProgress:
            return;                                     // 到達不能
        case SkillExecutionFailurePolicy.ResetProgressOnly:
            _skillRhythmState.Clear();
            _progressController.ResetProgress(now, _skillCooldownState.SkillReadyTimestamp);
            return;                                     // ← 常にここだけ実行される
        case SkillExecutionFailurePolicy.ResetProgressAndConsumeCooldown:
            _skillRhythmState.Clear();
            _skillCooldownState.SetSkillCooldown(now);
            _progressController.SkillTriggered(now, _skillCooldownState.SkillReadyTimestamp);
            return;                                     // 到達不能
    }
}
```

`TARGET_REJECT_POLICY` は **`private const`** であり、外部から変更する手段がない。
つまり3分岐のうち**2つはコンパイル時点で到達不能**であり、`SkillExecutionFailurePolicy`
enumの他2値は事実上デッドコードである。

「将来ポリシーを切り替えられるように」という意図は分かるが、
現状では読み手に「どの分岐が動くのか」を考えさせるコストだけが残っている。

**修正方針（いずれか）**:
1. **YAGNIに従う** — 実際に使う `ResetProgressOnly` の処理だけを直接書き、
   enumとswitchを削除する（最も単純）
2. **本当に切り替えたいなら** — `const` をやめてコンストラクタ引数か
   ScriptableObject設定に昇格させ、switchを生かす

現状は1と2の中間で、**どちらの利点も得られていない**状態。

---

## 【重大・YAGNI】使われない更新タイミング選択肢

**場所**: [CameraSystemView.cs:179-180, 223-253](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs)

```csharp
[SerializeField, Tooltip("カメラ更新タイミングの設定")]
private UpdateModeEnum _updateMode;
```

`UpdateModeEnum` の3値に対応するため、`FixedUpdate` / `Update` / `LateUpdate` の
**3つのUnityメッセージすべてを定義**している。実行時に処理するのは常に1つだけ。

Unityは「メソッドが存在するか」でメッセージ登録を決めるため、
**中身が即returnでも毎フレーム3回のディスパッチコストが発生する**
（[03. ライフサイクル問題](03_ライフサイクル問題.md) 参照）。

さらに `[DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]` が
クラスに付与されている以上、実行順は既に制御されており、
タイミング選択肢の必要性はさらに薄い。

**修正方針**: 実運用で使っている値を確認し、1つに固定する。
どうしても選択式が必要なら、`Initialize` で不要なコンポーネントを
`enabled = false` にする実装に変える。

---

## インターフェース135ファイルの棚卸し — 大半は正当なDIP境界だった

### 初版の記述を訂正

初版では「実装1件のインターフェースが多数あり YAGNI 違反」と書いたが、
定義元と実装先の層を実際に確認したところ**大半が正当だった**ため訂正する。

Clean Architectureでは、内側の層が外側を参照できない制約を満たすために
**内側がインターフェースを定義し、外側が実装する**。この場合、実装が1件でも
インターフェースは必須である。

本プロジェクトの単一実装インターフェースを調べると、以下が確認できた。

| パターン | 定義元 | 実装先 | 判定 |
| --- | --- | --- | --- |
| `IXxxRepository` | 2.Application | 5.InfraStructure | **正当**（永続化の逆転） |
| `IXxxViewModel` | 3.Adaptor | 4.View | **正当**（表示の逆転） |
| `IXxxViewSink` | 3.Adaptor | 4.View | **正当**（同上） |
| `IXxxOutputPort` | 2.Application | 3.Adaptor | **正当**（出力の逆転） |

検証例:

```
IHUDEnemyHealthViewModel  定義: 3.Adaptor/InGame/UI/  → 実装: 4.View/InGame/UI/
IFadeViewSink             定義: 3.Adaptor/OutGame/Scenario/ → 実装: 4.View/OutGame/Scenario/ViewModel.cs
IStageResultViewModel     定義: 3.Adaptor/InGame/Result/ → 実装: 4.View/InGame/Result/
```

とりわけシナリオの7つの `IXxxViewSink` は、**すべて
[4.View/OutGame/Scenario/ViewModel.cs](../../Assets/Scripts/Runtime/4.View/OutGame/Scenario/ViewModel.cs)
という1クラスが実装している**。これは「小さく分けた契約を1つの実装が満たす」形であり、
**ISPの模範**というべきもの。削除対象ではない。

全件は [付録A](#付録a-単一実装インターフェースの分類全78件) を参照。

### 残る本物のYAGNI — 実装・参照ゼロの3件

| ファイル | 状況 |
| --- | --- |
| [4.View/InGame/Enemy/IShellInitializer.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/IShellInitializer.cs) | 実装・参照ともにゼロ |
| [2.Application/InGame/Skill/IViewAction.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Skill/IViewAction.cs) | 実装・参照ともにゼロ |
| [3.Adaptor/OutGame/Scenario/IOutPutPort.Obsolete.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/IOutPutPort.Obsolete.cs) | `[Obsolete]` 付き。自ファイル以外から参照なし |

**シナリオパイプラインの問題はインターフェースの数ではなく、
Presenter と ViewSink という2つの層が1対1で並んでいる点**にある。
インターフェースを消すのではなく、Presenter層とViewSink層を統合すべき。
詳細は下記「4層を経由して値を1つ渡すシナリオパイプライン」を参照。

---

## 【中・KISS】4層を経由して値を1つ渡すシナリオパイプライン

**場所**: `3.Adaptor/OutGame/Scenario/`

```
EventHandler → ScenarioPresenterFacade → Presenter → ViewSink
```

各段が1行の委譲しか行っておらず、これが**表示要素の種類ごとに複製**されている
（Animation / Background / Fade / Layer / Portrait / Text の6種 × 約4ファイル）。

「値を1つ渡す」だけの処理に interface × 3 + class × 2 を要求する構造は、
KISSの観点で明確に過剰。

**修正方針**: Presenter層とViewSink層を統合し、`ScenarioEventHandler<TEvent>` の
ジェネリック基底で受け口をまとめる。

---

## 【中・KISS】5つの状態フィールドで手書きするフェード

**場所**: [ScenarioView.cs:225-247](../../Assets/Scripts/Runtime/4.View/OutGame/Scenario/ScenarioView.cs)

`_onFade` / `_time` / `_duration` / `_start` / `_end` の5フィールドと
毎フレームの `Update` 分岐で、LitMotionなら1行で書けることを再実装している。

```csharp
// 現状（約20行 + 5フィールド）
_time += Time.deltaTime;
...
float t = Mathf.Clamp01(_time / _duration);
_canvasGroup.alpha = Mathf.Lerp(_start, _end, t);
if (t >= 1f) { _onFade = false; }

// LitMotion使用時（1行）
LMotion.Create(_start, _end, _duration).BindToAlpha(_canvasGroup);
```

詳細は [07. ライブラリ未活用](07_ライブラリ未活用.md) を参照。

---

## 【中・KISS】初期化判定に13個のnullチェック

**場所**: [CameraSystemView.cs:383-392](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs)

毎フレーム13個のnullチェックを実行して「初期化済みか」を判定している。
実質1つのbooleanで済むことを13倍の複雑さで表現している例。

詳細は [02. Null参照リスク](02_Null参照リスクとアサート漏れ.md) を参照。

---

## 【中・YAGNI】導入したまま一度も使っていないライブラリ

| ライブラリ | 状況 |
| --- | --- |
| `com.cysharp.zstring` 2.6.0 | 利用ゼロ |
| `com.cysharp.observablecollections` 1.1.3 | 利用ゼロ |
| `com.cysharp.unitask` 2.5.10 | 1ファイルのみ（実質未活用） |

「いつか使うかもしれない」で追加された依存はビルドサイズと
判断コスト（「非同期は何を使うのが正解か」）を増やす。

**修正方針**: 使うなら移行計画を立てる、使わないなら `manifest.json` から外す。
中間状態が最も害が大きい。

---

## 【小・YAGNI】空のマーカーインターフェース3つ

**場所**: `6.Composition/`

`IInGameInitializationModule` / `IOutGameInitializationModule` / `IPersistentInitializationModule` は
本体が空で `IInitializationModule` を継承するだけ。

シーンごとにモジュールを型で区別する意図と思われるが、
モジュール収集は `FindObjectsByType<XxxInitializationModuleBase>` で
**基底クラスによって既に絞り込まれている**ため、インターフェースによる区別は使われていない。

---

## 【小・KISS】ページ名のハードコード列挙

**場所**: [SettingBase.cs:47-56](../../Assets/Scripts/Runtime/4.View/OutGame/Setting/SettingBase.cs)

`"AudioPage"` / `"ScreenPage"` / `"KeyPage"` を1つずつ非表示にしている。
配列 + ループにすればページ追加時の記述漏れが構造的に防げる。

---

## 逆に評価すべき「シンプルさ」

KISSの観点で**良い判断**として維持すべきもの。

- **コルーチン不使用** — `StartCoroutine` はプロジェクト全体でゼロ。
  async/awaitに一本化されており、例外処理と戻り値の扱いが統一されている
- **LINQをホットパスに持ち込んでいない** — `using System.Linq` は
  `6.Composition` の3ファイルのみ
- **`ReticleHudView` の自前プール** — `Stack<T>` + Show/Hide の20行弱で完結しており、
  `ObjectPool<T>` を持ち出すより単純
- **`readonly ref struct` によるDTO** — 「値を渡すだけ」を最も単純な形で表現している
- **細かく分けたインターフェース** — 上記のとおり、単一実装でも層の独立性を守っている

---

# 付録: 該当箇所の全列挙

## 付録A. 単一実装インターフェースの分類（全78件）

「定義元の層 → 実装先の層」で判定した。**削除対象は最後の3件のみ**。

### A-1. DIP境界（正当・削除不可）— 2.Application が定義、外側が実装

| # | インターフェース | 定義ファイル |
| --- | --- | --- |
| 1 | `IEnemyRaycastDetectRepository` | [2.Application/InGame/Enemy/IEnemyRaycastDetectRepository.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Enemy/IEnemyRaycastDetectRepository.cs) |
| 2 | `IEnemyWaveDefinitionRepository` | [2.Application/InGame/Enemy/IEnemyWaveDefinitionRepository.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Enemy/IEnemyWaveDefinitionRepository.cs) |
| 3 | `INearestAttackPositionSearchRepository` | [2.Application/InGame/Enemy/INearestAttackPositionSearchRepository.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Enemy/INearestAttackPositionSearchRepository.cs) |
| 4 | `ISkillRepository` | [2.Application/InGame/Skill/ISkillRepository.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Skill/ISkillRepository.cs) |
| 5 | `ISkillEffectExecutorResolver` | [2.Application/InGame/Skill/ISkillEffectExecutorResolver.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Skill/ISkillEffectExecutorResolver.cs) |
| 6 | `IPlayerApplication` | [2.Application/InGame/Player/IPlayerApplication.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Player/IPlayerApplication.cs) |
| 7 | `IScenarioCompletionNotifier` | [2.Application/OutGame/Scenario/IScenarioCompletionNotifier.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/Scenario/IScenarioCompletionNotifier.cs) |
| 8 | `IScenarioEventEmitter` | [2.Application/OutGame/Scenario/IScenarioEventEmitter.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/Scenario/IScenarioEventEmitter.cs) |
| 9 | `IScenarioSettingsRepository` | [2.Application/OutGame/Scenario/IScenarioSettingsRepository.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/Scenario/IScenarioSettingsRepository.cs) |
| 10 | `ITextAdvanceWaiter` | [2.Application/OutGame/Scenario/ITextAdvanceWaiter.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/Scenario/ITextAdvanceWaiter.cs) |
| 11 | `IScreenPresenter` | [2.Application/OutGame/Screen/IScreenPresenter.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/Screen/IScreenPresenter.cs) |
| 12 | `IScreenRuleRepository` | [2.Application/OutGame/Screen/IScreenRuleRepository.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/Screen/IScreenRuleRepository.cs) |
| 13 | `IScreenStateRepository` | [2.Application/OutGame/Screen/IScreenStateRepository.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/Screen/IScreenStateRepository.cs) |
| 14 | `IOwnedSkillRepository` | [2.Application/OutGame/SkillBuild/IOwnedSkillRepository.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/SkillBuild/IOwnedSkillRepository.cs) |
| 15 | `ISkillBuildRepository` | [2.Application/OutGame/SkillBuild/ISkillBuildRepository.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/SkillBuild/ISkillBuildRepository.cs) |
| 16 | `IOutGameSortieOutputPort` | [2.Application/OutGame/Sortie/IOutGameSortieOutputPort.cs](../../Assets/Scripts/Runtime/2.Application/OutGame/Sortie/IOutGameSortieOutputPort.cs) |
| 17 | `ILoadingOperationExecutor` | [2.Application/Persistent/Load/ILoadingOperationExecutor.cs](../../Assets/Scripts/Runtime/2.Application/Persistent/Load/ILoadingOperationExecutor.cs) |
| 18 | `ILoadingSession` | [2.Application/Persistent/Load/ILoadingSession.cs](../../Assets/Scripts/Runtime/2.Application/Persistent/Load/ILoadingSession.cs) |
| 19 | `ILoadingSessionFactory` | [2.Application/Persistent/Load/ILoadingSessionFactory.cs](../../Assets/Scripts/Runtime/2.Application/Persistent/Load/ILoadingSessionFactory.cs) |
| 20 | `ISceneInitializationReadiness` | [2.Application/Persistent/SceneManagement/ISceneInitializationReadiness.cs](../../Assets/Scripts/Runtime/2.Application/Persistent/SceneManagement/ISceneInitializationReadiness.cs) |
| 21 | `ISceneTransitionService` | [2.Application/Persistent/SceneManagement/ISceneTransitionService.cs](../../Assets/Scripts/Runtime/2.Application/Persistent/SceneManagement/ISceneTransitionService.cs) |

### A-2. DIP境界（正当）— 1.Domain が定義

| # | インターフェース | 定義ファイル |
| --- | --- | --- |
| 22 | `IAttackPipeline` | [1.Domain/InGame/Battle/IAttackPipeline.cs](../../Assets/Scripts/Runtime/1.Domain/InGame/Battle/IAttackPipeline.cs) |
| 23 | `IBuff` | [1.Domain/InGame/Buff/IBuff.cs](../../Assets/Scripts/Runtime/1.Domain/InGame/Buff/IBuff.cs) |
| 24 | `IBuffSystem` | [1.Domain/InGame/Buff/IBuffSystem.cs](../../Assets/Scripts/Runtime/1.Domain/InGame/Buff/IBuffSystem.cs) |
| 25 | `IStageClearRepository` | [1.Domain/OutGame/StageSelect/IStageClearRepository.cs](../../Assets/Scripts/Runtime/1.Domain/OutGame/StageSelect/IStageClearRepository.cs) |

`IBuff` / `IBuffSystem` は実装が各1件だが、これは[差分分析](../仕様書と実装の差分分析_2026-07-27.md)の
A-10で指摘した「バフシステムの配線未完了」の裏返し。
仕様上は攻撃/バフ/デバフの3ジャンルが必要なので、**実装が増える予定の正当な抽象**。

### A-3. DIP境界（正当）— 3.Adaptor が定義、4.View が実装

ViewModel / ViewSink 系。Adaptor は View を参照できないため必須。

| # | インターフェース | 定義ファイル |
| --- | --- | --- |
| 26 | `IAttackResultViewModel` | [3.Adaptor/InGame/Battle/IAttackResultViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/IAttackResultViewModel.cs) |
| 27 | `IDamageable` | [3.Adaptor/InGame/Battle/IDamageable.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/IDamageable.cs) |
| 28 | `ICharacterAnimationSignal` | [3.Adaptor/InGame/Animaiton/ICharacterAnimationSignal.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Animaiton/ICharacterAnimationSignal.cs) |
| 29 | `ICharacterAnimationViewContext` | [3.Adaptor/InGame/Animaiton/ICharacterAnimationViewContext.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Animaiton/ICharacterAnimationViewContext.cs) |
| 30 | `ICharacterAnimationViewModel` | [3.Adaptor/InGame/Animaiton/ICharacterAnimationViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Animaiton/ICharacterAnimationViewModel.cs) |
| 31 | `IDamageNumber` | [3.Adaptor/InGame/Enemy/IDamageNumber.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/IDamageNumber.cs) |
| 32 | `IEnemyWaveTimerView` | [3.Adaptor/InGame/Enemy/IEnemyWaveTimerView.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/IEnemyWaveTimerView.cs) |
| 33 | `INearestAttackPositionSearchViewModel` | [3.Adaptor/InGame/Enemy/INearestAttackPositionSearchViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/INearestAttackPositionSearchViewModel.cs) |
| 34 | `IShellLifeCycle` | [3.Adaptor/InGame/Enemy/IShellLifeCycle.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/IShellLifeCycle.cs) |
| 35 | `IShellSpawner` | [3.Adaptor/InGame/Enemy/IShellSpawner.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/IShellSpawner.cs) |
| 36 | `IMissionStepPopupView` | [3.Adaptor/InGame/Mission/IMissionStepPopupView.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/IMissionStepPopupView.cs) |
| 37 | `IMusicViewModel` | [3.Adaptor/InGame/Music/IMusicViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Music/IMusicViewModel.cs) |
| 38 | `IPlayerController` | [3.Adaptor/InGame/Player/IPlayerController.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Player/IPlayerController.cs) |
| 39 | `IScreenProjector` | [3.Adaptor/InGame/Reticle/IScreenProjector.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Reticle/IScreenProjector.cs) |
| 40 | `IStageResultViewModel` | [3.Adaptor/InGame/Result/IStageResultViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Result/IStageResultViewModel.cs) |
| 41 | `ISkillResultViewModel` | [3.Adaptor/InGame/Skill/ISkillResultViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/ISkillResultViewModel.cs) |
| 42 | `ISkillVisual` | [3.Adaptor/InGame/Skill/ISkillVisual.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/ISkillVisual.cs) |
| 43 | `IStageEffectViewModel` | [3.Adaptor/InGame/Stage/IStageEffectViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Stage/IStageEffectViewModel.cs) |
| 44 | `ITargetSystemViewModel` | [3.Adaptor/InGame/Target/ITargetSystemViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Target/ITargetSystemViewModel.cs) ※14メンバ・要分割（[16](16_SOLID原則.md)） |
| 45 | `ITargetableViewModel` | [3.Adaptor/InGame/Target/ITargetableViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Target/ITargetableViewModel.cs) |
| 46 | `IHUDEnemyHealthViewModel` | [3.Adaptor/InGame/UI/IHUDEnemyHealthViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/UI/IHUDEnemyHealthViewModel.cs) |
| 47 | `IIngameHudViewModel` | [3.Adaptor/InGame/UI/IIngameHudViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/UI/IIngameHudViewModel.cs) |
| 48 | `IBgmSelectorPlayer` | [3.Adaptor/Persistent/Music/IBgmSelectorPlayer.cs](../../Assets/Scripts/Runtime/3.Adaptor/Persistent/Music/IBgmSelectorPlayer.cs) |
| 49 | `IAnimationViewSink` | [3.Adaptor/OutGame/Scenario/IAnimationViewSink.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/IAnimationViewSink.cs) |
| 50 | `IBackgroundViewSink` | [3.Adaptor/OutGame/Scenario/IBackgroundViewSink.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/IBackgroundViewSink.cs) |
| 51 | `IFadeViewSink` | [3.Adaptor/OutGame/Scenario/IFadeViewSink.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/IFadeViewSink.cs) |
| 52 | `ILayerViewSink` | [3.Adaptor/OutGame/Scenario/ILayerViewSink.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/ILayerViewSink.cs) |
| 53 | `IPortraitViewSink` | [3.Adaptor/OutGame/Scenario/IPortraitViewSink.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/IPortraitViewSink.cs) |
| 54 | `ITextViewSink` | [3.Adaptor/OutGame/Scenario/ITextViewSink.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/ITextViewSink.cs) |
| 55 | `IScenarioCompletionViewSink` | [3.Adaptor/OutGame/Scenario/IScenarioCompletionViewSink.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/IScenarioCompletionViewSink.cs) |
| 56 | `IScreenController` | [3.Adaptor/OutGame/Screen/IScreenController.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Screen/IScreenController.cs) |
| 57 | `IScreenTransitionApplicable` | [3.Adaptor/OutGame/Screen/IScreenTransitionApplicable.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Screen/IScreenTransitionApplicable.cs) |
| 58 | `ISkillBuildViewModel` | [3.Adaptor/OutGame/SkillBuild/ISkillBuildViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillBuild/ISkillBuildViewModel.cs) |
| 59 | `IPlayerStatusShowable` | [3.Adaptor/OutGame/SkillTree/IPlayerStatusShowable.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/IPlayerStatusShowable.cs) |
| 60 | `IPlayerStatusViewModel` | [3.Adaptor/OutGame/SkillTree/IPlayerStatusViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/IPlayerStatusViewModel.cs) |
| 61 | `IPreviewVideoScreenViewModel` | [3.Adaptor/OutGame/SkillTree/IPreviewVideoScreenViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/IPreviewVideoScreenViewModel.cs) |
| 62 | `IPreviewVideoScreenViewShowable` | [3.Adaptor/OutGame/SkillTree/IPreviewVideoScreenViewShowable.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/IPreviewVideoScreenViewShowable.cs) |
| 63 | `ISkillDetailShowable` | [3.Adaptor/OutGame/SkillTree/ISkillDetailShowable.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/ISkillDetailShowable.cs) |
| 64 | `ISkillDetailViewModel` | [3.Adaptor/OutGame/SkillTree/ISkillDetailViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/ISkillDetailViewModel.cs) |
| 65 | `IStageConnectionViewModel` | [3.Adaptor/OutGame/StageSelect/IStageConnectionViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/StageSelect/IStageConnectionViewModel.cs) |
| 66 | `IStageDetailScreenShowable` | [3.Adaptor/OutGame/StageSelect/IStageDetailScreenShowable.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/StageSelect/IStageDetailScreenShowable.cs) |
| 67 | `IStageDetailViewModel` | [3.Adaptor/OutGame/StageSelect/IStageDetailViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/StageSelect/IStageDetailViewModel.cs) |
| 68 | `IStageNodeViewModel` | [3.Adaptor/OutGame/StageSelect/IStageNodeViewModel.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/StageSelect/IStageNodeViewModel.cs) |

**#49〜#55 の7つの `IXxxViewSink` はすべて
[4.View/OutGame/Scenario/ViewModel.cs](../../Assets/Scripts/Runtime/4.View/OutGame/Scenario/ViewModel.cs)
1クラスが実装**。ISPの模範例。

### A-4. 4.View / 6.Composition 内のインターフェース（判断が分かれる）

| # | インターフェース | 定義ファイル | 評価 |
| --- | --- | --- | --- |
| 69 | `IScreenView` | [4.View/OutGame/Screen/IScreenView.cs](../../Assets/Scripts/Runtime/4.View/OutGame/Screen/IScreenView.cs) | レジストリで多態利用。正当 |
| 70 | `ITargetable` | [4.View/InGame/Target/ITargetable.cs](../../Assets/Scripts/Runtime/4.View/InGame/Target/ITargetable.cs) | 正当 |
| 71 | `IShellPool` | [4.View/InGame/Enemy/IShellPool.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/IShellPool.cs) | 正当 |
| 72 | `ICameraTransform` | [6.Composition/Persistent/Camera/ICameraTransform.cs](../../Assets/Scripts/Runtime/6.Composition/Persistent/Camera/ICameraTransform.cs) | Composition内のみ。整理候補 |
| 73 | `IStageSceneInstance` | [6.Composition/InGame/Player/IStageSceneInstance.cs](../../Assets/Scripts/Runtime/6.Composition/InGame/Player/IStageSceneInstance.cs) | Composition内のみ。整理候補 |

### A-5. 削除対象（実装・参照ゼロ）— 3件

| # | インターフェース | ファイル |
| --- | --- | --- |
| 74 | `IShellInitializer` | [4.View/InGame/Enemy/IShellInitializer.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/IShellInitializer.cs) |
| 75 | `IViewAction` | [2.Application/InGame/Skill/IViewAction.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Skill/IViewAction.cs) |
| 76 | `IOutPutPort` | [3.Adaptor/OutGame/Scenario/IOutPutPort.Obsolete.cs](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Scenario/IOutPutPort.Obsolete.cs) |

**結論**: 135インターフェースのうち、削除して良いのは**3件のみ**。
インターフェースの数そのものは問題ではなかった。

## 付録B. 未使用ライブラリ（全3件）

| ライブラリ | manifest.json のバージョン | Runtime内の利用 |
| --- | --- | --- |
| `com.cysharp.zstring` | 2.6.0 | **0ファイル** |
| `com.cysharp.observablecollections` | 1.1.3 | **0ファイル** |
| `com.cysharp.unitask` | 2.5.10 | **1ファイル**（[AttackIntervalEvaluator.cs](../../Assets/Scripts/Runtime/2.Application/InGame/Battle/AttackIntervalEvaluator.cs)） |

移行対象の全12箇所は [11. 非同期処理 付録C](11_非同期処理の不純点.md#付録c-task-api-使用箇所unitask移行対象全12件) を参照。

## 付録C. 死んだ拡張点（全2件）

| # | 場所 | 内容 |
| --- | --- | --- |
| 1 | [SkillExecutionController.cs:16,97-113](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillExecutionController.cs) | `private const` に対する `switch`。3分岐中2つが到達不能 |
| 2 | [CameraSystemView.cs:179-180,223-253](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs) | `UpdateModeEnum` のため Update / FixedUpdate / LateUpdate を3つとも定義 |
