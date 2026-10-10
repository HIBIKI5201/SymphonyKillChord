# 作業の入口
- 現在のユーザー指示を優先する。既存の未コミット変更は保持し、今回の対象パスだけをstageする。
- まず対象・完了条件・必要な検証を決める。資料の選び方は [Docs/agent/README.md](Docs/agent/README.md#必要な資料だけを読む) を参照する。
- ファイル名探索は対象フォルダ内の `rg --files -g`、本文探索は `rg -n` で絞る。リポジトリ全体・仕様ミラー・過去レポートの全列挙や全文読込を避け、出力が切れたら範囲を狭める。

# コーディング規則と仕様
- C#変更・レビュー時: [設計思想](Assets/Scripts/DesignPhilosophy.md)、[コード規約](Assets/Scripts/CodeGuidelines.md)、設計思想から参照する対象レイヤーのクラス責務を読む。実装例は必要時だけ読む。
- 機能仕様は `Docs/NotionSpecifications` の対象機能・用語だけを読む。`NotionSpecifications_old` は現在の仕様の根拠にしない。

# エージェントの出力
- 解析・監査・調査・引継ぎは `Docs/agent/` に置いてコミットする。構成・命名は [README](Docs/agent/README.md) に従う。構成外の出力フォルダは作らない。

# ブランチ・PR
- 編集・コミットは指定された作業ブランチで行う。指定がなければ `feature/[段階]/[作業名]/agent`。新規ブランチは編集前に空pushし、同階層masterの生成を確認する。
- masterを直接編集・コミットしない。feature作業ブランチ→同階層masterへPR・セルフマージし、master→developのDraft PRを確認する。feature以外の作業ブランチのPR先はdevelop。
- 詳細・例外は [運用規則](.agents/skills/skc-feature-flow/references/branch-rules.md)。feature-flow依頼時は [共通スキル](.agents/skills/skc-feature-flow/SKILL.md) を使う。
- PRは `.github/PULL_REQUEST_TEMPLATE.md` に従い確認済み・未確認を正確に記す。テスト実行・developへのマージ可否は現在のユーザー指示に従う。
