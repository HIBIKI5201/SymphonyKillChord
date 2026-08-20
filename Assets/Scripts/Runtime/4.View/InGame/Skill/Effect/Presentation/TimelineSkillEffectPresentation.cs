using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     Timelineでスキルエフェクトを再生するストラテジー。
    ///     再生は投げっぱなしにせず、Timelineの終了まで待機して完了を通知する。
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
            EnsureDirector();
        }

        /// <summary>
        ///     PlayableGraphを事前生成し、初回再生時のヒッチを防ぐ。
        /// </summary>
        protected override void OnPrewarm()
        {
            EnsureDirector();
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
        ///     Timelineを先頭から再生し、終端に達するまで待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        protected override async Awaitable OnPlayAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            if (_director == null)
            {
                return;
            }

            _director.time = 0d;
            _director.Play();

            // BPMに応じてTimeline全体の進行速度を合わせる。
            ApplyPlaybackSpeed(context.PlaybackSpeed);

            // Playを呼んだ直後はstateが更新されていないため、1フレーム進めてから監視する。
            await Awaitable.NextFrameAsync(cancellationToken);
            while (_director != null
                && _director.state == PlayState.Playing
                && _director.time < _director.duration)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        /// <summary>
        ///     Timelineを停止して先頭へ戻す。
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
        ///     再生速度をPlayableGraphへ適用する。
        /// </summary>
        /// <param name="playbackSpeed"> 適用する再生速度倍率です。 </param>
        private void ApplyPlaybackSpeed(float playbackSpeed)
        {
            if (!_director.playableGraph.IsValid() || _director.playableGraph.GetRootPlayableCount() == 0)
            {
                return;
            }

            _director.playableGraph.GetRootPlayable(0).SetSpeed(playbackSpeed);
        }

        /// <summary>
        ///     PlayableDirectorの参照を必要時に解決する。
        /// </summary>
        private void EnsureDirector()
        {
            if (_director == null)
            {
                _director = GetComponent<PlayableDirector>();
            }
        }
    }
}
