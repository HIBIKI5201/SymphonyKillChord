# EnemyContainer

- id: 2b27c2c6-cc02-80fc-9da4-dd8005a499fc
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 敵 / EnemyContainer
- last_edited: 2025-11-22T21:18:18.692Z


### クラス概要
シーン内エネミー(EnemyManager)を一元管理する。
生存中のエネミーリストを保持、管理する。

### API

#### インデクサ
指定したインデックスの敵を返す。
インデックス範囲内を指定させるために
インデックスをループさせてる。

#### Register
enemiesに敵を追加する。
OnDeathイベントにenemies.Removeを登録。


## 内部実装

### フィールド変数

#### _enemies 
シーンに生存しているEnemyManagerを保持する
リスト。