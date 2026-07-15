using System;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     解放されるスキルの ID を表す値オブジェクト。
    /// </summary>
    public readonly struct UnlockSkillId : IEquatable<UnlockSkillId>
    {
        /// <summary>
        ///     解放されるスキルの ID を指定して初期化する。
        /// </summary>
        /// <param name="skillId"></param>
        public UnlockSkillId(int skillId)
        {
            _skillId = skillId;
        }

        /// <summary>
        ///    解放されるスキルの ID の値を取得する。
        /// </summary>
        public int Value => _skillId;

        /// <summary>
        ///     指定された <see cref="UnlockSkillId" /> と等しいかどうかを判断する。
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(UnlockSkillId other)
        {
            return _skillId == other._skillId;
        }

        /// <summary>
        ///     指定されたオブジェクトと等しいかどうかを判断する。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is UnlockSkillId other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得する。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _skillId.GetHashCode();
        }

        private readonly int _skillId;
    }
}
