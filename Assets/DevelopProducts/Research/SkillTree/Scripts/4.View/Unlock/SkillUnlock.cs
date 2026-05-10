using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [System.Serializable]
    public class SkillUnlock : ISkillUnlockEffect
    {
        public string Description => _description;

        [SerializeField] private int _skillId;
        [SerializeField] private string _description;
    }
}