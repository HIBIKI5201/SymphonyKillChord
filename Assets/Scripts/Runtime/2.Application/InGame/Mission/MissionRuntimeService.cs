using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    public class MissionRuntimeService
    {
        public MissionRuntimeService(
            MissionDefinition missionDefinition,
            MissionProgress missionProgress,
            MissionTimeAdvanceUsecase missionTimeAdvanceUseCase,
            MissionEnemyKilledUsecase missionEnemyKilledUseCase,
            MissionPlayerDeadUsecase missionPlayerDeadUseCase,
            MissionRuleRunner missionRuleRunner,
            MissionEvaluationRunner missionEvaluationRunner)
        {
            _missionDefinition = missionDefinition;
            _missionProgress = missionProgress;
            _missionTimeAdvanceUseCase = missionTimeAdvanceUseCase;
            _missionEnemyKilledUseCase = missionEnemyKilledUseCase;
            _missionPlayerDeadUseCase = missionPlayerDeadUseCase;
            _missionRuleRunner = missionRuleRunner;
            _missionEvaluationRunner = missionEvaluationRunner;
        }

        public MissionDefinition MissionDefinition => _missionDefinition;
        public MissionProgress MissionProgress => _missionProgress;

        public void Tick(float deltaTime)
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionTimeAdvanceUseCase.Execute(_missionProgress, deltaTime);
            _missionRuleRunner.Evaluate(_missionProgress);
        }

        public void OnEnemyKilled(EnemyMissionKey enemyMissionKey)
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionEnemyKilledUseCase.Execute(_missionProgress, enemyMissionKey);
            _missionRuleRunner.Evaluate(_missionProgress);
        }

        public void OnPlayerDead()
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionPlayerDeadUseCase.Execute(_missionProgress);
            _missionRuleRunner.Evaluate(_missionProgress);
        }

        public MissionEvaluationResult BuildEvaluationResult()
        {
            return _missionEvaluationRunner.Run(
                _missionProgress,
                _missionDefinition.EvaluationConditions);
        }

        private readonly MissionDefinition _missionDefinition;
        private readonly MissionProgress _missionProgress;
        private readonly MissionTimeAdvanceUsecase _missionTimeAdvanceUseCase;
        private readonly MissionEnemyKilledUsecase _missionEnemyKilledUseCase;
        private readonly MissionPlayerDeadUsecase _missionPlayerDeadUseCase;
        private readonly MissionRuleRunner _missionRuleRunner;
        private readonly MissionEvaluationRunner _missionEvaluationRunner;
    }
}
