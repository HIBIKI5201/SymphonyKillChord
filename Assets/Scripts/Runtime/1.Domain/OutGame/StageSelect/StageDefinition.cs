using System;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージの共通定義情報を表す抽象基底クラス。
    /// </summary>
    public abstract class StageDefinition
    {
        /// <summary>
        ///     ステージの共通定義情報を初期化する。
        /// </summary>
        /// <param name="stageId"> ステージのID。 </param>
        /// <param name="stageName"> ステージの名前。 </param>
        /// <param name="flavorText"> ステージのフレーバーテキスト。 </param>
        /// <param name="reward"> ステージの報酬情報。 </param>
        /// <param name="targetSceneName"> ステージのターゲットシーン名。 </param>
        protected StageDefinition(
            StageId stageId,
            string stageName,
            string flavorText,
            StageReward reward,
            string targetSceneName)
        {
            if (stageId.Value == 0)
            {
                throw new ArgumentException("Stage id must not be empty.", nameof(stageId));
            }

            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                throw new ArgumentException("Target scene name must not be empty.", nameof(targetSceneName));
            }

            _stageId = stageId;
            _stageName = stageName ?? string.Empty;
            _flavorText = flavorText ?? string.Empty;
            _reward = reward;
            _targetSceneName = targetSceneName;
        }

        /// <summary> ステージのID。 </summary>
        public StageId StageId => _stageId;
        /// <summary> ステージの種類。 </summary>
        public abstract StageType StageType { get; }
        /// <summary> ステージの名前。 </summary>
        public string StageName => _stageName;
        /// <summary> ステージのフレーバーテキスト。 </summary>
        public string FlavorText => _flavorText;
        /// <summary> ステージの報酬情報。 </summary>
        public StageReward Reward => _reward;
        /// <summary> ステージのターゲットシーン名。 </summary>
        public string TargetSceneName => _targetSceneName;
        /// <summary> チュートリアルステージの場合はtrue。 </summary>
        public virtual bool IsTutorial => false;

        private readonly StageId _stageId;
        private readonly string _stageName;
        private readonly string _flavorText;
        private readonly StageReward _reward;
        private readonly string _targetSceneName;
    }
}
