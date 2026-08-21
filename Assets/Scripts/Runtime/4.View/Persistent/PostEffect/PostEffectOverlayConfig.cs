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

        [SerializeField]
        [Tooltip("Volumeの影響から除外するレイヤーです。Baseカメラはこのレイヤーの描画を外し、Overlayカメラが上描きします。")]
        private LayerMask _excludedLayers;

        [SerializeField]
        [Tooltip("画面へ適用するVolumeProfileです。除外レイヤー以外に掛かります。")]
        private VolumeProfile _volumeProfile;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Volumeの強さです。")]
        private float _volumeWeight = DEFAULT_VOLUME_WEIGHT;

        private const float DEFAULT_VOLUME_WEIGHT = 1f;
    }
}
