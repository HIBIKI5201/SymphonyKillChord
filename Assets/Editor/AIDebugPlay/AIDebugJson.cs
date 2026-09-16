using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     QA応答用のプリミティブ値とコレクションをJSONへ変換する。
    /// </summary>
    internal static class AIDebugJson
    {
        /// <summary>
        ///     キーと値の組から応答オブジェクトを作る。
        /// </summary>
        internal static Dictionary<string, object> Object(params (string Key, object Value)[] entries)
        {
            var result = new Dictionary<string, object>();
            foreach (var entry in entries) { result.Add(entry.Key, entry.Value); }
            return result;
        }

        /// <summary>
        ///     未取得と実際のゼロ値を区別した応答を作る。
        /// </summary>
        internal static object Unavailable(string reason)
        {
            return Object(("available", false), ("reason", reason));
        }

        /// <summary>
        ///     例外で他の観測項目を失わないよう、項目ごとに取得する。
        /// </summary>
        internal static object Observe(Func<object> read)
        {
            try { return Object(("available", true), ("value", read())); }
            catch (Exception exception) { return Unavailable(exception.Message); }
        }

        /// <summary>
        ///     任意のゲームオブジェクトを反射走査せず、許可した値だけを書き出す。
        /// </summary>
        internal static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, new FiniteNumberConverter());
        }

        /// <summary>
        ///     未計測のNaNや無限大を、ゼロではなくnullとして出力する。
        /// </summary>
        private sealed class FiniteNumberConverter : JsonConverter
        {
            /// <summary>
            ///     浮動小数点数のみを変換対象とする。
            /// </summary>
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(float) || objectType == typeof(double);
            }

            /// <summary>
            ///     有限数値だけをJSONの数値として書き出す。
            /// </summary>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (double.IsNaN(number) || double.IsInfinity(number)) { writer.WriteNull(); }
                else { writer.WriteValue(number); }
            }

            /// <summary>
            ///     この変換器は書き出し専用のため読み込みを拒否する。
            /// </summary>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }
    }
}
