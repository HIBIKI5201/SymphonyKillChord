using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽同期システムの初期化パラメータを定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "MusicSystemInitSO", menuName = "Mock/MusicBattle/MusicSystemInitSO")]
    public class MusicSystemInitSO : ScriptableObject
    {
        /// <summary> BGMのBPM（Beats Per Minute）。 </summary>
        [Tooltip("BGMのBPM（Beats Per Minute）。")]
        public double Bpm = 120;
        /// <summary> BGMの拍子。 </summary>
        [Tooltip("BGMの拍子。")]
        public double TimeSignature = 4;
        /// <summary> 最初の小節の開始オフセット（ミリ秒）。 </summary>
        [Tooltip("最初の小節の開始オフセット（ミリ秒）。")]
        public long StartOffset = 0;
    }
}

