# CLIスクリプト管理ツール（Win32 / C++）と仕様書ビューア（EditorWindow）の検討

- 作成日: 2026-11-12
- 依頼: 「初期化やAndroidビルドのインポートなどのCLI向け bat / command を Win32 UI で管理できるようにして Admin にしたい。C++ メインで実装。仕様書の写しを Unity のウィンドウで表示するエディタも欲しい。それぞれ検討して」
- 範囲: 設計案のみ。コードは変更していない（CG-002）。実装に進むときは feature-flow で行う。
- 調べたもの: 本体リポジトリ develop `d32024d5` のルート・`BuildRunner/`・`HomePage/`・`PowerShell/`・`scripts/`・`Assets/Editor/`、仕様書リポジトリ main（最終コミット 2026-11-12T14:34:50Z）。
- 調べられなかったもの: ユーザーPC上のリポジトリ外のスクリプト（PCがつながっていなかった）。

---

## 1. CLIスクリプト管理ツール

### 1.1 現在のスクリプト一覧

リポジトリにある、ダブルクリックで動かす入口（`.bat` / `.command`）と、その本体。

| 入口（Windows / mac） | 本体 | 何をするか | 対話 | 管理者権限 |
| --- | --- | --- | --- | --- |
| `Setup.bat` / `Setup.command` | `PowerShell/ProjectSetup/Setup-Project.ps1`（`.sh`） | クローン後の初期化。サブモジュール取得、git設定、Unity・Android Build Support の確認、gh・Unity CLI の導入提案。`-Check` で確認だけ | あり（y/N） | **不要**。昇格すると害がある（後述） |
| `Clean-Branches.bat` / `.command` | `scripts/git/clean-local-branches.sh`（Git for Windows の bash） | develop取込済みのローカルブランチ削除。`--dry-run` あり | あり（y/N） | 不要 |
| `BuildRunner/runner-menu.bat` / `.command` | `runner-menu.ps1` | セルフホストランナーの管理メニュー（status / start / cpu / runs / watch / log / setup） | あり（番号入力）。引数で直接実行も可 | 不要 |
| `BuildRunner/start-runner.bat` / `.command` | `start-runner.ps1` → `run.cmd` | 起動前チェック後、そのウィンドウ自体がランナーになる | あり。**閉じてはいけない常駐ウィンドウ** | **昇格させてはいけない**（ジョブが管理者で動く） |
| `HomePage/dev.bat` / `.command` | `dev.ps1` | ホームページのローカルプレビュー（npm ci → astro dev） | なし（常駐サーバー） | 不要 |
| （入口なし）`BuildRunner/SetBuildCpuMode.ps1` | 同左 | 実行中ビルドの CPU 割合切替。runner-menu の `cpu` から呼ばれる | なし | 不要（同じユーザーのプロセスのみ操作。実機未確認） |
| （入口なし）`PowerShell/StartGemini.ps1` | 同左 | ルートで gemini CLI を起動 | あり | 不要 |
| （入口なし）`SinfoniaOperator/run-tool.sh` | 各 `.exe` | Notion エクスポート等の CLI を起動 | ツールによる | 不要 |

**「Androidビルドのインポート」に当たるスクリプトはリポジトリに見つからなかった。** Android ビルド関連は `Setup-Project.ps1`（Android Build Support の確認）、`BuildRunner`（Android ビルドのランナー）、Unity 内の `AutoBuilder` だけ。PC にだけ置いてあるスクリプトか、これから作るものなら、場所を教えてほしい（質問 Q1）。

### 1.2 管理者権限についての評価

**今あるスクリプトに、管理者権限が必要なものは無い。** 逆に、ツールごと管理者で起動する（UAC manifest `requireAdministrator`）と、子プロセスがすべて管理者になり次の害が出る。

1. **git の「dubious ownership」エラー**: 管理者で作ったファイル・フォルダ（サブモジュールのチェックアウト、`UserSettings/`、`HomePage/node_modules` など）の所有者が Administrators グループになり、あとで通常権限の git・Unity から `safe.directory` エラーや書き込み失敗が起きる。
2. **ランナーが管理者で動く**: `start-runner.bat` を昇格させると、GitHub Actions のジョブ（ワークフローのコード）が管理者権限で PC 上を操作できてしまう。セキュリティ上やってはいけない。
3. **ネットから取ったスクリプトの実行**: `Setup-Project.ps1` は Unity CLI を `Invoke-RestMethod … | Invoke-Expression` で入れる。管理者だと被害範囲が PC 全体になる。
4. **使い勝手**: 昇格したウィンドウには、エクスプローラーからのドラッグ＆ドロップができない（UIPI）。ネットワークドライブの割り当ても見えない。起動のたびに UAC の確認が出る。
5. **winget のインストール先が変わる**: ユーザー単位ではなくマシン全体に入るものがある。

そこで **⭐推奨は「ツール本体は通常権限（`asInvoker`）で起動し、管理者が要るスクリプトだけ、そのスクリプトを実行するときに昇格する」方式**。設定ファイルで `"elevate": true` と書いたものだけ `ShellExecuteEx` の `runas` で起動し、UAC を出す。ツールの一覧には盾アイコンを付ける。将来、管理者が要るスクリプト（ドライバ・ファイアウォール・`Program Files` への配置など）が増えても、これで対応できる（質問 Q2）。

### 1.3 構成案

| 項目 | 案 |
| --- | --- |
| 置き場 | ルート直下に `ScriptLauncher/`（`BuildRunner/` と同じ並び。Unity の `Assets/` 外なので取り込まれない） |
| 言語・UI | C++20、素の Win32 API ＋ Common Controls v6（ListView・Edit・Button）。外部 UI ライブラリは使わない。Unicode（`wWinMain`）、Per-Monitor DPI v2 を manifest で宣言 |
| ビルド | CMake（3.25 以上）＋ MSVC。Visual Studio 2022 の「フォルダーを開く」でそのまま開ける。CRT は静的リンク（`/MT`）にして VC++ 再頒布パッケージを不要にする |
| 配布 | ⭐ GitHub Actions（`windows-latest`）でビルドし、成果物を Release に添付する。exe をリポジトリにコミットする方式（SinfoniaOperator の前例あり）も可（質問 Q4） |
| 依存 | JSON 読み込みに nlohmann/json（MIT、ヘッダー1枚）を同梱。`third-party-notices.md` に追記する |
| mac 対応の余地 | 「設定の読込・プロセス起動・出力の受け取り」を UI から分けた `core/` に置き、mac 版は UI（Cocoa など）だけ差し替える |

```
ScriptLauncher/
├── CMakeLists.txt
├── README.md
├── launcher-scripts.json        … スクリプトの登録（正本）
├── src/
│   ├── core/                    … 設定読込・プロセス起動・出力のデコード（OS 依存は薄い層に分ける）
│   └── win32/                   … ウィンドウ・リソース・manifest
└── tests/                       … core の単体テスト（CTest）
```

### 1.4 スクリプトの登録方法

⭐ **設定ファイル（`launcher-scripts.json`）を正本にし、未登録のものを CI で警告する**（質問 Q3）。

```json
{
  "version": 1,
  "scripts": [
    {
      "id": "setup",
      "name": "プロジェクトの初期化",
      "category": "初期化",
      "windows": "Setup.bat",
      "mac": "Setup.command",
      "presets": [
        { "label": "確認だけ", "args": ["-Check"] }
      ],
      "mode": "console",
      "elevate": false,
      "description": "サブモジュール・git設定・Unityを確認し、直せるものは直す"
    }
  ]
}
```

- `mode`: `console`（新しいコンソールで開く。対話・常駐用）か `capture`（ツール内に出力を表示。対話の無いもの用）。
- 自動検出（ルートを走査して `.bat` を全部並べる）だけにすると、`run.cmd`（ランナー本体）のような直接起動してはいけないものまで並ぶため、表示名・分類・引数・権限を持てる設定ファイルのほうが安全。
- 登録漏れは、`scripts/component-menu/check_component_menu.py` と同じ形の Python チェックを CI に足して、「`.bat` / `.command` があるのに `launcher-scripts.json` に無い」ときに警告する。ツール側でも、未登録の入口を灰色で「未登録」と並べてよい。

### 1.5 画面と出力表示

- 左: 分類ごとのスクリプト一覧（ListView）。右: 説明・引数プリセット・「実行」ボタン・出力欄。
- **対話のあるスクリプト（今ある入口のほぼ全部）は、新しいコンソールウィンドウで開く**（`CreateProcessW` ＋ `CREATE_NEW_CONSOLE`）。`Read-Host`・`read -p` の入力、`pause` がそのまま動き、既存スクリプトを変えなくてよい。ツールは終了コードだけ受け取って一覧に表示する。
- **対話の無いもの（`-Check`・`--dry-run`・`runner-menu.bat status` など）は、出力をパイプで受けてツール内に表示**する。PowerShell 側は `[Console]::OutputEncoding` を UTF-8 にしているので UTF-8 として読み、ANSI の色指定は取り除く。
- ツールを閉じても子プロセスは道連れにしない（ジョブオブジェクトの kill-on-close を付けない）。`start-runner` と `dev` は常駐なので、ツールを閉じて止まると困る。
- 疑似コンソール（ConPTY）でコンソールをツール内に埋め込む案もあるが、実装量が大きいので初版では見送る。

### 1.6 リスク・注意点

| リスク | 対策 |
| --- | --- |
| ツールごと管理者で起動したときの所有者・ランナー問題（1.2） | 既定は `asInvoker`。昇格はスクリプト単位 |
| ツールの置き場がリポジトリ内なので、ブランチを切り替えると設定ファイルが変わる | 実行のたびに設定を読み直す。設定が読めないときは一覧を空にしてエラー表示 |
| bat を `cmd /c` で起動するときの引数のクォート崩れ | 引数は配列で持ち、`CommandLineToArgvW` の規則どおりに組み立てる関数を core に置いて単体テストする |
| Git for Windows・PowerShell 7 が入っていない PC | 起動前に `where` 相当で確認し、無いものは案内を出す（Setup.bat と同じ文言） |
| 未署名 exe の SmartScreen 警告 | 初回は「詳細情報 → 実行」が要ると README に書く。署名は費用がかかるので見送り |
| mac 版を後で作るときに設定がずれる | 設定ファイルに最初から `mac` の欄を持たせ、CI チェックも `.command` を対象にしておく |

### 1.7 段階

1. `launcher-scripts.json` と CI チェック（C++ 無しで先に入れられる）
2. Win32 版の一覧・新しいコンソールでの実行・終了コード表示
3. `capture` モードの出力表示、引数プリセット、昇格の盾アイコン
4. GitHub Actions でのビルドと Release 添付
5. （後回し）mac 版

---

## 2. 仕様書ビューア（Unity EditorWindow）

### 2.1 前提（調べた事実）

- 写しは `Docs/NotionSpecifications`（サブモジュール、仕様書リポジトリ main）。Markdown 2,768 ファイル、画像など込みで約 346MB、フォルダの深さは最大 7。
- ルートに目録 `_catalog.jsonl`（1ページ1行、`path`・`title`・`page_id`・`url`・`last_edited`・`database`・`properties`・`headings`・`links`）と `_catalog.md` がある（同期 CI が生成、形式バージョン 1、2,687 ページ・79 DB）。`spec_query.py` と MCP `spec` も同じ目録を読む。
- 本文で使われている記法: 表（約 21,900 行。DB ページは大きな表）、見出し H1〜H4、チェックリスト、コードブロック、引用、`<details>`、画像（相対パス）、ページ間リンク（`%` エンコードした相対パス）、`https://app.notion.com/p/<id>` リンク（約 2,800 件）。
- 更新は既存の `Assets/Editor/Scripts/SinfoniaOperator/NotionSpecificationsAutoPull.cs` が、エディタ起動時に1回、裏で pull している。
- Unity は 6000.3.10f1。UI Toolkit に Markdown 表示の部品は無い。

### 2.2 画面案

```
┌ 仕様書ビューア ───────────────────────────────────────────┐
│ [検索: 回避________] [本文も検索□]  写し: 2026-11-12 14:34 [更新] │
├──────────────┬────────────────────────────────────────┤
│ ▼ Symphony Kill Chord │ # 回避アクション                         │
│   ▼ システム概要       │ Notionで開く ↗  最終更新 2026-10-30      │
│     ・回避アクション   │ ─────────────────────                 │
│   ▶ データベース       │ ### 概要 …                            │
│   …                  │ | 項目 | 値 |                          │
│ 〔検索結果 12件〕      │ ← 戻る / 進む →                        │
└──────────────┴────────────────────────────────────────┘
```

- 左: ツリー（`TreeView`、UI Toolkit）。目録の `path` からフォルダ階層を組む。検索中は結果一覧に切り替え。
- 右: 本文。見出しの目次（ページ内ジャンプ）、戻る・進む、「Notionで開く」。
- 検索: 目録のタイトル・プロパティ・見出しを即時検索（全ページ分がメモリに収まる）。「本文も検索」を付けたときだけ、裏スレッドで Markdown 本文を全文検索し、件数が出たところから順に表示する。
- 窓口: `Window/KillChord/SpecViewer`（`EditorWindowPathConst` に定数を足す）。

### 2.3 Markdown の表示方法

| 案 | 中身 | 長所 | 短所 |
| --- | --- | --- | --- |
| ⭐A | Markdig（BSD-2-Clause、.NET の Markdown パーサー）で構文木にし、UI Toolkit の要素（`Label`・表用の行列・`Foldout`・`Image`）に変換する自前レンダラー | 表・チェックリスト・HTML ブロックを正しく分解できる。描画は UI Toolkit なのでエディタの見た目に合う | DLL を `Assets/Editor/Plugins/` に足す（Editor 限定）。`third-party-notices.md` に追記が要る |
| B | パーサーも自前（行単位で見出し・表・リスト・コードを判定する最小実装） | 依存が増えない | 入れ子のリストや表の中のリンクなど、崩れる箇所が出やすい。保守がつらい |
| C | 本文はプレーンテキストで表示し、整形は「ブラウザで開く」に任せる | すぐ作れる | 「Unity の中で読む」目的を満たさない |

補足:
- 強調・リンクは UI Toolkit のリッチテキスト（`<b>`・`<i>`・`<link>`）で表し、リンクのクリックはリンクタグのイベントで受ける。
- ページ間リンク（相対パス）はビューア内で開く。`app.notion.com/p/<id>` は目録の `page_id` で写しのパスを引けたらビューア内で、引けなければブラウザで開く（質問 Q6）。
- 画像は表示されたときに `Texture2D.LoadImage` で読み、幅を窓に合わせる。動画・PDF はファイル名のリンクだけにする。
- 大きな DB の表は、一定行（例: 200 行）を超えたら `MultiColumnListView` で仮想化して表示する。

### 2.4 更新方法

- ⭐ 既存の `NotionSpecificationsAutoPull` の pull 処理を、ビューアの「更新」ボタンからも呼べるように内部メソッドとして公開する。新しい取得処理は作らない。
- 写しの日時は `git -C Docs/NotionSpecifications log -1 --format=%cI` で取り、ヘッダーに出す（Docs/agent/README の「読んだページと日時を報告に書く」と同じ値）。
- サブモジュールが未取得・閲覧権限が無いときは、`Setup.bat` を実行するよう案内を出す。
- 目録の形式バージョンが 1 以外なら、警告を出して目録を使わない（ツリーはフォルダ走査で組む）。
- ファイル監視（`FileSystemWatcher`）はしない。pull のあとと「更新」押下時に目録を読み直すだけでよい。

### 2.5 既存の規約・構成との整合

| 項目 | 方針 |
| --- | --- |
| アセンブリ | ⭐ `Assets/Editor/SpecViewer/` に `KillChord.Editor.SpecViewer.asmdef`（Editor 限定、`autoReferenced: false`）を作る。`ProjectSetup`・`AIDebugPlay` と同じ分け方。メニューの定数と pull 処理を使うため `KillChord.Editor` を参照する（逆向きの参照は作らない）。`KillChord.Editor` の中に置く案もある（質問 Q5） |
| 名前空間 | `KillChord.Editor.SpecViewer` |
| CG-073（AddComponentMenu） | 対象外。シーン・プレハブに置く MonoBehaviour ではなく EditorWindow のため。CI の Component Menu Check も Editor の EditorWindow は見ない想定（実装時に確認する） |
| CG-010（メンバー順）・XML コメント | 既存の `SinfoniaOperatorWindow` と同じ書き方にそろえる |
| 層の分け方 | 目録の読み込み・検索・Markdown→表示モデル変換を UI から分け、表示モデル部分は EditMode テストを書く |
| 文字コード | CG-001 のとおり UTF-8 |
| ランタイムへの影響 | なし（Editor 限定アセンブリ・Editor 限定 DLL） |

### 2.6 リスク・注意点

| リスク | 対策 |
| --- | --- |
| 346MB を起動時に読むと重い | 起動時は目録（数 MB）だけ読む。本文・画像は開いたときに読む |
| Notion のエクスポート形式が変わり表示が崩れる | 表示できない記法はそのままテキストで出す（落とさない）。代表ページ数件で EditMode のスナップショット的なテストを置く |
| pull 中に開いたファイルが書き換わる | pull 中は「更新中」を表示し、終わったら目録を読み直して今のページを開き直す |
| 写しは読み取り専用であることを誤解される | 画面に「この写しは Notion から自動生成。直すのは Notion」と出し、編集機能は作らない |
| Markdig の導入 | Editor 限定にしてビルドに含めない。バージョンを固定して `third-party-notices.md` に載せる |

### 2.7 段階

1. 目録からツリーと検索（タイトル・見出し・プロパティ）、本文はプレーンテキスト表示
2. Markdig によるレンダリング（見出し・段落・リスト・表・コード・引用・リンク）
3. 画像、`<details>`、大きな表の仮想化、戻る・進む、目次
4. 本文の全文検索、`app.notion.com` リンクのローカル解決

---

## 3. 決めてほしいこと（⭐が推奨）

1. **Q1「Androidビルドのインポート」のスクリプトの場所**: リポジトリには見つからなかった。PC だけにあるなら場所を教えてほしい。⭐ 無ければ、今ある入口（Setup / Clean-Branches / runner-menu / start-runner / dev）で初版を作る。
2. **Q2 管理者権限の方式**:
   - ⭐ (a) ツールは通常権限で起動し、`"elevate": true` のスクリプトだけ実行時に UAC で昇格する
   - (b) 依頼どおり、ツールごと管理者で起動する（`requireAdministrator`）。1.2 の害（git の所有者エラー、ランナーが管理者で動く）を受け入れることになる
3. **Q3 スクリプトの登録方法**: ⭐ 設定ファイル `launcher-scripts.json` ＋ CI で登録漏れを警告 ／ ルートの自動検出だけ
4. **Q4 exe の配り方**: ⭐ GitHub Actions でビルドして Release に添付 ／ exe をリポジトリにコミット ／ 各自が CMake でビルド
5. **Q5 ビューアのアセンブリ**: ⭐ 専用の `KillChord.Editor.SpecViewer` を作る ／ `KillChord.Editor` の中に置く
6. **Q6 Markdown の描画**: ⭐ Markdig ＋ UI Toolkit の自前レンダラー（案A）／ パーサーも自前（案B）／ ブラウザ任せ（案C）
7. **Q7 着手の順番**: ⭐ 仕様書ビューア（依存が少なく既存の pull を使える）→ スクリプト管理ツール ／ 逆の順番
