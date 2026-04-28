using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     スキルの結果をViewModel向けDTOへ変換して渡すプレゼンタークラス。
    /// </summary>
    public class SkillResultPresenter
    {
        public SkillResultPresenter(ISkillResultViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        ///     スキル結果をViewModel向けDTOへ変換して渡すメソッド。
        /// </summary>
        /// <param name="result"></param>
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
