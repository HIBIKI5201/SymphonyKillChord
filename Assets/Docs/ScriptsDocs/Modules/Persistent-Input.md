# Persistent-Input

入力取得と履歴化のモジュールです。Unity Input System のイベントを内部形式へ変換し、時系列の履歴として保持します。

## 構造概要

### 1. Domain
- **BufferedInput**: `ActionId`, `Phase`, `Timestamp`, `VectorValue`, `FloatValue` を持つ履歴 1 件。
- **InputBufferingQueue**: `RingBuffer<BufferedInput>` ベースの履歴保持。
- **InputActionId**: 内部で扱う入力 ID。

### 2. Application
- **InputBufferRecorder**: `BufferedInput` を Queue に記録。

### 3. Adaptor
- **InputActionKind**: View 側イベント種別。
- **InputContext<T>**: デバイス非依存の入力データ。
- **InputIdConverter**: `InputActionKind` を `InputActionId` に変換。
- **RecordController**: `InputContext<T>` を `BufferedInput` に変換して記録。
- **InputTimestampProvider**: `Time.unscaledTime` ベースの時刻供給。

### 4. View
- **PlayerInputView**: `PlayerInput` のイベント購読と公開。
- **UnityInputMapController**: `Common / InGame / OutGame` の ActionMap 制御。
- **InputMapNames**: ActionMap 名の定数。

### 6. Composition
- **InputComposition**: View、Recorder、Queue、Controller を接続。
- **InputDebugLogger**: デバッグログ出力。

## 現在の実装メモ

- `PlayerInputView` は PC 向け `CallbackContext` とモバイル向け直接通知の両方を扱います。
- `InputBufferingQueue` には `OnBuffered` イベントがあり、記録時に通知を飛ばせます。
