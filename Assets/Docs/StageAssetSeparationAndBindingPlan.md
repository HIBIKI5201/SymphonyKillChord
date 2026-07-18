# ステージアセット分割・接続方式変更 設計計画書

## 目的

現在の `StageNodeAsset` と `StageDefinition` は、`StageType` によってバトル用・シナリオ用フィールドの片方だけを利用している。この構造を型ごとに分離し、無効な組み合わせをデータ入力時と生成時に防止する。

また、ステージ間の接続とクリア後の進行方法を `StageBindAsset` に集約し、次の構成をデータとして表現できるようにする。

```text
Scenario01 --[AutoAdvance]----> BattleTutorial
BattleTutorial --[ManualSelection]--> Scenario02
Scenario02 --[ManualSelection]------> Battle01
```

Battle同士、Scenario同士を含む、すべてのステージ種別間の接続を許可する。

## 対象外

- 接続を有効化する `Condition` は今回導入しない。
- 複数候補を選ぶ `Priority` は今回導入しない。
- ステージ種別の交互接続は制約にしない。
- UI Toolkitの要素名をBindへ保持しない。

## データ構造

### StageAssetBase

生成可能なステージアセットの抽象基底クラスとする。次の共通情報を保持する。

- StageId
- 初期解放状態
- ステージ名
- フレーバーテキスト
- クリア報酬
- 遷移先シーン名

`Create()` で `StageNode` を生成する。具体的な `StageDefinition` の生成は派生クラスの仮想メソッドへ委譲する。

### BattleStageAsset

バトル固有の次の情報を保持し、`BattleStageDefinition` を生成する。

- バトルシーン名
- 敵Wave定義Addressablesキー
- ミッション定義
- チュートリアル指定

### ScenarioStageAsset

シナリオ固有のシナリオIDを保持し、`ScenarioStageDefinition` を生成する。

### StageBindAsset

1つの有向接続を表すScriptableObjectとする。

| フィールド | 内容 |
| --- | --- |
| FromStage | 接続元の `StageAssetBase` |
| ToStage | 接続先の `StageAssetBase` |
| AdvanceMode | `ManualSelection` または `AutoAdvance` |

`ManualSelection` は接続先を解放した後、通常どおりホームでプレイヤーの選択を待つ。`AutoAdvance` は接続元の再生完了後、接続先を自動開始する。

### StageTreeAsset

ステージアセット一覧とBindアセット一覧を集約し、Domainの `StageTree` を生成する。エディタ検証では次をエラーとして扱う。

- StageIdの未設定・重複
- From/Toの未設定
- 自己接続
- StageTreeに登録されていないステージへの接続
- 同一From/To接続の重複
- 同じFromから複数の `AutoAdvance` を設定
- 複数のチュートリアルステージを設定

## Domain構造

`StageDefinition` を共通情報を持つ抽象基底クラスへ変更し、次の具象型を導入する。

```text
StageDefinition
├─ BattleStageDefinition
└─ ScenarioStageDefinition
```

接続を表す `StageNodeConnection` は `StageAdvanceMode` を保持する。`StageTree` の構築時に接続をFrom/ToごとのDictionaryへ索引化し、ゲーム進行中の接続解決ではリフレクションを利用しない。

## 自動遷移

`StageTree.TryGetAutoAdvanceTarget()` が接続元IDから自動遷移先を取得する。ステージ選択時に、そこから連続する `AutoAdvance` をキューへ予約する。各予約は接続元ステージが今回のプレイで正常完了した場合に限り一度だけ消費するため、バトル失敗・中断・シーン遷移失敗では次のステージを開始しない。

同じ仕組みで `Battle → Battle`、`Battle → Scenario`、`Scenario → Battle`、`Scenario → Scenario` の全組み合わせを扱う。Scenarioが連続する場合はScenarioシーンを維持して次のシナリオを再生し、Battleを挟む場合は一度帰還先シーンへ戻った後に予約済みステージを開始する。

遷移先の起動処理は具象Definitionの型に応じて一度だけ分岐する。毎フレームの型判定、リフレクションによる型検索、Asset探索は行わない。

## パフォーマンス方針

- ScriptableObjectからDomainへの変換は `StageTreeAsset.Create()` 時に一度だけ行う。
- StageId検索と接続検索は初期化時に構築したDictionaryを使う。
- 実行時の型分岐は各ステージの開始要求時に行う定数時間のパターンマッチに限定する。
- `System.Reflection`、`Activator.CreateInstance`、実行時の派生型列挙は使用しない。

## 移行手順

1. `StageDefinition` を抽象化し、Battle/Scenarioの具象Definitionを追加する。
2. `StageAssetBase`、`BattleStageAsset`、`ScenarioStageAsset` を追加する。
3. `StageBindAsset` と `StageAdvanceMode` を追加する。
4. `StageTree` と `StageTreeAsset` をBind駆動へ変更する。
5. `NodeTransitionRule`、`NodeTransitionRuleResolver`、`NodeTransitionActionType` を廃止する。
6. Adaptor・Compositionの参照型と自動遷移予約処理を更新する。
7. 既存の `StageNodeAsset` とインライン接続を新アセットへ移行する。
8. コンパイルとアセット参照整合性を確認する。

## 既存マスターデータの移行方針

既存のステージ内容と接続順は維持する。旧実装がScenarioからBattleへの後続処理を生成していた接続は `AutoAdvance`、それ以外は `ManualSelection` として移行する。

## 完了条件

- BattleアセットにScenario固有フィールドが存在しない。
- ScenarioアセットにBattle固有フィールドが存在しない。
- Bindごとに手動選択と自動遷移を選択できる。
- 同種ステージを連続して接続できる。
- 旧 `StageNodeAsset` と旧インライン接続への参照が残っていない。
- Runtimeコードにリフレクションが追加されていない。
- 関連Assemblyがコンパイルできる。
