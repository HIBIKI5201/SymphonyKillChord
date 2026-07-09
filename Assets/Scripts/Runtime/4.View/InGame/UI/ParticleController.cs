using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     パーティクルエフェクトの再生を担当するコンポーネント。
    /// </summary>
    public class ParticleController : MonoBehaviour
    {
        #region インスペクター表示フィールド
        /// <summary> 再生するヒットエフェクトのパーティクルシステム。 </summary>
        [SerializeField, Tooltip("再生するヒットエフェクトのパーティクルシステム。")]
        private ParticleSystem _hitEffect;

        [SerializeField, Tooltip("再生する予約時エフェクトのパーティクルシステム。")]
        private ParticleSystem _reserveEffect;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     指定された位置でパーティクルエフェクトを再生します。
        /// </summary>
        /// <param name="position">パーティクルエフェクトを再生するワールド座標。</param>
        public void PlayParticle(Vector3 position)
        {
            _hitEffect.transform.position = position;
            _hitEffect.Play();
        }

        /// <summary>
        ///     指定された位置で予約時パーティクルエフェクトを再生します。
        /// </summary>
        /// <param name="position">パーティクルエフェクトを再生するワールド座標。</param>
        public void PlayParticleReserve(Vector3 position)
        {
            _reserveEffect.transform.position = position;
            _reserveEffect.Play();
        }
        #endregion
    }
}
