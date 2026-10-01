# InputBuffer

- id: 2b07c2c6-cc02-8046-bc9b-c5d2f5eefe1b
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 基盤システム / InputBuffer
- last_edited: 2025-11-19T13:47:49.894Z


## クラス概要
プレイヤーの入力を扱うためのクラス。
InputSystemのPlayerInputをラップして、
それぞれの入力アクションをInputActionEntity<T>として公開する。
ゲームの他のシステムが、直接InputActionを触らずに済むようにする。

## 仕様意図
プレイヤーの入力を一元管理するために制作する。
購読側のイベントを変えるだけでいいようにする。

#### イベント
すべての入力処理はInputSystemのアクションコールバックを起点にし、
その入力結果をPublicEventとして通知する。
ゲームロジック側はこれらのイベントを購読することで、
入力タイミングに応じて処理を実行する。
InputSystem ⇒ InputBuffer ⇒ 各イベントで発火する。

## API
UnityのInputSystemから入力を受け取る(PlayerInput)コンポーネントが必須。
InputEntityクラスはInputBufferが使う基盤クラス。

### プロパティ

#### LookAction
- InputActionEntity<Vector2>型
視点操作入力を通知する。

#### MoveAction
- InputActionEntity<Vector2>型
移動入力を通知する。

#### LockOnSelectAction
- InputActionEntity<float>型
ロックオンの対象の選択,切り替え入力を通知する。

#### AttackAction
- InputActionEntity<float>型
攻撃入力を通知する。

## 内部実装

#### 操作登録
PlayerInputからアクション名でInputActionを取得して、
各InputActionEntityを生成する。

### 入力
Unity Input Systemのイベント(performed/started/canceled)が発火したとき、
CallbackContextから値を読み取り、対応するInputActionEntity<T>に渡す。
