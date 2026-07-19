using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     装備スキル構成に応じたBGMループを初期化し、拍の進行に合わせて
    ///     CRIセレクターラベルの切り替えを駆動するクラス。
    /// </summary>
    public class EquipmentBgmInitializer : MonoBehaviour
    {
        /// <summary>
        ///     依存を解決し、装備スキルからシーケンスを構築してBGMループを開始する。
        /// </summary>
        private async void Start()
        {
            try
            {
                if (_labelTable == null)
                {
                    Debug.LogWarning($"[{nameof(EquipmentBgmInitializer)}] {nameof(_labelTable)} が設定されていません。", this);
                    return;
                }

                _musicPlayer = await ServiceLocator.GetInstanceAsync<MusicPlayer>();
                _musicSyncState = await ServiceLocator.GetInstanceAsync<MusicSyncState>();
                SkillBuildDefinition skillBuild = await ServiceLocator.GetInstanceAsync<SkillBuildDefinition>();

                if (_musicPlayer == null || _musicSyncState == null)
                {
                    Debug.LogWarning($"[{nameof(EquipmentBgmInitializer)}] 依存の解決に失敗しました。", this);
                    return;
                }

                SkillBgmSelectorTable table = _labelTable.ToDomain();
                BgmSelectorSequence sequence = table.CreateSequence(CollectEquippedSkillIds(skillBuild));
                _service = new EquipmentBgmService(sequence, _measuresPerDivision);

                if (!_service.HasSequence)
                {
                    // 装備スキルが無い（または対応ラベルが無い）場合はセレクターを操作せず、
                    // CRIのデフォルト＝通常BGMをそのまま再生する正常系。
                    Debug.Log(
                        $"[{nameof(EquipmentBgmInitializer)}] 装備スキルの差分が無いため、通常BGM（セレクター切替なし）で再生します。",
                        this);
                    _service = null;
                    return;
                }

                _musicPlayer.SetSelectorLabel(SELECTOR_NAME, _service.InitialLabel);
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        ///     毎フレーム、現在の拍をサービスに渡して小節の切り替えを駆動する。
        /// </summary>
        private void Update()
        {
            if (_service == null || _musicSyncState == null || _musicPlayer == null)
            {
                return;
            }

            if (_service.TryAdvance(_musicSyncState.CurrentBeat, out string label))
            {
                _musicPlayer.SetSelectorLabel(SELECTOR_NAME, label);
            }
        }

        /// <summary>
        ///     装備スキル構成からスキルID列（スロット順）を収集する。
        /// </summary>
        /// <param name="skillBuild"> 装備スキル構成。 </param>
        /// <returns> 装備中スキルのID列。 </returns>
        private static IReadOnlyList<int> CollectEquippedSkillIds(SkillBuildDefinition skillBuild)
        {
            if (skillBuild?.EquippedSkills == null)
            {
                return Array.Empty<int>();
            }

            List<int> skillIds = new(skillBuild.EquippedSkills.Count);
            foreach (EquippedSkill equippedSkill in skillBuild.EquippedSkills)
            {
                if (!equippedSkill.HasSkill)
                {
                    continue;
                }

                skillIds.Add(equippedSkill.SkillTemplate.Id.Value);
            }

            return skillIds;
        }

        [SerializeField, Tooltip("スキルIDとCRIセレクターラベル名の対応表。")]
        private BgmSelectorLabelTableAsset _labelTable;
        [SerializeField, Tooltip("セレクターを切り替える1区切りの小節数。16小節ループを原曲/S1/S2/S3の4区切りで回すため既定は4。")]
        private int _measuresPerDivision = DEFAULT_MEASURES_PER_DIVISION;

        /// <summary> CRIのセレクター名。 </summary>
        private const string SELECTOR_NAME = "Selector_BGM";
        /// <summary> 1区切りの小節数の初期値。16小節ループを4区切りに分割するため4とする。 </summary>
        private const int DEFAULT_MEASURES_PER_DIVISION = 4;

        private MusicPlayer _musicPlayer;
        private MusicSyncState _musicSyncState;
        private EquipmentBgmService _service;
    }
}
