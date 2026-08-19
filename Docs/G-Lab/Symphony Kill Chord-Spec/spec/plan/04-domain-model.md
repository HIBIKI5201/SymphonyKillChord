# Stage 3: 仕様起点ドメインモデル（再定義）

**ステータス: complete（静的仕様証拠について完了）。** 2026-07-18 に公開仕様 35/35 ページと Ludus public OKF commit `b949cfa136fa27de101ace324f99a715f17e6846` を確認して再定義した。ランタイム観測は未実施である。境界名と直下の説明文は、今回の Anatomia 入力でバイト単位に固定する。

実装レイヤ、Unityの `MonoBehaviour`、Prefab、Scene、View、Presenter、Repository はドメインそのものではない。以下はプレイヤーに約束するルールと、その不変条件が一緒に変化する単位である。

## Musical Time and Adaptive Arrangement

音楽時間、BPM・拍子・小節位置、ビート同期スケジューリング、および装備中のキルコードに応じたイントロ・ループ・アウトロと2小節ブロックの動的編成を所有する。

- 集約/エンティティ: `MusicSyncSpec`、音楽時計、予約アクション列、BGMセクション/ブロック編成。
- 値オブジェクト: BPM、拍子、小節位置、再生区間、実行要求時刻。
- コマンド/イベント: 音楽開始、ブロック編成確定、予約、ビート到達、イントロ/ループ/アウトロ遷移。
- 不変条件: 全同期処理は同じ音楽時計を使う。予約は要求位置以後に一度だけ実行する。2/3スロット編成は仕様順を保つ。

## Rhythm Input and Kill Chord Resolution

攻撃・回避入力の間隔をノーツへ判定し、All/Just/タイムアウト規則とアクション種別を使ってキルコード列を照合し発動可否を決定する。

- 集約/エンティティ: リズム入力列、ノーツ状態、キルコード照合状態。
- 値オブジェクト: beat type、All note、Just判定範囲、リズムパターン、アクション種別。
- コマンド/イベント: 入力記録、判定、列の前進/リセット、キルコード成立/失敗。
- 不変条件: All note は列の先頭だけに使える。1.5小節以上の空白と被弾直後は基準入力になる。拍種6/8種類の矛盾は決定されるまで明示する。

## Player Action and Combat Targeting

移動、照準、攻撃、回避、硬直・クールダウン・キャンセル、遮蔽物を含む標的選択を所有し、プレイヤー意図を戦闘要求へ変換する。

- 集約/エンティティ: プレイヤー行動状態、攻撃選択、回避状態、標的候補。
- 値オブジェクト: 移動入力、照準方向、硬直時間、クールダウン、line-of-sight結果。
- コマンド/イベント: 移動、照準、攻撃、回避、標的確定、キャンセル。
- 不変条件: 障害物越しの射撃は命中しない。硬直/クールダウン中の入力可否と回避キャンセルは一つの方針で決まる。PCとAndroidの入力面は同じ行動意味へ変換する。

## Combat State and Effect Resolution

体力と能力値、攻撃定義、ダメージ・クリティカル、多段ヒット抑制、バフ・デバフ・回復の一回限りの適用を所有する。

- 集約/エンティティ: Character、Health、Attack、Effect collection。
- 値オブジェクト: HP、攻撃/防御、クリティカル率/倍率、効果量、継続時間。
- コマンド/イベント: 攻撃解決、被弾、回復、効果追加/失効、死亡。
- 不変条件: HPは範囲内に留まる。攻撃pipelineは定義順で一度だけ適用する。同一攻撃の多段適用を仕様方針に従って抑制する。

## Enemy Encounter and Stage Simulation

敵の判断、スポーン・プール・ウェーブ、予告付きビート攻撃、ボス、ステージギミック、および遭遇開始から終了までのシミュレーションを所有する。

- 集約/エンティティ: Enemy、Wave、Spawner、Boss phase、Stage effect。
- 値オブジェクト: enemy type、wave definition、attack area/line、stage effect ID。
- コマンド/イベント: spawn、wave開始/終了、敵攻撃予約、撃破、ギミック発火、遭遇終了。
- 不変条件: wave index/loop点とspawn数はauthoring範囲内。攻撃前に可読な予告を出す。ステージ効果は既知IDへ解決する。

## Mission Evaluation and Result

メイン・サブミッション、成功・失敗条件、経過時間・最大コンボ・ランク・報酬、勝敗リザルトと再出撃判断を所有する。

- 集約/エンティティ: Mission、条件木、評価記録、Stage result。
- 値オブジェクト: mission ID、終了理由、rank、elapsed time、combo、reward。
- コマンド/イベント: 進捗記録、成功/失敗、評価、報酬確定、完了、再出撃。
- 不変条件: 終了理由と結果は一度だけ確定する。報酬は定義された条件で一度だけ付与する。Home/作戦画面の戻り先矛盾は解決されるまで遷移側へ委ねない。

## Progression, Research and Loadout

ステージ解放、報酬ポイント、研究ツリー、スキル獲得・レベル、装備スロットと末尾ノーツ競合、出撃編成を所有する。

- 集約/エンティティ: Stage tree、Skill tree、Kill Chord、Loadout、Owned skill collection。
- 値オブジェクト: stage/node/skill ID、unlock cost、skill level、slot、progression point。
- コマンド/イベント: 報酬受取、node解放/リセット、level up、装備/解除、編成確定。
- 不変条件: 前提node/stageを満たす。未所持スキルは装備不可。同じ末尾ノーツのキルコードは同時装備不可。初期2枠、条件達成後3枠を守る。

## Narrative and Game Flow

新規/継続開始、シナリオ読了、ホーム・作戦・準備・戦闘・リザルト間の遷移、およびシーン完了の一回性を所有する。

- 集約/エンティティ: Game flow session、Scenario event stream、Screen/scene state、selected sortie。
- 値オブジェクト: scenario ID、event row、screen ID、transition rule。
- コマンド/イベント: new/continue、シナリオ再生/skip、screen open/close、出撃、scene loaded/completed。
- 不変条件: 初回は物語→戦闘tutorial→Home tutorialへ進む。Homeはアウトゲームhubである。完了通知とscene遷移は一度だけ発火する。

## Persistence and Player Settings

五分類の永続データ、プラットフォーム別保存、暗号化・移行・破損回復、および音量・操作・表示設定の確定/取消を所有する。

- 集約/エンティティ: Save data、Save slot、Settings draft/committed state。
- 値オブジェクト: save version、`DataID`、volume、display/input option、repository address。
- コマンド/イベント: load、save、migrate、reset、setting変更/確定/取消、corruption検出。
- 不変条件: 五分類を欠落させない。ID移行は冪等。保存失敗/破損は明示し、既存進捗を原子的に守る。未確定設定はキャンセルで破棄できる。

## Guidance, Feedback and Recovery

リズム・BGM要素・敵攻撃・ミッションの可視化、チュートリアル転移、ポーズ/再開カウントダウン、失敗Tips、ロード進捗を通じて行動発見と回復可能性を所有する。

- 集約/エンティティ: Guidance session、Tutorial step、Feedback cue、Recovery state。
- 値オブジェクト: cue priority、progress、tip、countdown、input prompt。
- コマンド/イベント: cue表示/消去、tutorial step達成、pause/resume、loading進捗、失敗tip選択。
- 不変条件: 音だけに依存せず同等の視覚手掛かりを出す。敵攻撃は反応前に予告する。初回学習は通常ステージの操作へ転移可能にする。中断/失敗後は次の有効行動を示す。

## 境界上の判断

- Unity lifecycle、Scene/Prefab/ScriptableObject、UI Toolkit、CRI、Addressables、Service Locator、Composition initializer は実装・基盤の証拠として別集計し、製品ドメインへ昇格しない。
- `Music` フォルダは Musical Time と Rhythm Input の二つの意味を含むため、ファイル名・型名の証拠で分ける。フォルダ境界だけで同一ドメインとみなさない。
- `Skill` は入力成立（Rhythm Input）、効果解決（Combat State）、所有/装備（Progression）の三境界に跨る。Anatomiaのoverlapは欠陥の証明ではなく、責務分割候補を探す信号として扱う。
- Guidance は表示技術ではなくプレイヤー向けルールを所有する。View/Presenterの詳細はこの境界のadapterであり、ドメイン定義を置換しない。
