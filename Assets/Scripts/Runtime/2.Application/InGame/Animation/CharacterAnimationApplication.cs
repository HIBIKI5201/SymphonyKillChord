using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public class CharacterAnimationApplication : ICharacterAnimationApplication
    {
        /// <summary> アニメーションのブレンド結果。 </summary>
        public CharacterAnimationBlendData BlendData
        {
            get
            {
                float walkWeight = _velocity.magnitude >= WALK_THRESHOLD
                    ? Mathf.Clamp01(_velocity.magnitude)
                    : 0f;
                return new CharacterAnimationBlendData(1f - walkWeight, walkWeight);
            }
        }

        /// <summary> 現在のアニメーション状態。 </summary>
        public CharacterAnimationState CurrentState
            => _velocity.magnitude < WALK_THRESHOLD ? CharacterAnimationState.Idle : CharacterAnimationState.Walk;

        /// <summary> アニメーションの再生速度。 </summary>
        public float AnimationSpeed => _bpm / BASE_BPM;

        /// <summary> キャラクターの速度を設定します。 </summary>
        public void SetVelocity(Vector2 velocity)
        {
            _velocity = velocity;
        }

        /// <summary> BPMを設定する。 </summary>
        public void SetBpm(float bpm)
        {
            _bpm = Mathf.Max(1f, bpm);
        }

        /// <summary>   歩き状態に遷移する速度の閾値。 </summary>
        private const float WALK_THRESHOLD = 0.1f;
        /// <summary>   基準となるBPM値。</summary>
        private const float BASE_BPM = 60f;
        private float _bpm;
        private Vector2 _velocity;
    }
}
