# PlayerStatus

- id: 2b07c2c6-cc02-8097-ae26-cedcc8f997a5
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / プレイヤー / PlayerStatus
- last_edited: 2026-01-20T02:02:01.834Z


## クラス概要
プレイヤーのステータスをScriptbleObjectで設定する。

## API

### プロパティ

#### MoveSpeed
- float型
プレイヤーのスピード。

#### Walk Acceleration Duration
- float型
プレイヤーの移動の速度が最大になるまでに掛かる時間。

#### RotationDamping
- float型
プレイヤーの回転の減衰率。 
数値が上がるほど回転に時間を要するようになる。

#### AttackPower
- float型
プレイヤーの攻撃力。

#### AttackRange
- float型
プレイヤーの攻撃が届く範囲。

#### MaxHealth
- float型
プレイヤーの最大体力。

## 内部実装
シリアライズprivateで定義したものをプロパティで公開している。