# ToDo 4: 攻撃中移動不可機能の実装計画

## 概要

本計画は、ゲーム内のプレイヤーキャラクターに対し、攻撃中は移動を禁止し、移動中に攻撃を開始した場合は即座に移動を停止させる機能（「攻撃中移動不可」および「移動中に攻撃した場合は停止」）を実装することを目的とします。

## 2. 既存コードの分析

### PlayerMover.cs

*   プレイヤーの移動ロジックを管理。
*   `SetPlayerVelocity(Vector3 velocity)`: 目標速度を設定。
*   `Update()` および `FixedUpdate()`: 設定された目標速度に基づいてプレイヤーを移動させる。
*   `CurrentVelocity` プロパティ: 現在の速度を取得。

### PlayerAttacker.cs

*   プレイヤーの攻撃ロジックを管理。
*   `Attack(ICharacter target, float signature)`: 攻撃処理を実行。
*   攻撃中かどうかを示す直接的な状態管理（フラグなど）は存在しない。

## 3. 実装計画

### 3.1. PlayerAttacker.cs の修正

#### 3.1.1. 攻撃状態管理フィールドの追加

`PlayerAttacker` クラスに、攻撃中かどうかを示すプライベートな `bool` 型フィールド `_isAttacking` を追加します。

```csharp
namespace Mock.MusicBattle.Player
{
    public class PlayerAttacker
    {
        // ... 既存のコード ...

        #region プライベートフィールド
        // ... 既存のプライベートフィールド ...
        /// <summary> プレイヤーが攻撃中かどうか。 </summary>
        private bool _isAttacking;
        #endregion

        // ... 既存のコード ...
    }
}
```

#### 3.1.2. 攻撃状態プロパティの追加

`_isAttacking` の状態を外部から参照できるパブリックなプロパティ `IsAttacking` を追加します。

```csharp
namespace Mock.MusicBattle.Player
{
    public class PlayerAttacker
    {
        // ... 既存のコード ...

        #region パブリックプロパティ
        /// <summary> プレイヤーが攻撃中かどうか。 </summary>
        public bool IsAttacking => _isAttacking;
        #endregion

        // ... 既存のコード ...
    }
}
```

#### 3.1.3. Attack メソッドの修正（攻撃開始時）

`Attack` メソッドの開始時に `_isAttacking` を `true` に設定し、攻撃処理の終了時に `_isAttacking` を `false` に設定します。

```csharp
namespace Mock.MusicBattle.Player
{
    public class PlayerAttacker
    {
        // ... 既存のコード ...

        // コンストラクタから PlayerMover を削除 (PlayerManager 経由で連携するため)
        public PlayerAttacker(PlayerStatus status, PlayerConfig config, PlayerManager player, MusicSyncManager musicSyncManager)
        {
            _status = status;
            _config = config;
            _player = player;
            _musicSyncManager = musicSyncManager;
        }

        // ... 既存のコード ...

        /// <summary>
        ///     指定されたターゲットに攻撃を行います。
        /// </summary>
        /// <param name="target">攻撃対象。</param>
        /// <param name="signature">攻撃の威力を決定する拍子。</param>
        public void Attack(ICharacter target, float signature)
        {
            _isAttacking = true; // 攻撃開始

            if (target == null) {
                _isAttacking = false; // 攻撃終了
                return;
            }

            Vector3 origin = _player.transform.position + Vector3.up * HEIGHT_RAY;

            #region デバッグ用
            _origin = origin;
            _direction = (target.Pivot - _origin).normalized;
            #endregion

            if (!CanAttackTarget(origin, target))
            {
                Debug.Log("Attack target not found or not reachable.");
                _isAttacking = false; // 攻撃終了
                return;
            }

            float attackPower = _status.AttackPower * 4 / signature;
            target.TakeDamage(attackPower);

            // MusicSyncのSignature履歴を取得し、特定のパターンと一致するかチェックする。
            for (int i = 0; i < _status.SpecialAttackPatterns.Length; i++)
            {
                RythemPatternData data = _status.SpecialAttackPatterns[i];
                if (_musicSyncManager.IsMatchInputTimeSignature(data))
                {
                    Debug.Log($"MusicSync Signature Pattern Matched! Pattern: {string.Join(", ", data.SignaturePattern.ToArray())}");
                }
            }
            // 攻撃終了処理（アニメーションイベントなどでの呼び出しを想定）
            // 現状では簡略化のためメソッドの最後に記述
            _isAttacking = false;
        }

        #region Publicメソッド
        // ... 既存のメソッド ...

        /// <summary>
        ///     攻撃状態を終了します。アニメーションイベントなどから呼び出すことを想定。
        /// </summary>
        public void EndAttack()
        {
            _isAttacking = false;
        }
        #endregion

        #region プライベートフィールド
        // ... 既存のプライベートフィールド ...
        #endregion
    }
}
```

### 3.2. PlayerMover.cs の修正

#### 3.2.1. PlayerAttacker の参照を追加

`PlayerMover` クラスのコンストラクタで `PlayerAttacker` のインスタンスを受け取り、プライベートフィールドに保持します。

```csharp
namespace Mock.MusicBattle.Player
{
    public class PlayerMover
    {
        // ... 既存のコード ...

        #region コンストラクタ
        public PlayerMover(PlayerStatus status, Rigidbody rb, Transform player, Transform camera, PlayerAttacker playerAttacker) // playerAttacker を追加
        {
            _status = status;
            _player = player;
            _camera = camera;
            _rb = rb;
            _playerAttacker = playerAttacker; // 追加
        }
        #endregion

        // ... 既存のコード ...

        #region Publicメソッド
        /// <summary>
        ///     プレイヤーの目標速度を設定します。
        /// </summary>
        /// <param name="velocity">設定する目標速度。</param>
        public void SetPlayerVelocity(Vector3 velocity)
        {
            _targetVelocity = velocity;
        }

        // ... 既存のコード ...

        /// <summary>
        ///     プレイヤーの移動を更新します（Updateフェーズで呼び出し）。
        /// </summary>
        public void Update()
        {
            if (_playerAttacker.IsAttacking) // 攻撃中は移動を停止
            {
                _targetVelocity = Vector3.zero; // 目標速度をリセット
                CurrentVelocity = Vector3.zero; // 現在の速度もリセット
                return;
            }

            float t = CalculateAccelerationLerpT();
            UpdateHorizontalVelocity(t);
            UpdateRotation();
        }
        #endregion

        // ... 既存のコード ...

        #region プライベートフィールド
        // ... 既存のプライベートフィールド ...
        /// <summary> プレイヤーの攻撃クラス。 </summary>
        private readonly PlayerAttacker _playerAttacker; // 追加
        #endregion

        // ... 既存のコード ...
    }
}
```

### 3.3. PlayerManager.cs または関連スクリプトの修正

`PlayerManager` が `PlayerMover` と `PlayerAttacker` をインスタンス化し、`PlayerMover` のコンストラクタに `PlayerAttacker` のインスタンスを渡します。攻撃入力が検出された際に、`PlayerMover` の移動を停止させ、その後 `PlayerAttacker.Attack()` を呼び出すようにします。

```csharp
// PlayerManager.cs (仮の修正例)

using UnityEngine;
// ...その他のusing ...

namespace Mock.MusicBattle.Player
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private MusicSyncManager _musicSyncManager;

        private Rigidbody _rb;
        private Transform _cameraTransform;

        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _cameraTransform = Camera.main.transform;

            _playerAttacker = new PlayerAttacker(_playerStatus, _playerConfig, this, _musicSyncManager);
            _playerMover = new PlayerMover(_playerStatus, _rb, transform, _cameraTransform, _playerAttacker);

            // PlayerAttackerが攻撃終了を外部に通知するためのイベントやデリゲートを設ける場合、ここで購読する
        }

        // 例：攻撃入力があった場合の処理
        public void HandleAttackInput(ICharacter target, float signature)
        {
            // 移動中に攻撃した場合、移動を停止させる
            _playerMover.SetPlayerVelocity(Vector3.zero);

            // 攻撃を開始する
            _playerAttacker.Attack(target, signature);

            // 攻撃アニメーションの終了を待ってから _playerAttacker.EndAttack() を呼び出す処理をここに含めるか、
            // アニメーションイベントなどで _playerAttacker.EndAttack() を呼び出す
        }

        // ...その他のメソッド (Updateなど) ...
    }
}
```

## 4. テスト計画

*   プレイヤーが移動中に攻撃ボタンを押した場合、即座に移動が停止し、攻撃アニメーションが開始することを確認。
*   プレイヤーが攻撃中に移動入力をしても、移動しないことを確認。
*   攻撃終了後、再び移動できることを確認。
*   `PlayerMover.CurrentVelocity` の値が、攻撃中は `Vector3.zero` になることを確認。

## 5. コード規約への準拠

上記の修正は、「Assets/Scripts/CodeGuidelines.md」に記載されている以下の規約に準拠します。

*   **サマリー・コメント**: 新規追加するフィールド、プロパティ、メソッドにはサマリーを付与します。コメントは日本語で「。」で終えます。
*   **命名規則**: フィールド変数はキャメルケースでプレフィックスに `_`、プロパティはパスカルケースなど、既存の命名規則に沿います。
*   **アクセス修飾子**: 必ず明示的に記述します。

## ステータス
**完了済み**
