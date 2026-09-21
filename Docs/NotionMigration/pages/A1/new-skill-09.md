親ページ: 3c07c2c6-cc02-8091-850a-d86b29ff7a52（DB「KillChordデータ」。DB はルートの「スキル」35a7c2c6-cc02-805b-9ad4-cbed45b4ed04 の中にある）

# 9スキル

- page_id: （新規。DB「KillChordデータ」3c07c2c6-cc02-8091-850a-d86b29ff7a52 に行を追加する）
- Notion パス: Symphony Kill Chord / スキル / 9スキル
- スナップショットの最終更新: ―（新規。本文はルートの「スキル」ページ 2026-08-25T02:43:35.191Z の「スキル一覧」から移す）
- 書き込み許可: 不可（ルートの「スキル」配下。手作業で貼る）
- 処置: 新規作成（ルートの「スキル」ページ本文の「9スキル」節を移す）

## 判定

- 信頼性: ―（新規）。移す元の記述は一部古い。確認コミット 73fefeb45、最終確認日 2026-09-22
- 整合性:
  - 実装の値は `Assets/Level/Data/Master/Skill/Templates/SkillData09.asset` と `Assets/Scripts/Runtime/2.Application/Player/SkillEffect/Skill_09.cs` で確かめた
  - 「スキル効果」: 旧: 「半径xm」「n秒間」「x倍」 → 新: 半径 20m（サブマシンガンの射程）・10 秒・5 倍
- 変更点:
  - ルートの「スキル」ページの「9スキル」節を、DB 行のテンプレート（1スキル・4スキルの行と同じ見出し）で新しい行に移した。既存の文は残した
  - 「スキル効果」節に現在値・再付与・通常攻撃のダメージ・成長・クールダウン・正本を追記した
  - 「演出詳細」節に実装済みの演出（プレハブ・アニメーションキー）を書いた
- 織り込んだ反映項目: BT-33, BT-35, BT-36
- 出典: `SkillData09.asset`（`_pattern`・`_skillType`・`_effectParameters`・`_effectParameterGrowths`・`_statusEffectReapplyPolicy`・`_skillNormalAttackDamagePolicy`・`_animationKey`・`_skillDetail`）、`Skill_09.cs`、`Assets/Level/Data/Master/InGame/Skill/Effect/SkillEffectCatalogConfig.asset`、反映項目 BT-33 の実値表
- 要確認:
  - スキル効果意図（企画）
  - 演出の流れと演出プレハブの中身（企画・実機確認。BT-35 は要実機確認）
  - フィールドはプレイヤーを中心に展開し、プレイヤーと一緒に動くか（本文「展開」の意味。実機確認）

## 適用する本文

（DB プロパティ）
- スキルジャンル: デバフ

### 機能詳細

#### コマンド
- 2 → 4 → 8 → 8

#### スキルジャンル
- デバフ

#### スキル効果
プレイヤーを中心として半径20m(サブマシンガンの射程と等しい)
のフィールドを展開し10秒間展開する。
[header_4] フィールド効果
フィールド内に存在する敵が攻撃に被弾した場合
攻撃がクリティカルだった場合、クリティカルダメージの倍率が5倍になる。
- 効果中にもう一度発動した場合は、効果時間を延長する
- コマンドの最後の入力（8拍子のサブマシンガン）の通常攻撃がそのまま出て、ダメージも出る
- レベルが 1 上がるごとにクリティカルダメージの倍率が 1.2 倍になる（上限 Lv10）
- クールダウンは 1 小節である
【要確認: フィールドがプレイヤーと一緒に動くか（その場に残るか）を実機で】
- 正本: `Assets/Level/Data/Master/Skill/Templates/SkillData09.asset`

| 名前 | 現在値（Lv0） | 単位 | 正本 |
| --- | --- | --- | --- |
| 効果時間 | 10 | 秒 | `SkillData09.asset` の DurationSeconds |
| クリティカルダメージの倍率 | 5 | 倍 | `SkillData09.asset` の CriticalMultiplier |
| フィールドの半径 | 20 | m | サブマシンガンの射程（`PlayerAttack_Beat8_SubmachineGun.asset` `_range`、`Skill_09.cs:35`） |

#### スキル効果意図
【要確認: スキル効果意図を企画に】

#### モチーフ
9号をイメージしたスキル。潜入・諜報能力から敵を内側から瓦解させる能力からデバフスキルとなった。
9番目のセフィラであるイェソドの守護天使 ガブリエルからメッセンジャーとしての役から敵の弱点を突くスキルとなった。

### 演出詳細

#### 実装済みの演出
- プレハブ: `Assets/Level/Prefabs/Master/InGame/SkillEffect/SkillEffect_skill_09.prefab`（`SkillEffectCatalogConfig.asset` に登録）
- Timeline は無い
- アニメーションキー: 空欄（通常攻撃のモーションで代用している）
- 発動時に全スキル共通のカメラシェイク（0.3 秒）とポストエフェクトが入る
【要確認: 演出の流れ（場面ごとの内容）を企画に。プレハブの中身が9スキル用か、共通の仮プレハブかを実機で（2〜10 のプレハブはファイルサイズがほぼ同じ）】
