# EnemyManager

- id: 2b07c2c6-cc02-806b-8d3a-d20427c0669c
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 敵 / EnemyManager
- last_edited: 2025-11-22T21:05:00.066Z


### クラス概要
敵キャラクターの主要な処理をするマネージャークラス。
HP管理
ターゲット設定
移動ロジックの初期化
死亡イベントの通知
その他エネミー個体に関する主要な処理をする。

### API

#### event Action OnDeath
healthが0になったときに発火。

#### Transform LockTarget
敵自身のTransform。
ロックオン時に使用。

#### Awake
RigidBodyやHealthEntityの初期化。
LockTargetを設定。

#### SetTarget(Transform target)
追従対象のTransformを設定。

#### InitializeMover()
EnemyMoverを初期化。
必要なデータがそろっていない場合は中断。

#### TakeDamage(float damage)
ダメージを与えHPを減らす。
0になったらOnDeath発火。

## 内部実装

エネミー個体の初期化処理
ターゲットの設定
ダメージ処理
物理更新を行う。

### フィールド変数

#### _enemyStatus
敵のステータス情報。

#### _target
追従対象。

#### _lockTarget
自身のTransform。

#### _rb
RigidBodyコンポーネント。

#### _healthEntity
HP管理用オブジェクト。

#### _enemyMover
移動を担当するオブジェクト。


