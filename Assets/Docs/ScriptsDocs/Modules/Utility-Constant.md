# Utility-Constant

定数と共通列挙型のモジュールです。

## 構造概要

### 0. Utility
- **PathConst**: `CREATE_ASSET_MENU_PATH = "KillChord/Runtime/"` を定義。
- **ExecutionOrderConst**: `INITIALIZATION`, `MOVEMENT`, `CAMERA_FOLLOW`, `HUD` を定義。
- **UpdateModeEnum**: `Stop`, `Update`, `FixedUpdate`, `LateUpdate`。
- **CameraLockOnState**: `Free`, `LockOnAuto`, `LockOnManual`。

## 各クラスの役割

### PathConst
- `CreateAssetMenu` の `menuName` などで使う共通パスをまとめます。

### ExecutionOrderConst
- `DefaultExecutionOrder` の値を一元化します。

### UpdateModeEnum
- カメラや更新処理でどの Unity 更新フェーズを使うかを表します。

### CameraLockOnState
- カメラが自由状態か、自動ロックオンか、手動ロックオンかを表します。
