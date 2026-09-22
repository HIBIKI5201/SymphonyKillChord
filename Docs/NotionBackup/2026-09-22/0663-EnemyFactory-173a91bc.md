# EnemyFactory

- id: 2b37c2c6-cc02-805a-8fd2-d67a173a91bc
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 敵 / EnemyFactory
- last_edited: 2025-11-22T21:45:15.563Z


### クラス概要
エネミー生成を一元管理するファクトリ。


### API

#### Init(EnemyContainer,Transform,GameObject)
ファクトリーが利用するものを初期化する。

### Spawn(EnemyStatus,Vector3)
エネミーの生成、またはプールから再利用して返す。
エネミーを初期化して生成する。


## 内部実装

### フィールド変数

#### _enemyPrefab
生成するエネミーを入れる。

#### _enemyContainer
初期化時のデータを
フィールドで参照を保存する。

#### _target
初期化時のデータを
ターゲットの参照を保存する。