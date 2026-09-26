# MusicSyncManager

- id: 2b17c2c6-cc02-800a-8f09-febafa31827f
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 音楽同期システム / MusicSyncManager
- last_edited: 2025-11-30T10:33:18.685Z


## 概要
音楽同期システムの全体を管理し、外部（他システムなど）へのインターフェースを持つクラス。

## 内部実装

### メンバー変数
拍子スケールリスト
インスタンス_CriMusicBuffer
インスタンス_CriMusicPlayer
インスタンス_MusicInputHandler
インスタンス_MusicActionHandler

### 処理仕様

#### 初期化
- 入力項目：
**BGM情報**
　BPM
　固有拍子
　最初小節の開始時間
- 戻り値：
なし
- 処理詳細：
BGM情報を各インスタンスを渡して、初期化を行う。
BGM再生を始める。

#### 入力受付
- 入力項目：
なし
- 戻り値：
入力によって成り立つ拍子
- 処理詳細
インスタンスMusicInputHandlerの入力時処理を呼び出し、その戻り値を返却する

#### アクション予約受付
- 入力項目：
小節タイミング構造体
　小節フラグ
　拍子スケール
　拍数
予約アクション
- 戻り値：
なし　
- 処理詳細
インスタンスMusicActionHandlerのアクション予約処理を呼び出す
