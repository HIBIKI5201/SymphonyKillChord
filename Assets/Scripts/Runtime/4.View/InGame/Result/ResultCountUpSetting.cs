using System;
using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Result
{
    /// <summary>
    ///     リザルト画面の数値をカウントアップ表示させる演出の設定です。
    /// </summary>
    [Serializable]
    public sealed class ResultCountUpSetting
    {
        /// <summary> 演出を再生するかどうか。 </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary> カウントアップにかける時間（秒）。 </summary>
        public float Duration => Mathf.Max(0f, _duration);

        /// <summary> カウントアップのイージング。 </summary>
        public Ease Ease => _ease;

        [SerializeField, Tooltip("数値のカウントアップ演出を再生するか。")]
        private bool _isEnabled = true;

        [SerializeField, Min(0f), Tooltip("カウントアップにかける時間（秒）。")]
        private float _duration = 1f;

        [SerializeField, Tooltip("カウントアップのイージング。")]
        private Ease _ease = Ease.OutCubic;
    }
}
