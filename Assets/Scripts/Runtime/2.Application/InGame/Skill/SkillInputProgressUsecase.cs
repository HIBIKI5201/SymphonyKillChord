using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     スキル入力の進捗を管理するユースケースクラス。
    /// </summary>
    public class SkillInputProgressUsecase
    {
        /// <summary>
        ///     現在の一致数と入力されたビートタイプに基づいて、次の一致数を計算する。
        /// </summary>
        /// <param name="pattern"> スキルの入力パターン。 </param>
        /// <param name="currentMatchedCount"> 現在の一致数。 </param>
        /// <param name="inputBeatType"> 入力されたビートタイプ。 </param>
        /// <returns> 次の一致数。 </returns>
        public int CalculateNextMatchedCount(
            ReadOnlySpan<BeatType> pattern,
            int currentMatchedCount,
            BeatType inputBeatType)
        {
            if (pattern.Length <= 0)
            {
                return 0;
            }

            if (currentMatchedCount < 0)
            {
                currentMatchedCount = 0;
            }

            // 現在待っているビートタイプと入力されたビートタイプが一致している場合、一致数を1増やす。
            if (currentMatchedCount < pattern.Length &&
                pattern[currentMatchedCount] == inputBeatType)
            {
                return currentMatchedCount + 1;
            }

            // 途中で一致しなくても、最初のビートタイプと入力されたビートタイプが一致している場合は、一致数を1にリセットする。
            if (pattern[0] == inputBeatType)
            {
                return 1;
            }

            return 0;
        }
    }
}
