# Docs/agent の収録一覧

[README](README.md) の構成に沿って置いた主な成果物と、旧置き場から移したものの一覧。調査・引継ぎで既存の資料を探すときだけ読む。

## 収録済み

| パス | 内容 |
| --- | --- |
| `analysis/omnipotens/2026-07-18_symphony-kill-chord/` | Omnipotens 最終解析（アーキテクチャレビュー・仕様起点ドメインモデル・UX レビュー）。旧 `Docs/G-Lab/` |
| `analysis/anatomia-augur/2026-09-06_symphony-kill-chord/` | Anatomia / Augur の解析結果とドメイン突き合わせ・QA テストケース。旧 `spec/analysis/2026-09-06/`（Issue #2081） |
| `analysis/runtime-audit/2026-09-17_runtime-scripts/` | ランタイムコード監査（購読解除漏れ・GC・設計原則など 18 観点）。旧 `Docs/RuntimeAudit/`（Issue #2081） |
| `reports/2026-09-28_event-exhibition-meeting.md` | 今後のイベント出展・コンテスト応募の検討会議の議事録（候補の規約調査と決定） |
| `reports/2026-09-29_homepage-news-meeting.md` | ホームページのお知らせ追加（TGS出展・ファミ通掲載）の議事録（Issue #2321 / #2322、タスク） |
| `reports/2026-09-24_spec-implementation-diff.md` | Notion仕様書と `Assets/Scripts/Runtime` 実装の全体差分レポート（10ドメイン・実質18件、根拠の file:line つき） |
| `handoffs/2026-10-05_spec-implementation-diff-notion-update.md` | 上記差分を受けた Notion 仕様書更新の引継ぎ（16ページ分の追記・修正内容と再開手順） |
| `handoffs/2026-10-05_notion-spec-overview-cleanup.md` | Notion「仕様概要（仕様リスト）」の整理の引継ぎ（設定画面のパイロットで決めた書き方と残りの作業） |
| `handoffs/2026-10-05_spec-guide-reinforcement.md` | Notion「仕様書 ガイド」全ページの補強兼要約の引継ぎ（決定事項・作成済みの貼り付け用本文・残りの作業） |
| `handoffs/2026-10-21_store-release-preparation.md` | Google Play・Steam のストア公開準備の引継ぎ（申告・仕様書・素材・アップロード鍵の状況、10/31 に出せるストアページ、クローズドテストまでの手順、道具の注意点） |
| `reports/2026-10-01_design-review-summary.md` | 全体設計レビューの総括（10 観点の評価・観点をまたぐ問題・優先順位・限界。Issue #2358〜#2363・#2415〜#2418） |
| `reports/2026-10-01_design-review-google-drive.md` | 全体設計レビュー: Google Drive の構成・命名・所有権・共有権限・ID 依存（Issue #2358） |
| `reports/2026-10-01_design-review-notion-spec.md` | 全体設計レビュー: Notion 仕様書の情報設計・規則の競合・仕様とコードの差（Issue #2359） |
| `reports/2026-10-01_design-review-game.md` | 全体設計レビュー: コアループ・メタループ・チュートリアル・スコープ（Issue #2360） |
| `reports/2026-10-01_design-review-code.md` | 全体設計レビュー: コード・アーキテクチャ（設計思想への準拠・モジュール境界・テスト）（Issue #2361） |
| `reports/2026-10-01_design-review-ops.md` | 全体設計レビュー: ブランチ・PR・CI・ドキュメント・引継ぎの運用（Issue #2362） |
| `reports/2026-10-01_design-review-harness.md` | 全体設計レビュー: AI エージェントの運用基盤（指示・スキル・道具・検証・記憶）（Issue #2363） |
| `reports/2026-10-01_design-review-asset-pipeline.md` | 全体設計レビュー: Drive → Assets → Addressables のアセットパイプラインと命名（Issue #2415） |
| `reports/2026-10-14_director-decisions.md` | ディレクター判断 21 件の決定一覧（判定の正本・報酬・チュートリアル・用語・運用・公開条件・Drive） |
| `reports/2026-10-16_lead-programmer-decisions.md` | リードプログラマー判断の決定一覧（開発用処理の除外・DataID 検証・固定 ID・保存互換・モジュール境界・CI・Codex・rc タグ・性能） |
| `reports/2026-10-01_design-review-ui-ux.md` | 全体設計レビュー: 画面遷移・入力・オンボーディング・アクセシビリティ（Issue #2416） |
| `reports/2026-10-01_design-review-qa.md` | 全体設計レビュー: 自動テスト・QA シート・再検証・リリース判定（Issue #2417） |
| `reports/2026-10-01_design-review-release.md` | 全体設計レビュー: ビルド・署名・ストア・性能・権利・セーブ互換（Issue #2418） |
| `reports/2026-10-27_tutorial-kill-step-deadlock.md` | チュートリアルの敵撃破ステップで敵が1体足りず進めなくなる不具合の原因調査（無敵バフを確定ダメージが貫通） |
| `reports/2026-11-03_spec-query-eval.md` | 仕様書の窓口（spec_query）と全文検索を評価セット40問で比べた結果（#2396 段階3） |
| `handoffs/2026-11-03_logic-graph-debugger-plan.md` | ゲームロジックをグラフ表示するデバッグウィンドウの実装計画（Unity 6.x の Graph Toolkit・GraphView の比較と決めてほしいこと） |
| `handoffs/2026-11-09_background-vertex-color-bake.md` | 背景の頂点カラーをテクスチャへベイクしLOD対応する企画（現行シェーダーの頂点カラー用途・ベイクツールの実現性・未確認点） |
| `reports/2026-11-08_harness-context-budget.md` | ハーネスのコンテキスト・トークン消費の棚卸しと削減（読み込みタイミング・`/context` の実測・スキル選択の確認・Codex 委任プロンプト。Issue #2753） |

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

