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



        [Header("Fade")]
        [SerializeField, Tooltip("黒画面を表示する時間")]
        private float _blackholdDuration = 0.2f;

        [SerializeField, Tooltip("黒画面からフェードアウトする時間")]
        private float _fadeOutDuration = 0.4f;

        [SerializeField, Tooltip("黒画面からフェードアウトする際のイージング")]
        private AnimationCurve _fadeOutEasing;
    }
}
