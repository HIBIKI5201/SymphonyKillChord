# CriMusicPlayer

- id: 2b17c2c6-cc02-8098-b12a-f12d3badfe9f
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 音楽同期システム / CriMusicPlayer
- last_edited: 2025-11-22T18:30:04.776Z


## 概要
BGMを再生するクラス。

## 内部実装

### メンバー変数
インスタンス_CriAtomSource（CRIプラグインのクラス）

### 処理

#### 初期化
- 入力項目：
　**BGM情報
　　**CriAtomSource
　　BPM
　　固有拍子
　　最初小節の開始時間
- 戻り値：
　なし
- 処理詳細：
　メンバー変数の値を設定する：
　　インスタンス_CriAtomSource：入力項目のCriAtomSource

#### BGM再生処理
- 入力項目：
　なし
- 戻り値：
　なし
- 処理詳細：
　インスタンス_CriAtomSourceの再生メソッドを呼び出す