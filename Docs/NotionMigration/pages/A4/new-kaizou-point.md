# 改造ポイント

- page_id: 新規（DB「用語リスト」に行を追加）
- Notion パス: Symphony Kill Chord / **用語** / 改造ポイント
- スナップショットの最終更新: —（新規）
- 書き込み許可: 不可（NotionMarkdownWriter の許可範囲は 仕様概要 / システム概要 / システムリスト / ワークフロー / マスターアセットリスト の配下）
- 処置: 新規作成
- DB プロパティ: 種類 = ゲームルール
- 最終確認日: 2026-09-22（確認したコミット 73fefeb45）

## 判定

- 信頼性: 最新 — マスターデータにリソースとして実装済み（`Assets/Level/Data/Master/OutGame/Resource/GameResource_SkillLevelupPoint.asset`、DataID `SkillLevelupPoint`、表示名「改造ポイント」）。用語 DB に行が無い
- 整合性: 用語「解放ポイント」（旧タイトル「解放ポイント／研究ポイント」、3a37c2c6-cc02-807f-a613-f63277c00e44）と対になるリソース。管理用アセットリスト / 所持ポイント（改造ポイント）(0180) にだけ出てくる
- 変更点:
  - 新規行を追加
- 織り込んだ反映項目: OG-105, OG-117, OG-104
- 出典: マスターデータ（上記、`_icon` 未設定・説明文は空）、実装 `Runtime/5.InfraStructure/OutGame/SkillBuild/SkillBuildRepository.cs:109-160`（1 回 1 ポイント）、`Runtime/2.Application/Persistent/Savedata/StageProgressSaveDataService.cs:148-199`（報酬）、30_ OG-104 / OG-105 / OG-117
- 要確認: リソースのアイコンと説明文が未設定（アセット・企画。OG-105）。レベルアップのコストを 1 固定とするか（企画。OG-117）
- 決定（2026-09-22 八幡）: No.1 定義文の「スキルをレベルアップする」を「キルコードをレベルアップする」に直した

## 適用する本文

英名: SkillLevelupPoint
改造画面でキルコードをレベルアップするときに使うポイント。レベルを1つ上げるごとに1ポイント消費する。ステージのクリア報酬（シナリオの読了を含む）として得る。画面では「改造ポイント」「改造P」と表記する。
出典: ステータスリスト / 管理用アセットリスト / 所持ポイント（改造ポイント）、仕様概要 / 改造画面、データ Assets/Level/Data/Master/OutGame/Resource/GameResource_SkillLevelupPoint.asset
---
