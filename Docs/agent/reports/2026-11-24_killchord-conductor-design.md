# ScriptLauncher を全役職の管理アプリ（KillChord Conductor）に作り直す設計

- 作成日: 2026-11-24（日本時間）
- 依頼: ScriptLauncher を、全役職がクローンして使う中央アプリにしたい。起動時は Unity・CRI・アセット管理などを開くホーム画面にし、スクリプトランチャーは下の階層へ。CLI 単体でも動き、GUI から CLI を操作できる層の構造にしたい。名前は KillChordAdmin か、より良い名前。
- 決定（2026-11-24、ユーザー「全部⭐で」）: 名前は KillChord Conductor（1-B）、GUI は CLI を子プロセスで呼ぶ（2-A）、役職は並び順だけ変える（3-A）、C++・素の Win32 のまま（4-A）、5段階に分けて1段階ずつ PR にする（5-A）。
- 第1段階（改名・CLI 化・ホーム画面・スクリプト一覧を下の階層へ）は同じ日に実装した。CLI のコマンドは `home`・`status`・`scripts`・`config`。`unity`・`cri`・`assets`・`docs` は次の段階で足す。
- 調べたもの: develop `c4be5e93` の `ScriptLauncher/`（C++20・Win32、core 約 830 行・win32 約 1,500 行）、`launcher-scripts.json`（7 件）、`Assets/Editor/Scripts/AssetManagement/`（DriveImportSync）、`SinfoniaOperator/`、`Sound/CRIAtomCraft`（非公開サブモジュール）、11-12 の検討（`2026-11-12_cli-launcher-and-spec-viewer.md`）。

---

## 1. 名前

| 案 | 中身 | 良い点 | 気になる点 |
| --- | --- | --- | --- |
| A. KillChordAdmin | 依頼どおり | 意図がすぐ分かる | 「Admin」は Windows の「管理者権限」と同じ言葉。今のツールは「管理者で開かない」が大事な約束なので、名前と説明が食い違う。リード専用の道具にも見え、全役職の入口という狙いとずれる |
| ⭐B. KillChord Conductor | 指揮者。オーケストラの各パート（＝各役職）をまとめて動かす | 「Symphony」の世界観に合う。全役職の中心という役割をそのまま表す。CLI 名 `conductor` が短く打ちやすい | 初見で「管理アプリ」と分かりにくいので、ウィンドウの題に「プロジェクト管理」と添える |
| C. KillChordHub | 中央の入口 | 分かりやすい | Unity Hub と名前が近く、「Unity Hub を開く」と言ったときに紛らわしい |

名前が決まると、次が変わる。

- 実行ファイル: ルートの `ScriptLauncher.exe` → `KillChordConductor.exe`（GUI）と `conductor.exe`（CLI）
- フォルダ: `ScriptLauncher/` → `Conductor/`
- CI: `[CI/CD] Script Launcher` → `[CI/CD] Conductor`、`scripts/launcher/` → `scripts/conductor/`
- 旧 `ScriptLauncher.exe` は消す（README に移行の一文を書く）

## 2. 層の構造

```
┌──────────────────────────────────────────┐
│ GUI  KillChordConductor.exe（Windows）           │  ホーム画面・各ページ。自分では何も実行しない
│   └ 押したボタンに当たる CLI コマンドを画面に出す      │
├──────────── 子プロセスとして呼ぶ（--json）─────────┤
│ CLI  conductor.exe（Windows・mac でビルドできる）   │  conductor unity open / conductor scripts run setup --preset check …
├──────────── 同じ C++ ライブラリ ───────────────┤
│ core  コマンド定義・設定読込・プロセス起動・出力整形    │  今の ScriptLauncher/src/core を広げる
├────────────────────────────────────────────┤
│ 既存のスクリプト・外部ツール                        │  Setup.bat、runner-menu、Unity CLI、CRI Atom Craft、gh …
└──────────────────────────────────────────┘
```

| 案 | GUI と CLI のつなぎ方 | 良い点 | 気になる点 |
| --- | --- | --- | --- |
| ⭐A | GUI は `conductor.exe … --json` を子プロセスで呼び、結果の JSON を表示する | GUI でできることは必ず CLI でもできる（層が崩れない）。画面に「同じことをするコマンド」を出せるので、慣れた人や AI エージェントはそのまま CLI で使える。CLI だけ先に mac へ持っていける | 1 操作ごとにプロセスを起こす（数十 ms。体感では分からない程度） |
| B | GUI も core を直接呼ぶ（同じプロセス） | 速い。作りが少し楽 | GUI だけの近道を作りやすく、CLI と挙動がずれていく |

- CLI は Windows のコンソール用（console subsystem）、GUI はウィンドウ用で、exe を 2 つに分ける。1 つの exe で両方を兼ねると、コンソールへの出力やダブルクリック時の黒い窓の扱いが不安定になるため。
- CLI の出力は、人が読む表（既定）と `--json`（GUI・AI 用）の 2 通り。終了コードの意味をそろえる（0 成功、1 失敗、2 使い方の誤り、3 前提が足りない）。

## 3. CLI のコマンド（初版）

```
conductor status                       … ブランチ・未コミット・サブモジュール・Unity と CRI の有無をまとめて表示
conductor unity open|status            … Unity CLI（unity）でこのプロジェクトを開く。CLI が無ければ Unity Hub で開く
conductor cri open                     … CRI Atom Craft で Sound/CRIAtomCraft のプロジェクトを開く（サブモジュールが無ければ権限の案内）
conductor assets open|sync             … Drive の取込フォルダを開く／Unity の DriveImportSync 窓を開く
conductor scripts list|run <id> [--preset <label>]   … 今のスクリプトランチャー（launcher-scripts.json）
conductor docs open                    … Notion のプロジェクト ドキュメント・Unity の仕様書ビューアを開く
conductor config get|set <key> <value> … この PC だけの設定（CRI Atom Craft の場所、役職など）
```

- `scripts` は今の `launcher-scripts.json` をそのまま使う（既存の 7 件と CI チェックを壊さない）。
- PC ごとの設定（CRI Atom Craft の exe の場所、選んだ役職）は git の管理外 `%LOCALAPPDATA%\KillChordConductor\settings.json` に置く。
- Unity・CRI・アセットのように中で判断が要るものは C++ の組み込みコマンドにする。単にスクリプトを動かすものは JSON 登録のまま。

## 4. 画面

```
┌ KillChord Conductor ─ プロジェクト管理 ───────────────── 役職: [サウンド ▼] ┐
│ ┌ 状態 ───────────────────────────────────────────────┐ │
│ │ develop（最新から 3 コミット遅れ）  未コミット 0  サブモジュール ✓  Unity 6000.3.10f1 ✓ │ │
│ └────────────────────────────────────────────────────┘ │
│  ［Unity を開く］ ［CRI Atom Craft を開く］ ［アセットを取り込む］ ［仕様書を見る］        │
│  ［ビルド・端末へ入れる］ ［プロジェクトの初期化］ ［スクリプト一覧 →］                  │
│                                                                   │
│ 最近の実行:  Setup -Check 成功（2 分前）  …                                     │
│ > conductor cri open        ← 押したボタンと同じ CLI コマンド                       │
└───────────────────────────────────────────────────────────────────┘
```

- ホーム: 上に状態、真ん中に大きな起動ボタン、下に最近の実行と、今のボタンに当たる CLI コマンド。
- 役職: 初回起動で選ぶ（プログラマー・プランナー・デザイナー・サウンド・リード）。⭐ 全部のボタンは常に出し、選んだ役職でよく使うものを前に並べるだけにする（隠すと「どこにあるか分からない」が起きるため）。
- 下の階層: 「スクリプト一覧」ページ＝今の ScriptLauncher の画面（一覧・プリセット・出力欄）。ほかに「設定」ページ。
- 技術: 今と同じ C++20・素の Win32（ランタイムの導入不要、日本語入力も問題なし）。

## 5. 段階

1. 改名と層分け: `Conductor/` へ移し、CLI `conductor.exe`（`scripts`・`status`・`config`）を作る。GUI はホーム＋スクリプト一覧ページにし、スクリプトの実行を CLI 経由に置き換える。
2. 起動ボタン: `unity`・`cri`・`docs`。
3. アセット: `assets`（Drive フォルダ・DriveImportSync 窓）。
4. ビルド: AutoBuilder・ランナー・Install_App をまとめたページ。
5. （後回し）mac 用 GUI。CLI は 1 の時点で mac でもビルドできる。

各段階を 1 本の feature-flow（agent→master→develop Draft）にする。

## 6. リスク

| リスク | 対策 |
| --- | --- |
| CRI Atom Craft の場所が PC ごとに違う | 既定の場所を探し、無ければ設定ページで選ばせて `settings.json` に保存 |
| `Sound/CRIAtomCraft` は非公開リポジトリ | 取れていなければ「閲覧権限をリードに頼む」と出す（サウンド以外の役職では警告にしない） |
| Unity CLI が無い PC | Unity Hub で開く方へ戻し、`Setup.bat` で CLI を入れる案内を出す |
| 改名で古い exe が残る | 旧 exe はコミットから消す。各 PC で pull すれば消える |
| 全役職が使う | 文言は今の README と同じく専門用語を避ける。危ない操作（アンインストール等）は今の `confirm` をそのまま使う |
