# コーディングに関する規則
- Assets/Scripts/DesignPhilosophy.md
- Assets/Scripts/CodeGuidelines.md
# 仕様に関するドキュメント
- Docs/NotionSpecifications
# 自律AIエージェントのブランチ・PR運用
- 編集・修正・コミットは必ず自分に指定された作業ブランチ（例: `feature/demo/just-judgement/agent`）で行う。レビュー対応も同じ作業ブランチで行う。
- ブランチ命名規則は `feature/○○/○○/agent` とする (例: `feature/demo/home-tutorial/agent`)。指定がない場合のみ `agent/○○` とする (例: `agent/cbt-qa-sheet`)。
- ブランチを作成したら、編集・コミットの前に空のままプッシュ (`git push -u origin <ブランチ名>`) してリモートにブランチを作成し、その後で作業を開始する。
  - 先にプッシュしておくと、`AutoCreateMasterBranch.yml` が作業前の内容で `master` を生成する。これにより、下記の「作業ブランチと `master` が同一コミットになる」例外ケースを避けられる。
- 同階層の `master` で直接編集・コミットしない。`master` の更新は作業ブランチからのセルフマージのみで行う。
- `feature/` で始まる作業ブランチは、同じ階層の `master` ブランチへセルフマージしてよい。マージ可否・タイミングは自由に判断してよい。
- `feature/` で始まらない作業ブランチは、Pull Request の作成先は常に `develop` とする。
- 例: `feature/demo/just-judgement/agent` → `feature/demo/just-judgement/master`。
- 作業ブランチから直接 `develop` にPRを作成せず、取り込み後の `master` から `develop` にPRを作成する。
  - ただし `feature/**/master` へのPRがマージされると、GitHub Actions (`AutoCreateDevelopPullRequest.yml`) が `master` → `develop` のドラフトPR (テンプレ構成込み) を自動作成する。そのため通常は手動で develop 向けPRを作る必要はない。
  - 例外: `AutoCreateMasterBranch.yml` は作業ブランチのpush時点の内容でそのまま `master` を自動生成するため、作業ブランチと `master` が同一コミットになり実際のマージが一度も発生しないケースがある。この場合は上記の自動化が発火しないため、develop向けPRは手動で作成する必要がある(この場合、`agent`→`master`のPRは「No commits between」で作成不可なので省略してよい)。
- PR本文は `.github/PULL_REQUEST_TEMPLATE.md` の構成を使用し、確認済み・未確認を正確に記載する(自動生成されるdevelop向けPRも同テンプレ構成)。
- テスト実行や `develop` へのマージ可否は、現在のセッションのユーザー指示に従う。
