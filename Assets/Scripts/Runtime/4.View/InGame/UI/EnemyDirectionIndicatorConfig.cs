using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     敵方向表示の表示数、位置、距離、フェード設定を保持する。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(EnemyDirectionIndicatorConfig),
        menuName = "KillChord/InGame/UI/Enemy Direction Indicator Config")]
    public sealed class EnemyDirectionIndicatorConfig : ScriptableObject
    {
        /// <summary> 最大表示数。 </summary>
        public int MaximumDisplayCount => _maximumDisplayCount;

        /// <summary> プレイヤー原点からの表示位置オフセット。 </summary>
        public Vector3 PositionOffset => _positionOffset;

        /// <summary> 表示対象とする最大距離。 </summary>
        public float MaximumDistance => _maximumDistance;

        /// <summary> フェードのイージング設定。 </summary>
        public Ease FadeEase => _fadeEase;

        /// <summary> 表示・非表示のフェード時間。 </summary>
        public float FadeDuration => _fadeDuration;

        /// <summary> 安全に生成できる最大表示数。 </summary>
        public const int MAXIMUM_DISPLAY_COUNT = 32;

        [SerializeField, Range(1, MAXIMUM_DISPLAY_COUNT), Tooltip("同時に表示する敵方向マーカーの最大数。")]
        private int _maximumDisplayCount = 3;

        [SerializeField, Tooltip("プレイヤー原点から見た敵方向表示のローカル位置。")]
        private Vector3 _positionOffset = new(0f, 1f, 0f);

        [SerializeField, Min(0f), Tooltip("敵方向表示の対象とする最大距離。")]
        private float _maximumDistance = 20f;

        [SerializeField, Tooltip("フェードのイージング設定。")]
        private Ease _fadeEase = Ease.OutQuad;

        [SerializeField, Min(0f), Tooltip("敵方向マーカーの表示・非表示に使用するフェード時間。")]
        private float _fadeDuration = 0.15f;
    }
}
