using KillChord.Runtime.InfraStructure.Player;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [System.Serializable]
    public class SkillUnlock : ISkillUnlockEffect
    {
        public int GetSkill()
        {
            return _skillData.Id.Value;
        }
        [SerializeField, Tooltip("スキルのデータ")] private SkillTemplateAsset _skillData;
    }
}
