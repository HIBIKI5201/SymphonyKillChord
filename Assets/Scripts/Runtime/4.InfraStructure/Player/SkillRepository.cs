using System;
using System.Collections.Generic;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    [CreateAssetMenu(fileName = "SkillRepository", menuName = "Scriptable Objects/SkillRepository")]
    public class SkillRepository : ScriptableObject, ISkillRepository
    {
        [SerializeField] private SkillData[] _skillDatas;

        //TODO　後々セーブデータから装備済みスキルを取得する形に変更する
        public IReadOnlyList<SkillDefinition> GetEquipmentSkills()
        {
            return ConvertSkillDefinitions();
        }

        private List<SkillDefinition> ConvertSkillDefinitions()
        {
            var skills = new List<SkillDefinition>();
            foreach (var skillData in _skillDatas)
            {
                skills.Add(skillData.ToSkillDefinition());
            }

            return skills;
        }
    }
}