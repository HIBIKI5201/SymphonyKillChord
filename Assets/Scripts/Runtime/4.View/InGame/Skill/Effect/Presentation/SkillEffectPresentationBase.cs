using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     スキルエフェクトの再生手段を表すストラテジーの基底クラス。
    ///     ParticleSystem、VFX Graph、LitMotion、Timelineなど手段ごとに派生させる。
    /// </summary>
    public abstract class SkillEffectPresentationBase : MonoBehaviour
    {
        /// <summary> 再生中かどうかです。 </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        ///     プール生成時に一度だけ呼ばれる事前準備を行う。
        /// </summary>
        public void Prewarm()
        {
            OnPrewarm();
        }

        /// <summary>
        ///     エフェクトを再生する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        public void Play(in SkillEffectContext context)
        {
            _elapsedSeconds = 0f;
            _isPlaying = true;
            OnPlay(context);
        }

        /// <summary>
        ///     エフェクトを停止する。
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying)
            {
                return;
            }

            _isPlaying = false;
            OnStop();
        }

        /// <summary>
        ///     再生状態を進行させ、継続中かどうかを返す。
        /// </summary>
        /// <param name="deltaTime"> 経過時間です。 </param>
        /// <returns> 再生が継続している場合はtrue。 </returns>
        public bool UpdatePlayback(float deltaTime)
        {
            if (!_isPlaying)
            {
                return false;
            }

            _elapsedSeconds += deltaTime;

            // 最短再生時間の間は、再生開始直後の判定ブレによる誤完了を防ぐ。
            if (_elapsedSeconds < _minimumDurationSeconds)
            {
                return true;
            }

            if (OnCheckPlaying(_elapsedSeconds))
            {
                return true;
            }

            _isPlaying = false;
            OnStop();
            return false;
        }

        [SerializeField, Min(0f), Tooltip("完了判定を開始するまでの最短再生時間です。")]
        private float _minimumDurationSeconds = 0.05f;

        /// <summary>
        ///     プール生成時の事前準備を行う。
        /// </summary>
        protected virtual void OnPrewarm()
        {
        }

        /// <summary>
        ///     派生クラスごとの再生処理を行う。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        protected abstract void OnPlay(in SkillEffectContext context);

        /// <summary>
        ///     派生クラスごとの停止処理を行う。
        /// </summary>
        protected abstract void OnStop();

        /// <summary>
        ///     派生クラスごとの再生継続判定を行う。
        /// </summary>
        /// <param name="elapsedSeconds"> 再生開始からの経過時間です。 </param>
        /// <returns> 再生が継続している場合はtrue。 </returns>
        protected abstract bool OnCheckPlaying(float elapsedSeconds);

        private float _elapsedSeconds;
        private bool _isPlaying;
    }
}
