# Persistent-SceneManagement

シーン遷移モジュールです。現在はフェード演出を挟まず、サービス経由で対象シーンをロードして Active Scene を切り替え、必要なら遷移元シーンをアンロードします。

## 構造概要

### 2. Application
- **ISceneTransitionService**: `ChangeSceneAsync(fromSceneName, toSceneName, cancellationToken)` を提供。

### 3. Adaptor
- **SceneTransitionController**: View からの遷移要求を受ける。

### 4. View
- **SceneTransitionView**: デバッグ用のシーン遷移ボタン。

### 5. InfraStructure
- **SceneTransitionService**: `SceneLoader` を使ってシーンのロード / アンロード / Active 化を行う実装。

### 6. Composition
- **SceneTransitionInitializer**: `SceneTransitionView` に controller を注入。

## 現在の実装メモ

- 以前の説明にあった「フェードアウト完了後にロード」のような演出フローは、現行コードには入っていません。
- `SceneTransitionView` はデバッグ用途の MonoBehaviour で、ボタンから直接 `ChangeScene()` を呼ぶ想定です。
