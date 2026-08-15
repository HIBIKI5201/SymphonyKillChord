using SymphonyFrameWork.Exceptions;
using SymphonyFrameWork.System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Playables;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     ポーズ状態を弾幕システムグループとPlayableDirectorへ伝えます。
    /// </summary>
    public sealed class BarragePauseBridge : MonoBehaviour, PauseManager.IPausable
    {
        /// <summary>
        ///     ポーズ開始時に弾幕演出を停止します。
        /// </summary>
        public void Pause()
        {
            ApplyPause(true);
        }

        /// <summary>
        ///     ポーズ解除時に弾幕演出を再開します。
        /// </summary>
        public void Resume()
        {
            ApplyPause(false);
        }

        [SerializeField, Tooltip("弾幕演出を再生するPlayableDirectorです。")]
        private PlayableDirector _director;

        /// <summary>
        ///     ポーズ通知の購読を開始します。
        /// </summary>
        private void Start()
        {
            try
            {
                PauseManager.IPausable.RegisterPauseManager(this);
            }
            catch (SymphonyNotInitializedException)
            {
                Debug.LogWarning(
                    $"[{nameof(BarragePauseBridge)}] PauseManagerが未初期化のため、ポーズ連動を登録できませんでした。",
                    this);
            }
        }

        /// <summary>
        ///     ポーズ通知の購読を解除します。
        /// </summary>
        private void OnDestroy()
        {
            try
            {
                PauseManager.IPausable.UnregisterPauseManager(this);
            }
            catch (SymphonyNotInitializedException)
            {
                // アプリ終了時などPauseManagerが先に破棄された場合は解除不要のため無視する。
            }
        }

        /// <summary>
        ///     ポーズ状態をEntity WorldとTimelineの両方へ反映します。
        /// </summary>
        /// <param name="isPaused"> ポーズ中にする場合はtrueです。 </param>
        private void ApplyPause(bool isPaused)
        {
            ApplyToEntityWorld(isPaused);
            ApplyToDirector(isPaused);
        }

        /// <summary>
        ///     弾幕システムグループの更新可否を切り替えます。
        /// </summary>
        /// <param name="isPaused"> ポーズ中にする場合はtrueです。 </param>
        private static void ApplyToEntityWorld(bool isPaused)
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) { return; }

            BarrageSystemGroup group = world.GetExistingSystemManaged<BarrageSystemGroup>();
            if (group == null) { return; }

            if (group.RateManager is BarragePauseRateManager rateManager)
            {
                rateManager.SetPaused(isPaused);
            }
        }

        /// <summary>
        ///     Timelineの進行を停止・再開します。
        /// </summary>
        /// <param name="isPaused"> ポーズ中にする場合はtrueです。 </param>
        private void ApplyToDirector(bool isPaused)
        {
            if (_director == null) { return; }
            if (!_director.playableGraph.IsValid()) { return; }

            // Pause()を使うとPlayableBehaviourのOnBehaviourPause/OnBehaviourPlayが走り、
            // 再開時にクリップが再トリガされてしまうため、再生速度を0にして止める。
            _director.playableGraph.GetRootPlayable(0)
                .SetSpeed(isPaused ? PAUSED_SPEED : NORMAL_SPEED);
        }

        private const double PAUSED_SPEED = 0d;

        private const double NORMAL_SPEED = 1d;
    }
}
