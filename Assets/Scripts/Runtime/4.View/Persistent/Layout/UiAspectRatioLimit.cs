using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Layout
{
    /// <summary>
    ///     UIを配置する範囲の横長の上限(16:9)を扱います。
    ///     <para>
    ///         19.5:9 などの横長の画面では、画面幅に合わせて広げると UI が横に引き延ばされます。
    ///         画面が上限より横長のときは、中央の 16:9 の範囲に UI を収め、左右を余白にします。
    ///     </para>
    /// </summary>
    public static class UiAspectRatioLimit
    {
        /// <summary> UIを配置する範囲の横幅÷高さの上限です。UIの基準解像度 1920x1080 に合わせます。 </summary>
        public const float MAX_ASPECT_RATIO = 16f / 9f;

        /// <summary>
        ///     画面の大きさから、UIを 16:9 に収めるために左右それぞれへ空ける幅の割合を求めます。
        /// </summary>
        /// <param name="width"> 画面や親要素の幅です。 </param>
        /// <param name="height"> 画面や親要素の高さです。 </param>
        /// <returns> 幅に対する片側の余白の割合(0～0.5)です。上限以下の横長さであれば0です。 </returns>
        public static float GetHorizontalInsetRatio(float width, float height)
        {
            if (width <= 0f || height <= 0f)
            {
                return 0f;
            }

            float limitedWidth = height * MAX_ASPECT_RATIO;
            if (width <= limitedWidth)
            {
                return 0f;
            }

            return Mathf.Clamp01((width - limitedWidth) / width) * HALF;
        }

        private const float HALF = 0.5f;
    }
}
