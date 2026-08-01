using System;
using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Result
{
    /// <summary>
    ///     リザルト画面のテキストを左から右方向へスライドインさせる演出の設定です。
    /// </summary>
    [Serializable]
    public sealed class ResultTextSlideInSetting
    {
        /// <summary>
        ///     演出を再生するかどうか。
        /// </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>
        ///     本来の位置から左へどれだけ離れた位置から流し始めるか（px）。
        /// </summary>
        public float Distance => Mathf.Max(0f, _distance);

        /// <summary>
        ///     テキスト1件あたりのスライドイン時間（秒）。
        /// </summary>
        public float Duration => Mathf.Max(0f, _duration);

        /// <summary>
        ///     テキスト1件ごとにずらす開始遅延（秒）。
        /// </summary>
        public float Interval => Mathf.Max(0f, _interval);

        /// <summary>
        ///     スライドインのイージング。
        /// </summary>
        public Ease Ease => _ease;

        /// <summary>
        ///     スライドインに合わせてフェードインするかどうか。
        /// </summary>
        public bool UseFade => _useFade;

        [SerializeField, Tooltip("テキストのスライドイン演出を再生するか。")]
        private bool _isEnabled = true;

        [SerializeField, Min(0f), Tooltip("本来の位置から左へどれだけ離れた位置から流し始めるか（px）。全テキストが同じ量だけ動く。")]
        private float _distance = 600f;

        [SerializeField, Min(0f), Tooltip("テキスト1件あたりのスライドイン時間（秒）。")]
        private float _duration = 0.35f;

        [SerializeField, Min(0f), Tooltip("テキスト1件ごとにずらす開始遅延（秒）。0で全件同時に再生する。")]
        private float _interval = 0.06f;

        [SerializeField, Tooltip("スライドインのイージング。")]
        private Ease _ease = Ease.OutCubic;

        [SerializeField, Tooltip("スライドインに合わせてフェードインするか。")]
        private bool _useFade = true;
    }
}
