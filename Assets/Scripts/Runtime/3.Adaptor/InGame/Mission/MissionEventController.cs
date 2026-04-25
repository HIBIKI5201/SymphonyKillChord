using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    public class MissionEventController
    {
        public MissionEventController(
            MissionRuntimeService missionRuntimeService,
            MissionHudPresenter missionHudPresenter)
        {
            _missionRuntimeService = missionRuntimeService;
            _missionHudPresenter = missionHudPresenter;
        }

        public void Tick(float deltaTime)
        {
            _missionRuntimeService.Tick(deltaTime);
            _missionHudPresenter.Present();
        }

        public void NotifyEnemyKilled(EnemyMissionKey enemyMissionKey)
        {
            _missionRuntimeService.OnEnemyKilled(enemyMissionKey);
            _missionHudPresenter.Present();
        }

        public void NotifyPlayerDead()
        {
            _missionRuntimeService.OnPlayerDead();
            _missionHudPresenter.Present();
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly MissionHudPresenter _missionHudPresenter;
    }
}
