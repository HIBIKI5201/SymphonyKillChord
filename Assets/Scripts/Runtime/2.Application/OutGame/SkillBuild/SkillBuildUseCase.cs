using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.SkillBuild
{
    /// <summary>
    ///     装備スキルのスロット操作や、装備スキルの保存・読み込みなど、装備スキルに関するビジネスロジックを担当するクラス。
    /// </summary>
    public sealed class SkillBuildUseCase
    {
        /// <summary>
        ///     SkillBuildUseCase クラスのコンストラクタ。
        /// </summary>
        /// <param name="skillBuildDefinition"> 装備スキルの定義を表すオブジェクト。 </param>
        public SkillBuildUseCase(SkillBuildDefinition skillBuildDefinition)
        {
            _skillBuildDefinition = skillBuildDefinition
                ?? throw new ArgumentNullException(nameof(skillBuildDefinition));

            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                throw new InvalidOperationException("SavedataSystem のインスタンスが見つかりません。");
            }

            _savedataSystem = savedataSystem;
        }

        /// <summary>
        ///     改造画面のセーブデータを非同期で読み込むメソッド。 
        ///     <para> 装備スキルの ID リストとスキルレベルアップポイントを含む SkillBuildData オブジェクトを返す。</para>
        /// </summary>
        public async ValueTask<SkillBuildData> LoadSkillBuildAsync()
        {
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            return saveData.SkillBuild;
        }

        /// <summary>
        ///     改造画面のセーブデータを非同期で保存するメソッド。
        /// </summary>
        /// <param name="equipmentSkillIDs"> 装備スキルの ID のリスト。 </param>
        /// <param name="skillLevelupPoint"> スキルレベルアップポイント。 </param>
        /// <returns> 非同期操作の完了を表す Task オブジェクト。 </returns>
        public async Task SaveSkillBuildAsync(List<int> equipmentSkillIDs, int skillLevelupPoint)
        {
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            saveData.SkillBuild.SetEquipmentSkillIDs(equipmentSkillIDs);
            saveData.SkillBuild.SetSkillLevelupPoint(skillLevelupPoint);

            await _savedataSystem.SaveAsync(saveData);
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
        private readonly SavedataSystem _savedataSystem;
    }
}
