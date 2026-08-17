# 概要
> 💡 **モジュール概要**
> ステージマップ（ノードグラフ）の表示・選択、クリア状態に基づくステージ解放、および選択ステージへの出撃（Sortie）を司るアウトゲームモジュールである。バトル出撃・シナリオ出撃・チュートリアル自動出撃という3種類の出撃経路を持つ。

| 項目 | 内容 |
| --- | --- |
| **モジュール名** | StageSelect |
| **カテゴリ** | OutGame |
| **ステータス** | 実装済み（一部TODO残存） |
| **最終更新日** | 2026-08-17 |

---

## 🏗️ クラス

| クラス名 | レイヤー | 役割・機能 |
| --- | --- | --- |
| **`StageId`** | Domain | ステージを識別するreadonly構造体（`int`ラップ） |
| **`StageType`** | Domain | `Battle`/`Scenario`を表すenum。出撃経路の分岐に使う |
| **`StageStatus`** | Domain | `Locked`/`Unlocked`/`Cleared`を表すenum |
| **`StageReward`** | Domain | クリア報酬（スキル構築/解放ポイント）を保持するreadonly struct |
| **`StageAdvanceMode`** | Domain | 接続元完了後に手動選択を待つか、自動遷移するかを表すenum |
| **`StageNodeConnection`** | Domain | ステージ間の前提関係と進行方法を表すreadonly struct |
| **`StageDefinition`** | Domain | ステージの共通メタデータを保持する抽象基底クラス |
| **`BattleStageDefinition`** | Domain | バトルシーン・ミッション・Wave定義・チュートリアル情報を保持する具象定義 |
| **`ScenarioStageDefinition`** | Domain | 再生するシナリオIDを保持する具象定義 |
| **`StageNode`** | Domain | `StageDefinition`+`StageStatus`を保持する可変Entity。`MarkAsCleared()`/`Unlock()`で状態遷移し`OnStatusChanged`を発火 |
| **`IStageClearRepository`** | Domain | クリア済みステージID取得の抽象（実装はInfrastructure層） |
| **`StageTree`** | Domain | 全ステージノードと接続関係を保持する集約。重複StageId・複数チュートリアルノードは`InvalidOperationException` |
| **`StageProgressService`** | Application | ステージクリア時の状態更新（クリアマーク＋次ステージ解放判定） |
| **`StageMapLayoutBuilder`** | Application | Bindをトポロジカル順に解析し、左から右へ並ぶ列・行位置を構築する |
| **`StageMapNodePosition`** | Application | 自動配置されたノードの列・行を保持するreadonly struct |
| **`StageSelectOpenUseCase`** | Application | 画面再オープン時のセーブデータ一括同期・新規クリア差分検出 |
| **`BattleSortieSelectionService`** | Adaptor | バトル出撃用の選択状態を構築する |
| **`StageDetailPresenter`** | Adaptor | 選択ノードから`StageDetailDTO`を構築 |
| **`StageNodePresenter`** | Adaptor | `StageNode.OnStatusChanged`を購読し、解放時は接続線アニメーションを再生してから状態をViewへ反映 |
| **`StageNodeAnimationSequencer`** | Adaptor | 複数ノード間のアニメーションをFIFOで直列化する共有シーケンサー |
| **`StageSelectController`** | Adaptor | ノード選択・詳細表示・出撃情報の取得を仲介 |
| **`NodeTransitionExecutor`** / **`PendingNodeTransition`** / **`PendingNodeTransitionState`** | Adaptor | 自動遷移の予約情報を保持し、後続の遷移チェーンを順に実行する |
| **`IStageNodeViewModel`** / **`IStageConnectionViewModel`** / **`IStageDetailViewModel`** / **`IStageDetailScreenShowable`** | Adaptor | ノード・接続線・詳細画面それぞれのView側契約 |
| **`StageStatusView`** | Adaptor | View層向けにステージ状態を表す列挙型 |
| **`SelectedBattleStageState`** | Adaptor | 選択中バトルステージの保持（namespace上は`Adaptor.InGame.StageSelect`。OutGame側が書き込み、InGame側が読み取る） |
| **`StageDetailScreenView`** | View | ステージ詳細画面（UI Toolkit）。出撃ボタン等を提供 |
| **`StageNodeView`** | View | ステージノード1件の見た目・クリック検知 |
| **`StageNodeConnectionView`** | View | ノード間接続線の解放アニメーション |
| **`StageAssetBase`** | Infrastructure | ステージ共通入力を保持し、Domainノードを生成する抽象基底ScriptableObject |
| **`BattleStageAsset`** | Infrastructure | バトル固有入力から`BattleStageDefinition`を生成するScriptableObject |
| **`ScenarioStageAsset`** | Infrastructure | シナリオ固有入力から`ScenarioStageDefinition`を生成するScriptableObject |
| **`StageBindAsset`** | Infrastructure | From/Toステージと`ManualSelection`/`AutoAdvance`を保持するScriptableObject |
| **`StageTreeAsset`** | Infrastructure | ステージツリー全体の定義を保持するアセット |
| **`SaveDataClearStageRepository`** | Infrastructure | `IStageClearRepository`実装。セーブデータからクリア済みステージIDを取得する |
| **`StageTreeAsset`** | Infrastructure | ステージとBindを集約し、`OnValidate()`でID・参照・接続重複を検証するScriptableObject |
| **`SaveDataClearStageRepository`** | Infrastructure | `IStageClearRepository`実装。`StageProgressData.ClearDatas`から実データを返す（スタブではない） |
| **`StageSelectInitializer`** | Composition | StageTreeからノード・接続線を動的生成・配置し、出撃要求を仲介する |
| **`StageSelectInitializer`** | Composition | ステージツリーの構築・登録（Order 110） |

### 🧩 Composition初期化情報

| 項目 | 内容 |
| --- | --- |
| **Initializerクラス** | `StageSelectInitializer` |
| **Order** | 110（`ScreenInitializer`(100)の後、`SkillTreeInitializer`(120)の前） |
| **公開する ModuleContainer / ServiceLocator登録型** | 専用の`ModuleContainer`は無く、`SelectedBattleStageState`・`SelectedMissionState`を個別にServiceLocatorへ登録する。出撃の実行はSortieモジュールが担う |

---

## 🔗 モジュール結合

```mermaid
graph TD
    %% 定義 (接続のないレイヤーは省略)
    subgraph StageSelectModule [StageSelect モジュール]
        SS_Domain["Domain<br>StageTree, StageNode"]
        SS_Adaptor["Adaptor<br>StageSelectController, SelectedBattleStageState"]
        SS_Composition["Composition<br>StageSelectInitializer"]
        SS_Domain --> SS_Adaptor
        SS_Adaptor --> SS_Composition
    end

    subgraph TitleModule [Title モジュール]
        T_Composition["Composition<br>TitleSceneInitializer"]
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
  * *依存箇所*: なし（参照の向きは逆）
  * *詳細*: チュートリアルの自動出撃はTitle側が主導する。`TitleSceneInitializer`が`StageTreeAsset`からチュートリアルノードを引き、`BattleSortieSelectionService`で選択状態を組み立てる
* **`Mission`**
  * *依存箇所*: `SelectedMissionState`, `OutGameMissionSelectController`
  * *詳細*: バトル出撃時に選択ステージの`MissionDefinition`をミッション選択状態へ設定する
* **`Scenario`**
  * *依存箇所*: `SelectedScenarioState`
  * *詳細*: シナリオ種別のステージ出撃時に、選択シナリオIDを設定する
* **`Persistent/Savedata`**
  * *依存箇所*: `StageProgressData`, `SaveData`
  * *詳細*: `SaveDataClearStageRepository`がクリア済みステージ一覧を読み込み、`StageSelectOpenUseCase`が画面表示時に一括反映す

### 📤 依存されているもの

* **`Enemy`**
  * *参照箇所*: `SelectedBattleStageState.CurrentStageDefinition.EnemyWaveDefinitionId`
  * *詳細*: `EnemyInitializer`（Order 700）が共通のWave定義リポジトリからステージ固有の定義をID検索するために参照する。選択が無い場合やIDが未登録の場合は初期化に失敗する
* **`InGame Bootstrap`**
  * *参照箇所*: `SelectedBattleStageState.HasSelectedBattleStage`, `BattleSceneName`
  * *詳細*: `IngameComposition`がInGameシーン起動時にこの状態を読み取り、バトルシーンを追加ロードする。選択が無い場合は初期化が失敗する
* **`Result`**
  * *参照箇所*: `SelectedBattleStageState`
  * *詳細*: リザルト画面がクリアデータの書き戻し等に選択中ステージ情報を参照する

---

# 詳細

## 🧅レイヤー情報

### ① Domain
ステージの識別（`StageId`）、種別（`StageType`）、状態（`StageStatus`）、報酬（`StageReward`）、接続関係（`StageNodeConnection`）、そして全体を束ねる集約`StageTree`を保持する。重複IDや複数チュートリアルノードは構築時に例外で検出する。
### ② Application
ステージクリア時の状態更新（`StageProgressService`）、画面表示時のセーブデータ同期（`StageSelectOpenUseCase`）、およびステージマップの自動レイアウト（`StageMapLayoutBuilder`）を実装する。出撃種別の分岐はSortieモジュールにある。
### ③ Adaptor
ノード選択・詳細表示を仲介する`StageSelectController`、クロスシーン状態（`SelectedBattleStageState`）、バトル出撃の選択状態を組み立てる`BattleSortieSelectionService`、自動遷移チェーンを順に実行する`NodeTransitionExecutor`を定義する。
### ④ View
ステージ詳細画面（`StageDetailScreenView`）、ノード（`StageNodeView`）、接続線アニメーション（`StageNodeConnectionView`）というUI Toolkitコンポーネント群を担当する。画面のコンテナ自体（表示/非表示の切り替え）はScreenモジュールが所有し、StageSelectはその中身を構築する。
### ⑤ Infrastructure
`BattleStageAsset`/`ScenarioStageAsset`がステージ内容、`StageBindAsset`が接続と進行方法、`StageTreeAsset`が全体構成を保持する。`SaveDataClearStageRepository`はクリア済みステージIDをセーブデータから取得する。
### ⑥ Composition
`StageSelectInitializer`（Order 110）がステージマップ構築・出撃仲介・チュートリアル自動出撃トリガーを担当し、`OutGameSortieInitializer`（Order 20）が出撃ユースケース一式を構築する。

## 🔌 拡張ポイント

> 新しいバトルまたはシナリオステージは対応する具象Assetを作成し、`StageBindAsset`と`StageTreeAsset`へ登録するデータ追加のみで対応できる。接続元と接続先のステージ種別の組み合わせに制限はない。

## 🔄処理フロー

主要な処理フローは、それぞれ子ページに分けている。

### ① 通常バトル出撃フロー（ステージ選択→戦闘準備画面）
プレイヤーがステージノードを選択し、出撃ボタンを押すまでの流れである。

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
チュートリアル未完了の場合、Titleシーンの時点で出撃先を決めてしまい、ステージ選択画面を経由しない。かつてはクロスシーンの一発フラグをStageSelect側が消費する形だったが、現在はTitle側で完結する。

```mermaid
sequenceDiagram
    autonumber
    participant Title as TitleSceneInitializer
    participant Tree as StageTreeAsset / StageTree
    participant Sel as BattleSortieSelectionService
    participant StartCtrl as TitleStartController

    Note over Title: SaveData.Tutorial.IsTutorialCompleted が false
    Title ->> Tree: TryGetTutorialNode()
    Tree -->> Title: チュートリアルのStageNode
    Title ->> Sel: TryPrepareBattleSortie(定義, 遷移先シーン)
    Sel -->> Title: SelectedBattleStageState 等を設定
    Title ->> StartCtrl: SetTutorialBattleTarget(遷移先シーン)
    Note over StartCtrl: 画面タップでステージ選択を挟まず直接出撃する
```
