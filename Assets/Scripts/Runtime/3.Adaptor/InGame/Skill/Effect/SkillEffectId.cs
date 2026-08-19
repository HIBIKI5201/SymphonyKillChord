using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクト定義を一意に識別するIDを表す構造体。
    /// </summary>
    public readonly struct SkillEffectId : IEquatable<SkillEffectId>
    {
        /// <summary>
        ///     ハッシュ値からIDを生成する。
        /// </summary>
        /// <param name="value"> IDのハッシュ値です。 </param>
        public SkillEffectId(int value)
        {
            _value = value;
        }

        /// <summary> IDのハッシュ値です。 </summary>
        public int Value => _value;

        /// <summary> 有効なIDかどうかです。 </summary>
        public bool IsValid => _value != 0;

        public static bool operator ==(SkillEffectId left, SkillEffectId right) => left.Equals(right);
        public static bool operator !=(SkillEffectId left, SkillEffectId right) => !left.Equals(right);

        /// <summary>
        ///     文字列キーから決定的なIDを生成する。
        /// </summary>
        /// <param name="key"> エフェクトの文字列キーです。 </param>
        /// <returns> 生成したIDです。空文字の場合は無効なIDを返します。 </returns>
        public static SkillEffectId FromKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default;
            }

            // FNV-1a 32bitで文字列から決定的なハッシュを生成する。
            unchecked
            {
                uint hash = FNV_OFFSET_BASIS;
                for (int i = 0; i < key.Length; i++)
                {
                    hash ^= key[i];
                    hash *= FNV_PRIME;
                }

                // ハッシュが0になった場合は無効IDと衝突するため補正する。
                int value = (int)hash;
                return new SkillEffectId(value == 0 ? 1 : value);
            }
        }

        /// <summary> 別のSkillEffectIdと等価か判定する。 </summary>
        public bool Equals(SkillEffectId other) => _value == other._value;

        /// <summary> オブジェクト等価性の判定を行う。 </summary>
        public override bool Equals(object obj) => obj is SkillEffectId other && Equals(other);

        /// <summary> ハッシュコードを取得する。 </summary>
        public override int GetHashCode() => _value;

        /// <summary> 数値IDの文字列表現を取得する。 </summary>
        /// <returns> 数値IDの文字列表現です。 </returns>
        public override string ToString() => _value.ToString();

        private const uint FNV_OFFSET_BASIS = 2166136261;
        private const uint FNV_PRIME = 16777619;

        private readonly int _value;
    }
}
