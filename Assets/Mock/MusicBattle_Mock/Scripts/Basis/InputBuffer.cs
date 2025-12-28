using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.MusicBattle.Basis
{
    /// <summary>
    ///     入力バッファクラス。
    ///     PlayerInputからの入力を抽象化し、イベントとして提供します。
    /// </summary>
    [RequireComponent(typeof(PlayerInput)), DefaultExecutionOrder(-1000)]
    public class InputBuffer : MonoBehaviour
    {
        #region パブリックプロパティ
        /// <summary> ルックアクションのInputActionEntity。 </summary>
        public InputActionEntity<Vector2> LookAction => _lookActionEntity;
        /// <summary> 移動アクションのInputActionEntity。 </summary>
        public InputActionEntity<Vector2> MoveAction => _moveActionEntity;
        /// <summary> ロックオン選択アクションのInputActionEntity。 </summary>
        public InputActionEntity<float> LockOnSelectAction => _lockOnSelectActionEntity;
        /// <summary> 攻撃アクションのInputActionEntity。 </summary>
        public InputActionEntity<float> AttackAction => _attackActionEntity;
        #endregion
        // PUBLIC_METHODS
        #region インスペクター表示フィールド
        /// <summary> ルックアクションの名前。 </summary>
        [SerializeField, Tooltip("ルックアクションの名前。")]
        private string _lookActionName = "Look";
        /// <summary> 移動アクションの名前。 </summary>
        [SerializeField, Tooltip("移動アクションの名前。")]
        private string _moveActionName = "Move";
        /// <summary> ロックオン選択アクションの名前。 </summary>
        [SerializeField, Tooltip("ロックオン選択アクションの名前。")]
        private string _lockOnSelectActionName = "LockOnSelect";
        /// <summary> 攻撃アクションの名前。 </summary>
        [SerializeField, Tooltip("攻撃アクションの名前。")]
        private string _attackActionName = "Attack";
        #endregion

        #region プライベートフィールド
        /// <summary> ルックアクションのInputActionEntity。 </summary>
        private InputActionEntity<Vector2> _lookActionEntity;
        /// <summary> 移動アクションのInputActionEntity。 </summary>
        private InputActionEntity<Vector2> _moveActionEntity;
        /// <summary> ロックオン選択アクションのInputActionEntity。 </summary>
        private InputActionEntity<float> _lockOnSelectActionEntity;
        /// <summary> 攻撃アクションのInputActionEntity。 </summary>
        private InputActionEntity<float> _attackActionEntity;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     PlayerInputコンポーネントからアクションを取得し、InputActionEntityを初期化します。
        /// </summary>
        public void Awake()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                _lookActionEntity = new InputActionEntity<Vector2>(playerInput.actions[_lookActionName]);
                _moveActionEntity = new InputActionEntity<Vector2>(playerInput.actions[_moveActionName]);
                _lockOnSelectActionEntity = new InputActionEntity<float>(playerInput.actions[_lockOnSelectActionName]);
                _attackActionEntity = new InputActionEntity<float>(playerInput.actions[_attackActionName]);
            }
        }

        /// <summary>
        ///     オブジェクトが破棄されるときに呼び出されます。
        ///     各InputActionEntityのリソースを解放します。
        /// </summary>
        public void OnDestroy()
        {
            _lookActionEntity.Dispose();
            _moveActionEntity.Dispose();
            _lockOnSelectActionEntity.Dispose();
            _attackActionEntity.Dispose();
        }
        #endregion
    }
}
