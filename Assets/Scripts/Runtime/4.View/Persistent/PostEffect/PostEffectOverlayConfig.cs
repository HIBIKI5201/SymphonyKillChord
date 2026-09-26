using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;

namespace KillChord.Runtime.View.Persistent.PostEffect
{
    /// <summary>
    ///     ポストプロセスから除外するレイヤーと、画面へ適用するVolumeの対応を保持するConfig。
    ///     このConfig単位でOverlayカメラが1つ起動し、除外レイヤーはVolumeの影響を受けずに上描きされる。
    /// </summary>
    [CreateAssetMenu(
        fileName = "PostEffectOverlayConfig",
        menuName = "KillChord/View/PostEffect/Post Effect Overlay Config")]
    public sealed class PostEffectOverlayConfig : ScriptableObject
    {
        /// <summary> Volumeの影響から除外するレイヤーです。 </summary>
        public LayerMask ExcludedLayers => _excludedLayers;

        /// <summary> 画面へ適用するVolumeProfileです。 </summary>
        public VolumeProfile VolumeProfile => _volumeProfile;

        /// <summary> Volumeの強さです。 </summary>
        public float VolumeWeight => _volumeWeight;

        /// <summary> 開始時にVolumeの強さを上げ切るまでの秒数です。0なら即座に適用します。 </summary>
        public float FadeInSeconds => _fadeInSeconds;

        /// <summary> 取り下げ時にVolumeの強さを0まで下げるまでの秒数です。0なら即座に停止します。 </summary>
        public float FadeOutSeconds => _fadeOutSeconds;

        /// <summary> Volumeの強さを変化させるイージングです。 </summary>
        public Ease FadeEase => _fadeEase;

        [SerializeField]
        [Tooltip("Volumeの影響から除外するレイヤーです。Baseカメラはこのレイヤーの描画を外し、Overlayカメラが上描きします。")]
        private LayerMask _excludedLayers;

        [SerializeField]
        [Tooltip("画面へ適用するVolumeProfileです。除外レイヤー以外に掛かります。")]
        private VolumeProfile _volumeProfile;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Volumeの強さです。")]
        private float _volumeWeight = DEFAULT_VOLUME_WEIGHT;

        [SerializeField, Min(0f)]
        [Tooltip("開始時にVolumeの強さを上げ切るまでの秒数です。0なら即座に適用します。")]
        private float _fadeInSeconds = DEFAULT_FADE_IN_SECONDS;

        [SerializeField, Min(0f)]
        [Tooltip("取り下げ時にVolumeの強さを0まで下げるまでの秒数です。0なら即座に停止します。")]
        private float _fadeOutSeconds = DEFAULT_FADE_OUT_SECONDS;

        [SerializeField]
        [Tooltip("Volumeの強さを変化させるイージングです。")]
        private Ease _fadeEase = Ease.OutQuad;

        private const float DEFAULT_VOLUME_WEIGHT = 1f;
        private const float DEFAULT_FADE_IN_SECONDS = 0.1f;
        private const float DEFAULT_FADE_OUT_SECONDS = 0.2f;
    }
}
