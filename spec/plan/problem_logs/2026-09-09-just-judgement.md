# ジャスト判定の経路不一致

- Date: 2026-09-09
- Status: fixed in working tree
- Area: InGame Music / Battle / RhythmGuide / PostEffect
- Severity: 攻撃・スキルのジャスト補正と画面演出が一致しない。

## Summary

フォーラム依頼「[SS] ジャスト判定の修正」。複数経路のジャスト判定を統一し、ビューとロジックで同じ結果を使う。

## Evidence

- `MusicSyncService.GetCurrentBeatType` は拍種の広い判定範囲内ならコールバックでジャストフラグを立てる。
- `RhythmJustService.IsJustHit` は読取り時にフラグを消費する。
- `ACLikeRhythmGuideView` は独自に計算したブロック番号でジャストを判定する。
- `RhythmGuidePostEffectPresenter` は攻撃後の画面状態を参照する。入力履歴はそれより前に `SkillController.TryExecuteSkill` で更新される。
- 現行 `InGameCanvas.prefab` は幅10・スケール5・全長120、ガイド全長1.5小節。60ブロックとなり、1ブロックは0.025小節に相当する。

## Regression Context

既知の修正からの再発かどうかは未確認。複数箇所に判定条件が存在することはソースで確認済み。

## Cause

拍種分類と狭いジャスト範囲が混同され、入力結果も共有されていない。UIレイアウトと描画タイミングが演出判定を左右する。

## Fix Requirements

- ジャスト範囲を既存のドメイン定義・マスターアセットに集約する。
- ジャスト範囲は現行ガイドの位置・幅を引き継ぎ、開始を含み終了を含まない。
- ジャスト範囲と拍種分類の境界が重なる場合はジャスト範囲の拍種を優先し、マーカーと攻撃拍種を一致させる。
- 判定の読取りは副作用を持たず、入力履歴更新前の結果を攻撃・スキル・多段ヒット・演出へ渡す。
- 履歴なし・拍オフセット前は非ジャストとする。1小節後はクランプ前の経過を評価し、ジャストが永続しないようにする。

## Verification

ユーザー指示によりコンパイル・単体・統合・PlayMode・起動テストは実行しない。差分と全参照を静的に確認する。
将来の回帰確認項目: 6拍種の開始/終了境界、繰返し読取り、初回入力、1小節超過、巻戻し、空振り、スキル発動、多段ヒット、ガイド更新順とレイアウト変更。

静的確認結果:

- `GetCurrentBeatType(out bool)` の呼出元は攻撃ControllerとガイドPresenterへ統一。副作用を持つ `RhythmJustService` は削除した。
- 攻撃Controllerは入力履歴更新前に判定し、スキル・ダメージ・多段ヒット保持値・`PlayerAttackPresenter` に同じboolを渡す。PresenterがDTOへ変換し、DIされたView層の `PlayerAttackSignal` から全画面演出へ伝える。
- 変更したイベントの購読・解除とミッション側ハンドラを確認した。
- ガイドが個別にアセットをロードする経路を削除し、MusicSyncServiceの判定定義をDTO経由で共有する。マーカー幅を別設定する未使用項目も削除した。
- マスターの判定アセットはリポジトリ内で1件。全6拍種へジャスト範囲を追加した。
- 削除スクリプトのGUIDや変更メソッドへのシリアライズ参照は検索で見つからなかった。
- `git diff --check` で空白エラーなし。コンパイル成功や実機動作を確認したという意味ではない。

移行したジャスト範囲（直前の入力からの経過 / 小節長）:

| 拍種 | 開始（含む） | 終了（含まない） |
| --- | --- | --- |
| 8 | 0.125 | 0.15 |
| 6 | 0.15 | 0.175 |
| 4 | 0.25 | 0.275 |
| 3 | 0.325 | 0.35 |
| 2 | 0.5 | 0.525 |
| 1 | 1 | 1.025 |

8拍の末尾で既存の通常拍種範囲と重なる区間は、ジャストマーカーに合わせて8拍を返す。設定値の変更箇所は `RhythmJudgmentDefinitionAsset` の `JustStartNormalized` / `JustEndNormalized` とする。

## Follow-up

実機でのジャスト補正とガイド・全画面演出の一致は未確認としてPRに記載する。

## PR #1512 レビュー対応（2026-09-09）

- 指摘: View通知はPresenterの責務であり、View向けイベントはDIされたSignalに置く。
- Controllerの `OnAttackBeatExecuted` は従来のミッション記録専用の拍種通知へ戻す。Viewのジャスト成否通知は `PlayerAttackPresenter` → `IPlayerAttackSignal` / `PlayerAttackSignal` に分離する。
- ヒットごとのダメージ表示を担当する `AttackResultPresenter` と、入力1回の攻撃成立を表示へ伝える `PlayerAttackPresenter` を分け、空振り・スキルによる通常攻撃スキップ・多段ヒットでも演出通知は入力1回につき1回とする。
- `PlayerInitializer` がSignalを生成してPresenterへDIし、既存の `PlayerModuleContainer` で公開する。ガイド初期化側はControllerでなくSignalを注入する。
- Signalの破棄とPresenterの購読解除をそれぞれ所有元の終了処理で行う。コンパイル・動作テストはユーザー指定により未実施。
