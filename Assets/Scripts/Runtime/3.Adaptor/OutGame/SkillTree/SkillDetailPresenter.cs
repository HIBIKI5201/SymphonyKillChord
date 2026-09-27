using KillChord.Runtime.Domain.OutGame.SkillTree;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキル詳細画面にデータを反映するクラス。
    /// </summary>
    public class SkillDetailPresenter
    {
        /// <summary>
        ///     スキル詳細の ViewModel を指定して生成する。
        /// </summary>
        public SkillDetailPresenter(ISkillDetailViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        ///     スキル詳細を ViewModel に渡して表示を更新する。
        /// </summary>
        public void Push(SkillDetailDTO dto)
        {
            _viewModel.Apply(dto);
        }

        private ISkillDetailViewModel _viewModel;
    }
}
