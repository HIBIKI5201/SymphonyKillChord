using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     すべての敵Waveの撃破を要求するクリア条件のアセットです。
    /// </summary>
    [Serializable]
    public sealed class AllEnemyWavesClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <summary>
        ///     すべての敵Waveの撃破を要求する条件を生成します。
        /// </summary>
        public override IMissionClearCondition Create(EnemyMissionKeyRepository missionKeyRepository)
        {
            return new AllEnemyWavesClearCondition();
        }

        /// <summary>
        ///     インスペクターに表示する条件の説明を構築します。
        /// </summary>
        protected override string BuildSummary()
        {
            return "最終Waveまでのすべての敵を撃破する条件";
        }
    }
}
