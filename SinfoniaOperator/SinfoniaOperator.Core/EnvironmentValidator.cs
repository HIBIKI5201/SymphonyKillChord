namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     設定値（JSON設定または環境変数）が設定されているかを検証するクラス。
    /// </summary>
    public static class EnvironmentValidator
    {
        /// <summary>
        ///     設定値が全て設定されているかを検証する。
        ///     設定されていない物がある場合はtrueを返す。
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        public static bool Validate(params EnvironmentVariable[] variables)
        {
            bool hasError = false;
            foreach (EnvironmentVariable variable in variables)
            {
                if (string.IsNullOrEmpty(variable.Value))
                {
                    OperatorLog.Write($"設定値 {variable.Key} が見つかりませんでした。(JSON設定または環境変数)");
                    hasError = true;
                }
            }
            return hasError;
        }
    }
}
