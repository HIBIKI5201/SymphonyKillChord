using DevelopProducts.Design.GameMode.Adaptor;
using DevelopProducts.Design.GameMode.Application;
using DevelopProducts.Design.GameMode.Domain;
using DevelopProducts.Design.GameMode.InfraStructure;
using DevelopProducts.Design.GameMode.View;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Composition
{
    /// <summary>
    ///     モック全体の初期化を行うクラス。
    ///     テスト用。
    /// </summary>
    public class GameModeMockInitilizer : MonoBehaviour
    {
        [SerializeField] private GameModeDefinition _gameModeDefinition;
        [SerializeField] private PlayerCollisionView _playerCollisionView;
        [SerializeField] private StageLoopView _stageLoopView;
        [SerializeField] private InGameHudView _inGameHudView;

        private void Awake()
        {
            PlayerRuntimeState playerRuntimeState = new PlayerRuntimeState(3);
            StageTimeState stageTimeState = new StageTimeState();
            EnemyKillStatics enemyKillStatics = new EnemyKillStatics();

            StageRuntimeContext context = new StageRuntimeContext(
                playerRuntimeState,
                enemyKillStatics,
                stageTimeState);

            StageRuleRunner stageRuleRunner = new StageRuleRunner(context,
                _gameModeDefinition.ClearConditions,
                _gameModeDefinition.FailConditions);

            EvaluationRunner evaluationRunner = new EvaluationRunner(
                context,
                _gameModeDefinition.EvaluationConditions);

            GameModeRuntime gameModeRuntime = new GameModeRuntime(
                stageRuleRunner,
                evaluationRunner);

            DamagePlayerUsecase damagePlayerUsecase = new DamagePlayerUsecase(context);
            DamageEnemyUsecase damageEnemyUsecase = new DamageEnemyUsecase(context);
            AdvanceTimeUsecase advanceTimeUsecase = new AdvanceTimeUsecase(context);

            StageHudViewModel stageHudViewModel = new StageHudViewModel();
            StageHudPresenter stageHudPresenter = new StageHudPresenter(context,
                gameModeRuntime, 
                stageHudViewModel);

            PlayerColisionController playerColisionController = new PlayerColisionController(damagePlayerUsecase,
                damageEnemyUsecase,
                gameModeRuntime,
                stageHudPresenter
                );

            stageHudPresenter.Present();

            _playerCollisionView.Initialize(playerColisionController);
            _stageLoopView.Initialize(
                advanceTimeUsecase,
                gameModeRuntime,
                stageHudPresenter,
                stageHudViewModel,
                _inGameHudView
                );
        }
    }
}
