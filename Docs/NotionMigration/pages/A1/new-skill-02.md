親ページ: 3c07c2c6-cc02-8091-850a-d86b29ff7a52（DB「KillChordデータ」。DB はルートの「スキル」35a7c2c6-cc02-805b-9ad4-cbed45b4ed04 の中にある）

# 2スキル

- page_id: （新規。DB「KillChordデータ」3c07c2c6-cc02-8091-850a-d86b29ff7a52 に行を追加する）
- Notion パス: Symphony Kill Chord / スキル / 2スキル
- スナップショットの最終更新: ―（新規。本文はルートの「スキル」ページ 2026-08-25T02:43:35.191Z の「スキル一覧」から移す）
- 書き込み許可: 不可（ルートの「スキル」配下。手作業で貼る）
- 処置: 新規作成（ルートの「スキル」ページ本文の「2スキル」節を移す）

## 判定

- 信頼性: ―（新規）。移す元の記述は一部古い。確認コミット 73fefeb45、最終確認日 2026-09-22
- 整合性:
  - 実装の値は `Assets/Level/Data/Master/Skill/Templates/SkillData02.asset` と `Assets/Scripts/Runtime/2.Application/Player/SkillEffect/Skill_02.cs` で確かめた
  - 値は既存の記述と実装が一致する
- 変更点:
  - ルートの「スキル」ページの「2スキル」節を、DB 行のテンプレート（1スキル・4スキルの行と同じ見出し）で新しい行に移した。既存の文は残した
  - 「スキル効果」節に現在値・再付与・通常攻撃のダメージ・成長・クールダウン・正本を追記した
  - 「演出詳細」節に実装済みの演出（プレハブ・アニメーションキー）を書いた
- 織り込んだ反映項目: BT-33, BT-35, BT-36
- 出典: `SkillData02.asset`（`_pattern`・`_skillType`・`_effectParameters`・`_effectParameterGrowths`・`_statusEffectReapplyPolicy`・`_skillNormalAttackDamagePolicy`・`_animationKey`・`_skillDetail`）、`Skill_02.cs`、`Assets/Level/Data/Master/InGame/Skill/Effect/SkillEffectCatalogConfig.asset`、反映項目 BT-33 の実値表
- 要確認:
  - スキル効果意図（企画）
  - 演出の流れと演出プレハブの中身（企画・実機確認。BT-35 は要実機確認）

## 適用する本文

（DB プロパティ）
- スキルジャンル: デバフ

### 機能詳細

#### コマンド
- 4 → 4 → 2

#### スキルジャンル
- デバフ

#### スキル効果
通常攻撃のショットガンの攻撃を発動し、
その攻撃に当たった対象に被ダメージ量25%増加のデバフを5秒間付与する。
- コマンドの最後の入力（2拍子のショットガン）の通常攻撃がそのまま出て、ダメージも出る
- 効果中の敵にもう一度付与した場合は、効果時間を延長する
- レベルが 1 上がるごとに被ダメージの増加率が 1.2 倍になる（上限 Lv10）
- クールダウンは 1 小節である
- 正本: `Assets/Level/Data/Master/Skill/Templates/SkillData02.asset`

#### スキル効果意図
【要確認: スキル効果意図を企画に】

#### モチーフ
2号をイメージしたスキル。ZeroDivideの戦術担当として、敵を制圧する戦術的技術からデバフスキルとなった。

### 演出詳細

#### 実装済みの演出
- プレハブ: `Assets/Level/Prefabs/Master/InGame/SkillEffect/SkillEffect_skill_02.prefab`（`SkillEffectCatalogConfig.asset` に登録）
- Timeline は無い
- アニメーションキー: `Skill_Left`
- 発動時に全スキル共通のカメラシェイク（0.3 秒）とポストエフェクトが入る
【要確認: 演出の流れ（場面ごとの内容）を企画に。プレハブの中身が2スキル用か、共通の仮プレハブかを実機で（2〜10 のプレハブはファイルサイズがほぼ同じ）】
