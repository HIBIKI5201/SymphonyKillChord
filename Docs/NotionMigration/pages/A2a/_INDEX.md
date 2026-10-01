# A2a インゲーム戦闘系のシステムページ — 原稿索引

**反映状況: 31/31 件 Notion へ反映済み（2026-09-23、Claude、`push --whole`）。** 反映後の照合で全件「書式正規化を超える差の可能性」の警告が出たが、`.notion-push-intended.md` との差分を複数件確認したところ、GFMパイプ表の`<table>`化・空行整形・末尾改行の違いのみで、内容の欠落は無かった。

- 対象: システムリストのうち、キャラ&バトル / 音楽 / スキル / プレイヤー / コンボ表示 / リズムガイド全画面演出 / キャラクターアニメーション / スキル効果 / 入力 / 状態効果（バフ・デバフ）と、その子ページ（処理フロー）。計 31 ページ（`areas/A2a.tsv`）
- 確認日: 2026-09-22 / 確認した実装: origin/develop 73fefeb45（worktree `.wt-notion-migration`。Assets 配下は 73fefeb45 と同一）/ Notion スナップショット 2026-09-22
- 書き方: PAGES_BRIEF の「追加指示（現行のレイアウトと書き方に合わせる）」に従い、既存の節構成・見出し・絵文字・表の列をそのまま使った。「適用する本文」は Notion 上の実際の形（`split_module_doc.py` の出力形。`# 概要 {color="gray_bg"}`、`## 🏗️ クラス {toggle="true"}` の中身はタブ字下げ、子ページは `<page url>`）で書いた。新しい節は既存の節の後ろに置いた

## 集計

| 区分 | 件数 |
| --- | --- |
| 対象ページ | 31 |
| 原稿あり | 31 |
| 変更不要 | 0 |
| 統合・アーカイブ・削除推奨 | 0 |

| 処置 | 件数 |
| --- | --- |
| 本文置き換え | 31 |
| 追記のみ | 0 |

| 信頼性 | 件数 |
| --- | --- |
| 最新 | 0 |
| 一部古い | 28 |
| 陳腐化 | 3（音楽 ②、リズムガイド全画面演出 ①、プレイヤー ①） |

書き込み許可は 31 件すべて「可」（システム概要の配下）。

## 領域全体で見つかった矛盾・重複

### 重大

1. **削除済みの `RhythmJustService` が 4 ページに残っている**（音楽、音楽 ②、リズムガイド全画面演出、同 ①）。PR #1513（2026-09-12）以降、判定は `IMusicSyncService.GetCurrentBeatType(out isJustHit)` が副作用なしで返し、`PlayerAttackController` → `PlayerAttackPresenter` → `IPlayerAttackSignal` で全画面演出・ガイド成功演出・振動・チュートリアル表示へ配る。4 ページとも同じ経路に描き直した（BT-09）。キャラ&バトル・プレイヤーのクラス表にも Presenter/Signal 4 型を足した
2. **入力履歴（`InputBufferingQueue`）は誰も読んでいない**のに、入力・プレイヤー・入力 ② が「Music/Skill がリズム判定に使う」「先行入力を実現する」と書いている。読むのは開発用の `InputDebugLogger` だけで、リズム判定は `RhythmState`、スキル判定は `SkillRhythmState` を使う（BT-22）→ 決定（2026-09-22 八幡）R13: 入力履歴は先行入力のために残す（現状は読む処理が無い）。入力・入力 ②・プレイヤーに書いた
3. **スキルの命中時効果（スキル 2・8）は「次の通常攻撃」ではなく、スキルを発動させたその攻撃で使われ、空振りなら失われる**。`PlayerAttackController` が `TryExecuteSkill` の直後に `Consume` するためである。スキル効果・同 ②・キャラ&バトルを直し、【要確認】を付けた
4. **仕様（仕様概要）と実装の食い違い**（いずれも仕様側のページは担当外。判定と本文の【要確認】に記載）
   - 回避はリズム履歴に入らず、スキルの照合はアクション種類を見ない（BT-19）→ 決定（2026-09-22 八幡）No.19: 回避はリズムを刻まない（実装どおり）。プレイヤー・スキル・スキル ① の【要確認】を外した
   - Just 倍率が全 6 武器で 1（仕様は 200%）（BT-01）。決定（2026-09-22 八幡）: データの誤り（`_justDamageMultiplier` を 2 に直す必要あり）
   - スキル 13 のアニメーションキーが `Skill_Left`（DB 行は `Skill_Right`）、スキル 4〜10 は空欄（BT-35）
   - コンボは空振り・リズムタイムアウト・通常攻撃をスキップするスキル・スキル 5 の HP 消費でも 0 に戻る。HUD は 4 以上で表示（BT-29）→ 決定（2026-09-22 八幡）No.20: 3 以上で表示（実装修正）。No.21: 被ダメージでリセットし、キルコード発動ではリセットしない（実装修正）。スキル 5 の HP 消費がどちらに当たるかは【要確認】
   - バリアは上限なしで、バフが切れても残る（BT-37）
5. **用語（決定（2026-09-22 八幡）No.1・2）**: スキル系モジュールのページ（スキル・同 ①②、スキル効果・同 ①②）の本文の説明語「スキル」を「キルコード」に直した。モジュール名・クラス名・ページ名・節見出し・「スキルツリー」・データの ID と対応する番号付きの呼称（「スキル 5」など）は残した。それ以外のページ（キャラ&バトル・状態効果など）は No.1 の運用に従い直していない。音楽の「Wave開始時」を「ウェーブ開始時」に直した（No.2）
6. **判定幅とオフセット**: → 決定（2026-09-22 八幡）No.10: 判定幅は小節長×割合（0.03125 小節、BPM84 で約 89ms、TGS で 1.25 倍に緩和、前後非対称）を音楽の「📝 ジャスト判定の要件」に追記。No.11: オフセットは判定と表示の両方をずらす（±0.30 秒・0.05 秒刻み）を音楽・音楽 ① に追記。自分の入力への効き方は【要実機確認】
7. **状態効果の再付与方針**: スキル 7 のマスター値は Ignore だが、コードが `AttackPowerIncreaseBuff`/`AttackPowerReductionDebuff` を Replace に固定しているため効かない（BT-37）

### 中

- `PlayerHealthHudPresenter` の所属がページ間で食い違っていた（プレイヤーは Player の Adaptor、キャラ&バトルは「`Adaptor.InGame.UI` の UI モジュール」）。実装の namespace は `Adaptor.InGame.Player`。キャラ&バトルの注記を直した
- 01 棚卸し B 表の誤り: 「`AttackPilpelineAsset` の誤字は Notion 側に無し」とあるが、Notion のキャラ&バトルにも残っている（クラス名は `AttackPipelineAsset`、ファイル名だけ `AttackPilpelineAsset.cs`）
- 削除・改名済みの型名: `ReturnToTitleInitializer`（入力）、`StageSceneInitializer`（プレイヤー。実体は `StageSceneObjects`）、`SkillUseCase`（スキル。実在名 `SkillUsecase`）、`EnemyMusicSpec`（音楽。現在は `MusicSyncSpec`）、`ActionParams`/`BeatStep`（キャラ&バトル）、`SkillVisual`（スキル効果。実体は `SkillView`）を直した
- 依存の向きの誤り: コンボ表示は Mission に依存しておらず、逆に Mission が `ComboHudPresenter` を呼ぶ。音楽の Title 依存は `IAudioSettings*` 経由に変わった。スキル効果の Target 依存は `ITargetRadiusQuery`/`IPlayerTargetRangeQuery`
- 処理フローの誤り: プレイヤーの攻撃パイプラインの順序（CriticalStep → WeaponDamageStep → OutOfRangeDamageStep）、状態効果の期限切れ（毎フレームではなく呼び出しのたび）、`SkillHitScheduler.Clear` の呼び出し元（ゲームプレイ停止ではなく `SkillInitializer.Shutdown`）、スキルのクールダウン中の扱い（`SkillExecutionFailurePolicy` は使わず、しかも現在到達しない）
- 構造の崩れ: 音楽のクラス表の途中に引用が入り、行と区切り行が重複して表が割れていた。1 つの表に直し、注記を表の後ろへ移した
- `RhythmGuideInitializer` は `Initialize()` の呼び出し元が無く、開発用シーンにしか置かれていない。本番のガイドは `ACLikeRhythmGuideInitializer`（Order 800）
- 同じ Order: `SkillEffectInitializer` と `SkillCrosshairProgressUIInitializer` が共に 440（互いを参照しないため現状は問題なし。IF-02 の規則案と関係する）→ 決定（2026-09-22 八幡）R77: 同じ Order で問題ないものは許容する。スキル効果・インゲームUI の Order の欄に「実行順は保証されないため、順序に依存するものは Order を分ける」と書いた

### 重複

- `プレイヤー / ② 攻撃実行フロー` と `キャラ&バトル / ① プレイヤー攻撃実行フロー` が同じ処理を描いていた。プレイヤー側は入力の受け付けと攻撃後の表示に絞り、ダメージ計算はキャラ&バトル ① を参照させた（統合はしない）
- 状態効果の補正の順序が `キャラ&バトル / ③` と `状態効果 / ①` の 2 か所にある。両方を同じ段階・順序（攻撃力 → クリティカル倍率 → 与ダメージ → 被ダメージ）にそろえた

### repo 側 NotionModuleDocs との統合

- Notion の方が新しいページ（音楽・リズムガイド全画面演出・スキル・スキル効果）は Notion の内容を土台にし、repo にしか無い記述が無いことを差分で確かめた
- repo の方が新しいページ（キャラクターアニメーション）は、repo で削除済みの注意書き（`Animaiton` フォルダ・Issue #1312）を Notion からも削除した
- 同一のページ（キャラ&バトル・プレイヤー・入力・コンボ表示・状態効果）は、実装との照合結果だけを反映した

## 織り込んだ反映項目

BT-09, BT-10, BT-12, BT-19, BT-22, BT-29, BT-35, BT-37, CM-18, CM-19, CM-20, CM-21, OG-218（入力マップの技術詳細のみ）, OG-219・OG-220（音楽ページの技術部分のみ）, HD-25, HD-26（プレイヤーの ④ View）

見送り: HD-23・HD-24（反映先は新規の被弾フィードバック節・アセット DB。キャラ&バトル ② に被弾演出の呼び出し点だけ描いた）

## ページ一覧

| page_id | タイトル | 書き込み許可 | 信頼性 | 処置 | 原稿 | 要点 |
| --- | --- | --- | --- | --- | --- | --- |
| 3957c2c6-cc02-80d8-b0c3-d586b36180fa | キャラ&バトル | 可 | 一部古い | 本文置き換え | [原稿](3957c2c6-cc02-80d8-b0c3-d586b36180fa.md) | `AttackPilpelineAsset`→`AttackPipelineAsset`、PR #1513 の Presenter/Signal 4 型を追加、`ActionParams`/`BeatStep` を削除、Skill 依存と Composition 節を追加、HUD Presenter の所属の注記を修正 → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）R6: 硬直は拍だけで決める（`AttackIntervalEvaluator` の行）。R9: 確定ダメージをプレイヤーのパイプラインに入れる（`ConfirmedDamage` の行） |
| 3bf7c2c6-cc02-815f-be7d-f3fe8018830c | ① プレイヤー攻撃実行フロー（入力イベント時） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-815f-be7d-f3fe8018830c.md) | 判定 1 回・`Consume`・Presenter 通知・ダメージ無しの 3 分岐を追加、パイプライン順序と `EOnTakeDamage` の位置を修正 → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.21: スキップ時の通知でコンボが 0 に戻る現状を不具合として追記 → 決定（2026-09-22 八幡）R6・R8・R9・R30: 硬直・障害物・確定ダメージ・対象なしの直線射撃を「仕様と現状の実装が違う」箇条書きで追記（確定ダメージの計算順は【要確認】） |
| 3bf7c2c6-cc02-81de-8e4b-d03a21ab3c44 | ② HP 変化とHUDへの反映フロー（ダメージ被弾時） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81de-8e4b-d03a21ab3c44.md) | `OnHealthChanged` の発火元を `CharacterEntity` に修正、無敵中・死亡・被弾演出・ミッション記録を追加 |
| 3bf7c2c6-cc02-8193-8e0c-e0604f828db6 | ③ 状態効果の適用フロー（攻撃前後） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8193-8e0c-e0604f828db6.md) | 呼び出し元を `AttackCalculator`/`CriticalStep`/`DamageExecutor` に修正し 4 段の補正に |
| 3d67c2c6-cc02-811b-ac59-e928713c9cc2 | ④ SkillHitSchedulerによる連撃タイミング制御 | 可 | 一部古い | 本文置き換え | [原稿](3d67c2c6-cc02-811b-ac59-e928713c9cc2.md) | `SkillHitLoopView` を追加、1 ヒットの適用先と `Clear` の呼び出し元を修正 |
| 3957c2c6-cc02-806b-9cc8-fbe9c9e8bf63 | 音楽 | 可 | 一部古い | 本文置き換え | [原稿](3957c2c6-cc02-806b-9cc8-fbe9c9e8bf63.md) | `RhythmJustService` 削除、`EnemyMusicSpec`→`MusicSyncSpec`、割れたクラス表を修復、Composition 節新設、依存を全面更新、ジャスト判定の要件節を新設（BT-10） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.10: 判定幅（小節長×割合）を要件節に追記。No.11: オフセットが判定と表示の両方に効く旨。No.2: ウェーブ → 決定（2026-09-22 八幡）R2: ジャスト帯は判定幅に関係なく一定の太さ（`ACLikeRhythmGuideView` の行。現状 1/3、実装修正） |
| 3bf7c2c6-cc02-81fa-a314-c728299d6ede | ① BGM 再生とリズム同期の更新フロー（毎フレーム） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81fa-a314-c728299d6ede.md) | 判定オフセット・`SetGameplayActive`・巻き戻し・2 小節タイムアウトを追加。AudioSource→CRI → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.11: 判定と表示の両方をずらす段落を追加（±0.30 秒・0.05 秒刻み、自分の入力への効き方は【要実機確認】） |
| 3bf7c2c6-cc02-81c9-8bc6-ea98fd68d791 | ② プレイヤー入力に対するジャスト判定フロー | 可 | 陳腐化 | 本文置き換え | [原稿](3bf7c2c6-cc02-81c9-8bc6-ea98fd68d791.md) | `RhythmJustService` 経路を `GetCurrentBeatType(out isJust)` → Presenter → Signal に全面書き換え |
| 3bf7c2c6-cc02-8125-a337-cc26fa0cbb3f | ③ リズムアクション予約フロー（Enemy 等の外部モジュールから） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8125-a337-cc26fa0cbb3f.md) | 実行主体を `MusicSyncService.Update` に修正、`MusicSyncSpec` 変換と `LeadNotificationScheduler` を追加 |
| 3957c2c6-cc02-806e-bbe8-e391a7ad8c86 | スキル | 可 | 一部古い | 本文置き換え | [原稿](3957c2c6-cc02-806e-bbe8-e391a7ad8c86.md) | `SkillUseCase`→`SkillUsecase`、Infrastructure（`SkillRepository`）ほか不足 12 型を追加、スキル入力ガイドを追記（BT-12）、攻撃だけがリズムを刻む旨（BT-19） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.1: 本文の説明語をキルコードに。No.19: 回避はリズムを刻まない（【要確認】を外した） |
| 3bf7c2c6-cc02-81aa-9f1b-ef1cacd3f8d5 | ① スキル発動フロー（リズムコマンド入力） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81aa-9f1b-ef1cacd3f8d5.md) | 起点を `PlayerAttackController`→`SkillController` に、クールダウン中の扱い・末尾一致・空撃ち・複数同時発動を修正 → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.1: 本文の説明語をキルコードに。No.19: 回避は起点にならない旨 |
| 3bf7c2c6-cc02-8110-8f4b-d3f8a5738048 | ② 入力進捗の表示フロー | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8110-8f4b-d3f8a5738048.md) | スキル入力ガイド（`SkillGuideProgressView`）の表示規則と色帯・演出値を追加（BT-12） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.1: 本文の説明語をキルコードに → 決定（2026-09-22 八幡）R5: 拍子・色・武器の表を追加（実装の 2 か所はすでに一致） |
| 3957c2c6-cc02-8017-9b5f-c04dd480c816 | プレイヤー | 可 | 一部古い | 本文置き換え | [原稿](3957c2c6-cc02-8017-9b5f-c04dd480c816.md) | 入力バッファの誤記修正（BT-22）、回避がリズムを刻まない旨（BT-19）、`StageSceneObjects`、Container の公開物、武器表示の規則（HD-25/26） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.19: 回避はリズムを刻まない（【要確認】を外した） → 決定（2026-09-22 八幡）R13: 入力履歴は先行入力のために残す（📥 依存しているもの） |
| 3bf7c2c6-cc02-81d8-a0ce-d293848ec481 | ① 通常移動フロー（毎フレーム） | 可 | 陳腐化 | 本文置き換え | [原稿](3bf7c2c6-cc02-81d8-a0ce-d293848ec481.md) | `PlayerView` 主導（入力保持・カメラ補正・`FixedUpdate` 反映）に全面書き換え |
| 3bf7c2c6-cc02-81ad-bf88-ef1cd0770beb | ② 攻撃実行フロー（入力イベント時） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81ad-bf88-ef1cd0770beb.md) | 受け付け条件・判定・攻撃後の表示を追加し、ダメージ計算はキャラ&バトル ① へ → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）R30: 対象がいなければ正面へ直線で撃つ（現状は飛ばない、実装修正） |
| 3bf7c2c6-cc02-81f7-aebb-c3c2d7d22f10 | コンボ表示 | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81f7-aebb-c3c2d7d22f10.md) | 依存の向きを修正（Mission → Combo）、表示閾値 4 を追加（BT-29） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.20: 仕様は 3 以上で表示、現状 4（実装修正） |
| 3bf7c2c6-cc02-81f8-897e-feb08a04a660 | ① コンボ表示の更新フロー | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81f8-897e-feb08a04a660.md) | 呼び出し元を修正、コンボ数が変わる契機の表を追加（BT-29、要企画確認 2 件） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.20: 3 以上で表示（現状 4）。No.21: キルコード発動ではリセットしない（現状と本来の仕様を併記、実装修正）。スキル 5 の HP 消費は【要確認】 |
| 3bf7c2c6-cc02-81c7-9528-ca85aee87fa3 | リズムガイド全画面演出 | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81c7-9528-ca85aee87fa3.md) | `RhythmJustService` 削除、`IPlayerAttackSignal` 購読へ、Vignette の現在値表、オーバーレイの登録元（`CameraInitializer`） |
| 3bf7c2c6-cc02-812b-ac16-d305bb01f5aa | ① 攻撃時の全画面演出フロー | 可 | 陳腐化 | 本文置き換え | [原稿](3bf7c2c6-cc02-812b-ac16-d305bb01f5aa.md) | `RhythmJustService` 経路を Signal 経路に全面書き換え |
| 3bf7c2c6-cc02-81be-a722-ccf86f074347 | キャラクターアニメーション | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81be-a722-ccf86f074347.md) | `Animaiton` 注意書きを削除（CM-18）、遷移ルール・位相同期・クリップ割り当ての 3 節を新設（CM-19〜21） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）R12: BlendTree 化と位相同期は続ける（未実装）。継続するかの【要確認】を外した |
| 3bf7c2c6-cc02-816c-bd45-f9abba361f98 | ① 移動アニメーションのブレンドフロー（毎フレーム） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-816c-bd45-f9abba361f98.md) | 位相同期の仕組みと途中生成の敵のずれを追記（CM-20） |
| 3bf7c2c6-cc02-8138-abda-cbff3308ad44 | ② ワンショット再生のフロー（攻撃・被弾時） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8138-abda-cbff3308ad44.md) | 同一クリップ頭出し・移動キャンセル・回避終了通知を追加（CM-19） |
| 3bf7c2c6-cc02-8184-8715-f2f741dc1d4f | スキル効果 | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8184-8715-f2f741dc1d4f.md) | Target/Battle 依存を修正、`SkillVisual`→`SkillView`、命中時効果のタイミング、スキルごとの演出の現状節を新設（BT-35） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.1: 本文の説明語をキルコードに（節見出しは残し【要確認】） → 決定（2026-09-22 八幡）R77: 同じ Order（440）で問題ないものは許容。実行順は保証されないと明記 |
| 3bf7c2c6-cc02-8133-b82b-de2ef61e9249 | ① 実行器の登録フロー（初期化時） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8133-b82b-de2ef61e9249.md) | 依存の生成と登録される ID（0〜10・13）を追加 → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.1: 本文の説明語をキルコードに |
| 3bf7c2c6-cc02-816a-b33c-da870be1fab5 | ② 命中時効果の付与フロー（伝染の例） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-816a-b33c-da870be1fab5.md) | 命中の通知元を `AttackExecutor` に、`ITargetRadiusQuery`、同じ攻撃で使われ空振りで失われる旨（要企画確認） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.1: 本文の説明語をキルコードに |
| 3bf7c2c6-cc02-8100-8e00-d8124e5c09bc | 入力 | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8100-8e00-d8124e5c09bc.md) | `ReturnToTitleInitializer` 削除、入力履歴は未使用（BT-22）、入力抑止と登録型、依存を更新、入力マップの割り当て表を新設（OG-218） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）R13: `InputBufferingQueue` は先行入力のために残す。R29: スマホの視点操作は画面右半分のスワイプ（現状は全画面、実装修正） → 決定（2026-09-22 八幡）R56: ゲームパッドの決定・キャンセルは設定で切り替え、既定は海外式（決定 South / キャンセル East。未実装 #2068） |
| 3bf7c2c6-cc02-8169-8ade-de45506dca3c | ① 入力の通知フロー | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8169-8ade-de45506dca3c.md) | 入力抑止中の分岐と入力マップの前提を追加 |
| 3bf7c2c6-cc02-8133-85d8-d8821dffb628 | ② 入力履歴の記録フロー | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-8133-85d8-d8821dffb628.md) | 「リズム判定が参照する」を削除（読み手なし）、時刻の取得元を修正（BT-22） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）R13: 先行入力のために残す。残すか削除するかの【要確認】を外した |
| 3bf7c2c6-cc02-819c-a123-d2d22318d23b | 状態効果（バフ・デバフ） | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-819c-a123-d2d22318d23b.md) | Infrastructure のバフ定義アセット 3 型を追加、補正の呼び出し元を修正、スキルごとの再付与方針とバリアの節を新設（BT-37） → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）R32: バフ・デバフの HUD は未実装として残す |
| 3bf7c2c6-cc02-81c7-a04c-c30f7ac31798 | ① ダメージ計算への補正フロー | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81c7-a04c-c30f7ac31798.md) | 呼び出し元を 3 か所に分け、クリティカル倍率の段と使用済み効果の除去を追加 |
| 3bf7c2c6-cc02-81b4-a3d3-dd438788c33f | ② 付与と期限切れのフロー | 可 | 一部古い | 本文置き換え | [原稿](3bf7c2c6-cc02-81b4-a3d3-dd438788c33f.md) | 「毎フレーム」を「呼び出しのたび」に修正、Stack・累積・方針ごとの処理を追加 |

## 実装の修正が必要（決定 2026-09-22 八幡 第 2 弾）

| 決定 | 内容 | 根拠 | ページ |
| --- | --- | --- | --- |
| No.20 | コンボの表示閾値を 4 から 3 にする | `Assets/Scripts/Runtime/6.Composition/InGame/Mission/InGameMissionInitializer.cs:449`、`Assets/Level/Scenes/Master/InGame.unity:57729`、`Assets/Scripts/Runtime/4.View/InGame/Combo/ComboHudView.cs:45` | コンボ表示、同 ① |
| No.21 | 通常攻撃のダメージをスキップするキルコードの発動時に「命中しなかった」と通知してコンボを 0 に戻している | `Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs:147-153` → `Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/MissionProgressRecorderController.cs:232-236` | コンボ表示 ①、キャラ&バトル ① |

## 実装の修正が必要（決定 2026-09-22 八幡 第 3 弾）

| 決定 | 内容 | Issue | 根拠 | ページ |
| --- | --- | --- | --- | --- |
| R2 | ジャスト帯を判定幅に関係なく一定の太さで描く | #2042 | `Assets/Scripts/Runtime/4.View/InGame/Music/ACLikeRhythmGuideView.cs:448` | 音楽 |
| R6 | 攻撃硬直の固定秒をやめ、拍だけで決める。押せない範囲をゲージで灰色にする | #2045 | `Assets/Level/Data/Master/Character/Player.asset:19`、`Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs:66` | キャラ&バトル、同 ① |
| R8 | 命中判定に障害物の判定を入れる | #2046 | `Assets/Scripts/Runtime/3.Adaptor/InGame/Target/TargetAreaQuery.cs:36-115` | キャラ&バトル ① |
| R9 | プレイヤーのパイプラインに確定ダメージを入れる | #2047 | `Assets/Level/Data/Master/InGame/Battle/PlayerAttackPipeline.asset:16-29`、`PlayerAttackSpec.asset:15` | キャラ&バトル、同 ① |
| R12 | ロコモーションを常駐 BlendTree へ移し、途中生成の敵も音楽の位相に合わせる | #2048 | `Assets/Scripts/Runtime/4.View/InGame/Animation/PlayableAnimationController.cs:45-55` | キャラクターアニメーション |
| R29 | スマホの視点操作を画面右半分のスワイプに限る | — | `Assets/Level/Prefabs/Master/InGame/SmartphoneCanvas.prefab:445-463`、`Assets/Scripts/Runtime/4.View/Persistent/Input/MobileInput.cs:151-165` | 入力 |
| R30 | ロックオン対象がいないときに正面へ直線で撃つ | #2055 | `Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs:134-139` | キャラ&バトル ①、プレイヤー ② |

## 反映前に解決すべき要確認（まとめ）

| # | 内容 | 確認先 | ページ |
| --- | --- | --- | --- |
| 1 | ~~回避がリズムを刻まない・アクション種類を見ない現状を仕様とするか（BT-19）~~ → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）No.19 で解決 | — | プレイヤー、スキル、スキル ① |
| 2 | スキル 5 の HP 消費でコンボが切れるのは、被ダメージ（リセットする）とキルコード発動（リセットしない）のどちらに当たるか（BT-29。キルコード発動でのリセットは決定 No.21 で不具合と決まった） | 企画 | コンボ表示 ① |
| 3 | 命中時効果（スキル 2・8）が発動させた攻撃の空振りで失われる | 企画 | スキル効果、スキル効果 ② |
| 4 | ~~スキル 7 の再付与方針（マスター Ignore / コード Replace）~~ → Discord（2026-08-08 企画の回答、進捗共有「バフとスキル改修」）で「新しい値で上書き」＝ Replace と決まっていた。マスター値の修正が残る。バリアの上限と持ち越し（BT-37）は未決 | 企画・プログラム | 状態効果 |
| 5 | 対象がいないときの空撃ち（クールダウン消費）と、到達しない `SkillExecutionFailurePolicy` | プログラム | スキル、スキル ① |
| 6 | スキル 13 のアニメーションキー、スキル 4〜10 の空欄、スキル 2〜10 のプレハブが仮かどうか（BT-35） | 企画・実機・エフェクト担当 | スキル効果 |
| 7 | ~~ロコモーションの BlendTree 化計画の継続~~（→ 決定 R12 で継続と決まった）、`Reserved` 枠の用途、GUID 未解決のクリップ（CM-20・CM-21） | 企画・プログラム・実機 | キャラクターアニメーション |
| 8 | ~~決定 East / キャンセル South の日本式配置を英語圏向けにも維持するか（OG-218）~~ → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）R56 で解決（設定で切り替え、既定は海外式。#2068） | — | 入力 |
| 9 | ~~読み手の無い入力履歴を残すか削除するか（BT-22）~~ → [議事録 2026-09-22 仕様整理](https://app.notion.com/p/3e37c2c6cc0281079e4bec7dd81be5de) 決定（2026-09-22 八幡）R13 で解決（先行入力のために残す） | — | 入力 ② |
| 10 | リズム判定オフセットがプレイヤー自身の入力の判定に効くか（判定が直前の入力との間隔で行われるため。決定 No.11） | 実機 | 音楽 ① |
| 11 | スキル効果の節見出し（「✨ スキルエフェクト演出基盤」ほか 3 つ）の「スキル」もキルコードに直すか（決定 No.1。見出し名は変えない方針で残した） | 企画 | スキル効果 |
| 12 | 確定ダメージ（R9）をダメージ計算の順序のどこに入れるか | 企画 | キャラ&バトル ① |

Discord 照合: 照合済み（2026-09-22、進捗共有「バフとスキル改修」「プランナー進捗共有」「コンボUI」、#プランナー）。上表の 2〜4 について分かったことは次のとおりである。各ページの原稿（状態効果・スキル効果・コンボ表示）はこの領域の担当外のため書き換えておらず、反映時にこの結果を織り込む。
- 4（スキル 7 の再付与方針）: 2026-08-08 に企画（プランナー）が実装担当の質問へ回答し、スキル 7 は「効果時間中の再発動は新しい値で上書き」「効果時間はバフの時間と等しい」「敵の攻撃力はデバフを除いた値を参照する」と決めた（進捗共有「バフとスキル改修」）。コード固定の `Replace` がこの決定と一致し、マスター値 `Ignore` が誤りである。バリアの上限と持ち越しについての決定は見つからない（要確認のまま）
- 4 の関連（スキル 4 のバリアの元になるダメージ）: 2026-08-17 に企画が「スキルで与えたダメージ」と回答した（進捗共有「プランナー進捗共有」）
- 同じ回答で、スキルのモーションは攻撃モーションと重ならない（スキル → 攻撃の順）、エフェクトは重なる、を全スキル共通とした（2026-08-08）。スキル 5 の HP 消費ダメージは会心などの確率抽選を行い、武器倍率はかけない（2026-08-16・08-17、PR #1278）
- 2（スキル 5 の HP 消費とコンボ）・3（命中時効果の空振り）: Discord に該当する検討は無い（要確認のまま）。コンボは被弾と空振りで途切れる実装で始まった（2026-08-09、進捗共有「コンボUI」）
- 2026-09-22 の決定との食い違い: なし
