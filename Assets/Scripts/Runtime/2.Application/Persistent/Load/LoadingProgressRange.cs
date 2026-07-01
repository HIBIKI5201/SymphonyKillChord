using System;
using UnityEngine;

namespace KillChord.Runtime.Application.Persistent.Load
{
    /// <summary>
    ///     ロード画面付き処理の進捗を、全体進捗の一部に変換して通知するIProgress実装。
    /// </summary>
    public class LoadingProgressRange : IProgress<float>
    {
        /// <summary>
        ///     進捗通知先と変換範囲を指定して生成する。
        /// </summary>
        /// <param name="progress"> 変換後の進捗通知先。 </param>
        /// <param name="startProgress"> 開始進捗。 </param>
        /// <param name="endProgress"> 終了進捗。 </param>
        public LoadingProgressRange(
            IProgress<float> progress,
            float startProgress,
            float endProgress)
        {
            _progress = progress
                ?? throw new ArgumentNullException(nameof(progress));

            _startProgress = Mathf.Clamp01(startProgress);
            _endProgress = Mathf.Clamp01(endProgress);

            if (_startProgress > _endProgress)
            {
                throw new ArgumentException(
                     "開始進捗は終了進捗以下である必要があります。");
            }
        }

        /// <summary>
        ///     0から1の進捗を指定範囲へ変換して通知する。
        /// </summary>
        /// <param name="value"> 0から1の進捗。 </param>
        public void Report(float value)
        {
            float normalizedProgress = Mathf.Clamp01(value);

            float convertedProgress =
                _startProgress
                + (_endProgress - _startProgress) * normalizedProgress;

            _progress.Report(convertedProgress);
        }

        private readonly IProgress<float> _progress;
        private readonly float _startProgress;
        private readonly float _endProgress;
    }
}
