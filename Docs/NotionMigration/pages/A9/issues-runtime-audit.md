# Runtime 実装監査（2026-07-27）の Issue 化リスト

- 対象: `Docs/RuntimeAudit/00〜18`（2026-07-27、`for-stream` ブランチで調査。842 ファイル・約 6 万行）
- 扱い: Notion のページは作らない。未解消の指摘だけを GitHub Issue にする（90_未決事項 D-06・D-16、02 §2.5「コード品質の監査結果（未解消のもの）も Issue にする」）
- 照合した実装: origin/develop 73fefeb45（worktree HEAD 6663221b9 は 73fefeb45 にドキュメントのコミットを足したもの）。照合日 2026-09-22
- 反映項目: TL-23
- パスは、特に書いていなければ `Assets/Scripts/Runtime/` からの相対パスである。行番号は 73fefeb45 のもの

## 判定

- 照合の範囲: 01〜12 の【確定】【確定・最優先】【最優先】【要検証】の指摘すべて（46 件）と、10 のコード重複（全面コピペ・完全同一ペア・デッドコード）、13〜18 のうち動作に関わる指摘か、数行で直せる指摘（13 の DRY 2 件、14 の到達不能分岐・Update 3 種・未使用ライブラリ、18 の綴り・表記ゆれ・HashCode）。【設計指摘】の一般論（ServiceLocator 定型ブロック 85 箇所、interface 135 件の棚卸し、400 行超のファイルなど）は Issue にしない（下の「Issue にしないもの」）
- 結果: 照合した 46 件のうち、未解消 34 件・一部解消 8 件・解消済み 3 件・対象消滅 1 件。未解消と一部解消（残りの部分）を、同じ箇所・同じ直し方のものはまとめて **27 件の Issue** にした
- 00 の「最優先で対応すべき 10 件」（実際は 11 件）の現状: 解消 1 件（1 位 CameraSystemView.OnDestroy）、一部解消 1 件（9 位 シェーダプロパティ。EnemyRaycastDetectView は直った、TripleShot は残る）、未解消 9 件
- 監査のときより悪化しているもの: `async void` 25 → 26 件、文字列補間つき `Debug.Log` 226 → 302 箇所（1 行の呼び出しだけ数えた場合）、`Usecase`/`UseCase` の混在、NearestAttackPositionSearchView の Raycast の回数（候補全件に Raycast するようになった）、View 層の ServiceLocator 直接参照（1 件増えた）
- 決定（2026-09-22 八幡）: No.13・No.14・No.16・No.22・No.25・No.26 と重なる Issue 案（#1・#4・#14・#20・#24）の「本文の要約」に、決定と、実装修正との関係を書き添えた。Issue 案の数と優先度は変えていない
- 決定（2026-09-22 八幡）: R6・R14・R2・R5 と重なる Issue 案（#4・#14・#8・#9）の「本文の要約」に、作成済みの Issue 番号（#2045・#2004・#2042・#2044）を書き添えた。#14 は既存の Issue #2004 に統合する
- 決定（2026-09-22 八幡）: R109 ラベル `runtime-audit` を作った（作成済み）。本表の 27 件の Issue（#1999〜#2025）に付与済み。「Issue の本文の型（案）」の後ろのラベル案を「ラベル: `runtime-audit`（作成済み・付与済み）」に直した
- 決定（2026-09-22 八幡）: R110 担当は空白にしておく。「要確認」から担当の割り振りを外し、ラベルの行の後ろに「担当は空白にしておく」と書いた
- 決定（2026-09-22 八幡）: R111 設計指摘の一般論は、コード規定に規則として書く（まとめ Issue にはしない）。「Issue にしないもの」の【要確認】を外した（コード規定の原稿は別の領域）
- 作成済みの Issue（本表の # → GitHub Issue）: #1 → #1999、#2 → #2000、#3 → #2001、#4 → #2002、#5 → #2003、#6 → #2005、#7 → #2006、#8 → #2007、#9 → #2008、#10 → #2015、#11 → #2009、#12 → #2010、#13 → #2016、#14 → #2004、#15 → #2017、#16 → #2018、#17 → #2011、#18 → #2019、#19 → #2020、#20 → #2012、#21 → #2021、#22 → #2013、#23 → #2022、#24 → #2014、#25 → #2023、#26 → #2024、#27 → #2025（扱いを決める Issue は #1967）
- 要確認:
  - 優先度の付け方（本表の「優先度」は監査の順位と影響の大きさから付けた案）。ラベル名は決定 R109、担当は決定 R110 で解決
  - #24 に書き添えた決定 No.16（予兆の後は射程外でも攻撃する）の修正が、ボスの攻撃にも要るか（射程外での取り消しがボス側にもあるか。プログラマーへ）
  - `Docs/RuntimeAudit/` を Issue 化の後に削除するか（`Docs/` は gitignore 対象で各自のローカルにしか無い。01 棚卸しでは削除対象）。削除する場合、Issue 本文から監査書へリンクしない

## Issue 一覧

優先度: 高 = 不具合・リークに直結する / 中 = 性能・保守性に効くが今すぐ壊れない / 低 = 表記・整理。

| # | 優先度 | タイトル案 | 本文の要約 | 根拠 file:line（73fefeb45） | 監査 ID |
|---|---|---|---|---|---|
| 1 | 高 | スキルツリーの解放で、解放対象が 0 件でもポイントだけ減る | `OnSkillUnlocked` は解放対象ノードが 0 件のとき `Debug.LogError` を出すが `return` しない。そのまま `BuildUnlockOrder` を空で回し、`ModifyPoint(-_costToUnlock)` でポイントだけ引く。LogError の直後に `return;` を入れる。決定（2026-09-22 八幡）No.22 で先祖ノードの一括解放は仕様どおりと決まったので、`BuildUnlockOrder` による一括解放はそのまま残す | `3.Adaptor/OutGame/SkillTree/SkillTreeController.cs:229-232,234,245` | RA02-1（00 の 4 位） |
| 2 | 高 | スキル解放の保存に失敗しても、プレイヤーには成功したように見える | 保存 `SaveSkillUnlockData` を誰も await せず、`ContinueWith(..., OnlyOnFaulted)` でログを出すだけである。ポイントは直前で減算済み。async メソッドにして try/catch で待ち、失敗したらポイントを戻すかエラーを表示する | `3.Adaptor/OutGame/SkillTree/SkillTreeController.cs:245,251-255` | RA11-3 |
| 3 | 高 | ボスの照準線のマテリアルが複製されたまま破棄されない | `LineRenderer.material` に毎フレームアクセスするため、初回に 3 本分のマテリアルが複製される。OnDestroy が無く、ボスが出現するたびに溜まる。あわせて `"_EmissionColor"` を毎フレーム文字列で引いている。ShellView / EnemyRaycastDetectView と同じく、複製を保持して OnDestroy で Destroy し、`Shader.PropertyToID` をキャッシュする | `4.View/InGame/Enemy/Boss/TripleShotRaycastDetectView.cs:255,260,265`（OnDestroy なし） | RA04-1（00 の 2 位）, RA03-3, RA04-4 の残り |
| 4 | 高 | 攻撃の硬直待ちが、シーンを破棄した後も動き続ける | `UniTask.Delay` に CancellationToken を渡しておらず、`.Forget()` で投げっぱなしのため、シーン破棄後に Entity を書き換える。ステージノードの接続線アニメのタイムアウトも `CancellationToken.None` で、本処理が先に終わっても 5 秒のタイマーが残る。寿命のトークンを渡し、接続線はリンクした CTS を完了時に Cancel する。なお、これは攻撃の間隔の待ちで、クリティカル時の敵の硬直（決定 No.13 で削除することになった処理）とは別である。決定（2026-09-22 八幡）R6 で攻撃硬直を拍だけで決めることになり、同じ待ちを直す Issue #2045 が作成済みである。重ならないよう、#2045 と一緒に直すか先に片付ける | `2.Application/InGame/Battle/AttackIntervalEvaluator.cs:39,50-53`、`4.View/OutGame/StageSelect/StageNodeConnectionView.cs:69` | RA03-1（00 の 5 位）, RA11-2 の残り |
| 5 | 高 | 敵の攻撃位置探索で、候補ごとに GC Alloc と Raycast が走る | `NavMeshPath.corners` は参照のたびに配列を確保するプロパティで、ループ条件と本体で 1 反復 3 回確保する。さらに PathComplete になった全候補に Raycast する（監査時より重い）。探索タイミングも個体でずれない。`GetCornersNonAlloc` にし、直線距離で枝刈りし、タイマーの初期値をずらす | `4.View/InGame/Enemy/NearestAttackPositionSearchView.cs:18,60-90,75,77,103-106` | RA04-2（00 の 3 位）, RA05-3 |
| 6 | 中 | 回避の開始・終了イベントを無名ラムダで購読しており、解除できない | `dodge.OnDodgeStarted` と `OnDodgeEnded` を無名ラムダで購読しているため `-=` できず、OnDestroy でも解除していない。すぐ下の `_onDodgeEndedHandler` と同じくフィールドに入れて購読し、OnDestroy で解除する | `6.Composition/InGame/Player/PlayerInitializer.cs:355,360,459-472` | RA01-2（00 の 8 位） |
| 7 | 中 | ACLikeRhythmGuideViewModel が購読を解除しない | View のイベント 3 件を購読したまま解除しない。後始末は View の OnDestroy での null 代入に頼っている。IDisposable を実装して `-=` し、所有者の破棄時に Dispose を呼ぶ | `4.View/InGame/Music/ACLikeRhythmGuideViewModel.cs:13-15`、`4.View/InGame/Music/ACLikeRhythmGuideView.cs:570-575` | RA01-3 の残り |
| 8 | 中 | リズムガイドの判定ゾーンを毎フレーム作り直している | Presenter が毎フレーム `_zones.Clear()` してから作り直し、受け手の View が毎フレーム全要素を比べて「変わっていない」と捨てている。作るときに `IReadOnlyList` を foreach するため、列挙子のボクシングも毎フレーム起きる。ゾーン定義が変わったときだけ通知する形にし、毎フレームの DTO から外す。同じファイルを、決定（2026-09-22 八幡）R2 のジャスト帯の太さの修正（Issue #2042）でも直す | `3.Adaptor/InGame/Music/RhythmGuidePresenter.cs:57-68,59`、`4.View/InGame/Music/ACLikeRhythmGuideViewModel.cs:39`、`4.View/InGame/Music/ACLikeRhythmGuideView.cs:239-251,981` | RA05-1, RA04-3（00 の 7 位） |
| 9 | 中 | ビート色の区間を毎回線形探索している | `GetBeatSectionIndex` はブロックごとに Just と通常ゾーンを 2 段で線形探索し、`TryGetBeatRange` の while ループの中からも繰り返し呼ばれる。ゾーンを作り直すときに区間の索引を配列で持っておく。同じファイルを、決定（2026-09-22 八幡）R5 の拍の色の修正（Issue #2044）でも直す | `4.View/InGame/Music/ACLikeRhythmGuideView.cs:1129-1155,135,333,339,343` | RA05-2 の残り |
| 10 | 低 | 旧リズムガイド（RhythmGuideView 系）を削除する | 本番の InGame シーンが使うのは ACLike の経路だけである。旧実装は Develop の AnimationTest シーンでだけ使われ、そのシーンでは新旧 2 本が毎フレーム動く。旧 View・UpdateView・Initializer を削除し、AnimationTest シーンを ACLike に置き換える | `4.View/InGame/Music/RhythmGuideUpdateView.cs:59`、`6.Composition/InGame/Music/RhythmGuideInitializer.cs:64`、`Assets/Level/Scenes/Develop/AnimationTest/InGameAnimation.unity` | RA04-5 |
| 11 | 中 | CameraSystemView の更新処理を整理する（Update 3 種・毎フレーム 13 個の null 判定） | `_updateMode` のために FixedUpdate / Update / LateUpdate の 3 つを定義し、2 つは即 return している。Tick では 13 個の依存を毎フレーム null 判定し、欠けていても何も出さずに return する。使うモードに固定し、依存の検証は Initialize で 1 回だけ行ってフラグで判定する | `4.View/InGame/Camera/CameraSystemView.cs:187,258-288,491-500` | RA03-2, RA14-2, RA02-3 |
| 12 | 中 | カメラの追従だけがフレームレートに依存する | 補間係数が `FollowLerpSpeed * DeltaTime` のままで、fps によって収束の速さが変わる。フレーム落ちで係数が 1 を超えると行き過ぎる。同じディレクトリのほかの 3 箇所と同じ指数減衰（`1 - Mathf.Exp(-speed * dt)`）に揃える | `4.View/InGame/Camera/Calculation/CameraFollowCalculator.cs:36` | RA07-2（00 の「費用対効果が高い 3 件」の 3 番目） |
| 13 | 低 | EnemyWaveTimerView が FixedUpdate の中で Time.deltaTime を使っている | FixedUpdate の中では fixedDeltaTime が返るので動作は正しいが、読み手を誤解させる。Update に移すか、`Time.fixedDeltaTime` と書く | `4.View/InGame/Enemy/EnemyWaveTimerView.cs:62,79` | RA03-4 |
| 14 | 高 | 敵の基礎攻撃力 10 が 3 つの UseCase に直書きされている | 敵の通常攻撃・ボスの 3 連射・弾の基礎ダメージが `new Damage(10)` の直書きで、同じ TODO が 3 箇所にある。マスターデータの値（`CharacterEntity.BaseDamage`）が既にあるので、3 つとも `attacker.BaseDamage` を使い、直書きを消す。決定（2026-09-22 八幡）No.14 で敵のステータスの正本はマスターデータと決まったので、この Issue で直書きをなくすとマスターデータの値が効くようになる。決定（2026-09-22 八幡）R14 で「敵の種類・攻撃ごとにマスターデータで設定する」と決まり、既存の Issue #2004 と同じ内容である。新しく作らず #2004 に統合する | `2.Application/InGame/Enemy/EnemyAttackUsecase.cs:50`、`2.Application/InGame/Enemy/Boss/EnemyTripleShotAttackUsecase.cs:49`、`2.Application/InGame/Enemy/ShellAttackUsecase.cs:25`、`1.Domain/InGame/Character/CharacterEntity.cs:79` | RA13-1 |
| 15 | 低 | 照準の原点判定（IsEnemyOrigin）と閾値 0.0001f が 2 クラスに重複している | 実装もマジックナンバーも同じ判定が 2 クラスにある。共通の定数か判定ヘルパーに抜き出す | `4.View/InGame/Enemy/EnemyRaycastDetectView.cs:388-390`、`4.View/InGame/Enemy/Boss/TripleShotRaycastDetectView.cs:334-336` | RA13-2 |
| 16 | 低 | スキル実行の失敗時ポリシーの switch に、実行されない分岐がある | `TARGET_REJECT_POLICY` が `private const` なので、`KeepProgress` と `ResetProgressAndConsumeCooldown` の分岐は実行されない。ResetProgressOnly の処理を直接書くか、設定値にして分岐を使えるようにする | `3.Adaptor/InGame/Skill/SkillExecutionController.cs:17,144-158` | RA14-1 |
| 17 | 中 | 値オブジェクト 9 種に `IEquatable<T>` の宣言を足す | 型付きの `Equals(T)` は実装済みなのに `: IEquatable<T>` を宣言していないため、`EqualityComparer<T>.Default` を通すとボクシングする。9 ファイルに 1 行ずつ足すだけで直る | `1.Domain/InGame/Character/AttackCooldown.cs:8`、`AttackPower.cs:9`、`AttackRangeMax.cs:8`、`AttackRangeMin.cs:8`、`AttackRotationSpeed.cs:8`、`DodgeCooldown.cs:8`、`DodgeDuration.cs:8`、`DodgeSpeed.cs:8`、`MoveSpeed.cs:8` | RA06-3（00 の 10 位、「費用対効果が高い 3 件」の 1 番目） |
| 18 | 低 | 集合の判定を List.Contains で行っている箇所を HashSet にする | 登録・解除の多い音量マネージャ 2 つと InGamePlayDirector、プール、ロード済みシーンの判定が `List<T>.Contains` の線形探索である。音量マネージャと InGamePlayDirector を HashSet にし、プールの Contains は消す（StageClearData はシリアライズの都合で List のまま） | `4.View/Persistent/Music/SoundEffectVolumeManager.cs:18,66`、`4.View/Persistent/Voice/VoiceVolumeManager.cs:18,65`、`6.Composition/InGame/Sequence/InGamePlayDirector.cs:53,62`、`4.View/InGame/Character/ParticleSystemPoolView.cs:28,51`、`4.View/InGame/Scene/IngameSceneView.cs:15,37` | RA06-2, RA05-4 |
| 19 | 低 | スキルツリーのリポジトリの探索方式を揃える | ID 検索は `ScriptableObjectRepositoryBase` の辞書に移ったが、SkillNodePhaseBindDataRepo と FindByName は手書きの線形探索のままである。PhaseBind も基底クラスへ移し、FindByName は呼び出し元を ID 検索に寄せる | `5.InfraStructure/OutGame/SkillTree/SkillNodePhaseBindDataRepo.cs:20-43`、`SkillNodeBindRepo.cs:26-46` | RA06-1 の残り |
| 20 | 中 | `async void` を減らす（現在 26 件） | `async void` の例外は呼び出し元に伝わらず握り潰される。監査の後、IngameComposition.Start とスポナー 4 件は直ったが、スキルツリーのリセット・チュートリアル・復帰処理などで 9 件増えた。起動系の Start 3 件とセーブデータの削除から順に `async UniTaskVoid` + `.Forget()` か Task を返す形にし、新しい `async void` を規約で禁じる。`TitleSceneInitializer.cs:562` はセーブデータのリセット処理で、決定（2026-09-22 八幡）No.25（言語と判定オフセットを残す）の実装修正と同じメソッドなので、あわせて直す。体験版の終了画面のセーブ削除 `Assets/Scripts/Demo/DemoRuntimeBootstrap.cs:689`（Runtime の外、決定 No.26）も `async void` である | 例: `6.Composition/Persistent/PersistentEntryPoint.cs:37`、`6.Composition/OutGame/OutGameSceneInitializer.cs:46`、`6.Composition/OutGame/Scenario/OutGameScenarioSceneInitializer.cs:39`、`6.Composition/OutGame/Title/TitleSceneInitializer.cs:562`、`6.Composition/OutGame/StageSelect/StageSelectInitializer.cs:634`（全件は `git grep -n "async void" -- Assets/Scripts/Runtime`） | RA11-1（00 の 6 位） |
| 21 | 低 | 非同期の書き方（Task / UniTask）を決め、使っていないライブラリを外す | UniTask を使うのは 2 ファイルだけで、`Task.Delay` が 11 箇所、`Task.Yield` が 1 箇所残る（シナリオ系は timeScale を受けない）。ObservableCollections は依存にあるが利用 0 件である。どちらに寄せるかを決め、残す `Task.Delay` には理由を書き、ObservableCollections は外すか使い道を決める | `2.Application/OutGame/Scenario/ScenarioUsecase.cs:190,235`、`3.Adaptor/OutGame/Scenario/TextEventHandler.cs:184`、`FadeEventHandler.cs:42`、`5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs:526`、`Packages/manifest.json:5-7` | RA07-1 の残り, RA14-3 の残り |
| 22 | 中 | Adaptor 層・View 層から ServiceLocator を直接参照している箇所を直す | Adaptor 層の 2 箇所が static に依存を取り出し、BattleSortieSelectionService はコンテナへの登録まで行う。View 層にも 1 件増えた。Composition 層で生成・解決して、コンストラクタか Initialize で渡す | `3.Adaptor/InGame/Skill/SkillUI/SkillCrosshairProgressController.cs:57`、`3.Adaptor/OutGame/StageSelect/BattleSortieSelectionService.cs:45,51,61,67`、`4.View/OutGame/Screen/SettingScreenView.cs:41` | RA08-1 |
| 23 | 低 | 文字列補間つきの Debug.Log がリリースビルドでも評価される | 補間つきの呼び出しが 1 行のものだけで 302 箇所 / 120 ファイルある（改行をまたぐものを含めると 517 箇所 / 167 ファイル）。`[Conditional]` 付きの共通ログを `0.Utility` に置き、件数の多い Composition 系から置き換える | 数え方: `git grep -c -E 'Debug\.Log(Warning\|Error\|Exception\|Assertion)?(Format)?\(\s*\$"' -- Assets/Scripts/Runtime`。既存の Conditional ラッパーは `4.View/OutGame/Navigation/NavigationDebugLog.cs:16` だけ | RA12-1 |
| 24 | 中 | 敵とボスの攻撃予約・AIFacade・BehaviorGraph ノードが丸ごと複製されている | 予約スケジューラ 3 ファイル（計 412 行）、AIFacade（計 458 行）、BehaviorGraph ノード（敵 11 本・ボス 8 本）が敵とボスで並んでいる。片方だけ直して食い違う事故が起きうる。予約処理を共通化し、ノードは Blackboard の型を型引数に取る基底で吸収する。決定（2026-09-22 八幡）No.13（クリティカル時の硬直を削除）と No.16（予兆の後は射程外でも攻撃する）の実装修正は、この Issue の対象の複製と重なる。硬直は `3.Adaptor/InGame/Enemy/EnemyAIController.cs:244-252` と `3.Adaptor/InGame/Enemy/Boss/BossAIController.cs:200-208` に同じ処理があり、両方を消す。射程外での取り消しがボス側にもあるかは確かめていない【要確認: プログラマーへ】。片方だけ直さないよう、この Issue と順番を決める | `2.Application/InGame/Enemy/Boss/BossAttackReservationUsecase.cs:10`、`2.Application/InGame/Enemy/EnemyAttackReservationUsecase.cs:12`、`2.Application/InGame/Enemy/ShellReservationUsecase.cs:13`、`4.View/InGame/Enemy/AIFacade/`、`4.View/InGame/Enemy/Boss/AIFacade/` | RA10-1（00 の 11 位） |
| 25 | 低 | 中身が同じクラスの組を統合する（5 組） | 音量マネージャ（Voice / SoundEffect）、And/Or の Clear/Fail 条件グループ 4 種、OutGame / Persistent の InitializationModuleBase、ScreenViewRegistry / TitleScreenViewRegistry は、型名や数行の違いを除いて同じ実装である。ジェネリック化・共通基底への集約で 1 つにする。DodgeCooldown / DodgeDuration は値オブジェクトなので統合せず、定型部分の生成で対応する | `4.View/Persistent/Voice/VoiceVolumeManager.cs`、`4.View/Persistent/Music/SoundEffectVolumeManager.cs`、`1.Domain/InGame/Mission/ClearCondition/AndClearConditionGroup.cs`・`OrClearConditionGroup.cs`、`1.Domain/InGame/Mission/FailCondition/AndFailConditionGroup.cs`・`OrFailConditionGroup.cs`、`6.Composition/OutGame/Bootstrap/OutGameInitializationModuleBase.cs`、`6.Composition/Persistent/Bootstrap/PersistentInitializationModuleBase.cs`、`6.Composition/OutGame/Screen/ScreenViewRegistry.cs:38-133`、`6.Composition/OutGame/Title/TitleScreenViewRegistry.cs:39-146`、`1.Domain/InGame/Character/DodgeCooldown.cs`・`DodgeDuration.cs` | RA10-2a〜e |
| 26 | 低 | 参照の無い型とコメントアウトされたコードを削除する（5 件） | 実装も参照も無い interface 4 件と、コメントアウトされたメソッド 1 件が残っている（`git grep -w` で自ファイル以外の参照 0 件）。.meta も含めて削除し、Assets/Docs の記述 1 件を直す | `2.Application/InGame/Skill/IViewAction.cs:8`、`2.Application/OutGame/Scenario/IScenarioEventHandler.cs:11`（非ジェネリック版）、`3.Adaptor/OutGame/Scenario/IOutPutPort.Obsolete.cs:7`、`4.View/InGame/Enemy/IShellInitializer.cs:8`、`4.View/OutGame/Setting/SettingBase.cs:58-70` | RA10-3 |
| 27 | 低 | 名前の綴りと表記ゆれを直す（AttackPilpelineAsset・Usecase/UseCase・Repo） | ファイル名とクラス名が `AttackPilpelineAsset` のままで、`AttackPipeline` で検索すると漏れる。`Usecase` 15 件と `UseCase` 10 件が混在し、`SkillUseCase.cs` はクラス名が `SkillUsecase` でファイル名と食い違う。`Repo` が 5 件、`Repository` が 43 件ある。Unity 上でリネームし（GUID は保たれる）、1 PR でまとめて揃える | `5.InfraStructure/InGame/Battle/AttackPilpelineAsset.cs`、`2.Application/InGame/Skill/SkillUseCase.cs:12`、`3.Adaptor/InGame/Skill/SkillExecutionController.cs:163`、`2.Application/InGame/Mission/MissionRuntimeService.cs:182-188`、`2.Application/OutGame/Scenario/ScenarioHandlerRepo.cs`、`5.InfraStructure/InGame/Enemy/Boss/BossAttackEntryRepo.cs`、`5.InfraStructure/OutGame/SkillTree/SkillNodeBindRepo.cs`・`SkillNodeDataRepo.cs`・`SkillNodePhaseBindDataRepo.cs` | RA18-2, RA18-3, RA18-4 |

## Issue の本文の型（案）

各 Issue は、不具合ログの Issue と同じ項目で書く。

```markdown
## 現象
（上表「本文の要約」の 1 文目）

## 根拠
- （上表「根拠 file:line」。73fefeb45 時点の行番号）
- 出典: Runtime 実装監査 2026-07-27（<観点>、監査 ID <RA..>）。行番号は 2026-09-22 に現行コードで確かめ直した

## 直し方の案
（上表「本文の要約」の最後の文）

## 確認方法
- コンパイルが通ること
- （リーク・GC の Issue は）Profiler の Memory / GC Alloc で、対象のシーンを 2 回以上出入りしても増えないこと
```

- ラベル: `runtime-audit`（作成済み。27 件に付与済み）。優先度のラベル（`priority: high` など）は案
- 担当は空白にしておく
- 監査書 `Docs/RuntimeAudit/` は各自のローカルにしか無い（`Docs/` は gitignore 対象）。Issue 本文にはパスではなく、上の「出典」の形で書く

## 解消済み・対象消滅（Issue にしない）

| 監査 ID | 内容 | 現状 | 根拠 |
|---|---|---|---|
| RA01-1 | CameraSystemView.OnDestroy の早期 return で static EventBus の解除が飛ぶ（00 の 1 位） | 解消済み | `4.View/InGame/Camera/CameraSystemView.cs:299-305` で解除が早期 return より前に移った（13a4173b5） |
| RA02-2 | HUDEnemyHealthView.Awake で配列を検証せずに末尾へアクセスする | 対象消滅 | `_sprites` と `RatioToIndex` はクロスヘアの作り直しで削除された（ba4bc1542）。SerializeField の null ガードが無い点は、監査の付録（設計指摘）の範囲 |
| RA18-1 | フォルダ名の綴り誤り（Animaiton） | 解消済み | `git ls-files` で 0 件（6aa560a2d でリネーム） |
| RA18-5 | 攻撃対象の特定に Entity の HashCode を使う | 解消済み | `3.Adaptor/InGame/Battle/PlayerAttackController.cs:380` が `targetEntity.Id` を使う（374a6607b） |
| RA01-3 の一部 | HUDEnemyHealthInitializer の購読解除 | 解消済み | `6.Composition/InGame/UI/HUDEnemyHealthInitializer.cs:66-71`（2d2e68d49）。残りは #7 |
| RA04-4 の一部 | EnemyRaycastDetectView のシェーダプロパティ | 解消済み | `4.View/InGame/Enemy/EnemyRaycastDetectView.cs:134-136,381-382`（31354c14c）。残りは #3 |
| RA11-2 の一部 | EnemySpawnPositionSearcher・ScreenViewBase の待機 | 解消済み・対象消滅 | `4.View/InGame/Enemy/EnemySpawnPositionSearcher.cs:55` にトークンが付いた（83ec9f350）。ScreenViewBase の該当コードは削除（8a5caa194）。残りは #4 |

## Issue にしないもの

- 【設計指摘】の一般論: ServiceLocator 取得の定型ブロック 85 箇所、`FindObjectsByType` 27 箇所、`Camera.main` 7 箇所、静的シングルトン、Composition 層への業務ロジック混入、初期化 Order 値の散在（35 ファイル。決定 R77: 同じ Order で問題ないものは許容し、順序に依存するものは Order を分ける）、裸の数値リテラル、interface 135 件の棚卸し、Controller 名の濫用、400 行超のファイル 21 件・60 行超のメソッド 17 件、3 段チェーン、ログのプレフィックスと context 引数の不統一、ReactiveProperty・LitMotion の未活用など。個別の不具合ではないため、コード規定に規則として書く（決定 R111。まとめ Issue にはしない）
- 監査の付録（`file:line` の全列挙）: 行番号が古く、件数も変わっている。Issue にするときは現行コードで数え直す（上表の #20・#23 は数え直した値）

## 照合で新たに見つかった点

- View 層の ServiceLocator 直接参照が 1 件増えた（`4.View/OutGame/Screen/SettingScreenView.cs:41`）。#22 に含めた
- `2.Application/InGame/Skill/SkillUseCase.cs` はファイル名とクラス名（`SkillUsecase`）が食い違う。コード規定「1 ファイルに公開型は 1 つとし、ファイル名を型名と一致させる」（RL-04）に反する。#27 に含めた
- `CancellationToken.None` を明示的に渡す箇所がほかにもある（`3.Adaptor/InGame/Result/StageResultController.cs:51,78,85`、`2.Application/OutGame/Scenario/ScenarioUsecase.cs:99`）。シーンのアンロードや完了通知なので意図的な可能性が高く、Issue にはしていない【要確認: プログラマーに意図を確認】
