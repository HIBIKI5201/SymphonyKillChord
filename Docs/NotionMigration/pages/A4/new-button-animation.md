# ボタンのアニメーションの実装

- page_id: 新規（DB「用語リスト」に行を追加）
- Notion パス: Symphony Kill Chord / **用語** / ボタンのアニメーションの実装
- スナップショットの最終更新: —（新規）
- 書き込み許可: 不可（NotionMarkdownWriter の許可範囲は 仕様概要 / システム概要 / システムリスト / ワークフロー / マスターアセットリスト の配下）
- 処置: 新規作成
- DB プロパティ: 種類 = 開発用語
- 最終確認日: 2026-09-22（確認したコミット 73fefeb45）

## 判定

- 信頼性: 最新 — 記載のクラス・ファイルは実在する（`Runtime/4.View/OutGame/Common/ButtonPulseAnimationManipulator.cs`、`ButtonAnimationExtensions.cs` の `EnableButtonPulseAnimation`、`Button.uss` の `.btn-scale-feedback`）
- 整合性: 同じ内容の移植先として、システム概要 / UI構築フロー (3167c2c6-cc02-80b0-93e3-f7432ae71f57) への追記案がある（OG-216）。用語 DB に置くか、UI 担当のシステム概要に置くかは片方に決める（両方に書かない）。拡縮の値は、`.btn-scale-feedback` の押下時 0.9 倍（`Button.uss:36-40`）と、OutGameNavigation.uss のコメントにある「選択中 1.1 倍 / 押下中 0.9 倍」がある
- 変更点:
  - 新規行を追加（ButtonTerminology.md の 2 語目を移植）
- 織り込んだ反映項目: RL-11, OG-216
- 出典: `Assets/Scripts/Runtime/4.View/OutGame/Common/ButtonTerminology.md`（2026-09-15）、USS `Assets/Level/Scenes/Develop/OutGameTest/ScreenTransitionTest/UI Toolkit/Uss/Button.uss`（`.btn-scale-feedback` :30-40）、`OutGameNavigation.uss:1-9`
- 要確認: 置き場所を用語 DB とシステム概要のどちらにするか（UI 担当。OG-216）

## 適用する本文

英名: btn-scale-feedback / ButtonPulseAnimationManipulator
ボタンの実装を指示するときの用語で、拡大・縮小の動き・タイミング・イージングだけを指す（画像には触れない）。対象は次の3つである。
- USS の .btn-scale-feedback クラス（押したときに0.9倍に縮む演出と、transition の基本設定）
- ButtonPulseAnimationManipulator（hover / focus 中に拡大・縮小を繰り返すパルス）
- EnableButtonPulseAnimation（パルスを要素に付ける拡張メソッド）

出典: Assets/Scripts/Runtime/4.View/OutGame/Common/ButtonTerminology.md
---
