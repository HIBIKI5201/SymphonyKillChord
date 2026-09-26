# Symphony Kill Chord — QAテストケース (CBT / 2026-09-06)

Anatomia のビジネスドメイン (10) x `spec/feature/game-spec.md` GS-01〜GS-15 x Notion 仕様本体 (296ページ) から起こした **56 ケース**。

| 総数 | critical | high | medium | 機械 | 人間 | 機械+人間 | CBTシートへ追加 | 仕様未確定で保留 |
|---|---|---|---|---|---|---|---|---|
| 56 | 17 | 29 | 10 | 29 | 19 | 8 | 13 | 6 |

**読み方** — 各行は `ID (実行者 / 優先度 / 種別 / 走らせ方) 概要 — 期待結果` + 補足。

- **機械** = Unity Test Framework で判定する → 設計は [machine-test-runner-design.md](../../test/machine-test-runner-design.md)、台帳は `.augur/tests.config.json`
- **人間** = 実機での見た目・音・操作感の判断が要る → CBT QAシートで人が確認する
- **機械+人間** = 値は機械、提示 (表示・音) は人間。値の検証だけを機械側に置く
- 種別は Augur の `assurance` / `regression` / `guardrail` (guardrail は退役免除)
- 手順・前提を含む全項目は [qa-testcases.csv](./qa-testcases.csv) にある
- ⚠ = 仕様が未確定で合否判定できない。裁定後に有効化する

## Rhythm Input and Kill Chord Resolution (6)

- **QA-RH-01** (機械+人間 / critical / assurance / EditMode) **6拍種の判定** — 入力間隔をBPM補正した拍種が1,2,3,4,6,8のいずれかで記録され、HUDのノーツ表示と一致する
  - GS-01 ・ CBT 4-3/4-4
- **QA-RH-02** (機械 / critical / assurance / EditMode) **Allノーツになる3条件** — 3条件すべてで拍が判定されずAllノーツとなり、コマンド先頭でのみ使用できる
  - GS-01 ・ CBT 4-5〜4-7
- **QA-RH-03** (機械+人間 / high / assurance / EditMode) **Just判定** — grid一致入力のみJust判定になり、判定表示が区別できる
  - GS-01 ・ CBT 4-8
- **QA-RH-04** (機械 / high / guardrail / EditMode) **攻撃硬直0.56秒** — 硬直時間0.56秒の間は次の攻撃が受け付けられない
  - GS-01 ・ CBT 4-9
- **QA-RH-05** (機械 / critical / assurance / EditMode) **キルコード列の照合と発動** — パターンとアクション種別条件の両方が一致したときだけスキルが発動する
  - GS-03 ・ CBT 6-1
- **QA-RH-06** (機械 / high / regression / EditMode) **タイムアウトでコマンド破棄** — 入力中のコマンド列が破棄され、次入力がAllノーツで先頭になる
  - GS-03 ・ CBT対応なし

## Player Action and Combat Targeting (5)

- **QA-PA-01** (機械 / critical / assurance / EditMode) **遮蔽物ありの射撃** — 障害物が無い場合のみ命中する
  - GS-02 ・ CBT 5-6/5-7
- **QA-PA-02** (機械 / high / assurance / EditMode) **リズムによる武器選択** — 2=ショットガン、4=ハンドガン、8=サブマシンガンの攻撃定義が選ばれる
  - GS-02 ・ CBT 5-2〜5-4
- **QA-PA-03** (機械 / high / assurance / EditMode) **回避のクールダウンとキャンセル** — クールダウン中は回避不可、キャンセル可否が仕様どおり
  - GS-02 ・ CBT 5-15 追加 ・ ⚠ 未取得(回避のCD値が仕様に無い)
- **QA-PA-04** (機械+人間 / medium / assurance / EditMode) **標的切替** — 遮蔽物越しの敵を含め、選択対象が仕様どおりの順で切り替わる
  - GS-02 ・ CBT 5-12
- **QA-VF-02** (人間 / medium / assurance) **カメラワーク** — カメラが仕様どおり追従し、壁抜け・急激な反転が起きない
  - GS-02 ・ CBT 5-19 追加

## Combat State and Effect Resolution (6)

- **QA-CS-01** (機械 / critical / assurance / EditMode) **ダメージ計算式 (バフ・デバフ無し)** — GS-02 の統一式どおり 基礎ダメージ量 × Justダメージ倍率 × クリティカルダメージ倍率 になる (バフ・デバフ項は 1 倍)
  - GS-02 ・ CBT 5-10 ・ ⚠ バフ・デバフ有りの条件は保留 — GS-02 の統一式がプレイヤー側は攻撃力バフで除算、敵側は乗算で矛盾
- **QA-CS-02** (機械 / high / assurance / EditMode) **クリティカル倍率** — クリティカル率%の確率で発生し、発生時は通常計算にクリティカルダメージ倍率が乗る
  - GS-02 ・ CBT 5-10
- **QA-CS-03** (機械 / high / regression / EditMode) **多段ヒット抑制** — 1回の攻撃で同一対象に重複ダメージが入らない
  - GS-02 ・ CBT 5-18 追加
- **QA-CS-04** (機械 / high / assurance / EditMode) **バフ/デバフ/回復の一回限り適用** — 効果が重複適用されず、仕様どおり上書き/延長される
  - GS-03 ・ CBT 6-11 追加
- **QA-CS-05** (機械 / critical / assurance / PlayMode) **HP0で死亡・戦闘終了** — プレイヤー死亡で戦闘が終了し、敗北リザルトへ遷移する
  - GS-02 ・ CBT 5-11
- **QA-VF-01** (人間 / high / assurance) **スキルエフェクトの表示** — 発動したスキルに対応するエフェクトが正しい位置・向き・尺で再生される
  - GS-03 ・ CBT 6-13 追加

## Musical Time and Adaptive Arrangement (5)

- **QA-MT-01** (機械+人間 / critical / assurance / PlayMode) **BGM構成 4/16/4** — 4小節intro → 16小節loop → 4小節outro の構成で再生される
  - GS-04 ・ CBT 7-2
- **QA-MT-02** (機械+人間 / high / assurance / PlayMode) **装備スキル数によるアレンジ順** — 2個: original→skill1→original→skill2 / 3個: original→skill1→skill2→skill3
  - GS-04 ・ CBT 7-4/7-5
- **QA-MT-03** (機械+人間 / high / assurance / PlayMode) **2小節ブロック差し替え** — loopが2小節block単位で差し替わり、拍がずれない
  - GS-04 ・ CBT 7-3
- **QA-MT-04** (機械 / critical / guardrail / PlayMode) **音楽時間と敵アクションの同期維持** — 時間経過でも敵アクションと拍のずれが蓄積しない
  - GS-04 ・ CBT 7-11 追加
- **QA-MT-05** (人間 / medium / assurance) **HUDの再生中要素表示** — 現在再生中の音楽要素が識別できる
  - GS-04 ・ CBT 7-6

## Enemy Encounter and Stage Simulation (4)

- **QA-EN-01** (人間 / critical / assurance) **敵攻撃の予告表示** — 攻撃前にline または area の予告が表示され、拍に同期する
  - GS-02 ・ CBT 5-9
- **QA-EN-02** (機械 / high / assurance / EditMode) **ウェーブ/スポーン** — ウェーブが仕様どおりの順で出現し、プール枯渇や二重スポーンが起きない
  - GS-05 ・ CBT 5-16 追加
- **QA-EN-03** (人間 / high / assurance) **ボス戦** — ボス固有の予告・ギミックが動作し、撃破で遭遇が終了する
  - GS-05 ・ CBT 5-17 追加
- **QA-EN-04** (人間 / high / guardrail) **ステージ尺 約2分** — 約2分に収まる
  - GS-05 ・ CBT 8-11 追加

## Mission Evaluation and Result (5)

- **QA-ME-01** (人間 / critical / assurance) **勝利リザルトの表示項目** — ステージ/キャラクター/メイン・サブミッション/経過時間/最大コンボ/ランクが表示される
  - GS-05 ・ CBT 8-3
- **QA-ME-02** (人間 / high / assurance) **敗北リザルトの表示項目** — サブミッション/ランクの代わりに攻略Tipsが表示され、完了と再出撃が選べる
  - GS-05 ・ CBT 8-4/8-5
- **QA-ME-03** (機械 / high / assurance / EditMode) **ランク判定 C/B/A/S** — メインのみ=C / サブ1つ達成=B / 2つ=A / 3つ=S になる (サブミッションは重複ありで3つ課される)
  - GS-06 ・ CBT 8-6
- **QA-ME-04** (機械 / medium / assurance / EditMode) **リザルト後の戻り先** — 仕様で定めた画面へ戻る
  - GS-05 ・ CBT 8-10 ・ ⚠ 要裁定(GS-05矛盾: Home と 作戦/StageSelect で記述が割れる)
- **QA-ME-05** (機械+人間 / high / assurance / PlayMode) **再出撃** — 同じステージが編成を保ったまま開始する
  - GS-05 ・ CBT 8-5

## Progression, Research and Loadout (5)

- **QA-PR-01** (機械 / critical / assurance / EditMode) **末尾ノーツ競合** — 同時装備できず、理由が提示される
  - GS-03 ・ CBT 6-9
- **QA-PR-02** (機械 / high / assurance / EditMode) **装備枠の拡張** — 装備枠が2から3へ増える
  - GS-03 ・ CBT 6-8
- **QA-PR-03** (機械 / high / assurance / PlayMode) **研究ツリーの解放とリセット** — 現在パラメータ/コスト/解放可否が正しく表示され、リセットでポイントが戻る
  - GS-06 ・ CBT 9-10/9-11
- **QA-PR-04** (機械 / medium / assurance / EditMode) **スキルレベル上限** — 個別の上限で頭打ちになり、超過投資できない
  - GS-03 ・ CBT 6-12 追加
- **QA-PR-05** (機械 / high / assurance / PlayMode) **ステージ解放と報酬** — 左→右の解放ツリーが進み、報酬ポイントが加算される
  - GS-06 ・ CBT 9-2/8-7

## Narrative and Game Flow (5)

- **QA-NA-01** (人間 / critical / assurance) **New Game 導線** — title→最初のstory→battle tutorial→home tutorial→Home の順で進む
  - GS-06 ・ CBT 2-6
- **QA-NA-02** (人間 / critical / assurance) **Continue 導線** — title→Home→Operation→選択story/stage→mission完了 と進む
  - GS-06 ・ CBT 2-7
- **QA-NA-03** (機械 / high / regression / PlayMode) **シーン完了の一回性** — 完了処理が二重に走らず、進行フラグが1回だけ立つ
  - GS-06 ・ CBT 11-12 追加
- **QA-NA-04** (機械 / medium / assurance / PlayMode) **サイドシナリオ** — 読了状態が保存され、ツリー表示に反映される
  - GS-06 ・ CBT 9-16 追加
- **QA-VF-03** (人間 / medium / regression) **シーケンス演出の中断** — 演出が破綻せず、復帰後に状態が一致する
  - GS-06 ・ CBT 11-13 追加

## Persistence and Player Settings (5)

- **QA-PS-01** (機械 / critical / assurance / PlayMode) **5分類の永続化** — 5分類すべてが復元される
  - GS-07 ・ CBT 10-1〜10-5
- **QA-PS-02** (人間 / high / assurance) **プラットフォーム別保存** — 各プラットフォームに適した保存先で復元される
  - GS-07 ・ CBT 10-7/10-8
- **QA-PS-03** (機械 / high / regression / PlayMode) **セーブ破損復旧** — クラッシュせず、仕様で定めた復旧/初期化が行われる
  - GS-07 ・ CBT 10-6/10-10 ・ ⚠ 要仕様(GS-07未解決: 破損復旧・migration が未指定)
- **QA-PS-04** (人間 / high / assurance) **設定オーバーレイの決定/取消** — シーン遷移せずoverlayで開き、取消で変更が戻り、決定で保存される
  - GS-09 ・ CBT 9-13
- **QA-PS-05** (人間 / medium / assurance) **タイトルのセーブデータリセット** — 確認のうえ全データが初期化される
  - GS-09 ・ CBT 2-4/2-5 ・ ⚠ 要仕様(GS-09未解決: reset確認の有無が未指定)

## Guidance, Feedback and Recovery (6)

- **QA-GU-01** (機械+人間 / critical / assurance / PlayMode) **ポーズと再開カウントダウン** — 時間が停止し、再開時にカウントダウンが入る
  - GS-09 ・ CBT 11-1〜11-3
- **QA-GU-02** (人間 / high / assurance) **HUD 3種** — 3種すべてが表示され、状態と一致する
  - GS-09 ・ CBT対応なし
- **QA-GU-03** (人間 / high / assurance) **チュートリアル(ステージ0)の予告** — リズムアクション/通常戦闘/ボスが通常進行前に予告される
  - GS-05 ・ CBT 3-1〜3-4 ・ ⚠ 要仕様(GS-09未解決: チュートリアル完了基準)
- **QA-GU-04** (人間 / medium / assurance) **ロード進捗表示** — ロード進捗が表示され、無反応時間が生じない
  - GS-09 ・ CBT 11-10
- **QA-GU-05** (人間 / high / assurance) **コントローラ操作とモバイル操作** — すべての画面がナビゲート可能で、到達不能な要素が無い
  - GS-09 ・ CBT 11-8/11-9
- **QA-GU-06** (人間 / medium / assurance) **改造画面の未保存離脱** — 保存/破棄の判断が求められる
  - GS-09 ・ CBT 9-9

## (cross-cutting) (4)

- **QA-QP-01** (機械 / critical / guardrail / Performance) **PC 60FPS** — 60 FPS 以上 (frame_time p95 <= 16.6ms)
  - GS-08 ・ CBT対応なし
- **QA-QP-02** (機械 / critical / guardrail / Performance) **Android 30FPS** — 30 FPS 以上 (frame_time p95 <= 33.3ms)
  - GS-08 ・ CBT 12-1
- **QA-QP-03** (機械 / high / guardrail / Performance) **インゲームロード3秒** — 3秒以内
  - GS-08 ・ CBT 12-2
- **QA-QP-04** (機械 / high / guardrail / Performance) **15分セッションの安定性** — エラー0件、メモリリークがほぼ無い
  - GS-08 ・ CBT 12-3/12-4

## 走らせ方の内訳 (機械 / 機械+人間 の 37 件)

- **EditMode** (20): QA-RH-01, QA-RH-02, QA-RH-03, QA-RH-04, QA-RH-05, QA-RH-06, QA-CS-01, QA-CS-02, QA-CS-03, QA-CS-04, QA-PA-01, QA-PA-02, QA-PA-03, QA-PR-01, QA-PR-02, QA-PR-04, QA-ME-03, QA-ME-04, QA-EN-02, QA-PA-04
- **PlayMode** (13): QA-CS-05, QA-MT-01, QA-MT-02, QA-MT-03, QA-MT-04, QA-NA-03, QA-NA-04, QA-PS-01, QA-PS-03, QA-PR-03, QA-PR-05, QA-GU-01, QA-ME-05
- **Performance** (4): QA-QP-01, QA-QP-02, QA-QP-03, QA-QP-04

詳細は [machine-test-runner-design.md](../../test/machine-test-runner-design.md)。
