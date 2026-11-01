using System;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     ジャスト成立時のスローモーションと、その後に遅れを取り戻す再生倍率を時刻から計算する。
    ///     実時間に対するアニメーションの遅れを、スロー中は一定の割合で増やし、
    ///     取り戻し中は Ease Out（最初が速く、等速へ滑らかに戻る）で0まで減らす。
    ///     遅れは必ず0へ戻るため、スローの後もアニメーションと音楽のタイミングはずれない。
    /// </summary>
    public sealed class JustSlowMotionCurve
    {
        /// <summary>
        ///     スローモーションの設定を受け取る。
        /// </summary>
        /// <param name="settings"> スローモーションの長さと倍率の設定。 </param>
        public JustSlowMotionCurve(JustSlowMotionSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        ///     指定時刻からスローモーションを始める。取り戻し中に呼ばれた場合は、残っている遅れに上乗せする。
        /// </summary>
        /// <param name="time"> 開始時刻（秒）。 </param>
        public void StartAt(float time)
        {
            _initialLagSeconds = GetLagSeconds(time);
            _startTime = time;
            _hasStarted = true;
        }

        /// <summary>
        ///     指定時刻での、実時間に対するアニメーションの遅れ（秒）を取得する。
        /// </summary>
        /// <param name="time"> 時刻（秒）。 </param>
        /// <returns> アニメーションの遅れ（秒）。遅れが無いときは0。 </returns>
        public float GetLagSeconds(float time)
        {
            if (!_hasStarted)
            {
                return 0f;
            }

            float elapsed = time - _startTime;
            if (elapsed <= 0f)
            {
                return _initialLagSeconds;
            }

            // スロー中は、等速との差の分だけ遅れが増える。
            float slowDuration = _settings.SlowDurationSeconds;
            float lagGrowthRate = 1f - _settings.SlowPlaybackScale;
            if (elapsed < slowDuration)
            {
                return _initialLagSeconds + lagGrowthRate * elapsed;
            }

            // 取り戻し中は、遅れを (1 - 進捗)^3 で減らす。再生倍率は最初が最も速く、終わりで等速になる。
            float maxLag = _initialLagSeconds + lagGrowthRate * slowDuration;
            float progress = (elapsed - slowDuration) / _settings.CatchUpDurationSeconds;
            if (progress >= 1f)
            {
                return 0f;
            }

            float remaining = 1f - progress;
            return maxLag * remaining * remaining * remaining;
        }

        /// <summary>
        ///     直前のフレームから指定時刻までの、平均のアニメーション再生倍率を取得する。
        ///     遅れの差分から求めるため、フレームの長さによらず遅れの合計が設定どおりになる。
        /// </summary>
        /// <param name="time"> 現在の時刻（秒）。 </param>
        /// <param name="deltaTime"> 直前のフレームからの経過時間（秒）。 </param>
        /// <returns> アニメーション再生倍率。等速は1。 </returns>
        public float GetPlaybackScale(float time, float deltaTime)
        {
            if (!_hasStarted || deltaTime <= 0f)
            {
                return 1f;
            }

            float lagDelta = GetLagSeconds(time) - GetLagSeconds(time - deltaTime);
            return Mathf.Max(0f, 1f - lagDelta / deltaTime);
        }

        private readonly JustSlowMotionSettings _settings;
        private float _startTime;
        private float _initialLagSeconds;
        private bool _hasStarted;
    }
}
