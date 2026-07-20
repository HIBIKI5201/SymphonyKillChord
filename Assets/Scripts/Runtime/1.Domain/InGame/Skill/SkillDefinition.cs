using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Constant;
using System;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルの定義（条件・効果・演出）を保持するドメインクラス。
    /// </summary>
    public class SkillDefinition : IEquatable<SkillDefinition>
    {
        /// <summary> スキルの識別子。 </summary>
        public SkillId Id { get; }

        /// <summary> スキルの入力パターン。 </summary>
        public SkillPattern SkillPattern { get; }

        /// <summary> クールダウン時間の長さ。 </summary>
        public SkillCooldownTime CooldownTime { get; }

        /// <summary> スキルの種類。 </summary>
        public SkillType Type { get; }

        /// <summary> スキルの効果定義です。 </summary>
        public SkillEffectSpec EffectSpec { get; }

        /// <summary> スキル発動時に再生するアニメーションのキー。空なら通常攻撃アニメーションを使う。 </summary>
        public string AnimationKey { get; }

        private const int MIN_PATTERN_LENGTH = 1;

        /// <summary>
        ///     コンストラクタ。ID・パターン・効果を指定して初期化する。
        /// </summary>
        public SkillDefinition(SkillId id, SkillPattern skillPattern, double cooldownBarRatio, SkillType type, SkillEffectSpec effectSpec, double bpm, string animationKey)
        {
            if (!double.IsFinite(bpm) || bpm <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(bpm), "BPMは0超えの有限値で設定してください。");
            }

            if (!double.IsFinite(cooldownBarRatio) || cooldownBarRatio < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownBarRatio), "クールダウン時間の小節比率は0以上の有限値で設定してください。");
            }

            Id = id;
            SkillPattern = skillPattern;
            Type = type;
            EffectSpec = effectSpec;
            AnimationKey = animationKey;
            CooldownTime = new SkillCooldownTime(CalcCooldownTime(cooldownBarRatio, bpm));
        }

        /// <summary>
        ///     逆順の入力履歴からこのスキルの発動条件に一致するか判定する。
        /// </summary>
        /// <param name="reversInput"> 逆順に並べた入力履歴です。 </param>
        /// <returns> 一致する場合はtrue。 </returns>
        public bool IsMatch(ReadOnlySpan<BeatType> reversInput)
        {
            int length = SkillPattern.Signatures.Length;
            if (length < MIN_PATTERN_LENGTH) { return false; }
            if (reversInput.Length < length) { return false; }

            ReadOnlySpan<BeatType> pattern = reversInput.Slice(0, length);
            return SkillPattern.EqualsInReverse(pattern);
        }

        /// <summary>
        ///     指定したSkillDefinitionと等価か判定する。
        /// </summary>
        /// <param name="other"> 比較対象のSkillDefinitionです。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public bool Equals(SkillDefinition other)
        {
            if (other == null || Id != other.Id) { return false; }
            return SkillPattern == other.SkillPattern;
        }

        /// <summary>
        ///     現在のBPMを指定し、スキルのクールダウン時間を計算する。
        /// </summary>
        /// <param name="cooldownBarRatio"> クールダウンの小節比率です。 </param>
        /// <param name="bpm"> 現在のBPMです。 </param>
        /// <returns> クールダウン秒数です。 </returns>
        private double CalcCooldownTime(double cooldownBarRatio, double bpm)
        {
            double secondsPerBar = MusicConstants.SECONDS_PER_MINUTE / bpm * MusicConstants.STANDARD_BEATS_PER_BAR;
            return secondsPerBar * cooldownBarRatio;
        }
    }
}
