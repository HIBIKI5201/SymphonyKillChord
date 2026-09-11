using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.Skill
{
    /// <summary>
    ///     アウトゲーム画面で共通利用するスキル表示文字列。
    /// </summary>
    public readonly struct SkillDisplayText : IEquatable<SkillDisplayText>
    {
        /// <summary>
        ///     表示文字列を初期化する。
        /// </summary>
        /// <param name="comboLabel"> 発動コンボ表示。 </param>
        /// <param name="skillTypeLabel"> スキル種類表示。 </param>
        /// <param name="hasEffectDescription"> 効果説明を表示する場合は true。 </param>
        /// <param name="effectDescription"> 効果説明。 </param>
        /// <param name="comboStepColors"> 発動コマンドの入力順に並んだ、拍子に対応する色一覧。 </param>
        public SkillDisplayText(
            string comboLabel,
            string skillTypeLabel,
            bool hasEffectDescription,
            string effectDescription,
            Color[] comboStepColors)
        {
            ComboLabel = comboLabel ?? string.Empty;
            SkillTypeLabel = skillTypeLabel ?? string.Empty;
            HasEffectDescription = hasEffectDescription;
            EffectDescription = effectDescription ?? string.Empty;
            ComboStepColors = comboStepColors ?? Array.Empty<Color>();
        }

        /// <summary> 発動コンボ表示。 </summary>
        public string ComboLabel { get; }

        /// <summary> スキル種類表示。 </summary>
        public string SkillTypeLabel { get; }

        /// <summary> 効果説明を表示する場合は true。 </summary>
        public bool HasEffectDescription { get; }

        /// <summary> 効果説明。 </summary>
        public string EffectDescription { get; }

        /// <summary> 発動コマンドの入力順に並んだ、拍子に対応する色一覧。 </summary>
        public Color[] ComboStepColors { get; }

        /// <summary>
        ///     等値比較を行う。
        /// </summary>
        /// <param name="other"> 比較対象。 </param>
        /// <returns> 同じ表示文字列の場合は true。 </returns>
        public bool Equals(SkillDisplayText other)
        {
            return ComboLabel == other.ComboLabel &&
                   SkillTypeLabel == other.SkillTypeLabel &&
                   HasEffectDescription == other.HasEffectDescription &&
                   EffectDescription == other.EffectDescription;
        }

        /// <summary>
        ///     等値比較を行う。
        /// </summary>
        /// <param name="obj"> 比較対象。 </param>
        /// <returns> 同じ表示文字列の場合は true。 </returns>
        public override bool Equals(object obj)
        {
            return obj is SkillDisplayText other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得する。
        /// </summary>
        /// <returns> ハッシュコード。 </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                ComboLabel,
                SkillTypeLabel,
                HasEffectDescription,
                EffectDescription);
        }
    }
}
