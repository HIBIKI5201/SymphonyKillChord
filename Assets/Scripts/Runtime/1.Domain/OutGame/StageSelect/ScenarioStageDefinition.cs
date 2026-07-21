using System;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     シナリオステージ固有の定義情報を表すクラス。
    /// </summary>
    public sealed class ScenarioStageDefinition : StageDefinition
    {
        /// <summary>
        ///     シナリオステージの定義情報を初期化する。
        /// </summary>
        /// <param name="stageId"> ステージのID。 </param>
        /// <param name="stageName"> ステージの名前。 </param>
        /// <param name="flavorText"> ステージのフレーバーテキスト。 </param>
        /// <param name="reward"> ステージの報酬情報。 </param>
        /// <param name="targetSceneName"> ステージのターゲットシーン名。 </param>
        /// <param name="scenarioId"> 再生するシナリオID。 </param>
        public ScenarioStageDefinition(
            StageId stageId,
            string stageName,
            string flavorText,
            StageReward reward,
            string targetSceneName,
            string scenarioId)
            : base(stageId, stageName, flavorText, reward, targetSceneName)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                throw new ArgumentException("Scenario id must not be empty.", nameof(scenarioId));
            }

            _scenarioId = scenarioId;
        }

        /// <summary> ステージの種類。 </summary>
        public override StageType StageType => StageType.Scenario;
        /// <summary> 再生するシナリオID。 </summary>
        public string ScenarioId => _scenarioId;

        private readonly string _scenarioId;
    }
}
