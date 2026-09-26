using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Common
{
    /// <summary>
    ///     発動コマンドの拍子を、色分けした六角形アイコンの行として構築する共通処理。
    ///     正六角形の形状は正六角形スプライト(UI_hexagon)の画像を使い、色は
    ///     Image.tintColor で塗り分ける。
    /// </summary>
    public static class ComboHexRowBuilder
    {
        /// <summary>
        ///     行要素の中身を、指定した色一覧に応じた六角形アイコンへ差し替える。
        /// </summary>
        /// <param name="row"> 六角形を並べる行要素。 </param>
        /// <param name="stepColors"> 発動コマンドの入力順に並んだ色一覧。 </param>
        /// <param name="hexSprite"> 六角形の形状スプライト(UI_hexagon)。 </param>
        /// <param name="hexClassName"> 六角形要素のUSSクラス名。 </param>
        public static void Build(
            VisualElement row,
            Color[] stepColors,
            Sprite hexSprite,
            string hexClassName)
        {
            row.Clear();
            if (stepColors == null)
            {
                return;
            }

            for (int i = 0; i < stepColors.Length; i++)
            {
                row.Add(CreateHex(stepColors[i], hexSprite, hexClassName));
            }
        }

        /// <summary>
        ///     六角形アイコンを1つ生成する。
        /// </summary>
        private static VisualElement CreateHex(Color color, Sprite hexSprite, string hexClassName)
        {
            Image hex = new()
            {
                sprite = hexSprite,
                tintColor = color,
                scaleMode = ScaleMode.ScaleToFit,
            };
            hex.AddToClassList(hexClassName);
            return hex;
        }
    }
}
