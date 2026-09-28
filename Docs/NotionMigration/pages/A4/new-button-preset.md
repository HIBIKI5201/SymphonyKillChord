# ボタンの実装（一式）

- page_id: 新規（DB「用語リスト」に行を追加）
- Notion パス: Symphony Kill Chord / **用語** / ボタンの実装（一式）
- スナップショットの最終更新: —（新規）
- 書き込み許可: 不可（NotionMarkdownWriter の許可範囲は 仕様概要 / システム概要 / システムリスト / ワークフロー / マスターアセットリスト の配下）
- 処置: 新規作成
- DB プロパティ: 種類 = 開発用語
- 最終確認日: 2026-09-22（確認したコミット 73fefeb45）

## 判定

- 信頼性: 最新 — `ButtonPresetExtensions.ApplyBasicButtonPreset` と `UINavigationExtensions` の `MakeNavigable` / `RegisterActivation` は実在する（`Runtime/4.View/OutGame/Common/ButtonPresetExtensions.cs`、`Runtime/4.View/OutGame/Navigation/UINavigationExtensions.cs`）
- 整合性: 同じ内容の移植先として、システム概要 / UI構築フロー (3167c2c6-cc02-80b0-93e3-f7432ae71f57) への追記案がある（OG-216）。用語 DB に置くか、UI 担当のシステム概要に置くかは片方に決める（両方に書かない）
- 変更点:
  - 新規行を追加（ButtonTerminology.md の 3 語目と使い方の例を移植）
- 織り込んだ反映項目: RL-11, OG-216
- 出典: `Assets/Scripts/Runtime/4.View/OutGame/Common/ButtonTerminology.md`（2026-09-15）、USS `Assets/Level/Scenes/Develop/OutGameTest/ScreenTransitionTest/UI Toolkit/Uss/Button.uss`
- 要確認: 置き場所を用語 DB とシステム概要のどちらにするか（UI 担当。OG-216）

## 適用する本文

英名: ApplyBasicButtonPreset
ボタンの実装を指示するときの用語で、見た目・アニメーション・クリックやフォーカスの挙動をまとめた、完成品のボタン一式を指す。ButtonPresetExtensions.ApplyBasicButtonPreset を1回呼ぶと、見た目とアニメーションの USS クラスの付与、フォーカス / クリックへの対応（MakeNavigable / RegisterActivation）、パルスアニメーションの付与をまとめて行う。
見た目だけ、アニメーションだけを使うときは、UXML で class="btn-skin" や class="btn-scale-feedback" を個別に付け、必要なら EnableButtonPulseAnimation だけを呼ぶ。
出典: Assets/Scripts/Runtime/4.View/OutGame/Common/ButtonTerminology.md
---
