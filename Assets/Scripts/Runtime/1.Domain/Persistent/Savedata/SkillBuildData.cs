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

        // 各種データを保持するメンバー変数
        [SerializeField, Tooltip("プレイヤーの装備スキルの ID のリスト")]
        private List<int> _equipmentSkillIDs = new List<int>();
        [SerializeField, Tooltip("プレイヤーのスキルレベルアップポイント")]
        private int _skillLevelupPoint;
    }
}
