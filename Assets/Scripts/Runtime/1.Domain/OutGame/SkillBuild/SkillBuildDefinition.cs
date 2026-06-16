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
        /// <param name="equippedSkills"> プレイヤーが装備しているスキルの配列。 </param>
        public SkillBuildDefinition(EquippedSkill[] equippedSkills)
        {
            _equippedSkills = equippedSkills;
        }

        /// <summary>
        ///     プレイヤーが装備しているスキルの配列を更新するメソッド。
        ///     装備変更が行われた際に呼び出されることを想定している。
        /// </summary>
        /// <param name="newEquippedSkills"> 新しい装備スキルの配列。 </param>
        public void UpdateEquippedSkills(in EquippedSkill[] newEquippedSkills)
        {
            _equippedSkills = newEquippedSkills;
        }

        /// <summary>
        ///    プレイヤーが装備しているスキルの配列の特定のスロットを更新するメソッド。
        /// </summary>
        /// <param name="slotIndex"> 更新するスロットのインデックス。 </param>
        /// <param name="newEquippedSkill"> 新しい装備スキル。 </param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public void ChangeEquippedSkill(int slotIndex, EquippedSkill newEquippedSkill)
        {
            if (slotIndex < 0 || slotIndex >= _equippedSkills.Length)
            {
                throw new System.ArgumentOutOfRangeException(nameof(slotIndex), "スロットインデックスが範囲外です。");
            }
            _equippedSkills[slotIndex] = newEquippedSkill;
        }

        /// <summary>
        ///     プレイヤーが装備しているスキルの配列の特定のスロットを入れ替えるメソッド。
        /// </summary>
        /// <param name="slotIndex1"> 入れ替え元のスロットのインデックス。 </param>
        /// <param name="slotIndex2"> 入れ替え先のスロットのインデックス。 </param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public void SwapEquippedSkills(int slotIndex1, int slotIndex2)
        {
            if (slotIndex1 < 0 || slotIndex1 >= _equippedSkills.Length ||
                slotIndex2 < 0 || slotIndex2 >= _equippedSkills.Length)
            {
                throw new System.ArgumentOutOfRangeException("スロットインデックスが範囲外です。");
            }
            var temp = _equippedSkills[slotIndex1];
            _equippedSkills[slotIndex1] = _equippedSkills[slotIndex2];
            _equippedSkills[slotIndex2] = temp;
        }

        /// <summary> プレイヤーが装備しているスキルの配列を取得するプロパティ。 </summary>
        public IReadOnlyList<EquippedSkill> EquippedSkills => _equippedSkills;

        private EquippedSkill[] _equippedSkills;
    }
}
