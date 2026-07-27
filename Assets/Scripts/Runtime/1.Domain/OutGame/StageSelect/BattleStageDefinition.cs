using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Mission;
using System;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     バトルステージ固有の定義情報を表すクラス。
    /// </summary>
    public sealed class BattleStageDefinition : StageDefinition
    {
        /// <summary>
        ///     バトルステージの定義情報を初期化する。
        /// </summary>
        /// <param name="stageId"> ステージのID。 </param>
        /// <param name="stageName"> ステージの名前。 </param>
        /// <param name="flavorText"> ステージのフレーバーテキスト。 </param>
        /// <param name="reward"> ステージの報酬情報。 </param>
        /// <param name="targetSceneName"> ステージのターゲットシーン名。 </param>
        /// <param name="battleSceneName"> バトルパートのシーン名。 </param>
        /// <param name="missionDefinition"> ミッション定義。 </param>
        /// <param name="isTutorial"> チュートリアルステージの場合はtrue。 </param>
        /// <param name="enemyWaveDefinitionId"> 敵Wave定義ID。 </param>
        public BattleStageDefinition(
            StageId stageId,
            string stageName,
            string flavorText,
            StageReward reward,
            string targetSceneName,
            string battleSceneName,
            MissionDefinition missionDefinition,
            bool isTutorial,
            EnemyWaveDefinitionId enemyWaveDefinitionId)
            : base(stageId, stageName, flavorText, reward, targetSceneName)
        {
            if (string.IsNullOrWhiteSpace(battleSceneName))
            {
                throw new ArgumentException("Battle scene name must not be empty.", nameof(battleSceneName));
            }

            if (missionDefinition == null)
            {
                throw new ArgumentNullException(nameof(missionDefinition));
            }

            if (enemyWaveDefinitionId.Value == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(enemyWaveDefinitionId),
                    "Enemy wave definition ID must not be zero.");
            }

            _battleSceneName = battleSceneName;
            _missionDefinition = missionDefinition;
            _isTutorial = isTutorial;
            _enemyWaveDefinitionId = enemyWaveDefinitionId;
        }

        /// <summary> ステージの種類。 </summary>
        public override StageType StageType => StageType.Battle;
        /// <summary> バトルパートのシーン名。 </summary>
        public string BattleSceneName => _battleSceneName;
        /// <summary> ステージのミッション定義。 </summary>
        public MissionDefinition MissionDefinition => _missionDefinition;
        /// <summary> チュートリアルステージの場合はtrue。 </summary>
        public override bool IsTutorial => _isTutorial;
        /// <summary> 敵Wave定義ID。 </summary>
        public EnemyWaveDefinitionId EnemyWaveDefinitionId => _enemyWaveDefinitionId;

        private readonly string _battleSceneName;
        private readonly MissionDefinition _missionDefinition;
        private readonly bool _isTutorial;
        private readonly EnemyWaveDefinitionId _enemyWaveDefinitionId;
    }
}
