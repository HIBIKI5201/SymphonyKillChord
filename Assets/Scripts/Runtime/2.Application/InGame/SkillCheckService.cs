using System;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public class SkillCheckService
    {
        private ISkillRepository _repository;

        public SkillCheckService(ISkillRepository repository)
        {
            _repository = repository;
        }

        public bool TryCheckSkills(ReadOnlySpan<int> history, out SkillId skillId)
        {
            var skills = _repository.GetEquipmentSkills();

            foreach (var skillDefinition in skills)
            {
                if (skillDefinition.IsMatch(history))
                {
                    skillId = skillDefinition.Id;
                    return true;
                }
            }

            skillId = default;
            return false;
        }
    }
}