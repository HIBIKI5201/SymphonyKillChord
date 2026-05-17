using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行状態をViewModelに反映するプレゼンタークラス。
    /// </summary>
    public class SkillInputProgressPresenter
    {
        /// <summary>
        ///     スキル入力進行状態をViewModelに反映するプレゼンタークラスを生成する。
        /// </summary>
        /// <param name="viewModel"> スキル入力進行の表示を仲介するViewModel。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillInputProgressPresenter(ISkillInputProgressViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
        
        /// <summary>
        ///     表示更新を開始する。
        /// </summary>
        public void BeginPresent()
        {
            _viewModel.Clear();
        }

        /// <summary>
        ///     スキル1件分の入力進行状態をViewModelに反映する。
        /// </summary>
        /// <param name="skill"> スキル定義。 </param>
        /// <param name="matchedCount"> 現在の一致数。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Push(SkillDefinition skill, int matchedCount)
        {
            if (skill == null)
            {
                throw new ArgumentNullException(nameof(skill));
            }

            ReadOnlySpan<BeatType> skillPattern = skill.SkillPattern.Signatures;
            Span<SkillInputProgressStepDTO> steps = stackalloc SkillInputProgressStepDTO[skillPattern.Length];

            for (int i = 0; i < skillPattern.Length; i++)
            {
                // matchedCountより前のステップはアクティブ、以降のステップは非アクティブとする
                steps[i] = new SkillInputProgressStepDTO(
                    (int)skillPattern[i],
                    i < matchedCount);
            }

            SkillInputProgressRowDTO dto = new SkillInputProgressRowDTO(skill.Id.Value, steps);
            _viewModel.Apply(in dto);
        }

        /// <summary>
        ///     全体の表示更新を確定する。
        /// </summary>
        public void EndPresent()
        {
            _viewModel.Commit();
        }

        private readonly ISkillInputProgressViewModel _viewModel;
    }
}
