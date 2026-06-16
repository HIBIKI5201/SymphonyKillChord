using KillChord.Runtime.Domain.OutGame.SkillBuild;
using UnityEngine;

namespace KillChord.Runtime.Application.OutGame.SkillBuild
{
    /// <summary>
    ///    プレイヤーの装備スキルの構成を管理するユースケースクラス。
    ///    装備スキルの変更や入れ替えなど、スキル編成に関するビジネスロジックを提供する。
    /// </summary>
    public sealed class SkillBuildUseCase
    {
        /// <summary>
        ///     SkillBuildUseCase クラスのコンストラクタ。
        /// </summary>
        /// <param name="skillBuildDefinition"> 装備スキルの定義を表すオブジェクト。 </param>
        public SkillBuildUseCase(SkillBuildDefinition skillBuildDefinition)
        {
            _skillBuildDefinition = skillBuildDefinition;
        }
        
        /// <summary>
        ///     装備スキルの配列を更新するメソッド。
        /// </summary>
        /// <param name="newEquippedSkills"> 新しい装備スキルの配列。 </param>
        public void UpdateEquippedSkills(in EquippedSkill[] newEquippedSkills)
        {
            _skillBuildDefinition.UpdateEquippedSkills(newEquippedSkills);
        }

        /// <summary>
        ///     プレイヤーが装備スキルのスロットを変更する際に呼び出されるメソッド。指定されたスロットインデックスに新しい装備スキルを設定する。
        /// </summary>
        /// <param name="slotIndex"> 変更するスロットのインデックス。 </param>
        /// <param name="newEquippedSkill"> 新しい装備スキル。 </param>
        public void ChangeEquippedSkill(int slotIndex, EquippedSkill newEquippedSkill)
        {
            _skillBuildDefinition.ChangeEquippedSkill(slotIndex, newEquippedSkill);
        }

        /// <summary>
        ///     装備スキル同士の入れ替えを行うメソッド。
        /// </summary>
        /// <param name="slotIndex1"> 入れ替え元のスロットのインデックス。 </param>
        /// <param name="slotIndex2"> 入れ替え先のスロットのインデックス。 </param>
        public void SwapEquippedSkills(int slotIndex1, int slotIndex2)
        {
            _skillBuildDefinition.SwapEquippedSkills(slotIndex1, slotIndex2);
        }

        private readonly SkillBuildDefinition _skillBuildDefinition;
    }
}
