using KillChord.Runtime.Domain.InGame.Stage;
using System;

namespace KillChord.Runtime.InfraStructure.InGame.Stage
{
    /// <summary>
    ///     建物倒壊ステージ演出を入力するAssetです。
    /// </summary>
    [Serializable]
    public sealed class BuildingCollapseStageEffectAsset : StageEffectAssetBase
    {
        /// <summary>
        ///     建物倒壊ステージ演出定義を生成します。
        /// </summary>
        /// <returns> 生成した定義です。 </returns>
        public override IStageEffectDefinition Create()
        {
            return CreateDefinition(StageEffectKind.BuildingCollapse);
        }

        /// <summary>
        ///     設定内容のサマリーを生成します。
        /// </summary>
        /// <returns> サマリーです。 </returns>
        protected override string BuildSummary()
        {
            return $"建物倒壊演出: {EffectId}";
        }
    }
}
