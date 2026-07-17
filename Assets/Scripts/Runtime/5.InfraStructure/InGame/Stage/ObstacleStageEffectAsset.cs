using KillChord.Runtime.Domain.InGame.Stage;
using System;

namespace KillChord.Runtime.InfraStructure.InGame.Stage
{
    /// <summary>
    ///     障害物生成ステージ演出を入力するAssetです。
    /// </summary>
    [Serializable]
    public sealed class ObstacleStageEffectAsset : StageEffectAssetBase
    {
        /// <summary>
        ///     障害物生成ステージ演出定義を生成します。
        /// </summary>
        /// <returns> 生成した定義です。 </returns>
        public override IStageEffectDefinition Create()
        {
            return CreateDefinition(StageEffectKind.Obstacle);
        }

        /// <summary>
        ///     設定内容のサマリーを生成します。
        /// </summary>
        /// <returns> サマリーです。 </returns>
        protected override string BuildSummary()
        {
            return $"障害物演出: {EffectId}";
        }
    }
}
