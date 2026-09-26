using KillChord.Runtime.Utility.Diagnostics;
using System.Diagnostics;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     コントローラー操作の不具合調査用に、フォーカス移動と決定/キャンセル操作の流れを
    ///     Unityコンソールへ出力する一時的な診断ログです。調査が終わったら削除してください。
    /// </summary>
    public static class NavigationDebugLog
    {
        /// <summary>
        ///     診断メッセージを出力します。エディタ以外のビルドでは呼び出し自体が除去されます。
        /// </summary>
        /// <param name="message"> 出力するメッセージです。 </param>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            DevLog.Log($"[NavDebug] {message}");
        }

        /// <summary>
        ///     要素を型名と要素名で簡潔に表します。
        /// </summary>
        /// <param name="element"> 対象の要素です。nullの場合は"null"を返します。 </param>
        public static string Describe(VisualElement element)
        {
            if (element == null)
            {
                return "null";
            }

            string name = string.IsNullOrEmpty(element.name) ? "(no name)" : element.name;
            return $"{element.GetType().Name}#{name}";
        }
    }
}
