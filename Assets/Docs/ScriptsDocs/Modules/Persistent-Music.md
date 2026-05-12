# Persistent-Music

常駐シーンで BGM を再生し、他シーンから参照できるようにするモジュールです。

## 構造概要

### 4. View
- **MusicPlayer**: `CriAtomSource` を使って BGM の再生と停止を行う。
- **MusicViewModel**: 再生する cue 名を保持し、`MusicPlayer` が購読する。

### 6. Composition
- **MusicPlayerInitializer**: `MusicPlayer` と `MusicViewModel` を結び付けて `ServiceLocator` に登録する。

## 現在の実装メモ

- `MusicPlayer.Time` で現在の再生時間を秒単位で取得できます。
- `Persistent-Music` は「再生」の責務に限定されており、ビート同期ロジックは `InGame-Music` にあります。
