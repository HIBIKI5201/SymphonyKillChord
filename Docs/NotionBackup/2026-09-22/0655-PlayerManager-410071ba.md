# PlayerManager

- id: 2b07c2c6-cc02-80ac-87ee-edf0410071ba
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / プレイヤー / PlayerManager
- last_edited: 2025-11-22T16:54:55.815Z


## クラス概要
プレイヤーキャラクターの主要な処理を一括して管理するプレイヤー中心の司令塔クラス。

## API

### イベント

#### OnHealthChanged
- 引数（float currentHelath,float maxHelath）
プレイヤーの体力が変化した際に発火。
UIのHPメーターを更新するためのイベント

### メソッド

#### Init
- 戻り値なし。引数（InputManager,CameraManager,HelthbarManager,HelthEntity）

#### TakeDamage
- 戻り値なし。引数(float)
引数を受け取り、HelthEntityのTakeDamageにそのまま引数を渡している。
ダメージ処理はHelthEntityで処理する。

## 内部実装

### メンバ変数
PlayerStatusとPlayerConfigの二つのScriptbleObjecctをシリアライズでセットする。
InputBuffer,PlayerMover,PlayerAttacker,HealthEntityの参照の変数を保持する。

#### _moveInput
InputBufferのMoveActionからの入力を一時保存するための変数。

#### _hitGrounds
衝突したcollisionを重複なく記録しておくコレクション。
地面のcollisionを入れている。

### Init
GetComponentやPlayerMover、PlayerAttack、HealthEntityなどをインスタンス化している。
また、イベント登録もしている。
Initはゲームマネージャーで呼ばれる。

### Ondisable
イベント登録解除している。
イベントを登録、解除している内容はMoveAction,JumpAction,AttackAction。

#### Update
velocityをplayermoverのメソッドから戻り値として受け取る。
playermoverのSetPlayerVelocityメソッドにvelocityを渡している。

### 地面判定
衝突面の法線ベクトルを取得して、地面との接触かどうか判定している
内積で地面かどうか判定。壁だったら0。天井は-1。判定を0.5f以上にすることで斜面にも対応。