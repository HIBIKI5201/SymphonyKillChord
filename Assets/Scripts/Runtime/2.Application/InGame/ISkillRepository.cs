using System.Collections.Generic;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface ISkillRepository
    {
        IReadOnlyList<SkillDefinition> GetEquipmentSkills();
    }
}