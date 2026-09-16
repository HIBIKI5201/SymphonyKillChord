# AI QAツールとQAシートの対応

## 作業状態

- 提出ブランチ: agent/ai-qa-json-api。引き継ぎ元: agent/ai-debug-attack-queue（0d555a829）。実装コミット: 8e40a0c44。
- 2026-09-15、PA24_八幡拓音さんの「QAシートに必要なツール」「実装していいよ」で実装開始。
- 会話上の段階: 実装・起動確認後の調整。人間の実装承認を根拠にCcをimplementation revision 2へ更新。Ccの登録APIがプロジェクト名から識別できる .wt-SymphonyKillChord-ai-qa を用意し、repo/branchの一致を確認した。
- PA24_八幡拓音さんの「プッシュとDraftPRして」「適切なブランチにプッシュして」を受け、agent/ai-qa-json-apiをpushし、Issue #1571に関連するdevelop向けDraft PRへ提出する。コンパイル・PlayModeの必須チェックは未確認のまま明示する。
- Runtime・シーン・Prefab・マスターデータは変更しない。Editor C#と共有JSで実装。
- 外部資料は初期の範囲制限に従い未読だったが、その後の人間の許可によりCcフックと登録手順を参照した。共通メモリは未読。
- 人間のUnity起動指示を受け、2026-09-15に作業用worktreeをUnity 6000.3.10f1で起動。ライセンス認証成功。初回コンパイルは依存ライブラリ未解決で失敗（Logs/qa-editor-startup.log）。ZStringでUnsafe未解決（CS0103）、既存RuntimeでR3未解決（CS0246）を確認。Assets/Settings/Nuget/packages.configには両方の依存宣言があり、既存CIにもUnity起動前のNuGet復元処理がある。QAツールのコンパイル確認は未完了。PlayMode、実機、15分走行は未実施。今回の提出ではテスト起動・mergeは行わない。

## 正本

- [CBT QAシート](CBT_QAシート_2026-09-06.md)。§15の未確定・旧仕様との食い違いを保持する。
- [PC版QAシート](PC版_QAシート_2026-09-07.md)。ゲームロジックはCBT §3～§10を流用する。

QA項目や合否条件を書き換えない。ツール実行成功、状態取得成功、QA合格は別である。--qa CBT:4-3 のように元シートの行IDを証跡へ付ける。

## 対応表

| QA項目 | 使用する機能 | 確認方法と残る検証 |
|---|---|---|
| CBT 2-6～2-7、3章 | snapshotのscene/ui/save.tutorialPhase、button | 遷移前後を保存。説明表示・学習転移は画像と人による確認 |
| CBT 4-1～4-4、4-8、4-10 | 攻撃キュー、rhythm履歴、monitorのattackJudgment | 6拍種の成立結果を確認。回避は実入力または既存入力ツールで操作し履歴を読む |
| CBT 4-5～4-7、4-9 | 入力履歴、playerDamage、attackJudgment | 初回・長い待機・被弾・硬直条件を準備して観測。primeは余分な基準攻撃を入れるため、このケースの操作には使用しない |
| CBT 5-1～5-4、5-6、5-10 | targets/player、attack、enemyDamage | 対象HP差分・武器名・命中・ダメージを照合。遮蔽物配置は画像で確認。5-10の式未確定を維持 |
| CBT 5-8～5-9、5-11～5-14 | mission.actions、player、targets、スクショ | 敵攻撃数・死亡と終了理由を確認。予告・見た目・揺れは画像/動画 |
| CBT 6-1～6-6 | skillイベント、enemyDamage、player/targets、rhythm | スキルIDの成立と効果を別に確認。バフ倍率や未公開の条件内部値を推測しない |
| CBT 6-7～6-10、9-6～9-12 | save装備順/解放ID、UIテキスト/ボタン | 編成・研究をUIで操作し前後差分を保存。個別レベル・装備枠数は表示と実操作で確認 |
| CBT 7章 | rhythm音源時刻/BPM、UI、画像/音声 | 時刻の進行を観測。BGM cue、phrase、実際に聞こえる音やBluetooth遅延は音声・実機確認 |
| CBT 8-1～8-8、8-10 | mission終了状態、UI、save前後差分、button | 勝敗・結果表示・報酬/再出撃前後を保存。戻り先を毎回記録。永続化は再起動後に確認 |
| CBT 9-1～9-5、9-13～9-15 | UI一覧、button、scene、スクショ | 通常ボタンで移動しStage_03制限等を確認。Button.onClickを物理クリックの証拠にしない |
| CBT 10-1～10-5 | saveキャッシュスナップショット | 再起動前後を比較。キャッシュがあるだけで保存成功としない |
| CBT 11-1～11-3、11-10 | sequence、gameTime/timeScale、UI、button | ポーズ、再開カウントダウン、ロード表示を観測。Editor一時停止で代用しない |
| CBT 12-3～12-4 / PC 6-3～6-4 | run --seconds 900、monitor | エラー・メモリ時系列。Editorでの事前検証。15分満了前にミッション終了した場合は未達成 |
| CBT 12-1～12-2、12-6 / PC 6-1～6-2、6-5～6-6 | monitor、snapshot、UI再出撃 | UIから5回再入場し区間を比較。製品FPS/3秒ロードの合否は実ビルドで測る |
| CBT 13章 / PC 9章 | UIテキスト、スクショ、既存入力ツール | 仮文言と画像を照合。文字切れ・読みやすさ・D&D・ヒット領域は別途確認 |

### Editorのみでは完了できない項目

CBTのインストール/ストア掲載/法務、端末中断/復帰、熱/電池/ハードウェア、強制終了/破損復旧/暗号化、およびPCの配布/Windowsウィンドウ/実入力デバイス/OSフォーカス/音声出力/Steamは、元シートどおり実機・製品ビルドまたは人が担当する。本ツールはこれらを実行済み・合格にしない。

## 操作入口

共有手順: [SKILL.md](../../.agents/skills/ai-debug-attack-queue/SKILL.md)。取得項目と制限: [JSON API](../../.agents/skills/ai-debug-attack-queue/references/json-api.md)。

```powershell
# 現在の状態をJSONで取得
node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-qa.mjs snapshot

# 六拍種を2回、最大120秒
node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-qa.mjs run --qa CBT:4-3 --seconds 120 --repeat 2 --queue "purple:1,blue:1,cyan:1,green:1,yellow:1,orange:1"

# 15分観測。入力は別途通常のプレイで行う
node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-qa.mjs run --qa PC:6-3 --seconds 900
```

上記は利用時の例であり、この実装セッションでの実行記録ではない。

## 受入確認（実行承認のある環境で実施）

この実装での静的確認: JS 5ファイルのnode --check成功、変更済み追跡ファイルのgit diff --check成功、追加C#の.meta存在確認、asmdef JSON解析とEditor限定設定確認、Runtime/Packages/ProjectSettings差分なし。これらはUnityコンパイルや動作テストを代替しない。

- Editor C#コンパイル。EditModeではavailable:falseになること。
- タイトル→戦闘→リザルト→ホームでframe・シーン・HP・ミッション・装備順を取得できること。
- 読み取り前後でセーブへの書き込みやゲーム状態の変更が起きないこと。
- 六拍種入力でキュー完了・実際のJust・スキル発動・命中の証拠を区別できること。
- 不正指定・二重実行・同一ID再送・他IDcancel・実マウス押下・期限・PlayMode終了・Reloadを確認。
- Ctrl+C・通信切断・API拒否で証跡が残り、所有する入力と購読だけが解除されること。
- 15分観測・5回再入場でログ/イベント/メモリの上限と欠落表示を確認。

計測限界: 約1秒区間のFPS、Editor全体のUnity割当メモリ、最大128イベント、最大32警告/エラー。欠落区間を完全な証跡として扱わない。
