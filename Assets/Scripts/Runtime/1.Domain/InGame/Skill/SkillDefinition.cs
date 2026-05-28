using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Player;
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

        /// <summary> スキルの入力パターン。 </summary>
        public SkillCooldownTime CooldownTime { get;  }

        /// <summary> スキルの効果実装。 </summary>
        public ISkillEffect Effect { get; }

        #region 定数

        private const int MIN_PATTERN_LENGTH = 1;

        #endregion

        /// <summary>
        ///     コンストラクタ。ID・パターン・効果を指定して初期化する。
        /// </summary>
        public SkillDefinition(SkillId id, SkillPattern skillPattern, ISkillEffect effect, double bpm)
        {
            Id = id;
            SkillPattern = skillPattern;
            Effect = effect;
            CooldownTime = new SkillCooldownTime(CalcCooldownTime(bpm));
        }

        /// <summary>
        ///     逆順の入力履歴からこのスキルの発動条件に一致するか判定する。
        /// </summary>
        /// <param name="reversInput">逆順に並べた入力履歴</param>
        /// <returns>一致する場合はtrue。</returns>
        public bool IsMatch(ReadOnlySpan<BeatType> reversInput)
        {
            var length = SkillPattern.Signatures.Length;
            if (length < MIN_PATTERN_LENGTH) return false;
            if (reversInput.Length < length) return false;

            ReadOnlySpan<BeatType> pattern = reversInput.Slice(0, length);
            return SkillPattern.EqualsInReverse(pattern);
        }
        
        /// <summary>
        ///     現在のBPMを指定し、スキルのクールダウン時間を計算する。
        /// </summary>
        /// <param name="bpm"></param>
        private double CalcCooldownTime(double bpm)
        {
            double cooldown = 0;
            // 1小節の長さ
            double secondsPerBar = MusicConstants.SECONDS_PER_MINUTE / bpm * MusicConstants.STANDARD_BEATS_PER_BAR;

            // スキル発動するための拍の長さの合計をクールタイムとする
            for (int i = 0; i < SkillPattern.Signatures.Length; i++)
            {
                cooldown += secondsPerBar / (double)SkillPattern.Signatures[i];
            }
            // 誤差を埋めるため16拍子を一つ足す
            cooldown += secondsPerBar / 16;
            return cooldown;
        }

        /// <summary>
        ///     指定したSkillDefinitionと等価か判定する。
        /// </summary>
        /// <param name="other">比較対象のSkillDefinition</param>
        /// <returns>等しい場合はtrue。</returns>
        public bool Equals(SkillDefinition other)
        {
            if (other == null || Id != other.Id) return false;
            return SkillPattern == other.SkillPattern;
        }
    }
}
