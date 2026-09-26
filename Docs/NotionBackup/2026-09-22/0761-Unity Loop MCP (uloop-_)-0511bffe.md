# Unity Loop MCP (uloop-*)

- id: 3d67c2c6-cc02-81af-adb7-de390511bffe
- path: Symphony Kill Chord / システム概要 / Unity Loop MCP (uloop-*)
- last_edited: 2026-09-09T04:38:31.098Z

- カテゴリー: 開発用


## Unity Loop MCP (uloop-*)

### プロパティ
| プロパティ | 値 |
|---|---|
| カテゴリー | 開発用 |
| 場所 |  |
| 日付 |  |
| 概要 | AIエージェントからUnity Editorを外部操作するための`uloop`コマンド群について |
---

## 説明
`npx uloop-cli`経由でUnity Editorを外部から操作する仕組みである。
Claude Code等のAIコーディングエージェントがCLI経由でUnity Editorへコマンドを発行し、コンパイル、シーン検査、動的コード実行、ログ取得、スクリーンショット撮影などを行える。
本プロジェクトでは`.claude/skills/uloop-*/`[配下にツールごとのSKILL.md](http://配下にツールごとのSKILL.md)としてラップされており、エージェントは用途に応じて該当するSKILLを呼び出す。

## 詳細

### 仕組み

#### 前提
Unity Editorが起動しており、CLIから接続できる状態であることが前提となる。
Editorが未起動、またはクリーンな再起動が必要な場合は`uloop launch`を使用する。
- `-r, --restart`でUnityを再起動できる
- `-p, --platform`でビルドターゲットを指定できる
- Unityのバージョン検出、プロジェクトパスの探索、Unity Hubへの登録も行う

#### コマンド共通仕様
各コマンドは`--project-path`オプションでプロジェクトパスを明示的に指定できる（省略時はカレントディレクトリを対象とする）。
V3のブール系オプションは値を取らないフラグ形式であり、存在すれば有効、存在しなければ無効となる。

### ツール一覧

#### uloop launch
Unity Editorを起動する。既に起動している場合は既存ウィンドウにフォーカスし、起動が必要な場合はCLI操作が可能になるまで待機してから終了する。

#### uloop compile
プロジェクトのコンパイルを実行し、エラー・警告件数を返す。
`--force-recompile`で強制的にフルリコンパイル（ドメインリロードを伴う）を行える。
`compiling.lock`等のロックファイルが残ってCLIが固まる場合は`uloop fix`でロックファイルを除去してから再試行する。

#### uloop execute-dynamic-code
既存のuloopツールでは調査・編集しきれない場合に、Unity APIを含むC#コードを動的に実行する。
シーン操作、プレハブ操作、SerializedObject経由の編集、AssetDatabaseのリフレッシュや.meta生成、メニュー実行、PlayMode自動化などに使用する。
`--code`（単行）または`--code-file`（複数行、PowerShellでの行落ち対策として推奨）でC#の直接記述（クラス/名前空間/メソッドのラップ不要）を渡し、コンパイル結果と実行結果、ログをJSONで受け取る。
`System.IO.*`やアセット作成系API（`AssetDatabase.CreateFolder`等）、`.cs`/`.asmdef`ファイルの作成・編集はコンパイル時に拒否される。
[header_4] 運用上の注意
動的コード実行によるUnity API呼び出しは、対象APIによってはUnity Editor自体をフリーズさせるリスクがある。
特にNavMesh関連API（`AddNavMeshData`、`CalculateTriangulation`等）を経由した動的コード実行は、過去にUnityエディタのフリーズを引き起こしたことがある。
このようなリスクのあるAPIを動的コードから呼び出す場合は、影響範囲を切り分けたうえで慎重に実行し、事前にユーザーへ確認を取ることが望ましい。

#### uloop find-game-objects
名前パターン・タグ・レイヤー・コンポーネントなどの条件でGameObjectを検索する。
`--search-mode Selected`を指定すると、Unity Editorで現在選択中のGameObjectの詳細（コンポーネントとそのプロパティ）を取得できる。
複数選択時は結果がインラインではなくファイル出力（`.uloop/outputs/FindGameObjectsResults/`）になる。

#### uloop get-hierarchy
シーンの階層構造をツリー形式で取得する。
`--root-path`でルートを指定、または`--use-selection`で現在選択中のGameObjectをルートとして扱える。
取得結果はレスポンスに含まれず、指定されたJSONファイルへ書き出される。

#### uloop get-logs
Unity Console上のログ（Log/Warning/Error）を取得する。
`--search-text`によるテキスト検索（正規表現も可）や、スタックトレースの取得・検索にも対応する。
コンパイル、テスト、PlayMode実行、動的コード実行の後に結果を確認する用途で使う。

#### uloop clear-console
Unity Consoleのログをクリアする。
コンパイルやテスト、デバッグ作業の前に古いログが結果を紛らわせないよう、事前にクリアする用途で使う。

#### uloop screenshot
Unity EditorのウィンドウやGame Viewの描画をPNGとして撮影する。
`--capture-mode rendering`ではPlayMode中のゲーム描画のみをキャプチャし、座標系がマウス入力シミュレーション（simulate-mouse系ツール）と一致する。
`--annotate-elements`（UI要素への注釈）や`--annotate-raycast-grid`（3Dレイキャスト候補点への注釈）を併用することで、クリック可能な座標をAIエージェントが特定しやすくなる。
ビジュアル確認、デバッグ、ドキュメント作成、UI要素の座標特定に使用する。

#### uloop record-input（V3で削除）
`uloop record-input`はV3で削除されたコマンドである。
CLIからPlayMode入力を記録する手段はなくなっており、記録操作はUnity Editorの **Window > Unity CLI Loop > Recordings** からGUI経由で行う必要がある。
CLIエージェント経由でこの操作を自動化したい場合は、`uloop-execute-dynamic-code`による代替実装、または人手によるGUI操作を検討する。
決定論的リプレイに関する設計上の注意点自体は、記録方法の変更後も変わらず有効である。