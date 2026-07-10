using KillChord.Runtime.Application.OutGame.SkillBuild;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillBuild
{
    /// <summary>
    ///     プレイヤーの装備スキルに関するデータの永続化や取得を担当するリポジトリの実装クラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(SkillBuildRepository),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "SkillBuild/" + nameof(SkillBuildRepository))]
    public class SkillBuildRepository : ScriptableObject, ISkillBuildRepository
    {
        /// <summary>
        ///     プレイヤーの装備スキルのリストを非同期で取得する。
        /// </summary>
        /// <returns></returns>
        public async ValueTask<IReadOnlyList<EquippedSkill>> GetEquippedSkills()
        {
            if (_equippedSkills == null)
            {
                await LoadSkillBuild();
            }

            return _equippedSkills.AsReadOnly();
        }

        /// <summary>
        ///     プレイヤーの装備スキルのリストを非同期でロードする。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public async ValueTask<IReadOnlyList<EquippedSkill>> LoadSkillBuild()
        {
            ValidateDependencies();
            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                throw new System.InvalidOperationException("SavedataSystem が ServiceLocator に登録されていません。");
            }

            SaveData saveData = await savedataSystem.LoadAsync<SaveData>();
            BuildEquippedSkills(saveData.SkillBuild.EquipmentSkillIDs);
            return _equippedSkills.AsReadOnly();
        }

        [SerializeField, Tooltip("スキル ID から SkillTemplate を取得するリポジトリ。")]
        private SkillRepository _skillRepository;

        private List<EquippedSkill> _equippedSkills;

        /// <summary>
        ///    依存関係が設定されているか確認する。
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        private void ValidateDependencies()
        {
            if (_skillRepository == null)
            {
                throw new System.InvalidOperationException("SkillRepository が設定されていません。");
            }
        }

        /// <summary>
        ///   指定されたスキル ID のリストから EquippedSkill のリストを構築する。
        /// </summary>
        /// <param name="skillIds"></param>
        private void BuildEquippedSkills(IReadOnlyList<int> skillIds)
        {
            ValidateDependencies();
            _equippedSkills ??= new List<EquippedSkill>();
            _equippedSkills.Clear();
            for (int i = 0; i < skillIds.Count; i++)
            {
                int skillId = skillIds[i];
                if (!_skillRepository.TryGetSkill(skillId, out var skillData))
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"スキル ID '{skillId}' に対応する SkillTemplate が見つかりませんでした。");
#endif
                    continue;
                }

                _equippedSkills.Add(new EquippedSkill(skillData));
            }
        }
    }
}

