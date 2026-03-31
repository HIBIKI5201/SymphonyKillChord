using System;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class ParticleController : MonoBehaviour
    {
        #region パブリックプロパティ
        /// <summary> このクラスのシングルトンインスタンス。 </summary>
        public static ParticleController Instance { get; private set; }
        #endregion

        #region インスペクター表示フィールド
        /// <summary> 再生するヒットエフェクトのパーティクルシステム。 </summary>
        [SerializeField, Tooltip("再生するヒットエフェクトのパーティクルシステム。")]
        private ParticleSystem _hitEffect;

        [SerializeField, Tooltip("再生する予約時エフェクトのパーティクルシステム。")]
        private ParticleSystem _reserveEffect;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     シングルトンインスタンスを設定します。
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject); // 既にインスタンスが存在する場合は自身を破棄する。
            }
        }
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

        public void PlayParticleReserve(Vector3 position)
        {
            _reserveEffect.transform.position = position;
            _reserveEffect.Play();
        }
        #endregion
    }
}
