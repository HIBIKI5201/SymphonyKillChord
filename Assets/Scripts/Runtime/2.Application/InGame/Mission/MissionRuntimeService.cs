using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     ミッションのランタイムにおける状態と処理を管理するサービスクラス。
    /// </summary>
    public class MissionRuntimeService
    {
        /// <summary>
        ///     MissionRuntimeService クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="missionDefinition">ミッション定義。</param>
        /// <param name="missionProgress">進行状況。</param>
        /// <param name="missionTimeAdvanceUseCase">時間経過ユースケース。</param>
        /// <param name="missionEnemyKilledUseCase">敵撃破ユースケース。</param>
        /// <param name="missionPlayerDeadUseCase">プレイヤー死亡ユースケース。</param>
        /// <param name="missionRuleRunner">ルール評価器。</param>
        /// <param name="missionEvaluationRunner">評価実行器。</param>
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

        /// <summary> ミッション定義を取得します。 </summary>
        public MissionDefinition MissionDefinition => _missionDefinition;
        /// <summary> 進行状況を取得します。 </summary>
        public MissionProgress MissionProgress => _missionProgress;

        /// <summary>
        ///     定期更新処理を行います。
        /// </summary>
        /// <param name="deltaTime">経過時間。</param>
        public void Tick(float deltaTime)
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionTimeAdvanceUseCase.Execute(_missionProgress, deltaTime);
            _missionRuleRunner.Evaluate(_missionProgress);
        }

        /// <summary>
        ///     敵が撃破された際の処理を行います。
        /// </summary>
        /// <param name="enemyMissionKey">敵のキー。</param>
        public void OnEnemyKilled(EnemyMissionKey enemyMissionKey)
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionEnemyKilledUseCase.Execute(_missionProgress, enemyMissionKey);
            _missionRuleRunner.Evaluate(_missionProgress);
        }

        /// <summary>
        ///     プレイヤーが死亡した際の処理を行います。
        /// </summary>
        public void OnPlayerDead()
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionPlayerDeadUseCase.Execute(_missionProgress);
            _missionRuleRunner.Evaluate(_missionProgress);
        }

        /// <summary>
        ///     評価結果を構築します。
        /// </summary>
        /// <returns>評価結果。</returns>
        public MissionEvaluationResult BuildEvaluationResult()
        {
            return _missionEvaluationRunner.Run(
                _missionProgress,
                _missionDefinition.EvaluationConditions);
        }

        /// <summary> ミッション定義。 </summary>
        private readonly MissionDefinition _missionDefinition;
        /// <summary> 進行状況。 </summary>
        private readonly MissionProgress _missionProgress;
        /// <summary> 時間経過ユースケース。 </summary>
        private readonly MissionTimeAdvanceUsecase _missionTimeAdvanceUseCase;
        /// <summary> 敵撃破ユースケース。 </summary>
        private readonly MissionEnemyKilledUsecase _missionEnemyKilledUseCase;
        /// <summary> プレイヤー死亡ユースケース。 </summary>
        private readonly MissionPlayerDeadUsecase _missionPlayerDeadUseCase;
        /// <summary> ルール評価器。 </summary>
        private readonly MissionRuleRunner _missionRuleRunner;
        /// <summary> 評価実行器。 </summary>
        private readonly MissionEvaluationRunner _missionEvaluationRunner;
    }
}
