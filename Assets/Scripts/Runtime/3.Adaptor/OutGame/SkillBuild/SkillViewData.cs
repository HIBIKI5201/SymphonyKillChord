using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     ViewModel が保持するスキル表示データ。
    /// </summary>
    public readonly struct SkillViewData : IEquatable<SkillViewData>
    {
        /// <summary>
        ///     スキル表示データを初期化する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        /// <param name="displayName"> 表示名。 </param>
        /// <param name="icon"> アイコン。 </param>
        /// <param name="comboLabel"> 発動コンボ表示。 </param>
        /// <param name="skillTypeLabel"> スキル種類表示。 </param>
        /// <param name="hasEffectDescription"> 効果説明を表示する場合は true。 </param>
        /// <param name="effectDescription"> 効果説明。 </param>
        /// <param name="tips"> 改造画面に表示するスキルTips。 </param>
        /// <param name="level"> レベル。 </param>
        public SkillViewData(
            int skillId,
            string displayName,
            Sprite icon,
            string comboLabel,
            string skillTypeLabel,
            bool hasEffectDescription,
            string effectDescription,
            string tips,
            int level)
        {
            SkillId = skillId;
            DisplayName = displayName ?? string.Empty;
            Icon = icon;
            ComboLabel = comboLabel ?? string.Empty;
            SkillTypeLabel = skillTypeLabel ?? string.Empty;
            HasEffectDescription = hasEffectDescription;
            EffectDescription = effectDescription ?? string.Empty;
            Tips = tips ?? string.Empty;
            Level = level;
        }

        /// <summary> スキル ID。 </summary>
        public int SkillId { get; }

        /// <summary> 表示名。 </summary>
        public string DisplayName { get; }

        /// <summary> アイコン。 </summary>
        public Sprite Icon { get; }

        /// <summary> 発動コンボ表示。 </summary>
        public string ComboLabel { get; }

        /// <summary> スキル種類表示。 </summary>
        public string SkillTypeLabel { get; }

        /// <summary> 効果説明を表示する場合は true。 </summary>
        public bool HasEffectDescription { get; }

        /// <summary> 効果説明。 </summary>
        public string EffectDescription { get; }

        /// <summary> 改造画面に表示するスキルTips。 </summary>
        public string Tips { get; }

        /// <summary> レベル。 </summary>
        public int Level { get; }

        /// <summary>
        ///     等値比較を行う。
        /// </summary>
        /// <param name="other"> 比較対象。 </param>
        /// <returns> 同じ表示データの場合は true。 </returns>
        public bool Equals(SkillViewData other)
        {
            return SkillId == other.SkillId &&
                   DisplayName == other.DisplayName &&
                   Icon == other.Icon &&
                   ComboLabel == other.ComboLabel &&
                   SkillTypeLabel == other.SkillTypeLabel &&
                   HasEffectDescription == other.HasEffectDescription &&
                   EffectDescription == other.EffectDescription &&
                   Tips == other.Tips &&
                   Level == other.Level;
        }

        /// <summary>
        ///     等値比較を行う。
        /// </summary>
        /// <param name="obj"> 比較対象。 </param>
        /// <returns> 同じ表示データの場合は true。 </returns>
        public override bool Equals(object obj)
        {
            return obj is SkillViewData other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得する。
        /// </summary>
        /// <returns> ハッシュコード。 </returns>
        public override int GetHashCode()
        {
            HashCode hashCode = new();
            hashCode.Add(SkillId);
            hashCode.Add(DisplayName);
            hashCode.Add(Icon);
            hashCode.Add(ComboLabel);
            hashCode.Add(SkillTypeLabel);
            hashCode.Add(HasEffectDescription);
            hashCode.Add(EffectDescription);
            hashCode.Add(Tips);
            hashCode.Add(Level);
            return hashCode.ToHashCode();
        }
    }
}
