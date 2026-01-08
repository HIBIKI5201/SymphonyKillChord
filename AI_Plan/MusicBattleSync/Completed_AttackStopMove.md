# 機能計画: 移動中に攻撃した場合は停止

## 1. 概要
プレイヤーが移動中に攻撃アクションを開始した場合、その瞬間にキャラクターの移動を即座に停止させる。

## 2. 変更箇所と必要な工程

### 2.1. PlayerManager.cs (または攻撃入力を処理しているクラス)
1.  **攻撃入力処理の調整**: 攻撃入力を検出する箇所で、`PlayerAttacker.Attack()` メソッドを呼び出す前に、`PlayerMover.SetPlayerVelocity(Vector3.zero)` を呼び出して移動を停止させます。
    *   `PlayerManager` が `PlayerMover` のインスタンスを保持している必要があります。

### 2.2. PlayerMover.cs
1.  **SetPlayerVelocityメソッドの確認**: `SetPlayerVelocity(Vector3 velocity)` メソッドが、与えられた速度を即座に反映できることを確認します。現在の実装では `_targetVelocity` に設定され、`Update` メソッドで補間されているため、`Update` メソッドの先頭で `_targetVelocity = Vector3.zero; CurrentVelocity = Vector3.zero;` のように即座に速度を0にする処理が必要になる場合があります。

## 3. テスト項目
*   プレイヤーが様々な速度で移動中に攻撃ボタンを押した際に、即座に移動が停止することを確認。
*   攻撃アニメーションが正常に開始されることを確認。

## 4. 考慮事項
*   「攻撃中移動不可」機能と密接に関連しており、同時に実装または連携を考慮する必要があります。
*   移動停止がスムーズに行われるか、急停止による不自然さがないか確認が必要です。
