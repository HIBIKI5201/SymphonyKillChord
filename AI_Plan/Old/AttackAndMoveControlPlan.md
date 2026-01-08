# 攻撃中移動不可と移動中に攻撃した場合は停止機能の実装計画

## 1. 概要

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

`Attack` メソッドの開始時に `_isAttacking` を `true` に設定します。また、移動停止のため `PlayerMover` の参照が必要になります。

```csharp
namespace Mock.MusicBattle.Player
{
    public class PlayerAttacker
    {
        // ... 既存のコード ...

        // コンストラクタにPlayerMoverを追加
        public PlayerAttacker(PlayerStatus status, PlayerConfig config, PlayerManager player, MusicSyncManager musicSyncManager, PlayerMover playerMover)
        {
            _status = status;
            _config = config;
            _player = player;
            _musicSyncManager = musicSyncManager;
            _playerMover = playerMover; // 追加
        }

        /// <summary>
        ///     指定されたターゲットに攻撃を行います。
        /// </summary>
        /// <param name="target">攻撃対象。</param>
        /// <param name="signature">攻撃の威力を決定する拍子。</param>
        public void Attack(ICharacter target, float signature)
        {
            _isAttacking = true; // 攻撃開始
            _playerMover.SetPlayerVelocity(Vector3.zero); // 移動を停止

            // ... 既存のAttackメソッドの処理 ...
        }

        // ... 既存のコード ...

        #region プライベートフィールド
        // ... 既存のプライベートフィールド ...
        /// <summary> プレイヤーの移動クラス。 </summary>
        private readonly PlayerMover _playerMover; // 追加
        #endregion

        // ... 既存のコード ...
    }
}
```

#### 3.1.4. 攻撃終了処理の検討

現在の `Attack` メソッドは同期的に処理が完了します。攻撃アニメーションの終了などを待つ場合は、別途アニメーションイベントなどを利用して `_isAttacking` を `false` に戻すメカニズムが必要です。今回は簡略化のため、攻撃が完了したとみなせるポイントで `_isAttacking = false;` を追加することを想定しますが、これは実際の攻撃アニメーションの再生時間などによって調整が必要になります。

**注意**: 今回の計画では、`Attack` メソッドが呼ばれたら即座に移動を停止させますが、`_isAttacking` が `false` になるタイミングについては、攻撃アニメーションの再生などの実装状況に応じて、別途詳細な設計が必要になります。現時点では、`Attack` メソッド内で攻撃処理が完了したと仮定し、暫定的にメソッドの終わりに`_isAttacking = false;`を記述するか、または外部から`EndAttack()`のようなメソッドを呼び出す想定にします。

**暫定対応（Attackメソッドの末尾で攻撃状態を解除）**:

```csharp
        public void Attack(ICharacter target, float signature)
        {
            _isAttacking = true; // 攻撃開始
            _playerMover.SetPlayerVelocity(Vector3.zero); // 移動を停止

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
            _isAttacking = false; // 攻撃終了 (暫定)
        }
```
**代替案（EndAttackメソッドの追加）**:

後々アニメーションイベントなどから呼び出すことを想定し、`EndAttack` メソッドを追加することも検討できます。

```csharp
        #region Publicメソッド
        // ... 既存のメソッド ...

        /// <summary>
        ///     攻撃状態を終了します。
        /// </summary>
        public void EndAttack()
        {
            _isAttacking = false;
        }
        #endregion
```

今回は、実装の簡潔さを優先し、`Attack` メソッドの終了時に `_isAttacking = false;` を設定する方針で進めます。より正確な攻撃終了タイミングの制御が必要になった場合は、`EndAttack()` メソッドの導入を検討します。

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

        #region プライベートフィールド
        // ... 既存のプライベートフィールド ...
        /// <summary> プレイヤーの攻撃クラス。 </summary>
        private readonly PlayerAttacker _playerAttacker; // 追加
        #endregion

        // ... 既存のコード ...
    }
}
```

#### 3.2.2. Update メソッドの修正（攻撃中の移動制限）

`Update` メソッド内で、`_playerAttacker.IsAttacking` が `true` の場合、移動処理をスキップするように変更します。

```csharp
namespace Mock.MusicBattle.Player
{
    public class PlayerMover
    {
        // ... 既存のコード ...

        #region Publicメソッド
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
    }
}
```

### 3.3. PlayerManager.cs または関連スクリプトの修正

`PlayerMover` および `PlayerAttacker` のコンストラクタに変更を加えたため、これらのクラスをインスタンス化している箇所（おそらく `PlayerManager.cs`）も修正する必要があります。

具体的には、`PlayerAttacker` のインスタンスを生成する際に `PlayerMover` のインスタンスを渡し、`PlayerMover` のインスタンスを生成する際に `PlayerAttacker` のインスタンスを渡すように変更します。これは循環参照になるため、インスタンス生成の順序や、インターフェースを導入するなどの設計変更が必要になる可能性があります。

**簡略化されたアプローチ**:
今回は、`PlayerManager` で `PlayerMover` と `PlayerAttacker` を生成する際に、お互いのインスタンスを渡すように修正します。

```csharp
// PlayerManager.cs (仮)

using UnityEngine;
// ... その他のusing ...

namespace Mock.MusicBattle.Player
{
    public class PlayerManager : MonoBehaviour
    {
        // ... 既存のフィールド ...
        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;
        // ...

        private void Awake()
        {
            // ... 既存の初期化処理 ...

            // インスタンスを先に生成（ここでは仮にnullを渡すか、後で設定）
            _playerMover = new PlayerMover(/* ... */, null); // 暫定的にnullを渡す
            _playerAttacker = new PlayerAttacker(/* ... */, null); // 暫定的にnullを渡す

            // 循環参照を解決するため、後からセットする
            // PlayerMoverのコンストラクタを調整するか、SetAttackerメソッドなどを追加
            // PlayerAttackerのコンストラクタを調整するか、SetMoverメソッドなどを追加

            // 以下の例は、コンストラクタで直接渡す場合を想定しています。
            // 実際のPlayerManagerのコード構造に合わせて調整してください。
            _playerMover = new PlayerMover(playerStatus, _rb, transform, _cameraTransform, _playerAttacker);
            _playerAttacker = new PlayerAttacker(playerStatus, playerConfig, this, musicSyncManager, _playerMover);

            // もしコンストラクタで渡せない場合は、初期化メソッドを呼ぶ
            // _playerMover.Initialize(_playerAttacker);
            // _playerAttacker.Initialize(_playerMover);

            // この部分の具体的な修正はPlayerManager.csの内容に依存します。
            // 循環参照を避けるための設計パターン（例：インターフェースによる依存性逆転）も検討の余地あり。
        }

        // ...
    }
}
```
**PlayerManagerの修正に関する詳細**:

`PlayerManager.cs` の具体的な修正は、`PlayerManager` がどのように `PlayerMover` と `PlayerAttacker` をインスタンス化しているかによって異なります。

もし `PlayerManager` が `Awake()` や `Start()` メソッド内で `new` キーワードでこれらを生成している場合、以下のように修正できます。

```csharp
// PlayerManager.cs (仮の修正例 - PlayerAttackerを先に生成し、その参照をPlayerMoverに渡す)

using UnityEngine;
// ...その他のusing ...

namespace Mock.MusicBattle.Player
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private PlayerStatus _playerStatus; // PlayerStatusの参照
        [SerializeField] private PlayerConfig _playerConfig; // PlayerConfigの参照
        [SerializeField] private MusicSyncManager _musicSyncManager; // MusicSyncManagerの参照

        private Rigidbody _rb; // Rigidbodyの参照
        private Transform _cameraTransform; // カメラのTransformの参照

        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>(); // Rigidbodyを取得
            _cameraTransform = Camera.main.transform; // メインカメラのTransformを取得 (仮)

            // PlayerAttackerを先にインスタンス化し、PlayerMoverの参照は後で設定する
            _playerAttacker = new PlayerAttacker(_playerStatus, _playerConfig, this, _musicSyncManager, null); // PlayerMoverは暫定的にnull

            // PlayerMoverをインスタンス化
            _playerMover = new PlayerMover(_playerStatus, _rb, transform, _cameraTransform, _playerAttacker);

            // PlayerAttackerにPlayerMoverの参照を設定 (SetMoverメソッドをPlayerAttackerに追加する必要がある)
            _playerAttacker.SetMover(_playerMover);

            // もしくは、PlayerAttackerのコンストラクタをPlayerMoverの後に呼び出すように調整するなど
        }

        // PlayerAttackerに以下のメソッドを追加
        // public void SetMover(PlayerMover mover) { _playerMover = mover; }

        // ...
    }
}
```

この計画では、循環参照を解決するために `PlayerAttacker` に `SetMover` メソッドを追加するアプローチを取るか、インスタンス生成の順序を工夫する必要があります。

今回は、簡潔にするため、`PlayerAttacker`のコンストラクタには`PlayerMover`を渡さず、`PlayerAttacker`の`Attack`メソッド内で`PlayerManager`経由で`PlayerMover`にアクセスする形、または`PlayerAttacker`に`SetPlayerMover`のようなセッターメソッドを追加して後から設定する形がより現実的かもしれません。

**最終的なPlayerAttacker, PlayerMoverのコンストラクタ変更案:**

*   `PlayerAttacker`のコンストラクタには`PlayerMover`を渡さない。
*   `PlayerMover`のコンストラクタには`PlayerAttacker`を渡す。

これで`PlayerMover`から`PlayerAttacker.IsAttacking`を参照できます。
そして「移動中に攻撃した場合は停止」については、`PlayerManager`が攻撃入力を検知し、`PlayerAttacker.Attack`を呼び出す直前に`PlayerMover.SetPlayerVelocity(Vector3.zero)`を呼ぶ、または`PlayerAttacker.Attack`メソッド内で`PlayerManager`を経由して`PlayerMover`にアクセスするようにします。

より現実的なのは、`PlayerManager`が `PlayerMover` と `PlayerAttacker` の両方を管理し、必要に応じてそれぞれのメソッドを呼び出す形です。

**再考：PlayerManagerでの連携**

`PlayerManager`が全てのコンポーネントを管理しているため、`PlayerManager`で攻撃入力を受け取り、そこで`PlayerAttacker.Attack()`を呼び出す前に`PlayerMover.SetPlayerVelocity(Vector3.zero)`を呼び出すのが最も自然で、循環参照も発生しません。

この場合、`PlayerAttacker`には`PlayerMover`の参照は不要になります。

#### PlayerAttacker.cs の再修正案

`_playerMover` の参照は不要になります。
`Attack` メソッド内で `_isAttacking = true;` をセットし、攻撃終了時に `_isAttacking = false;` をセットします。

```csharp
namespace Mock.MusicBattle.Player
{
    public class PlayerAttacker
    {
        // ... 既存のコード ...

        // コンストラクタから PlayerMover を削除
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

            // ... 既存のAttackメソッドの処理 ...

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
        // private readonly PlayerMover _playerMover; // 削除
        #endregion
    }
}
```

#### PlayerMover.cs の再修正案

`PlayerAttacker` の参照はそのまま保持します。

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

#### PlayerManager.cs または関連スクリプトの修正案

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

この計画であれば、循環参照を避けることができ、責務もより明確になります。

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

---