using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    /// <summary>
    ///     スキルのデータを保持し、提供するためのリポジトリクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillRepository", menuName = "Scriptable Objects/SkillRepository")]
    public class SkillRepository : ScriptableObjectRepositoryBase<SkillId, SkillTemplate, SkillTemplateAsset>, ISkillRepository
    {
        /// <summary>
        ///     指定されたスキル ID に対応する SkillTemplateAsset を取得しようとします。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public bool TryGetSkill(SkillId id, out SkillTemplate skillData)
        {
#if UNITY_EDITOR
            InvalidateCache();
#endif

            return TryFind(id, out skillData);
        }

        /// <summary>
        ///     登録されている全てのスキルデータを取得します。
        /// </summary>
        /// <returns> 全スキルデータ。 </returns>
        public IReadOnlyCollection<SkillTemplate> GetAllSkills()
        {
#if UNITY_EDITOR
            InvalidateCache();
#endif
            return GetAllValues();
        }

        public SkillDefinition GetSkill(SkillId id, double bpm)
        {
#if UNITY_EDITOR
            InvalidateCache();
#endif
            if (!TryFind(id, out SkillTemplate skillData))
            {
                throw new KeyNotFoundException($"指定されたスキルID {id} に対応するスキルデータが見つかりませんでした。");
            }
            return skillData.ToSkillDefinition(bpm);
        }

        [SerializeField] private SkillTemplateAsset[] _skillDataAssets;

        protected override IReadOnlyList<SkillTemplateAsset> GetEntries() => _skillDataAssets;

        protected override bool TryBuild(SkillTemplateAsset entry, out SkillId id, out SkillTemplate value)
        {
            id = entry.Id;
            value = entry.ToDomain();
            return true;
        }
    }
}

