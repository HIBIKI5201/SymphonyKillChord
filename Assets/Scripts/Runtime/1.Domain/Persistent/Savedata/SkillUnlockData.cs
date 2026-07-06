using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     スキル解放情報を保持するセーブデータクラス。
    /// </summary>
    [Serializable]
    public sealed class SkillUnlockData
    {
        /// <summary>
        ///    データを初期化するコンストラクタ。
        /// </summary>
        public SkillUnlockData()
        {
            _researchPoint = 0;
            _unlockedSkillNodeIds = new int[0];
            _unlockedSkillIds = new int[0];
        }

        /// <summary> 研究ポイント </summary>
        public int ResearchPoint => _researchPoint;
        /// <summary> 解放済みのノードID </summary>
        public int[] UnlockedSkillNodeIds => _unlockedSkillNodeIds;
        /// <summary> 解放済みのスキルID </summary>
        public int[] UnlockedSkillIds => _unlockedSkillIds;

        /// <summary>
        ///     研究ポイントの値を設定する。
        /// </summary>
        /// <param name="value"></param>
        public void SetResearchPoint(int value)
        {
            _researchPoint = value;
        }

        /// <summary>
        ///     解放済みのノードIDを設定する。
        /// </summary>
        /// <param name="value"></param>
        public void SetUnlockedSkillNodeIds(int[] value)
        {
            _unlockedSkillNodeIds = value;
        }

        /// <summary>
        ///     解放済みのスキルIDを設定する。
        /// </summary>
        /// <param name="value"></param>
        public void SetUnlockedSkillIds(int[] value)
        {
            _unlockedSkillIds = value;
        }

        [SerializeField, Tooltip("研究ポイント")]
        private int _researchPoint;

        [SerializeField, Tooltip("解放済みのノード ID")]
        private int[] _unlockedSkillNodeIds;

        [SerializeField, Tooltip("解放済みのスキル ID")]
        private int[] _unlockedSkillIds;
    }
}
