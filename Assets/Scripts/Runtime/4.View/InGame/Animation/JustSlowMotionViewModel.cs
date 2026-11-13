using KillChord.Runtime.Adaptor.InGame.Animation;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     ジャスト成立時のスローモーションの状態を保持し、
    ///     プレイヤーと敵のアニメーションへ現在の再生倍率を渡すViewModel。
    ///     ゲーム内時刻（Time.time）で進めるため、ポーズ中は止まる。
    /// </summary>
    public sealed class JustSlowMotionViewModel : IJustSlowMotionViewModel
    {
        /// <summary>
        ///     スローモーションの設定を受け取る。
        /// </summary>
        /// <param name="settings"> スローモーションの長さと倍率の設定。 </param>
        public JustSlowMotionViewModel(JustSlowMotionSettings settings)
        {
            _curve = new JustSlowMotionCurve(settings);
        }

        /// <summary>
        ///     このフレームのアニメーション再生倍率を取得する。
        /// </summary>
        /// <returns> アニメーション再生倍率。等速は1。 </returns>
        public float GetCurrentPlaybackScale()
        {
            return _curve.GetPlaybackScale(Time.time, Time.deltaTime);
        }

        /// <summary>
        ///     現在の時刻からスローモーションを始める。
        /// </summary>
        public void PlayJustSlowMotion()
        {
            _curve.StartAt(Time.time);
        }

        private readonly JustSlowMotionCurve _curve;
    }
}
