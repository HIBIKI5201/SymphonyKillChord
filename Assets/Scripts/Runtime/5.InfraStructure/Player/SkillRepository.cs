using System;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using SymphonyKillChord.InfraStructure.Player;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    /// <summary>
    ///     スキルのデータを保持し、提供するためのリポジトリクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillRepository", menuName = "Scriptable Objects/SkillRepository")]
    public class SkillRepository : ScriptableObject, ISkillRepository
    {
        public SkillDefinition GetSkill(int id)
        {
            SkillDataAsset asset = Array.Find(_skillDataAssets, x => x.Id == id);
            return asset.ToDomain().ToSkillDefinition();
        }

        [SerializeField] private SkillDataAsset[] _skillDataAssets;
    }
}