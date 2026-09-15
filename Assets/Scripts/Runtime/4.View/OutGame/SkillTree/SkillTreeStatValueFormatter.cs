using System;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリー画面で使うステータス数値の表示形式を統一するユーティリティ。
    /// </summary>
    public static class SkillTreeStatValueFormatter
    {
        /// <summary>
        ///     小数点以下を切り捨てた数値を文字列へ変換する。
        /// </summary>
        /// <param name="value"> 変換する値。 </param>
        /// <returns> 小数点以下を切り捨てた文字列。 </returns>
        public static string FormatTruncated(float value)
        {
            return Math.Floor(value).ToString();
        }

        /// <summary>
        ///     比率を小数点以下切り捨てのパーセント文字列へ変換する。
        /// </summary>
        /// <param name="value"> 0から1を基準とした比率。 </param>
        /// <returns> パーセント表記の文字列。 </returns>
        public static string FormatPercentage(float value)
        {
            return $"{Math.Floor(value * 100f)}%";
        }

        /// <summary>
        ///     倍率を小数点2桁の「倍」表記へ変換する。
        /// </summary>
        /// <param name="value"> 倍率。 </param>
        /// <returns> 「倍」表記の文字列。 </returns>
        public static string FormatMultiplier(float value)
        {
            return $"{value:0.00}倍";
        }
    }
}
