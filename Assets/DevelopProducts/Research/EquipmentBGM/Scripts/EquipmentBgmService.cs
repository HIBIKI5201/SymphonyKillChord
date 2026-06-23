using System;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.View.Persistent.Music;
using UnityEngine;

namespace DevelopProducts.EquipmentBGM
{
    /// <summary>
    ///     スキル発動に応じてCRIセレクターを切り替えるサービスクラス。
    /// </summary>
    public class EquipmentBgmService : IDisposable
    {
        /// <summary>
        ///     EquipmentBgmService を初期化する。
        /// </summary>
        /// <param name="musicPlayer"> セレクター切り替えの対象となる音楽プレイヤー。 </param>
        /// <param name="table"> スキルIDとセレクターラベル名の対応表。 </param>
        /// <param name="skillController"> スキル発動イベントの購読元。 </param>
        /// <param name="skillBuild"> 装備中スキルの参照。スロットインデックスからスキルIDへの変換に使用する。 </param>
        public EquipmentBgmService(MusicPlayer musicPlayer, BgmSelectorLabelTable table, SkillController skillController, SkillBuildDefinition skillBuild)
        {
            _musicPlayer = musicPlayer;
            _table = table;
            _skillController = skillController;
            _skillBuild = skillBuild;

            _skillController.OnSkillActivated += SwitchSelectorBySkill;
        }

        /// <summary>
        ///     スキル発動時に呼び出す。スロットインデックスから装備スキルIDを取得し、
        ///     テーブルを引いてCRIセレクターを切り替える。
        ///     CRI側のビート同期設定により、切り替えは次のブロック頭で反映される。
        /// </summary>
        /// <param name="slotIndex"> 発動したスキルのスロットインデックス。 </param>
        public void SwitchSelectorBySkill(int slotIndex)
        {
            if (slotIndex >= _skillBuild.SlotCount)
            {
                Debug.LogWarning($"[EquipmentBGM] スロットインデックス {slotIndex} がスロット数の範囲外です。");
                return;
            }

            var skill = _skillBuild.EquippedSkills[slotIndex];
            if (!skill.HasSkill)
            {
                Debug.LogWarning($"[EquipmentBGM] スロット {slotIndex} にスキルが装備されていません。");
                return;
            }

            int skillId = skill.SkillData.Id;
            if (!_table.TryGetLabel(skillId, out string label))
            {
                Debug.LogWarning($"[EquipmentBGM] スキルID {skillId} に対応するラベルがテーブルに未登録。");
                return;
            }

            _musicPlayer.SetSelectorLabel(SELECTOR_NAME, label);
            Debug.Log($"<color=green>[EquipmentBGM] スロット{slotIndex}（スキルID:{skillId}）→ セレクター切り替え: {label}</color>");
        }

        /// <summary>
        ///     スキル発動イベントの購読を解除する。
        /// </summary>
        public void Dispose()
        {
            _skillController.OnSkillActivated -= SwitchSelectorBySkill;
        }

        private const string SELECTOR_NAME = "Selector_BGM";

        private readonly MusicPlayer _musicPlayer;
        private readonly BgmSelectorLabelTable _table;
        private readonly SkillController _skillController;
        private readonly SkillBuildDefinition _skillBuild;
    }
}
