using KillChord.Runtime.Application.OutGame.SkillBuild;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Constant;
using SymphonyFrameWork.System.SaveSystem;
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
        private const int EMPTY_SKILL_ID = -1;

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
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            BuildEquippedSkills(saveData.SkillBuild.EquipmentSkillIDs);
            return _equippedSkills.AsReadOnly();
        }

        /// <summary>
        ///     プレイヤーの装備スキル構成を保存する。
        /// </summary>
        /// <param name="equippedSkills"> 保存する装備スキル構成。 </param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task SaveSkillBuildAsync(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            if (equippedSkills == null)
            {
                throw new System.ArgumentNullException(nameof(equippedSkills));
            }

            List<int> skillIds = BuildSkillIds(equippedSkills);
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            List<int> previousSkillIds = new(saveData.SkillBuild.EquipmentSkillIDs);

            saveData.SkillBuild.SetEquipmentSkillIDs(skillIds);
            try
            {
                await SaveStore.SaveAsync<SaveData>();
            }
            catch
            {
                // SaveStore は同一インスタンスをキャッシュするため、
                // 書き込み失敗時はキャッシュ上の値も保存前へ戻す。
                saveData.SkillBuild.SetEquipmentSkillIDs(previousSkillIds);
                throw;
            }

            _equippedSkills = new List<EquippedSkill>(equippedSkills);
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
                if (skillId == EMPTY_SKILL_ID)
                {
                    _equippedSkills.Add(default);
                    continue;
                }

                if (!_skillRepository.TryGetSkill(new SkillId(skillId), out var skillData))
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"スキル ID '{skillId}' に対応する SkillTemplate が見つかりませんでした。");
#endif
                    _equippedSkills.Add(default);
                    continue;
                }

                _equippedSkills.Add(new EquippedSkill(skillData));
            }
        }

        /// <summary>
        ///     装備スキル構成から保存用スキル ID 一覧を構築する。
        /// </summary>
        /// <param name="equippedSkills"> 装備スキル構成。 </param>
        /// <returns> 保存用スキル ID 一覧。 </returns>
        private List<int> BuildSkillIds(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            List<int> result = new List<int>(equippedSkills.Count);
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                EquippedSkill equippedSkill = equippedSkills[i];
                result.Add(
                    equippedSkill.HasSkill
                        ? equippedSkill.SkillTemplate.Id.Value
                        : EMPTY_SKILL_ID);
            }

            return result;
        }
    }
}

