# EnemyStatus

- id: 2b07c2c6-cc02-8050-9c06-df31cb4b22d1
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 敵 / EnemyStatus
- last_edited: 2025-11-20T22:29:18.824Z


### クラス概要
エネミーの状態
エネミーのステータスをScriptableObjectで設定する。

## API

### プロパティ

#### MoveSpeed
- float型
エネミーのスピード。

#### AttackPower
- float型
エネミーーの攻撃力。

#### AttackRange
- float型
エネミーの攻撃が届く範囲。

#### MaxHealth
- float型
エネミーの最大体力。


## 内部実装
シリアライズprivateで定義したものをプロパティで公開している。
エネミーの状態を定義する。
