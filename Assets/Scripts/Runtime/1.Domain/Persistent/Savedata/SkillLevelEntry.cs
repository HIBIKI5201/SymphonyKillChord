using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     スキル1件分の現在レベルを表す保存データ。
    /// </summary>
    [Serializable]
    public sealed class SkillLevelEntry
    {
        /// <summary>
        ///     スキルレベルの保存データを初期化する。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <param name="level"> 現在のレベル。 </param>
        public SkillLevelEntry(int skillId, int level)
        {
            _skillId = skillId;
            _level = level;
        }

        /// <summary> スキルID。 </summary>
        public int SkillId => _skillId;

        /// <summary> 現在のレベル。 </summary>
        public int Level => _level;

        /// <summary>
        ///     レベルを更新する。
        /// </summary>
        /// <param name="level"> 新しいレベル。 </param>
        internal void SetLevel(int level)
        {
            _level = level;
        }

        [SerializeField, Tooltip("スキル ID。")] private int _skillId;
        [SerializeField, Tooltip("スキルレベル。")] private int _level;
    }
}
