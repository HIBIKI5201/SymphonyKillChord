using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.Persistent.Layout
{
    /// <summary>
    ///     UIDocument のルート要素を、画面中央の 16:9 の範囲に収めます。
    ///     <para>
    ///         PanelSettings の Scale With Screen Size は画面全体へ広げるため、
    ///         19.5:9 などの横長の画面では割合で指定した幅のUIが横へ引き延ばされます。
    ///         ルート要素の左右に余白を付け、基準解像度(1920x1080)と同じ縦横比で配置します。
    ///     </para>
    /// </summary>
    public sealed class UIDocumentAspectRatioLimiter : IDisposable
    {
        /// <summary>
        ///     ルート要素の大きさの変化を監視し、現在の画面に合わせて範囲を制限します。
        /// </summary>
        /// <param name="root"> UIDocument のルート要素です。 </param>
        /// <exception cref="ArgumentNullException"> root が null の場合にスローされます。 </exception>
        public UIDocumentAspectRatioLimiter(VisualElement root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _root.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            Apply();
        }

        /// <summary>
        ///     監視を解除します。付けた余白はそのまま残します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _root.UnregisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            _isDisposed = true;
        }

        private const float PERCENT = 100f;
        private const float UNAPPLIED_INSET = -1f;

        private readonly VisualElement _root;
        private float _appliedInsetRatio = UNAPPLIED_INSET;
        private bool _isDisposed;

        /// <summary>
        ///     画面の大きさが変わったときに範囲を合わせ直します。
        /// </summary>
        /// <param name="geometryChangedEvent"> 大きさの変化のイベントです。 </param>
        private void HandleGeometryChanged(GeometryChangedEvent geometryChangedEvent)
        {
            Apply();
        }

        /// <summary>
        ///     画面の縦横比から左右の余白を求め、ルート要素へ反映します。
        /// </summary>
        private void Apply()
        {
            float insetRatio = UiAspectRatioLimit.GetHorizontalInsetRatio(Screen.width, Screen.height);

            // 自身の変更でも大きさの変化が通知されるため、値が同じなら何もしない。
            if (Mathf.Approximately(insetRatio, _appliedInsetRatio))
            {
                return;
            }

            _appliedInsetRatio = insetRatio;

            // 左右の余白は割合で指定し、高さは画面全体のままにする。
            Length inset = Length.Percent(insetRatio * PERCENT);
            _root.style.position = Position.Absolute;
            _root.style.top = 0f;
            _root.style.bottom = 0f;
            _root.style.left = inset;
            _root.style.right = inset;
        }
    }
}
