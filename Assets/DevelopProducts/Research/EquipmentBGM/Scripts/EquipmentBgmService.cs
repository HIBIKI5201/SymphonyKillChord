using System;
using System.Collections.Generic;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.View.Persistent.Music;
using UnityEngine;
namespace DevelopProducts.EquipmentBGM
{
    /// <summary>
    ///     装備スキルの構成に基づきCRIセレクターを小節ごとに順番に切り替えるサービスクラス。
    ///     2スキル時: [原曲, S1, 原曲, S2] の4小節ループ。
    ///     3スキル時: [原曲, S1, S2, S3] の4小節ループ。
    /// </summary>
    public class EquipmentBgmService
    {
        /// <summary>
        ///     EquipmentBgmService を初期化し、装備スキルからセレクターシーケンスを構築する。
        /// </summary>
        /// <param name="musicPlayer"> セレクター切り替えの対象となる音楽プレイヤー。 </param>
        /// <param name="table"> スキルIDとセレクターラベル名の対応表。 </param>
        /// <param name="skillBuild"> 装備中スキルの参照。 </param>
        /// <param name="measuresPerStep"> シーケンスを1ステップ進める間隔（小節数）。 </param>
        public EquipmentBgmService(MusicPlayer musicPlayer, BgmSelectorLabelTable table, SkillBuildDefinition skillBuild, int measuresPerStep = 1)
        {
            _musicPlayer = musicPlayer;
            _measuresPerStep = Mathf.Max(1, measuresPerStep);
            _sequence = BuildSequence(table, skillBuild);

            if (_sequence.Length == 0)
            {
                Debug.LogWarning("[EquipmentBGM] セレクターシーケンスが空です。スキル装備とテーブル設定を確認してください。");
                return;
            }

            _musicPlayer.SetSelectorLabel(SELECTOR_NAME, _sequence[0]);
            _previousStep = 0;
            Debug.Log($"<color=green>[EquipmentBGM] 初期ラベル設定: {_sequence[0]}（シーケンス全{_sequence.Length}ステップ、{_measuresPerStep}小節/ステップ）</color>");
        }

        /// <summary>
        ///     毎フレーム呼び出す。小節の変わり目を検出して次の小節のセレクターラベルをCRIに予約する。
        ///     CRI側のビート同期設定により、切り替えは次のブロック頭で反映される。
        /// </summary>
        /// <param name="currentBeat"> MusicSyncState.CurrentBeat の値。 </param>
        public void Tick(int currentBeat)
        {
            if (_sequence.Length == 0) return;

            int currentStep = currentBeat / (BEATS_PER_MEASURE * _measuresPerStep);
            if (currentStep == _previousStep) return;
            _previousStep = currentStep;

            _queuedIndex = (_queuedIndex + 1) % _sequence.Length;
            string label = _sequence[_queuedIndex];
            _musicPlayer.SetSelectorLabel(SELECTOR_NAME, label);
            Debug.Log($"<color=green>[EquipmentBGM] ステップ{currentStep}（{_measuresPerStep}小節ごと）→ 次ブロック予約: {label}</color>");
        }

        /// <summary>
        ///     装備スキルと対応表から、小節ごとに切り替えるセレクターラベルのシーケンスを構築する。
        /// </summary>
        /// <param name="table"> スキルIDとセレクターラベル名の対応表。 </param>
        /// <param name="skillBuild"> 装備中スキルの参照。 </param>
        /// <returns> 小節ごとに切り替えるセレクターラベルの配列。 </returns>
        private string[] BuildSequence(BgmSelectorLabelTable table, SkillBuildDefinition skillBuild)
        {
            var skillLabels = new List<string>();
            foreach (var slot in skillBuild.EquippedSkills)
            {
                if (!slot.HasSkill) continue;
                if (table.TryGetLabel(slot.SkillData.Id, out string label))
                    skillLabels.Add(label);
            }

            if (skillLabels.Count == 0) return Array.Empty<string>();

            string original = table.OriginalLabel;

            // スキルが2つの場合
            if (skillLabels.Count == 2)
                return new[] { original, skillLabels[0], original, skillLabels[1] };

            // スキルが3つの場合
            if (skillLabels.Count == 3)
                return new[] { original, skillLabels[0], skillLabels[1], skillLabels[2] };

            Debug.LogWarning($"[EquipmentBGM] 想定外のスキル数: {skillLabels.Count}。スキルラベルのみでシーケンスを構築します。");
            return skillLabels.ToArray();
        }

        /// <summary> CRIのセレクター名。 </summary>
        private const string SELECTOR_NAME = "Selector_BGM";
        /// <summary> 1小節あたりの拍数。 </summary>
        private const int BEATS_PER_MEASURE = 4;

        /// <summary> セレクター切り替えの対象となる音楽プレイヤー。 </summary>
        private readonly MusicPlayer _musicPlayer;
        /// <summary> 小節ごとに切り替えるセレクターラベルのシーケンス。 </summary>
        private readonly string[] _sequence;
        /// <summary> シーケンスを1ステップ進める間隔（小節数）。 </summary>
        private readonly int _measuresPerStep;
        /// <summary> 現在予約しているシーケンスのインデックス。 </summary>
        private int _queuedIndex;
        /// <summary> 前回処理したステップ番号。 </summary>
        private int _previousStep = -1;
    }
}
