# InGame-Camera

カメラ制御モジュールです。追従、フリールック、ロックオン、遮蔽物による距離補正を `CameraSystemApplication` が統合します。

## 構造概要

### 1. Domain
- **CameraParameter / CameraSystemParameter**: カメラ関連の各種設定。
- **ILockOnTarget**: ロックオン対象の抽象。

### 2. Application
- **CameraSystemApplication**: カメラ全体の司令塔。
- **CameraFollowApplication**: 注視対象への追従オフセット更新。
- **CameraRotationApplication**: カメラ回転の補正。
- **CameraBoneFreeLookRotationApplication**: 通常時の入力回転。
- **CameraBoneLockOnRotationApplication**: ロックオン時の向き制御。
- **TargetSelector**: 対象選択ロジック。
- **CameraSystemContext**: 1 フレーム分の入力文脈。

### 3. Adaptor
- **CameraSystemController**: View からの更新要求を `CameraSystemApplication` へ橋渡し。
- **TargetManager / LockOnTarget**: 対象管理。

### 4. View
- **CameraSystemView**: 実際の Camera Transform 反映。

### 5. InfraStructure
- **CameraSystemConfig**: Inspector からパラメータを供給する ScriptableObject / 設定元。

### 6. Composition
- **CameraSystemInitializer**: 各 Application を組み立てて View と接続。
- **CameraSystemParameterDebug**: Editor 用のパラメータ調整。

## 現在の実装メモ

- `CameraSystemApplication` は `CameraLockOnState` を内部に持ち、入力が入るとオートロックオンを解除します。
- 距離補正には `Physics.SphereCast` を使い、遮蔽物があるとカメラ距離を短縮します。
