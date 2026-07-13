using KillChord.Runtime.Application;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///    デバッグ用の入手済みスキルリポジトリ実装クラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(OwnedSkillRepositoryDebug), 
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "SkillBuild/" 
        + nameof(OwnedSkillRepositoryDebug))]
    public class OwnedSkillRepositoryDebug : ScriptableObject, IOwnedSkillRepository
    {
        public ValueTask<IReadOnlyList<EquippedSkill>> GetOwnedSkills()
        {
            RebuildEquippedSkills();
            return new ValueTask<IReadOnlyList<EquippedSkill>>(_equippedSkills.AsReadOnly());
        }

        public ValueTask<IReadOnlyList<EquippedSkill>> LoadOwnedSkillsAsync()
        {
            if (_skillDataAssets == null || _skillDataAssets.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(_skillDataAssets), "スキルデータが存在しません。");
            }

            RebuildEquippedSkills();
            return new ValueTask<IReadOnlyList<EquippedSkill>>(_equippedSkills.AsReadOnly());
        }

        [Header("仮の入手済みスキル")]
        [SerializeField]
        private List<SkillTemplateAsset> _skillDataAssets;

        private List<EquippedSkill> _equippedSkills;

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
    }
}

