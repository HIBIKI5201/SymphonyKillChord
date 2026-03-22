using System;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class DateTimeUtility
    {
        public static bool ConvertDateUtcToJst(DateTime? utc, out DateTime jst)
        {
            if (utc == null)
            {
                jst = default;
                return false;
            }

            return ConvertDateUtcToJst(utc.Value, out jst);
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
        public static bool IsDayOfWeek(DayOfWeek dayOfWeek)
        {
            DateTime date = DateTime.UtcNow.AddHours(9);
            return date.DayOfWeek == dayOfWeek;
        }
    }
}
