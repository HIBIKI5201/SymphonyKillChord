# 概要
> 💡 **モジュール概要**
> インゲーム中のプレイヤーキャラクターの移動（通常移動・回避）、入力制御（インプットバッファリング）、および攻撃・戦闘の実行制御（`PlayerAttackController`）を司るモジュールです。

| 項目 | 内容 |
| --- | --- |
| **モジュール名** | Player |
| **カテゴリ** | InGame |
| **ステータス** | 実装済み |
| **最終更新日** | 2026-08-17 |

---

## 🏗️ クラス

| クラス名 | レイヤー | 役割・機能 |
| --- | --- | --- |
| **`PlayerMoveSpec`** | Domain | 移動速度や回避距離などの設定値を保持するピュアクラス |
| **`PlayerApplication`** | Application | プレイヤーの状態・挙動を総括するメインユースケース |
| **`PlayerMovementApplication`** | Application | 通常移動時の物理演算・補間 |
| **`PlayerDodgeMovementApplication`** | Application | 回避アクションのタイムラインと無敵時間処理 |
| **`PlayerController`** | Adaptor | 入力バッファから操作を取り出してApplicationに委譲するコントローラー（`IPlayerController`実装） |
| **`PlayerHealthHudPresenter`** | Adaptor | プレイヤーHPをHUDへ反映するPresenter |
| **`PlayerInputSuppressionState`** | Adaptor | プレイヤー入力を一定時間だけ無効化する状態を管理 |
| **`PlayerAttackController`** | Adaptor | プレイヤーの攻撃実行を制御。`OnAttackExecuted`イベントを公開し、Missionモジュール等から購読される（namespace上は`Adaptor.InGame.Battle`） |
| **`PlayerView`** | View | 実際のRigidbodyやTransformを操作するMonoBehaviour |
| **`PlayerAttackWeaponView`** | View | 攻撃BeatTypeに応じた武器モデルの表示切替と攻撃SE再生 |
| **`WeaponItemView`** | View | 武器1本分の表示切替・SE再生・エフェクト再生 |
| **`PlayerAttackWeaponConfig`** / **`PlayerAttackAnimationConfig`** / **`PlayerAttackAnimationEntry`** | View | 攻撃時の武器とアニメーションの対応設定 |
| **`PlayerGroundOffsetView`** | View | 接地位置の補正 |
| **`PlayerSpawnPoint`** | View | プレイヤー生成位置を示すマーカー |
| **`PlayerMoveSpecAsset`** | Infrastructure | 移動・回避の設定値を保持するScriptableObject（`PlayerMoveSpec`の供給元） |
| **`PlayerInitializer`** | Composition | インプット、カメラ、HUD、攻撃制御などのシステムをプレイヤー具象にDIする |
| **`PlayerModuleContainer`** | Composition | `PlayerInitializer`/`PlayerView`/`PlayerEntity`/`PlayerAttackController`をServiceLocatorへ公開するContainer。Enemy/Bossモジュールが参照する |
| **`PlayerStatusBonusInitializer`** / **`PlayerStatusBonusModuleContainer`** | Composition | インゲームで適用するステータスボーナスの初期化と公開（Order 490） |
| **`StageSceneInitializer`** / **`IStageSceneInstance`** | Composition | ステージシーン内で共有する参照（生成位置など）の登録と公開 |
| **`PlayerMoveSpecDebug`** | Composition | 移動設定値をデバッグ表示・調整するための補助 |

### 🧩 Composition初期化情報

| 項目 | 内容 |
| --- | --- |
| **Initializerクラス** | `PlayerInitializer` |
| **Order** | 500（`PlayerInitializer`）／490（`PlayerStatusBonusInitializer`） |
| **公開する ModuleContainer / ServiceLocator登録型** | `PlayerModuleContainer`（`PlayerInitializer`, `PlayerView`, `PlayerEntity`（`CharacterEntity`）, `PlayerAttackController`を保持） |

---

## 🔗 モジュール結合

```mermaid
graph TD
    %% 定義 (接続のないレイヤーは省略)
    subgraph PlayerModule [Player モジュール]
        P_Adaptor["Adaptor<br>PlayerController, PlayerAttackController"]
        P_Composition["Composition<br>PlayerInitializer, PlayerModuleContainer"]
        P_Adaptor --> P_Composition
    end

    subgraph PersistentInputModule [Persistent/Input モジュール]
        PI_View["View<br>PlayerInputView"]
    end

    subgraph MusicModule [Music モジュール]
        M_App["Application<br>IMusicSyncService"]
    end

    subgraph CameraModule [Camera / Persistent モジュール]
        C_Composition["Composition<br>ICameraTransform"]
    end

    subgraph TargetModule [Target モジュール]
        T_Adaptor["Adaptor<br>TargetSystemController"]
    end

    subgraph MissionModule [Mission モジュール]
        MS_Adaptor["Adaptor<br>MissionEventController, MissionProgressRecorderController"]
    end

    subgraph EnemyModule [Enemy モジュール]
        E_Composition["Composition<br>EnemyInitializer, BossInitializer"]
    end

    %% 依存関係
    PI_View -->|"入力バッファ経由の操作"| P_Adaptor
    P_Adaptor -->|"ジャストタイミング取得"| M_App
    P_Composition -->|"カメラ正面方向の取得"| C_Composition
    P_Adaptor -->|"攻撃対象の解決"| T_Adaptor
    P_Adaptor -->|"死亡/被ダメージ/回避成績を通知"| MS_Adaptor
    MS_Adaptor -->|"OnAttackExecuted / OnHealthChanged を購読"| P_Adaptor
    E_Composition -->|"PlayerModuleContainer を直接取得"| P_Composition
```

### 📥 依存しているもの

* **`Persistent/Input`**
  * *依存箇所*: `PlayerInputView`, `InputBufferingQueue`
  * *詳細*: プレイヤーの入力デバイスからの入力を「入力バッファリングキュー」経由で受け取り、遅延フレームのない滑らかな先行入力を実現します。
* **`Music`**
  * *依存箇所*: `IMusicSyncService`, `MusicSyncState`
  * *詳細*: 回避（Dodge）のリズム同期判定や、アクション時のジャストタイミングの取得で使用します。
* **`Camera` / `Persistent`**
  * *依存箇所*: `ICameraTransform`（Persistent Composition層の抽象）
  * *詳細*: プレイヤーが移動する際、カメラの現在の正面方向を基準にして進行方向を計算するために使用します。
* **`Target`**
  * *依存箇所*: `TargetSystemController`, `ITargetableViewModel`
  * *詳細*: `PlayerAttackController`の攻撃対象解決やスキル発動時のターゲット補正で使用します。
* **`InGame/UI & HUD`**
  * *依存箇所*: `InGameHudInitializer`, `SkillInputProgressUIInitializer`
  * *詳細*: プレイヤー自身のHPやスキルのクールダウン、スキル発動のリズム入力進捗を画面に反映するため、各UI初期化に依存（取得してセットアップ）します。
* **`Mission`**
  * *依存箇所*: `MissionEventController`
  * *詳細*: プレイヤーの死亡や被ダメージ、回避成功実績などのイベントをミッションシステムへ通知します。

### 📤 依存されているもの

* **`Enemy`**
  * *参照箇所*: `PlayerModuleContainer`（Compositionでの直接結合）, `CharacterEntity`
  * *詳細*: 敵のAI（移動目標・攻撃先）として、プレイヤーのTransformやEntity情報を提供します。
* **`Camera`**
  * *参照箇所*: `PlayerInputView`
  * *詳細*: カメラが回転操作を受け取るための入力ソースとして、プレイヤーインプットを参照します。
* **`Mission`**
  * *参照箇所*: `PlayerAttackController.OnAttackExecuted`, `CharacterEntity.OnHealthChanged`
  * *詳細*: `MissionProgressRecorderController`がこれらのイベントを購読し、被ダメージ量・使用武器種・コンボ数をミッション評価条件として記録します。

---

# 詳細

## 🧅レイヤー情報

### ① Domain
移動速度や回避距離などの設定値をまとめた`PlayerMoveSpec`を保持します。
### ② Application
プレイヤーの状態・挙動を総括する`PlayerApplication`、通常移動の物理演算・補間を担う`PlayerMovementApplication`、回避アクションのタイムラインと無敵時間処理を担う`PlayerDodgeMovementApplication`といった、移動・回避に特化したピュアなロジッククラスを実装します。
### ③ Adaptor
入力バッファから操作を取り出してApplicationに委譲する`PlayerController`と、攻撃実行を制御し`OnAttackExecuted`イベントを公開する`PlayerAttackController`（実体は`Adaptor.InGame.Battle`名前空間）を定義します。
### ④ View
実際のRigidbodyやTransformを操作する`PlayerView`が、MonoBehaviourとしての表示・物理制御を担当します。
### ⑤ Infrastructure
`PlayerMoveSpecAsset`が移動・回避の設定値をScriptableObjectとして保持し、Domainの`PlayerMoveSpec`へ供給します。
### ⑥ Composition
`PlayerInitializer`（Order 500）がインプット・カメラ・HUD・攻撃制御などのシステムをプレイヤー具象にDIし、`PlayerModuleContainer`として他モジュールへ公開します。`PlayerStatusBonusInitializer`（Order 490）が先に走り、スキルビルド由来のステータスボーナスを適用します。`StageSceneInitializer`はステージシーン内で共有する参照を`IStageSceneInstance`として公開します。

## 🔌 拡張ポイント

> 現状、ポリモーフィックな拡張ポイント（`SubclassSelector`等）はありません。新しい移動・回避挙動を追加する場合は、対応するApplicationクラスの追加、または既存クラスへの分岐追加という形になります。

## 🔄処理フロー

主要な処理フローは、それぞれ子ページに分けています。

### ① 通常移動フロー（毎フレーム）
入力バッファから取り出した移動操作を、カメラ正面方向を基準に補正して反映します。

```mermaid
sequenceDiagram
    autonumber
    participant InputView as PlayerInputView
    participant Controller as PlayerController
    participant MoveApp as PlayerMovementApplication
    participant CamT as ICameraTransform
    participant View as PlayerView

    Note over InputView: 毎フレーム of Update ループ
    InputView ->> Controller: 入力バッファから移動操作を取得
    Controller ->> CamT: カメラ正面方向を取得
    CamT -->> Controller: 方向ベクトルを返却
    Controller ->> MoveApp: 移動計算要求（入力・カメラ方向）
    MoveApp -->> Controller: 移動後座標を返却
    Controller ->> View: Transform/Rigidbodyへ反映
```

### ② 攻撃実行フロー（入力イベント時）
攻撃入力を受け、ターゲット解決からダメージ適用・イベント通知までを実行します。

```mermaid
sequenceDiagram
    autonumber
    actor Player as プレイヤー
    participant PAC as PlayerAttackController
    participant TSC as TargetSystemController (Targetモジュール)
    participant MissionRec as MissionProgressRecorderController (Missionモジュール)

    Player ->> PAC: 攻撃ボタン押下
    PAC ->> TSC: 現在ターゲットの取得
    TSC -->> PAC: ターゲットEntity返却
    PAC ->> PAC: ダメージ計算・適用（Character&Battleモジュール）
    PAC -->> MissionRec: OnAttackExecuted イベント発火
```