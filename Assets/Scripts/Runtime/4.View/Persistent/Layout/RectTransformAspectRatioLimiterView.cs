using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Layout
{
    /// <summary>
    ///     uGUI の要素を、親の中央にある 16:9 の範囲を基準に配置し直します。
    ///     <para>
    ///         CanvasScaler の Expand では横長の画面ほど Canvas が横に広がり、
    ///         左右に引き伸ばす設定の要素(テキスト枠など)が横へ引き延ばされます。
    ///         横方向のアンカーを中央の 16:9 の範囲へ写すことで、16:9 の画面と同じ見た目に保ちます。
    ///     </para>
    /// </summary>
    [AddComponentMenu("KillChord/View/Layout/RectTransformAspectRatioLimiterView")]
    [RequireComponent(typeof(RectTransform))]
    public sealed class RectTransformAspectRatioLimiterView : MonoBehaviour
    {
        private const int SIDE_COUNT = 2;

        private RectTransform _rectTransform;
        private Vector2 _originalAnchorMin;
        private Vector2 _originalAnchorMax;
        private Vector2 _appliedParentSize;
        private bool _hasOriginalAnchors;

        /// <summary>
        ///     元のアンカーを記録し、現在の親の大きさに合わせて配置します。
        /// </summary>
        private void OnEnable()
        {
            _rectTransform = (RectTransform)transform;
            _originalAnchorMin = _rectTransform.anchorMin;
            _originalAnchorMax = _rectTransform.anchorMax;
            _hasOriginalAnchors = true;
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
        ///     元のアンカーへ戻します。
        /// </summary>
        private void OnDisable()
        {
            if (!_hasOriginalAnchors || _rectTransform == null)
            {
                return;
            }

            _rectTransform.anchorMin = _originalAnchorMin;
            _rectTransform.anchorMax = _originalAnchorMax;
            _hasOriginalAnchors = false;
        }

        /// <summary>
        ///     親の縦横比から左右の余白を求め、横方向のアンカーを中央の範囲へ写します。
        /// </summary>
        private void Apply()
        {
            if (!_hasOriginalAnchors || _rectTransform.parent is not RectTransform parent)
            {
                return;
            }

            Vector2 parentSize = parent.rect.size;
            if (parentSize == _appliedParentSize)
            {
                return;
            }

            _appliedParentSize = parentSize;
            float insetRatio = UiAspectRatioLimit.GetHorizontalInsetRatio(parentSize.x, parentSize.y);
            float usableRatio = 1f - insetRatio * SIDE_COUNT;

            // 0～1 の横アンカーを、左右の余白を除いた範囲の中へ写す。
            _rectTransform.anchorMin = new Vector2(
                insetRatio + _originalAnchorMin.x * usableRatio, _originalAnchorMin.y);
            _rectTransform.anchorMax = new Vector2(
                insetRatio + _originalAnchorMax.x * usableRatio, _originalAnchorMax.y);
        }
    }
}
