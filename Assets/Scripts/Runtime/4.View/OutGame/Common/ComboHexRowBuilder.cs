using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Common
{
    /// <summary>
    ///     発動コマンドの拍子を、色分けした六角形アイコンの行として構築する共通処理。
    ///     UI Toolkit には多角形の直接指定がないため、上下の三角形(枠線トリック)+中央の矩形の
    ///     3要素を同色で組み合わせて六角形を表現する。
    /// </summary>
    public static class ComboHexRowBuilder
    {
        /// <summary>
        ///     行要素の中身を、指定した色一覧に応じた六角形アイコンへ差し替える。
        /// </summary>
        /// <param name="row"> 六角形を並べる行要素。 </param>
        /// <param name="stepColors"> 発動コマンドの入力順に並んだ色一覧。 </param>
        /// <param name="hexClassName"> 六角形コンテナのUSSクラス名。 </param>
        /// <param name="capTopClassName"> 上部キャップのUSSクラス名。 </param>
        /// <param name="rectClassName"> 中央矩形のUSSクラス名。 </param>
        /// <param name="capBottomClassName"> 下部キャップのUSSクラス名。 </param>
        public static void Build(
            VisualElement row,
            Color[] stepColors,
            string hexClassName,
            string capTopClassName,
            string rectClassName,
            string capBottomClassName)
        {
            row.Clear();
            if (stepColors == null)
            {
                return;
            }

            for (int i = 0; i < stepColors.Length; i++)
            {
                row.Add(CreateHex(stepColors[i], hexClassName, capTopClassName, rectClassName, capBottomClassName));
            }
        }

        /// <summary>
        ///     六角形アイコンを1つ生成する。
        /// </summary>
        private static VisualElement CreateHex(
            Color color,
            string hexClassName,
            string capTopClassName,
            string rectClassName,
            string capBottomClassName)
        {
            VisualElement hex = new();
            hex.AddToClassList(hexClassName);

            VisualElement capTop = new();
            capTop.AddToClassList(capTopClassName);
            capTop.style.borderBottomColor = color;

            VisualElement rect = new();
            rect.AddToClassList(rectClassName);
            rect.style.backgroundColor = color;

            VisualElement capBottom = new();
            capBottom.AddToClassList(capBottomClassName);
            capBottom.style.borderTopColor = color;

            hex.Add(capTop);
            hex.Add(rect);
            hex.Add(capBottom);
            return hex;
        }
    }
}
