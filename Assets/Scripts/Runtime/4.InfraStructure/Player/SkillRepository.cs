using System;
using System.Collections.Generic;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    [CreateAssetMenu(fileName = "SkillRepository", menuName = "Scriptable Objects/SkillRepository")]
    public class SkillRepository : ScriptableObject, ISkillRepository
    {
        [SerializeField] private SkillData[] _skillDatas;
        
        private List<SkillDefinition> ConvertSkillDefinitions()
        {
            var skills = new List<SkillDefinition>();
            foreach (var skillData in _skillDatas)
            {
                skills.Add(skillData.ToSkillDefinition());
            }

            return skills;
        }

        public SkillDefinition GetSkill(int id)
        {
            return Array.Find(_skillDatas, x => x.Id == id).ToSkillDefinition();
        }
    }
}