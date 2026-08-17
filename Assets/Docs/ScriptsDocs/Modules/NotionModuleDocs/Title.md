# 概要
> 💡 **モジュール概要**
> タイトル画面の表示、初回起動判定によるチュートリアル自動出撃のトリガー、オプション画面（音量設定・セーブデータリセット）を司るモジュールである。他モジュールから参照されることのない、アプリケーションの入口である。

| 項目 | 内容 |
| --- | --- |
| **モジュール名** | Title |
| **カテゴリ** | OutGame |
| **ステータス** | 実装済み |
| **最終更新日** | 2026-08-17 |

---

## 🏗️ クラス

| クラス名 | レイヤー | 役割・機能 |
| --- | --- | --- |
| **`TitleStartController`** | Adaptor | `StartGameAsync`でシーン遷移を駆動。多重タップを防止する |
| **`TitleSceneView`** | View | タイトル画面本体。タップでゲーム開始、オプションボタンでオプション画面表示 |
| **`MenuScreenView`** | View | メニュー画面（オプション/クレジットへの導線） |
| **`OptionsScreenView`** | View | オプション画面（音量設定タブ・データリセットタブのコンテナ） |
| **`CreditScreenView`** | View | クレジット画面 |
| **`VolumeSettingsTabView`** | View | BGM/SE音量スライダーを`MusicPlayer`/`SoundEffectVolumeManager`（具象クラス）に直接バインド |
| **`DataResetTabView`** | View | セーブデータリセットボタン。押下で`OutGameUIEvent.OnDataResetButtonClicked`を発火 |
| **`TitleSceneInitializer`** | Composition | 初回起動判定、各種View構築、データリセット処理を担当 |
| **`TitleScreenViewRegistry`** | Composition | `ScreenId`（Title/Menu/Options/Credit）と各Viewの対応表 |

> セーブデータリセット処理専用のクラスは存在せず、`TitleSceneInitializer.HandleDataResetButtonClicked`という私有メソッドとして実装されている。

### 🧩 Composition初期化情報

| 項目 | 内容 |
| --- | --- |
| **Initializerクラス** | `TitleSceneInitializer` |
| **Order** | 20（Titleシーン内） |
| **公開する ModuleContainer / ServiceLocator登録型** | 専用の`ModuleContainer`は無し。初回起動/データリセット時にチュートリアルステージの出撃準備を行い、遷移先を`TitleStartController`へ設定する |

---

## 🔗 モジュール結合

```mermaid
graph TD
    %% 定義 (接続のないレイヤーは省略)
    subgraph TitleModule [Title モジュール]
        T_Adaptor["Adaptor<br>TitleStartController"]
        T_View["View<br>TitleSceneView, VolumeSettingsTabView"]
        T_Composition["Composition<br>TitleSceneInitializer"]
        T_View --> T_Adaptor
        T_Adaptor --> T_Composition
    end

    subgraph SavedataModule [Persistent/Savedata モジュール]
        SD_Domain["Domain<br>SaveData, TutorialData"]
    end

    subgraph MusicModule [Music/Persistent モジュール]
        M_View["View<br>MusicPlayer, SoundEffectVolumeManager"]
    end

    subgraph SceneManagementModule [Persistent/SceneManagement モジュール]
        SM_Adaptor["Adaptor<br>SceneTransitionController"]
    end

    subgraph StageSelectModule [StageSelect モジュール]
        SS_Adaptor["Adaptor<br>BattleSortieSelectionService"]
    end

    %% 依存関係
    T_Composition -->|"IsTutorialCompleted判定・データリセット"| SD_Domain
    T_View -->|"音量取得・設定"| M_View
    T_Adaptor -->|"追加ロード・アンロード（初期化完了待機込み）"| SM_Adaptor
    T_Composition -->|"チュートリアルステージの出撃準備"| SS_Adaptor
```

### 📥 依存しているもの

* **`Persistent/Savedata`**
  * *依存箇所*: `SaveData`, `TutorialData`
  * *詳細*: `SaveData.Tutorial.IsTutorialCompleted`で初回起動判定を行う。読み書きはSymphonyFrameworkの`SaveStore`（`IsLoaded`/`Get`/`LoadAsync`/`DeleteAsync`）へ統合されました
* **`Music` / `Persistent`**
  * *依存箇所*: `MusicPlayer`, `SoundEffectVolumeManager`（具象クラス）
  * *詳細*: `VolumeSettingsTabView`がBGM/SE音量スライダーを直接バインドする。既存の`IVolumeManager`抽象は使用していない
* **`Persistent/SceneManagement`**
  * *依存箇所*: `SceneTransitionController`
  * *詳細*: `TitleStartController.StartGameAsync`が追加ロード・アンロードに使用する（`ISceneInitializationReadiness`によるモジュール初期化完了待機の恩恵を受ける）
* **`Persistent/Savedata`（初期スキル）**
  * *依存箇所*: `InitialSkillLoadoutService`
  * *詳細*: セーブデータ削除後に初期スキル構成を再適用する。これが無いとリセット直後にスキルが未装備のまま出撃できてしまう
* **`StageSelect`**
  * *依存箇所*: `StageTreeAsset`, `BattleSortieSelectionService`, `SelectedBattleStageState`
  * *詳細*: 初回起動時・データリセット後に`TryPrepareTutorialBattleSortie()`がステージツリーからチュートリアルノードを引き、バトル出撃の選択状態を組み立てて`TitleStartController`へ遷移先シーンを渡す

### 📤 依存されているもの

* なし
  * *詳細*: Titleはアプリケーションの入口であり、他モジュールから直接参照されることはない。チュートリアルの出撃準備もTitle側で完結しており、StageSelectはその結果として設定された選択状態を読むだけである

---

# 詳細

## 🧅レイヤー情報

### ① Domain
当モジュールでは使用していない。
### ② Application
当モジュールでは使用していない（汎用Screenモジュールの`ShowScreenUseCase`等を利用するが、Title固有のApplication層クラスはない）。
### ③ Adaptor
`TitleStartController`がシーン遷移を駆動し、多重タップを防止する。
### ④ View
`TitleSceneView`/`MenuScreenView`/`OptionsScreenView`/`CreditScreenView`という画面遷移チェーンと、`VolumeSettingsTabView`/`DataResetTabView`というオプションタブを担当する。
### ⑤ Infrastructure
当モジュールでは使用していない。
### ⑥ Composition
`TitleSceneInitializer`（Order 20）が、初回起動判定・各View構築・イベント購読・データリセット処理を一手に担う。

## 🔌 拡張ポイント

> 現状、ポリモーフィックな拡張ポイント（`SubclassSelector`等）はない。新しいオプションタブを追加する場合は、`VolumeSettingsTabView`/`DataResetTabView`と同様の`IDisposable`実装クラスを追加し、`TitleSceneInitializer`に組み込む形になる。

## 🔄処理フロー

主要な処理フローは、それぞれ子ページに分けている。

### ① 通常起動フロー
セーブデータのチュートリアル完了フラグに応じて遷移先を決定する。

```mermaid
sequenceDiagram
    autonumber
    participant Init as TitleSceneInitializer
    participant Save as SavedataSystem
    participant View as TitleSceneView
    actor Player as プレイヤー
    participant StartCtrl as TitleStartController

    Init ->> Save: SaveStore.IsLoaded<SaveData>()
    alt 読み込み済み
        Init ->> Save: SaveStore.Get<SaveData>()
    else 未読み込み
        Init ->> Save: SaveStore.LoadAsync<SaveData>()
    end
    Save -->> Init: SaveData（Tutorial.IsTutorialCompleted含む）
    alt IsTutorialCompleted == true
        Init ->> View: SetTargetSceneName(_targetSceneName)
    else 初回起動
        Init ->> View: SetTargetSceneName(_firstLaunchTargetSceneName)
        Init ->> Init: TryPrepareTutorialBattleSortie()
    end
    Player ->> View: 画面タップ
    View ->> StartCtrl: StartGameAsync(currentScene, targetScene)
    StartCtrl ->> StartCtrl: ChangeSceneKeepingLoadingAsync もしくは LoadAdditiveAsync → Titleシーンを UnloadAsync
```

### ② セーブデータリセットフロー
オプション画面からセーブデータを削除し、初回起動状態へ戻す。

```mermaid
sequenceDiagram
    autonumber
    actor Player as プレイヤー
    participant ResetTab as DataResetTabView
    participant Init as TitleSceneInitializer
    participant Save as SavedataSystem
    participant View as TitleSceneView

    Player ->> ResetTab: リセットボタン押下
    ResetTab -->> Init: OutGameUIEvent.OnDataResetButtonClicked
    Init ->> Save: SaveStore.DeleteAsync<SaveData>()
    Init ->> Save: SaveStore.LoadAsync<SaveData>()（再読込）
    Init ->> Save: InitialSkillLoadoutService で初期スキルを再適用
    Init ->> View: SetTargetSceneName(_firstLaunchTargetSceneName)
    Init ->> Init: TryPrepareTutorialBattleSortie()（再準備）
```