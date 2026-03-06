# ToDo 5: コード修正の実装

## 概要
このタスクは、「攻撃中移動不可」と「移動中に攻撃した場合は停止」の機能を実現するため、`PlayerAttacker.cs` と `PlayerMover.cs`、および関連する `PlayerManager.cs` のコードを修正することを目的とします。

## ステータス
**保留 (ユーザーによる手動実装)**

## 計画詳細
「AI_Plan/04_PlanAttackMoveControl.md」に記載された計画に基づき、以下の修正を行います。

### PlayerAttacker.cs
- 攻撃中を示す `_isAttacking` フィールドと `IsAttacking` プロパティを追加。
- `Attack` メソッド内で `_isAttacking` を `true` に設定し、攻撃処理の終了時に `_isAttacking` を `false` に設定する。必要であれば `EndAttack` メソッドを追加し、アニメーションイベントなどから呼び出す。

### PlayerMover.cs
- コンストラクタで `PlayerAttacker` のインスタンスを受け取り、プライベートフィールドに保持。
- `Update` メソッド内で `_playerAttacker.IsAttacking` が `true` の場合、移動処理をスキップし、`_targetVelocity` および `CurrentVelocity` を `Vector3.zero` に設定する。

### PlayerManager.cs
- `PlayerMover` と `PlayerAttacker` のインスタンス化時に、`PlayerMover` のコンストラクタに `PlayerAttacker` のインスタンスを渡すように修正。
- 攻撃入力が検出された際、`PlayerAttacker.Attack()` を呼び出す前に `PlayerMover.SetPlayerVelocity(Vector3.zero)` を呼び出すことで、移動を停止させる。

## 考慮事項
- 攻撃終了のタイミングは、アニメーションイベントなど、実際のゲームの振る舞いに合わせて調整する必要があります。
- 既存のコード規約 (`Assets/Scripts/CodeGuidelines.md`) に厳密に準拠して修正を行います。
