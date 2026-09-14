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
        /// <summary> ジャストタイミング位置を囲む枠線の色。 </summary>
        public Color JustOutlineColor => _justOutlineColor;

        /// <summary> ジャストタイミング位置を囲む枠線の太さ。 </summary>
        public float JustOutlineThickness => _justOutlineThickness;

        /// <summary> ジャストタイミング位置を囲む枠線が、ブロックの上へはみ出す量。 </summary>
        public float JustOutlineUpperExtend => _justOutlineUpperExtend;

        /// <summary> ジャストタイミング位置を囲む枠線が、ブロックの下へはみ出す量。 </summary>
        public float JustOutlineLowerExtend => _justOutlineLowerExtend;

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

        [Header("ジャスト位置の枠線")]
        [SerializeField, Tooltip("ジャストタイミング位置を囲む枠線の色。")]
        private Color _justOutlineColor = new Color(1f, 0.85f, 0.25f, 0.9f);

        [SerializeField, Min(0.1f), Tooltip("ジャストタイミング位置を囲む枠線の太さ。左右の枠線はブロックの内側へ描画するため、ブロック幅の半分を超えると枠線同士が重なります。")]
        private float _justOutlineThickness = 2f;

        [SerializeField, Min(0f), Tooltip("枠線がブロックの上へはみ出す量。ブロックは左右が隣と密着しているため、視認性は上下の張り出しで確保します。")]
        private float _justOutlineUpperExtend = 32f;

        [SerializeField, Min(0f), Tooltip("枠線がブロックの下へはみ出す量。ガイドの基準線より下を詰めたい場合は上より小さくします。")]
        private float _justOutlineLowerExtend = 32f;

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
