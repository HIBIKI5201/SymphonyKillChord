using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Layout
{
    /// <summary>
    ///     uGUI の画像を、縦横比を保ったまま親の全体を覆うように配置します。
    ///     <para>
    ///         横長の画面では上下を、縦長の画面では左右を切り取ります。
    ///         切り取る量は注目点(手前のキャラクターの上半身)の位置で振り分けます。
    ///     </para>
    /// </summary>
    [AddComponentMenu("KillChord/View/Layout/RectTransformCoverFitterView")]
    [RequireComponent(typeof(RectTransform))]
    public sealed class RectTransformCoverFitterView : MonoBehaviour
    {
        private const float HALF = 0.5f;

        [SerializeField, Tooltip("表示する画像の横幅÷高さ。キービジュアルは 16:9(1.7777778)。")]
        private float _imageAspectRatio = KeyVisualCoverLayout.KEY_VISUAL_ASPECT_RATIO;

        [SerializeField, Tooltip("画像の注目点。画像の左上を(0, 0)、右下を(1, 1)とした位置で、切り取るときにこの位置を画面に残す。")]
        private Vector2 _focus = KeyVisualCoverLayout.KEY_VISUAL_FOCUS;

        private RectTransform _rectTransform;
        private Vector2 _appliedParentSize;

        /// <summary>
        ///     現在の親の大きさに合わせて配置します。
        /// </summary>
        private void OnEnable()
        {
            _rectTransform = (RectTransform)transform;
            _appliedParentSize = Vector2.zero;
            Apply();
        }

        /// <summary>
        ///     親の大きさが変わったときに配置し直します。
        /// </summary>
        private void LateUpdate()
        {
            Apply();
        }

        /// <summary>
        ///     親を覆う矩形を求め、親の中央を基準にした位置と大きさへ反映します。
        /// </summary>
        private void Apply()
        {
            if (_rectTransform == null || _rectTransform.parent is not RectTransform parent)
            {
                return;
            }

            Vector2 parentSize = parent.rect.size;
            if (parentSize == _appliedParentSize)
            {
                return;
            }

            _appliedParentSize = parentSize;
            Rect cover = KeyVisualCoverLayout.Calculate(parentSize, _imageAspectRatio, _focus);

            // 左上原点・下向きの矩形を、親の中央を原点・上向きの位置へ直す。
            Vector2 center = cover.center - parentSize * HALF;
            _rectTransform.anchorMin = new Vector2(HALF, HALF);
            _rectTransform.anchorMax = new Vector2(HALF, HALF);
            _rectTransform.pivot = new Vector2(HALF, HALF);
            _rectTransform.sizeDelta = cover.size;
            _rectTransform.anchoredPosition = new Vector2(center.x, -center.y);
        }
    }
}
