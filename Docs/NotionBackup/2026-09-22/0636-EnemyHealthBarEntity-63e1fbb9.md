# EnemyHealthBarEntity

- id: 2b57c2c6-cc02-8075-a054-f1ff63e1fbb9
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / UI / EnemyHealthBarEntity
- last_edited: 2025-11-28T17:30:42.238Z


## API

### メソッド

#### BindData
- 引数情報
  - `healthEntity`  [[HealthEntity]] 型
  - `transform`  Transform型

## 内部実装
[[2b87c2c6-cc02-808a-9928-ff1fc3c43eed]] のイベントの
OnHealthChanged時に現在体力/最大体力にバーの長さを変更し
OnDeath時にDisposeされる。