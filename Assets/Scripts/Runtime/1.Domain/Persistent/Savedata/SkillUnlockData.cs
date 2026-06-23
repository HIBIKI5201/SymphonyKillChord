using System;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     スキル解放情報を保持するセーブデータクラス。
    /// </summary>
    [Serializable]
    public sealed class SkillUnlockData
    {
        public SkillUnlockData()
        {
            ResearchPoint = 0;
            UnlockedSkillNodeIds = new int[0];
        }

        /// <summary> 研究ポイント </summary>
        public int ResearchPoint;
        /// <summary> 解放済みのノードID </summary>
        public int[] UnlockedSkillNodeIds;

        /// <summary>
        ///     研究ポイントの値を設定する。
        /// </summary>
        /// <param name="value"></param>
        public void SetResearchPoint(int value)
        {
            ResearchPoint = value;
        }

        /// <summary>
        ///     解放済みのノードIDを設定する。
        /// </summary>
        /// <param name="value"></param>
        public void SetUnlockedSkillNodeIds(int[] value)
        {
            UnlockedSkillNodeIds = value;
        }
    }
}
