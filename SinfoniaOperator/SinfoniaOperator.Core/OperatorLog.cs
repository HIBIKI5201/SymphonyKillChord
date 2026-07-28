using System;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     SinfoniaOperatorのログ出力先を差し替え可能にするクラス。
    ///     既定では標準出力へ書き込む。UnityエディタではDebug.Log等へ差し替える。
    /// </summary>
    public static class OperatorLog
    {
        /// <summary>
        ///     ログの出力先を差し替える。
        /// </summary>
        /// <param name="writer"></param>
        public static void SetWriter(Action<string> writer)
        {
            _writer = writer;
        }

        /// <summary>
        ///     ログを出力する。
        /// </summary>
        /// <param name="message"></param>
        public static void Write(string message)
        {
            _writer?.Invoke(message);
        }

        private static Action<string> _writer = Console.WriteLine;
    }
}
