using System;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキル効果で使用する数値と表示形式を保持する値オブジェクトです。
    /// </summary>
    public readonly struct SkillEffectParameter : IEquatable<SkillEffectParameter>
    {
        /// <summary>
        ///     スキル効果パラメータを初期化します。
        /// </summary>
        /// <param name="id"> パラメータ識別子です。 </param>
        /// <param name="value"> ゲーム処理で使用する値です。 </param>
        /// <param name="displayFormat"> 画面表示時の形式です。 </param>
        public SkillEffectParameter(
            SkillEffectParameterId id,
            double value,
            SkillEffectValueFormat displayFormat)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "スキル効果値には有限値を指定してください。");
            }

            Id = id;
            Value = value;
            DisplayFormat = displayFormat;
        }

        /// <summary> パラメータ識別子です。 </summary>
        public SkillEffectParameterId Id { get; }

        /// <summary> ゲーム処理で使用する値です。 </summary>
        public double Value { get; }

        /// <summary> 画面表示時の形式です。 </summary>
        public SkillEffectValueFormat DisplayFormat { get; }

        /// <summary>
        ///     値が等しいかを判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public bool Equals(SkillEffectParameter other)
        {
            return Id == other.Id &&
                   Value.Equals(other.Value) &&
                   DisplayFormat == other.DisplayFormat;
        }

        /// <summary>
        ///     オブジェクトが等しいかを判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public override bool Equals(object obj)
        {
            return obj is SkillEffectParameter other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value, DisplayFormat);
        }
    }
}
