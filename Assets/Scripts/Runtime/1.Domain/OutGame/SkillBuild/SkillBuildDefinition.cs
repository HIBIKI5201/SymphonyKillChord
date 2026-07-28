using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.OutGame.SkillBuild
{
    /// <summary>
    ///     プレイヤーが装備しているスキルを表すクラス。
    /// </summary>
    public sealed class SkillBuildDefinition
    {
        /// <summary>
        ///    プレイヤーが装備しているスキルの配列を初期化するコンストラクタ。
        /// </summary>
        /// <param name="equippedSkills"> プレイヤーが装備しているスキル配列。 </param>
        public SkillBuildDefinition(EquippedSkill[] equippedSkills = null)
        {
            _equippedSkills = BuildSlotArray(equippedSkills);
        }

        /// <summary> プレイヤーが装備しているスキルの配列を取得する。 </summary>
        public IReadOnlyList<EquippedSkill> EquippedSkills => _equippedSkills;

        /// <summary> 現在のスロット数を取得する。 </summary>
        public int SlotCount => _equippedSkills.Length;

        /// <summary> 初期スロット数。 </summary>
        public const int INITIAL_SLOT_COUNT = 2;

        /// <summary>
        ///     プレイヤーが装備しているスキルの配列を更新するメソッド。
        ///     装備変更が行われた際に呼び出されることを想定している。
        /// </summary>
        /// <param name="newEquippedSkills"> 新しい装備スキル配列。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UpdateEquippedSkills(in EquippedSkill[] newEquippedSkills)
        {
            if (newEquippedSkills == null)
            {
                throw new ArgumentNullException(nameof(newEquippedSkills));
            }

            _equippedSkills = BuildSlotArray(newEquippedSkills);
        }

        /// <summary>
        ///     指定したスロット数まで編成スロットを拡張する。
        /// </summary>
        /// <param name="slotCount"> 拡張後のスロット数。 </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void EnsureSlotCount(int slotCount)
        {
            if (slotCount < INITIAL_SLOT_COUNT)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotCount),
                    $"スロット数は {INITIAL_SLOT_COUNT} 以上である必要があります。");
            }

            if (slotCount <= _equippedSkills.Length)
            {
                return;
            }

            EquippedSkill[] expandedSkills = new EquippedSkill[slotCount];
            Array.Copy(_equippedSkills, expandedSkills, _equippedSkills.Length);
            _equippedSkills = expandedSkills;
        }

        /// <summary>
        ///     装備スキル構成が現在のスロット条件を満たすことを検証する。
        /// </summary>
        /// <param name="equippedSkills"> 検証する装備スキル構成。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void ValidateEquipment(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            if (equippedSkills == null)
            {
                throw new ArgumentNullException(nameof(equippedSkills));
            }

            if (equippedSkills.Count != SlotCount)
            {
                throw new ArgumentException(
                    $"装備スキル数は現在のスロット数と一致する必要があります。 slotCount={SlotCount}",
                    nameof(equippedSkills));
            }
        }

        private EquippedSkill[] _equippedSkills;

        /// <summary>
        ///     スロット配列を最小スロット数を満たす形で構築する。
        /// </summary>
        /// <param name="equippedSkills"> 元となる装備スキル一覧。 </param>
        /// <returns> 正規化したスロット配列。 </returns>
        private EquippedSkill[] BuildSlotArray(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            int slotCount = Math.Max(INITIAL_SLOT_COUNT, equippedSkills?.Count ?? 0);
            EquippedSkill[] result = new EquippedSkill[slotCount];

            if (equippedSkills == null)
            {
                return result;
            }

            for (int i = 0; i < equippedSkills.Count; i++)
            {
                result[i] = equippedSkills[i];
            }

            return result;
        }
    }
}
