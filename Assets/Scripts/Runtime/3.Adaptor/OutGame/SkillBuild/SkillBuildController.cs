using KillChord.Runtime.Application.OutGame.SkillBuild;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の保存要求をユースケースへ伝えるコントローラー。
    /// </summary>
    public sealed class SkillBuildController : IDisposable
    {
        /// <summary>
        ///     コントローラーを初期化する。
        /// </summary>
        /// <param name="skillBuildUseCase"> スキル編成ユースケース。 </param>
        /// <param name="viewModel"> 監視対象 ViewModel。 </param>
        /// <param name="ownedSkills"> 入手済みスキル一覧。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public SkillBuildController(
            SkillBuildUseCase skillBuildUseCase,
            ISkillBuildViewModel viewModel,
            IReadOnlyList<SkillTemplate> ownedSkills)
        {
            _skillBuildUseCase = skillBuildUseCase ?? throw new ArgumentNullException(nameof(skillBuildUseCase));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            if (ownedSkills == null)
            {
                throw new ArgumentNullException(nameof(ownedSkills));
            }

            BuildSkillDataMap(ownedSkills);
            _viewModel.OnSaveRequested += HandleSaveRequestedHandler;
        }

        /// <summary>
        ///     リソースを解放する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _viewModel.OnSaveRequested -= HandleSaveRequestedHandler;
            _isDisposed = true;
        }

        /// <summary>
        ///    入手済みスキル一覧を更新する。
        /// </summary>
        /// <param name="ownedSkills"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UpdateOwnedSkills(IReadOnlyList<SkillTemplate> ownedSkills)
        {
            if (ownedSkills == null)
            {
                throw new ArgumentNullException(nameof(ownedSkills));
            }

            BuildSkillDataMap(ownedSkills);
        }

        private const int EMPTY_SKILL_ID = -1;

        private readonly SkillBuildUseCase _skillBuildUseCase;
        private readonly ISkillBuildViewModel _viewModel;
        private Dictionary<SkillId, SkillTemplate> _skillDataMap;
        private bool _isDisposed;

        /// <summary>
        ///     保存要求を受け取り、ユースケースへ反映する。
        /// </summary>
        /// <param name="skillIds"> 保存対象のスキル ID 一覧。 </param>
        private async Task<bool> HandleSaveRequestedHandler(ReadOnlyMemory<int> skillIds)
        {
            EquippedSkill[] equippedSkills = BuildEquippedSkills(skillIds.Span);
            _skillBuildUseCase.UpdateEquippedSkills(equippedSkills);

            try
            {
                await _skillBuildUseCase.SaveSkillBuildAsync(new List<int>(skillIds.ToArray()));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"スキルビルドの保存中にエラーが発生しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     スキル ID 一覧から装備スキル配列を構築する。
        /// </summary>
        /// <param name="skillIds"> スキル ID 一覧。 </param>
        /// <returns> 装備スキル配列。 </returns>
        /// <exception cref="InvalidOperationException"></exception>
        private EquippedSkill[] BuildEquippedSkills(ReadOnlySpan<int> skillIds)
        {
            EquippedSkill[] equippedSkills = new EquippedSkill[skillIds.Length];

            for (int i = 0; i < skillIds.Length; i++)
            {
                int skillId = skillIds[i];
                if (skillId == EMPTY_SKILL_ID)
                {
                    equippedSkills[i] = default;
                    continue;
                }

                if (!_skillDataMap.TryGetValue(new SkillId(skillId), out SkillTemplate skillData))
                {
                    throw new InvalidOperationException($"保存対象のスキル ID が入手済み一覧に存在しません。 skillId={skillId}");
                }

                equippedSkills[i] = new EquippedSkill(skillData);
            }

            return equippedSkills;
        }

        /// <summary>
        ///     スキル ID をキーにした辞書を構築する。
        /// </summary>
        /// <param name="ownedSkills"> 入手済みスキル一覧。 </param>
        /// <returns> スキル辞書。 </returns>
        /// <exception cref="ArgumentException"></exception>
        private void BuildSkillDataMap(IReadOnlyList<SkillTemplate> ownedSkills)
        {
            if (ownedSkills == null)
            {
                throw new ArgumentNullException(nameof(ownedSkills));
            }

            Dictionary<SkillId, SkillTemplate> skillDataMap = new Dictionary<SkillId, SkillTemplate>(ownedSkills.Count);

            for (int i = 0; i < ownedSkills.Count; i++)
            {
                SkillTemplate skillData = ownedSkills[i];
                if (skillData == null)
                {
                    throw new ArgumentException($"入手済みスキル一覧に null が存在します。 index={i}", nameof(ownedSkills));
                }

                if (skillDataMap.ContainsKey(skillData.Id))
                {
                    throw new ArgumentException($"重複したスキル ID が存在します。 skillId={skillData.Id}", nameof(ownedSkills));
                }

                skillDataMap.Add(skillData.Id, skillData);
            }

            _skillDataMap = skillDataMap;
        }
    }
}

