using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     仕様書ビューアの色と文字の設定。エディタの明暗テーマに合わせる。
    /// </summary>
    public static class SpecViewerStyles
    {
        /// <summary> リンクの文字色（#RRGGBB）。 </summary>
        public static string LinkColorHex => EditorGUIUtility.isProSkin ? "#6CB4FF" : "#0B5CAD";

        /// <summary> 文中のコードの背景色（#RRGGBBAA）。 </summary>
        public static string InlineCodeBackgroundHex => EditorGUIUtility.isProSkin ? "#FFFFFF22" : "#00000018";

        /// <summary> 枠線の色。 </summary>
        public static Color BorderColor => EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);

        /// <summary> 引用の背景色。 </summary>
        public static Color QuoteBackground => EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.04f) : new Color(0f, 0f, 0f, 0.03f);

        /// <summary> コールアウトの背景色。 </summary>
        public static Color CalloutBackground => EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.07f) : new Color(0f, 0f, 0f, 0.05f);

        /// <summary> コードブロックの背景色。 </summary>
        public static Color CodeBackground => EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.25f) : new Color(0f, 0f, 0f, 0.06f);

        /// <summary> 表の見出し行の背景色。 </summary>
        public static Color TableHeaderBackground => EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.08f) : new Color(0f, 0f, 0f, 0.06f);

        /// <summary>
        ///     要素に等幅フォントを使う。エディタに同梱のフォントが見つからなければ何もしない。
        /// </summary>
        /// <param name="element">対象の要素。</param>
        public static void ApplyMonospaceFont(VisualElement element)
        {
            if (EditorGUIUtility.Load(MONOSPACE_FONT_PATH) is Font font)
            {
                element.style.unityFontDefinition = FontDefinition.FromFont(font);
            }
        }

        private const string MONOSPACE_FONT_PATH = "Fonts/RobotoMono/RobotoMono-Regular.ttf";
    }
}
