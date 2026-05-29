using System;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Player;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Skill;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.Player
{
    /// <summary>
    ///     スキルの設定データを保持するドメインクラス。
    /// </summary>
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