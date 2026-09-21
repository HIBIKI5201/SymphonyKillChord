# スロット拡張

- page_id: 3a37c2c6-cc02-80ef-801f-c7fadeb7c3cc
- Notion パス: Symphony Kill Chord / **用語** / スロット拡張
- スナップショットの最終更新: 2026-07-20T08:54:58.598Z
- 書き込み許可: 不可（NotionMarkdownWriter の許可範囲は 仕様概要 / システム概要 / システムリスト / ワークフロー / マスターアセットリスト の配下）
- 処置: 本文置き換え
- DB プロパティ: 種類 = ゲームルール（現状空欄）
- 最終確認日: 2026-09-22（確認したコミット 73fefeb45）

## 判定

- 信頼性: 一部古い — 2→3 は実装と一致する（`Runtime/1.Domain/OutGame/SkillBuild/SkillBuildDefinition.cs:30` `INITIAL_SLOT_COUNT = 2`、拡張は `HasSkillSlotBonus` のノード数）。ただし拡張ノード N7 のデータにフラグが無く、実際には増えない可能性がある（OG-110）
- 整合性: 仕様概要 / 研究画面 (3437c2c6-cc02-80d1-8adc-d63d047f4199) の解放条件「バフ・デバフ1,2と攻撃２を取得した時」とノードの親子（N7 の親は N113）の関係は未照合
- 変更点:
  - 定義文に「初期の編成枠は 2。拡張ノード（N7、コスト 20）を解放すると 1 枠増える」を追記
  - ※注記を追加: N7 のデータに拡張フラグが無い件（要実機確認）
  - 出典: 既存の「仕様概要/仕様リスト/スキル.md、研究画面.md」は残し、データ Assets/Level/Data/Master/OutGame/SkillTree/SkillNodeData/SkillNodeData-N7.asset を追加
- 織り込んだ反映項目: OG-110（用語側の反映）
- 出典: 実装 `SkillBuildDefinition.cs:30`、`Runtime/2.Application/OutGame/SkillTree/SkillTreeService.cs:251-270`、マスターデータ `SkillNodeData-N7.asset`（`UnlockCost: 20`、`_hasSkillSlotBonus` なし）
- 要確認: N7 解放で編成枠が増えるか（実機確認。増えなければデータ修正タスクにする）

## 適用する本文

英名: SkillSlotExpansion
キルコード編成可能数を2→3個に増やす、研究画面上の特別なノード解放要素。初期の編成枠は2で、スロット拡張ノード（N7、必要ポイント20）を解放すると1枠増える。
※現行データの N7 には拡張の指定（スキル編成枠を1つ増やすフラグ）が入っていない。【要確認: N7 解放で実際に枠が増えるか（実機）】
出典: 仕様概要/仕様リスト/スキル.md、研究画面.md、データ Assets/Level/Data/Master/OutGame/SkillTree/SkillNodeData/SkillNodeData-N7.asset
---
