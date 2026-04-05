using Discord.Rest;
using System;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class DateTimeUtility
    {
        public static bool ConvertDateUtcToJst(DateTime? utc, out DateTime jst)
        {
            if (utc == null || !utc.HasValue)
            {
                jst = DateTime.MinValue;
                return false;
            }

            return ConvertDateUtcToJst(utc.Value, out jst);
        }

        /// <summary>
        ///     日本時間の現在を取得。
        /// </summary>
        /// <returns></returns>
        public static DateTime JstNow()
        {
            DateTime now = DateTime.UtcNow;
            ConvertDateUtcToJst(now, out DateTime jst);
            return jst;
        }

        /// <summary>
        ///     UTCからJSTに変換する。変換できない場合はfalseを返す。
        /// </summary>
        /// <param name="utc"></param>
        /// <param name="jst"></param>
        /// <returns></returns>
        public static bool ConvertDateUtcToJst(DateTime utc, out DateTime jst)
        {
            jst = utc.AddHours(9);
            return true;
        }

        /// <summary>
        ///     曜日を取得する。
        /// </summary>
        public static bool IsTodayDayOfWeek(DayOfWeek dayOfWeek)
        {
            DateTime date = JstNow();
            return date.DayOfWeek == dayOfWeek;
        }
    }
}
