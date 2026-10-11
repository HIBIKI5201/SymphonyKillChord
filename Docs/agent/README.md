# Docs/agent

自律AIエージェントが生成するファイル（解析記録・監査結果・調査レポート・引継ぎ資料）の置き場。
ここは git の追跡対象で、生成物は下記の構成に沿って置き、コミットする。

## 必要な資料だけを読む

| 作業 | 最初に読む | 必要になったときに追加 |
| --- | --- | --- |
| C#実装・差分レビュー | `Assets/Scripts/DesignPhilosophy.md`、`CodeGuidelines.md` | `DesignClassRoles.md` の対象レイヤー、対象型・呼び出し元、対象機能の仕様・用語。`DesignExamples.md` は実装例が必要なときだけ |
| 全体アーキテクチャ監査 | 上記と `skc-architecture-audit` | モジュール単位でコードを読む。調査済み・未調査を区別する |
| feature-flow | `.agents/skills/skc-feature-flow/SKILL.md` | 作業開始・PR・復旧の各段階で指定されたreferenceだけ |
| Unity操作・QA | `.agents/skills/` の該当スキル | JSON観測を先に使い、画像・ログ・階層は必要な範囲だけ取得する |
| 資料・CI・運用の修正 | 対象ファイルとその参照元 | C#・Unity挙動を変更しなければUnity起動・全規約・全仕様読込は不要 |
| 調査・引継ぎ | このREADMEの配置規則 | 対象テーマの既存レポートだけ。過去の全レポートを読まない |

- 探索はファイル名 → 見出し・一致行 → 該当節の順に進める。`Docs/NotionSpecifications` はまず機能に対応するサブフォルダを絞る。大量一致は `rg -l`、必要な一致行は `rg -n -m` を使う（`-m` はファイルごとの上限）。
- 同じセッションで読んだ同じ版の資料は再読しない。編集・ブランチ変更・同期があれば対象資料を再確認する。
- ログや階層は件数・期間・ルートを絞る。出力が切れたら上限を増やす前に探索範囲を狭める。
- 外部サービスの読取・更新は依頼に必要なときだけ。報告用の巨大な生ログはコミットしない。
- 引継ぎには目的、現在のブランチ・PR、変更したパス、検証結果、未解決点、次の操作だけを残す。会話・全ログ・全文仕様の転載はしない。

## ハーネスの保守

- Codex共通スキルは `.agents/skills/`、Codex専用は `.codex/skills/`。同名を両方に置かない。Claudeの独自入口は `.claude/skills/` に置き、共有可能な手順は共通本体へリンクする。
- 現在のセッションで削除前のスキル一覧が残っている場合は `.agents/skills/<name>/SKILL.md` を参照する。次のセッションで一覧の重複解消を確認する。
- 正本の規則をスキル本文へ転載しない。短い入口に対象・必須条件・条件付きの参照先を置き、モード固有の詳細をreferencesへ分ける。
- `python scripts/harness/check_harness.py` で入口の文字数上限、リンク、スキル名の再重複を確認する。CIでも同じチェックを実行する。数字はトークン実測ではなくUTF-8本文の文字数である。

## ディレクトリ構成

```
Docs/agent/
├── README.md                              … このファイル（構成と命名規則）
├── analysis/                              … 解析ツールが一括生成する成果物
│   └── <tool>/
│       └── <YYYY-MM-DD>_<target>/         … 1回の解析実行 = 1フォルダ
│           ├── report/                    … 閲覧用（HTML など）
│           └── spec/                      … 機械可読の中間成果物（md / json など）
├── reports/                               … 単発の調査・監査レポート
│   └── <YYYY-MM-DD>_<topic>.md
└── handoffs/                              … 作業の引継ぎ資料・作業計画
    └── <YYYY-MM-DD>_<topic>.md
```

| フォルダ | 置くもの | 置かないもの |
| --- | --- | --- |
| `analysis/` | 解析ツール（Omnipotens など）の1回分の出力一式 | 手書きのメモ、単発の調査結果 |
| `reports/` | 1テーマの調査・監査・比較の結果（1ファイルで完結するもの） | 複数ファイルにまたがるツール出力 |
| `handoffs/` | 次の担当（人・エージェント）に渡す引継ぎ資料、作業計画 | 確定した仕様（仕様は `Docs/NotionSpecifications` / Notion が正本） |

## 命名規則

- 日付は作成日を `YYYY-MM-DD` で先頭に付ける（一覧で時系列に並ぶように）。
- `<tool>` `<target>` `<topic>` は英小文字のケバブケース（例: `omnipotens`, `symphony-kill-chord`, `save-data-audit`）。
- 同じテーマを更新する場合は新しい日付で別フォルダ・別ファイルを作り、古いものは消さない（差分を追えるようにする）。

## 置かないもの

- 秘密情報（認証情報、Drive の ID やリンク、メンバーのメールアドレスなど）。公開リポジトリのため。
- 再生成できる大きな生ログ・キャッシュ。必要な部分だけをレポートに要約して残す。
- Unity プロジェクトから参照されるアセット（`Assets/` 配下に置く）。

## 収録済み

| パス | 内容 |
| --- | --- |
| `analysis/omnipotens/2026-07-18_symphony-kill-chord/` | Omnipotens 最終解析（アーキテクチャレビュー・仕様起点ドメインモデル・UX レビュー）。旧 `Docs/G-Lab/` |
| `analysis/anatomia-augur/2026-09-06_symphony-kill-chord/` | Anatomia / Augur の解析結果とドメイン突き合わせ・QA テストケース。旧 `spec/analysis/2026-09-06/`（Issue #2081） |
| `analysis/runtime-audit/2026-09-17_runtime-scripts/` | ランタイムコード監査（購読解除漏れ・GC・設計原則など 18 観点）。旧 `Docs/RuntimeAudit/`（Issue #2081） |
| `reports/2026-09-28_event-exhibition-meeting.md` | 今後のイベント出展・コンテスト応募の検討会議の議事録（候補の規約調査と決定） |
| `reports/2026-09-29_homepage-news-meeting.md` | ホームページのお知らせ追加（TGS出展・ファミ通掲載）の議事録（Issue #2321 / #2322、タスク） |
| `reports/2026-10-01_design-review-summary.md` | 全体設計レビューの総括（10 観点の評価・観点をまたぐ問題・優先順位・限界。Issue #2358〜#2363・#2415〜#2418） |
| `reports/2026-10-01_design-review-google-drive.md` | 全体設計レビュー: Google Drive の構成・命名・所有権・共有権限・ID 依存（Issue #2358） |
| `reports/2026-10-01_design-review-notion-spec.md` | 全体設計レビュー: Notion 仕様書の情報設計・規則の競合・仕様とコードの差（Issue #2359） |
| `reports/2026-10-01_design-review-game.md` | 全体設計レビュー: コアループ・メタループ・チュートリアル・スコープ（Issue #2360） |
| `reports/2026-10-01_design-review-code.md` | 全体設計レビュー: コード・アーキテクチャ（設計思想への準拠・モジュール境界・テスト）（Issue #2361） |
| `reports/2026-10-01_design-review-ops.md` | 全体設計レビュー: ブランチ・PR・CI・ドキュメント・引継ぎの運用（Issue #2362） |
| `reports/2026-10-01_design-review-harness.md` | 全体設計レビュー: AI エージェントの運用基盤（指示・スキル・道具・検証・記憶）（Issue #2363） |
| `reports/2026-10-01_design-review-asset-pipeline.md` | 全体設計レビュー: Drive → Assets → Addressables のアセットパイプラインと命名（Issue #2415） |
| `reports/2026-10-01_design-review-ui-ux.md` | 全体設計レビュー: 画面遷移・入力・オンボーディング・アクセシビリティ（Issue #2416） |
| `reports/2026-10-01_design-review-qa.md` | 全体設計レビュー: 自動テスト・QA シート・再検証・リリース判定（Issue #2417） |
| `reports/2026-10-01_design-review-release.md` | 全体設計レビュー: ビルド・署名・ストア・性能・権利・セーブ互換（Issue #2418） |

## 旧置き場から移したもの（Issue #2081）

命名規則より前に作られたため、ファイル名は元のまま（日本語名・日付が末尾）にしている。

| 旧パス | 新パス |
| --- | --- |
| `Docs/AI資料まとめ/*.md` | `reports/`（同名ファイル） |
| `Docs/AI資料まとめ/仕様更新ドラフト_2026-08-03/` | `handoffs/仕様更新ドラフト_2026-08-03/` |
| `Docs/` 直下の調査・差分分析・発注書・権利確認一覧（5 件） | `reports/`（同名ファイル） |
| `Docs/` 直下の引継ぎ資料・計画・Notion 下書き（9 件） | `handoffs/`（同名ファイル） |
| `Docs/アセット名リスト_命名規則提案.md`・`Docs/用語集_追加候補.md` | `Docs/AI資料まとめ/` の同内容ファイルと重複していたため削除し、`reports/` 側に一本化 |
| `Docs/RuntimeAudit/` | `analysis/runtime-audit/2026-09-17_runtime-scripts/report/` |
| `spec/analysis/2026-09-06/` | `analysis/anatomia-augur/2026-09-06_symphony-kill-chord/spec/` |
