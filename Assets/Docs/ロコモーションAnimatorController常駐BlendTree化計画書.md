# ロコモーションAnimatorController常駐BlendTree化計画書

- 作成日: 2026-07-17
- 対象: キャラクターアニメーション基盤のロコモーション（Idle/Walk等のステートアニメーション）部分
- 関連文書: `Assets/Docs/攻撃アニメーション遷移改善計画書.md`（先行実施。本計画はその後に着手する）
- 目的:
  - ロコモーションを`AnimatorControllerPlayable`（常駐BlendTree）へ移行し、**「ステートアニメーション＝AnimatorController / ワンショット＝コード制御のPlayable」という境目を構造として明確にする**
  - 敵キャラクター等の拡張時、ロコモーションのバリエーション追加（歩き分け・方向ブレンド等）を**コード変更なしにControllerアセットの編集だけ**で行えるようにする
  - 音楽同期（BPM速度同期＋位相同期）を維持する

---

## 1. 背景と方針決定の経緯

### 1.1 音楽位相同期の要件

このゲームのロコモーションは音楽に**位相まで**同期する。仕様例: 移動クリップが2秒・BPM60のとき、曲の2.3秒目に移動入力すると、クリップは0.3秒目から表示される（`クリップ時間 = 音楽時間 mod クリップ長`）。

現行実装（`PlayableAnimationController`、`Assets/Scripts/Runtime/4.View/InGame/Animation/PlayableAnimationController.cs`）では、全クリップを初期化時に再生開始して以後一切再スタートせず、ウェイトだけを操作するため、この位相同期が**「クリップを止めないこと」の副産物として自動成立**している。

### 1.2 AnimatorController化の検討結果（決定事項）

AnimatorControllerで位相同期を保つ方法は2つ検討した。

- **方法A（不採用）**: ステート遷移のたびにコードから`Play(state, layer, normalizedTime)`で位相を指定する。Controllerのアセット側遷移（遷移オフセットは固定値）が位相同期に使えず、結局全遷移がコード経由になり、Controller化の利点が消えるため不採用。
- **方法B（採用）**: **ロコモーションを1つのBlendTreeステートに常駐させ、一度も遷移で出ない**。ステートが再スタートしないため現行Playable方式と同じ原理で位相が保たれる。
  - 制約: 1D BlendTreeは子クリップを正規化時間で同期させるため、**子クリップの拍数（尺）が揃っていないとブレンド比率に応じて実効速度が変わり位相がズレる**。→ **「BlendTreeに入れる全クリップは同一拍数（または整数倍）で制作する」規約**を採用する。現状のクリップはこの制約を満たせることを確認済み（チーム判断）。

---

## 2. 現状整理

### 2.1 現行のロコモーション実装（置き換え対象）

- `CharacterAnimationLocomotionCalculator`（`Assets/Scripts/Runtime/4.View/InGame/Animation/CharacterAnimationLocomotionCalculator.cs`）
  - `ApplyBaseWeights`（L41-64）: 速度の大きさからIdle/Walkのウェイトを手計算（`WALK_THRESHOLD = 0.1`、walkWeight = 速度のClamp01）。
  - `AnimationSpeed`（L17）: BPM/60の再生速度。
- `CharacterAnimationView.Update`（`CharacterAnimationView.cs:48-71`）: 毎フレーム上記ウェイトをMixerへ反映し、その上にワンショットのオーバーレイウェイトを重ねる。
- Mixer入力はenum`CharacterAnimationClipType`（Idle=0, Walk=1, Dodge=2, Attack=3）＋動的追加分（BeatType攻撃・ワンショットキー）の1次元配列（`AnimationComposition.Init`が構築）。

### 2.2 現状の既知ギャップ（本計画で同時に解消する）

- **途中スポーン時の位相ズレ**: `PlayableAnimationController`のコンストラクタは`_graph.Play()`するだけで、曲の現在時刻への頭出しをしていない。曲の途中でスポーンする敵は、グラフ生成時刻からの経過時間がクリップ時間になるため、**音楽との位相が合っていない**。プレイヤーは曲開始と同時に初期化されるため顕在化していないだけ。
  - 音楽の現在時刻は`MusicSyncState.PlayTime`（`Assets/Scripts/Runtime/3.Adaptor/InGame/Music/MusicSyncState.cs:12`、double）で取得できる。
- **Idle↔Walk切り替えの段差**: walkウェイトが速度の生値で、`WALK_THRESHOLD`を跨いだ瞬間に段差が出る（現状は物理側の加減速が実質のスムージング）。BlendTree化に伴いパラメータ側で減衰を掛けられる構造にする。

---

## 3. 設計

### 3.1 グラフ構成（変更後）

```
PlayableGraph（既存・音楽同期の主導権はここのまま）
 └ AnimationMixerPlayable
     ├ [0] AnimatorControllerPlayable  … ロコモーション常駐BlendTree（本計画で新設）
     ├ [1] 回避クリップ                … ワンショット（コード制御・従来どおり）
     ├ [2] 攻撃クリップ（既定）
     └ [3..n] BeatType攻撃・キー指定ワンショット
```

- ワンショット側の仕組み（オーバーレイウェイト計算、課題1の同一クリップ判定、課題2の移動キャンセル、`CharacterAnimationRequest`のオプション駆動）は**一切変更しない**。
- オーバーレイによるベース減衰は従来の`otherScale = 1f - weight`がそのまま入力[0]に効く（ロコモーションが1入力に集約されるためむしろ単純化する）。

### 3.2 AnimatorControllerアセットの規約

- **1レイヤー・1ステートのみ**。ステートは1D BlendTree（パラメータ: `Speed`、float）で、Idle（threshold 0）〜Walk（threshold 1）を配置する。
- **遷移矢印を一切作らない**（Entry→ステートのデフォルト遷移のみ）。常駐が位相同期の前提であるため、これはスタイルではなく**破ってはならない規約**。
- **BlendTreeの子クリップは同一拍数（または整数倍）で制作する**（1.2の制約）。
- キャラクターごとに別Controllerアセットを作成できる（プレイヤー用・敵種別用）。敵の歩きバリエーション追加はControllerとクリップの差し替えのみで完結する。
- Controllerアセットは`AnimatorControllerPlayable`の生成にのみ使い、**`Animator`コンポーネントの`runtimeAnimatorController`には絶対にアサインしない**（アサインするとグラフと二重評価になる）。

### 3.3 音楽同期の実現方法

| 同期 | 実現方法 |
| --- | --- |
| 速度同期（BPM） | 既存の`SetAnimationSpeed`の適用対象に`AnimatorControllerPlayable`を含める（`SetSpeed`はControllerPlayableにも有効で、内部のBlendTreeごとBPM追従する）。BPM変化時も従来どおり毎フレーム反映 |
| 位相同期（常駐中） | ステートから出ないことで自動成立（1.2の方法B） |
| 位相同期（初期化時） | **初期化時に一度だけ**`AnimatorControllerPlayable.Play(ステートhash, 0, normalizedTime)`で頭出しする。`normalizedTime = (MusicSyncState.PlayTime × 再生速度) mod クリップ長 ÷ クリップ長`（基準拍換算はワンショット側の`CharacterAnimationOneShotTimingCalculator`と整合させる）。これにより**曲の途中でスポーンする敵の位相ズレ（2.2）も解消**される |

- `AnimatorControllerPlayable`は`IAnimatorControllerPlayable`として`Play(stateNameHash, layer, normalizedTime)`・`SetFloat`・`GetCurrentAnimatorStateInfo`を持つため、`Animator`コンポーネントを介さずに完結する。

### 3.4 コード側の変更

- `PlayableAnimationController`
  - コンストラクタに`RuntimeAnimatorController`（ロコモーション用）を受け取る口を追加し、入力[0]を`AnimatorControllerPlayable`として接続する。初期化時の位相頭出し（3.3）もここで行う。
  - `SetAnimationSpeed`の対象にControllerPlayableを追加。
  - `SetLocomotionSpeedParameter(float value)`（仮称）を追加し、`SetFloat("Speed", value)`を委譲する。
- `CharacterAnimationView`
  - `_locomotionCalculator.ApplyBaseWeights`によるIdle/Walkウェイト計算を廃止し、代わりに速度の大きさを`SetLocomotionSpeedParameter`へ渡す。入力[0]のベースウェイトは常に1（オーバーレイ減衰のみ適用）。
  - パラメータの段差対策（2.2）として、必要なら渡す値に指数減衰を掛ける（`SetFloat`のダンプ付きオーバーロードはControllerPlayableに無いためコード側で行う）。
  - 課題2の移動キャンセル判定はViewModelの速度を直接見る実装（関連文書3.2）なので**影響なし**。歩行しきい値定数はView側に残す。
- `CharacterAnimationLocomotionCalculator`
  - `ApplyBaseWeights`は廃止。`AnimationSpeed`（BPM→速度）は利用箇所が残るため、速度計算のみのクラスに縮小するか`View`へ吸収する。
- `AnimationComposition`
  - クリップ配列からIdle/Walkを除き、入力[0]をController、[1]以降をワンショットとする新しいインデックス割当を構築する。`CharacterAnimationClipType`のIdle/Walkはロコモーション側へ移ったことをenumコメント等で明示する（インデックス再割当は関連文書の分離リファクタリング後に行うため影響範囲はComposition内に収まる）。
  - Controllerアセットは`CharacterAnimationCatalogConfig`（または新設のロコモーション設定）でキャラクターごとに指定する。

### 3.5 検証支援（規約をツールで守る）

規約（3.2）は人が破りうるため、エディタバリデーションを用意する。

- Controllerアセットの検証: レイヤー数=1、ステート数=1、遷移なし、BlendTree子クリップの尺が基準拍数の整数倍で揃っているか。
- 実装形態はエディタ拡張（インポート時チェックまたはメニュー実行のLint）とし、違反時はエラーログでアセット名と理由を出す。

---

## 4. 実施順序と関連計画との関係

1. **先行**: `攻撃アニメーション遷移改善計画書.md`の全工程（プレイヤー/敵の分離＋課題1＋課題2）を完了させる。
   - 理由: 分離リファクタリングでComposition/Signalの責務が整理され、本計画のインデックス再割当の影響範囲が小さくなる。またワンショット側の挙動が確定していれば、本計画のリグレッション判定が「ロコモーションだけ変わったか」に絞れる。
2. **本計画 フェーズ1（プレイヤーのみ移行）**
   1. Controllerアセット作成（プレイヤー用Idle/Walk BlendTree）＋バリデーション実装
   2. `PlayableAnimationController`・`CharacterAnimationView`・`AnimationComposition`の対応（3.4）
   3. 受け入れ確認（5章）
3. **本計画 フェーズ2（敵へ展開）**
   - 敵用Controllerアセットを作成し、`EnemyLifeCycle`のComposition呼び出しへ差し替え。**途中スポーンの位相頭出し（3.3）が敵で正しく効くことをここで確認する。**

---

## 5. 受け入れ基準

| ケース | 期待挙動 |
| --- | --- |
| クリップ2秒・BPM60で、曲の2.3秒目に移動入力 | 歩きモーションが0.3秒目の位相で表示される（仕様例そのまま） |
| 停止↔移動を何度繰り返しても | 足が拍から外れない（位相が累積ズレしない） |
| BPMが曲中で変化 | 再生速度が追従し、拍単位の位相が維持される |
| 曲の途中で敵がスポーン | スポーン直後から敵の歩きが拍に合っている（現状の既知ギャップ2.2の解消確認） |
| 攻撃・回避・死亡等のワンショット | 従来どおり（課題1・課題2の挙動含め変化なし） |
| オーバーレイ中→終了 | ロコモーションへの復帰ブレンドが従来と同等の見た目 |
| バリデーション | 遷移を追加した/尺の違うクリップを入れたControllerがエラー検出される |

---

## 6. リスク・留意点

- **規約依存の設計である**: 「遷移を作らない」「拍数を揃える」はアセット制作側の規律に依存する。3.5のバリデーションを実装工程に必ず含める（後回しにすると規約が形骸化する）。
- **BlendTree拡張時の拍数制約**: 将来Run・方向ブレンド等を追加する際も同一拍数制約が掛かる。アニメーション発注時の仕様書に「基準拍数」を明記する運用が必要。
- **`AnimatorControllerPlayable`のウェイト0時の挙動**: Mixerのウェイトが0でもControllerPlayable内部の時間は進む（＝位相は保たれる）想定だが、実装時に長時間ウェイト0後の復帰で位相が合っていることを実機確認する。
- **パラメータ減衰の入れすぎ注意**: 段差対策の指数減衰を強くすると、移動開始時に歩きの立ち上がりが遅れて音楽同期の気持ちよさを損なう。減衰係数はSerializeFieldで調整可能にする。
- **`CharacterAnimationClipType`の意味変化**: Idle/WalkがMixer入力から消えるため、enumを参照している箇所の再グレップを実装時に必ず行う（現時点の把握では`AnimationComposition`と`CharacterAnimationLocomotionCalculator`のみだが要再確認）。
- **研究用実装との乖離**: `Assets/DevelopProducts/Research/AnimationControl/`の別系統Playable実装は本計画の対象外。混乱防止のため、本計画完了時にREADME等で「本番はこちら」と明示するか、不要なら削除を検討する。
