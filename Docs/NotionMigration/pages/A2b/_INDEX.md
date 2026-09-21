# A2b インゲームのその他のシステムページ — 索引

- 対象: システムリスト「インゲーム」のうち、敵 / カメラ / カメラシステムパラメータ / ミッション / リザルト / シークエンス / ターゲットシステム / インゲームUI / レティクル / ステージ演出 / StageNodeAssetの使い方 / リアルトゥーンシェーダー / EventBus とその子ページ（48 ページ）
- 確認日 / コミット: 2026-09-22 / 73fefeb45（Notion スナップショットも 2026-09-22 取得）
- 書き方: PAGES_BRIEF.md の「追加指示: 現行のレイアウトと書き方に合わせる」に従った。既存の節構成・見出し・口調は保ったまま、誤りの修正と足りない情報の追記だけを行った
- 調査方法: 静的な調査のみ。Unity での実機確認はしていない

## 集計

| 処置 | 件数 |
| --- | --- |
| 本文置き換え | 39 |
| 追記のみ | 6 |
| 変更不要 | 3 |
| 統合・アーカイブ推奨 | 0（StageNodeAssetの使い方は置き換え案で書き、アーカイブ案は要確認に残した） |
| 合計 | 48 |

| 信頼性 | 件数 |
| --- | --- |
| 最新 | 10 |
| 一部古い | 36 |
| 陳腐化 | 2（StageNodeAssetの使い方、シークエンス ③ タイトル復帰フロー（ESC長押し）） |
| 空 / 判定不能 | 0 |

## 領域全体で見つかった矛盾・重複

1. **存在しないクラスや、撤回済みの機能が残っている。**
   - シークエンスと子③に、`ReturnToTitleInitializer`（Order 450。2026-09-04 に削除）と「ESC 長押しでタイトル復帰」（PR #1483 で撤回）が残っていた。
   - 現在は、ポーズ画面の「タイトルへ戻る」ボタン → `PauseWindowInitializer`（Order 1010）→ `ReturnToTitleController` の順に処理する。
   - 担当外の「入力」ページにも同じ記述が残っている。
   - `StageNodeAsset` もリポジトリに存在しない。現在は `BattleStageAsset` / `ScenarioStageAsset` + `StageBindAsset` + `StageTreeAsset` で構成する（EN-32 / OG-109 / RL-15）。
2. **依存の向きが誤っている。**
   - カメラのロックオンは「攻撃者に付け替え」と書かれているが、実装では命中した敵（`EOnTakeDamage.DefenderId`）に付け替える。
   - Enemy と Mission は互いに依存している。ページには一方向の依存しか書かれていない。
   - カメラ（Order 600）は Player（Order 500）にも依存している。
   - `SequenceModuleContainer` と `InGameHudInitializer` は「参照元なし」とされているが、実際は複数のモジュールから参照されている。
   - ステージ演出は「ServiceLocator に登録」とされているが、実際は何も登録していない。
3. **ページ同士で記述が食い違っている。**
   - 敵ページは「Container 経由」、ミッションページは「直接取得」と書いている。実装は Container 経由である。
   - 「アウトラインもカラフルにしたい」は、マテリアル分離の説明を「顔だけアウトライン処理を変えたい」に譲っているが、そのページに説明が無い。
4. **Initializer の Order と継承が誤っている。** `InGameHudInitializer` / `SkillInputProgressUIInitializer` は「継承しない」とされているが、実際は `InGameInitializationModuleBase` を継承していて、Order は 495 / 442。
5. **反映項目（20_ 文書）の側に誤りがある。**
   - CM-10 の「`_collisionMask` は Nothing」は誤りで、実際の値は m_Bits 51。このマスクは Obstacle レイヤーを含まないため、壁避けが効かない恐れがある（要実機確認）。
   - HD-18 の「戦闘ポーズで弾幕も止まる」は根拠が弱い。
6. **実装側にも懸念がある（要確認）。**
   - `TargetSystemController.OnTargetLocked` はロックオン入力では発火しない。そのため、ミッションの LockOn 行動が計上されない恐れがある。
   - クリアボイス・ゲームオーバーボイス・クリア BGM の Cue が空で、今は鳴らない。
   - チュートリアルのミッション定義が、Master と Demo で別のアセットになっている（TU-04）。
7. **研究ページが実装とつながっていない。** リアルトゥーンシェーダー系のページは、2026-03 の ShaderGraph 試作の記録のままだった。実装は SilToon（`Assets/Scripts/Shaders/SilToon/`）なので、各ページに実装との対応を追記した（AS-16）。

## ページ一覧

| page_id | タイトル | 書き込み許可 | 信頼性 | 処置 | 原稿 | 要点 |
| --- | --- | --- | --- | --- | --- | --- |
| 3167c2c6-cc02-8059-b7c7-dffda7f42a76 | リアルトゥーンシェーダー | 可 | 一部古い | 追記のみ | 3167c2c6-cc02-8059-b7c7-dffda7f42a76.md | 「実装（SilToon）」の節を追加した（パス・プロパティ・研究との対応）。AS-16 |
| 3167c2c6-cc02-80b3-a6ef-ca7045b77969 | 背面法を利用したアウトラインの作成 | 可 | 最新 | 追記のみ | 3167c2c6-cc02-80b3-a6ef-ca7045b77969.md | SilToon の OUTLINE パス（Cull Front）と処理の順番を追記 |
| 31e7c2c6-cc02-8097-abfc-c97ecafb660a | ハードエッジ問題 | 可 | 最新 | 本文置き換え（軽微） | 31e7c2c6-cc02-8097-abfc-c97ecafb660a.md | コード例で未宣言だった normals を修正。保存先（UV4）と焼き込みツールを追記 |
| 3167c2c6-cc02-80ed-9eec-c1db0493263d | アウトラインの太さを制御したい | 可 | 最新 | 追記のみ | 3167c2c6-cc02-80ed-9eec-c1db0493263d.md | 実装の計算（0.5〜1 に寄せてから補間）と現在の値を追記。209,655 文字の大半は埋め込み画像 |
| 3167c2c6-cc02-8053-907e-e22bba0986fa | アウトラインもカラフルにしたい | 可 | 一部古い | 追記のみ | 3167c2c6-cc02-8053-907e-e22bba0986fa.md | SilToon では未実装（アウトラインは単色の _OutlineColor）と明記 |
| 3207c2c6-cc02-80d3-9fa2-eab8ad0dd7fb | 顔だけアウトライン処理を変えたい(簡略化) | 可 | 最新 | 追記のみ | 3207c2c6-cc02-80d3-9fa2-eab8ad0dd7fb.md | _ZOffset による実装と、肌のマテリアルの値（0.01）を追記 |
| 3797c2c6-cc02-8083-8469-f92524ecff07 | StageNodeAssetの使い方 | 可 | 陳腐化 | 本文置き換え（改名推奨） | 3797c2c6-cc02-8083-8469-f92524ecff07.md | BattleStage / ScenarioStage / StageBind / StageTree の構成に書き直した。EN-32 / OG-109 / RL-15 |
| 37b7c2c6-cc02-80d4-9552-de328e622fe3 | カメラシステムパラメータ | 可 | 一部古い | 本文置き換え | 37b7c2c6-cc02-80d4-9552-de328e622fe3.md | 7 項目から 24 項目に拡充し、現在の値を追加。_boneRotateSpeed の意味を修正。CM-10 |
| 3957c2c6-cc02-809c-9b9d-f97c42fd9b3b | カメラ | 可 | 一部古い | 本文置き換え | 3957c2c6-cc02-809c-9b9d-f97c42fd9b3b.md | シェイク・PostEffect・Player への依存・外部制御を追加。付け替え先を DefenderId に訂正。CM-05 / CM-12 / CM-13 |
| 3bf7c2c6-cc02-8141-90f4-ef2fa4cc119a | ① 通常追従・回転フロー（毎フレーム） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8141-90f4-ef2fa4cc119a.md | 候補の更新・シェイク・LateUpdate・外部制御中の停止を追加 |
| 3bf7c2c6-cc02-811a-bbad-f6faee96225f | ② ロックオン開始フロー（入力・被弾時） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-811a-bbad-f6faee96225f.md | 「攻撃者 ID」を DefenderId に訂正し、分岐を 4 つに整理。CM-05 |
| 3bf7c2c6-cc02-8169-8fe2-d1bfb8f182c0 | ③ オートロックオン解除フロー | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8169-8fe2-d1bfb8f182c0.md | 解除条件 3 つに現在の値を併記。対象が消えたときの解除を追加 |
| 3957c2c6-cc02-80cd-94c0-f412b11ffcf7 | 敵 | 可 | 一部古い | 本文置き換え（子ページ⑤を新設） | 3957c2c6-cc02-80cd-94c0-f412b11ffcf7.md | TGS 期に追加された実装、Mission との双方向の依存、ステージ概念の所在を追記。EN-25 / EN-30 |
| 3bf7c2c6-cc02-8193-b6cf-cfd3ff3cb46f | ① AI 移動制御フロー（毎フレーム） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8193-b6cf-cfd3ff3cb46f.md | 存在しない EnemyView を EnemyMoveView に修正。攻撃の予約は BehaviorGraph 側で行う |
| 3bf7c2c6-cc02-81df-b1af-ef9310f5693b | ② リズム同期攻撃予約フロー（非同期・イベント駆動） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-81df-b1af-ef9310f5693b.md | IEnemyAttackController を経由する形に修正。予兆デカールと取り消しの分岐を追加 |
| 3bf7c2c6-cc02-818e-9975-fa89910dbb43 | ③ ウェーブスポナー & Wave開始通知フロー（ウェーブクリア／開始時） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-818e-9975-fa89910dbb43.md | 時間切れ・入場待ち・OnWaveAllCleared を追加。EN-17 |
| 3d67c2c6-cc02-816f-830b-dc4ee6f3e1df | ④ 攻撃後行動選択フロー（攻撃実行直後） | 可 | 最新 | 本文置き換え（小） | 3d67c2c6-cc02-816f-830b-dc4ee6f3e1df.md | 味方の検索と、上書きした移動先の解除を追記 |
| 39e7c2c6-cc02-80d5-ac4e-cd46b87fd375 | ミッション | 可 | 一部古い | 本文置き換え | 39e7c2c6-cc02-80d5-ac4e-cd46b87fd375.md | 会話・全 Wave 撃破・チュートリアル表示を追加。Container の中身・型名・依存を修正。EN-24 / SC-09 / TU-03 |
| 3bf7c2c6-cc02-81cb-9df1-d2491411dc0a | ① ミッション開始フロー（ステージ読み込み時） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-81cb-9df1-d2491411dc0a.md | MissionId から Addressables のリポジトリで読み込む流れに修正 |
| 3bf7c2c6-cc02-8144-aeef-c67ace0b4a39 | ② 敵撃破→クリア判定→保存→リザルト表示フロー | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8144-aeef-c67ace0b4a39.md | HP0 での即時通知と、全 Wave 撃破の 2 経路を記載。EN-16 |
| 3bf7c2c6-cc02-81fe-a927-eadd5d8f3456 | ③ サブミッション進捗のHUD表示フロー（毎フレーム／イベント時） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-81fe-a927-eadd5d8f3456.md | 表示状態を Failed / Challenging / Succeeded に修正。EN-22 |
| 3bf7c2c6-cc02-8184-ab76-d50a4ac7e15a | ④ 目標シーケンスの進行フロー（ステップ更新時） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8184-ab76-d50a4ac7e15a.md | ステップを進めるのは MissionRuntimeService（TryAdvance）と修正 |
| 3c67c2c6-cc02-8179-a0c0-e067bb5af6b7 | ⑤ 目標ステップ進入時アクションの実行フロー | 可 | 一部古い | 本文置き換え | 3c67c2c6-cc02-8179-a0c0-e067bb5af6b7.md | 会話の実行器と表示ルールを追加。SC-09 / TU-03 |
| 3c67c2c6-cc02-81ef-9ed5-cbaa8c1fcc73 | ⑥ インゲーム中のシナリオ再生フロー | 可 | 一部古い | 本文置き換え | 3c67c2c6-cc02-81ef-9ed5-cbaa8c1fcc73.md | 終了時の処理の順番を修正。ポーズに失敗したときの分岐を追加 |
| 39e7c2c6-cc02-808d-8c6b-dd69ea4edfa8 | リザルト | 可 | 一部古い | 本文置き換え | 39e7c2c6-cc02-808d-8c6b-dd69ea4edfa8.md | 依存元に PauseWindow を追加。演出の値・ボタン操作・固定文言を追記。HD-13 / HD-21 / HD-22 |
| 3bf7c2c6-cc02-8102-8fb3-d99d2351ef21 | ① クリア → 完了ボタン → OutGame帰還フロー | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8102-8fb3-d99d2351ef21.md | Canvas の無効化・文字の Timeline・ポリシーによる分岐・失敗時の復帰を追加 |
| 3bf7c2c6-cc02-814c-a084-e500c1e18251 | ② ゲームオーバー → リトライボタン → InGame再読込フロー | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-814c-a084-e500c1e18251.md | リトライ時に選択状態を消す点、Tips、ポーズ画面と処理を共用する点を追記 |
| 39e7c2c6-cc02-804f-8624-f38e02cf7b72 | シークエンス | 可 | 一部古い | 本文置き換え | 39e7c2c6-cc02-804f-8624-f38e02cf7b72.md | ReturnToTitleInitializer を削除。View 3 クラスと参照元を追加。HD-11 / HD-13 / HD-33 |
| 3bf7c2c6-cc02-8196-befb-f6b6598210f2 | ① ステージ開始演出フロー | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8196-befb-f6b6598210f2.md | StartAsync を Start に修正。ロード完了待ちと演出の時間を追記。HD-12 |
| 3bf7c2c6-cc02-813a-b8d9-e1b983c89a04 | ② クリア→保存→リザルト表示フロー | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-813a-b8d9-e1b983c89a04.md | SaveClearAsync の引数を修正し、MarkCompleted とクリア演出の値を追加。HD-14 / HD-15 |
| 3bf7c2c6-cc02-81f7-8423-d89bf8c0ffd6 | ③ タイトル復帰フロー（ESC長押し） | 可 | 陳腐化 | 本文置き換え（改題「（ポーズ画面）」） | 3bf7c2c6-cc02-81f7-8423-d89bf8c0ffd6.md | ESC 長押しは撤回済み。ポーズ画面 → PauseWindowInitializer → ReturnToTitleController の流れに書き換えた。HD-17 |
| 3c67c2c6-cc02-81f3-b9fa-c1775a59771d | ④ 戦闘ポーズフロー（通常ポーズとシナリオポーズ） | 可 | 一部古い | 本文置き換え | 3c67c2c6-cc02-81f3-b9fa-c1775a59771d.md | OnBattlePauseInput を OnOptionInput に修正。timeScale・受付条件・ボタンを追記。HD-18 |
| 39e7c2c6-cc02-80e9-933f-f02210602bf6 | ターゲットシステム | 可 | 一部古い | 本文置き換え | 39e7c2c6-cc02-80e9-933f-f02210602bf6.md | 距離のクエリ 2 種・候補・EOnLockOnAcquired を追加し、依存元を修正。CM-16 / CM-17 |
| 3bf7c2c6-cc02-8183-a078-f311d88d7cbe | ① ターゲット登録フロー（敵出現時） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8183-a078-f311d88d7cbe.md | 登録するのは EnemyLifeCycle と訂正。死亡時の登録解除を追加 |
| 3bf7c2c6-cc02-815a-a6d6-cfcb3edb3cbd | ② ターゲット選択フロー（ロックオン入力時） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-815a-a6d6-cfcb3edb3cbd.md | SwitchTarget を ChangeTarget / TrySwitchTarget の 2 分岐に修正 |
| 3bf7c2c6-cc02-810f-837a-cbe81abae339 | ③ 範囲判定フロー（攻撃・範囲スキル） | 可 | 最新 | 追記のみ | 3bf7c2c6-cc02-810f-837a-cbe81abae339.md | 呼び出し側が渡す値（上限 1000m、スキルは 12m・半角 30°）を追記 |
| 39e7c2c6-cc02-8078-b593-ff189580264e | インゲームUI | 可 | 一部古い | 本文置き換え | 39e7c2c6-cc02-8078-b593-ff189580264e.md | Order を全件記載（495 / 442 ほか）し、継承と依存元を訂正。CM-14 / CM-15 / HD-07 / HD-09 / HD-10 / HD-30 |
| 3bf7c2c6-cc02-8177-b536-eb2bd0bf5c1c | ① プレイヤーHPバー更新フロー（被ダメージ時） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-8177-b536-eb2bd0bf5c1c.md | 表示クラスを HealthHudView から HealthTextView / HealthBarView に修正 |
| 3bf7c2c6-cc02-816f-abd4-ec73dae405d0 | ② ロックオン敵体力表示フロー（毎フレーム） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-816f-abd4-ec73dae405d0.md | 分岐を 3 つ（ロック中 / 候補 / 非表示）に修正。HD-09 |
| 3d67c2c6-cc02-816d-bc9d-d0a44b549c00 | EventBus | 可 | 一部古い | 本文置き換え | 3d67c2c6-cc02-816d-bc9d-d0a44b549c00.md | EOnLockOnAcquired と利用箇所 4 件を追加。PlayerView が購読する目的（Hit SE）を訂正。IF-06 |
| 3d67c2c6-cc02-81e9-8c32-cac2af5bd265 | ① イベント購読の登録タイミング | 可 | 最新 | 変更不要 | — | EventBus.cs の静的コンストラクタと Register の処理に一致 |
| 3d67c2c6-cc02-81fc-834d-d7b0b874436e | ② イベント発火から複数リスナーへの配信 | 可 | 最新 | 変更不要 | — | GetInvocationList と try/catch の処理に一致 |
| 3d67c2c6-cc02-81bc-9e56-d92f89197f1c | ③ シーンロード時のリスナー解除（プレイセッション開始時） | 可 | 最新 | 変更不要 | — | SubsystemRegistration でのリセットに一致 |
| 3bf7c2c6-cc02-81b8-ad8c-cc400c63704e | レティクル | 可 | 最新 | 本文置き換え | 3bf7c2c6-cc02-81b8-ad8c-cc400c63704e.md | 図の依存の向きを訂正。Camera.main への依存と「候補」の定義を追記。CM-14 |
| 3bf7c2c6-cc02-81ae-aad1-e1e9477cfd3a | ① レティクル表示フロー（毎フレーム） | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-81ae-aad1-e1e9477cfd3a.md | 起点を View の LateUpdate に訂正。画面外の敵の除外とマーカーのプールを追加 |
| 3bf7c2c6-cc02-81df-9f25-fc4dc8e9408b | ステージ演出 | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-81df-9f25-fc4dc8e9408b.md | 拍への予約と Music への依存を追加。「登録あり」を「登録なし」に訂正。HD-27 / HD-28 |
| 3bf7c2c6-cc02-81a9-8af1-eacf4ec01d81 | ① Wave開始時のステージ演出フロー | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-81a9-8af1-eacf4ec01d81.md | 通知の受け手を Initializer に修正。カタログの参照と拍への予約を追加。HD-27 |
| 3bf7c2c6-cc02-816a-9add-cd9c83bf1602 | ② Timeline駆動の弾幕フロー | 可 | 一部古い | 本文置き換え | 3bf7c2c6-cc02-816a-9add-cd9c83bf1602.md | 見た目だけの演出で当たり判定は無く、Stage_01 限定。ポーズは PauseManager 経由。HD-28 |
