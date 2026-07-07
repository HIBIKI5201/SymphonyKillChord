
---

# 📌 基本情報
インゲーム中のカメラシステム（プレイヤーの追従、フリーカメラ操作、および敵のロックオン・ターゲット選択システム）を司るモジュールです。
| 項目 | 内容 |
| --- | --- |
| **モジュール名** | Camera |
| **カテゴリ** | InGame / System |
| **アーキテクチャ** | クリーンアーキテクチャ (Domain, Application, Adaptor, View, Composition) |

---

## 🏗️ クラス（レイヤー構成）

| クラス名 | レイヤー | 役割・機能 |
| --- | --- | --- |
| **`ICameraTransform`** | Composition (Persistent) | カメラの座標/向きを提供する抽象インターフェース（常駐シーンの CameraInitializer.cs 内に定義） |
| **`CameraSystemParameter`** | Domain | カメラ動作の設定用SOなどに対となるピュアデータ |
| **`ILockOnTarget`** | Domain | ロックオン対象が実装すべきインターフェース |
| **`CameraSystemApplication`** | Application | カメラシステム全体を束ねるロジック |
| **`CameraFollowApplication`** | Application | プレイヤーへの追従制御 |
| **`CameraFollowVelocityApplication`** | Application | 速度に基づく動的追従ズーム等（※実際は struct） |
| **`CameraRotationApplication`** | Application | カメラの向き回転 |
| **`CameraBoneFreeLookRotationApplication`** | Application | フリー視点時のボーン回転制御 |
| **`CameraBoneLockOnRotationApplication`** | Application | ロックオン時のボーン追従回転制御 |
| **`TargetSelector`** | Application | ロックオン候補となるターゲットの検索・判定と選択管理を行うロジッククラス |
| **`CameraSystemController`** | Adaptor | 入力やステートに応じた操作伝達をアプリケーション層へ仲介する |
| **`CameraSystemPresenter`** | Adaptor | ビュー側への表示パラメータ伝達 |
| **`TargetSelectorController`** | Adaptor | 他モジュール（バトル・スキル等）が現在のターゲット情報を取得するためのアダプター窓口 |
| **`LockOnTargetGateway`** | Adaptor | ターゲット選択の実装仲介 |
| **`CameraSystemView`** | View | 実際のUnityシネマシーンやカメラコンポーネントおよびプレイヤー入力の受け口 |
| **`CameraSystemInitializer`** | Composition | インプットとカメラコンポーネントのDI（依存注入） |

---

## 🔗 モジュール間依存関係

カメラモジュールは、他のモジュール（Persistent, Player, Enemy）と疎結合で連携します（外部と接続のない内部レイヤーは省略）。

```mermaid
graph TD
    %% 定義 (接続のないレイヤーは省略)
    subgraph CameraModule [Camera モジュール]
        Domain[Domain<br>ICameraTransform<br>ILockOnTarget]
        Adaptor[Adaptor<br>TargetSelectorController]
    end
    
    subgraph PersistentModule [Persistent モジュール]
        PlayerInputView[PlayerInputView]
    end
    
    subgraph PlayerModule [Player モジュール]
        PlayerInitializer[PlayerInitializer]
    end
    
    subgraph EnemyModule [Enemy モジュール]
        EnemyLifeCycle[EnemyLifeCycle]
    end

    %% 依存関係
    PlayerInputView -->|インプット提供| Adaptor
    PlayerInitializer -->|追従対象Transform| Adaptor
    PlayerInitializer -->|カメラ方向の取得| Domain
    EnemyLifeCycle -->|ILockOnTargetの実装| Domain
    Adaptor -->|ロックオン対象取得| Domain
```

### 📥 依存しているもの（外部 → Camera）

* **`Persistent`**
  * *依存箇所*: `PlayerInputView`
  * *詳細*: カメラの視点移動操作（右スティック/マウス）のために、常駐シーンのインプットビューからインプット情報を受け取ります。
* **`InGame/Player`**
  * *依存箇所*: `PlayerInitializer`
  * *詳細*: 追従対象としてプレイヤーの `transform` を必要とします。
* **`InGame/Enemy`**
  * *依存箇所*: `ITarget` / `ILockOnTarget`
  * *詳細*: ロックオン対象となる敵を取得します。Domain層に用意した抽象インターフェース `ILockOnTarget` を通じて弱結合で参照しています。

### 📤 依存されているもの（Camera → 外部）

* **`InGame/Player`**
  * *参照箇所*: `ICameraTransform`, `TargetSelectorController`
  * *詳細*: プレイヤーはカメラの正面方向を基準に移動方向を決定するため、カメラの Transform 情報に依存します。また、スキル発動時の自動ターゲット選択のために `TargetSelectorController` を参照します。
* **`InGame/Enemy`**
  * *参照箇所*: `ILockOnTarget`
  * *詳細*: 敵キャラクターはカメラからロックオンされ得る対象として `ILockOnTarget` インターフェースを実装します。

---

# 🔄 処理の流れ（シークエンス図）

主要な処理フローごとに分けて記述します。

### ① カメラの通常追従フロー（毎フレーム）
プレイヤーの移動に追従し、プレイヤーの移動速度に基づいて動的にズーム距離を計算・反映します。

```mermaid
sequenceDiagram
    autonumber
    participant CSApp as CameraSystemApplication
    participant CFA as CameraFollowApplication
    participant CFVA as CameraFollowVelocityApplication
    participant CSPres as CameraSystemPresenter
    participant CSView as CameraSystemView

    Note over CSApp: 毎フレームのUpdate / LateUpdateループ
    CSApp ->> CFA: 追従座標計算依頼 (プレイヤー位置を渡す)
    CFA -->> CSApp: 基本追従座標を返却
    CSApp ->> CFVA: ズーム・ラグ計算依頼 (プレイヤー速度を渡す)
    CFVA -->> CSApp: 動的ズーム反映後の座標を返却
    CSApp ->> CSPres: 最新のカメラ座標データを伝達
    CSPres ->> CSView: シネマシーンの仮想カメラ等に反映
```

### ② ターゲットロックオン & 切り替えフロー（入力イベント時）
プレイヤーがロックオンボタンを押すか、スティック操作等によってロックオン対象を切り替える際の処理フローです。

```mermaid
sequenceDiagram
    autonumber
    actor Player as プレイヤー入力
    participant CSView as CameraSystemView
    participant CSCont as CameraSystemController
    participant CSApp as CameraSystemApplication
    participant TS as TargetSelector
    participant LOTG as LockOnTargetGateway

    Player ->> CSView: ロックオンボタン押下 / ターゲット切り替え入力
    CSView ->> CSCont: ロックオン状態切り替え要求 (ToggleLockOnState)
    CSCont ->> CSApp: ロックオン状態トグル要求 (ToggleLockOnState)
    CSApp ->> TS: ターゲット切り替え要求 (ChangeTarget)
    TS ->> LOTG: ロックオン候補（ILockOnTarget）の一覧を取得
    LOTG -->> TS: リスト返却
    TS ->> TS: 画面中央に最も近いターゲットを選定・更新
    CSApp ->> CSApp: カメラ状態を「ロックオンモード（LockOnManual）」へ遷移
```

### ③ ロックオン中の注視回転 & UI表示フロー（毎フレーム）
ロックオンモード中にターゲットを自動的に画面中央付近に捉え続け、ターゲットUIを描画するための情報を他モジュールへ連携する処理フローです。

```mermaid
sequenceDiagram
    autonumber
    participant CSApp as CameraSystemApplication
    participant CBLR as CameraBoneLockOnRotationApplication
    participant CSPres as CameraSystemPresenter
    participant CSView as CameraSystemView
    participant TSC as TargetSelectorController
    participant OtherUI as ロックオンUI (他モジュール)

    Note over CSApp: 毎フレームの Update / LateUpdateループ (ロックオン時)
    CSApp ->> CBLR: ロックオン回転角計算要求 (ターゲット座標・自機座標)
    CBLR -->> CSApp: 計算された回転値（Yaw/Pitch）を返却
    CSApp ->> CSPres: 最新のカメラ位置・回転データを伝達
    CSPres ->> CSView: カメラ用ボーン/Transformに回転を反映

    Note over OtherUI: 毎フレームの描画更新ループ
    OtherUI ->> TSC: 現在のロックオン対象情報（座標等）を取得
    TSC -->> OtherUI: ターゲット情報返却
    OtherUI ->> OtherUI: 画面上にロックオンマークをレンダリング
```