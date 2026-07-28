using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     達成した評価条件数からステージランクを算出します。
    /// </summary>
    public static class StageRankCalculator
    {
        /// <summary>
        ///     ステージランクを算出します。
        /// </summary>
        /// <param name="achievedCount"> 達成した評価条件数です。 </param>
        /// <returns> 算出したステージランクです。 </returns>
        public static StageRank Calculate(int achievedCount)
        {
            if (achievedCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(achievedCount));
            }

            return achievedCount switch
            {
                0 => StageRank.C,
                1 => StageRank.B,
                2 => StageRank.A,
                _ => StageRank.S,
            };
        }
    }
}
