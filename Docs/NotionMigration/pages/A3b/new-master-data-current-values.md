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
- 決定（2026-09-22 八幡）: No.14 敵のステータスは仕様ページから外し、マスターデータ側に現状値を書く。このページを新設し、歩兵・砲兵が同じ `Enemy.asset` を参照することを「敵の種類と参照アセット」節に書いた
- 決定（2026-09-22 八幡）: No.17 ボスは実装値（HP 5000・三方向 45°）を正とする。「キャラクターステータス」「ボスの攻撃」節に値と正本を書いた
- 決定（2026-09-22 八幡）: No.15 初回攻撃は 3 拍目が正。「攻撃のタイミング」節に現状値（2）と本来の仕様を併記した
- 実装の修正が必要: 敵の初回攻撃の拍を 3 拍目にする（`Assets/Level/Data/Master/InGame/Battle/ExampleEncountMusicData.asset:17` `_targetBeat: 2`）
- 織り込んだ反映項目: なし（決定 No.14・15・17 による新設）
- 出典: マスターデータ `Assets/Level/Data/Master/Character/{Enemy,MarkedEnemy,CharacterDefinitionRepository}.asset`、`Assets/Level/Data/Develop/Boss/{BossCharacterData,BossAttackEntry1〜3,BossAttackEntryRepo}.asset`、`Assets/Level/Data/Master/InGame/Enemy/Definitions/*.asset`、`Assets/Level/Data/Master/InGame/Battle/{ExapleAttackDefinition,ExampleAttackParameter,ExampleAttackPipeline,ExampleEnemyMoveData,ExampleEncountMusicData,ExampleBattleMusicData}.asset`、`Assets/Level/Data/Master/InGame/Enemy/{ShellAttackData,ShellMusicData}.asset`（参照は各 .meta の guid で照合）。コード `CharacterDefinitionAsset.cs:44-58`、`ConfirmedDamage.cs:19-24`、`TripleShotRaycastDetectView.cs:116`、`BossAttackKind.cs:8-16`、`EnemyMoveView.cs:176`、`ShellLifeCycle.cs:37`。Addressables は全件 `GameData.Shared`（製品版・体験版で共通）
- 要確認:
  - 攻撃硬直時間の単位（秒か）。コード（`AttackInterval.cs`）に単位の記載が無い（エンジニア）
  - 小節フラグ・拍目の数え方（拍目が 1 始まりか）。決定 No.15 は 1 始まりとして扱っている（エンジニア）
  - ボスのデータが `Assets/Level/Data/Develop/Boss/` にある。製品版にも入る（GameData.Shared）ので、`Master/` へ移すか（エンジニア・企画）
  - 敵ごとの本来の値（企画の意図値）。このページは現状値だけを書く（企画）

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
| ジャストダメージ倍率 | 0 | 倍 | `Assets/Level/Data/Master/InGame/Battle/ExapleAttackDefinition.asset:18` |
- `ConfirmedDamage` は、ダメージが確定ダメージより小さいときに確定ダメージまで引き上げる（`ConfirmedDamage.cs:19-24`）。
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
