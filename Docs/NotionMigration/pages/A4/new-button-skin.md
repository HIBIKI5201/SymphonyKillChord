# ボタンの見た目の実装

- page_id: 新規（DB「用語リスト」に行を追加）
- Notion パス: Symphony Kill Chord / **用語** / ボタンの見た目の実装
- スナップショットの最終更新: —（新規）
- 書き込み許可: 不可（NotionMarkdownWriter の許可範囲は 仕様概要 / システム概要 / システムリスト / ワークフロー / マスターアセットリスト の配下）
- 処置: 新規作成
- DB プロパティ: 種類 = 開発用語
- 最終確認日: 2026-09-22（確認したコミット 73fefeb45）

## 判定

- 信頼性: 一部古い（元文書） — ButtonTerminology.md は「`.btn-skin` がデフォルトサイズ（200×100px）も持つ」とするが、実際の `Button.uss` では `.btn-skin` は背景画像の切り替えだけで、200×100px は `.button` クラスが持つ
- 整合性: 同じ内容の移植先として、システム概要 / UI構築フロー (3167c2c6-cc02-80b0-93e3-f7432ae71f57) への追記案がある（OG-216）。用語 DB に置くか、UI 担当のシステム概要に置くかは片方に決める（両方に書かない）
- 変更点:
  - 新規行を追加（ButtonTerminology.md の 1 語目を移植し、サイズの誤りを直した）
- 織り込んだ反映項目: RL-11, OG-216
- 出典: `Assets/Scripts/Runtime/4.View/OutGame/Common/ButtonTerminology.md`（2026-09-15）、USS `Assets/Level/Scenes/Develop/OutGameTest/ScreenTransitionTest/UI Toolkit/Uss/Button.uss`（`.btn-skin` :4-14、`.button` :42-44）、画像 `Assets/Arts/Images/Sprites/UI/OutGame/UI_Button_off.png`・`UI_Button_on.png`
- 要確認: 置き場所を用語 DB とシステム概要のどちらにするか（UI 担当。OG-216）

## 適用する本文

英名: btn-skin
ボタンの実装を指示するときの用語で、画像・色など静的な見た目だけを指す（動きは含まない）。USS の .btn-skin クラスで、通常時は UI_Button_off.png、hover / focus / active 時は UI_Button_on.png に背景画像を切り替える。
※大きさは .btn-skin では決めない。標準の 200×100px は .button クラスが持ち、個別に変えるときは UXML の inline style（width / height）で上書きする。
出典: Assets/Scripts/Runtime/4.View/OutGame/Common/ButtonTerminology.md、Button.uss
---
