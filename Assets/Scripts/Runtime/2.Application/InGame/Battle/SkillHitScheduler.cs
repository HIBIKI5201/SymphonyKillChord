using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     連撃スキルのヒットを時間差で適用するスケジューラ。
    ///     ダメージのタイミングはこのスケジューラが権限を持ち、エフェクトの生成可否には依存しない。
    /// </summary>
    public sealed class SkillHitScheduler
    {
        /// <summary>
        ///     BPMに応じた再生速度倍率を設定する。
        ///     演出と同じ倍率を適用し、ダメージのタイミングを揃える。
        /// </summary>
        /// <param name="playbackSpeed"> 再生速度倍率です。 </param>
        public void SetPlaybackSpeed(float playbackSpeed)
        {
            _playbackSpeed = playbackSpeed > 0f ? playbackSpeed : 1f;
        }

        /// <summary>
        ///     連撃を予約する。
        /// </summary>
        /// <param name="hitCount"> 総ヒット数です。 </param>
        /// <param name="delaySeconds"> 1発目までの待機時間です。 </param>
        /// <param name="intervalSeconds"> 2発目以降の間隔です。 </param>
        /// <param name="onHit"> 各ヒットで実行する処理です。継続する場合はtrueを返します。 </param>
        public void Schedule(int hitCount, float delaySeconds, float intervalSeconds, Func<int, bool> onHit)
        {
            if (hitCount <= 0 || onHit == null)
            {
                return;
            }

            _pendingHits.Add(new PendingHit(
                hitCount,
                MathF.Max(0f, delaySeconds) / _playbackSpeed,
                MathF.Max(0f, intervalSeconds) / _playbackSpeed,
                onHit));
        }

        /// <summary>
        ///     経過時間を進め、時刻に達したヒットを適用する。
        /// </summary>
        /// <param name="deltaTime"> 経過時間です。 </param>
        public void Tick(float deltaTime)
        {
            if (_pendingHits.Count == 0)
            {
                return;
            }

            for (int i = _pendingHits.Count - 1; i >= 0; i--)
            {
                PendingHit pendingHit = _pendingHits[i];
                if (!pendingHit.Advance(deltaTime))
                {
                    _pendingHits.RemoveAt(i);
                    continue;
                }

                _pendingHits[i] = pendingHit;
            }
        }

        /// <summary>
        ///     予約中の連撃をすべて破棄する。
        /// </summary>
        public void Clear()
        {
            _pendingHits.Clear();
        }

        private readonly List<PendingHit> _pendingHits = new();
        private float _playbackSpeed = 1f;

        /// <summary>
        ///     予約中の連撃1件分の状態を保持する構造体。
        /// </summary>
        private struct PendingHit
        {
            /// <summary>
            ///     予約内容を受け取って初期化する。
            /// </summary>
            /// <param name="remainingHitCount"> 残りヒット数です。 </param>
            /// <param name="delaySeconds"> 1発目までの待機時間です。 </param>
            /// <param name="intervalSeconds"> 2発目以降の間隔です。 </param>
            /// <param name="onHit"> 各ヒットで実行する処理です。 </param>
            public PendingHit(int remainingHitCount, float delaySeconds, float intervalSeconds, Func<int, bool> onHit)
            {
                _remainingHitCount = remainingHitCount;
                _remainingSeconds = delaySeconds;
                _intervalSeconds = intervalSeconds;
                _onHit = onHit;
                _appliedHitCount = 0;
            }

            /// <summary>
            ///     経過時間を進め、到達したヒットを適用する。
            /// </summary>
            /// <param name="deltaTime"> 経過時間です。 </param>
            /// <returns> 予約を継続する場合はtrueです。 </returns>
            public bool Advance(float deltaTime)
            {
                _remainingSeconds -= deltaTime;

                // 1フレームで複数ヒット分の時間が経過する場合も取りこぼさない。
                while (_remainingSeconds <= 0f && _remainingHitCount > 0)
                {
                    _remainingHitCount--;
                    _appliedHitCount++;

                    if (!_onHit.Invoke(_appliedHitCount))
                    {
                        return false;
                    }

                    if (_remainingHitCount <= 0)
                    {
                        return false;
                    }

                    _remainingSeconds += _intervalSeconds;

                    // 間隔が0の場合は同一フレームで残りをすべて適用する。
                    if (_intervalSeconds <= 0f)
                    {
                        continue;
                    }
                }

                return _remainingHitCount > 0;
            }

            private readonly float _intervalSeconds;
            private readonly Func<int, bool> _onHit;
            private int _remainingHitCount;
            private int _appliedHitCount;
            private float _remainingSeconds;
        }
    }
}
