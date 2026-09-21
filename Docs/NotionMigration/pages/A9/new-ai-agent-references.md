# AI エージェント向け参照先一覧

- page_id: （新規。親ページ: ワークフロー 2ef7c2c6-cc02-8001-b4f0-d69b499c64f0）
- Notion パス: Symphony Kill Chord / ワークフロー / AI エージェント向け参照先一覧（新規）
- スナップショットの最終更新: —（新規ページ）
- 書き込み許可: 可（親ページのワークフローが許可範囲の中）
- 処置: 新規作成

## 判定

- 信頼性: 新規 — Notion には、AI のスキルや設定がどの文書を読んでいるかの一覧が無い（RL-01）。本文は現行のスキル・設定ファイル（`AGENTS.md`、`.claude/skills/`、`.codex/skills/`、`.agents/skills/`、`.coderabbit.yaml`）を `git grep` で調べて書いた。今後の参照先は `02_キャッシュ運用とNotion記述規約.md` §0・§2・§3 の決定と案に沿った。確認したコミット 73fefeb45、最終確認日 2026-09-22
- 整合性:
  - A5 の新規原稿「Notion 仕様書の運用（正本とキャッシュ）」（`pages/A5/new-notion-spec-operation.md`）と役割を分けた。キャッシュの置き場・取り直しの手順・記録の置き場（`Docs/agent/`、不具合ログは Issue）はあちらが正本で、本ページは要点 1 行とリンクだけにした。本ページは「どのスキルが、どの文書を読むか」と「スクリプトの使い方」を受け持つ
  - A5 の社内ツールの原稿（3c87c2c6-cc02-815e-b882-f8e5182ea23f、節「AIエージェント向けの設定とスキル」）は、スキルの置き場と役割の概要である。本ページはその詳細として、同節からリンクする
  - RL-01 の反映先は「システム概要 / AI エージェント向け参照先一覧」だったが、依頼に従い親をワークフローにした（ワークフローには社内ツール・Notion 仕様書の運用が並ぶため、読み手が同じ）
  - 実装との矛盾: `.coderabbit.yaml:5-7` はレビュー規約に `Assets/Docs/AGENTS.md` を指定しているが、このファイルは存在しない（RL-03）。ほかの壊れた参照とあわせて「壊れている参照」に書いた
  - 実装との矛盾: `.claude/skills/notion-spec-write/references/module-docs.md:6-14` は「正本はリポジトリ側」と定めているが、2026-09-22 の決定（Notion が正本）と逆である（RL-02）。本文では「今後は廃止」と書いた
- 変更点:
  - 新規作成。ワークフロー配下の既存ページ（社内ツール・ビルドとリリース）と A5 の新規ページと同じく「このページについて」のコールアウト → 番号なしの節 → 表 にした
  - 節: 仕様書の参照先 / 規約・QA の参照先 / スクリプト / 壊れている参照 / リポジトリに残すもの
  - 「## ブランチ名」（新しい節。既存の節の後ろ）: 決定 No.6 のブランチ命名と、AI エージェントのブランチ名、CI の条件を足した
  - 表の「今後」の列は 02 の案（`Library/NotionCache/` のパスなど）であり、T-1〜T-4 の実装で確定する。本文にも「予定（案）」と書いた
- 織り込んだ反映項目: TL-24, RL-01, RL-02（スクリプトの扱いのみ）, RL-03, RL-17（リポジトリに残すもの）
- 出典:
  - 実装: `AGENTS.md:1-5`、`.claude/skills/code-guideline-check/SKILL.md:3,10-11,19,36`、`.claude/skills/codex-implement/SKILL.md:11,19,30-31,53-65,129-136`、`.claude/skills/notion-spec-diff-check/SKILL.md:3,9,11,27,39,76,123`、`.claude/skills/notion-spec-write/SKILL.md:8,49`、`.claude/skills/notion-spec-write/references/module-docs.md:6-40`、`references/writing-rules.md:4,17-30`、`.claude/skills/sinfonia-importers/SKILL.md:14,32`、`.claude/skills/ai-debug-qa/SKILL.md:7`、`.codex/skills/ai-debug-qa/SKILL.md:7`、`.codex/skills/symphony-kill-chord-code-review/SKILL.md:14-15`、`references/source-routing.md:5-7,13-14,20-21`、`.agents/skills/ai-debug-attack-queue/references/qa-coverage.md:3`、`.coderabbit.yaml:5-7`、`Assets/Docs/ScriptsDocs/CodingConventions.txt:112`、`Assets/Docs/README.md`（0 バイト）
  - スクリプト: `scripts/notion/sync_module.py:1-11`、`scripts/notion/split_module_doc.py:1-12,116`、`scripts/notion/apply_writing_rules.py:1-7,95-111`、`scripts/codex_runner.py:1-20,426-460`
  - 決定・案: `Docs/NotionMigration/02_キャッシュ運用とNotion記述規約.md` §0（2026-09-22 八幡）、§2.1・§2.5・§2.6（T-1〜T-8）・§3・§4、`90_未決事項.md` D-01・D-06・D-16
- 決定（2026-09-22 八幡）: No.6 ブランチ名から「レイヤー」をなくし、feature ブランチは `feature/[段階]/[プロダクト名]/[個人名]`（統合先 `feature/[段階]/[プロダクト名]/master`）とする。`agent` はルートに使わず個人名の位置に入れる（`feature/demo/<プロダクト名>/agent`）。今の `agent/…` は旧運用。`hotfix/…` は残す。「## ブランチ名」の節を新しく足した。CI の条件は `.github/workflows/AutoCreateMasterBranch.yml:24-47`・`ValidateFeaturePRTarget.yml:14,25-32`・`AutoCreateDevelopPullRequest.yml:7,11` を読んで書いた
- 実装の修正が必要: `AGENTS.md:8` の「指定がない場合のみ `agent/○○` とする (例: `agent/cbt-qa-sheet`)」を削除し、`feature/[段階]/[プロダクト名]/agent` に統一する（`AGENTS.md:7-8` の例はすでに新しい形式）
- 要確認:
  - designer/… と develop/…（`feature/designer/master` のような designer 系を含む）の扱い（八幡さんへ。決定 No.6 で未決）
  - `hotfix/tgs-<Issue 番号>` の統合先（develop へ直接 PR するか）。`AGENTS.md:11` の「`feature/` で始まらない作業ブランチは develop へ PR」から develop と読めるが、hotfix 専用の記述は無い（プログラマーへ）
  - 「今後」の列のキャッシュパス（`Library/NotionCache/rules/code-guidelines.md` など）は案である。T-3 のキャッシュマップで確定したら直す
  - `scripts/notion/sync_module.py`・`split_module_doc.py` を廃止する時期（T-8。RL-02 は要企画確認）。廃止までは今の手順で使ってよいか
  - `apply_writing_rules.py` は Notion 正本になっても使い道がある（手元の原稿の口調を揃える）。残すかどうか（要プログラマー確認）
  - CodeRabbit のレビュー規約に何を指定するか（Notion の URL を書くか、参照を外すか。T-6）
  - `spec/`（Anatomia / Augur の分析結果）と `Docs/G-Lab/` を残すか（RL-17、要企画確認）

## 適用する本文

## AIエージェント向け参照先一覧
> 💡 **このページについて**
AIエージェント（Claude Code・Codexなど）のスキルや設定が、どの文書を読んで動いているかの一覧である。Notionの仕様書やリポジトリの文書を移動・削除・改名するときは、先にこの一覧で、読んでいるスキルが無いかを確かめる。仕様書のキャッシュの取り直し方と、記録の置き場は、Notion 仕様書の運用のページにまとめている。
[[Notion 仕様書の運用（正本とキャッシュ）]]
[[社内ツール]]

## 仕様書の参照先
AIは、Notionの仕様書をリポジトリに書き出した写しを読む。仕様の正本はNotionである（2026-09-22決定）。
| 読むもの | 今の参照先 | 今後の参照先（予定） |
|---|---|---|
| `AGENTS.md`（すべてのAIエージェント） | `Docs/NotionSpecifications` | `Library/NotionSpecifications` |
| Claude Code: notion-spec-diff-check（仕様書と実装の差分調査） | `Docs/NotionSpecifications/` | `Library/NotionSpecifications/` |
| Claude Code: notion-spec-write（Notionへの書き込み） | `Docs/NotionSpecifications/`配下の`.md`をpullの対象に指定する | `Library/NotionSpecifications/` |
| Claude Code: sinfonia-importers（NotionMarkdownExporterの実行） | 書き出し先`Docs/NotionSpecifications` | 書き出し先`Library/NotionSpecifications` |
| Codex: symphony-kill-chord-code-review | `Docs/NotionSpecifications/Symphony Kill Chord/`（用語リストを含む） | `Library/NotionSpecifications/` |
- どちらの置き場もgitの対象外である。各自がNotionのトークンを使って書き出す
- 今後の参照先への切り替えは、書き出し先の変更と同時に行う

## 規約・QAの参照先
スキルが判定の根拠に使う文書である。今はリポジトリ内の文書を直接読んでいる。今後はNotionを正本とし、スキルはそのキャッシュを読む。
| 読むもの | 今の参照先（リポジトリ内） | 今後の正本（Notion） | 今後の参照先（予定） |
|---|---|---|---|
| `AGENTS.md`、`.gemini/GEMINI.md`（`AGENTS.md`経由） | `Assets/Scripts/DesignPhilosophy.md`、`Assets/Scripts/CodeGuidelines.md` | 設計思想、コード規定 | `Library/NotionCache/rules/design-philosophy.md`、`Library/NotionCache/rules/code-guidelines.md` |
| Claude Code: code-guideline-check（コード規約チェック） | 同上（チェック項目の根拠） | 同上 | 同上 |
| Claude Code: codex-implement（Codexへの実装委譲） | 同上（Codexへ渡す規約） | 同上 | 同上 |
| Claude Code: notion-spec-diff-check | `Assets/Scripts/DesignPhilosophy.md` | 設計思想 | `Library/NotionCache/rules/design-philosophy.md` |
| Codex: symphony-kill-chord-code-review | `DesignPhilosophy.md`、`CodeGuidelines.md`、`Assets/Docs/ScriptsDocs/Architecture.txt`、`CodingConventions.txt`、`Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/` | 設計思想、コード規定、システムリストの各ページ | 規約は`Library/NotionCache/rules/`、モジュールは`Library/NotionSpecifications/` |
| Claude Code・Codex: ai-debug-qa、`.agents`のai-debug-attack-queue | `Docs/QA/AI_QAツール対応表.md` | QA / AI QAツール対応表 | `Library/NotionCache/qa/ai-qa-tool-map.md` |
| Claude Code: notion-spec-write（モジュール文書） | `Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/<モジュール>.md`、`NotionDocsRules.txt` | システムリストの各ページ、モジュールドキュメントテンプレ | リポジトリ側の文書は廃止する。Notionを直接編集する |
| Claude Code: notion-spec-write（書き方） | `Docs/NotionSpecifications/`の仕様書 ライティング規則 | 仕様書 ライティング規則 | `Library/NotionCache/rules/notion-writing-rules.md` |
| CodeRabbit（`.coderabbit.yaml`） | `Assets/Docs/AGENTS.md`（存在しない） | コード規定、設計思想 | 【要確認: NotionのURLを書くか、参照を外すかをプログラマーに確認】 |
[[コード規定]]
[[設計思想]]
[[仕様書 ライティング規則]]
[[モジュールドキュメントテンプレ]]
- 今後の参照先のパスは案である。キャッシュの対応表（`SinfoniaOperator/notion-cache-map.json`、予定）で確定する
- CodeRabbit・GitHub Actions・クラウドで動くAIセッションは、各自のマシンのキャッシュを読めない
- Editorのツール（C#）とCIは、リポジトリ内の`.md`・`.txt`の文書を読んでいない

## スクリプト
リポジトリの`scripts/`にある、AIの作業用のスクリプトである。Pythonで動く。

### Codexの実行（codex_runner.py）
codex-implementスキルが、Codexに実装を任せるときに使う。実行の前にCodexの残りの利用枠を確かめ、少なければCodexを呼ばずに終わる。
```bash
python scripts/codex_runner.py "<プロンプト>" "<出力先パス>"
python scripts/codex_runner.py --prompt-file <プロンプトファイル> "<出力先パス>"
python scripts/codex_runner.py --check-only --json
```
| 終了コード | 意味 |
|---|---|
| 0 | Codexが実行され、出力先のファイルができた |
| 1 | エラー（Codexが見つからない、実行に失敗した、出力が無い） |
| 2 | 利用枠が足りないので実行しなかった。呼び出し側は自分で実装する |
- 利用枠の下限は既定で10%である（`--threshold`で変える）
- 既定のタイムアウトは1,800秒である（`--timeout`で変える）
- Codexの実行ファイルはPATHに無い。`codex_runner.py`がCodex Desktopに同梱されたものを探す。見つからないときだけ、環境変数`CODEX_BIN`に絶対パスを設定する
- Codexに渡す規約は、上の「規約・QAの参照先」のコード規定と設計思想である

### Notionのモジュール文書の同期（sync_module.py・split_module_doc.py）
リポジトリのモジュール文書（`Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/<モジュール>.md`）を、Notionのシステムリストのページへ反映する。
```bash
python scripts/notion/sync_module.py "<モジュール文書.md>" "<NotionのページIDか、書き出した仕様書の.mdパス>" "<作業ディレクトリ>"
```
- 処理の順番: Notionから本文を取る（pull） → 処理フローの節を切り出す（`split_module_doc.py`） → 無い子ページを作り、ある子ページを更新する → 本文全体を置き換える（`push --whole`）
- 子ページはタイトルで突き合わせる。**処理フローの見出しを変えると、別の子ページとして作られる。** 見出しを変えたときは、古い子ページをNotionで削除する
- 仕様の正本はNotionになった（2026-09-22決定）ため、このスクリプトは廃止する予定である。廃止の後は、システムリストのページをNotionで直接編集する【要確認: 廃止の時期をプログラマーのリードに確認】

### 口調の変換（apply_writing_rules.py）
Markdownの文書を、仕様書 ライティング規則の口調（である調、リストと括弧内は句点なし）に揃える。ファイルをその場で書き換える。
```bash
python scripts/notion/apply_writing_rules.py <対象.md> [<対象.md> ...]
```
- 機械的な変換なので、変換の後は必ず目で確かめる（例：「反映します」が「反映す」になる）
- H1の背景色は、このスクリプトでは付かない。Notionに書き込むときに付ける

## 壊れている参照
次の参照は、参照先のファイルが存在しない。参照先を移す作業のときに、一緒に直す。
| 参照元 | 参照先（存在しない） |
|---|---|
| `.coderabbit.yaml` | `Assets/Docs/AGENTS.md`（CodeRabbitは規約を読まないままレビューしている） |
| Claude Code: notion-spec-diff-check | 前例として挙げる`Docs/仕様書と実装の差分分析_2026-07-27.md`（あるのは`_2026-08-23.md`）、`Docs/Missionステップ用バフ・デバフ付与機構_計画書.md`、`SinfoniaOperator/sinfonia-operator.settings.json` |
| `Assets/Docs/ScriptsDocs/CodingConventions.txt` | `ArchitectureValidationReport.txt` |
- `Assets/Docs/README.md`は中身が空である

## リポジトリに残すもの
リポジトリの文書をNotionへ移した後も、次のものはリポジトリに実体として残す。
- AIが読む設定とスキル: `AGENTS.md`、`.gemini/GEMINI.md`、`.claude/`・`.codex/`・`.agents/`のスキル、`.coderabbit.yaml`。中身は持たせず、Notionのキャッシュへの参照だけを書く
- キャッシュの対応表（`SinfoniaOperator/notion-cache-map.json`、予定）
- `scripts/`のスクリプト（廃止するものを除く）
- `README.md`、`third-party-notices.md`、フォントの`OFL.txt`、PRテンプレート、ゲームが読むCSV、Apps Scriptのコード、ツールのREADME
【要確認: `spec/`（解析結果）と`Docs/G-Lab/`を残すかを企画に確認】

## ブランチ名
AIエージェントも人と同じ命名でブランチを作る。ブランチ名に「レイヤー」は入れない。
- featureブランチ: `feature/[段階]/[プロダクト名]/[個人名]`。段階は`demo`・`beta`・`research`・`release`など
- 統合先: 同じ階層の`feature/[段階]/[プロダクト名]/master`
- AIエージェントの作業ブランチ: 個人名の位置に`agent`を入れる（例：`feature/demo/<プロダクト名>/agent`）。`agent`をブランチの先頭には使わない
- 今の`agent/…`形式（例：`agent/cbt-qa-sheet`）は旧運用である。`AGENTS.md`の記述も新しい形式に直す
- 不具合の修正: `hotfix/…`は今のまま使う。TGSのIssueを直すときは`hotfix/tgs-<Issue番号>`とする
- `designer/…`と`develop/…`の扱いは未定である【要確認: designer/… と develop/… の扱いを八幡さんに】
- 過去のブランチの記録（例：`feature/research/boss/miyamoto`）の名前は変えない

CIは次のように動く。`…/agent`もfeatureブランチと同じ扱いになる。
- `feature/`で始まり、4階層以上で、末尾が`master`でないブランチを作ると、同じ階層の`master`ブランチを自動で作る（`AutoCreateMasterBranch.yml`）
- 同じ条件のブランチからのPRは、作成先が同じ階層の`master`かを検査する（`ValidateFeaturePRTarget.yml`）
- `feature/**/master`へのPRがマージされると、`master`から`develop`へのドラフトPRを自動で作る（`AutoCreateDevelopPullRequest.yml`）
