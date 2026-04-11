# Persistent 機能構造

`Persistent` は常駐シーンで使う共有機能を扱います。現在の対象は入力履歴、BGM 再生、シーン遷移です。

## モジュール詳細

- **[Persistent-Input](./Modules/Persistent-Input.md)**: Input System からの入力取得と履歴化。
- **[Persistent-Music](./Modules/Persistent-Music.md)**: 常駐 BGM 再生。
- **[Persistent-SceneManagement](./Modules/Persistent-SceneManagement.md)**: シーンの追加ロード・切り替え・アンロード。

## レイヤー構造

### 1. Domain
- **Input**: `BufferedInput`, `InputActionId`, `InputBufferingQueue`。
- **Music**: `Beat`。

### 2. Application
- **Input**: `InputBufferRecorder`。
- **SceneManagement**: `ISceneTransitionService`。

### 3. Adaptor
- **Input**: `InputActionKind`, `InputContext<T>`, `InputIdConverter`, `InputTimestampProvider`, `RecordController`。
- **SceneManagement**: `SceneTransitionController`。

### 4. View
- **Input**: `PlayerInputView`, `UnityInputMapController`, `InputMapNames`。
- **Music**: `MusicPlayer`。
- **SceneManagement**: `SceneTransitionView`。

### 5. InfraStructure
- **SceneManagement**: `SceneTransitionService`。

### 6. Composition
- **EntryPoint**: `PersistentEntryPoint`。
- **Initializer**: `InputComposition`, `MusicPlayerInitializer`, `SceneTransitionInitializer`。

## 現在の実装メモ

- `PersistentEntryPoint` は初回シーンを加算ロードして Active Scene を切り替えるエントリーポイントです。
- `Persistent-Music` 側には現在、`MusicSyncService` のようなビート同期処理は含まれていません。ビート同期は `InGame` 側です。
