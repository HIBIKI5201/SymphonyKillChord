# AutoBuilder

- id: 3d67c2c6-cc02-8130-9cb7-ec47e253a290
- path: Symphony Kill Chord / システム概要 / AutoBuilder
- last_edited: 2026-09-09T04:38:43.080Z

- カテゴリー: 開発用


## AutoBuilder

### プロパティ
| プロパティ | 値 |
|---|---|
| カテゴリー | 開発用 |
| 場所 | `Assets/Editor/Scripts/AutoBuilder/` |
| 日付 |  |
| 概要 | 複数のBuildProfileを順番にビルドするエディタ拡張。ローカルビルドとGitHub ActionsのCIビルドの両方から使用される |
---

## 説明
複数のUnity `BuildProfile`を順番にビルドし、それぞれを専用のサブフォルダへ出力するエディタ拡張である。
ローカルでの手動ビルド用のEditorWindowと、CI（GitHub Actions）のバッチモードから直接呼び出されるエントリポイントの両方を持ち、両者はビルド処理の中核ロジックを共有している。

## 詳細

### クラス一覧
| クラス | 役割 |
|---|---|
| `AutoBuildWindow` | メニューから開くEditorWindow。「Master Build」「Develop Build」ボタンによるローカルでの一括ビルド操作を提供する |
| `AutoBuildExecuter` | ドメインリロードに耐えるビルド実行本体。`SessionState`にセッションを保存し、リロード後も続きから再開する |
| `AutoBuilder` | CI（バッチモード）用のエントリポイント。コマンドライン引数を解釈し`AutoBuildExecuter`へ処理を委譲する |
| `AutoBuilderSettings` | `ProjectSettings/AutoBuilderSettings.asset`に保存される設定（出力先パス・対象BuildProfile配列）を保持する`ScriptableSingleton` |
| `AutoBuilderProvider` | Project Settingsウィンドウ上でAutoBuilderSettingsを編集するための`SettingsProvider` |

### 仕組み

#### 設定（AutoBuilderSettings）
`MasterPath` / `MasterBuildProfiles`（Masterビルド用）と、`DevelopPath` / `DevelopBuildProfiles`（Developビルド用）の2系統を持つ。
パスが空、末尾がスラッシュで終わっていない、BuildProfileが空・null混入・重複している場合はいずれも`AutoBuilderProvider`のGUI上で警告が表示される。

#### ローカル実行（AutoBuildWindow → AutoBuildExecuter）
1. `AutoBuildWindow`の「Master Build」または「Develop Build」ボタンを押すと`AutoBuildExecuter.Run(path, profiles)`が呼ばれる。
2. 対象の`BuildProfile`配列を順番にビルドする。出力先は`path/<ProfileName>/<ProductName><拡張子>`。
3. セッション状態（現在の処理位置、出力先、失敗有無など）は`SessionState`へJSONとして保存される。ビルドプロファイル切り替え等でドメインリロードが発生しても、`InitializeOnLoadMethod`経由で自動的に処理を再開する。
4. 同一プロファイルへの再試行回数には上限（`MAX_PROFILE_ATTEMPT_COUNT`=10）があり、超過した場合はそのプロファイルを失敗としてスキップし次へ進む。
5. ビルド中に発生したエラー/例外/アサートのログは、ドメインリロードでConsoleが消える前にセッションとは別のキーへ記録され、リロード後に再出力される。
6. 各プロファイルのビルドが成功すると、出力ディレクトリはZIP圧縮されて元データは削除される（ディスク使用量を1プロファイル分に抑えるため）。失敗した場合は出力ディレクトリごと削除され、リリース対象に含まれない。
7. 全プロファイル終了後、開始前に設定されていたBuildProfileへ復元し、手動実行時はダイアログで成否を通知する。

#### CI実行（AutoBuilder.RunFromCli）
CIはUnityを1プロセスで起動して完走させ、そのまま終了するため、`AutoBuildExecuter`のようなドメインリロード耐性の仕組みは不要であり、`RunFromCli`はセッション状態を永続化しない。
- コマンドライン引数`-buildMode`（`Development`/`Master`、省略時は両方）と`-selectedProfiles`（カンマ区切りのプロファイル名でさらに絞り込み）を解釈する。
- 出力先は環境変数`UNITY_BUILD_OUTPUT_DIR`（CIが`Builds/<BuildMode>`に設定）を優先し、未設定時は`<プロジェクト>/../Builds`を使用する。
- 最終的に同じ`AutoBuildExecuter.Run`を`isBatchMode: true`で呼び出し、ビルド本体のロジックはローカル実行と共有する。
- 全プロファイル終了後、`EditorApplication.Exit(exitCode)`でUnityプロセスを終了する（0: 全成功、1: いずれか失敗または例外）。

### `.github/workflows/BuildAndRelease.yml`との対応
CIは自前のビルドパイプラインを持たず、AutoBuilderのCLIエントリポイント（`AutoBuilder.RunFromCli`）をそのままバッチモードで呼び出しており、ローカルビルドと処理の実体は同一である。
- トリガーは`develop`/`main`へのPRマージ、または`workflow_dispatch`による手動実行のみ。
- ベースブランチから`BuildMode`を決定する（`develop`→`Development`/タグ接頭辞`dev`、`main`→`Master`/タグ接頭辞`release`）。
- `workflow_dispatch`時のみ、Android/Windows/MacOS/iOSの各チェックボックス入力を`-selectedProfiles`引数に変換してプロファイルを絞り込める（PRマージ起動時は絞り込みなしの全件ビルド）。
- ビルド出力先は`AutoBuilderSettings.asset`をテキストとして直接パースして取得し、`UNITY_BUILD_OUTPUT_DIR`環境変数としてUnityプロセスへ渡す。
- Google Driveから`.unitypackage`依存関係をダウンロードして展開・配置し、`NuGetForUnity.Cli`でNuGetパッケージを復元したうえで、下記コマンドでUnityをバッチモード起動する。
```
Unity.exe -batchmode -projectPath <repo> -executeMethod KillChord.Editor.AutoBuilder.AutoBuilder.RunFromCli -buildMode <Development|Master> [-selectedProfiles <names>] -logFile <logfile>
```
- Unity側は`AutoBuildExecuter`がプロファイルごとに都度ZIP化・元データ削除まで済ませているため、CI側では再圧縮はせず、タグ名を付与してファイル名を変更・配置するのみ。
- 最後に`softprops/action-gh-release`でタグ付きのGitHub Releaseを作成し、各プロファイルのZIPを添付する。Developmentはprerelease扱いとなる。
- ローカルで`RunFromCli`の再現に成功したのにCIで失敗する場合は、ビルドロジック自体よりも環境差異（Google Driveから毎回取得し直す`.unitypackage`依存関係、NuGet復元状態、`UNITY_EXE_PATH`等のシークレット・変数）を疑うべきである。

### 処理フロー
```Mermaid
flowchart TD
    subgraph Local["ローカル実行"]
        A[AutoBuildWindow: Master/Developボタン] --> B[AutoBuildExecuter.Run]
    end
    subgraph CI["CI実行 (BuildAndRelease.yml)"]
        C[Unity.exe -batchmode -executeMethod AutoBuilder.RunFromCli] --> D[AutoBuilder.PerformMultipleBuilds]
        D --> B
    end
    B --> E[BuildSessionをSessionStateへ保存]
    E --> F{対象プロファイルは残っているか}
    F -- あり --> G[BuildProfileを切り替えてBuildPipeline.BuildPlayerを実行]
    G --> H{ビルド成功?}
    H -- 成功 --> I[出力フォルダをZIP化し元データ削除]
    H -- 失敗 --> J[出力フォルダを削除]
    I --> F
    J --> F
    F -- なし --> K[開始前のBuildProfileへ復元]
    K --> L{バッチモード?}
    L -- はい --> M[EditorApplication.Exitで終了コードを返す]
    L -- いいえ --> N[ダイアログで成否を通知]
    M --> O[CI: ZIPにタグ付与しGitHub Releaseへ添付]
```