# 敵・ボスのステータス

- page_id: 新規
- Notion パス: Symphony Kill Chord / 敵・ボスのステータス（DB「マスターデータ」に行を追加する）
- スナップショットの最終更新: —（新規）
- 書き込み許可: 不可（DB「マスターデータ」はルート直下で、許可範囲の外。手で貼る）
- 処置: 新規作成

## 判定

- 信頼性: 最新 — 2026-09-22 / 73fefeb45 の実データ（`Assets/Level/Data/**` の .asset と、ボスのプレハブ）を読んで書いた
- 整合性:
  - 仕様ページ（敵・ボスの仕様、他領域）からは数値を外し、このページを参照させる（決定 No.14）。ボスの HP 5000・三方向攻撃の左右 45° は実装値を正とする（決定 No.17）
  - 歩兵（`enemy_infantry_default`）と砲兵（`enemy_artillery_default`）は同じ `Enemy.asset`（guid 93524034675db3e489e93312c0366c23）を参照している（`EnemyDefinition_Infantry_Default.asset:20`、`EnemyDefinition_Artillery_Default.asset:20`）。移動・音楽・ミッションキーも 3 種の敵で同じアセットを参照する（`EnemyDefinition_*.asset:21-24`）
  - ボスのステータスはプレハブ `Assets/Level/Prefabs/Develop/Boss.prefab:592-596` が CharacterDefinitionRepository の ID `boss_character` と Addressables キー `ExampleEnemyMoveData` で引く。`BossCharacterData.asset` を直接参照するアセットは `CharacterDefinitionRepository.asset:17` だけである。Boss.prefab は `Assets/Level/Scenes/Master/Stages/Stage_01.unity` に置かれ、角度などの上書きは無い
  - 敵の攻撃仕様 `ExampleAttackParameter.asset:15-16` に `_criticalChance: 0.1` / `_criticalDamageMultiplier: 2` が残っているが、`AttackSpecAsset` は確定ダメージしか持たない（`AttackSpecAsset.cs:14`）ため読まれない旧データである。本文には書いていない
  - 初回攻撃の拍（`ExampleEncountMusicData.asset:17` `_targetBeat: 2`）は仕様（3 拍目）と違う（決定 No.15、実装の不具合）
- 変更点:
  - 新規ページ。敵の種類と参照アセット / キャラクターステータス / 攻撃 / 移動と射程 / 攻撃のタイミング / 砲弾 / ボスの攻撃 の 7 節を作った
  - 「攻撃」節: 表に「基礎ダメージ（全種類共通）10（コードの固定値）」の行と、マスターデータへ移す方針の 1 文を足した（R14）
  - 新節「キルコードの効果」をボスの攻撃の後ろに足した（R11）
- 決定（2026-09-22 八幡）: No.14 敵のステータスは仕様ページから外し、マスターデータ側に現状値を書く。このページを新設し、歩兵・砲兵が同じ `Enemy.asset` を参照することを「敵の種類と参照アセット」節に書いた
- 決定（2026-09-22 八幡）: No.17 ボスは実装値（HP 5000・三方向 45°）を正とする。「キャラクターステータス」「ボスの攻撃」節に値と正本を書いた
- 決定（2026-09-22 八幡）: No.15 初回攻撃は 3 拍目が正。「攻撃のタイミング」節に現状値（2）と本来の仕様を併記した
- 実装の修正が必要: 敵の初回攻撃の拍を 3 拍目にする（`Assets/Level/Data/Master/InGame/Battle/ExampleEncountMusicData.asset:17` `_targetBeat: 2`）
- 決定（2026-09-22 八幡）: R11 キルコードの効果の値はマスターデータに置き、仕様ページには効果の説明だけを書く。新節「キルコードの効果」に 12 キルコードの現在値（`SkillData*.asset` の `_effectParameters`）と成長の方式を書いた。A1 の各キルコードの原稿にあった値と照合し、すべて一致した
- 決定（2026-09-22 八幡）: R14 敵の攻撃ダメージの固定値 10 はマスターデータに移し、敵の種類・攻撃ごとに設定できるようにする。「攻撃」節の表に現状（コードの固定値 10）を足し、マスターデータへ移す方針（未実装）を書いた
- 実装の修正が必要: 敵の攻撃の基礎ダメージがコードの固定値 10 である。マスターデータに移し、敵の種類・攻撃ごとに設定できるようにする（Issue #2004、根拠 `Assets/Scripts/Runtime/2.Application/InGame/Enemy/EnemyAttackUsecase.cs:50`、`Assets/Scripts/Runtime/2.Application/InGame/Enemy/ShellAttackUsecase.cs:25`、`Assets/Scripts/Runtime/2.Application/InGame/Enemy/Boss/EnemyTripleShotAttackUsecase.cs:49`）
- 織り込んだ反映項目: なし（決定 No.14・15・17 による新設。R11・R14 で節を追加）
- 出典: マスターデータ `Assets/Level/Data/Master/Character/{Enemy,MarkedEnemy,CharacterDefinitionRepository}.asset`、`Assets/Level/Data/Develop/Boss/{BossCharacterData,BossAttackEntry1〜3,BossAttackEntryRepo}.asset`、`Assets/Level/Data/Master/InGame/Enemy/Definitions/*.asset`、`Assets/Level/Data/Master/InGame/Battle/{ExapleAttackDefinition,ExampleAttackParameter,ExampleAttackPipeline,ExampleEnemyMoveData,ExampleEncountMusicData,ExampleBattleMusicData}.asset`、`Assets/Level/Data/Master/InGame/Enemy/{ShellAttackData,ShellMusicData}.asset`（参照は各 .meta の guid で照合）。コード `CharacterDefinitionAsset.cs:44-58`、`ConfirmedDamage.cs:19-24`、`TripleShotRaycastDetectView.cs:116`、`BossAttackKind.cs:8-16`、`EnemyMoveView.cs:176`、`ShellLifeCycle.cs:37`。Addressables は全件 `GameData.Shared`（製品版・体験版で共通）
- 要確認:
  - 攻撃硬直時間の単位（秒か）。コード（`AttackInterval.cs`）に単位の記載が無い（エンジニア）
  - 小節フラグ・拍目の数え方（拍目が 1 始まりか）。決定 No.15 は 1 始まりとして扱っている（エンジニア）
  - ボスのデータが `Assets/Level/Data/Develop/Boss/` にある。製品版にも入る（GameData.Shared）ので、`Master/` へ移すか（エンジニア・企画）
  - 敵ごとの本来の値（企画の意図値）。このページは現状値だけを書く（企画）
  - R11 でキルコードの値もこのページに置いた。ページ名を「敵・ボスのステータス」のままにするか、キルコードを含む名前にするか、キルコードの値を別の行に分けるか（企画）
  - 敵の種類・攻撃ごとの基礎ダメージの値（R14。Issue #2004 で決める）（企画）

## 適用する本文

値は 2026-09-22 時点の実データである。仕様ページには数値を書かず、このページを参照する。製品版と体験版（TGS 2026）で同じデータを使う（Addressables の Group は `GameData.Shared`）。
データの作り方・変え方は「システム概要 / マスターデータ運用ガイド（プランナー向け）」の②③を参照する。

## 敵の種類と参照アセット
| 敵 | 処理種別 | プレハブ | キャラクターステータス定義 | 正本 |
|---|---|---|---|---|
| 歩兵（`enemy_infantry_default`） | `Infantry` | `EnemyInfantry.prefab` | `Enemy`（ID `enemy`） | `Assets/Level/Data/Master/InGame/Enemy/Definitions/EnemyDefinition_Infantry_Default.asset:18-20` |
| 砲兵（`enemy_artillery_default`） | `Artillery` | `EnemyArtillery.prefab` | `Enemy`（ID `enemy`） | `Assets/Level/Data/Master/InGame/Enemy/Definitions/EnemyDefinition_Artillery_Default.asset:18-20` |
| 砲兵（Marked）（`enemy_artillery_marked`） | `Artillery` | `EnemyArtillery.prefab` | `MarkedEnemy`（ID `Marked`） | `Assets/Level/Data/Master/InGame/Enemy/Definitions/EnemyDefinition_Artillery_Marked.asset:18-20` |
| ボス | ボス専用（`BossLifeCycle`） | `Assets/Level/Prefabs/Develop/Boss.prefab` | `BossCharacterData`（ID `boss_character`） | `Assets/Level/Prefabs/Develop/Boss.prefab:592-596` |
- 歩兵と砲兵は**同じ `Enemy`（`Assets/Level/Data/Master/Character/Enemy.asset`）を参照している。** HP などを変えると両方が変わる。分けるには `CharacterDefinitionAsset` を新しく作り、個別の敵データの「キャラクターステータス定義」を差し替える。
- 移動（`ExampleEnemyMoveData`）、初回攻撃・通常攻撃の音楽仕様、撃破時のミッションキーは、3 種の敵で同じアセットを参照している（`EnemyDefinition_*.asset:21-24`）。

## キャラクターステータス
| 名前 | 値 | 単位 | 正本のアセット |
|---|---|---|---|
| 歩兵・砲兵の最大体力 | 1000 | — | `Assets/Level/Data/Master/Character/Enemy.asset:20` |
| 歩兵・砲兵の基本ダメージ | 0 | — | `Assets/Level/Data/Master/Character/Enemy.asset:23` |
| 歩兵・砲兵の会心率 | 0 | 0〜1 | `Assets/Level/Data/Master/Character/Enemy.asset:24` |
| 歩兵・砲兵の攻撃硬直時間 | 0.5 | 【要確認: 単位（秒か）をエンジニアに】 | `Assets/Level/Data/Master/Character/Enemy.asset:19` |
| 砲兵（Marked）の最大体力 | 2500 | — | `Assets/Level/Data/Master/Character/MarkedEnemy.asset:20` |
| 砲兵（Marked）の基本ダメージ | 15 | — | `Assets/Level/Data/Master/Character/MarkedEnemy.asset:23` |
| 砲兵（Marked）の会心率 | 0 | 0〜1 | `Assets/Level/Data/Master/Character/MarkedEnemy.asset:24` |
| 砲兵（Marked）の攻撃硬直時間 | 0.5 | 【要確認: 同上】 | `Assets/Level/Data/Master/Character/MarkedEnemy.asset:19` |
| ボスの最大体力 | 5000 | — | `Assets/Level/Data/Develop/Boss/BossCharacterData.asset:20` |
| ボスの基本ダメージ | 0 | — | `Assets/Level/Data/Develop/Boss/BossCharacterData.asset:23` |
| ボスの会心率 | 0（アセットに項目が無く、既定値） | 0〜1 | `Assets/Scripts/Runtime/5.InfraStructure/InGame/Character/CharacterDefinitionAsset.cs:58` |
| ボスの攻撃硬直時間 | 0.5 | 【要確認: 同上】 | `Assets/Level/Data/Develop/Boss/BossCharacterData.asset:19` |

## 攻撃
3 種の敵とボスは、同じ攻撃定義 `ExapleAttackDefinition` を使う（`Enemy.asset:22`、`MarkedEnemy.asset:22`、`BossCharacterData.asset:22`）。
| 名前 | 値 | 単位 | 正本のアセット |
|---|---|---|---|
| 確定ダメージ | 5 | — | `Assets/Level/Data/Master/InGame/Battle/ExampleAttackParameter.asset:17` |
| 処理ステップ | `CriticalStep` → `ConfirmedDamage` | — | `Assets/Level/Data/Master/InGame/Battle/ExampleAttackPipeline.asset:15-25` |
| 基礎ダメージ（全種類共通） | 10 | — | コードの固定値（`Assets/Scripts/Runtime/2.Application/InGame/Enemy/EnemyAttackUsecase.cs:50`、`ShellAttackUsecase.cs:25`、`Boss/EnemyTripleShotAttackUsecase.cs:49`） |
| ジャストダメージ倍率 | 0 | 倍 | `Assets/Level/Data/Master/InGame/Battle/ExapleAttackDefinition.asset:18` |
- `ConfirmedDamage` は、ダメージが確定ダメージより小さいときに確定ダメージまで引き上げる（`ConfirmedDamage.cs:19-24`）。
- 敵の攻撃の基礎ダメージは、現状は全種類でコードの固定値 10 である。マスターデータに移し、敵の種類・攻撃ごとに設定できるようにする（未実装）。
- 武器ダメージ倍率・射程などの武器パラメータはアセットに設定が無く、処理ステップにも `WeaponDamageStep` / `OutOfRangeDamageStep` が入っていないため、敵の攻撃には使われない。

## 移動と射程
3 種の敵とボスは、同じ `ExampleEnemyMoveData` を使う（ボスは Addressables キーで参照する。`Boss.prefab:596`）。
| 名前 | 値 | 単位 | 正本のアセット |
|---|---|---|---|
| 移動速度 | 3 | m/秒（個体差の倍率が掛かる。`EnemyMoveView.cs:176`） | `Assets/Level/Data/Master/InGame/Battle/ExampleEnemyMoveData.asset:15` |
| 最小攻撃距離 | 7 | m | `Assets/Level/Data/Master/InGame/Battle/ExampleEnemyMoveData.asset:16` |
| 最大攻撃距離 | 12 | m | `Assets/Level/Data/Master/InGame/Battle/ExampleEnemyMoveData.asset:17` |

## 攻撃のタイミング
| 名前 | 小節フラグ | 拍子 | 拍目 | 正本のアセット |
|---|---|---|---|---|
| 敵の初回攻撃 | 1 | 4 | 2 | `Assets/Level/Data/Master/InGame/Battle/ExampleEncountMusicData.asset:15-17` |
| 敵の通常攻撃・ボスの全攻撃 | 2 | 4 | 3 | `Assets/Level/Data/Master/InGame/Battle/ExampleBattleMusicData.asset:15-17` |
| 砲弾の着弾 | 1 | 4 | 4 | `Assets/Level/Data/Master/InGame/Enemy/ShellMusicData.asset:15-17` |
- 敵の初回攻撃は、本来の仕様では 3 拍目である。現状は 2 になっており、実装の不具合として修正する（`ExampleEncountMusicData.asset:17`）。

## 砲弾
| 名前 | 値 | 単位 | 正本のアセット |
|---|---|---|---|
| 爆発半径 | 3 | m | `Assets/Level/Data/Master/InGame/Enemy/ShellAttackData.asset:15` |
- 砲兵とボスの砲弾は、`InGame` シーンの砲弾プレハブ（`ShellLifeCycle`）が Addressables キー `ShellAttackData` / `ShellMusicData` で読み込む。

## ボスの攻撃
ボスは `BossAttackEntryRepo` の 3 つの攻撃から選ぶ。どの攻撃も攻撃定義リストの 0 番目（`ExapleAttackDefinition`）を使う。
| 攻撃 | 種別 | 内容 | 正本のアセット |
|---|---|---|---|
| 通常攻撃 1 | `Infantry` | 直線・射線判定 | `Assets/Level/Data/Develop/Boss/BossAttackEntry1.asset:15-17` |
| 通常攻撃 2 | `Artillery` | 迫撃・円形範囲 | `Assets/Level/Data/Develop/Boss/BossAttackEntry2.asset:15-17` |
| 特殊攻撃 1 | `TripleShot` | 三方向直線 | `Assets/Level/Data/Develop/Boss/BossAttackEntry3.asset:15-17` |

| 名前 | 値 | 単位 | 正本のアセット |
|---|---|---|---|
| 三方向攻撃の左右の角度 | 45 | 度（正面から左右へ） | `Assets/Level/Prefabs/Develop/Boss.prefab:678`（既定値 `TripleShotRaycastDetectView.cs:116`） |
- ボスのデータは `Assets/Level/Data/Develop/Boss/` にあるが、`GameData.Shared` に登録されており製品版にも入る。【要確認: `Master/` へ移すかをエンジニア・企画に】

## キルコードの効果
キルコードの効果の説明は仕様ページ（仕様概要 / キルコード の各ページ）にあり、値はこの表を正とする。値は Lv0 のものである。正本はすべて `Assets/Level/Data/Master/Skill/Templates/SkillData<番号>.asset` の `_effectParameters`（行番号は各アセットの `_value` の行）。
| キルコード | 名前 | 値 | 単位 | 正本のアセット |
|---|---|---|---|---|
| skill_00 | 威力（DamageMultiplier） | 5 | 倍（攻撃力に対する。500%） | `SkillData00.asset:26` |
| skill_01 | 威力（DamageMultiplier） | 7.5 | 倍（攻撃力に対する。750%） | `SkillData01.asset:26` |
| skill_02 | 被ダメージの増加率（DamageTakenIncreaseRate） | 0.25 | 割合（25%） | `SkillData02.asset:26` |
| skill_02 | 効果時間（DurationSeconds） | 5 | 秒 | `SkillData02.asset:29` |
| skill_03 | 威力（DamageMultiplier） | 6 | 倍（攻撃力に対する。600%） | `SkillData03.asset:26` |
| skill_03 | ターゲット以外への威力（SecondaryDamageRate） | 0.5 | 割合（キルコードの威力に対する。50%） | `SkillData03.asset:29` |
| skill_04 | 過剰 HP の獲得率（BarrierGainRate） | 0.03 | 割合（与えたダメージに対する。3%） | `SkillData04.asset:26` |
| skill_04 | 効果時間（DurationSeconds） | 5 | 秒 | `SkillData04.asset:29` |
| skill_05 | HP の消費割合（HealthCostRatio） | 0.3 | 割合（最大 HP に対する。30%） | `SkillData05.asset:26` |
| skill_05 | 威力（DamageMultiplier） | 10 | 倍（消費 HP × 10 に対する。1000%） | `SkillData05.asset:29` |
| skill_06 | 回復率（LifeStealRate） | 0.01 | 割合（与えたダメージに対する。1%） | `SkillData06.asset:26` |
| skill_06 | 1 発の回復上限（HealPerHitCap） | 5 | HP | `SkillData06.asset:29` |
| skill_06 | 効果時間（DurationSeconds） | 5 | 秒 | `SkillData06.asset:32` |
| skill_07 | 攻撃回数（HitCount） | 3 | 回 | `SkillData07.asset:26` |
| skill_07 | 攻撃力の減少率（AttackPowerReductionRate） | 0.1 | 割合（対象の最大攻撃力に対する。10%） | `SkillData07.asset:29` |
| skill_07 | 敵一体の減少量の上限（AttackPowerReductionCap） | 10 | — | `SkillData07.asset:32` |
| skill_07 | 効果時間（DurationSeconds） | 10 | 秒 | `SkillData07.asset:35` |
| skill_08 | 伝染の範囲（InfectionRange） | 10 | m（半径） | `SkillData08.asset:26` |
| skill_08 | 伝染回数（InfectionTriggerCount） | 5 | 回 | `SkillData08.asset:29` |
| skill_08 | 伝染ダメージ（InfectionDamageRate） | 3 | 倍（攻撃を受けた敵の被ダメージに対する。300%） | `SkillData08.asset:32` |
| skill_09 | クリティカルダメージの倍率（CriticalMultiplier） | 5 | 倍 | `SkillData09.asset:26` |
| skill_09 | 効果時間（DurationSeconds） | 10 | 秒 | `SkillData09.asset:29` |
| skill_10 | 軽減率（DamageReductionRate） | 0.3 | 割合（30%） | `SkillData10.asset:26` |
| skill_10 | 軽減する回数（DamageReductionHitCount） | 3 | 回 | `SkillData10.asset:29` |
| skill_13 | 威力（DamageMultiplier） | 0.6 | 倍（攻撃力に対する。60%） | `SkillData13.asset:26` |
| skill_13 | 攻撃回数（HitCount） | 5 | 回 | `SkillData13.asset:29` |
| skill_13 | クリティカルダメージの倍率（CriticalMultiplier） | 2 | 倍 | `SkillData13.asset:32` |
| skill_13 | 攻撃の開始（HitDelaySeconds） | 0.8 | 秒（発動から） | `SkillData13.asset:35` |
| skill_13 | 攻撃の間隔（HitIntervalSeconds） | 0.06 | 秒 | `SkillData13.asset:38` |
- レベルによる成長は、各アセットの `_effectParameterGrowths` にある。12 キルコードとも 10 段階（上限 Lv10）で、1 段ごとに対象の値が 1.2 倍になる。
- 値の名前（DamageMultiplier など）は `Assets/Scripts/Runtime/1.Domain/InGame/Skill/SkillEffectParameterId.cs:8-27` の識別子である。
- キルコードの値のほかに、コード側の定数で決まっている値がある: skill_03 の当たる範囲（直線の左右の幅）1.5m、skill_07 の範囲の距離 12m（研究で加算）と角度（左右それぞれ）30 度（`Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillTargetResolver.cs:16-18`）、skill_09 のフィールドの半径（サブマシンガンの射程 20m、`PlayerAttack_Beat8_SubmachineGun.asset` の `_range`）。
