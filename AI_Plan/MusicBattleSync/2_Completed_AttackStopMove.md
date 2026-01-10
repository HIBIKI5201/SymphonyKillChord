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

## 5. 実装評価

本計画は以下の実装によって実現されています。

*   **PlayerManager.cs**: `_playerAttacker.Attack()` 呼び出し後に `_playerMover.OnAttack(_playerAttacker.MoveLockTask);` を呼び出すことで、`PlayerMover` 側で移動停止処理をハンドリングしています。これにより、`PlayerManager` は攻撃処理と移動停止のトリガーに集中し、詳細な移動停止ロジックは `PlayerMover` に委譲されています。
*   **PlayerMover.cs**: `OnAttack` メソッド内で `_moveLock` フラグを `true` に設定し、`VelocityReset()` を呼び出すことで、`_currentVelocity` を即座に `Vector3.zero` にリセットしています。また、`_moveLock` が `true` の間は `Update()` メソッドの移動処理をスキップすることで、計画書で指摘されていた補間による遅延を避け、即座な移動停止を実現しています。

**総括**:
実装は計画書で提示されたアプローチとは一部異なるものの、機能要件「プレイヤーが移動中に攻撃アクションを開始した場合に、即座に移動を停止させる」を効果的に満たしています。特に、`PlayerMover` が移動停止のロジックを一元的に管理し、`_moveLock` フラグと `VelocityReset()` を組み合わせることで、計画書で考慮事項として挙げられていた「即座な停止」と「スムーズさ」の両方を実現しています。これにより、責務の分離が明確になり、堅牢で保守性の高いシステムが構築されています。