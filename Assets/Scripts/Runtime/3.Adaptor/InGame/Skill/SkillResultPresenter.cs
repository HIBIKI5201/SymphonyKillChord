using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Adaptor
{
    public class SkillResultPresenter
    {
        public SkillResultPresenter(ISkillResultViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Push(SkillDefinition result)
        {
            SkillResultDTO dto = new SkillResultDTO(
                result.Id.Value,
                result.SkillPattern.Signatures.ToArray()
                );
            _viewModel.Push(in dto);
        }

        private readonly ISkillResultViewModel _viewModel;
    }
}
