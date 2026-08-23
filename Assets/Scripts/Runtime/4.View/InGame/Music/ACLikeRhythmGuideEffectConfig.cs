using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     AC風リズムガイドのジャストタイミング演出設定。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(ACLikeRhythmGuideEffectConfig),
        menuName = "KillChord/InGame/Music/ACLikeRhythmGuideEffectConfig")]
    public sealed class ACLikeRhythmGuideEffectConfig : ScriptableObject
    {
        /// <summary> ジャストタイミング位置を示す帯の色。 </summary>
        public Color MarkerColor => _markerColor;

        /// <summary> ジャストタイミング位置を示す帯の幅。 </summary>
        public float MarkerWidth => _markerWidth;

        /// <summary> ジャストタイミング位置を示す帯の高さ。 </summary>
        public float MarkerHeight => _markerHeight;

        /// <summary> ジャストタイミング位置を示す帯の垂直方向の位置補正。 </summary>
        public float MarkerVerticalOffset => _markerVerticalOffset;

        /// <summary> ジャストタイミング成立時のフラッシュ色。 </summary>
        public Color FlashColor => _flashColor;

        /// <summary> ジャストタイミング成立時のフラッシュ時間。 </summary>
        public float FlashDuration => _flashDuration;

        /// <summary> ジャストタイミング成立時のフラッシュイージング。 </summary>
        public Ease FlashEase => _flashEase;

        /// <summary> 通常タイミングの縮小イージング。 </summary>
        public Ease NormalTimingEase => _normalTimingEase;

        /// <summary> ジャストタイミング成立時に追加で伸びる高さ。 </summary>
        public float JustOvershootAmount => _justOvershootAmount;

        /// <summary> ジャストタイミング成立時に伸びる時間。 </summary>
        public float JustOvershootDuration => _justOvershootDuration;

        /// <summary> ジャストタイミング成立後に縮む時間。 </summary>
        public float JustReturnDuration => _justReturnDuration;

        /// <summary> ジャストタイミング成立時に伸びるイージング。 </summary>
        public Ease JustOvershootEase => _justOvershootEase;

        /// <summary> ジャストタイミング成立後に縮むイージング。 </summary>
        public Ease JustReturnEase => _justReturnEase;

        /// <summary> ジャストタイミング成立時にVignetteを再生するか。 </summary>
        public bool IsVignetteEnabled => _isVignetteEnabled;

        /// <summary> ジャストタイミング成立時のVignette強度。 </summary>
        public float VignetteIntensity => _vignetteIntensity;

        /// <summary> ジャストタイミング以外の入力時のVignette強度。 </summary>
        public float NormalVignetteIntensity => _normalVignetteIntensity;

        /// <summary> ジャストタイミング成立時のVignette時間。 </summary>
        public float VignetteDuration => _vignetteDuration;

        /// <summary> ジャストタイミング以外の入力時のVignette時間。 </summary>
        public float NormalVignetteDuration => _normalVignetteDuration;

        /// <summary> ジャストタイミング成立時のVignetteイージング。 </summary>
        public Ease VignetteEase => _vignetteEase;

        [Header("判定ウィンドウ")]
        [SerializeField, Tooltip("ジャストタイミング位置を示す帯の色。")]
        private Color _markerColor = new Color(1f, 0.85f, 0.25f, 0.45f);

        [SerializeField, Min(0.1f), Tooltip("ジャストタイミング位置を示す帯の幅。")]
        private float _markerWidth = 3f;

        [SerializeField, Min(0.1f), Tooltip("ジャストタイミング位置を示す帯の高さ。")]
        private float _markerHeight = 192f;

        [SerializeField, Tooltip("ジャストタイミング位置を示す帯の垂直方向の位置補正。帯の下端をガイドの基準線へ合わせるために使用します。")]
        private float _markerVerticalOffset = -20f;

        [Header("判定色")]
        [SerializeField, Tooltip("ジャストタイミング成立時にビートを一瞬変更する色。")]
        private Color _flashColor = new Color(1f, 0.9f, 0.45f, 1f);

        [SerializeField, Min(0.01f), Tooltip("ジャストタイミング成立時のフラッシュ時間（秒）。")]
        private float _flashDuration = 0.2f;

        [SerializeField, Tooltip("ジャストタイミング成立時のフラッシュイージング。")]
        private Ease _flashEase = Ease.OutQuad;

        [Header("モーション")]
        [SerializeField, Tooltip("通常タイミングの縮小イージング。")]
        private Ease _normalTimingEase = Ease.OutCirc;

        [SerializeField, Min(0f), Tooltip("ジャストタイミング成立時に追加で伸びる高さ。")]
        private float _justOvershootAmount = 64f;

        [SerializeField, Min(0.01f), Tooltip("ジャストタイミング成立時に伸びる時間（秒）。")]
        private float _justOvershootDuration = 0.12f;

        [SerializeField, Min(0.01f), Tooltip("ジャストタイミング成立後に縮む時間（秒）。")]
        private float _justReturnDuration = 0.45f;

        [SerializeField, Tooltip("ジャストタイミング成立時に伸びるイージング。")]
        private Ease _justOvershootEase = Ease.OutBack;

        [SerializeField, Tooltip("ジャストタイミング成立後に縮むイージング。")]
        private Ease _justReturnEase = Ease.OutCirc;

        [Header("全画面Vignette")]
        [SerializeField, Tooltip("ジャストタイミング成立時にビート色連動Vignetteを再生するか。")]
        private bool _isVignetteEnabled = true;

        [SerializeField, Range(0f, 1f), Tooltip("ジャストタイミング成立時のVignette強度。")]
        private float _vignetteIntensity = 0.22f;

        [SerializeField, Range(0f, 1f), Tooltip("ジャストタイミング以外の入力時のVignette強度。")]
        private float _normalVignetteIntensity = 0.1f;

        [SerializeField, Min(0.01f), Tooltip("ジャストタイミング成立時のVignette時間（秒）。")]
        private float _vignetteDuration = 0.28f;

        [SerializeField, Min(0.01f), Tooltip("ジャストタイミング以外の入力時のVignette時間（秒）。")]
        private float _normalVignetteDuration = 0.14f;

        [SerializeField, Tooltip("ジャストタイミング成立時のVignetteイージング。")]
        private Ease _vignetteEase = Ease.OutQuad;
    }
}
