using System;
using System.Collections.Generic;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    /// <summary>
    ///     スキルのデータを保持し、提供するためのリポジトリクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillRepository", menuName = "Scriptable Objects/SkillRepository")]
    public class SkillRepository : ScriptableObject, ISkillRepository
    {
        [SerializeField] private SkillData[] _skillDatas;
        
        public SkillDefinition GetSkill(int id)
        {
            return Array.Find(_skillDatas, x => x.Id == id).ToSkillDefinition();
        }
    }
}