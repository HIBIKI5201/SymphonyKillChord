# Editor QA JSON API v1

窓口は KillChord.Editor.AIDebugPlay.AIDebugQaApi。既存uloopの execute-dynamic-code で呼ぶ。専用HTTPサーバー、RuntimeのAPI、ビルドへの組み込みは追加しない。

## スナップショット

GetSnapshotJson(bool includeUi = true) は同一のEditor呼び出し内で値をコピーする。Unityメインスレッドから呼ぶ。

- メタデータ: schemaVersion=1、success、environment=UnityEditor、projectPath、capturedAtUtc、frame、editorTimeSeconds、gameTimeSeconds、isPlaying、isEditorPaused、timeScale、activeScene、scenes。
- sections内の各項目: {available:true,value:{...}} または {available:false,reason:"..."}。
- successは応答取得を表す。QA合格や全セクションの取得成功ではない。

| セクション | 主な値 | 制限 |
|---|---|---|
| player | ID、HP、最大HP、バリア、死亡、無敵、位置、攻撃硬直・クールダウン、入力抑制 | Controller未初期化の値はnull |
| rhythm | BPM、音源時刻、現在拍、Just範囲、判定定義、拍種・時刻・Attack/Dodge履歴 | 既存リングバッファ範囲。isJustWindowAtCaptureは取得時点の窓であり過去入力の判定ではない |
| mission | 名称、目標文、経過秒、目標段階、終了理由、撃破、被ダメージ、コンボ、行動種別別回数 | endReasonとisFinishedで勝敗・進行中を区別 |
| targets | 現在対象ID、登録対象の位置・生存・HP | 最大256件。totalとtruncatedを確認 |
| sequence | isBattlePaused | Editor一時停止とは別 |
| save | 装備IDの順序、研究ポイント、解放ID、クリアステージと評価ID、チュートリアル段階、音量 | 読み込み済みキャッシュのコピー。ディスク保存・暗号化・強制終了復旧は未検証。個別スキルレベルはこの公開データに存在しない |
| ui | ボタンID・階層パス・操作可否、TMP/uGUIテキスト | 各一覧最大256件、各テキスト2048文字。遮蔽・Canvas alpha・画面外・クリック位置は判定しない |
| observedCombat | runId、記録中か、イベント列と件数 | monitor開始以後のみ。停止後は最後の観測履歴を返す |

observedCombat.value.recentEvents:

- attackBeat: 成立した拍種。
- attackJudgment: 実際の成立Signalが通知した拍数とJust成否。
- attack: 武器名と初撃の命中有無。多段ヒット全体の命中数ではない。
- skill: 実行が成立したスキルID。効果成功はHP・ダメージ等で別途確認。
- enemyDamage: 対象ID、ダメージ、クリティカル、Just、攻撃種別。
- playerDamage: プレイヤー被ダメージ。

最大128イベント。総sequence・破棄件数・種別別件数を保持する。ポーリング間に上限を超えると全イベントは復元できない。欠落を成功扱いしない。

## 観測

AIDebugQaMonitor.Start(runId, durationSeconds)、GetStatusJson()、Stop(runId)を公開。UUIDと1～3600秒を指定し、同時観測と他IDの停止を拒否する。

約1秒ごとのゲームフレーム差分÷Editor実時間を計測する。minimumSampleFpsは区間FPSの最小値であり、最も遅い1フレームの逆数ではない。Editor一時停止中は計測から除外するが、期限は実時間で進む。ゲーム内ポーズはスナップショットで別途記録する。

メモリはProfiler.GetTotalAllocatedMemoryLong()。Editor全体のUnity割当であり、OSプロセスメモリでも製品ビルドの使用量でもない。JSON取得自体の負荷も含む。製品の60/30 FPS基準をこの値だけで合格にしない。

警告/エラーは累計と直近32件（メッセージ・スタック各2048文字）を保存する。Assembly Reload・PlayMode終了はInterrupted。リロード後はSessionStateの直前結果を返し、自動再開しない。

## 操作と結果

buttonはButton.onClickの発行。completionVerifiedとinputDeviceVerifiedはfalse。操作後の状態を別途読む。状態取得APIに操作を混ぜない。

runは標準出力にJSON、診断を標準エラーへ出す。通信/API拒否・観測失敗・キャンセル・後始末失敗では非ゼロ終了。終了コード0もQA合格ではない。summary.jsonのqaVerdictは常にUnverified。

リズムの時間制御はEditor内で行う。Nodeの待機は観測頻度だけに使用する。[uloop公式v3移行資料](https://github.com/hatayama/unity-cli-loop/blob/main/Packages/src/Documentation~/whats-new-v3.md)も参照。
