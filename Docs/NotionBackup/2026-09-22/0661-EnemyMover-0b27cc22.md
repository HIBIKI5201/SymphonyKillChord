# EnemyMover

- id: 2b07c2c6-cc02-8068-931d-d5fa0b27cc22
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 敵 / EnemyMover
- last_edited: 2025-11-22T21:17:00.950Z


### クラス概要
移動と距離判定を行う。

## API

### EnemyMover(Transform,Transform,EnemyStatus,RigidBody)
コンストラクタ。
エネミーマネージャーに必要なものを初期化する。

### Init(EnemyStatus)
エネミーステータスをフィールドとして保存する。

### float DistanceToTarget()
現在の距離を返す。

### MoveTo
ターゲットとエネミーの距離が射程距離内になるまで近づく。

## 内部実装

### フィールド変数

#### _enemyStatus
移動速度や攻撃射程などの敵ステータス。

#### _rigidBody
RigidBodyコンポーネント。

### _target 
追従対象のTransform。

#### _enemy
敵自身のTransform。

