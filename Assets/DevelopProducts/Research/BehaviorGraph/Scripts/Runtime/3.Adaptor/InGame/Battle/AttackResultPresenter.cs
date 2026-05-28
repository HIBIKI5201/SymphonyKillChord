using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     攻撃結果をViewModel向けDTOへ変換して渡すプレゼンタークラス。
    /// </summary>
    public class AttackResultPresenter
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="viewModel"></param>
        public AttackResultPresenter(IAttackResultViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        ///     攻撃結果をViewModel向けDTOへ変換して渡すメソッド。
        /// </summary>
        /// <param name="result"></param>
        public void Push(AttackResult result)
        {
            AttackResultDTO dto = new AttackResultDTO(
                result.FinalDamage.Value,
                result.IsCritical
                );

            _viewModel.Push(in dto);
        }

        private readonly IAttackResultViewModel _viewModel;
    }
}
