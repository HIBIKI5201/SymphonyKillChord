# UI/UX 設計レビュー（2026-10-01, Codex）

## 要約

全体評価は **C（A〜Eの5段階：基本導線は成立、操作・学習・表示の契約に補強が必要）**。
ホーム・作戦・研究・改造の役割分離、共通ナビゲーション、視覚リズムガイドと字幕は良い土台である。
7月のUXレビュー以降、ホーム案内、装備バッジ、未保存確認、入力アイコン、判定オフセットの実装を確認した。
一方、色名に依存する学習、台本と実ミッションの差、決定配置と案内アイコンの差、ボタン作動方式の混在が残る。
最新Notionミラー本体はこのチェックアウトに無く、トンマナとの一致および実機の視認性は未確認である。
今週は入力契約とチュートリアルの照合を優先し、全面的なUI方式移行は行わないことを提案する。

## 現状の構造

### 調査条件と根拠

- 表題の日付は指定された基準日。実際の確認日は2026-10-06。
- ローカル資料・コード・設定を読み、指定の本ファイルだけを書いた。
- Git操作、Unity起動、コンパイル、テスト、Issue更新、画像閲覧は行っていない。
- 通常のAGENTS.mdの保存・コミット規則より、今回の出力先・Git禁止指示を優先した。
- 会話は韓国語、成果物は今回指定された日本語とした。
- `R/` は `Assets/Scripts/Runtime/` を表す。
- `L/` は `Assets/Level/`、`LOC/` は `Assets/Localization/Tables/` を表す。
- `JUL/` は `Docs/agent/analysis/omnipotens/2026-07-18_symphony-kill-chord/spec/` を表す。
- `OTHER/` は `C:/Users/takut/GameProject/SKC-design-review/Docs/agent/reports/` を表す。
- 行番号は現チェックアウトのファイルを基準にする。
- 文書はページ名・節名で示す場合がある。過去の検証結果は今回の検証と区別する。
- 「実装あり」は静的確認であり、シーンへの完全な配線や実機動作の合格を意味しない。
- 推測は明記し、資料に残る不具合を現在の再現済み不具合とは扱わない。

### 最新仕様の利用可能性

指定された `Docs/NotionSpecifications/Symphony Kill Chord/` は存在しなかった。
`Docs/NotionSpecifications/` にあるMarkdownはREADMEのみである。
READMEはサブモジュールと同期ワークフローによるミラー運用を説明している。
Git操作禁止のため取得・初期化は実施していない。
別作業ルートのNotionSpecificationsにも本体ディレクトリを確認できなかった。
したがって、トップページ、仕様書ガイド、仕様概要、システム概要、ワークフローの全文は読めていない。
「デザイン」「トンマナ」を直接照合したという結論も出せない。
他観点レビューの引用は二次資料として利用し、最新仕様の代用とはしない。
根拠：`Docs/NotionSpecifications/README.md`、`OTHER/2026-10-01_design-review-notion-spec.md`「調査範囲と根拠の表記」。

### ビルドに登録されたシーン

| 順序 | シーン | 有効 | UI上の役割 |
|---|---|---|---|
| 0 | Persistent | はい | 入力・音量・言語・シーン遷移などの常駐基盤 |
| 1 | Title | はい | 開始、メニュー、オプション、クレジット |
| 2 | OutGame | はい | ホーム、作戦、研究、改造、設定 |
| 3 | InGame | はい | 戦闘共通UI、チュートリアル、リザルト |
| 4 | Scenario | はい | シナリオ表示 |
| 5 | Stage_01 | はい | ステージ内容 |
| 6 | Stage_02 | はい | ステージ内容 |
| 7 | DemoEnd | いいえ | デモ終了専用。通常設定ではビルドに含まれない |

根拠：`ProjectSettings/EditorBuildSettings.asset:7`。
BuildSettingsの順序は起動順・全遷移順そのものではない。
TitleStartControllerは対象シーンを加算ロードした後にTitleをアンロードする。
根拠：`R/3.Adaptor/OutGame/Title/TitleStartController.cs:32`。
DemoEndの利用可否はデモビルド設定も含めて別途確認が必要である。

### 画面遷移の全体像

```text
Persistent（常駐）
└─ Title
   ├─ Menu / Options ─ Credit
   │  └─ データ削除確認
   ├─ 通常開始 ─ OutGame / Home
   └─ 初回開始 ─ Opening Scenario
                  └─ OutGame（自動チュートリアル出撃経路）
                     └─ InGame + 選択ステージ
                        └─ チュートリアル戦闘
                           └─ Result ─ OutGame / Home案内

OutGame / Home
├─ 作戦（StageSelect）
│  ├─ ステージ詳細 / ミッション選択 / 装備確認
│  ├─ シナリオ選択 ─ Scenario ─ 復帰経路
│  └─ 戦闘選択 ─ InGame + Stage_01等
├─ 研究（SkillTree）
│  ├─ ノード / 効果詳細 / プレビュー / ステータス
│  └─ 解放確認 / 進行度リセット確認
├─ 改造（SkillBuild）
│  ├─ 全スキル一覧 / ジャンル絞込 / 装備スロット / 強化
│  └─ 保存 / 未保存変更確認
└─ 設定（Setting）─ タイトルに戻る確認

InGame / HUD
├─ ミッション / HP等 / リズムガイド / スキル入力進行
├─ 説明ポップアップ / 会話字幕 / チュートリアル成否
├─ 停止・再開 / タイトル復帰経路
└─ Result
   ├─ 完了 ─ 選択状態のReturnSceneName
   └─ 再挑戦 ─ 同じ戦闘シーンを再ロード
      （デモでは終了方針による上書きあり）
```

この図は現コードと補助文書からの復元であり、最新Notionの公式遷移図ではない。
初回分岐：`R/6.Composition/OutGame/Title/TitleSceneInitializer.cs:693`。
自動出撃開始：`R/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs:317`。
ホーム案内開始条件：`R/6.Composition/OutGame/Tutorial/OutGameTutorialInitializer.cs:182`。
通常画面区分：`R/1.Domain/OutGame/Screen/ScreenId.cs:6`。
結果からの完了・再挑戦：`R/3.Adaptor/InGame/Result/StageResultController.cs:47`、`:78`。
デモ方針：`Assets/Scripts/Demo/DemoStageResultExitPolicy.cs`。
BattlePreparationは列挙型の予約値であり、現役の戦闘準備画面として図に復活させない。
根拠：`R/1.Domain/OutGame/Screen/ScreenId.cs:18`。

### 実装方式と操作の境界

| 領域 | 主な方式 | 操作・表示の機構 | 評価 |
|---|---|---|---|
| Title・OutGame | UI Toolkit | VisualElement、UXML、USS、ScreenViewBase | 画面間の共通化が進む |
| 作戦マップ | UI Toolkit | 動的ノード、横ドラッグ、ズーム、フォーカス枠 | UI動作がInitializerにも集中 |
| 研究・改造 | UI Toolkit | ノード・カード、確認ダイアログ、明示フォーカス | ドラッグと決定入力の両系統 |
| 戦闘リズムガイド | uGUI | Image、RectTransform、アニメーション | 判定位置と描画幅を共有する工夫あり |
| リザルト | uGUI + TMP | Button、EventSystem、数値演出 | 戦闘側の操作方式を維持 |
| チュートリアル会話 | uGUI + TMP | LocalizedString、字幕アニメーション | 字幕の実装を確認 |
| モバイル戦闘操作 | uGUI / Input System | OnScreenStick、タップ、スティックフリック | Touch専用処理が存在 |

根拠：HomeScreenView、StageSelectInitializer、ACLikeRhythmGuideView、StageResultView、MissionDialogueView、MobileStickFlickInput。
二方式の併用自体は欠陥ではない。戦闘とメニューで適した方式を分けられる。
問題はフォーカス、決定、キャンセル、設定保持、表示状態の意味が共通化されているかである。
共通契約を整える方が、uGUIを一括移行するより小さなチームの負担を抑えられる。

### 発注・引継ぎ・現コードの整合

| 資料 | 記載 | 現コードとの照合 |
|---|---|---|
| UI発注書 8/31 | 作戦・研究・改造、専用フレーム | HomeScreenViewの画面区分と整合 |
| 同上 | 研究ポイント、改造ポイント | Homeには「解放ポイント」「改造ポイント」の表示キーがある |
| 同上 | 通常と非活性は同じ見た目、押下で暗色化 | 現Homeはfocus同期・pulseも使う。状態仕様の追記が必要 |
| 同上 | スキルジャンルの表示箇所は未確定 | 現改造には専用FilterBarが存在。資料の未確定状態は古い |
| 同上 | リセット確認・敗北Tips | 対応するViewとPresenterが存在 |
| 改造引継ぎ 9/4 | 解放済み／未解放、装備バッジ | SkillListViewとSkillBuildScreenViewに対応実装あり |
| 同上 | ドラッグ後の並び崩れ修正 | 元インデックスへのInsertを現コードで確認 |
| 同上 | 新規セーブ進行不能は保留 | 固定スキルoverrideは現在のTutorial assetに存在。解消の証明ではない |
| 作戦引継ぎ 9/8 | 初回ズーム位置異常は未解決 | 後続の幾何更新・フォーカス処理あり。再現状況は不明 |
| 研究引継ぎ 9/8 | ドラッグ共有・横方向未検証 | 同Manipulatorを作戦でも利用。共有変更の受入範囲は要確認 |

根拠：`Docs/agent/reports/UI発注書_2026-08-31.md`、各引継ぎ資料の「実装内容」「検証状況」。
現コード：`R/4.View/OutGame/Screen/HomeScreenView.cs:72`、`:148`。
改造：`R/4.View/OutGame/Screen/SkillBuildScreenView.cs:57`、SkillElementDragAndDropManipulatorのSnapBackToStart。
作戦：`R/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs:1241`、`:1408`。
引継ぎの「コンパイル済み」は資料作成時の結果であり、今回の品質保証には流用しない。

### デバイス別の実装と案内

| 行動 | キーボード／マウス | ゲームパッドの定義 | タッチ／Android |
|---|---|---|---|
| 移動 | WASD | 左スティック | 仮想スティック |
| 視点 | マウスdelta | 右スティック | EnhancedTouchの領域追跡 |
| 攻撃 | 左クリック | 右トリガー／右フェイスボタン | ボタン、説明中の直接タップ処理 |
| 回避 | Space | 下フェイスボタン | 仮想スティックのフリック処理 |
| ロックオン | 中クリック | 左トリガー | 具体的な実機到達性は未確認 |
| 対象切替 | Q／R | 左右ショルダー | 具体的な実機到達性は未確認 |
| オプション | Esc | Start | 戻る操作とOS操作の統合は未確認 |
| UI決定／取消 | Enter／Esc | 右／下を設定で入替 | UI Point／ClickにTouch bindingあり |
| シナリオ | Enter、Shift、Space、Esc等 | フェイスボタン、Start等 | 全機能のタッチ経路は未確認 |

根拠：`Assets/Settings/Input/KillChordInputActioMap.inputactions` の各map。
これはアセットの基本定義であり、実行時override後の表示ではない。
決定配置は `R/4.View/Persistent/Input/GamepadButtonLayoutView.cs:47` で変更する。
基本定義は日本式、保存設定の既定値は海外式である。
攻撃・回避のInGame bindingはこの決定配置切替の対象に含まれない。
台本の「ロックオン：R1／右クリック」は現定義のLT／中クリックと違う。
台本の「攻撃：□」も現定義の右フェイスボタン／RTと違う。
現テキストには `{input.attack_button}` が入り、固定文字列から改善している。
ただし台本・収録・操作説明画像まで同じ変更が反映されたかは未確認である。

入力種別ObserverはKeyboard、Xbox、PlayStation、Switchを扱う。
KeyboardとMouseの操作をKeyboardにまとめ、未知ゲームパッドはXbox側へ分類する。
Touch種別はenumとObserverに存在しない。
根拠：`R/4.View/Persistent/Input/InputDeviceKind.cs:6`、InputDeviceKindObserver:81。
Localizationのchooseも同じ4種類である。
根拠：`LOC/UICommon_ja.asset:479`、InputDeviceLocalizationVariable:129。
画像アイコンはTMP Sprite AssetからUI ToolkitのImageへ切り出す共通機構がある。
根拠：`R/4.View/Persistent/Localization/InputGlyphImage.cs:24`。

### オンボーディングと情報負荷

| 項目 | 8/5台本 | 現Tutorial asset／コード |
|---|---|---|
| 入口 | 初回シナリオ | TutorialPhaseによる初回分岐あり |
| 順序 | 移動→ロックオン→4→8→2→任意1/3/6→回避→KC | 移動→回避→攻撃説明→6色の課題→KC→撃破 |
| 最小成功 | 4拍を2回 | 緑の実行課題が参照するAttackFourBeatは4回 |
| 安全性 | 敵攻撃力0 | DamageReductionBuffのreductionRate=1を使用 |
| 固定装備 | skill0、skill13 | skill_00、skill_13のoverrideあり |
| 教示媒体 | 無線＋テキスト＋コマンド | ポップアップ画像、ガイド文、字幕、成否表示 |
| 終了後 | 通常課題への転移 | BattleCompleted後にHomeの4項目を案内 |

根拠：`Docs/agent/reports/チュートリアルフロー詳細_ボイス収録用_2026-08-05.md` §3、Phase 3。
現データ：`L/Data/Master/OutGame/StageSelect/Mission/MissionDefinition_Tutorial.asset:22`、`:27`、`:191`、`:542`。
保存段階：`R/1.Domain/Persistent/Savedata/TutorialPhase.cs`。
Home案内：`R/6.Composition/OutGame/Tutorial/OutGameTutorialInitializer.cs:28`。
台本の一部は提案・未決判断であり、現コードとの違いを直ちに仕様違反としない。
重要なのは採用済みフロー・収録音声・実データを一組で照合できないことである。
緑の説明ポップアップと実行課題は異なる参照条件を持つ。
RefIdsに残る古い条件だけを数えて現在の必要操作回数を判断しない。
緑の実行課題はrid末尾015→4420578705146129→AttackFourBeat/4回を追跡した。
全課題の所要時間、再試行回数、離脱地点は実測していない。

### 視認性・フィードバック・設定

| 機能 | 確認した実装 | 実用上の限界 |
|---|---|---|
| リズム位相 | ACLikeRhythmGuideView、Zone、Just輪郭、帯、現在拍 | コントラスト・視線移動は未観察 |
| 目標拍の強調 | TargetFeedbackPresenter、非対象帯の減光 | 色名中心の説明を補う非色覚の意味表示は要確認 |
| ジャスト判定 | MusicSyncServiceの確定判定をViewへ渡す | 早い／遅いの診断表示は今回確認できず |
| チュートリアル成否 | Perfect／Success／Miss、発光ポップアップ | 何を外したかの説明は弱い |
| スキル成立 | Skill入力進行関連Presenter／UI設定 | HUD全体との干渉は未確認 |
| ダメージ | TMP数値とCritical! | 表示整形・翻訳に一部固定文字列あり |
| 敗北Tips | MissionDefinitionから1件選択 | 原因を使わずランダム選択 |
| 字幕 | TutorialSubtitles、LocalizedString、fallback | 字幕サイズ・独立した常時表示設定は未確認 |
| 音量 | BGM／SE／Voiceの独立した0〜10 | 会場騒音下での拍と音声の分離は未測定 |
| オフセット | ±300ms、50ms刻み、判定時刻へ加算 | 細かな調整と校正UIは確認できず |
| 操作支援 | 感度、反転、auto lock、振動、決定配置 | 任意リマップ・hold/toggleは未確認 |
| セーフエリア | リズムガイドでScreen.safeArea使用 | OutGame全体の対応を保証しない |

根拠：ACLikeRhythmGuideView:255、:627、TutorialAttackFeedbackPresenter:60。
MissionDialogueView:18、:155、StageResultPresenter:123、AttackResultView:38。
EnvironmentSettingsData:94、MusicSyncController:35、AudioSettingsData:27。
色覚・字幕・音量・オフセットを「未実装」の一語でまとめるのは不正確である。
字幕・音量・オフセットは存在し、残る課題は利用可能性と調整の粒度である。

## 良い点

1. **画面の目的が分かれる。** Homeから作戦・研究・改造に進み、育成と装備を分けている。
   根拠：HomeScreenViewの各Activation handler、ScreenId。
2. **メニューの操作共通化がある。** MakeNavigable、RegisterActivation、ModalNavigationScope等が存在する。
   根拠：`R/4.View/OutGame/Navigation/UINavigationExtensions.cs:63`、`:87`。
3. **コントローラーの選択を可視化する。** Homeではhoverとfocusの親クラス同期、作戦では選択枠がある。
   根拠：HomeScreenView:148、StageSelectInitializer:402。
4. **改造で装備候補が消えない。** 一覧とスロットを分け、装備バッジ・未解放表示・ジャンル絞込を持つ。
   根拠：改造引継ぎ§2、SkillListView、SkillBuildSlotLayout。
5. **破壊的操作の確認がある。** 研究リセット、データ削除、タイトル復帰で確認UIとフォーカス管理を使う。
   根拠：SkillTreeResetDialogView:40、MenuScreenView:420、SettingScreenView:284。
6. **変更の喪失を減らす。** 改造の未保存確認、環境設定のworking／committed複製が存在する。
   根拠：SkillBuildScreenView:65、EnvironmentSettingsController:32、:254。
7. **リズム表示が判定データと結び付く。** Just成否と帯・輪郭を同一描画幅で扱う。
   根拠：ACLikeRhythmGuideView:255、:266。
8. **音声以外の教示がある。** 会話字幕と入力アイコンがあり、無音検証に進める基盤がある。
   根拠：MissionDialogueView:202、InputGlyphTextFormatter:35。
9. **モバイル専用の障害を扱う。** 説明中のタップ攻撃は操作UIを除外し、フリックも専用処理を持つ。
   根拠：MobileTapAttackInputのIsOverInteractiveUi、MobileStickFlickInput:91。

## 問題点

| ID | 重大度 | 問題 | 根拠 | 影響 |
|---|---|---|---|---|
| UX-01 | 高 | 6色の課題のガイド文が色名を主な手掛かりにする。非色覚での等価な識別契約を確認できない | Tutorial asset:55〜105、ACLikeRhythmGuideView:132 | 色を見分けにくいプレイヤーが指定タイミングを学べない可能性（推測） |
| UX-02 | 高 | 台本と実ミッションで順序・教示回数・操作表記が異なる | 8/5台本§3、Phase 2/3、Tutorial asset:27〜117、:191、:542 | 収録・説明画像・成功条件の不整合による学習混乱（推測） |
| UX-03 | 高 | 決定配置overrideは設定依存だがsubmit glyphは機種だけで右ボタン固定 | GamepadButtonLayoutView:47、LOC/UICommon_ja.asset:540、Shared Data.asset:511 | 既定の海外式で実操作と案内が違う。画面での出現頻度は未確認 |
| UX-04 | 中 | ジャンルフィルタ・環境保存はclicked購読で、他UIのRegisterActivationと作動契約が違う | SkillGenreFilterBarView:24、:53、EnvironmentSettingsView:225、:231、SkillBuildScreenView:329 | 決定入力で反応しない経路の可能性（推測）。Unityの標準Button経路を含め実行確認が必要 |
| UX-05 | 中 | Touchを入力案内の種別として扱わない | InputDeviceKind:6、InputDeviceKindObserver:81、LOC/UICommon_ja.asset:479 | Androidで外付け機器の案内が残る可能性（推測）。仮想Gamepadの影響も要確認 |
| UX-06 | 中 | オフセットは50ms刻み。校正結果を提示するUIを確認できない | EnvironmentSettingsData:94〜108、EnvironmentSettingsView:40、MusicSyncController:37 | 設定が存在しても細かな遅延へ合わせにくい。体感影響は未測定 |
| UX-07 | 中 | Home案内の日本語文、フィルタ「全て」、Critical!等が固定文字列 | OutGameTutorialInitializer:28〜41、SkillGenreFilterBarView:23、AttackResultView:41 | 英語設定でも一部教示が翻訳されない実装経路 |
| UX-08 | 中 | 発注書の状態仕様・用語・未確定欄が現UIから遅れる | UI発注書のホーム／ポイント／ジャンル節、HomeScreenView:72、SkillBuildScreenView:57 | デザイナーと実装担当で追加状態・納品範囲の解釈がずれる |
| UX-09 | 中 | 作戦初回ズーム、新規セーブ進行不能、共有ドラッグ変更の後続受入証拠が不足 | 作戦引継ぎ§5、改造引継ぎ§3-3/§5、研究引継ぎ§4 | 初回体験の障害を解消済みと誤認しやすい。現再現は未確認 |
| UX-10 | 中 | 敗北Tipsの選択がプレイ原因を使用しない | StageResultPresenter:123〜132 | 同じ失敗に関係しないTipsが選ばれ、再挑戦の学習につながりにくい可能性（推測） |
| UX-11 | 低 | スキルのソートが表示名末尾の数字に依存する | SkillListView:63〜64、:376〜397 | 正式命名・翻訳で順序が変わる可能性。運用拡大時の負担 |
| UX-12 | 中 | 最新デザイン・トンマナ・トップ仕様を直接読めない | NotionSpecificationsのローカル列挙、README | 最新正本との一致・正式な採否を本レビューで確定できない調査上の制約 |

UX-04は静的な作動方式の差を確定し、実機の無反応を推測として区別した。
clickedだけで必ず不動になると断定はしない。
UX-09も過去資料の未検証事項であり、現在のバグ件数には算入しない。
色覚モード・字幕サイズ・画面揺れ低減・任意リマップは設定モデルで確認できなかった。
別アセットや別実装で提供されている可能性があるため、「製品に一切ない」とは断定しない。

## 改善提案

### すぐやる（今週）

#### P1. 決定・取消・アイコンの契約を合わせる

- **何を**：FilterBar、環境保存、submit glyph、決定配置設定を一組として照合する。
- **なぜ**：選択できるのに確定できない、案内と実ボタンが違う状態は基本導線を損なう。
- **手順**：まず現コードの決定イベントを観察し、clicked経路の挙動を確認する。
- 確認した差に応じてRegisterActivationへ揃え、重複発火も検査する。
- glyphには決定配置を反映し、攻撃glyphとUI決定glyphを別の用途として保つ。
- Keyboard、各パッド、海外式／日本式で「開く→絞込→保存→取消」を一巡する。
- **工数感**：S〜M。**関連問題ID**：UX-03、UX-04。

#### P2. チュートリアルを収録・データ・教示の一表で確定する

- **何を**：採用フロー、台詞ID、popup key、必要行動、回数、固定装備を対応付ける。
- **なぜ**：台本は提案を含み、実データには異なる順序と条件がある。
- **手順**：現assetのclearConditionStepsから参照されるRefIdsだけを追跡する。
- 4拍4回などの現条件を台本の2回成功と照合し、企画担当が採用値を決める。
- ロックオン・攻撃表記を実bindingへ合わせ、収録済み音声との齟齬を確認する。
- 色指定には拍の数字・武器名・形を併記し、白黒でも課題を識別できるようにする。
- 新規セーブから初回シナリオ→戦闘→Home案内まで一本で受け入れる。
- **工数感**：M。**関連問題ID**：UX-01、UX-02、UX-09。

#### P3. 引継ぎの未確認事項を再判定する

- **何を**：初回ズーム、初期装備、ドラッグ復帰、横スクロールの現状を更新する。
- **なぜ**：古い未解決記録と修正済みコードが並存している。
- **手順**：作戦に入った直後の一回目と二回目を、同じ解像度・入力で比較する。
- 改造は空振りドラッグ、装備、解除、filter後のfocus復元を確認する。
- 初期装備は新規セーブと既存セーブで分け、overrideと保存付与を混同しない。
- 引継ぎの検証欄を「現再現／解消確認／未確認」に更新する。
- **工数感**：S〜M。**関連問題ID**：UX-09。

### 次に（1か月）

#### P4. UIの状態表を発注と実装の共通受入基準にする

- **何を**：normal、hover、focus、pressed、disabled、locked、equipped、unsavedの意味を定める。
- **なぜ**：発注書に無いfocus演出や未確定のgenre表示が現実装に存在する。
- **手順**：Home、作戦、研究、改造、Resultから各状態の代表例を一つずつ選ぶ。
- 枠・アイコン・文字・音の担当を明記し、disabledとnormalを識別可能にする。
- 研究／解放ポイントの用語、ノード状態、Tips文字数の未確定欄を決着させる。
- 実装担当とUI制作担当が同じ表に承認・組込み・検証の別状態を記録する。
- 最新Notionが利用可能になった時点でデザイン／トンマナへ照合する。
- **工数感**：M。**関連問題ID**：UX-08、UX-12。

#### P5. モバイル案内と調整可能性を補う

- **何を**：Touch用案内、細かなオフセット、校正、字幕・揺れの設定を段階追加する。
- **なぜ**：既存の操作支援を、プレイヤーが自力で使える状態へ進める必要がある。
- **手順**：Touch表示種別を設け、仮想Gamepad入力と実パッドの接続を区別する。
- フリック回避の方向・受付時間を文字と動きで案内し、実機の誤操作を記録する。
- オフセットは現50msを維持する案と細かな刻みを比較し、±境界と保存互換を確認する。
- 校正では早い／遅いの方向と調整後の試行結果を提示する。
- 字幕の読みやすさ・画面揺れ低減は必要な項目から実装する。
- **工数感**：M。**関連問題ID**：UX-01、UX-05、UX-06。

#### P6. 翻訳と順序を表示名から切り離す

- **何を**：Home案内・フィルタ・戦闘feedbackを翻訳キーへ、ソートを安定した順序値へ移す。
- **なぜ**：言語を変えた際に教示と一覧順序が不安定になる。
- **手順**：今回確認した固定文字列から着手し、ja/enの空値・長文を確認する。
- DTOへ順序値を渡し、表示名末尾を解析する重複処理を廃止する。
- アイコン未ロード時にも行動名のfallbackを残すことを検討する。
- **工数感**：S〜M。**関連問題ID**：UX-07、UX-11。

#### P7. 通常戦闘への学習転移と敗北回復を測る

- **何を**：教示を減らした課題と、失敗原因に対応したTipsを用意する。
- **なぜ**：チュートリアル成功と、通常戦闘での自力成功は別である。
- **手順**：4拍→8拍→KCの各成功後に、同じ課題を案内を減らして一度試す。
- 最初のKCまでの時間、助言回数、通常戦闘での成立率を記録する。
- Tipsはtimeout、違う拍、対象問題など実際に取れる原因から優先表示する。
- 判別できない時は一般Tipsへ戻し、誤診断を断定しない。
- **工数感**：M。**関連問題ID**：UX-02、UX-10。

### 将来

#### P8. HUDの統合検証とアクセシビリティの継続評価

- **何を**：リズム・命中・KC・敵予告・目標の同時提示を実プレイで評価する。
- **なぜ**：静的な機能数から注視点や因果の理解は判断できない。
- **手順**：同一敵・同一カメラで4拍と8拍を比較し、KC成立までを切らずに記録する。
- 無音、白黒、低音量、小画面、字幕長文の条件でルール説明と課題成功を測る。
- UI方式は維持し、フォーカス・モーダル・設定保存・表示状態の共通契約を拡充する。
- 色覚プリセット、任意リマップ、字幕拡大は利用者の失敗箇所に応じて追加する。
- **工数感**：L。**関連問題ID**：UX-01、UX-05、UX-06、UX-08。

## 他観点との接点

### 7月UXレビューからの変化

| 7月の論点 | 現在の証拠 | 判定 |
|---|---|---|
| visual beat／HUD契約が薄い | Zone、Just輪郭、対象帯強調、Skill進行UI | 実装証拠は増えた。無音理解は未検証 |
| 字幕・設定・校正の不足 | TutorialSubtitles、独立音量、判定offset | 字幕と設定は改善。校正・粒度は課題 |
| tutorialのcompetence gate | 回数条件、固定装備、無敵化、成否表示 | 成功ゲートあり。台本との整合・fadeは未確定 |
| destructive reset／unsaved | 研究リセット、データ削除、未保存確認 | 確認経路を実装。全文と復元結果は実機未検証 |
| device mapping未確認 | 実binding、Observer、glyph、決定配置 | 可視化は進む。Touchとsubmit配置が不足 |
| 敗北Tipsの診断性 | ランダムなTips選択 | 原因別の回復導線は残課題 |
| Adaptive BGM未配線 | 本レビューはUI中心に調査 | 解消判定を保留。音楽担当観点と照合する |
| normal stageへのtransfer | Home案内は増えた | 戦闘の自力転移証拠とは別。未測定 |

根拠：`JUL/plan/10-ux-review.md`「アクセシビリティ」「改善案」「配信映え」。
7月の数値は当時の仕様coverage heuristicであり、本レビューのC評価との数値比較はしない。
過去レポートの「未配線」を現在も未配線と断定しない。

### 他レビューと分担する論点

| 観点 | 接点 | 本レビューの範囲 |
|---|---|---|
| ゲーム設計 | 拍の意味、KC条件、初回学習、回復 | 数式・戦闘バランスの再監査はしない |
| コード設計 | StageSelectInitializerの責務集中、共通View | UI動作の受入境界に絞る |
| Notion仕様 | 正本・提案・履歴の区別、未確定欄 | UIの採用値と収録・実データの照合を求める |
| アセット | 状態差分、画像内文字、フォント、アイコン | 制作完了と組込み完了を分ける |
| Drive | Source／Export、UI納品物の対応 | 整理計画を重複提案しない |
| QA | 初回一巡、デバイス、無音、白黒、再挑戦 | 検証観点を提示。今回実行はしない |
| 運用 | PRの確認済み／未確認、引継ぎ更新 | 既存テンプレの証拠欄を利用する |

参照：`OTHER/2026-10-01_design-review-game.md`、`...-code.md`、`...-notion-spec.md`、`...-google-drive.md`。
コードレビューのInitializer肥大化は既知であるため、独立した重大度付き指摘として重複計上しない。
Drive snapshotとIssue #2141の保存文面には、提出品ごとのSource／Export整理がある。
UIでは納品素材を状態仕様へ紐付け、旧版を誤って組み込まないことが接点となる。
Discord「UI作成」ログでは、クリティカル装飾と会心率アイコンの呼称・用途への質問がある。
根拠：`Docs/DiscordLog/進捗共有_UI作成_1537380042987016202.txt:418`。
これはUX-08の意味・用途の対応表を必要とする実際の制作上の証拠である。
同ログにはフォント指定もあるが、全画面へ実適用された証拠とは扱わない。
根拠：同ファイル:534。
PRテンプレはPlayMode結果と未確認事項の区別を求めるため、P1〜P3の受入証拠を残せる。
根拠：`.github/PULL_REQUEST_TEMPLATE.md`「確認済みの内容」「未確認・残論点」。
今回の静的レビューを、そのPlayMode確認済み欄に転記してはならない。

## 未確認事項

- 最新Notionのトップページ、ガイド、仕様概要、システム概要、ワークフロー。
- 「デザイン」「トンマナ」の正式な配色・余白・フォント・モチーフ規定との一致。
- 10/1時点の仕様と10/6確認コードの差分。Git操作を行っていないため基準コミット未固定。
- UI素材の完成状況と、全画面での実使用。画像は今回見ていない。
- uGUI／UI Toolkit間のEventSystem・InputSystem設定の全シーン実配線。
- FilterBar・環境保存の決定入力経路。clicked差は確認したが無反応は未再現。
- 海外式／日本式のglyph差が実際にどの画面で露出するか。
- Touch・仮想Gamepad・実パッド混在時のObserver分類と案内。
- Androidのノッチ、画面比率、タップ対象サイズ、フリック誤判定。
- 作戦の初回ズーム異常と改造の新規セーブ進行不能の現再現・解消状況。
- Tutorial assetの全参照先・全会話Cueと実収録音声の照合。
- 台本の1/3/6拍削減案が正式採用されたか。現データには6色課題がある。
- 初回シナリオのスキップから戦闘への移行と、途中終了後の再開挙動。
- 全チュートリアルの実時間、最初のKCまでの時間、案内なしの転移成功率。
- ゲーム全体の字幕サイズ・画面揺れ低減・色覚・リマップ設定の有無。
- BGMダッキング、音声と拍の聞き分け、振動強度・音量の端末差。
- offsetの端末別校正精度と、±300msで対応できる実測遅延の範囲。
- 判定失敗がタイミング・拍種・対象・timeoutのどれかを初見が説明できるか。
- HUDと敵予告・照準・字幕の重なり、英語長文の切れ、低解像度での判読性。
- リザルト数値演出中の入力、再挑戦連打、ロード失敗からの復帰。
- デモ版の終了画面がビルド設定・終了方針で利用可能か。
- GitHub Issueの最新状態と重複の完全照合。番号は保存資料の関連先としてのみ利用した。

本レビューは静的設計の評価であり、製品の操作性・アクセシビリティの合格認定ではない。
