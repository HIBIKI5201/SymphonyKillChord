using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     移動速度とBPMから基本アニメーションのブレンド値を計算する。
    /// </summary>
    public sealed class CharacterAnimationLocomotionCalculator
    {
        /// <summary> 歩行アニメーションへ切り替える速度のしきい値。 </summary>
        public const float WALK_THRESHOLD = 0.1f;

        /// <summary> 現在のBPMに基づく再生速度を取得する。 </summary>
        public float AnimationSpeed => _bpm / BASE_BPM;

        /// <summary>
        ///     移動速度を設定する。
        /// </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        public void SetVelocity(Vector2 velocity)
        {
            _velocity = velocity;
        }

        /// <summary>
        ///     BPMを設定する。
        /// </summary>
        /// <param name="bpm"> 現在のBPM。 </param>
        public void SetBpm(float bpm)
        {
            _bpm = Mathf.Max(1f, bpm);
        }

        /// <summary>
        ///     歩行ウェイトを目標値へ時間をかけて近づける。
        ///     しきい値を跨いだ瞬間にIdleとWalkが切り替わる段差を防ぐ。
        /// </summary>
        /// <param name="deltaTime"> 前フレームからの経過秒数。 </param>
        /// <param name="blendSeconds"> 歩行ウェイトが0から1まで変わるのにかける秒数。 </param>
        public void UpdateWalkWeight(float deltaTime, float blendSeconds)
        {
            // しきい値でウェイト0になるよう速度を写し、しきい値の前後で値が飛ばないようにする。
            float targetWeight = Mathf.InverseLerp(WALK_THRESHOLD, WALK_FULL_SPEED, _velocity.magnitude);

            if (blendSeconds <= 0f)
            {
                _walkWeight = targetWeight;
                return;
            }

            _walkWeight = Mathf.MoveTowards(_walkWeight, targetWeight, deltaTime / blendSeconds);
        }

        /// <summary>
        ///     曲の拍位置から、ロコモーションのクリップを頭出しする再生位置を計算する。
        ///     クリップはBPM60（1拍＝1秒）で作る前提のため、拍数をそのまま秒に読み替える。
        /// </summary>
        /// <param name="beat"> 最初の小節頭からの拍数。負の値も扱う。 </param>
        /// <param name="clipLength"> クリップの長さ（秒）。 </param>
        /// <returns> クリップ内の再生位置（秒）。クリップ長が0以下なら0。 </returns>
        public double CalculateClipTime(double beat, float clipLength)
        {
            if (clipLength <= 0f || double.IsNaN(beat) || double.IsInfinity(beat))
            {
                return 0d;
            }

            double clipTime = beat * (SECONDS_PER_MINUTE / BASE_BPM) % clipLength;
            return clipTime < 0d ? clipTime + clipLength : clipTime;
        }

        /// <summary>
        ///     基本移動アニメーションのウェイトを書き込む。
        /// </summary>
        /// <param name="weights"> 書き込み先のウェイト配列。 </param>
        public void ApplyBaseWeights(float[] weights)
        {
            if (weights == null || weights.Length == 0)
            {
                return;
            }

            float walkWeight = _walkWeight;

            int idleIndex = (int)CharacterAnimationClipType.Idle;
            int walkIndex = (int)CharacterAnimationClipType.Walk;

            if (idleIndex < weights.Length)
            {
                weights[idleIndex] = 1f - walkWeight;
            }

            if (walkIndex < weights.Length)
            {
                weights[walkIndex] = walkWeight;
            }
        }

        private const float BASE_BPM = 60f;
        private const float SECONDS_PER_MINUTE = 60f;
        /// <summary> 歩行ウェイトが1になる速度。 </summary>
        private const float WALK_FULL_SPEED = 1f;

        private float _bpm = BASE_BPM;
        private Vector2 _velocity;
        private float _walkWeight;
    }
}
