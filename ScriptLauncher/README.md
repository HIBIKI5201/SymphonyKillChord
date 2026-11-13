# スクリプト管理ツール（ScriptLauncher）

リポジトリにある、ダブルクリックで動かすスクリプト（`.bat` / `.ps1`）を、一覧から選んで動かす Windows のツール。
C++20 と素の Win32 API で作っている。mac 版はまだ無い（設定ファイルには `mac` の欄を先に持たせている）。

## 使い方

1. ルートの `ScriptLauncher.exe` をダブルクリックする。初回は SmartScreen の警告が出ることがある（署名していないため）。「詳細情報」→「実行」で開く。
2. 左の一覧からスクリプトを選び、「引数」で動かし方を選んで「実行」を押す（一覧をダブルクリックしても動く）。
   - **新しいウィンドウで開く**もの: 入力を待つもの・常駐するもの。今までどおりのコンソールが開く。ツールを閉じても止まらない。
   - **出力をこの画面に出す**もの: 「確認だけ」「候補を見るだけ」「状態を見る」など、入力を待たないもの。右下に出力が出る。「止める」で子プロセスごと止まる。ツールを閉じても止まる。
3. 「前回の結果」の列に、成功・失敗（終了コード）が出る。

### 管理者の権限について

ツールは通常の権限で動く（ツールを管理者で開かない）。管理者で動かすと、そこから動かしたスクリプトもすべて管理者になり、次の害が出る。

- 作ったファイル・フォルダの持ち主が Administrators になり、あとで通常の権限の git・Unity から `dubious ownership` や書き込みのエラーが出る。
- ビルドランナーのジョブ（ワークフローのコード）が、PC を管理者の権限で操作できてしまう。

管理者が要るスクリプトだけ、登録ファイルに `"elevate": true` と書く。一覧と「実行」ボタンに盾の印が付き、実行のときに UAC の確認が出る。今の登録には、管理者が要るものは無い。

## スクリプトを登録する

正本は [launcher-scripts.json](launcher-scripts.json)。ツールは前に出てきたときに読み直すので、書き換えたあと開き直さなくてよい。

```json
{
  "id": "setup",
  "name": "プロジェクトの初期化",
  "category": "初期化",
  "windows": "Setup.bat",
  "mac": "Setup.command",
  "description": "一覧の右に出す説明",
  "mode": "console",
  "presets": [
    { "label": "初期化する", "args": [] },
    { "label": "確認だけ（何も変えない）", "args": ["-Check"], "mode": "capture" }
  ]
}
```

| 欄 | 必須 | 中身 |
| --- | --- | --- |
| `id` | ○ | 重複しない識別子 |
| `name` / `category` | ○ | 一覧に出す名前と分類（分類は書いた順に並ぶ） |
| `windows` | ○ | リポジトリのルートからの `/` 区切りのパス。`.bat` / `.cmd` / `.ps1` / `.exe` |
| `mac` | | mac 版を作るとき用。`.command` のパス |
| `description` | | 説明 |
| `mode` | | `console`（新しいウィンドウ、既定）か `capture`（出力をこの画面に出す）。入力を待つものを `capture` にすると、入力が空で進む |
| `presets` | | 引数の組。`mode` を組ごとに変えられる。無ければ「引数なし」だけ |
| `elevate` | | `true` なら管理者として動かす。`capture` とは一緒にできない |
| `confirm` | | 動かす前に出す確認の文（「はい」を押したときだけ動く） |
| `localOnly` | | `true` なら、git の管理外で PC にだけ置くファイル（`Builds/` など）。無いときは「この PC に無い」と出す |

`.bat` に渡す引数には、`"` `&` `|` `<` `>` `^` `%` `!` `(` `)` を使えない（cmd.exe が特別に扱うため）。

`.bat` / `.command` を足したのに登録し忘れると、PR の「[CI/CD] Script Launcher」が警告を出す。わざと登録しないものは、`"ignored": [{ "path": "...", "reason": "..." }]` に理由と一緒に書く。手元では `python scripts/launcher/check_launcher_scripts.py` で調べられる。

## ビルド

Visual Studio 2022 以降（「C++ によるデスクトップ開発」）と CMake 3.25 以降が要る。

```powershell
cmake -S ScriptLauncher -B ScriptLauncher\build -A x64
cmake --build ScriptLauncher\build --config Release
ctest --test-dir ScriptLauncher\build -C Release --output-on-failure
Copy-Item ScriptLauncher\build\Release\ScriptLauncher.exe .\ScriptLauncher.exe
```

Visual Studio の「フォルダーを開く」で `ScriptLauncher` フォルダを開いてもビルドできる。CRT は静的リンク（`/MT`）なので、VC++ の再頒布パッケージを入れていない PC でも動く。

コードを直したら、ルートの `ScriptLauncher.exe` も作り直してコミットする。PR では「[CI/CD] Script Launcher」が MSVC でビルドし、exe を成果物（Artifacts）に置く。

## 構成

```
ScriptLauncher/
├── CMakeLists.txt
├── launcher-scripts.json   … スクリプトの登録（正本）
├── src/core/               … OS に依存しない部分（設定の読込・コマンドの組立・出力の整形）。Linux でもテストできる
├── src/win32/              … 画面・プロセスの起動・manifest
├── tests/CoreTests.cpp     … core の単体テスト（CTest）
└── third_party/nlohmann/   … JSON の読み込み（nlohmann/json 3.11.3、MIT License）
```

- `.bat` は `cmd.exe /d /s /c`、`.ps1` は Windows PowerShell（`-ExecutionPolicy Bypass`）で動かす。どちらも System32 のものを使う。新しいウィンドウで開く `.ps1` は、終わったあと Enter を押すまでウィンドウを残す。
- 作業フォルダはスクリプトのあるフォルダ（ダブルクリックしたときと同じ）。
- 出力は行ごとに、UTF-8 として読めれば UTF-8、読めなければ OEM コードページ（日本語の Windows なら CP932）として読む。PowerShell は UTF-8、cmd.exe の `pause` などは CP932 で出すため。色の指定（ANSI のエスケープ）は取り除く。
- 設計の経緯は [Docs/agent/reports/2026-11-12_cli-launcher-and-spec-viewer.md](../Docs/agent/reports/2026-11-12_cli-launcher-and-spec-viewer.md)。
