using KillChord.Runtime.Application.OutGame.SkillBuild;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Constant;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillBuild
{
    /// <summary>
    ///     デバッグ用の SkillBuildRepository の実装クラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(SkillBuildRepositoryDebug),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "SkillBuild/" + nameof(SkillBuildRepositoryDebug))]
    public class SkillBuildRepositoryDebug : ScriptableObject, ISkillBuildRepository
    {
        public ValueTask<IReadOnlyList<EquippedSkill>> GetEquippedSkills()
        {
            RebuildEquippedSkills();
            return new ValueTask<IReadOnlyList<EquippedSkill>>(_equippedSkills.AsReadOnly());
        }

        public ValueTask<IReadOnlyList<EquippedSkill>> LoadSkillBuild()
        {
            if (_skillDataAssets == null || _skillDataAssets.Count == 0)
            {
                throw new ArgumentNullException(nameof(_skillDataAssets), "スキルデータが存在しません。");
            }

            RebuildEquippedSkills();
            _skillBuildDefinition = new SkillBuildDefinition(_equippedSkills.ToArray());
            return new ValueTask<IReadOnlyList<EquippedSkill>>(_equippedSkills.AsReadOnly());
        }

        public void SaveSkillBuild(SkillBuildDefinition skillBuildDefinition)
        {
            // セーブシステムの本実装が行われていないため、ここでは何もしない。
        }

        /// <summary>
        ///     [SerializeField] のスキルデータアセットから EquippedSkill のリストを再構築する。
        /// </summary>
        private void RebuildEquippedSkills()
        {
            _equippedSkills ??= new List<EquippedSkill>();
            _equippedSkills.Clear();

            if (_skillDataAssets == null)
            {
                return;
            }

            for (int i = 0; i < _skillDataAssets.Count; i++)
            {
                if (_skillDataAssets[i] == null)
                {
                    continue;
                }

                _equippedSkills.Add(new EquippedSkill(_skillDataAssets[i].ToDomain()));
            }
        }

        [Header("仮のスキル編成")]
        [SerializeField]
        private List<SkillDataAsset> _skillDataAssets = new List<SkillDataAsset>();

        private List<EquippedSkill> _equippedSkills = new List<EquippedSkill>();
        private SkillBuildDefinition _skillBuildDefinition;
    }
}
