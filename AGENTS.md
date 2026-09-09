# コーディングに関する規則
- Assets/Scripts/DesignPhilosophy.md
- Assets/Scripts/CodeGuidelines.md
# 仕様に関するドキュメント
- Docs/NotionSpecifications

# 自律AIエージェントのブランチ・PR運用
- `feature/` で始まる作業ブランチは、同じ階層の `master` ブランチへセルフマージする。
- 例: `feature/demo/just-judgement/agent` → `feature/demo/just-judgement/master`。
- 作業ブランチから直接 `develop` にPRを作成せず、取り込み後の `master` から `develop` にPRを作成する。
- PR本文は `.github/PULL_REQUEST_TEMPLATE.md` の構成を使用し、確認済み・未確認を正確に記載する。
- テスト実行や `develop` へのマージ可否は、現在のセッションのユーザー指示に従う。
