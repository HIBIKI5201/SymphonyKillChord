# 機能計画: 攻撃中移動不可

## 1. 概要
プレイヤーが攻撃アクションを行っている間、移動入力を受け付けず、キャラクターが移動しないようにする。

## 2. 変更箇所と必要な工程

### 2.1. PlayerAttacker.cs
1.  **攻撃中フラグの追加**: プレイヤーが攻撃中であることを示す `bool _isAttacking` フィールドと、その状態を外部から取得するための `public bool IsAttacking` プロパティを追加します。
2.  **フラグの状態変更**:
    *   `Attack()` メソッドの開始時に `_isAttacking = true;` を設定します。
    *   攻撃アニメーションの終了時、または特定の攻撃完了タイミングで `_isAttacking = false;` を設定するためのメカニズムを設けます（例: `EndAttack()` メソッドを追加し、アニメーションイベントから呼び出す、または攻撃時間を管理する）。

### 2.2. PlayerMover.cs
1.  **PlayerAttackerの参照追加**: コンストラクタで `PlayerAttacker` のインスタンスを受け取り、プライベートフィールドに保持します。
2.  **移動制限ロジックの追加**: `Update()` メソッド内で `_playerAttacker.IsAttacking` が `true` の場合、移動入力を無視し、目標速度 (`_targetVelocity`) および現在の速度 (`CurrentVelocity`) を `Vector3.zero` に設定します。

### 2.3. PlayerManager.cs (またはPlayerAttackerとPlayerMoverを初期化しているクラス)
1.  **インスタンス生成の調整**: `PlayerMover` のインスタンスを生成する際に、`PlayerAttacker` のインスタンスをコンストラクタに渡せるように初期化順序を調整します。
    *   `PlayerAttacker` を先に生成し、そのインスタンスを `PlayerMover` のコンストラクタに渡す形が考えられます。

## 3. テスト項目
*   攻撃中に移動入力を与えても、キャラクターが移動しないことを確認。
*   攻撃終了後、正常に移動できることを確認。
*   デバッグ表示などで `IsAttacking` フラグの状態と `PlayerMover.CurrentVelocity` の値を監視し、意図通りに変化していることを確認。

## 4. 考慮事項
*   攻撃終了の正確なタイミング (`_isAttacking = false;` にするタイミング) が重要です。アニメーションシステムとの連携が必須になります。
*   `PlayerManager` で `PlayerAttacker` と `PlayerMover` の依存関係を適切に解決する必要があります。
