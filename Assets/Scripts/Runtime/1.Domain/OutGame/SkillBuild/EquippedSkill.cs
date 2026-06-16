using KillChord.Runtime.Domain.OutGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillBuild
{
    /// <summary>
    ///     所持しているスキルを表す値型オブジェクト。
    /// </summary>
    public readonly struct EquippedSkill : IEquatable<EquippedSkill>
    {
        public EquippedSkill(SkillData skillData)
        {
            _skillData = skillData;
        }

        /// <summary> スキルのデータを取得するプロパティ。 </summary>
        public SkillData SkillData => _skillData;

        /// <summary>
        ///     等値比較を行う。
        /// </summary>
        /// <param name="other"> 比較対象の EquippedSkill オブジェクト。 </param>
        /// <returns> 等しい場合は true、それ以外の場合は false を返す。 </returns>
        public bool Equals(EquippedSkill other)
        {
            return _skillData.Equals(other._skillData);
        }

        /// <summary>
        ///     オブジェクトの等値比較を行う。
        /// </summary>
        /// <param name="obj"> 比較対象のオブジェクト。 </param>
        /// <returns> 等しい場合は true、それ以外の場合は false を返す。 </returns>
        public override bool Equals(object obj)
        {
            return obj is EquippedSkill other && Equals(other);
        }

        /// <summary>
        ///    ハッシュコードを取得する。
        /// </summary>
        public override int GetHashCode()
        {
            return _skillData.GetHashCode();
        }

        private readonly SkillData _skillData;
    }
}
