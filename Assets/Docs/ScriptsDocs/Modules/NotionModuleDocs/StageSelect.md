# 概要
> 💡 **モジュール概要**
> ステージマップ（ノードグラフ）の表示・選択、クリア状態に基づくステージ解放、および選択ステージへの出撃（Sortie）を司るアウトゲームモジュールです。バトル出撃・シナリオ出撃・チュートリアル自動出撃という3種類の出撃経路を持ちます。

| 項目 | 内容 |
| --- | --- |
| **モジュール名** | StageSelect |
| **カテゴリ** | OutGame |
| **ステータス** | 実装済み（一部TODO残存、既知の課題を参照） |
| **最終更新日** | 2026-07-15 |

---

## 🏗️ クラス

| クラス名 | レイヤー | 役割・機能 |
| --- | --- | --- |
| **`StageId`** | Domain | ステージを識別するreadonly構造体（`int`ラップ） |
| **`StageType`** | Domain | `Battle`/`Scenario`を表すenum。出撃経路の分岐に使う |
| **`StageStatus`** | Domain | `Locked`/`Unlocked`/`Cleared`を表すenum |
| **`StageReward`** | Domain | クリア報酬（スキル構築/解放ポイント）を保持するreadonly struct |
| **`StageNodeConnection`** | Domain | ステージ間の前提関係（`FromStageId`→`ToStageId`）を表すreadonly struct |
| **`StageDefinition`** | Domain | ステージの不変メタデータ（名前・報酬・遷移先シーン・ミッション定義・チュートリアルフラグ・Wave定義キー等） |
| **`StageNode`** | Domain | `StageDefinition`+`StageStatus`を保持する可変Entity。`MarkAsCleared()`/`Unlock()`で状態遷移し`OnStatusChanged`を発火 |
| **`IStageClearRepository`** | Domain | クリア済みステージID取得の抽象（実装はInfrastructure層） |
| **`StageTree`** | Domain | 全ステージノードと接続関係を保持する集約。重複StageId・複数チュートリアルノードは`InvalidOperationException` |
| **`StageProgressService`** | Application | ステージクリア時の状態更新（クリアマーク＋次ステージ解放判定） |
| **`StageSelectOpenUseCase`** | Application | 画面再オープン時のセーブデータ一括同期・新規クリア差分検出 |
| **`IOutGameSortieOutputPort`** | Application | 出撃結果（戦闘準備画面表示/即時出撃/シナリオUI切替）の出力境界 |
| **`OutGameSortieUseCase`** | Application | 出撃種別に応じた分岐処理（`RequestSortieAsync`/`RequestImmediateBattleSortie`） |
| **`StageDetailPresenter`** | Adaptor | 選択ノードから`StageDetailDTO`を構築 |
| **`StageNodePresenter`** | Adaptor | `StageNode.OnStatusChanged`を購読し、解放時は接続線アニメーションを再生してから状態をViewへ反映 |
| **`StageNodeAnimationSequencer`** | Adaptor | 複数ノード間のアニメーションをFIFOで直列化する共有シーケンサー |
| **`StageSelectController`** | Adaptor | ノード選択・詳細表示・出撃情報の取得を仲介 |
| **`TutorialSortieRequestState`** | Adaptor | Title→StageSelectのクロスシーン一発フラグ（チュートリアル自動出撃要求） |
| **`OutGameSortieController`** | Adaptor | `OutGameSortieUseCase`への薄いパススルー |
| **`SelectedBattleStageState`** | Adaptor | 選択中バトルステージの保持（namespace上は`Adaptor.InGame.StageSelect`。OutGame側が書き込み、InGame側が読み取る） |
| **`StageDetailScreenView`** | View | ステージ詳細画面（UI Toolkit）。出撃ボタン等を提供 |
| **`StageNodeView`** | View | ステージノード1件の見た目・クリック検知 |
| **`StageNodeConnectionView`** | View | ノード間接続線の解放アニメーション |
| **`StageNodeAsset`** | Infrastructure | ステージノード定義のScriptableObject。`_stageId>0`とBattleステージの`EnemyWaveDefinitionAssetKey`必須を検証 |
| **`StageTreeAsset`** | Infrastructure | 全ステージノード＋接続関係のScriptableObject。`OnValidate()`でエディタ時に重複IDを警告 |
| **`SaveDataClearStageRepository`** | Infrastructure | `IStageClearRepository`実装。`StageProgressData.ClearDatas`から実データを返す（スタブではない） |
| **`StageSelectInitializer`** | Composition | ステージマップの構築、出撃要求の仲介、チュートリアル自動出撃のトリガー |
| **`OutGameSortieInitializer`** | Composition | `OutGameSortieUseCase`/`OutGameSortieController`の構築・登録 |
| **`OutGameSortieOutputPort`** | Composition | `IOutGameSortieOutputPort`実装。`OutGameUIEvent`と`InputComposition`の両方に依存するためComposition層に配置 |

### 🧩 Composition初期化情報

| 項目 | 内容 |
| --- | --- |
| **Initializerクラス** | `StageSelectInitializer` / `OutGameSortieInitializer` |
| **Order** | `StageSelectInitializer` = 110（`ScreenInitializer`(100)の後、`SkillTreeInitializer`(120)の前） / `OutGameSortieInitializer` = 20 |
| **公開する ModuleContainer / ServiceLocator登録型** | 専用の`ModuleContainer`は無く、`SelectedBattleStageState`・`SelectedMissionState`（個別インスタンス）・`OutGameSortieController`をServiceLocatorへ直接登録（既知の課題を参照） |

---

## 🔗 モジュール結合

```mermaid
graph TD
    %% 定義 (接続のないレイヤーは省略)
    subgraph StageSelectModule [StageSelect モジュール]
        SS_Domain["Domain<br>StageTree, StageNode"]
        SS_Adaptor["Adaptor<br>StageSelectController, SelectedBattleStageState"]
        SS_Composition["Composition<br>StageSelectInitializer, OutGameSortieController"]
        SS_Domain --> SS_Adaptor
        SS_Adaptor --> SS_Composition
    end

    subgraph TitleModule [Title モジュール]
        T_Adaptor["Adaptor<br>TutorialSortieRequestState"]
    end

    subgraph MissionModule [Mission モジュール]
        MS_Adaptor["Adaptor<br>SelectedMissionState"]
    end

    subgraph ScenarioModule [Scenario モジュール]
        SC_Adaptor["Adaptor<br>SelectedScenarioState"]
    end

    subgraph SavedataModule [Persistent/Savedata モジュール]
        SD_Domain["Domain<br>StageProgressData"]
    end

    subgraph EnemyModule [Enemy モジュール]
        E_Composition["Composition<br>EnemyInitializer"]
    end

    subgraph InGameBootstrapModule [InGame Bootstrap モジュール]
        IG_Composition["Composition<br>IngameComposition"]
    end

    %% 依存関係
    T_Adaptor -->|"チュートリアル自動出撃要求"| SS_Composition
    SS_Composition -->|"ミッション選択"| MS_Adaptor
    SS_Composition -->|"シナリオ選択"| SC_Adaptor
    SD_Domain -->|"クリア済みステージID"| SS_Domain
    SS_Adaptor -->|"選択中ステージ情報の取得"| E_Composition
    SS_Adaptor -->|"選択中ステージ情報の取得"| IG_Composition
```

### 📥 依存しているもの

* **`Title`**
  * *依存箇所*: `TutorialSortieRequestState`
  * *詳細*: Titleが初回起動時等に登録・リクエストした一発フラグを`StageSelectInitializer.Ready()`が消費し、チュートリアルステージへ自動出撃します。
* **`Mission`**
  * *依存箇所*: `SelectedMissionState`, `OutGameMissionSelectController`
  * *詳細*: バトル出撃時に選択ステージの`MissionDefinition`をミッション選択状態へ設定します。
* **`Scenario`**
  * *依存箇所*: `SelectedScenarioState`
  * *詳細*: シナリオ種別のステージ出撃時に、選択シナリオIDを設定します。
* **`Persistent/Savedata`**
  * *依存箇所*: `StageProgressData`, `SaveData`
  * *詳細*: `SaveDataClearStageRepository`がクリア済みステージ一覧を読み込み、`StageSelectOpenUseCase`が画面表示時に一括反映します。

### 📤 依存されているもの

* **`Enemy`**
  * *参照箇所*: `SelectedBattleStageState.CurrentStageDefinition.EnemyWaveDefinitionAssetKey`
  * *詳細*: `EnemyInitializer`（Order 700）がステージ固有のWave定義をAddressablesロードするために参照します。選択が無い場合は初期化に失敗します。
* **`InGame Bootstrap`**
  * *参照箇所*: `SelectedBattleStageState.HasSelectedBattleStage`, `BattleSceneName`
  * *詳細*: `IngameComposition`がInGameシーン起動時にこの状態を読み取り、バトルシーンを追加ロードします。選択が無い場合は初期化が失敗します。
* **`Result`**
  * *参照箇所*: `SelectedBattleStageState`
  * *詳細*: リザルト画面がクリアデータの書き戻し等に選択中ステージ情報を参照します。

---

# 詳細

## 🧅レイヤー情報

### ① Domain
ステージの識別（`StageId`）、種別（`StageType`）、状態（`StageStatus`）、報酬（`StageReward`）、接続関係（`StageNodeConnection`）、そして全体を束ねる集約`StageTree`を保持します。重複IDや複数チュートリアルノードは構築時に例外で検出します。
### ② Application
ステージクリア時の状態更新（`StageProgressService`）、画面表示時のセーブデータ同期（`StageSelectOpenUseCase`）、および出撃種別に応じた分岐処理（`OutGameSortieUseCase`）を実装します。
### ③ Adaptor
ノード選択・詳細表示を仲介する`StageSelectController`、クロスシーン状態（`SelectedBattleStageState`、`TutorialSortieRequestState`）、出撃要求の薄いパススルー（`OutGameSortieController`）を定義します。
### ④ View
ステージ詳細画面（`StageDetailScreenView`）、ノード（`StageNodeView`）、接続線アニメーション（`StageNodeConnectionView`）というUI Toolkitコンポーネント群を担当します。画面のコンテナ自体（表示/非表示の切り替え）はScreenモジュールが所有し、StageSelectはその中身を構築します。
### ⑤ Infrastructure
`StageNodeAsset`/`StageTreeAsset`がステージ構成をScriptableObjectとして保持し、`SaveDataClearStageRepository`がクリア済みステージIDをセーブデータから取得します。
### ⑥ Composition
`StageSelectInitializer`（Order 110）がステージマップ構築・出撃仲介・チュートリアル自動出撃トリガーを担当し、`OutGameSortieInitializer`（Order 20）が出撃ユースケース一式を構築します。

## 🔌 拡張ポイント

> 現状、ポリモーフィックな拡張ポイント（`SubclassSelector`等）はありません。新しいステージを追加する場合は`StageNodeAsset`を新規作成し`StageTreeAsset`へ登録するデータ追加のみで対応できます（コード変更不要）。

## 🔄処理フロー

主要な処理フローごとに分けて記述します。

### ① 通常バトル出撃フロー（ステージ選択→戦闘準備画面）
プレイヤーがステージノードを選択し、出撃ボタンを押すまでの流れです。

```mermaid
sequenceDiagram
    autonumber
    actor Player as プレイヤー
    participant NodeView as StageNodeView
    participant Init as StageSelectInitializer
    participant Controller as StageSelectController
    participant SortieCtrl as OutGameSortieController
    participant SortieUC as OutGameSortieUseCase

    Player ->> NodeView: ノードをクリック
    NodeView -->> Init: OutGameUIEvent.OnStageNodeSelected
    Init ->> Controller: OnStageNodeSelected(nodeId)
    Controller ->> Controller: StageDetailPresenter.Push → 詳細画面表示
    Player ->> Init: 出撃ボタン押下 (OnSortieRequested)
    Init ->> Init: SelectedBattleStageState / SelectedMissionState を設定
    Init ->> SortieCtrl: RequestSortieAsync(Battle, ...)
    SortieCtrl ->> SortieUC: RequestSortieAsync
    SortieUC ->> SortieUC: 戦闘準備画面を表示（IOutGameSortieOutputPort）
```

### ② チュートリアル自動出撃フロー（初回起動時）
Titleが要求したフラグをStageSelectが消費し、戦闘準備画面を経由せず直接出撃します。

```mermaid
sequenceDiagram
    autonumber
    participant Title as TitleSceneInitializer
    participant Flag as TutorialSortieRequestState
    participant Init as StageSelectInitializer (Ready)
    participant Tree as StageTree
    participant SortieCtrl as OutGameSortieController

    Title ->> Flag: Request()（ServiceLocatorへ登録）
    Note over Init: OutGameシーンのReady()フェーズ
    Init ->> Flag: IsRequested を確認
    Init ->> Tree: TryGetTutorialNode()
    Tree -->> Init: チュートリアルStageNode
    Init ->> Init: SelectedBattleStageState等を設定
    Init ->> SortieCtrl: RequestImmediateBattleSortie(targetSceneName)
    Note over SortieCtrl: 戦闘準備画面を経由せず即時出撃
    Init ->> Flag: TryConsume() + ServiceLocatorから登録解除
```
