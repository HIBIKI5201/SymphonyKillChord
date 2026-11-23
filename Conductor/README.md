# KillChord Conductor（プロジェクト管理アプリ）

全役職がクローンして使う、このプロジェクトの入口のアプリ。名前は「指揮者」から。各パート（役職）の仕事をここから始める。

- **GUI** `KillChordConductor.exe`（Windows）: Unity・CRI Atom Craft・素材・ドキュメントを開くホーム、すべての操作、実行の記録、設定。自分では何も実行せず、CLI を呼ぶ。
- **CLI** `conductor.exe`（Windows。mac・Linux でもビルドできる）: GUI でできることは、すべてこれでもできる。
- どちらもリポジトリのルートに置く。C++20。GUI の画面は `Conductor/ui` の HTML で、Windows に入っている WebView2（Microsoft Edge と同じ部品）で出す。

```
┌ 画面  Conductor/ui（HTML・CSS・JS）┐  ボタンごとに、同じことをする CLI のコマンドを出す
│        ↓ WebView2 のメッセージ（cli・run・stop・pick）
├ GUI  KillChordConductor.exe      ┤  画面を出し、頼まれたら conductor.exe を子プロセスで呼ぶだけ
│        ↓ 子プロセスとして呼ぶ（一覧・状態は --json で受け取る）
├ CLI  conductor.exe               ┤  conductor home / status / unity / cri / assets / docs / scripts / config
│        ↓ 同じ C++ のライブラリ
├ core（src/core）             ┤  登録ファイルの読込・コマンドの組立・JSON の出し入れ
│        ↓
└ 既存のスクリプト・外部ツール    ┘  Setup.bat、runner-menu、git …
```

## 使い方（GUI）

1. ルートの `KillChordConductor.exe` をダブルクリックする。初回は SmartScreen の警告が出ることがある（署名していないため）。「詳細情報」→「実行」で開く。
2. 役職が決まっていなければ、ホームの「気をつけること」に出る。「設定」で役職を選ぶと、その役職がよく使う操作が前に並ぶ（ほかの操作も消えない）。役職はこの PC だけの設定で、git には入らない。
3. **ホーム**
   - 上の帯: ブランチ・未コミット・サブモジュール・Unity の状態。前に出てきたときと 15 秒ごとに読み直す。
   - **起動する**: Unity・CRI Atom Craft・素材（Google Drive）・ドキュメントのカード。入っていない・見つからないときは、カードのボタンが「Unity Hub で入れる」「初期化して取る」「場所を選ぶ」に変わる。
   - **気をつけること**: サブモジュールを取っていない・Unity CLI が無い・リモートより遅れている、など。直すボタンが横に出る。
   - **よく使う操作**: home.json のボタン。役職の分が前に並ぶ。
4. **すべての操作**: conductor の組み込みの操作と、登録したスクリプトの一覧（前の ScriptLauncher の画面の代わり）。名前・説明・コマンドで探せる。選ぶと右に引数の組・同じことをするコマンド・動かし方が出る。Enter かダブルクリックで動かす。
   - **新しいウィンドウで開く**もの: 入力を待つもの・常駐するもの。アプリを閉じても止まらない。
   - **この画面に出す**もの: 「確認だけ」「候補を見るだけ」など、入力を待たないもの。下から出力の欄が出る。「止める」で子プロセスごと止まる。
5. **実行の記録**: 動かしたものと結果。選ぶと出力をもう一度見られる（この PC の WebView2 の保存領域に、最近の 40 件だけ残す）。
6. **探す・動かす**（`Ctrl+K`）: 操作・スクリプトの引数の組・ドキュメント・ページ・役職を、言葉で探してそのまま動かす。
7. キー: `Ctrl+K` 探す、`Ctrl+1`〜`4` ページ、`/` すべての操作で探す、`F5` 読み直す、`Esc` 閉じる。

画面が開かないときは、WebView2 ランタイムが入っていない（Windows 10 の古い版など）。出てくる確認で「はい」を押すと入れるページが開く。それまでは CLI で同じことができる。

### 画面だけを直すとき

`Conductor/ui` を直したら、アプリを開き直すだけで反映される（exe を作り直さなくてよい）。ブラウザで見るときは、`Conductor/ui` で `python -m http.server` などを動かして `index.html` を開く。そのときは `dev-mock.js` が conductor の代わりにそれらしい答えを返す（`?state=fresh` を付けると、初期化前の PC のつもりで答える）。環境変数 `KILLCHORD_CONDUCTOR_DEVTOOLS=1` を付けてアプリを開くと、F12 で開発者ツールが開く。

## 使い方（CLI）

リポジトリの中で動かす（PowerShell の例）。

```powershell
.\conductor.exe home                          # ホームのボタンと、同じことをするコマンド
.\conductor.exe status                        # ブランチ・未コミット・サブモジュール・Unity・CRI・Drive の状態
.\conductor.exe unity open                    # このプロジェクトを Unity で開く（Unity CLI → エディタの順に探す）
.\conductor.exe unity install                 # この版の Unity を Unity Hub で入れる画面を開く
.\conductor.exe cri open                      # CRI Atom Craft でサウンドのプロジェクトを開く
.\conductor.exe assets open                   # Google Drive の SymphonyKillChord フォルダを開く
.\conductor.exe assets import                 # 開いている Unity で、Drive の素材を取り込む
.\conductor.exe docs list                     # よく見るドキュメント（Notion・GitHub）
.\conductor.exe docs open specs               # ドキュメントをブラウザで開く
.\conductor.exe scripts list                  # 登録したスクリプトと引数の組
.\conductor.exe scripts run setup --preset 2  # 「プロジェクトの初期化」を「確認だけ」で動かす
.\conductor.exe config set role sound         # 役職を設定する（programmer / planner / designer / sound / lead）
.\conductor.exe --help
```

- `--json` を付けると、GUI・AI エージェント向けに JSON で出す。
- `--root <パス>` でリポジトリを指定できる（省くと、今のフォルダと exe の場所から上へ探す）。
- `scripts run` は、スクリプトの終了コードをそのまま返す。確認の文があるスクリプトは `[y/N]` で聞く（`--yes` で飛ばす）。
- conductor 自身の終了コード: 0 成功、1 失敗、2 使い方の誤り、3 前提が足りない、4 取り消した。
- この PC だけの設定は `%LOCALAPPDATA%\KillChordConductor\settings.json`（mac・Linux は `~/.config/killchord-conductor/settings.json`）。キーは `role`・`unityEditorPath`・`criAtomCraftPath`・`driveFolder`（`conductor config list` で説明が出る）。
- `unity open` は、`unityEditorPath` を設定していればそのエディタ、無ければ Unity CLI（`unity open`）、それも無ければ Unity Hub の入れ先（`%ProgramFiles%\Unity\Hub\Editor\<版>`）を使う。どれも無ければ終了コード 3 で、Unity Hub で入れる URL を出す。
- `assets import` と `docs viewer` は、開いている Unity のメニュー（`Tools/KillChord/DriveImportSync/Sync Now`・`Window/KillChord/SpecViewer`）を Unity CLI の `script eval` で動かす。`conductor unity menu <パス>` でほかのメニューも動かせる。
- `cri open` は `Sound/CRIAtomCraft` の `.atmcproject` を探して開く。CRI Atom Craft の exe は `criAtomCraftPath`、無ければ Program Files などから探し、それも無ければファイルの関連付けで開く。
- mac・Linux では、登録の `mac` の欄（`.command`）を bash で動かす。

### 管理者の権限について

アプリは通常の権限で動く（管理者で開かない）。管理者で動かすと、そこから動かしたスクリプトもすべて管理者になり、次の害が出る。

- 作ったファイル・フォルダの持ち主が Administrators になり、あとで通常の権限の git・Unity から `dubious ownership` や書き込みのエラーが出る。
- ビルドランナーのジョブ（ワークフローのコード）が、PC を管理者の権限で操作できてしまう。

管理者が要るスクリプトだけ、登録ファイルに `"elevate": true` と書く。一覧と「実行」ボタンに盾の印が付き、CLI が実行のときに UAC の確認を出す。今の登録には、管理者が要るものは無い。

## スクリプトを登録する

正本は [launcher-scripts.json](launcher-scripts.json)。アプリは前に出てきたときに読み直すので、書き換えたあと開き直さなくてよい。

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
| `id` | ○ | 重複しない識別子（`conductor scripts run <id>` で使う） |
| `name` / `category` | ○ | 一覧に出す名前と分類（分類は書いた順に並ぶ） |
| `windows` | ○ | リポジトリのルートからの `/` 区切りのパス。`.bat` / `.cmd` / `.ps1` / `.exe` |
| `mac` | | mac・Linux の CLI で動かす `.command` のパス |
| `description` | | 説明 |
| `mode` | | `console`（新しいウィンドウ、既定）か `capture`（出力をこの画面に出す）。入力を待つものを `capture` にすると、入力が空で進む |
| `presets` | | 引数の組。`mode` を組ごとに変えられる。無ければ「引数なし」だけ |
| `elevate` | | `true` なら管理者として動かす。`capture` とは一緒にできない |
| `confirm` | | 動かす前に出す確認の文（「はい」を押したときだけ動く） |
| `localOnly` | | `true` なら、git の管理外で PC にだけ置くファイル（`Builds/` など）。無いときは「この PC に無い」と出す |

`.bat` に渡す引数には、`"` `&` `|` `<` `>` `^` `%` `!` `(` `)` を使えない（cmd.exe が特別に扱うため）。

`.bat` / `.command` を足したのに登録し忘れると、PR の「[CI/CD] Conductor」が警告を出す。わざと登録しないものは、`"ignored": [{ "path": "...", "reason": "..." }]` に理由と一緒に書く。手元では `python scripts/conductor/check_launcher_scripts.py` で調べられる。

## ホーム画面のボタンを登録する

正本は [home.json](home.json)。

```json
{
  "version": 1,
  "roles": [ { "id": "sound", "label": "サウンド" } ],
  "tiles": [
    { "id": "setup-check", "title": "初期化できているか確かめる", "note": "ボタンの下の説明",
      "script": "setup", "preset": "確認だけ（何も変えない）", "roles": ["sound"] },
    { "id": "unity-open", "title": "Unity を開く", "action": "unity-open", "roles": ["sound"] }
  ],
  "links": [
    { "id": "specs", "title": "仕様リスト", "note": "説明", "url": "https://www.notion.so/..." }
  ]
}
```

- `script`・`action`・`page`（`scripts` か `settings`）のどれか1つを書く。`script` は launcher-scripts.json の `id`、`preset` はその引数の組の `label`（省くと最初の組）。
- `action` は conductor の組み込みの操作: `unity-open`・`unity-install`・`cri-open`・`assets-open`・`assets-import`・`docs-viewer`・`repo-open`。起動のカード（Unity・CRI・素材・ドキュメント）の並びも、役職の順に並んだこれらのボタンの順で決まる。
- `links` はドキュメントのカードと `conductor docs` に出る。`url` は `https://` で始める。
- `roles` に書いた役職の人には、そのボタンが前に並ぶ。書いていない役職でも、ボタンは後ろに出る。
- 無いスクリプト・引数の組・役職・操作を書くと、CLI がエラーを出し、ctest（PR の CI）も失敗する。

## ビルド

Visual Studio 2022 以降（「C++ によるデスクトップ開発」）と CMake 3.25 以降が要る。

```powershell
cmake -S Conductor -B Conductor\build -A x64
cmake --build Conductor\build --config Release
ctest --test-dir Conductor\build -C Release --output-on-failure
Copy-Item Conductor\build\Release\conductor.exe, Conductor\build\Release\KillChordConductor.exe .\
```

Visual Studio の「フォルダーを開く」で `Conductor` フォルダを開いてもビルドできる。CRT は静的リンク（`/MT`）なので、VC++ の再頒布パッケージを入れていない PC でも動く。MinGW-w64（`-static`）でもビルドできる（WebView2.h が使う `EventToken.h` は `third_party/webview2/mingw` の小さな写しで補う）。

GUI は `WebView2Loader.dll` を実行時に読む（静的リンクしない）。exe の隣に無ければ `Conductor/third_party/webview2/x64/WebView2Loader.dll` を使うので、ルートに dll を置かなくてよい。閲覧の記録などは `%LOCALAPPDATA%\KillChordConductor\WebView2` に置く。

コードを直したら、ルートの `conductor.exe` と `KillChordConductor.exe` も作り直してコミットする。PR では「[CI/CD] Conductor」が MSVC でビルドし、2つの exe を成果物（Artifacts）に置く。

## 構成

```
Conductor/
├── CMakeLists.txt
├── launcher-scripts.json   … スクリプトの登録（正本）
├── home.json               … ホーム画面のボタンと役職（正本）
├── src/core/               … OS に依存しない部分（登録の読込・コマンドの組立・出力の整形・JSON）。どの OS でもテストできる
├── src/cli/                … CLI（conductor）。PlatformWindows.cpp / PlatformPosix.cpp で OS の違いを分ける
├── src/gui/                … GUI（KillChordConductor）。WebView2 で画面を出し、画面の頼みを CLI に渡す・manifest
├── src/win32/              … CLI と GUI が共有する Windows 用の部品（文字コードの変換）
├── ui/                     … 画面（index.html・app.css・app.js・icons.js。dev-mock.js はブラウザで見るときの仮の答え）
├── tests/CoreTests.cpp     … core の単体テスト（CTest）。CLI を実際のリポジトリで動かすテストも CMakeLists.txt にある
├── third_party/nlohmann/   … JSON の読み込み（nlohmann/json 3.11.3、MIT License）
└── third_party/webview2/   … WebView2 SDK 1.0.2903.40 のヘッダーと x64 の WebView2Loader.dll（BSD 3-Clause License）
```

- `.bat` は `cmd.exe /d /s /c`、`.ps1` は Windows PowerShell（`-ExecutionPolicy Bypass`）で動かす。どちらも System32 のものを使う。GUI が新しいウィンドウで開く `.ps1` は、終わったあと Enter を押すまでウィンドウを残す（CLI の `--hold`）。
- 作業フォルダはスクリプトのあるフォルダ（ダブルクリックしたときと同じ）。
- GUI が出力を受けるときは、CLI の標準出力をパイプで受け、CLI はそのパイプをスクリプトへそのまま渡す。出力は行ごとに、UTF-8 として読めれば UTF-8、読めなければ OEM コードページ（日本語の Windows なら CP932）として読む。色の指定（ANSI のエスケープ）は取り除く。
- 設計の経緯は [Docs/agent/reports/2026-11-24_killchord-conductor-design.md](../Docs/agent/reports/2026-11-24_killchord-conductor-design.md)（前身の ScriptLauncher は [2026-11-12_cli-launcher-and-spec-viewer.md](../Docs/agent/reports/2026-11-12_cli-launcher-and-spec-viewer.md)）。
- 画面と GUI のやりとり: 画面は `chrome.webview.postMessage` で `{type, id, ...}` を送り、GUI は `{type: "reply", id, result}` で答える。`cli`（conductor を動かして標準出力・標準エラー・終了コードを返す）、`run`（スクリプトを動かす。`kind` は console・capture・hidden）、`stop`、`pick`（ファイル・フォルダを選ぶ画面）、`copy`（クリップボード）。出力は `output`・`exited` で届く。画面は仮のホスト名 `https://conductor.example/` で `Conductor/ui` を読み、ほかの URL へは移らない（リンクは `conductor docs open` でブラウザに開く）。
