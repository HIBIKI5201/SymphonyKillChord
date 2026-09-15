using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     プレイヤーの装備スキル構成のセーブデータを表すクラス。
    ///     <para> プレイヤーの装備スキルの ID のリストと、スキルレベルアップポイントを保持する。 </para>
    /// </summary>
    [Serializable]
    public sealed class SkillBuildData
    {
        /// <summary>
        ///     プレイヤーの装備スキルの ID のリスト。
        /// </summary>
        public IReadOnlyList<int> EquipmentSkillIDs => _equipmentSkillIDs;

        /// <summary>
        ///     プレイヤーのスキルレベルアップポイント。
        /// </summary>
        public int SkillLevelupPoint => _skillLevelupPoint;

        /// <summary>
        ///     スキルIDごとの現在レベルの保存記録一覧。
        /// </summary>
        public IReadOnlyList<SkillLevelEntry> SkillLevels => _skillLevels;

        /// <summary>
        ///    プレイヤーの装備スキルの ID のリストを設定する。
        /// </summary>
        /// <param name="skillIDs"> 設定する装備スキルの ID のリスト。 </param>
        /// <exception cref="ArgumentNullException"> skillIDs が null の場合にスローされます。</exception>
        public void SetEquipmentSkillIDs(List<int> skillIDs)
        {
            if (skillIDs == null)
            {
                throw new ArgumentNullException(nameof(skillIDs), "装備スキルの ID のリストは null にできません。");
            }

            // 装備スロットが増える可能性があるため、容量を調整する
            // リストの再割当てを減らし、必要な容量を確保することでパフォーマンスを向上させる
            if (_equipmentSkillIDs.Capacity < skillIDs.Count)
            {
                _equipmentSkillIDs.Capacity = skillIDs.Count;
            }

            _equipmentSkillIDs.Clear();
            _equipmentSkillIDs.AddRange(skillIDs);
        }

        /// <summary>
        ///    プレイヤーのスキルレベルアップポイントを設定する。
        /// </summary>
        /// <param name="point"> 設定するスキルレベルアップポイントの値。 </param>
        /// <exception cref="ArgumentOutOfRangeException"> point が 0 未満の場合にスローされます。</exception>
        public void SetSkillLevelupPoint(int point)
        {
            if (point < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(point), "スキルレベルアップポイントは 0 以上である必要があります。");
            }
            _skillLevelupPoint = point;
        }

        /// <summary>
        ///     指定したスキルの現在レベルを取得する。保存記録が無い場合は既定値を返す。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <param name="defaultLevel"> 保存記録が無い場合に返す既定レベル(テンプレートの基準レベル)。 </param>
        /// <returns> 現在のレベル。 </returns>
        public int GetSkillLevel(int skillId, int defaultLevel)
        {
            SkillLevelEntry entry = FindSkillLevelEntry(skillId);
            return entry?.Level ?? defaultLevel;
        }

        /// <summary>
        ///     指定したスキルの現在レベルを設定する。既存の記録が無ければ新規追加する。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <param name="level"> 設定するレベル。 </param>
        /// <exception cref="ArgumentOutOfRangeException"> level が 0 未満の場合にスローされます。</exception>
        public void SetSkillLevel(int skillId, int level)
        {
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "スキルレベルは 0 以上である必要があります。");
            }

            SkillLevelEntry entry = FindSkillLevelEntry(skillId);
            if (entry != null)
            {
                entry.SetLevel(level);
                return;
            }

            _skillLevels.Add(new SkillLevelEntry(skillId, level));
        }

        /// <summary>
        ///     指定したスキルIDのレベル保存記録を検索する。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <returns> 見つかった保存記録。存在しない場合は null。 </returns>
        private SkillLevelEntry FindSkillLevelEntry(int skillId)
        {
            for (int i = 0; i < _skillLevels.Count; i++)
            {
                SkillLevelEntry entry = _skillLevels[i];
                if (entry != null && entry.SkillId == skillId)
                {
                    return entry;
                }
            }

            return null;
        }

        // 各種データを保持するメンバー変数
        [SerializeField, Tooltip("プレイヤーの装備スキルの ID のリスト")]
        private List<int> _equipmentSkillIDs = new List<int>();
        [SerializeField, Tooltip("プレイヤーのスキルレベルアップポイント")]
        private int _skillLevelupPoint;
        [SerializeField, Tooltip("スキルIDごとの現在レベルの保存記録")]
        private List<SkillLevelEntry> _skillLevels = new List<SkillLevelEntry>();
    }
}
