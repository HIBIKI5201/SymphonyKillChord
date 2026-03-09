using System;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal readonly struct EnvironmentVariable
    {
        public readonly string Key { get; }
        public readonly string Value { get; }
        public EnvironmentVariable(string key)
        {
            Key = key;
            Value = Environment.GetEnvironmentVariable(key) ?? string.Empty;
        }

        public static implicit operator string(EnvironmentVariable variable)
        {
            if (string.IsNullOrEmpty(variable.Value))
            {
                throw new InvalidOperationException($"環境変数 {variable.Key} が見つかりませんでした。");
            }
            
            return variable.Value;
        }

        public static implicit operator ulong(EnvironmentVariable variable)
        {
            if (!ulong.TryParse(variable.Value, out ulong result))
            {
                throw new InvalidOperationException($"環境変数 {variable.Key} の値 {variable.Value} を数値に変換できませんでした。");
            }

            return result;
        }
    }
}
