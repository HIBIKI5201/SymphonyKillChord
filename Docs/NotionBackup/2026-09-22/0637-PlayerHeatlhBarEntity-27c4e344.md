# PlayerHeatlhBarEntity

- id: 2b57c2c6-cc02-80fc-86e0-fc4f27c4e344
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / UI / PlayerHeatlhBarEntity
- last_edited: 2025-11-27T04:30:00.023Z


## API

### メソッド

#### BindData
- 引数情報
  - `healthEntity`  [[2b87c2c6-cc02-808a-9928-ff1fc3c43eed]] 型
    - バインドする体力エンティティ

## 内部実装
[[2b87c2c6-cc02-808a-9928-ff1fc3c43eed]] のイベントの
OnHealthChanged時に現在体力/最大体力にバーの長さを変更し
OnDeath時にDisposeされる。
