# IngameHUDManager

- id: 2b57c2c6-cc02-804c-9c3f-c372bcf15145
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / UI / IngameHUDManager
- last_edited: 2025-11-27T04:32:01.446Z


## API

### メソッド

#### Init
- 引数情報
  - `playerHealthEntity`  [[HealthEntity]] 型
    - プレイヤーの体力エンティティを[[PlayerHeatlhBarEntity]] にバインドする。

#### PopupDamgeText
- 引数情報
  - `damage`  float型
  - `position`  Vector3型
    - ダメージを喰らった対象のワールド座標
ダメージのポップアップテキストを表示する。
[[2b87c2c6-cc02-8030-a120-da2774de7a4c]] インスタンスを使用する。

## 内部実装
[[2b87c2c6-cc02-80f6-9629-dbe00db72bce]] によるプールからダメージテキストを取得する。
