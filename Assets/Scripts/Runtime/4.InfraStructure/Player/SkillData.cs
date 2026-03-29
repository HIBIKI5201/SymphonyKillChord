using System;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    [Serializable]
    public class SkillData
    {
        public int Id => _id;
        public int[] Pattern => _pattern;

        [SerializeField] private int _id;
        [SerializeField] private int[] _pattern;

        public SkillDefinition ToSkillDefinition()
        {
            return new SkillDefinition(
                new SkillId(_id),
                new SkillPattern(_pattern));
        }
    }
}