using KillChord.Runtime.Domain.InGame.Stage;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Stage
{
    /// <summary>
    ///     ステージ演出定義をViewModelへ伝達します。
    /// </summary>
    public sealed class StageEffectPresenter
    {
        /// <summary>
        ///     ステージ演出Presenterを生成します。
        /// </summary>
        /// <param name="viewModel"> 出力先ViewModelです。 </param>
        public StageEffectPresenter(IStageEffectViewModel viewModel)
        {
            _viewModel = viewModel
                ?? throw new ArgumentNullException(nameof(viewModel));
        }

        /// <summary>
        ///     ステージ演出をViewModelへ反映します。
        /// </summary>
        /// <param name="definition"> 演出定義です。 </param>
        public void Present(IStageEffectDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            _viewModel.Apply(
                definition.EffectId,
                ConvertKind(definition.Kind));
        }

        private readonly IStageEffectViewModel _viewModel;

        /// <summary>
        ///     Domainの演出種類をView用へ変換します。
        /// </summary>
        /// <param name="kind"> Domainの演出種類です。 </param>
        /// <returns> View用の演出種類です。 </returns>
        private static StageEffectViewKind ConvertKind(StageEffectKind kind)
        {
            return kind switch
            {
                StageEffectKind.Explosion => StageEffectViewKind.Explosion,
                StageEffectKind.BuildingCollapse => StageEffectViewKind.BuildingCollapse,
                StageEffectKind.Obstacle => StageEffectViewKind.Obstacle,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            };
        }
    }
}
