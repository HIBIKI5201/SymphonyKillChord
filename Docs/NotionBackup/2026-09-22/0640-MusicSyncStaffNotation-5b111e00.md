# MusicSyncStaffNotation

- id: 2b57c2c6-cc02-8065-8a9b-c80c5b111e00
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / UI / MusicSyncStaffNotation
- last_edited: 2025-11-30T07:15:26.743Z


## API

### メソッド

#### BindData
- 引数情報
  - `bpm`  int型

#### PopNote
- 引数情報
  - `rhythmScale`  int型

## 内部実装

### スタック
UI上には4小節までのスタックが表示される。
小節ごとの区切りに大きな線が、拍ごとの区切りに小さな線がある。

### ノーツの移動
4小節後に消滅するように、バインドされたbpmに則って移動速度が計算される。
