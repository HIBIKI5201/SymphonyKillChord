親ページ: 3c07c2c6-cc02-8091-850a-d86b29ff7a52（DB「KillChordデータ」。DB はルートの「スキル」35a7c2c6-cc02-805b-9ad4-cbed45b4ed04 の中にある）

# 5スキル

- page_id: （新規。DB「KillChordデータ」3c07c2c6-cc02-8091-850a-d86b29ff7a52 に行を追加する）
- Notion パス: Symphony Kill Chord / スキル / 5スキル
- スナップショットの最終更新: ―（新規。本文はルートの「スキル」ページ 2026-08-25T02:43:35.191Z の「スキル一覧」から移す）
- 書き込み許可: 不可（ルートの「スキル」配下。手作業で貼る）
- 処置: 新規作成（ルートの「スキル」ページ本文の「5スキル」節を移す）

## 判定

- 信頼性: ―（新規）。移す元の記述は一部古い。確認コミット 73fefeb45、最終確認日 2026-09-22
- 整合性:
  - 実装の値は `Assets/Level/Data/Master/Skill/Templates/SkillData05.asset` と `Assets/Scripts/Runtime/2.Application/Player/SkillEffect/Skill_05.cs` で確かめた
  - 旧: 「n%消費」「消費量×m」 → 新: 最大 HP の 30%、消費 HP × 10 × 1000%
- 変更点:
  - ルートの「スキル」ページの「5スキル」節を、DB 行のテンプレート（1スキル・4スキルの行と同じ見出し）で新しい行に移した。既存の文は残した
  - 「スキル効果」節に現在値・再付与・通常攻撃のダメージ・成長・クールダウン・正本を追記した
  - 「演出詳細」節に実装済みの演出（プレハブ・アニメーションキー）を書いた
- 織り込んだ反映項目: BT-33, BT-35, BT-36
- 出典: `SkillData05.asset`（`_pattern`・`_skillType`・`_effectParameters`・`_effectParameterGrowths`・`_statusEffectReapplyPolicy`・`_skillNormalAttackDamagePolicy`・`_animationKey`・`_skillDetail`）、`Skill_05.cs`、`Assets/Level/Data/Master/InGame/Skill/Effect/SkillEffectCatalogConfig.asset`、反映項目 BT-33 の実値表
- 要確認:
  - スキル効果意図（企画）
  - 演出の流れと演出プレハブの中身（企画・実機確認。BT-35 は要実機確認）
  - 桁合わせ後の計算を追認するか（企画）

## 適用する本文

（DB プロパティ）
- スキルジャンル: 攻撃

### 機能詳細

#### コマンド
- 4 → 2 → 8 → 8

#### スキルジャンル
- 攻撃

#### スキル効果
自身の体力を30%消費して、その消費量×10×1000％のダメージを与える。
- 消費する体力は最大 HP に対する割合である
- HP は最低 1 残る。消費できる HP が少ないときは、実際に消費した HP でダメージを計算する
- 攻撃力の補正と武器倍率はかけない
- コマンドの最後の入力の通常攻撃のダメージは出さない（この攻撃に置き換える）
- レベルが 1 上がるごとに威力が 1.2 倍になる（上限 Lv10）
- クールダウンは 1 小節である
※2026-09-17 に戦闘の数値の桁を合わせた（敵の HP とプレイヤーの攻撃力を 10 倍にした）ため、消費した HP を 10 倍してからダメージにする
【要確認: 桁合わせ後の計算（消費 HP × 10）を仕様として追認するかを企画に】
- 正本: `Assets/Level/Data/Master/Skill/Templates/SkillData05.asset`

| 名前 | 現在値（Lv0） | 単位 | 正本 |
| --- | --- | --- | --- |
| HP の消費割合 | 30 | %（最大 HP に対する割合） | `SkillData05.asset` の HealthCostRatio |
| 威力 | 1000 | %（消費 HP × 10 に対する割合） | `SkillData05.asset` の DamageMultiplier |
| 残る HP の下限 | 1 | ― | `CharacterEntity.ConsumeHealth` の `minimumHealth` |

#### スキル効果意図
【要確認: スキル効果意図を企画に】

#### モチーフ
5号をイメージしたスキル。前線を担うポイントマンとして、前線を切り崩す攻撃スキルとなった。
5番目のセフィラであるゲブラーの守護天使 カマエルから、最も攻撃的なスキルとなった。

### 演出詳細

#### 実装済みの演出
- プレハブ: `Assets/Level/Prefabs/Master/InGame/SkillEffect/SkillEffect_skill_05.prefab`（`SkillEffectCatalogConfig.asset` に登録）
- Timeline は無い
- アニメーションキー: 空欄（通常攻撃のモーションで代用している）
- 発動時に全スキル共通のカメラシェイク（0.3 秒）とポストエフェクトが入る
【要確認: 演出の流れ（場面ごとの内容）を企画に。プレハブの中身が5スキル用か、共通の仮プレハブかを実機で（2〜10 のプレハブはファイルサイズがほぼ同じ）】
