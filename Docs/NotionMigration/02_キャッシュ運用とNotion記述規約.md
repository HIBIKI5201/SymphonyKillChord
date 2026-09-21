# キャッシュ運用と Notion 記述規約

作成日: 2026-09-22

## 0. 決定事項

2026-09-22、八幡さんが次のとおり決定した（2 回に分けて指示があった）。

| 項目 | 決定 |
| --- | --- |
| 正本 | **Notion が正本である。** リポジトリ上の文書は Notion の写し（キャッシュ）とする |
| ツール用の文書 | コード規約・QA 対応表など、AI スキルやツールが読む文書も Notion に置く。リポジトリにはそのキャッシュを置き、ツールはキャッシュを読む |
| 書き方 | キャッシュとして機械に読ませる前提で、Notion 側の文書の書き方を見直す |
| 書き込み許可範囲 | NotionMarkdownWriter の許可範囲の拡張は後日行う |
| **キャッシュの置き場** | **`Library/` 配下に置き、git では管理しない。** 今は `Docs/NotionSpecifications/` に置かれているので、`Library/` へ移す |
| **キャッシュの更新** | **各自が手動で実行する。加えて、スキルから AI が実行する。**（CI での定期実行はしない） |
| **ログ** | **`Docs/` に `agent` ディレクトリを作り、そこで管理する** |

この決定で [90_未決事項.md](90_未決事項.md) の D-01〜D-03・D-05・D-11〜D-14 が決着した。
本書は、決定を実現するための**キャッシュの仕組みの設計案**と、**Notion 側の記述規約案**である。
パス名など細部はまだ案であり、実装（§2.6 の T-1〜T-7）のときに確定させる。

---

## 1. 現状の仕組みと、変える点

Notion からリポジトリへ落とす仕組みは既に 1 つある。`SinfoniaOperator/NotionMarkdownExporter.exe` が、ルートページ配下の全ページを `Docs/NotionSpecifications/`（gitignore 対象）に書き出す。
これを次の点で変える。

| # | 現状 | 変える点 |
| --- | --- | --- |
| 1 | 出力先が `Docs/NotionSpecifications/` | **`Library/` 配下へ移す**（決定）。`Docs/` はエージェントのログ置き場になる |
| 2 | 出力パスが**ページタイトルの階層**で決まる（例: `.../Symphony Kill Chord/システム概要/コード規定.md`） | Notion でページ名を変えたり移動したりするとパスが変わり、そのパスを読むスキルが壊れる。**ツールが読むページは page id で固定パスに結び付ける**（§2.2） |
| 3 | 取得は常にルート配下の全ページ（約 2,000 ページ、初回は数十分） | スキルが要るのは十数ページである。**指定したページだけを取り直すモード**を足す（§2.6 T-2） |
| 4 | 出力ファイルに正本の URL と取得時点が書かれない | ファイルの先頭に書く（§2.3） |

---

## 2. 設計案

### 2.1 置き場

| 種類 | 置き場（案） | 中身 | git |
| --- | --- | --- | --- |
| 全体ミラー | `Library/NotionSpecifications/` | 仕様書の全ページ（現在の `Docs/NotionSpecifications/` を移す） | 管理しない（`/[Ll]ibrary/` は既に `.gitignore` の対象） |
| ツール用キャッシュ | `Library/NotionCache/` | スキル・AI 指示が固定パスで読むページだけ | 管理しない |
| キャッシュマップ | `SinfoniaOperator/notion-cache-map.json` | page id と `Library/NotionCache/` 内のパスの対応表 | **管理する**（全員で同じ対応表を使うため） |
| エージェントのログ | `Docs/agent/` | 不具合ログ・差分レポートなど（§2.5） | D-16 で決める |

`Library/` を選ぶ利点と、気をつける点:

- `Library/` は Unity の生成物置き場で、もともと git の対象外である。消えても作り直せるという性質が、キャッシュと一致する。
- Unity の不具合対処で `Library/` を丸ごと消すと、キャッシュも消える。スキルは「キャッシュが無ければ取り直す」前提で書く（§2.4）。
- Unity が `Library/` 直下の知らないフォルダを消すことはないが、作業の前に実機で一度確かめる（T-1 の受入条件）。

### 2.2 固定パスの付け方（キャッシュマップ）

パスをページタイトルから作らず、**キャッシュマップで page id とパスを結び付ける**。Notion 側でページ名を変えても、移動しても、キャッシュのパスは変わらない。

```json
// SinfoniaOperator/notion-cache-map.json（案）
{
  "output": "Library/NotionCache",
  "pages": [
    { "id": "2a87c2c6-cc02-80f9-a10a-f0b72b95fbb3", "path": "rules/code-guidelines.md",  "readers": ["code-guideline-check", "codex-implement", "AGENTS.md"] },
    { "id": "31c7c2c6-cc02-80f3-92c0-e6929d962476", "path": "rules/design-philosophy.md", "readers": ["code-guideline-check", "notion-spec-diff-check"] }
  ]
}
```

- `path` は英小文字とハイフンで書く。日本語のファイル名は、`git ls-files` で `core.quotepath=off` を付けないと取りこぼす（棚卸しで 57 件漏れた）ほか、シェル経由で文字化けしやすい。
- `readers` には、そのキャッシュを読むスキル・設定ファイルを書く。ページの削除や統合の影響範囲を、この欄だけで判断できるようにするため。
- 子ページもキャッシュする場合は、子ページを 1 件ずつ対応表に載せる（自動では辿らない）。

### 2.3 キャッシュファイルの先頭

生成したファイルの先頭には、必ず次のヘッダを付ける。

```markdown
<!--
NOTION CACHE — このファイルを編集しないこと。正本は Notion である。
source: https://www.notion.so/<page id>
page_id: <page id>
last_edited: 2026-09-22T08:00:00Z   (Notion 側の最終更新)
fetched: 2026-09-22T10:00:00Z       (取得日時)
update: ./SinfoniaOperator/NotionMarkdownExporter.exe --cache-map SinfoniaOperator/notion-cache-map.json
-->
```

### 2.4 更新の方法

決定どおり、**各自の手動実行**と、**スキルからの AI 実行**の 2 経路とする。CI での定期実行はしない。

| 経路 | いつ | コマンド（案） |
| --- | --- | --- |
| 手動 | 仕様書を読む・直す前、規約が変わったと聞いたとき | 全体ミラー: `./SinfoniaOperator/NotionMarkdownExporter.exe`（出力先は `Library/NotionSpecifications`）<br>ツール用キャッシュ: `./SinfoniaOperator/NotionMarkdownExporter.exe --cache-map SinfoniaOperator/notion-cache-map.json` |
| スキル（AI） | スキルがキャッシュを読む直前 | 同上のツール用キャッシュのコマンド |

スキルがキャッシュを読むときの手順（各スキルに共通で書く）:

1. キャッシュファイルが**無ければ**、ツール用キャッシュのコマンドを実行する。
2. **有っても**ヘッダの `fetched` が古ければ（目安 24 時間。D-17）、同じコマンドを実行する。エクスポーターは Notion の `last_edited` を比べて変わったページだけを取り直すので、変更が無ければすぐ終わる。
3. 実行に失敗したとき（`NOTION_TOKEN` が無い、ネットワークが無いなど）は、手元のキャッシュで続ける。その場合、**キャッシュの取得日時と、失敗したことをユーザーに伝える**。キャッシュも無ければ作業を止め、トークンの設定をユーザーに頼む。

この方式の代償:

- **各自のマシンに `NOTION_TOKEN` が要る**（`SinfoniaOperator/sinfonia-operator.secrets.json`）。トークンを持たないメンバーの AI はキャッシュを作れない。
- **GitHub 上で動くツールはキャッシュを読めない。** CodeRabbit（`.coderabbit.yaml`）、GitHub Actions、クラウドで動く AI セッションが該当する。これらには Notion の URL を示すか、読む対象から外す（T-6）。
- 仕様検索 Bot（Oracle VM）は `Docs/NotionSpecifications` を前提にしている（`SinfoniaOperator.SpecSearch/MarkdownChunker.cs:69`）。VM 側でエクスポーターを動かす構成に合わせて直す（T-5）。

### 2.5 エージェントのログ（`Docs/agent/`）

AI エージェントやスキルが書き出す記録は、`Docs/agent/` にまとめる（決定）。構成案:

| ディレクトリ（案） | 中身 | 現在の書き先 |
| --- | --- | --- |
| `Docs/agent/problem-logs/` | 不具合ログ（record-problem-log スキル） | `spec/plan/problem_logs/` |
| `Docs/agent/spec-diff/` | 仕様書と実装の差分レポート（notion-spec-diff-check スキル） | `Docs/` 直下 |
| `Docs/agent/qa-evidence/` | AI QA ツールの観測記録・証跡 | 未定（要確認） |
| `Docs/agent/sessions/` | セッションの作業ログ（必要な場合） | — |

`Docs/` は `.gitignore:122` で丸ごと除外されている。**ログを git で共有するかどうかは未決（D-16）** である。共有するなら `.gitignore` に `!Docs/agent/` を足す。共有しないなら各自のローカル記録になる。

ログは記録であって仕様ではない。仕様に関わる結論が出たら Notion へ書く。ログに書いたままにしない。

### 2.6 必要な実装（移行作業とは別タスク）

| # | 内容 | 備考 |
| --- | --- | --- |
| T-1 | 全体ミラーの出力先を `Library/NotionSpecifications` に変える | 変更箇所: `sinfonia-operator.env.json` の `NOTION_EXPORT_OUTPUT`、`NotionMarkdownExporter/ExporterOptions.cs:190,290`（既定値とヘルプ）、`NotionMarkdownWriter/WriterEnvironment.cs:110`、`SinfoniaOperator/SinfoniaOperator.cs:504,514`、両ツールの README、`.gitignore:96`（不要になる）。**受入条件: Unity を起動・再インポートしても `Library/NotionSpecifications` が残ること** |
| T-2 | NotionMarkdownExporter に、キャッシュマップに載ったページだけを取得し、固定パスへヘッダ付きで書き出すモードを足す（`--cache-map <path>`） | Enhanced Markdown API による変換はそのまま使う。子ページへのリンクは Notion の URL のまま残す |
| T-3 | キャッシュマップ `SinfoniaOperator/notion-cache-map.json` を作る | §3 の表から作る |
| T-4 | スキル・AI 指示の参照先をキャッシュパスへ書き換え、§2.4 の「読む前の手順」を入れる | 対象: `AGENTS.md`、code-guideline-check、codex-implement、notion-spec-diff-check、notion-spec-write、sinfonia-importers、ai-debug-qa、`.codex` のレビュースキル（`source-routing.md`）。全体ミラーを指している箇所も `Library/NotionSpecifications` に直す |
| T-5 | 仕様検索 Bot の参照パスを直す | `SinfoniaOperator.SpecSearch/MarkdownChunker.cs:69`、`deploy/oracle-vm-setup.md`（sparse-checkout 前提の手順は既に壊れている。D-09） |
| T-6 | GitHub 上で動くツールの扱いを決めて直す | `.coderabbit.yaml:7`（今も存在しない `Assets/Docs/AGENTS.md` を指している）には Notion の URL を書くか、参照を外す |
| T-7 | 書き先を `Docs/agent/` に変える | record-problem-log（LUDIARS 共通スキルなので、プロジェクト側で書き先を上書きできるか要確認）、notion-spec-diff-check の出力先 |
| T-8 | `scripts/notion/sync_module.py`・`split_module_doc.py` を廃止し、`notion-spec-write/references/module-docs.md` を「Notion を直接編集する」運用へ書き換える | repo → Notion の一方向同期を止める（D-01） |

---

## 3. ツール用キャッシュの対象（案）

[01_ドキュメント棚卸し.md](01_ドキュメント棚卸し.md) の 2-1・2-3 で、スキルやツールが読むと判定した文書を、Notion 正本へ移したあとの形にまとめた。
「キャッシュ化」は、Notion へ移して正本とし、リポジトリ（`Library/NotionCache/`）にはキャッシュだけを置くという意味である。

| 現在のリポジトリ文書 | Notion の正本（移行先） | キャッシュパス（案、`Library/NotionCache/` からの相対） | 読むもの |
| --- | --- | --- | --- |
| `Assets/Scripts/CodeGuidelines.md` と `Assets/Docs/ScriptsDocs/CodingConventions.txt`（重複のため統合） | システム概要 / コード規定 | `rules/code-guidelines.md` | code-guideline-check、codex-implement、`.codex` のレビュースキル、`AGENTS.md` |
| `Assets/Scripts/DesignPhilosophy.md` と `Assets/Docs/ScriptsDocs/Architecture.txt`（旧版のため統合） | システム概要 / 設計思想 | `rules/design-philosophy.md` | 同上、notion-spec-diff-check |
| `.github/PULL_REQUEST_TEMPLATE.md` の書き方の説明 | 既存「PR 作成ガイドライン」 | （キャッシュ不要） | テンプレート本体は GitHub の機能なのでリポジトリに残す |
| `Docs/QA/AI_QAツール対応表.md` | 新規: ワークフロー / QA / AI QA ツール対応表 | `qa/ai-qa-tool-map.md` | ai-debug-qa（3 スキル） |
| `Docs/QA/CBT_QAシート_2026-09-06.md`、`PC版_QAシート_2026-09-07.md` | 新規: ワークフロー / QA / 各シート | `qa/cbt-qa-sheet.md`、`qa/pc-qa-sheet.md` | AI QA ツール（行 ID `CBT:4-3` などを証跡キーに使う） |
| `Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/*.md` | 既存: システムリストの各ページ | （キャッシュ不要） | T-8 で同期の仕組みごと廃止する。AI が読むときは全体ミラーを使う |
| `Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/NotionDocsRules.txt` | 本書 §5 の規約とあわせて 仕様書 ガイド 配下へ | `rules/notion-writing-rules.md` | notion-spec-write |
| `Assets/Docs/PlannerGuide/MasterDataGuide.md` | 既存: マスターデータ運用ガイド（プランナー向け）。Notion 側が古いので、repo の 09-16 版で更新してから正本にする | （キャッシュ不要） | 現在パスで読むスキルは無い（プランナーと AI は全体ミラーで読む） |
| `Assets/Scripts/ScenarioCsvGuide.md` | 既存: シナリオコマンド使い方 / シナリオCSV～ドライブ同期～ | （キャッシュ不要） | 現在パスで読むスキルは無い |
| `spec/feature/game-spec.md` | 仕様概要の各ページ | （保留） | Augur 設定（`spec/domains/*.domain.json`）が見出しアンカー 22 本を参照している。Augur の参照先をキャッシュへ変えられるかを確かめてから決める |

**リポジトリに実体として残すもの**（キャッシュではない）: キャッシュを読み込む側の設定ファイルである。`AGENTS.md`、`.gemini/GEMINI.md`、`.claude/`・`.codex/`・`.agents/` のスキル、`.coderabbit.yaml`、キャッシュマップがこれにあたる。
これらは内容を持たず、キャッシュへの参照だけを持つ薄いファイルにする。
加えて、`README.md`・`third-party-notices.md`・フォントの `OFL.txt`・PR テンプレート・ゲームが読む CSV・Apps Script のコード・ツールの README も残す。

---

## 4. 参照の書き換え方

スキルや AI 指示の中で、リポジトリの文書をパスで指している箇所を、次の形に書き換える。

```markdown
コード規約: `Library/NotionCache/rules/code-guidelines.md`
（Notion「システム概要 / コード規定」のキャッシュ。git 管理外。
 無い・古い場合は `./SinfoniaOperator/NotionMarkdownExporter.exe --cache-map SinfoniaOperator/notion-cache-map.json` を実行してから読む）
```

- スキルの判定基準を、スキル本文の中に書き写さない。書き写すと、正本・キャッシュ・スキルの 3 か所に同じ内容ができる。
- 棚卸しで見つかった壊れた参照（`.coderabbit.yaml` → `Assets/Docs/AGENTS.md` など）は、この書き換えのときに一緒に直す。

---

## 5. Notion 記述規約（ツールが読む前提の書き方）案

既存の「仕様書 ライティング規則」（である調・句点・H1 背景色）に加えて、次の規約を定める。
**ツール用キャッシュの対象ページ（§3）には必須とする。** その他の仕様ページも、この規約に沿って書くと全体ミラーから AI が正しく読める。

### 5.1 ページの構成

| # | 規約 | 理由 |
| --- | --- | --- |
| W-01 | 1 ページに 1 つの主題だけを書く。キャッシュ対象のページには、別の主題を子ページとして混ぜない | キャッシュは 1 ページ単位で取るため |
| W-02 | ページの冒頭に「このページについて」の短い節を置き、次を書く: 何の正本か / 対象読者（人・AI・ツール名） / 最終確認日と、確認した実装のコミットまたはバージョン | 読んだ側が鮮度と適用範囲を判断できるようにするため |
| W-03 | 規則・仕様の本文と、背景・経緯・議論を分ける。背景は末尾の「背景」トグルに入れる | ツールが判定に使うのは本文だけである。経緯が本文に混ざると、過去の案を現行ルールと誤読する |
| W-04 | 体験版だけの値や TGS 限定の設定には、「体験版（TGS 2026）設定」と明記して通常の値と並べる | 今回の調査で、体験版の値と製品版の値が区別できない箇所が多かった |

### 5.2 使うブロック

| # | 規約 | 理由 |
| --- | --- | --- |
| W-05 | 使ってよいブロックは、見出し・段落・箇条書き・番号付きリスト・表（シンプルテーブル）・コード・引用・コールアウトとする | Markdown に変換しても意味が崩れないため |
| W-06 | 次のものに本文を入れない: 同期ブロック / リンクドデータベースビュー / 画像だけの説明 / カラム / 埋め込み | 変換で内容が落ちる、または参照先の内容が取れない |
| W-07 | 規則そのものをトグルの中に隠さない（トグルは背景・補足・参考画像に使う） | 人が読むときに見落とすため。AI にとっても、変換結果の構造が崩れやすい |
| W-08 | 重要な情報をメンション（@ページ・@人）だけで表さない。ページ名や役割名を文字で書く | 変換後にメンションが ID だけになることがある |

### 5.3 識別子と数値

| # | 規約 | 理由 |
| --- | --- | --- |
| W-09 | ツールや他の文書から参照される規則・項目には、**変えない ID** を振る（例: コード規約 `CG-012`、QA `CBT:4-3`、仕様条項 `BT-JUST-01`）。参照は見出し名ではなく ID で行う | 見出し名は書き換えられる。今は QA ツールの証跡キーと Augur のアンカーが見出しや行番号に依存していて、少し直しただけで壊れる |
| W-10 | ID を持つ項目は、表の 1 列目か見出しの先頭に ID を置く | ツールが ID を機械的に拾えるようにするため |
| W-11 | 数値パラメータは表で書く。列は「名前 / 値 / 単位 / 正本のアセットまたはコード」とする | 今回の差分の大半は数値のずれだった。値の置き場が分かれば、実装と突き合わせられる |
| W-12 | 正本がマスターデータ（ScriptableObject）にある値は、Notion には「現在値」として書き、正本のアセットパスを添える | 値の正本が 2 か所に分かれるのを防ぐため |

### 5.4 運用

| # | 規約 | 理由 |
| --- | --- | --- |
| W-13 | **API キー・トークン・パスワード・スプレッドシート ID などの秘密情報を書かない。** 置き場所（例: `sinfonia-operator.secrets.json`）だけを書く | 公開ページに API キーが平文で載っていた（SC-06、TL-21）。キャッシュに入ると各自のマシンにも複製される |
| W-14 | ページを削除・統合するときは、キャッシュマップ（`cache-map.json`）に載っていないかを先に確かめる | page id で結び付けているため、ページを消すとキャッシュの取得が失敗する |
| W-15 | 仕様を変える実装 PR では、同じ PR の中で Notion の該当ページも更新する。PR テンプレートに「Notion の更新先（URL）」の欄を足す | 実装が先に進んで仕様書が遅れる、という今回の問題の再発を防ぐため |
| W-16 | キャッシュファイルを直接編集しない。直したいときは Notion を直し、キャッシュを取り直す | 次の取得で上書きされて消えるため |
| W-17 | ツールがリポジトリへ書き出したログ（`Docs/agent/`）に仕様の結論を残したままにしない。結論は Notion の該当ページに書く | ログは正本ではないため（§2.5） |

---

## 6. 移行作業への影響

[00_移行作業手順書.md](00_移行作業手順書.md) は、この決定に合わせて次のように読み替える（手順書にも反映済み）。

- **段階 1（スナップショット）**: 全体ミラーの出力先は T-1 の後は `Library/NotionSpecifications/` になる。
- **段階 3（文書の移植）**: 01 で「リポジトリ残置を推奨」とした文書のうち、§3 の表に載っているものは「Notion へ移し、キャッシュ化する」に変わる。
- **段階 4（参照の付け替え）**: 参照先はキャッシュパスにする（§4）。T-2・T-3（キャッシュの生成）が先に要る。
- **段階 5（削除 PR）**: 削除するのは、Notion へ移した文書の実体である。`Docs/` フォルダはエージェントのログ置き場（`Docs/agent/`）として残る。
- **システムリスト**: 最後の repo → Notion 同期は行わない。repo の方が新しい 3 件（SourceDataProvider・Animation・NotionDocsRules）の差分だけを Notion へ手で反映し、以後は Notion を直接編集する（T-8）。
