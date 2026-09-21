# ローカライズ

- page_id: （新規。親ページ: **仕様概要** 27d7c2c6-cc02-819a-9547-f211f355dfec、DB「仕様リスト」27d7c2c6-cc02-8131-aee3-cf5d08b0b03a の行として作る）
- Notion パス: Symphony Kill Chord / **仕様概要** / ローカライズ（新規）
- スナップショットの最終更新: —（新規ページ）
- 書き込み許可: 可（親ページの仕様概要が許可範囲の中）
- 処置: 新規作成
- DB のプロパティ: サマリー「表示言語（日本語・英語）の対応範囲と翻訳テーブルについて」 / カテゴリー「全体」

## 判定

- 信頼性: 新規 — Notion のスナップショット（1,078 ページ）に Localization・`UICommon`・`LocalizedElementText` の記述は 0 件だった（OG-214、TU-14）。本文は現行の実装（確認したコミット 73fefeb45）と PR #1554・#1667・#1673・#1701・#1743・#1756・#1783・#1805 の本文から書いた。最終確認日 2026-09-22
- 整合性:
  - コンフィグ（3337c2c6-cc02-8090-b06c-d9617104076d）は「言語設定」を「重要な、時間がかかる項目」としてアウトゲームにまとめるとしている。実装もホームの環境設定とタイトルのメニューだけにあり、矛盾しない
  - 設定画面（3437c2c6-cc02-807b-a886-c75657af8359）に言語の項目が無い。A1 の設定画面の原稿（OG-212）から本ページへリンクする前提で書いた
  - 用語リスト（27d7c2c6-cc02-813b-a93f-e6baf4d4f679）の「英名」が英訳の元になっている（#1667 の本文「Notion 用語集の英名を踏まえて作成」）。一方で「解放ポイント／研究ポイント」は 1 語扱いのままで、英語テーブルの仮訳の原因になっている（OG-215）。決定 No.3 で「解放ポイント」に揃える
  - 実装との矛盾は無い。英語テーブルに日本語が残るのは、#1729 の方針「英訳が無い場合は英語テーブルでも日本語を使う」による意図的な仮置きである（#1783 本文）
  - TU-14 の「アセットに書いた日本語へフォールバックする」は、UI では `LocalizedElementText` の `fallback` 引数（UXML・C# に書いた日本語）にあたる。実装と一致する
  - #1743 の時点で保留だった「素材待ちのチュートリアル画像 10 行」は #1756 で解消済み（英語用の `*_en.png` 10 枚が `TutorialPopupImages_en` に登録されている）。本文では解消済みとして書いた
- 変更点:
  - 新規作成。仕様リストの既存ページ（コンフィグ・セーブデータ・設定画面）と同じく「仕様概要（関連ページ・関連用語）→ 仕様意図 → 詳細説明」の節にした
  - 「詳細説明」に、対応言語 / 言語の切り替え / 翻訳テーブル / 訳語の決め方 / 英語テーブルに日本語が残っている項目 / ローカライズしていない表示 の 6 節を置いた
  - 数値が並ぶ箇所（テーブルごとのキー数、未翻訳キー）は表にした
- 織り込んだ反映項目: OG-214, OG-212（言語設定の事実部分）, TU-14, TU-08・TU-11・DM-07・TU-06（ローカライズしていない表示の列挙のみ）, OG-217（Tips が日本語だけである点）, OG-215（英訳の仮置きと用語のゆれの関係のみ）, IF-03（初期化失敗時の復帰画面の文言）, HD-13（演出メッセージとリザルト見出しの英語固定）
- 出典:
  - 実装: `Assets/Localization/LocalizationSettings.asset:15-47`（起動時のロケール選択）、`Assets/Localization/Locales/Japanese.asset:16`・`English.asset:16`、`Assets/Localization/Tables/`（3 テーブル。キー数は Shared Data の `m_Key` を数えた）、`Assets/Localization/Tables/UICommon_en.asset:329-346`、`Packages/manifest.json:28`（com.unity.localization 1.5.9）
  - 実装: `Assets/Scripts/Runtime/1.Domain/Persistent/Savedata/GameLanguage.cs`、`Assets/Scripts/Runtime/3.Adaptor/Persistent/Environment/EnvironmentSettingsController.cs:130,243,219-222,298-303`、`Assets/Scripts/Runtime/4.View/Persistent/Environment/LanguageApplier.cs:16-27`、`Assets/Scripts/Runtime/4.View/Persistent/Localization/LocalizedElementText.cs`、`LocalizationInitializer.cs`、`Assets/Scripts/Runtime/4.View/OutGame/Title/LanguageSettingsTabView.cs`
  - 実装（ローカライズしていない表示）: `Assets/Level/Data/Master/InGame/LoadingTipsConfig.asset:16-`、`Assets/Scripts/Demo/DemoTimerView.cs:99-110`、`Assets/Scripts/Runtime/6.Composition/OutGame/Tutorial/OutGameTutorialInitializer.cs:28`、`Assets/Scripts/Runtime/4.View/InGame/Result/StageResultView.cs:218-222`、`Assets/Scripts/Runtime/4.View/InGame/Result/StageResultMissionItemView.cs:56-59`、`Assets/Level/Scenes/Master/InGame.unity:2964,22670,39935,139009`、`Assets/Scripts/Runtime/4.View/InGame/Sequence/StageSequenceMessageView.cs:81`、`Assets/Scripts/Runtime/4.View/OutGame/SkillBuild/SkillGenreFilterBarView.cs:23`、`Assets/Scripts/Runtime/6.Composition/OutGame/OutGameSceneInitializer.cs:233`、`Assets/Scripts/Runtime/4.View/Persistent/Load/EventNotificationView.cs:112-128`
  - Editor: `Assets/Editor/Scripts/Localization/TutorialLocalizationSetup.cs:97`、`UICommonLocalizationSetup.cs:117`、`Assets/Editor/Scripts/Addressables/AddressableKeyValidator.cs`
  - PR: #1554（Localization 導入）、#1667（UICommon と LocalizedElementText）、#1673（タイトルの言語設定）、#1701（言語名の表示）、#1743（Workbook の確定訳）、#1756（チュートリアル画像の日英）、#1783（残りの UI）、#1805（初期化待ち）
  - 議事録: シナリオ部門制作会議（33e7c2c6-cc02-80db-b350-dd2c211a179a）「ローカライズに関して: シナリオ本文・フレーバーテキストは余裕が出てきたら」
- 決定（2026-09-22 八幡）: No.3 用語は「解放ポイント」「解放P」に揃える。「### 英語テーブルに日本語が残っている項目」の表は実 UI 文言の引用なので「研究ポイント」「研究P」のまま残し、表の後ろに直す方針を書いた。同じ節の【要確認】から「どちらに揃えるか」を外した
- 決定（2026-09-22 八幡）: No.25・No.26 タイトル画面のセーブデータのリセットと、体験版の終了画面（DemoEnd）でのセーブデータ削除では、選んだ言語を残す。「### 言語の切り替え」に 1 行足し、未実装と書いた
- 実装の修正が必要: スキルツリーのリセット確認と解放確認の文言の「研究ポイント」「研究P」を「解放ポイント」「解放P」にする（`Assets/Localization/Tables/UICommon_ja.asset:330,334`、`Assets/Localization/Tables/UICommon_en.asset:330,334`、`Assets/Editor/Scripts/Localization/UICommonLocalizationSetup.cs:103-104`）
- 実装の修正が必要: タイトル画面のセーブデータのリセットで言語と判定オフセットを残す。今は音量だけを残してセーブファイルを消している（`Assets/Scripts/Runtime/6.Composition/OutGame/Title/TitleSceneInitializer.cs:572,581,627`。言語・判定オフセットは `Assets/Scripts/Runtime/1.Domain/Persistent/Savedata/EnvironmentSettingsData.cs:194,200` でセーブデータの中にある）
- 実装の修正が必要: 体験版の終了画面でのセーブデータ削除で言語と音量を残す。今は何も残さずセーブファイルを消している（`Assets/Scripts/Demo/DemoRuntimeBootstrap.cs:689-709`、削除は `:702`）
- 要確認:
  - 訳語の正本（外部 Workbook）の置き場所と管理者。Workbook の URL は PR にも書かれていない（要企画確認、OG-214）
  - 英語テーブルに日本語が残っている 5 キーの英訳（要企画確認。用語は決定 No.3 で「解放ポイント」に決まったので、英訳は既存の `ui.points.unlock`「Unlock Points」に揃えるかを確かめる）
  - スキルツリー画面のタイトル「研究」（`Assets/Localization/Tables/UICommon_ja.asset:242`、`Assets/Editor/Scripts/Localization/UICommonLocalizationSetup.cs:81`）と、本文の画面名「研究画面」は決定 No.3 の対象外として変えていない。画面名も「解放」系に揃えるか（要企画確認）
  - スキルツリーのノード名「発／拡張／完」の英訳（#1743 で暫定訳 Start／Ext／End の採否が回答待ち）
  - 「ローカライズしていない表示」に挙げた各項目を、今後ローカライズするか、日本語（または英語）固定とするか（要企画確認。TU-08・TU-11・DM-07・HD-13・OG-217）
  - 英語設定での実際の表示（文字の切れ・はみ出し）は実機で確かめていない（#1667・#1743 とも未確認と記載、要実機確認）
  - 本ページを仕様リスト DB の行として作るとき、DB の「カテゴリー」に「全体」を使ってよいか（既存の選択肢に合わせた）

## 適用する本文

## 仕様概要
ゲーム内の表示を、プレイヤーが選んだ言語で表示する。対応している言語は日本語と英語の2つである。
翻訳した文字と画像は、Unity Localizationのテーブルで管理する。

### 関連ページ
[[コンフィグ]]
[[設定画面]]
[[タイトル画面]]
[[チュートリアル]]

### 関連用語
[[用語リスト]]

## 仕様意図
日本語を読めないプレイヤーにも遊んでもらうため。
画面ごとに文字を直書きせず、テーブルに集めることで、訳語の変更や言語の追加を画面の実装に触れずに行えるようにするため。

## 詳細説明

### 対応言語
| 言語 | ロケールコード | 設定画面での表示 |
|---|---|---|
| 日本語 | ja | 日本語 |
| 英語 | en | English |
- 初期値は日本語である
- 設定画面の選択肢は、今の表示言語にかかわらず「日本語」「English」と表示する

### 言語の切り替え
- 言語は、ホームの設定画面（環境設定）と、タイトル画面のメニューから変えられる
- ホームの設定画面では、「設定を保存」で確定する。保存せずに閉じると、元の言語に戻る
- タイトル画面のメニューでは、切り替えた時点で保存する（音量の設定と同じ動き）
- 切り替えると、表示中の画面の文字と画像がすぐに選んだ言語へ変わる
- 選んだ言語はセーブデータ（オプションの内容）に保存し、次に起動したときも同じ言語で表示する
- タイトル画面でセーブデータをリセットしたときと、体験版の終了画面でセーブデータを消したときも、選んだ言語は残す（未実装。今はどちらも、保存されている言語が初期値の日本語に戻る）

### 翻訳テーブル
翻訳は次の3つのテーブルで管理する。テーブルは`Assets/Localization/Tables/`にある。
| テーブル | 種類 | キーの数 | 用途 |
|---|---|---|---|
| UICommon | 文字 | 99 | アウトゲームの画面（タイトル・ホーム・作戦・改造・研究・設定）の見出し・ボタン・確認ダイアログ、インゲームの操作案内、体験版の通知 |
| TutorialSubtitles | 文字 | 11 | チュートリアル戦闘のトライアドの台詞の字幕（`triad.tutorial.01`〜`11`） |
| TutorialPopupImages | 画像 | 10 | チュートリアル戦闘の説明ポップアップの画像（日本語版・英語版の2枚ずつ） |
- UICommonのキーは`ui.<画面>.<項目>`の形で付ける（例：`ui.setting.language`）
- テーブルの初期の中身は、Editorのメニュー`KillChord/Localization/UI共通テキスト用テーブルをセットアップ`と`KillChord/Localization/チュートリアル用テーブルをセットアップ`で作る
- テーブルの読み込みが終わるまでは、画面に書かれている日本語を表示する。翻訳が取れなかったときも日本語のままにする

### 訳語の決め方
- 英訳は、外部のWorkbookで確定した訳を使う【要確認: Workbookの置き場所と管理者を企画に確認】
- 用語の英名は、用語リストの「英名」に合わせる
- 英訳が決まっていない項目は、英語のテーブルにも日本語を入れておく（英語設定でも日本語で表示される）

### 英語テーブルに日本語が残っている項目
英訳が未定のため、英語設定でも日本語で表示する。
| キー | 表示される日本語 | 使われる場所 |
|---|---|---|
| `ui.setting.language` | 言語 | 設定画面の項目名 |
| `ui.setting.vibration` | 振動 | 設定画面の項目名 |
| `ui.setting.rhythm_offset` | リズム判定タイミング | 設定画面の項目名 |
| `ui.skill_tree.reset_message_format` | 返却される研究ポイント（文の後半） | 研究画面のリセット確認 |
| `ui.skill_tree.unlock_points_format` | 研究P | 研究画面の解放確認 |
- 表の最後の2項目の「研究ポイント」「研究P」は、日本語・英語のテーブルとも「解放ポイント」「解放P」に直す（今は「研究」のまま表示される）
【要確認: 5項目の英訳を企画に確認】
- スキルツリーのノード名「発／拡張／完」の英訳も未定である【要確認: 暫定訳Start／Ext／Endを採るかを企画に確認】

### ローカライズしていない表示
次の表示は翻訳テーブルを通らないため、言語を切り替えても変わらない。
- 日本語のまま表示されるもの
  - ロード画面のTips
  - シナリオの本文（シナリオのCSVは日本語だけ）
  - チュートリアル戦闘のミッション名・ミッション文・ガイド文
  - ホームのチュートリアルの説明文
  - スキル名・スキルの説明など、マスターデータに書かれた文字
  - 改造画面のジャンル絞り込みの「全て」
  - リザルト画面のミッションの「達成」「未達成」と、「最大コンボ」「戦闘時間」「リトライ」「完了」などの見出しとボタン
  - 体験版のタイマー（「全体」「ホーム」）
- 英語のまま表示されるもの
  - 戦闘の演出メッセージ（「Mission Start!」など）と、リザルト画面の見出し（「Mission Complete」「Mission Failed」）
  - チュートリアル戦闘の判定表示（「Success」「Miss」「Perfect」）
- テーブルを使わず、画面側で日本語と英語を切り替えているもの
  - 画面の読み込みに失敗したときの復帰画面の文言
  - 体験版の通知（時間切れ・ホームタイマー終了・セーブデータのリセット）の、翻訳の読み込み中に出す文言
【要確認: 上の各項目を今後ローカライズするか、今の言語で固定とするかを企画に確認】
