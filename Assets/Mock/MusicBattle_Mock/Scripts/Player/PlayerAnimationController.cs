using UnityEngine;

namespace Mock.MusicBattle.Player
{
    /// <summary>
    ///     プレイヤーのアニメーションを制御するクラス。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        #region Publicメソッド
        /// <summary>
        ///     速度値をアニメーターに設定します。
        /// </summary>
        /// <param name="value">設定する速度値。</param>
        public void MoveVelocity(float value)
        {
            _animator?.SetFloat(_moveVelocityHash, value);
        }
        #endregion

        #region インスペクター表示フィールド
        /// <summary> 移動速度をアニメーターに設定するためのパラメーター名。 </summary>
        [SerializeField, Tooltip("移動速度をアニメーターに設定するためのパラメーター名。")]
        private string _moveVelocity = "MoveVelocity";
        #endregion

        #region プライベートフィールド
        /// <summary> アニメーターコンポーネント。 </summary>
        private Animator _animator;
        /// <summary> 移動速度パラメーターのハッシュ値。 </summary>
        private int _moveVelocityHash;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     Animatorコンポーネントを取得します。
        /// </summary>
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        ///     インスペクターで値が変更されたときに呼び出されます。
        ///     移動速度パラメーターのハッシュ値を更新します。
        /// </summary>
        private void OnValidate()
        {
            _moveVelocityHash = Animator.StringToHash(_moveVelocity);
        }
        #endregion
    }
}
