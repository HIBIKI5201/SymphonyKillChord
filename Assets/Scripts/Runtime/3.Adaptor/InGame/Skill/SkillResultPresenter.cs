using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// プレゼンター：ドメインのSkillDefinitionをViewModel用DTOへ変換して通知する責務を持つ。
    /// </summary>
    public class SkillResultPresenter
    {
        /// <summary>
        /// コンストラクタ。出力先のViewModelを受け取る。
        /// </summary>
        public SkillResultPresenter(ISkillResultViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// ドメインのSkillDefinitionをSkillResultDTOに変換してViewModelに渡す。
        /// </summary>
        public void Push(SkillDefinition result)
        {
            var span = result.SkillPattern.Signatures;
            int[] arr = new int[span.Length];
            for (int i = 0; i < span.Length; i++) 
                arr[i] = (int)span[i];
            var dto = new SkillResultDTO(result.Id.Value, arr); 
            _viewModel.Push(in dto);
        }

        private readonly ISkillResultViewModel _viewModel;
    }
}
