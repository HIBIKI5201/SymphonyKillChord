# 仕様書と実装の差分分析

作成日: 2026-09-24
対象ブランチ: `feature/demo/outGameUI/master`（`develop` 派生、HEAD `f0cbf828b` / 2026-09-19時点）
対象コード: `Assets/Scripts/Runtime` 全体（0.Utility〜6.Composition の全レイヤー）。プロジェクト全域を10ドメインに分割し、10体のエージェントで並列調査した。
対象仕様書: Notion仕様書エクスポートを本日 2026-09-24 に取得した最新版。ただし個々のページ自体は多くが 2026-09-22〜23 にチーム側で一斉更新されたばかりのものである。

## 0. 調査の前提

- 前回の全面調査は `Docs/仕様書と実装の差分分析_2026-08-23.md`（`Docs/` 直下、git 管理外）（`Assets/Scripts/Runtime` の2026-08-18〜23の差分のみを対象とした部分調査）。今回はそれ以来はじめての、プロジェクト全域を対象とした調査である。
- 調査は以下の10ドメインに分割し、各ドメインごとに独立したエージェントが該当する実装フォルダと対応するNotion仕様書ページを1件ずつ突き合わせた。

  | ID | ドメイン | 実装スコープ | 主な仕様書 |
  | --- | --- | --- | --- |
  | G1 | 戦闘コア系 | `InGame/{Battle,Skill,SkillEffect,StatusEffect,Buff,Target,Reticle}`, `Player/SkillEffect` | 攻撃パイプライン・キャラクタデータ, スキル, スキル効果, 状態効果（バフ・デバフ）, ターゲットシステム, レティクル |
  | G2 | 敵・キャラクター表現系 | `InGame/{Enemy,Character,Animation,Camera,Haptics}` | 敵, 敵関連, キャラクターアニメーション, カメラ, カメラシステムパラメータ, Bossシステム |
  | G3 | インゲーム進行制御系 | `InGame/{Mission,Sequence,Player,Result}` | ミッション, シークエンス, プレイヤー, ポーズシステム開発, リザルト |
  | G4 | インゲーム音楽・UI表現系 | `InGame/{Music,Combo,UI,Stage,Scene,PostEffect}` | 音楽, 音楽同期, リズムガイド全画面演出, コンボ表示, インゲームUI, ステージ演出 |
  | G5 | アウトゲーム画面遷移・メタ系 | `OutGame/{StageSelect,Sortie,Screen,Title,Navigation,Common}` | ステージセレクト, 出撃, 戦闘準備, タイトル, スクリーン, アウトゲーム遷移フロー |
  | G6 | 育成・改造系 | `OutGame/{SkillTree,SkillBuild,Skill}` | スキルツリー, 改造画面, 研究画面 |
  | G7 | シナリオ・リソース・チュートリアル系 | `OutGame/{Scenario,Resource,Tutorial}` | シナリオシステム, シナリオCSV, シナリオコマンド, シナリオパート, チュートリアル |
  | G8 | 永続化コア系 | `Persistent/{Savedata,Input,SceneManagement,Load}` | セーブシステム, セーブ, 入力, シーンマネージャー |
  | G9 | 音量設定・ポストエフェクト・横断基盤系 | `Persistent/{Music,Audio,PostEffect,Environment,Voice,Localization}`, `OutGame/Setting`, `0.Utility/Identity`, `5.InfraStructure/Repository` | セッティング, 装備BGM切り替えシステム, マスターデータ運用ガイド, SourceDataProvider |
  | G10 | 技術系リファレンス・方針/企画系文書 | Runtime横断（クラス名の存在確認）、方針文書は実装との直接対応なし | SpringBone仕様概要, GCスパイク対策, Behavior Graphについて, 企画概要書, α/β版開発指針 等 |

- 各ドメインとも、対象仕様書ページの多くが調査前日までにチーム自身の手で実装ベースに書き直されていたため、**全体として仕様書と実装の整合性は非常に高い**。今回検出された差分は、その一斉更新をすり抜けた項目や、更新後に発生したごく最近の実装変更に限られる。
- 前回レポート（2026-08-23）で指摘した項目のその後を横断的に確認した結果は次のとおり: B-1(スキル演出基盤)・B-2(ミッションのステップ進入アクション)・B-3(戦闘ポーズ)・B-4(音量設定の永続化)・B-5(ポストエフェクトオーバーレイ)・B-6(制作メンバー一覧)・B-10(DataID/SourceData属性群) は**解消済み**。C-1(音楽同期とリズムガイドの改修)は**解消済み（一致を確認）**。C-2(SkillHitScheduler)は今回未検証（G1が別観点で調査したが矛盾は見つからず）。B-7〜B-9は今回のスコープでは個別に再検証していない。

| 記号 | 意味 |
| --- | --- |
| A | 仕様書にあるが実装が無い |
| B | 実装があるが仕様書に無い |
| C | 双方に穴がある（記述はあるが実装が古い/部分的、あるいは記述自体が実装で裏付けられていない） |

## サマリー（優先度順）

| 優先度 | ドメイン | 分類 | 概要 |
| --- | --- | --- | --- |
| 高 | G6 | C-1 | 研究画面のプレビュー動画ボタンが一時的に無効化されているが仕様書未記載 |
| 高 | G6 | C-2 | 装備枠拡張ノード(N7等)に`_hasSkillSlotBonus`フラグが1件も立っておらず、編成枠が実際に増えない |
| 高 | G5 | B-1 | OutGameの空間ナビゲーション基盤(`SpatialNavigationResolver`等)が未文書化 |
| 高 | G1 | C-1 | スキル2〜10のエフェクトプレハブが実質同一の仮素材であることをファイルサイズで裏付け |
| 中 | G1 | C-2 | スキル7の状態効果再付与ポリシーがマスターデータを無視してコード側でハードコード |
| 中 | G6 | A-1 | キルコードのレベルアップ時の効果音が未実装 |
| 中 | G5 | C-1 | 「設定画面をホーム以外から開けるか」の仕様書記述が矛盾したまま（実装は解決済み） |
| 中 | G4 | B-1 | `MusicConstants`（BPM/拍換算の共通定数）が音楽モジュール仕様に未記載 |
| 中 | G9 | B-1 | ローカライズ連携基盤(`LocalizationInitializer`/`LocalizedElementText`)が未文書化 |
| 低 | G1 | A-1 | コンボ表示閾値、2026-09-22決定の「3コンボ以上」が実装(4)に未反映 |
| 低 | G2 | C-1 | 敵アニメーション`Reserved`枠の用途、仕様書の「要確認」は実装で解決済み |
| 低 | G4 | B-2 | `IngameSceneView`がView層に責務を直置きしたまま未整理・未文書化 |
| 低 | G6 | C-3 | スキルツリーのTips表示が改造画面と異なり構造化されていない |
| 低 | G7 | C-1 | シナリオCSVのテスト用・旧版ファイルの`develop`フォルダ移動が未実施 |
| 低 | G8 | C-1 | 入力バッファ「読む処理が無い」の記述、Developビルド限定の購読者が実在 |
| 低 | G9 | B-2 | `ScriptableObjectRepositoryBase`（マスターデータリポジトリ共通基底）が未文書化 |
| 低 | G9 | C-1 | 用語集(`用語.md`)に`DataID`のエントリが依然として無い |
| 低 | G10 | B-1 | 雑魚敵AIの索敵待機ノード`WaitForDiscoveryAction`がノード一覧表に未記載 |
| （差分なし） | G3 | — | ミッション・シークエンス・プレイヤー行動制御・リザルトは調査済みで新規差分なし |

---

## G1. 戦闘コア系(攻撃・スキル・状態効果・ターゲティング)

**調査の前提**: `システム概要/システムリスト`配下の対象ページ(攻撃パイプライン・キャラ&バトル・スキル・スキル効果・状態効果・ターゲットシステム・レティクル)はいずれも2026-09-22〜23に一斉更新されており、実装(HEAD f0cbf828b、2026-09-19時点)より新しい。クラス構成・レイヤー・Order値・API・依存関係を実装と突き合わせた結果、ほぼ完全に一致していた(`SkillHitScheduler`の4API、各Initializerの`Order`値、`Skill_02/07/08/09/13`の依存注入、`PendingAttackEffectService`のAPI、`ConfirmedDamage`が`PlayerAttackPipeline.asset`に無いこと、`BeatType`に`All`ノーツが残っていないこと、`ClearBarrier`の未使用など、spec側が既に「未実装/要確認」と明記している項目はすべて実装側でも同じ状態を確認した)。したがって A(仕様書にあるが実装が無い)・B(実装があるが仕様書に無い)に該当する新規項目は見つからなかった。以下はspec自身が「要確認」と留保している箇所について、実装側の裏付け調査で状況を具体化できたものである。

### C-1. スキル2〜10のエフェクトプレハブが実質同一の仮素材である疑いを実データで裏付け
**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/システム概要/システムリスト/スキル効果.md:432` 「【要確認】スキル2〜10のプレハブはファイルサイズがほぼ同じで、共通の仮プレハブの可能性がある。実機とエフェクト担当に確認する」
**実装の状況** — `Assets/Level/Data/Master/InGame/Skill/Effect/SkillEffectCatalogConfig.asset` の`_entries`はskill_00〜10・13の12件で個別GUIDを持つが、実ファイルサイズを確認すると`SkillEffect_skill_02.prefab`〜`SkillEffect_skill_10.prefab`(9個、`Assets/Level/Prefabs/Master/InGame/SkillEffect/`)はすべて126,025〜126,031バイトとほぼ同一なのに対し、`SkillEffect_skill_00.prefab`は396,374バイト、`skill_01`は397,005バイト、`skill_13`は749,369バイトと明確に異なる。仕様書の懸念どおり、スキル2〜10は個別ファイルではあるものの内容が共通の仮プレハブである可能性が高いことをファイルサイズで確認した。
**優先度**: 高(エフェクト担当がプレースホルダーと本番演出を区別する判断材料になるため)

### C-2. スキル7の状態効果再付与ポリシーがマスターデータを無視してコード側でハードコードされていることを確認
**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/システム概要/システムリスト/状態効果（バフ・デバフ）.md:347` 「Replace（コードで固定）。マスター値は Ignore だが使われない【要確認】」(スキル7の行、`SkillData07.asset`を正本として参照)
**実装の状況** — `Assets/Level/Data/Master/Skill/Templates/SkillData07.asset:66` は `_statusEffectReapplyPolicy: 4`(`StatusEffectReapplyPolicy.Ignore`、`Assets/Scripts/Runtime/1.Domain/InGame/StatusEffect/StatusEffectReapplyPolicy.cs:16`)だが、`Assets/Scripts/Runtime/2.Application/InGame/Buff/AttackPowerIncreaseBuff.cs:20`と`Assets/Scripts/Runtime/2.Application/InGame/Buff/AttackPowerReductionDebuff.cs:17`はいずれもコンストラクタで`StatusEffectReapplyPolicy.Replace`を直接`base()`へ渡しており、マスターデータの値を一切参照しない。仕様書の懸念を実コードで確認した(企画側が`SkillData07.asset`のInspector値を変更しても効果が変わらない状態)。
**優先度**: 中(実害は無い(現状Replaceが意図どおりの模様)が、マスターデータ上の設定項目が死んでいるため、企画が誤って調整しようとすると気づかず時間を失う)

### A-1. コンボ表示閾値が2026-09-22決定の「3コンボ以上」に未反映
**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/仕様概要/仕様リスト/コンボについて.md:64-65` 「3コンボ以上で表示し、0〜2コンボでは表示しない…現状の実装は4コンボ以上で表示している」。同ファイル`:162`の議事録決定(八幡)No.20で「コンボは3コンボ以上で表示する(実装は4から。実装を直す)」と明記。
**実装の状況** — `Assets/Level/Scenes/Master/InGame.unity:57729` は `_comboVisibleCount: 4`のまま。対応するフィールド既定値も`Assets/Scripts/Runtime/6.Composition/InGame/Mission/InGameMissionInitializer.cs:449` で `private int _comboVisibleCount = 4;` となっており、決定事項が未反映であることをシーンアセットとコード両方で確認した。
**優先度**: 低(数値1件の調整のみで、既に仕様書・実装双方が同じ事実を把握している)

## G2. 敵・キャラクター表現系(敵AI・アニメーション・カメラ・ハプティクス)

対象仕様書はすべて2026-09-22/23の最終更新で、クラス一覧・既知の制限・「要確認」注記・経緯テーブルが実装側の現状(クリティカル被弾硬直バグ、発見システムの無効化未対応、`BeginExternalControl`/`EndExternalControl`の未使用、ボスの`OnLowHp`が演出に未接続、マーク付き砲兵の専用プレハブ未作成、カメラシェイク4種のパラメータ、`EnemyType`列挙、`DamageNumberType.Skill`の未使用化、ゲームパッド振動の仕様、等)と file:line レベルで一致しており、新規のA/B分類の差分は見つからなかった。

### C-1. 敵アニメーションの`Reserved`枠の用途が仕様書側で未解決のまま放置されている
**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/システム概要/システムリスト/キャラクターアニメーション.md:335`
> 敵は`Reserved`を含む5枠を使う（`EnemyAnimationConfig.asset`）。【要確認: Reserved枠の用途をプログラム担当に確認】

仕様書は`Reserved`枠（`CharacterAnimationClipType.Reserved = 4`）が何のために使われているかを把握できておらず、「プログラム担当に確認」が必要な未解決事項として明記されたままになっている。

**実装の状況** — 実装側には明確な答えがすでに存在する。
- `Assets/Scripts/Runtime/4.View/InGame/Animation/CharacterAnimationClipType.cs:12` — `Reserved = 4`。
- `Assets/Scripts/Runtime/4.View/InGame/Animation/CharacterAnimationView.cs:66-77` — `_context.ViewModel.IsReserving`が真のときにロコモーションのウェイトを退け（`_weights[i] *= (1f - _reserveBlend)`）、`Reserved`枠のウェイトを`_reserveBlend`まで上げる。すなわち`Reserved`は「攻撃予約中の構え」ポーズである。
- `Assets/Scripts/Runtime/4.View/InGame/Animation/CharacterAnimationViewModel.cs:14,28-31` — `IsReserving`プロパティと`SetReserving(bool)`。
- `Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyMoveView.cs:269` — `_characterAnimationViewModel?.SetReserving(_enemyAIController.IsAttacking)`。敵AIが攻撃予約中（`EnemyAIController.IsAttacking`、攻撃デカール表示〜発射までの区間）の間だけ`Reserved`枠へブレンドし、それ以外（移動中・射程外・攻撃終了後）は`false`に戻す（同ファイル 70, 243, 327, 464, 477行目）。

つまり`Reserved`は「敵が攻撃予約中に構えるポーズ」専用の枠であり、通常のIdle/Walk/Dodge/Attackとは独立してブレンドされる仕組みが実装済みである。仕様書側の「要確認」は実装を数分読めば解消できる内容だが、2026-09-23の最終更新時点でも未反映のまま残っている。

**優先度**: 低（動作に影響する不具合ではなく、仕様書のドキュメント欠落。次回の仕様更新で埋めれば足りる）

## G3. インゲーム進行制御系(ミッション・シークエンス・プレイヤー行動制御・リザルト)

前回レポート（2026-08-23）で「仕様書側に足りていない」とされたB-2（ミッションのステップ進入アクション）とB-3（戦闘ポーズ）は、いずれも解消されていた。

- B-2相当: `システム概要/システムリスト/ミッション.md`（最終更新2026-09-22）が`IMissionStepEntryAction`/`PlayVoiceStepEntryAction`/`SetSkillExecutionEnabledStepEntryAction`/`ToggleEnemyBattleAIStepEntryAction`/`PlayDialogueStepEntryAction`/`MissionStepEntryActionController`/`IMissionStepEntryActionExecutor`/`ScenarioPlaybackClearCondition`/`MissionScenarioController`/`IScenarioInputModeController`まで、クラス表・拡張ポイント・処理フローの各節で網羅している。
- B-3相当: `システム概要/システムリスト/シークエンス.md`が`BattlePauseController`/`IBattlePauseModule`/`IBattlePausable`/`IScenarioBattlePauseController`/`BattlePauseModule`を明記し、`ポーズシステム開発.md`も「これは研究資料であり製品は`BattlePauseModule`が`Time.timeScale`で実装」と誤解防止の注記を追加済み。

実装との突き合わせ（`Assets/Scripts/Runtime/{1.Domain,2.Application,3.Adaptor,4.View,5.InfraStructure,6.Composition}/InGame/{Mission,Sequence,Player,Result}/` 配下の全C#ファイル、計約190ファイル）を行い、`MissionActionKind`の全列挙値、`PlayerActionRestrictionReason.Silence`の未使用状態、各Initializerの`Order`値、`_comboVisibleCount`、`IBattlePausable`の宣言のみで参照0件、`DemoStageResultExitPolicy`など多数の項目が仕様書の記述と一致することを確認した。16個の個別クリア条件Assetクラスが仕様書のクラス表に無いが、これは`[SerializeReference, SubclassSelector]`により登録不要で追加できる拡張ポイントとして仕様書が包括的に説明しているため、欠落ではなく意図的な記述省略と判断した。

**結論**: このドメインについて、A/B/Cいずれの分類でも報告に値する具体的な差分は見つからなかった。

## G4. インゲーム音楽同期・UI表現系(音楽・リズムガイド・コンボ・UI・ステージ演出)

前回(2026-08-23)差分分析でC-1として「要確認」に留まっていた「音楽同期とリズムガイドの改修」について、今回1行単位で仕様書と実装を突き合わせて確認した結果、**この項目は解消済み**と判定した。`ACLikeRhythmGuideEffectConfig.asset`のVignette数値、`_comboVisibleCount`、`HealthBarView`の赤/緑フラッシュ、`InGameControllerGuideView`のXbox固定表記、`RhythmGuideInitializer`(死んだ初期化クラス)、`BgmSelectorLabelTableAsset`のBGM登録数など、仕様書が挙げる具体的な数値・既知の未実装項目(Issue番号付き)がすべて実装と一致することを確認した。前回B-5(ポストエフェクトオーバーレイ)も解消済み。

以下、新たに見つけた差分のみ報告する（A・Cは0件のため省略）。

### B-1. MusicConstants（BPM/拍換算の共通定数）が音楽モジュール仕様に未記載
**仕様書の記述** — 該当なし。`Docs/NotionSpecifications/Symphony Kill Chord/システム概要/システムリスト/音楽.md` のクラス表（Domain〜Compositionの全レイヤーを網羅）に `MusicConstants` の記載がない。他モジュールの仕様書（例: `敵.md`）は自モジュールが使う0.Utility層の定数クラスを記載する慣習があり、これが音楽モジュールだけ抜けている。
**実装の状況** — `Assets/Scripts/Runtime/0.Utility/Constant/MusicConstants.cs:5-33`。`SECONDS_PER_MINUTE`/`STANDARD_BEATS_PER_BAR`/`HALF_BEAT_THRESHOLD`/`BASE_BPM` とBPM→再生速度倍率変換 `GetPlaybackSpeed(double bpm)` を持つ。`RhythmDefinition.cs:29-30`、`MusicTimingCalculator.cs:30`、`MusicSyncState.cs:63,89`、`EquipmentBgmController.cs:107`、`SkillDefinition.cs:93` など音楽同期の中核計算が広くこれに依存し、`Assets/Scripts/Runtime/6.Composition/InGame/Skill/SkillInitializer.cs:195` は前回差分分析(2026-08-23 B-1)で指摘された「スキルエフェクトのBPM連動再生速度」を実際に `MusicConstants.GetPlaybackSpeed` で実装している。
**優先度**: 中（前回報告のスキルエフェクト演出基盤(B-1)が参照する計算式の実体であり、音楽モジュールのドキュメントとして本来含めるべき基礎定数のため）

### B-2. IngameSceneView（インゲーム内シーン追加ロード）が未文書化・レイヤー未整理のまま存在
**仕様書の記述** — 該当なし。`シーンマネージャー.md` を含め、Notion仕様書全体を検索しても `IngameSceneView`/`LoadScene`/`UnloadScene` の記載は1件もヒットしない。
**実装の状況** — `Assets/Scripts/Runtime/4.View/InGame/Scene/IngameSceneView.cs:11-13`。クラス冒頭のコメントで `"いったんここにすべて実装。後でレイヤー分けする"` と明記されており、シーンの追加ロード(`LoadScene:22-28`)・アンロード(`UnloadScene:35-47`)・全アンロード(`UnloadAllScenes:52-55`)というAdaptor/Application相当の責務を、Composition層のDIも介さずView層のMonoBehaviourに直置きしている。`DesignPhilosophy.md`のレイヤー原則（View→Adaptorのみ参照可、Viewはゲームオブジェクト操作に限定）から逸脱した状態が実装コメントで自認されたまま残っている。
**優先度**: 低（機能は成立しているが、設計原則からの既知の逸脱がドキュメント・追跡Issueのどちらにも残されていない）

**補足（調査範囲）**: `InGame/Music`・`InGame/Combo`・`InGame/UI`・`InGame/Stage`（弾幕ECSを含む）・`InGame/PostEffect`の全層（0.Utility〜6.Composition）のファイルを列挙し、対応する仕様書クラス表と1対1で突き合わせた。結果、上記2件を除く全クラスが仕様書側に記載済み、または仕様書側で「未実装」「要確認」としてIssue/PR番号付きで既に追跡されている（＝仕様と実装が一致した状態）ため、A・Cに該当する新規差分はゼロ件だった。`InGame/Scene`フォルダは`IngameSceneView.cs`1ファイルのみで全量確認済み。

## G5. アウトゲーム画面遷移・メタ系(ステージセレクト・出撃・戦闘準備・タイトル・スクリーン)

**調査メモ**: 対象仕様書(`スクリーン.md` `ステージセレクト.md` `出撃.md` `戦闘準備.md` `タイトル.md` `ホームページ仕様.md` および `仕様概要/仕様リスト` 配下5件)はいずれも2026-09-22/23に一斉更新されており、実装済みクラス構成・Issue番号まで具体的にトレースされていた。前回レポートのB-6(制作メンバー一覧/クレジット)は、`スクリーン.md:176-204,388-415`に`MemberData`/`MemberCsvRepository`等のクラス表とシーケンス図が追記されており**解消済み**。戦闘準備画面の廃止(PR #1839)についても実装側に`BattlePreparation`という語がenum予約値以外に一切残っていないことを確認し、`戦闘準備.md`の記述と一致している。以下は残存する差分のみ。

### B-1. OutGameの空間ナビゲーション基盤(`4.View/OutGame/Navigation`)

**実装の状況** — Home/StageSelect/SkillTree/SkillBuild/Setting/Creditのゲームパッド・キーボード操作を横断的に支えるViewインフラ一式。
- `Assets/Scripts/Runtime/4.View/OutGame/Navigation/SpatialNavigationResolver.cs:15` — UI Toolkit標準の自動ナビゲーションでは正しく次候補を拾えない非格子レイアウト(スキルツリーの木構造など)向けに、実座標から方向候補を独自解決するリゾルバ。
- `Assets/Scripts/Runtime/4.View/OutGame/Navigation/HierarchicalNavigationScope.cs:10` — 親子関係を持つ操作レベルを切り替え、現在レベルだけをフォーカス可能にするクラス。
- `Assets/Scripts/Runtime/4.View/OutGame/Navigation/ModalNavigationScope.cs:18` — モーダル表示中、内側だけにフォーカスを閉じ込めるクラス。
- `Assets/Scripts/Runtime/4.View/OutGame/Navigation/UIActivationEvent.cs:8` / `UINavigationExtensions.cs:14` — クリック以外の経路(コントローラーSubmit)からの作動要求イベントと、要素をマウス/コントローラー両対応にする拡張メソッド群。
- `Assets/Scripts/Runtime/4.View/OutGame/Navigation/NavigationDebugLog.cs:10` — フォーカス移動の診断ログ(コメント上は「調査が終わったら削除」の一時コード)。

**仕様書の記述** — 該当なし。`ModalNavigationScope`は`Tutorial.md:222`で1回だけ名前が出るのみで、モジュールとしての説明は無い。`ホーム画面.md:63-93`(コントローラーのフォーカス移動先の表)や`作戦画面.md:248-250`(「詳細を開いている間は、閉じるまでフォーカスが詳細の外に出ない」)はこの基盤が実現している挙動を記述しているが、実装モジュール名(`SpatialNavigationResolver`等)には一度も触れていない。

**優先度**: 高(複数の既存画面のコントローラー操作を裏で支える共通基盤であり、UI担当が触る際の入口になるため)

### C-1. 設定画面をホーム以外から開けるかという保留事項

**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/仕様概要/仕様リスト/アウトゲーム遷移フロー.md:249`「設定画面をホーム以外（作戦・改造・研究）から開けるか: 保留（要実機確認）」。一方で同じ仕様書ツリー内の`作戦画面.md:251`には「作戦画面から設定画面を開くボタンがある」と明記されており、仕様書内で記述が矛盾したまま残っている。

**実装の状況** — 実装は既に上記の問いに答えを出している。`OutGameUIEvent.OnShownSettingScreen`(`Assets/Scripts/Runtime/4.View/OutGame/Screen/OutGameUIEvent.cs:47`)が、Home(`Assets/Scripts/Runtime/4.View/OutGame/Screen/HomeScreenView.cs:263`)・StageSelect(`Assets/Scripts/Runtime/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs:583-588`、UI要素名`"SettingShortcutButton"`は同ファイル104行)・SkillTree(`Assets/Scripts/Runtime/4.View/OutGame/Screen/SkillTreeScreenView.cs:151-153`)・SkillBuild(`Assets/Scripts/Runtime/4.View/OutGame/Screen/SkillBuildScreenView.cs:588-590`)の4画面すべてから発火されており、`ScreenInitializer.HandleSettingsShown`(`Assets/Scripts/Runtime/6.Composition/OutGame/Screen/ScreenInitializer.cs:543-548`)がそれを受けて`ShowSetting()`する。したがって「作戦・改造・研究のいずれからも設定画面を開ける」ことはコード上明確であり、`アウトゲーム遷移フロー.md`側の「保留」表記が古い。

**優先度**: 中(実機確認を待つまでもなくコードで裏付けが取れるため、仕様書側の「保留」を解除できる)

## G6. 育成・改造系(スキルツリー・改造/研究画面)

### A-1. キルコードのレベルアップ時の効果音が未実装
**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/仕様概要/仕様リスト/改造画面.md:90` 「キルコードをセットした時とレベルが上昇したときに、効果音を流す。」（セット時は「カチッ」という効果音の指定あり、レベルアップ時も効果音を流す旨は明記されている）
**実装の状況** — `Assets/Scripts/Runtime/3.Adaptor/OutGame/Audio/UISoundEffectKind.cs:6-13` の列挙体は `Select`/`SkillSet` の2種類のみで、レベルアップ専用の効果音種別が存在しない。レベルアップの実処理は `Assets/Scripts/Runtime/6.Composition/OutGame/SkillBuild/SkillBuildInitializer.cs:618-631`(`HandleSkillLevelUpHandler`) → `Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillBuild/SkillBuildController.cs:77`(`LevelUpAsync`) → `Assets/Scripts/Runtime/2.Application/OutGame/SkillBuild/SkillBuildUseCase.cs:56`(`LevelUpSkillAsync`) → `Assets/Scripts/Runtime/5.InfraStructure/OutGame/SkillBuild/SkillBuildRepository.cs:115`(`TryLevelUpSkillAsync`) の経路だが、いずれの層にも `IUISoundEffectCommand`/`soundEffectCommand` の参照が無い。セット時の効果音は `Assets/Scripts/Runtime/4.View/OutGame/SkillBuild/SkillElementControllerEquipController.cs:606` と `SkillElementDragAndDropSetup.cs:124` で `UISoundEffectKind.SkillSet` を再生しており実装済みだが、レベルアップ側は完全に欠落している。
**優先度**: 中（TGS体験版の未実装リストにも挙げられておらず、仕様との単純な取りこぼしと見られる）

### C-1. 研究画面のプレビュー動画ボタンが実装側で一時的に無効化されている
**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/仕様概要/仕様リスト/研究画面.md:78` 「使用映像はクリックした時に、周りを少し暗くさせて映像を出す。使用映像を設定しているのはN2・N3・N6の3つのマスだけで、それ以外のマスでは映像のボタンを押せない」— 有効な機能として記載されており、`研究画面.md`の「未実装の項目」に該当する記述は無い。`スキルツリー.md:136-139`のクラス表にも`PreviewVideoScreenView`が現行機能として記載されている。
**実装の状況** — `Assets/Scripts/Runtime/4.View/OutGame/SkillTree/SkillDetailScreenView.cs:96-97` に `// プレビュー再生ボタンは一時的に非表示にしている。` というコメント付きで `_previewVideoButton.style.display = DisplayStyle.None;` が常時実行されており、`dto.HasPreviewVideo`（N2/N3/N6判定、マスターデータは`SkillNodeData-N2.asset`等に`PreviewVideoClip`設定済みで裏付けあり）による`SetEnabled`(`SkillDetailScreenView.cs:117`)より後に強制的に非表示へ上書きされ、ボタンは常に見えない。この変更は同ブランチ上のコミット`c6349c4c5`(2026-09-15, 作者 KoukiMatsushita)によるもの。`研究画面.md`・`改造画面.md`いずれの「未実装の項目」節にもこの一時無効化の記載が無い。
**優先度**: 高（体験版直前の意図的な無効化が仕様書に反映されておらず、他エージェントが「実装済み」と誤認する可能性がある）

### C-2. 装備枠拡張ノードにフラグが立っておらず「編成枠が増えない」仕様書の懸念が実データで確認された
**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/仕様概要/仕様リスト/研究画面.md:165-167` 「編成枠を1つ増やすマス（現在はN7「装備スロットを一枠解放」、コスト20）を解放すると、編成枠が1つ増える…現在のデータでは、N7を含むすべてのマスでこの設定が入っておらず、編成枠が増えない可能性がある【要確認: 実機で確認し、データの修正を起票】」— 断定はせず「可能性がある」という要確認扱い。
**実装の状況** — `Assets/Scripts/Runtime/5.InfraStructure/OutGame/SkillTree/SkillNodeData.cs:36`(`_hasSkillSlotBonus`)を実データ全49ノードに対して確認したところ、`Assets/Level/Data/Master/OutGame/SkillTree/SkillNodeData/SkillNodeData-N7.asset`（コスト20、説明文「装備スロットを一枠解放」で仕様書と一致）を含め、フラグが`1`になっているノードは1件も無い。すなわち仕様書が「可能性」として保留した状態は、現状のマスターデータでは確定的な事実（N7を解放しても編成枠は増えない）である。
**優先度**: 高（要確認の裏取りが取れたため、企画確認を待たずデータ起票に進められる状態）

### C-3. スキルツリー詳細のTips表示が改造画面と異なり単一テキストへ埋め込まれている
**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/仕様概要/仕様リスト/研究画面.md:75-76` 「キルコードをクリックしたら…キルコードの映像と説明、キルコード獲得ボタンが出る。キルコードの説明の下に各実験体思い出一部を記載」— 説明とTips相当の文章が別項目であるかのように読める記述。`改造画面.md:195`（2026-07-27決定）は改造画面・作戦準備画面について「効果の表示は『種類』『効果』『Tips』の順にする」と明記し、`スキル編成.md`のTipsは`SkillViewData.Tips`という専用フィールドで実装されている。
**実装の状況** — スキルツリー(研究画面)側は`Assets/Level/Data/Master/OutGame/SkillTree/SkillNodeData/SkillNodeData-N7.asset`の`SkillDetail`フィールドに「装備スロットを一枠解放\n\nTips\n四輪駆動車、…」のように効果文とTips文が改行区切りの一つの文字列として直接埋め込まれており、`Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/SkillDetailDTO.cs:18,54`(`SkillDetail`)・`Assets/Scripts/Runtime/4.View/OutGame/SkillTree/SkillDetailScreenView.cs:92`(`_skillDetail.text = dto.SkillDetail;`)は専用のTips欄を持たず単一ラベルへそのまま表示する。改造画面のような構造化されたTips欄（`SkillDetailView.cs:81-83`の`_tipsLabel`）は存在しない。
**優先度**: 低（表示結果としては改行で視覚的に区切られるため実害は小さいが、データ入力者が手打ちで「Tips」という文字列を埋め込む前提の仕様であり、改造画面側の構造化方式と統一されていない）

**調査補足**: `研究モック.md`は2026-09-22の決定でページ名が「研究」に変更されており、現在は`仕様概要/仕様リスト/研究.md`として存在する。同ページはアウトゲームの研究画面（スキルツリー）とは関係が無いα版プロトタイプの記録であり、本ドメインとは無関係のため対象から除外した。`スキルツリー開発について.md`は自身が「2026-05時点の旧設計のアーカイブ」であることを明記済みのため、差分としては報告していない。

## G7. シナリオ・リソース・チュートリアル系

対象範囲: `OutGame/{Scenario,Resource,Tutorial}` 配下の全実装と5つの仕様書。この領域の仕様書は2026-09-22〜23に大規模改訂されており、実装から逆生成されたと明記される`ScenarioCommandSpec.md`を含め、実装との整合性が極めて高い。`ScenarioSettingsAsset.asset`の実値、`TutorialOverlayView.cs`の数値、`OutGameTutorialInitializer.cs`の文言など、仕様書に記載された具体的数値・文言を実装と照合したが、いずれも一致した。仕様書が「未実装」と明記する項目も、対応する実装が実際に存在しないことを確認済みで、これらは差分ではない。A/B分類に該当する明確な差分は見つからなかった。

### C-1. シナリオCSVの旧版・テスト用ファイルの `develop` フォルダへの移動が未実施

**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/仕様概要/仕様リスト/シナリオパート.md:396-406`
> 2026-09-22 決定（八幡）R71: 「第1話の CSV の名前を直し、テスト用の CSV を `develop` フォルダへ移す（#2078）」
> 2026-09-22 決定（八幡）R108: 「テスト用・旧版のシナリオ CSV は `develop` という名前のフォルダへ移す（#2078）」

**実装の状況** — `Assets/StreamingAssets/ScenarioAuthoring/` 配下に `develop` サブフォルダは存在しない。テスト用・旧版とみられるファイル（`ScenarioTest.events.csv`、`InGameScenarioTest.events.csv`、`ScenarioDisplayExtensionTest.events.csv`、`Test.events.csv`、`Episode_1_first.events.csv`、`Episode_1_first(edit).events.csv`、`Episode_1_second.events.csv`）が本番用（`Episode_1.events.csv`〜`Episode_12.events.csv`）と同じ`ScenarioAuthoring/`直下に混在したままである。決定から2日経過しているが、フォルダ分離は未反映。

**優先度**: 低（プレイ挙動には影響しないデータ整理タスクだが、`Episode_1_first(edit)` のような命名の重複ファイルが本番シナリオIDと混在しており、誤って参照されるリスクがある）

## G8. 永続化コア系(セーブ・入力・シーン管理・ロード)

**調査範囲**: `Persistent/{Savedata,Input,SceneManagement,Load}` と対応する8本のNotion仕様書。対象仕様書は2026-09-22〜23に最新化されており、実装との整合性を約30項目にわたって file:line で突き合わせたが、A・Bに該当する項目は見つからなかった。仕様書側が既に「未実装」「要確認」と明記している既知のギャップ(音量以外を保持しないリセット処理、体験版終了時の全削除、暗号化未実装、`PersistentFileSaveDataLoaderStrategy`の死骸化、ゲームパッドEast/South配置の未実装等)は、いずれも実装と完全に一致していることを確認済みであり、新規の差分ではない。

### C-1. 入力バッファの「読む処理が無い」という記述と、デバッグ用購読者の存在

**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/システム概要/システムリスト/入力.md:65` `InputBufferingQueue`「先行入力のために残している（現状は読む処理が無い）」、および同ファイル:237「記録した履歴を読むゲーム側の処理は現在無い」。

**実装の状況** — `Assets/Scripts/Runtime/3.Adaptor/InGame/Player/PlayerController.cs:20,27` は `InputBufferingQueue` を受け取ってフィールドに保持するのみで、以降どこからも読み出していない（仕様の記述通り）。一方、`Assets/Scripts/Develop/Composition/Persistent/Input/InputDebugLogger.cs:16-17` は `InputComposition.GetBufferedInputBuffer` から取得した `InputBufferingQueue` の `OnBuffered` イベントを購読し、`Debug.Log` へ出力している。ゲームロジックとしての消費者は確かに存在しないが、「読む処理が完全に無い」わけではなく、Developレイヤーのデバッグ購読者が1件だけ存在する。

**優先度**: 低（Developビルド限定のログ出力であり、ゲームプレイへの影響はない。仕様書の文言を「ゲームロジックとして読む処理はまだ無い」等に微修正すれば解消する程度の指摘）

## G9. 音量設定・ポストエフェクト・横断基盤系

### 前回指摘事項の解消確認(参考情報)

前回レポートのB-4(音量設定の永続化とUI操作音)、B-5(ポストエフェクトオーバーレイ)、B-10(DataIDとSourceData属性群)は、いずれも**今回のエクスポート(2026-09-23取得)で解消済み**であることを確認した(B-4→`セッティング.md:160-218`+`音楽.md:238-256`、B-5→`カメラ.md:128-253`+`リズムガイド全画面演出.md:230-255`、B-10→`マスターデータ運用ガイド（プランナー向け）.md:626-651`+新規ページ`システム概要/システムリスト/SourceDataProvider.md`)。

以下、今回新たに見つかった差分のみを報告する。

### B-1. ローカライズ連携の技術実装(`Persistent/Localization`)がシステムリストに未記載

**実装の状況** — `Assets/Scripts/Runtime/4.View/Persistent/Localization/LocalizationInitializer.cs:10`(`LocalizationSettings.InitializationOperation`の完了を待って処理を実行する静的クラス)、`Assets/Scripts/Runtime/4.View/Persistent/Localization/LocalizedElementText.cs:9`(`LocalizedString`購読とフォールバック表示を担うUI連携クラス)。`LocalizedElementText`は`OutGame/Screen`・`OutGame/SkillBuild`・`OutGame/SkillTree`・`OutGame/StageSelect`・`OutGame/Title`・`OutGame/Setting`・`Persistent/Load`・`InGame/UI`にまたがる21ファイルから参照されている。

**仕様書の記述** — 該当なし。`仕様概要/仕様リスト/ローカライズ.md`は対応言語・翻訳テーブル・訳語運用という設計仕様のみを扱い、クラス名は一切登場しない。`システム概要/システムリスト/`配下に「ローカライズ」を扱う専用ページが無く(`セッティング.md`の`LanguageApplier`行が唯一のクラス言及)、`LocalizationInitializer`/`LocalizedElementText`という初期化待機・UI連携の共通基盤は技術文書に一切現れない。

**優先度**: 中(21ファイルから使われる横断的な仕組みであり、Music/Setting/PostEffect/SourceDataProviderが専用システムリストページを持つのと対照的に技術ドキュメントの空白域になっている)

### B-2. `ScriptableObjectRepositoryBase<TId,TValue,TEntry>`(マスターデータリポジトリ共通基底)が未記載

**実装の状況** — `Assets/Scripts/Runtime/5.InfraStructure/Repository/ScriptableObjectRepositoryBase.cs:12`。ID検索用辞書のキャッシュ・遅延構築(`TryFind`/`InvalidateCache`/`GetAllValues`)を提供する抽象基底で、`CharacterDefinitionRepository`・`EnemyDefinitionRepository`・`EnemyWaveDefinitionRepository`・`EnemyMissionKeyRepository`・`MissionDefinitionRepository`・`GameResourceDefinitionRepository`・`SkillNodeBindRepo`・`SkillNodeDataRepo`・`SkillRepository`の9箇所が継承している。

**仕様書の記述** — 該当なし。`マスターデータ運用ガイド（プランナー向け）.md`や`SourceDataProvider.md`はプランナー操作・DataID採番・Addressables登録は詳細に記述するが、各`*Repository`が実際にどうIDから値を引くか(共通基底の存在)には触れていない。

**優先度**: 低(プランナー運用に影響しない実装内部の共通化であり、知らなくても運用ガイドの手順は成立するため)

### C-1. `用語.md`に`DataID`の用語集エントリが依然として無い

**仕様書の記述** — `Symphony Kill Chord/用語.md:12-123`(用語の対応表)には他の中核概念はすべて正式名・別名・コード上の名前が対応付けられて登録されているが、`DataID`という語自体はこの表にも本文にも1件もヒットしない。`マスターデータ運用ガイド（プランナー向け）.md:626`や`SourceDataProvider.md:54`には概念自体は厚く記載されている。

**実装の状況** — `Assets/Scripts/Runtime/0.Utility/Identity/DataID.cs:11`(`public struct DataID`)。実装は健在で、前回報告時から変化なし。

**優先度**: 低(技術ドキュメント側は前回のB-10指摘を受けて十分に整備されたが、プランナー・企画が最初に参照する用語集への横展開だけが漏れている軽微な後追い漏れ)

## G10. 技術系リファレンス・方針/企画系文書

`システム概要/システムリスト`配下の技術系文書12件はすべて2026-09-22〜23に更新されており、実装ファイルパス・クラス名・パッケージ名を伴う記述の大半が実装と一致していることを確認した（SpringBone仕様概要, GCスパイク対策, リアルトゥーンシェーダー, UI構築フロー, Adaptive Performance Package, SinfoniaAssetManager/Operator 等）。既知の未解決点は文書自身が【要確認】として既に記録済みのため、新規の指摘としては報告しない。

`仕様概要/仕様リスト`配下の方針・企画系6件（企画概要書・α版/β版開発指針・体験版開発指針・体験版仕様・レベルデザイン方針）は、いずれも「記録（アーカイブ）」であることを明記した上で現行仕様への参照リンクを持ち、既知の食い違いもすべて文書内の経緯欄・【要確認】として既に記録されている。実装のマスターデータとも矛盾する記述は見つからなかった（方針文書の矛盾＝D分類は該当なし）。

### B-1. 雑魚敵AIの索敵待機ノード「WaitForDiscoveryAction」が未記載

**仕様書の記述** — `Docs/NotionSpecifications/Symphony Kill Chord/システム概要/システムリスト/Behavior Graphについて.md:126-186`（「作成済みノード一覧」表）。雑魚敵用ノードとして列挙されているのは `AttackTargetAction` / `GetStunnedAction` / `MoveToAttackAction` / `StopMovingAction` / `IsTargetInAttackRangeCondition` / `IsAimSightClearCondition` / `IsAttackingCondition` / `IsStunnedCondition` / `IsDiscoveredCondition` / `IsBattleAiActivatedCondition` の10種のみ。

**実装の状況** — `Assets/Scripts/Runtime/4.View/InGame/Enemy/BehaviorGraphNode/Action/WaitForDiscoveryAction.cs:11` に `Unity.Behavior.Action` を継承した `WaitForDiscoveryAction` クラスが実装されている。`OnUpdate()`（同ファイル24-46行）で硬直中断・発見成功・索敵継続（`LookAround()`呼び出し）を分岐する索敵ステートノードであり、`Assets/Level/BehaviorGraphs/EnemyAIGraph.asset` から3箇所で実際に参照されている。ボス用グラフ（`BossGraph.asset`）には対応するノードが無い。

**優先度**: 低（雑魚敵AIの索敵挙動を理解する際の一次資料である「作成済みノード一覧」表に漏れがあるため、AI担当がノード一覧だけを見て把握しようとすると見落とす）
