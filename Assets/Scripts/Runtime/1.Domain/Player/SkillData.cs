using System;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///     スキルの設定データを保持するドメインクラス。
    /// </summary>
    [Serializable]
    public class SkillData
    {
        public int Id => _id;
        public BeatType[] Pattern => _pattern;

        [SerializeReference, SubclassSelector] ISkillEffect _skillEffect;
        [SerializeReference, SubclassSelector] ISkillVisual _skillVisual;

        [SerializeField] private int _id;
        [SerializeField] private BeatType[] _pattern;

        public SkillDefinition ToSkillDefinition()
        {
            return new SkillDefinition(
                new SkillId(_id),
                new SkillPattern(new(_pattern)),
                _skillEffect,
                _skillVisual);
        }
    }
}