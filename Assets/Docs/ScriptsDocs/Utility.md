# Utility 機能構造

`Utility` はプロジェクト全体から参照される汎用コードをまとめた層です。

## モジュール詳細

- **[Utility-Collections](./Modules/Utility-Collections.md)**: `PriorityQueue`、`RingBuffer`。
- **[Utility-Constant](./Modules/Utility-Constant.md)**: `PathConst`、`ExecutionOrderConst`、共通列挙型。

## レイヤー構造

### 0. Utility
- **Collections**: `PriorityQueue<TElement, TPriority>`, `RingBuffer<T>`。
- **Constant**: `PathConst`, `ExecutionOrderConst`。
- **InGame 用列挙型**: `UpdateModeEnum`, `CameraLockOnState`。

## 現在の実装メモ

- `PriorityQueue` は .NET の優先度付きキュー実装を持ち込んだ形です。
- `RingBuffer<T>` は `where T : unmanaged` 制約付きで、`BufferedInput` のような値型履歴を GC を抑えて扱う用途に使われています。
