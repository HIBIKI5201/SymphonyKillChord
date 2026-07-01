using KillChord.Runtime.Application;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Constant;
using System.Collections.Generic;
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
        public IReadOnlyList<EquippedSkill> GetOwnedSkills()
        {
            RebuildEquippedSkills();
            return _equippedSkills.AsReadOnly();
        }

        public void LoadOwnedSkills()
        {
            if (_skillDataAssets == null || _skillDataAssets.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(_skillDataAssets), "スキルデータが存在しません。");
            }

            RebuildEquippedSkills();
        }

        public void SaveOwnedSkills(IReadOnlyList<EquippedSkill> ownedSkills)
        {
            // デバッグ用のリポジトリではセーブは行わないため、何もしない
        }

        [Header("仮の入手済みスキル")]
        [SerializeField]
        private List<SkillDataAsset> _skillDataAssets;

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
