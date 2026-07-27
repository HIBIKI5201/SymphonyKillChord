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

## 【重大・YAGNI】インターフェース135ファイルの多くが実装1件

`interface I*` を宣言するファイルは **135** あり、うち相当数が実装1件以下。

```
IAnimationViewSink / IBackgroundViewSink / IFadeViewSink / ILayerViewSink
IAttackPipeline / IAttackResultViewModel / IBgmSelectorPlayer
ICameraTransform / ICharacterAnimationSignal / ICharacterAnimationViewContext
ICharacterAnimationViewModel / IDamageNumber / IHUDEnemyHealthViewModel
IIngameHudViewModel / IMissionStepPopupView / IMusicViewModel  ... 他多数
```

### 重要な区別

**実装1件でも正当なインターフェース**がある。Clean Architectureでは
依存を逆転させるために、内側の層がインターフェースを定義し外側が実装する。

| 分類 | 例 | 判定 |
| --- | --- | --- |
| **層をまたぐDIP境界** | `IEnemyWaveDefinitionRepository`（Applicationが定義・InfraStructureが実装） | **正当**。実装1件でも必要 |
| **層をまたぐDIP境界** | `IScenarioRepository`、`ILoadingOperationExecutor` | **正当** |
| **同一層内の純粋な間接化** | `IAnimationViewSink` / `IFadeViewSink` / `ILayerViewSink` / `IBackgroundViewSink` | **YAGNI違反**。Presenter→ViewSinkは同じAdaptor層内で1対1 |
| **同一層内の純粋な間接化** | `IStageEffectDefinition`（実装は `StageEffectDefinition` sealed のみ、値保持DTO） | **YAGNI違反** |

**修正方針**: 「このインターフェースは層境界を越えているか」を基準に棚卸しする。
越えていないものは削除候補。特にシナリオパイプラインの
`IXxxOutputPort` / `IXxxViewSink` 群は、[10. コード重複](10_コード重複と過剰抽象化.md) で
指摘した4層構造そのものであり、まとめて整理できる。

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
