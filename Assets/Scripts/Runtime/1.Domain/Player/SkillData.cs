using System;
using KillChord.Runtime.Domain;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Runtime.Domain.Player
{
    [Serializable]
    public class SkillData
    {
        public int Id => _id;
        public int[] Pattern => _pattern;

        [SerializeReference, SubclassSelector] ISkillEffect _skillEffect;
        [SerializeReference, SubclassSelector] ISkillVisual _skillVisual;

        [SerializeField] private int _id;
        [SerializeField] private int[] _pattern;

        public SkillDefinition ToSkillDefinition()
        {
            return new SkillDefinition(
                new SkillId(_id),
                new SkillPattern(_pattern),
                _skillEffect,
                _skillVisual);
        }
    }
}