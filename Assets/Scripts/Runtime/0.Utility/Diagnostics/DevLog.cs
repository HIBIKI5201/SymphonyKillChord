using System.Diagnostics;

namespace KillChord.Runtime.Utility.Diagnostics
{
    /// <summary>
    ///     開発中だけ出力する情報ログ。
    ///     エディタと Development Build 以外では呼び出し自体が除去されるため、引数の文字列補間も評価されない。
    ///     警告とエラーはリリースビルドでも必要なため、<see cref="UnityEngine.Debug"/> を直接使う。
    /// </summary>
    public static class DevLog
    {
        /// <summary>
        ///     情報ログを出力する。
        /// </summary>
        /// <param name="message"> 出力するメッセージ。 </param>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <summary>
        ///     情報ログを、コンテキストのオブジェクトを付けて出力する。
        /// </summary>
        /// <param name="message"> 出力するメッセージ。 </param>
        /// <param name="context"> ログを選択したときにハイライトするオブジェクト。 </param>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }
    }
}
