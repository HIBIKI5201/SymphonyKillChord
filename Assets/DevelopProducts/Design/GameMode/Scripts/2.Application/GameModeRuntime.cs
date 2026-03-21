using UnityEngine;

namespace DevelopProducts.Design.GameMode.Application
{
    /// <summary>
    ///     GameModeの実行状態を管理するクラス。ゲームの進行状況を追跡し、ステージの終了条件を評価する役割を持つ。
    /// </summary>
    public class GameModeRuntime
    {
        public GameModeRuntime(StageRuleRunner stageRuleRunner, EvaluationRunner evaluationRunner)
        {
            _stageRuleRunner = stageRuleRunner;
            _evaluationRunner = evaluationRunner;
        }

        public bool IsFinished => _stageRuleRunner.IsStageEnded;
        public StageEndReason EndReason => _stageRuleRunner.EndReason;

        public void Tick()
        {
            _stageRuleRunner.EvaluateStageEnd();
        }

        public EvaluationResult BuildEvaluationResult()
        {
            return _evaluationRunner.Run();
        }

        private readonly StageRuleRunner _stageRuleRunner;
        private readonly EvaluationRunner _evaluationRunner;
    }
}
