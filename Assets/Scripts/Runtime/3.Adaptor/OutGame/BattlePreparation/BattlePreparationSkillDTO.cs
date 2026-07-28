using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.BattlePreparation
{
    /// <summary>
    ///     戦闘準備画面に表示する1スロット分のスキル情報です。
    /// </summary>
    public readonly struct BattlePreparationSkillDTO : IEquatable<BattlePreparationSkillDTO>
    {
        /// <summary>
        ///     表示情報を初期化します。
        /// </summary>
        /// <param name="slotIndex"> スロット番号です。 </param>
        /// <param name="hasSkill"> スキルが装備されている場合はtrue。 </param>
        /// <param name="icon"> スキルアイコンです。 </param>
        /// <param name="displayName"> スキル表示名です。 </param>
        /// <param name="comboLabel"> 発動コンボ表示です。 </param>
        /// <param name="skillTypeLabel"> スキル種類表示です。 </param>
        /// <param name="hasEffectDescription"> 効果説明を表示する場合はtrue。 </param>
        /// <param name="effectDescription"> 効果説明です。 </param>
        public BattlePreparationSkillDTO(
            int slotIndex,
            bool hasSkill,
            Sprite icon,
            string displayName,
            string comboLabel,
            string skillTypeLabel,
            bool hasEffectDescription,
            string effectDescription)
        {
            SlotIndex = slotIndex;
            HasSkill = hasSkill;
            Icon = icon;
            DisplayName = displayName;
            ComboLabel = comboLabel;
            SkillTypeLabel = skillTypeLabel;
            HasEffectDescription = hasEffectDescription;
            EffectDescription = effectDescription;
        }

        /// <summary> スロット番号です。 </summary>
        public int SlotIndex { get; }

        /// <summary> スキルが装備されている場合はtrue。 </summary>
        public bool HasSkill { get; }

        /// <summary> スキルアイコンです。 </summary>
        public Sprite Icon { get; }

        /// <summary> スキル表示名です。 </summary>
        public string DisplayName { get; }

        /// <summary> 発動コンボ表示です。 </summary>
        public string ComboLabel { get; }

        /// <summary> スキル種類表示です。 </summary>
        public string SkillTypeLabel { get; }

        /// <summary> 効果説明を表示する場合はtrue。 </summary>
        public bool HasEffectDescription { get; }

        /// <summary> 効果説明です。 </summary>
        public string EffectDescription { get; }

        /// <summary>
        ///     等値比較を行います。
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(BattlePreparationSkillDTO other)
        {
            return SlotIndex == other.SlotIndex &&
                   HasSkill == other.HasSkill &&
                   Icon == other.Icon &&
                   DisplayName == other.DisplayName &&
                   ComboLabel == other.ComboLabel &&
                   SkillTypeLabel == other.SkillTypeLabel &&
                   HasEffectDescription == other.HasEffectDescription &&
                   EffectDescription == other.EffectDescription;
        }

        /// <summary>
        ///     等値比較を行います。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is BattlePreparationSkillDTO other && Equals(other);
        }

        /// <summary>
        ///    ハッシュコードを取得します。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                SlotIndex,
                HasSkill,
                Icon,
                DisplayName,
                ComboLabel,
                SkillTypeLabel,
                HasEffectDescription,
                EffectDescription);
        }
    }
}
