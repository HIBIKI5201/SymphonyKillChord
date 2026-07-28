using KillChord.Runtime.Domain.OutGame.SkillTree;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキル詳細画面にデータを反映するクラス。
    /// </summary>
    public class SkillDetailPresenter
    {
        public SkillDetailPresenter(ISkillDetailViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public void Push(SkillDetailDTO dto)
        {
            _viewModel.Apply(dto);
        }

        private ISkillDetailViewModel _viewModel;
    }
}
