using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;
using UnityEngine.Playables;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     Timelineでスキルエフェクトを再生するストラテジー。
    /// </summary>
    public sealed class TimelineSkillEffectPresentation : SkillEffectPresentationBase
    {
        [SerializeField, Tooltip("再生するPlayableDirectorです。未設定時は自身から取得します。")]
        private PlayableDirector _director;

        /// <summary>
        ///     PlayableDirectorの参照を解決する。
        /// </summary>
        private void Awake()
        {
            if (_director == null)
            {
                _director = GetComponent<PlayableDirector>();
            }
        }

        /// <summary>
        ///     PlayableGraphを事前生成し、初回再生時のヒッチを防ぐ。
        /// </summary>
        protected override void OnPrewarm()
        {
            if (_director == null)
            {
                _director = GetComponent<PlayableDirector>();
            }

            if (_director == null)
            {
                return;
            }

            _director.playOnAwake = false;
            _director.extrapolationMode = DirectorWrapMode.None;
            _director.RebuildGraph();
            _director.Stop();
        }

        /// <summary>
        ///     Timelineを先頭から再生する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        protected override void OnPlay(in SkillEffectContext context)
        {
            if (_director == null)
            {
                return;
            }

            _director.time = 0d;
            _director.Play();
        }

        /// <summary>
        ///     Timelineを停止する。
        /// </summary>
        protected override void OnStop()
        {
            if (_director == null)
            {
                return;
            }

            _director.Stop();
            _director.time = 0d;
        }

        /// <summary>
        ///     再生時間の経過で再生継続を判定する。
        /// </summary>
        /// <param name="elapsedSeconds"> 再生開始からの経過時間です。 </param>
        /// <returns> 再生が継続している場合はtrue。 </returns>
        protected override bool OnCheckPlaying(float elapsedSeconds)
        {
            if (_director == null)
            {
                return false;
            }

            return _director.state == PlayState.Playing && _director.time < _director.duration;
        }
    }
}
