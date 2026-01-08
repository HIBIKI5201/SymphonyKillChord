# ToDo 3: 移動・攻撃ロジックの理解

## 概要
このタスクは、特定された `PlayerMover.cs` と `PlayerAttacker.cs` の内容を読み込み、現在の移動および攻撃のロジックを詳細に理解することを目的とします。

## ステータス
**完了済み**

## 実施内容
`PlayerMover.cs` が `SetPlayerVelocity` で目標速度を設定し、`Update` および `FixedUpdate` で移動を処理していることを確認しました。
`PlayerAttacker.cs` が `Attack` メソッドで攻撃処理を実行しているものの、攻撃中を示す状態管理がクラス内に存在しないことを理解しました。
