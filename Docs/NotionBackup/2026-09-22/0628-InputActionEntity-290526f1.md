# InputActionEntity

- id: 2b07c2c6-cc02-80e6-a238-c9c6290526f1
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / 基盤システム / InputActionEntity
- last_edited: 2025-11-19T12:56:28.797Z


## クラス概要
イベントの登録を直感的にするために使用される。

## API

### イベント

#### Started
- Action<T>型
登録されているイベントハンドラーを管理するイベント。
引数に<T>型の値を受け取る。
‘add’でハンドラー登録、’remove’でハンドラー解除。
内部では’InputAction.started’にラップしたハンドラーを登録し、
CallbackContextから’T’型に変換して渡す。

#### Performed
- Action<T>型
登録されているイベントハンドラーを管理するイベント。
引数に<T>型の値を受け取る。
‘add’でハンドラー登録、’remove’でハンドラー解除。
内部では’InputAction.started’にラップしたハンドラーを登録し、
CallbackContextから’T’型に変換して渡す。

#### Canceled
- Action<T>型
登録されているイベントハンドラーを管理するイベント。
引数に<T>型の値を受け取る。
‘add’でハンドラー登録、’remove’でハンドラー解除。
内部では’InputAction.started’にラップしたハンドラーを登録し、
CallbackContextから’T’型に変換して渡す。

### メソッド

#### InvokeStarted
- 引数
・T型 value
 登録されている全てのStartedイベントハンドラーを手動で呼び出します。
デバッグや単体テストで、InputSystemを経由せずにイベントを発火させたいときに使用。

#### InvokePerformed
- 引数
・T型 value
 登録されている全てのPerformedイベントハンドラーを手動で呼び出します。
デバッグや単体テストで、InputSystemを経由せずにイベントを発火させたいときに使用。

#### InvokeCanceled
- 引数
・T型 value
 登録されている全てのCanceledイベントハンドラーを手動で呼び出します。
デバッグや単体テストで、InputSystemを経由せずにイベントを発火させたいときに使用。


