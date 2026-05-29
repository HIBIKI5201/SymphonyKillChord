using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルごとの入力進行状態を管理するクラス。
    /// </summary>
    public class SkillInputProgressState
    {
        /// <summary>
        ///     指定スキルの現在の一致数を取得する。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <returns> 指定スキルの現在の一致数。 </returns>
        public int GetMatchedCount(int skillId)
        {
            return _matchedCounts.TryGetValue(skillId, out int matchedCount)
                ? matchedCount
                : 0;
        }

        /// <summary>
        ///     指定スキルの現在の一致数を設定する。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <param name="matchedCount"> 設定する一致数。 </param>
        public void SetMatchedCount(int skillId, int matchedCount)
        {
            _matchedCounts[skillId] = matchedCount;
        }

        /// <summary>
        ///     指定スキルの現在の一致数をリセットする。
        /// </summary>
        /// <param name="skillId"></param>
        public void Reset(int skillId)
        {
            _matchedCounts[skillId] = 0;
        }

        /// <summary>
        ///     全スキルの現在の一致数をリセットする。
        /// </summary>
        public void Clear()
        {
            _matchedCounts.Clear();
        }

        private readonly Dictionary<int, int> _matchedCounts = new();
    }
}
