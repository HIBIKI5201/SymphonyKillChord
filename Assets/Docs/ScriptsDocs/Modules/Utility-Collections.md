# Utility-Collections

汎用データ構造のモジュールです。

## 構造概要

### 0. Utility
- **PriorityQueue<TElement, TPriority>**: 4 分木ヒープ実装の優先度付きキュー。
- **RingBuffer<T>**: 固定長の循環バッファ。`T : unmanaged` 制約あり。

## 各クラスの役割

### PriorityQueue<TElement, TPriority>
- `Enqueue`, `Dequeue`, `TryPeek`, `TryDequeue`, `Remove` などを持ちます。
- `MusicSyncService` の予約アクション管理に使われています。

### RingBuffer<T>
- 容量超過時は古い要素を上書きします。
- `PeekFirst`, `PeekLast`, `AsReadonlySpan`, `Clear` を提供します。
- `InputBufferingQueue` の内部ストレージとして使われています。
