using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     ミッションに関するイベントを制御するクラス。
    /// </summary>
    public class MissionEventController
    {
        /// <summary>
        ///     MissionEventController クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="missionRuntimeService">ミッションランタイムサービス。</param>
        /// <param name="missionHudPresenter">ミッションHUDプレゼンター。</param>
        public MissionEventController(
            MissionRuntimeService missionRuntimeService,
            MissionHudPresenter missionHudPresenter)
        {
            _missionRuntimeService = missionRuntimeService;
            _missionHudPresenter = missionHudPresenter;
        }

        /// <summary>
        ///     定期更新処理を行います。
        /// </summary>
        /// <param name="deltaTime">経過時間。</param>
        public void Tick(float deltaTime)
        {
            _missionRuntimeService.Tick(deltaTime);
            _missionHudPresenter.Present();
        }

        /// <summary>
        ///     敵が撃破されたことを通知します。
        /// </summary>
        /// <param name="enemyMissionKey">敵のキー。</param>
        public void NotifyEnemyKilled(EnemyMissionKey enemyMissionKey)
        {
            _missionRuntimeService.OnEnemyKilled(enemyMissionKey);
            _missionHudPresenter.Present();
        }

        /// <summary>
        ///     プレイヤーが死亡したことを通知します。
        /// </summary>
        public void NotifyPlayerDead()
        {
            _missionRuntimeService.OnPlayerDead();
            _missionHudPresenter.Present();
        }

        /// <summary>
        ///     プレイヤー行動が発動したことを通知します。
        /// </summary>
        /// <param name="actionKind">発動した行動の種別。</param>
        public void NotifyActionPerformed(MissionActionKind actionKind)
        {
            _missionRuntimeService.OnActionPerformed(actionKind);
            _missionHudPresenter.Present();
        }

        /// <summary>
        ///     ひとつの操作から発生した複数のプレイヤー行動を、まとめて通知します。
        /// </summary>
        /// <param name="actionKinds">同時に発動した行動の種別一覧。</param>
        public void NotifyActionsPerformed(IReadOnlyList<MissionActionKind> actionKinds)
        {
            _missionRuntimeService.OnActionsPerformed(actionKinds);
            _missionHudPresenter.Present();
        }

        /// <summary> ミッションランタイムサービス。 </summary>
        private readonly MissionRuntimeService _missionRuntimeService;
        /// <summary> ミッションHUDプレゼンター。 </summary>
        private readonly MissionHudPresenter _missionHudPresenter;
    }
}
