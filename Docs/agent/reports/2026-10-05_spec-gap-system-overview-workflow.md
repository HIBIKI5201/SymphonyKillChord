# システム概要・ワークフローの書き足りない部分の精査

調査日: 2026-10-05 / 対象: Notion「システム概要」「ワークフロー」配下 / 照合先: `.github/workflows/`・`.claude/`・`.gitmodules`・`Assets/Editor`

方法: Notionのミラーを読み、実装（Actions定義・フック・サブモジュール設定・スキル）と突き合わせた。
ページの本文はいずれも`pull`で取り直し、ミラーと最終更新日時が一致すること（＝ミラーが陳腐化していないこと）を確かめた。

## 結論

2026-09-30に仕様書のミラーをサブモジュール化し、PRの行き先検証を作り替えた（#2325・`0eb4d48b2`）。
SinfoniaOperatorのページはこれに追従したが、**ワークフロー側とGitHub運用規定が旧運用のまま**残っている。
旧運用の記述は、読んだ人を誤った手順（手でエクスポーターを回す・存在しないワークフロー名）へ導く。

## 反映する項目（今回Notionへ書く）

| # | ページ | 食い違い | 根拠 |
| --- | --- | --- | --- |
| 1 | GitHub 運用規定 | `ValidateFeaturePRTarget`（コミットステータス`feature-pr-target`）は存在しない。実体は`ValidatePRTarget.yml`（`pr-target`）で、全PRの向き先を検証する。`ValidateDevelopPR`・`ValidateHotfixPR`（`develop-pr-rule`）、`NotionSpecificationsSync`、`SinfoniaOperator`日次、`DeploySinfoniaOperator`の記述が自動化の節に無い | `.github/workflows/ValidatePRTarget.yml` ほか |
| 2 | ビルドとリリース | 「その他の自動処理」の表が同じく`ValidateFeaturePRTarget`を挙げ、`ValidatePRTarget`・`ValidateDevelopPR`・`ValidateHotfixPR`・`NotionSpecificationsSync`が無い | 同上 |
| 3 | Notion 仕様書の運用 | 「各自が手で取り直す」「GitHub Actionsなどで定期的に取り直すことはしない」「全体の写しは管理しない」とあるが、実際は3時間ごとのActionsが非公開リポジトリへpushし、本体はサブモジュールで取り込む。起動時に自動pullされる。AIの記録`Docs/agent/`を「gitの対象外」とするが、`AGENTS.md`は「コミットする」と定め、実際に追跡されている | `NotionSpecificationsSync.yml`、`.gitmodules`、`.claude/settings.json`、`NotionSpecificationsAutoPull.cs`、`AGENTS.md`、`git ls-files Docs/agent` |
| 4 | 社内ツール | 「書き出された仕様書の扱い」が`Library/`へ移す予定のままで、サブモジュール化・自動同期に触れない。NotionMarkdownExporterの行と「初回は1,800ページ以上」の警告が、手動実行を前提にしている。AIのスキル一覧に`skc-feature-flow`・`skc-issue-report`・`skc-architecture-audit`が無い | 同上、`.claude/skills/` |
| 5 | AIエージェント向け参照先一覧 | 「ブランチ名」節が`ValidateFeaturePRTarget.yml`を挙げる（今回はここだけ直す）。仕様書の参照先の表は「今後`Library/`」の予定を載せるが、サブモジュールに確定した。表の書き直しはスキルごとの参照先の確認が要るため未着手 | `ValidatePRTarget.yml`、`.gitmodules` |

## 反映しない項目（要判断・規模が大きい）

- **システムリストに独立ページが無いモジュール**: `Assets/Scripts/Runtime`の層に実装があるのに、モジュール名を題にしたページが無い。
  Haptics、Voice、Localization、PostEffect、Environment、Load（ロード画面）、Audio（OutGame/Persistent）、Navigation。
  他ページ内の言及はあるが（例: Localizationは6ページ）、モジュール文書の節構成（概要・依存関係・Initializer・処理フロー）では書かれていない。
  1モジュールずつ実装を読む作業になるため、別Issueにして進める。
- `ValidateDevelopPR`が必須とするPRテンプレのチェック項目と、GitHub運用規定の「PR作成ガイドライン」の関係。
  運用規定には「適用範囲を要確認」の注記が残っている。CIは**develop向けの全PR（Draft除く）**に必須チェックを課しているため、
  注記の問いの答えが実装で出ている。ただし方針の決定はリードプログラマーの判断である。今回は事実だけを書き、注記は残した。
- `.coderabbit.yaml`の壊れた参照（`Assets/Docs/AGENTS.md`）は参照先一覧に既出。未解決のまま。

## 書き込みの方針

- 5ページとも部分更新（`push`、`--whole`なし）。書き換えは該当箇所に絞った。
- 事実はワークフロー定義とコードから取り、コミットメッセージから推測しない。
- 日付が絡む決定（2026-09-30のサブモジュール化）は、PR・コミットの日付を使う。

## 反映結果（2026-10-05）

5ページとも`push --confirm`で反映し、送信後の再取得で内容の一致を確認した。
GitHub 運用規定は、追記した3項目（同期・日次通知・配備）が初回の送信で落ちたため再送した。
直前の文脈に`mention-page`を含む位置は置換の一意化に失敗するため、「自動化」節の末尾へ置いた（CodeRabbitの前ではない）。
