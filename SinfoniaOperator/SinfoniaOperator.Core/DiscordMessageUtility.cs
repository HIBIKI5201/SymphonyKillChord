using System.Collections.Generic;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     Discordへのメッセージ送信で共通利用するユーティリティクラス。
    /// </summary>
    internal static class DiscordMessageUtility
    {
        /// <summary> Discordの1メッセージあたりの最大文字数。 </summary>
        public const int MAX_LENGTH = 2000;

        /// <summary>
        ///     2000文字を上限に、できるだけ改行位置でメッセージを分割する。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitContent(string content)
        {
            string remainingContent = content;

            while (remainingContent.Length > MAX_LENGTH)
            {
                // 2000文字以内で最後の改行を探す
                int splitIndex = remainingContent.LastIndexOf('\n', MAX_LENGTH - 1);

                // 改行が見つからない場合は2000文字で強制分割
                if (splitIndex == -1)
                {
                    splitIndex = MAX_LENGTH;
                }
                else
                {
                    // 改行文字そのものを含めて分割する
                    splitIndex++;
                }

                yield return remainingContent.Substring(0, splitIndex);
                remainingContent = remainingContent.Substring(splitIndex);
            }

            if (!string.IsNullOrWhiteSpace(remainingContent))
            {
                yield return remainingContent;
            }
        }
    }
}
