using System;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     設定値のキーと値を保持する構造体。
    ///     値はOperatorConfig経由（JSON設定または環境変数）で取得する。
    /// </summary>
    public readonly struct EnvironmentVariable
    {
        public EnvironmentVariable(string key)
        {
            Key = key;
            Value = OperatorConfig.GetValue(key);
        }

        public readonly string Key;
        public readonly string Value;

        public static implicit operator string(EnvironmentVariable variable)
        {
            if (string.IsNullOrEmpty(variable.Value))
            {
                throw new InvalidOperationException($"設定値 {variable.Key} が見つかりませんでした。");
            }

            return variable.Value;
        }

        public static implicit operator ulong(EnvironmentVariable variable)
        {
            if (!ulong.TryParse(variable.Value, out ulong result))
            {
                throw new InvalidOperationException($"設定値 {variable.Key} の値 {variable.Value} を数値に変換できませんでした。");
            }

            return result;
        }
    }
}
