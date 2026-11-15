using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Layout
{
    /// <summary>
    ///     キービジュアルを縦横比を保ったまま画面いっぱいに広げる配置を求めます。
    ///     <para>
    ///         画面が画像より横長のときは上下を、縦長のときは左右を切り取ります。
    ///         切り取る量は注目点(手前のキャラクターの上半身)の位置で上下・左右に振り分け、
    ///         注目点が画面内の同じ割合の位置に残るようにします。
    ///     </para>
    /// </summary>
    public static class KeyVisualCoverLayout
    {
        /// <summary> キービジュアル画像(1920x1080)の横幅÷高さです。 </summary>
        public const float KEY_VISUAL_ASPECT_RATIO = 16f / 9f;

        /// <summary> キービジュアルの注目点です。画像の左上を(0, 0)、右下を(1, 1)とした手前のキャラクターの上半身の位置です。 </summary>
        public static readonly Vector2 KEY_VISUAL_FOCUS = new(0.19f, 0.38f);

        /// <summary>
        ///     画面を覆う画像の位置と大きさを求めます。
        /// </summary>
        /// <param name="containerSize"> 覆う範囲(画面)の大きさです。 </param>
        /// <param name="imageAspectRatio"> 画像の横幅÷高さです。 </param>
        /// <param name="focus"> 画像の注目点です。左上を(0, 0)、右下を(1, 1)とします。 </param>
        /// <returns> 覆う範囲の左上を原点、下向きを正とした画像の矩形です。 </returns>
        public static Rect Calculate(Vector2 containerSize, float imageAspectRatio, Vector2 focus)
        {
            if (containerSize.x <= 0f || containerSize.y <= 0f || imageAspectRatio <= 0f)
            {
                return new Rect(Vector2.zero, containerSize);
            }

            // 縦横比を保ち、幅と高さのどちらも覆う大きさにする。
            float width = Mathf.Max(containerSize.x, containerSize.y * imageAspectRatio);
            float height = width / imageAspectRatio;

            // はみ出した分を注目点の割合で振り分け、注目点を画面内の同じ割合の位置に残す。
            float x = (containerSize.x - width) * Mathf.Clamp01(focus.x);
            float y = (containerSize.y - height) * Mathf.Clamp01(focus.y);
            return new Rect(x, y, width, height);
        }
    }
}
