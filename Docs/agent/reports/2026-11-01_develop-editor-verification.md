# develop に入った実装の Unity エディタ確認（2026-11-01）

## 目的と範囲

- 2026-10-30〜11-01 に develop へマージされた PR を、PC の Unity エディタで確かめられる範囲で確認した。
- 対象: develop `89a2c497b`（#2690 マージ時点）。メインクローンで確認し、worktree は使っていない。
- 確認方法: Unity 6000.3.10f1 を `uloop launch` で起動し、コンパイル、EditMode テスト、プレハブ・シーン・データの静的確認、PlayMode（裏世界ハブ経由の戦闘）を行った。
- 裏世界を開くため、確認中だけアクティブな Build Profile を `Release_Dev_Windows` に切り替え、最後に `Demo_Master_Windows` に戻した。
- 入力の注入は、Unity にフォーカスが渡らず（Mouse デバイスが無効）使えなかった。攻撃入力が要る確認は、Presenter・ViewModel を直接呼ぶ形に置き換えた。

## 全体の結果

| 項目 | 結果 |
| --- | --- |
| コンパイル（Demo_Master / Release_Dev の両プロファイル） | エラー 0 |
| EditMode テスト（`KillChord.Tests.EditMode`） | 63 件すべて成功 |
| EditMode テスト（全体） | 208 成功・26 失敗。失敗はすべて gitignore 対象の `Assets/AssetStoreTools/VRM Inporter`（UniGLTF）で、プロジェクトのコードとは関係しない |

## PR ごとの結果

| PR | 内容 | 確認したこと | 結果 |
| --- | --- | --- | --- |
| #2632 | 攻撃入力のクオンタイズ | `MusicTimingCalculatorTests`・`MusicSyncServiceTests` を含む EditMode テスト | 合格 |
| #2634 | ポーズ画面「ゲームに戻る」・スマホのポーズボタン | PlayMode でポーズを開き、「ゲームに戻る」を押すと閉じて `timeScale` が 1 に戻る。プレハブ・シーンに参照切れなし | 合格（スマホのボタンは PC では生成されないため未確認） |
| #2636 | 裏世界の入口とハブ | タイトルに「裏世界」ボタンが出る。F12 でハブが開く | 合格 |
| #2640 / #2655 | ステージ報酬・ステージ05〜09・ステージツリー | `StageTreeAsset` の参照 46 件に欠けなし | 合格（静的のみ） |
| #2641 | セーブのバージョン・予備・復旧 | `SaveDataPayloadFormatTests` を含む EditMode テスト | 合格 |
| #2645 | 被弾時のゲームパッド振動 | InGameCanvas に参照切れなし | 静的のみ。ゲームパッドが無く振動は未確認 |
| #2646 | 改造画面の収集率表示 | UXML の参照先が存在する | 静的のみ。改造画面は開いていない |
| #2652 | UniTask への統一 | 戦闘・シーン遷移を通しで動かして例外なし | 合格（下の「既存の不具合」を除く） |
| #2654 | 未使用コードの削除 | コンパイル | 合格 |
| #2665 | Just スローと金色のダメージ数値 | `PlayJustSlowMotion()` を呼ぶと、プレイヤーの再生倍率が 0.5 で約 0.3 秒続き、その後 1.94 倍で取り戻す。敵テストで出た歩兵 3 体にも ViewModel が渡っている。`DamageView.prefab` の Just 色は差分どおり | 合格（実際の Just 入力からの発火と見た目は未確認） |
| #2666 | HP バーの赤い残像と白い点滅 | ボステストで被弾後、HP 80 のときにバーの右端に赤い残像が見えた | 合格（点滅の見た目は未確認） |
| #2667 | 裏世界のプレイヤー操作テスト・敵テスト | 操作テストは敵 0 体・HP 100 のまま。敵テストは歩兵 3 体が出る。リザルトの「完了」でハブへ戻る。ポーズ「タイトルに戻る」でタイトルへ戻る | 合格（「リトライ」は下の不具合で失敗） |
| #2668 | マスターテキストの英語表示 | `MasterText_en` / `_ja` とも 158 件、空欄なし、Addressables 登録あり | 合格（言語切替の画面確認はしていない） |
| #2670 | 裏世界のボステスト | ボス（HP 2500）が 1 体だけ出る | 合格 |
| #2672 | 攻撃パイプラインの meta の衝突マーカー | コンパイル・アセット読込 | 合格 |
| #2677 | クリアボイスの Cue | `VOICE_Symphony_Gameclear1〜3` が `StreamingAssets/CueSheet_0.acb` にある | 合格（クリア時の再生は未確認） |
| #2679 | ステージ演出カタログ | アセットが読め、Addressables `GameData.Shared` に登録。中身 0 件は PR の意図どおり | 合格 |
| #2682 | StageSequenceMusicView の削除 | InGame.unity に削除したスクリプトの GUID が残っていない | 合格 |
| #2684 | アセット名の綴り修正 | 改名 48 ファイル（アセット 24・.meta 24）のうち .meta はすべて中身が無変更（GUID 維持） | 合格 |
| #2688 | 開発用の残置物の削除 | OutGame.unity・Addressables から外れている | 合格 |
| #2663 / #2664 / #2675 / #2686 / #2690 | ツール・資料のみ | エディタでの確認対象外（#2663 はコンパイルのみ） | 対象外 |

## 見つかった不具合

### リザルトの「リトライ」で戦闘が再開しない（今回の PR 以前から）

- 再現: 裏世界の敵テストで負け、リザルトの「リトライ」を押す。InGame と Stage_01 が閉じたまま、Persistent だけが残る。
- ログ: `OperationCanceledException`。`StageResultView.OnRetryButtonClicked` が `destroyCancellationToken` を `RetryAsync` に渡しており、遷移中に InGame シーンを閉じたときにこの View が破棄され、再読み込みが取り消される（`SceneTransitionService.ReloadSceneAsync` 271 行）。
- 「完了」も同じ形で同じ例外を出すが、こちらは遷移が先に進むのでハブへは戻れる。
- 由来: トークンを渡すようにしたのは 2026-10-20 の #1967（`2643f57e5`）。今回確認した PR では変わっていない。本番の流れでもコード経路は同じだが、本番ステージからのリトライは試していない。

## 既存の気付き（今回の PR とは関係しない）

- `InGame.unity` の `Canvases/StageSequenceCanvas` に Missing Script が 1 つある。2026-08-03 に削除したカメラ演出 View（GUID `603da19df244fb94384fe63c8c703fec`）の残り。
- `InGame.unity` に `EnemyArtillery(Clone)` が 2 体保存されている（2026-08〜09 のコミットから）。
- UXML・USS の画像参照 7 件（`UI/WIP/UI_btn_square.png` など）が、パスも GUID も解決しない。2026-09-26 の #2082 から。
- `ConfirmedDamage` が `[SerializeReference]` で使われているのに `[Serializable]` が無いという警告が出る。

## 確認作業の副作用と後始末

- Build Profile を切り替えると、Addressables の `GameData.Demo` / `GameData.Release` の IncludeInBuild が入れ替わった。`GameDataVariantBuildSettings.ApplyActiveProfile()` で Demo に戻した。
- PlayMode で TMP の動的フォント 3 件と `ProjectSettings.asset`（改行コードのみ）が書き換わったので、確認前の状態に戻した。
- 確認後の `git status` は確認前と同じ。Unity は終了済み。

## 未確認

- 実際の攻撃入力（Just 判定からのスロー発火、クオンタイズの手触り）、ゲームパッドの振動、クリアボイスの再生、改造画面の収集率、言語を英語に切り替えた画面。
