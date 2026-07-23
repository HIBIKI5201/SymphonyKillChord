using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージ開始時のシーケンスの設定情報を保持するクラス。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(StageStartSequenceConfig),
        menuName = "KillChord/StageStartSequenceConfig")]
    public class StageStartSequenceConfig : ScriptableObject
    {
        /// <summary> 黒画面を表示する時間。 </summary>
        public float BlackholdDuration => _blackholdDuration;

        /// <summary> 黒画面からフェードアウトする時間。 </summary>
        public float FadeOutDuration => _fadeOutDuration;

        /// <summary> 黒画面からフェードアウトする際のイージング。 </summary>
        public AnimationCurve FadeOutEasing => _fadeOutEasing;

        /// <summary> プレイヤーを基準にしたカメラの注視点オフセット。 </summary>
        public Vector3 FrontLookAtLocalOffset => _frontLookAtLocalOffset;

        /// <summary> 正面ショットのカメラ位置。 </summary>
        public float FrontDistance => _frontDistance;

        /// <summary> 正面ショットの水平角度。 </summary>
        public float FrontHorizontalAngle => _frontHorizontalAngle;

        /// <summary> 正面ショットの迎角。 </summary>
        public float FrontVerticalAngle => _frontVerticalAngle;

        /// <summary> 正面ショットのロール角度。 </summary>
        public float FrontRollAngle => _frontRollAngle;

        /// <summary> プレイヤーを基準にしたカメラの注視点オフセット。 </summary>
        public Vector3 RearLookAtLocalOffset => _rearLookAtLocalOffset;

        /// <summary> 背面ショットのカメラ位置。 </summary>
        public float RearDistance => _rearDistance;

        /// <summary> 背面ショットの水平角度。 </summary>
        public float RearHorizontalAngle => _rearHorizontalAngle;

        /// <summary> 背面ショットの迎角。 </summary>
        public float RearVerticalAngle => _rearVerticalAngle;

        /// <summary> 背面ショットのロール角度。 </summary>
        public float RearRollAngle => _rearRollAngle;

        /// <summary> 背面ショットの保持時間。 </summary>
        public float RearHoldDuration => _rearHoldDuration;

        /// <summary> 正面ショットから背面ショットへのオービット時間。 </summary>
        public float OrbitDuration => _orbitDuration;

        /// <summary> 正面ショットから背面ショットへのオービットのイージング。 </summary>
        public AnimationCurve OrbitEasing => _orbitEasing;

        /// <summary> 背面ショットからプレイヤー操作へのハンドオフ時間。 </summary>
        public float HandoffDuration => _handoffDuration;

        /// <summary> 背面ショットからプレイヤー操作へのハンドオフのイージング。 </summary>
        public AnimationCurve HandoffEasing => _handoffEasing;

        [Header("Fade")]
        [SerializeField, Tooltip("黒画面を表示する時間")]
        private float _blackholdDuration = 0.2f;

        [SerializeField, Tooltip("黒画面からフェードアウトする時間")]
        private float _fadeOutDuration = 0.4f;

        [SerializeField, Tooltip("黒画面からフェードアウトする際のイージング")]
        private AnimationCurve _fadeOutEasing;

        [Header("FrontShot")]
        [SerializeField, Tooltip("プレイヤーを基準にしたカメラの注視点オフセット")]
        private Vector3 _frontLookAtLocalOffset = new Vector3(0, 1.5f, 0);

        [SerializeField, Min(0.01f), Tooltip("正面ショットのカメラ位置")]
        private float _frontDistance = 5f;

        [SerializeField, Range(-80f, 80f), Tooltip("正面ショットの水平角度")]
        private float _frontHorizontalAngle = 0f;

        [SerializeField, Range(-80f, 80f), Tooltip("正面ショットの迎角")]
        private float _frontVerticalAngle = 0f;

        [SerializeField, Range(-45f, 45f), Tooltip("正面ショットのロール角度")]
        private float _frontRollAngle = 0f;

        [Header("RearShot")]
        [SerializeField, Tooltip("プレイヤーを基準にしたカメラの注視点オフセット")]
        private Vector3 _rearLookAtLocalOffset = new Vector3(0, 1.5f, 0);

        [SerializeField, Min(0.01f), Tooltip("背面ショットのカメラ位置")]
        private float _rearDistance = 5f;

        [SerializeField, Tooltip("背面ショットの水平角度")]
        private float _rearHorizontalAngle = 180f;

        [SerializeField, Range(-80f, 80f), Tooltip("背面ショットの迎角")]
        private float _rearVerticalAngle = 0f;

        [SerializeField, Range(-45f, 45f), Tooltip("背面ショットのロール角度")]
        private float _rearRollAngle = 0f;

        [Header("Orbit")]
        [SerializeField, Min(0.01f), Tooltip("背面ショットの保持時間")]
        private float _rearHoldDuration = 2f;

        [SerializeField, Min(0.1f), Tooltip("正面ショットから背面ショットへのオービット時間")]
        private float _orbitDuration = 2f;

        [SerializeField, Tooltip("正面ショットから背面ショットへのオービットのイージング")]
        private AnimationCurve _orbitEasing;

        [Header("Handoff")]
        [SerializeField, Min(0f), Tooltip("背面ショットからプレイヤー操作へのハンドオフ時間")]
        private float _handoffDuration = 0.2f;

        [SerializeField, Tooltip("背面ショットからプレイヤー操作へのハンドオフのイージング")]
        private AnimationCurve _handoffEasing;
    }
}
