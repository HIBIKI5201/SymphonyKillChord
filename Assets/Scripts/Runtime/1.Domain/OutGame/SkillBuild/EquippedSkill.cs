using KillChord.Runtime.Domain.Player;
using System;

namespace KillChord.Runtime.Domain.OutGame.SkillBuild
{
    /// <summary>
    ///     所持しているスキルを表す値型オブジェクト。
    /// </summary>
    public readonly struct EquippedSkill : IEquatable<EquippedSkill>
    {
        /// <summary>
        ///     EquippedSkill を初期化する。
        /// </summary>
        /// <param name="skillData"> 装備するスキルデータ。 </param>
        public EquippedSkill(SkillTemplate skillData)
        {
            _skillData = skillData;
        }

        /// <summary> スキルが設定されているかどうかを取得する。 </summary>
        public bool HasSkill => _skillData != null;

        /// <summary> スキルのデータを取得するプロパティ。 </summary>
        public SkillTemplate SkillTemplate => _skillData;

        /// <summary>
        ///     等値比較を行う。
        /// </summary>
        /// <param name="other"> 比較対象の EquippedSkill オブジェクト。 </param>
        /// <returns> 等しい場合は true、それ以外の場合は false を返す。 </returns>
        public bool Equals(EquippedSkill other)
        {
            return Equals(_skillData, other._skillData);
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
        ///     ハッシュコードを取得する。
        /// </summary>
        public override int GetHashCode()
        {
            return _skillData != null ? _skillData.GetHashCode() : 0;
        }

        private readonly SkillTemplate _skillData;
    }
}

