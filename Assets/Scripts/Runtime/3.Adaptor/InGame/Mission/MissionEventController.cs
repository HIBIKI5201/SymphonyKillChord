using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;

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

        /// <summary> ミッションランタイムサービス。 </summary>
        private readonly MissionRuntimeService _missionRuntimeService;
        /// <summary> ミッションHUDプレゼンター。 </summary>
        private readonly MissionHudPresenter _missionHudPresenter;
    }
}
