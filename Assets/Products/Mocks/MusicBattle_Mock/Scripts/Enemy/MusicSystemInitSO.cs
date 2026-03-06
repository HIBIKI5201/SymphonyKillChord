using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽同期システムの初期化パラメータを定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "MusicSystemInitSO", menuName = "Mock/MusicBattle/MusicSystemInitSO")]
    public class MusicSystemInitSO : ScriptableObject
    {
        #region パブリックプロパティ
        /// <summary> BGMのBPM（Beats Per Minute）。 </summary>
        public double Bpm => _bpm;
        /// <summary> BGMの拍子。 </summary>
        public double TimeSignature => _timeSignature;
        /// <summary> 最初の小節の開始オフセット（ミリ秒）。 </summary>
        public long StartOffset => _startOffset;
        #endregion

        #region インスペクター表示フィールド
        /// <summary> BGMのBPM（Beats Per Minute）。 </summary>
        [Tooltip("BGMのBPM（Beats Per Minute）。")]
        [SerializeField]
        private double _bpm = 120;
        /// <summary> BGMの拍子。 </summary>
        [Tooltip("BGMの拍子。")]
        [SerializeField]
        private double _timeSignature = 4;
        /// <summary> 最初の小節の開始オフセット（ミリ秒）。 </summary>
        [Tooltip("最初の小節の開始オフセット（ミリ秒）。")]
        [SerializeField]
        private long _startOffset = 0;
        #endregion
    }
}

