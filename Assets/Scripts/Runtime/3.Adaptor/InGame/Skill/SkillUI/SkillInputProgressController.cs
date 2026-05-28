using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力の進捗を管理するコントローラクラス。
    /// </summary>
    public class SkillInputProgressController
    {
        /// <summary>
        ///     スキル入力の進捗を管理するコントローラクラスを生成する。
        /// </summary>
        /// <param name="usecase"> スキル入力進捗の計算を行うユースケース。 </param>
        /// <param name="state"> スキル入力進捗の状態を管理するオブジェクト。 </param>
        /// <param name="presenter"> スキル入力進捗の表示を担当するプレゼンター。 </param>
        public SkillInputProgressController(
            SkillInputProgressUsecase usecase,
            SkillInputProgressState state,
            SkillInputProgressPresenter presenter)
        {
            _usecase = usecase ?? throw new ArgumentNullException(nameof(usecase));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        /// <summary>
        ///     入力されたビートに基づいて、スキルの入力進捗を更新する。
        /// </summary>
        /// <param name="skills"> 更新対象のスキル定義のリスト。 </param>
        /// <param name="inputBeatType"> 入力されたビートの種類。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UpdateProgress(IReadOnlyList<SkillDefinition> skills, BeatType inputBeatType)
        {
            if (skills == null)
            {
                throw new ArgumentNullException(nameof(skills));
            }

            _presenter.BeginPresent();

            try
            {
                for (int i = 0; i < skills.Count; i++)
                {
                    SkillDefinition skill = skills[i];

                    if (skill == null)
                    {
                        continue;
                    }

                    int skillId = skill.Id.Value;
                    int currentMatchedCount = _state.GetMatchedCount(skillId);

                    // スキルごとに、今回の入力でどれだけパターンがマッチしたかを計算する。
                    int nextMatchedCount = _usecase.CalculateNextMatchedCount(
                        skill.SkillPattern.Signatures,
                        currentMatchedCount,
                        inputBeatType);

                    _state.SetMatchedCount(skillId, nextMatchedCount);
                    _presenter.Push(skill, nextMatchedCount);
                }
            }
            finally
            {
                _presenter.EndPresent();
            }
        }

        /// <summary>
        ///     指定したスキルの入力進捗をリセットする。
        /// </summary>
        /// <param name="skillId"> リセット対象のスキルID。 </param>
        public void ResetSkill(int skillId)
        {
            _state.Reset(skillId);
        }

        private readonly SkillInputProgressUsecase _usecase;
        private readonly SkillInputProgressState _state;
        private readonly SkillInputProgressPresenter _presenter;
    }
}
