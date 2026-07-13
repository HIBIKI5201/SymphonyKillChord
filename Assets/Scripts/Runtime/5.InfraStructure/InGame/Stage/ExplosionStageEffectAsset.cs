using KillChord.Runtime.Domain.InGame.Stage;
using System;

namespace KillChord.Runtime.InfraStructure.InGame.Stage
{
    /// <summary>
    ///     爆発ステージ演出を入力するAssetです。
    /// </summary>
    [Serializable]
    public sealed class ExplosionStageEffectAsset : StageEffectAssetBase
    {
        /// <summary>
        ///     爆発ステージ演出定義を生成します。
        /// </summary>
        /// <returns> 生成した定義です。 </returns>
        public override IStageEffectDefinition Create()
        {
            return CreateDefinition(StageEffectKind.Explosion);
        }

        /// <summary>
        ///     設定内容のサマリーを生成します。
        /// </summary>
        /// <returns> サマリーです。 </returns>
        protected override string BuildSummary()
        {
            return $"爆発演出: {EffectId}";
        }
    }
}
