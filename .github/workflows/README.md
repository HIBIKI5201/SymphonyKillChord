# ワークフロー名の接頭辞

`name:` の先頭に、役割ごとに次の接頭辞を付ける（2026-10-20 決定）。

| 接頭辞 | 対象 | 例 |
| --- | --- | --- |
| `[Git]` | ブランチ運用ルールの検証と、ブランチ・PR の自動作成。コードの中身は見ない | Validate PR Target、AutoCreateMasterBranch |
| `[CI/CD]` | ビルド・テスト・デプロイ。PR でコードや資料の中身を検査するチェックもここに入れる | Unity Build and Release、Roslyn Analyzers、Agent Harness |
| `[Bot]` | 定期実行の通知・同期 | Notion Specifications Sync |
