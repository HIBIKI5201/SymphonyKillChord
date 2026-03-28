using System.Collections.Generic;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillRepository", menuName = "Scriptable Objects/SkillRepository")]
public class SkillRepository : ScriptableObject, ISkillRepository
{
    
    //TODO　後々セーブデータから装備済みスキルを取得する形に変更する
    public IReadOnlyList<SkillDefinition> GetEquipmentSkills()
    {
        
    }
}
