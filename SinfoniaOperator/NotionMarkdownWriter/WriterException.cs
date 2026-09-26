using System;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     利用者への提示を前提とした、書き込みツールの想定内エラー。
    ///     スタックトレースではなくメッセージだけを表示して終了する。
    /// </summary>
    internal sealed class WriterException : Exception
    {
        /// <summary>
        ///     想定内エラーを生成する。
        /// </summary>
        /// <param name="message">利用者へ表示するメッセージ。</param>
        internal WriterException(string message) : base(message)
        {
        }
    }
}
