using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     移動速度とBPMから基本アニメーションのブレンド値を計算する。
    /// </summary>
    public sealed class CharacterAnimationLocomotionCalculator
    {
        private const float WALK_THRESHOLD = 0.1f;
        private const float BASE_BPM = 60f;

        private float _bpm = BASE_BPM;
        private Vector2 _velocity;

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
        ///     基本移動アニメーションのウェイトを書き込む。
        /// </summary>
        /// <param name="weights"> 書き込み先のウェイト配列。 </param>
        public void ApplyBaseWeights(float[] weights)
        {
            if (weights == null || weights.Length == 0)
            {
                return;
            }

            float walkWeight = _velocity.magnitude >= WALK_THRESHOLD
                ? Mathf.Clamp01(_velocity.magnitude)
                : 0f;

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
    }
}
