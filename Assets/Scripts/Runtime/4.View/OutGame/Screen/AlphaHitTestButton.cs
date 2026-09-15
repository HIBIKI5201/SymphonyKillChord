using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     背景画像(background-image)のアルファ値を参照して当たり判定を行うButton。
    ///     矩形のレイアウトサイズではなく、実際に表示されている不透明ピクセルの範囲だけがクリックに反応する。
    /// </summary>
    [UxmlElement]
    public partial class AlphaHitTestButton : Button
    {
        /// <summary> 当たり判定とみなすアルファ値の閾値です。 </summary>
        [UxmlAttribute]
        public float AlphaThreshold { get; set; } = DEFAULT_ALPHA_THRESHOLD;

        /// <summary>
        ///     背景画像のアルファ値を参照して、指定座標が不透明ピクセル上にあるかどうかを判定します。
        ///     テクスチャが取得できない、またはRead/Writeが無効な場合は矩形判定にフォールバックします。
        /// </summary>
        /// <param name="localPoint"> 要素ローカル座標(左上原点)の判定対象座標。 </param>
        /// <returns> 不透明ピクセル上であればtrue。 </returns>
        public override bool ContainsPoint(Vector2 localPoint)
        {
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;
            if (localPoint.x < 0.0f || localPoint.y < 0.0f || localPoint.x > width || localPoint.y > height)
            {
                return false;
            }

            Background background = resolvedStyle.backgroundImage;
            Texture2D texture = background.texture != null ? background.texture : background.sprite?.texture;
            if (texture == null || !texture.isReadable || width <= 0.0f || height <= 0.0f)
            {
                return base.ContainsPoint(localPoint);
            }

            float u = Mathf.Clamp01(localPoint.x / width);
            float v = Mathf.Clamp01(1.0f - localPoint.y / height);
            int pixelX = Mathf.Clamp(Mathf.FloorToInt(u * texture.width), 0, texture.width - 1);
            int pixelY = Mathf.Clamp(Mathf.FloorToInt(v * texture.height), 0, texture.height - 1);

            return texture.GetPixel(pixelX, pixelY).a >= AlphaThreshold;
        }

        private const float DEFAULT_ALPHA_THRESHOLD = 0.1f;
    }
}
