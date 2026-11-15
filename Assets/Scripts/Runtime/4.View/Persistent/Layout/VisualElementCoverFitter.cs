using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.Persistent.Layout
{
    /// <summary>
    ///     UI Toolkit の要素を、縦横比を保ったまま画面(パネル)全体を覆うように配置します。
    ///     <para>
    ///         親が 16:9 の範囲に制限されていても、画面の左右・上下の端まで広げます。
    ///         はみ出した分は <see cref="KeyVisualCoverLayout"/> の注目点に合わせて切り取られます。
    ///     </para>
    /// </summary>
    public sealed class VisualElementCoverFitter : IDisposable
    {
        /// <summary>
        ///     画面と親要素の大きさの変化を監視し、要素を画面全体へ広げます。
        /// </summary>
        /// <param name="element"> 画面を覆わせる要素です。親を持つ必要があります。 </param>
        /// <param name="imageAspectRatio"> 要素に表示する画像の横幅÷高さです。 </param>
        /// <param name="focus"> 画像の注目点です。左上を(0, 0)、右下を(1, 1)とします。 </param>
        /// <exception cref="ArgumentNullException"> element が null の場合にスローされます。 </exception>
        public VisualElementCoverFitter(VisualElement element, float imageAspectRatio, Vector2 focus)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _imageAspectRatio = imageAspectRatio;
            _focus = focus;

            _element.style.position = Position.Absolute;
            _element.RegisterCallback<AttachToPanelEvent>(HandleAttachToPanel);
            _element.RegisterCallback<DetachFromPanelEvent>(HandleDetachFromPanel);
            if (_element.panel != null)
            {
                RegisterLayoutCallbacks();
            }
        }

        /// <summary>
        ///     監視を解除します。配置はそのまま残します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            UnregisterLayoutCallbacks();
            _element.UnregisterCallback<AttachToPanelEvent>(HandleAttachToPanel);
            _element.UnregisterCallback<DetachFromPanelEvent>(HandleDetachFromPanel);
            _isDisposed = true;
        }

        private readonly VisualElement _element;
        private readonly float _imageAspectRatio;
        private readonly Vector2 _focus;
        private VisualElement _panelRoot;
        private VisualElement _parent;
        private bool _isDisposed;

        /// <summary>
        ///     パネルへ追加されたときに大きさの監視を始めます。
        /// </summary>
        /// <param name="attachEvent"> 追加のイベントです。 </param>
        private void HandleAttachToPanel(AttachToPanelEvent attachEvent)
        {
            RegisterLayoutCallbacks();
        }

        /// <summary>
        ///     パネルから外されたときに大きさの監視をやめます。
        /// </summary>
        /// <param name="detachEvent"> 取り外しのイベントです。 </param>
        private void HandleDetachFromPanel(DetachFromPanelEvent detachEvent)
        {
            UnregisterLayoutCallbacks();
        }

        /// <summary>
        ///     画面または親の大きさが変わったときに配置し直します。
        /// </summary>
        /// <param name="geometryChangedEvent"> 大きさの変化のイベントです。 </param>
        private void HandleGeometryChanged(GeometryChangedEvent geometryChangedEvent)
        {
            Apply();
        }

        /// <summary>
        ///     パネルの最上位要素と親要素の大きさの変化を監視します。
        /// </summary>
        private void RegisterLayoutCallbacks()
        {
            UnregisterLayoutCallbacks();
            _panelRoot = _element.panel?.visualTree;
            _parent = _element.parent;
            _panelRoot?.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            _parent?.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            Apply();
        }

        /// <summary>
        ///     大きさの変化の監視を解除します。
        /// </summary>
        private void UnregisterLayoutCallbacks()
        {
            _panelRoot?.UnregisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            _parent?.UnregisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            _panelRoot = null;
            _parent = null;
        }

        /// <summary>
        ///     画面全体を覆う矩形を求め、親要素の座標へ直して反映します。
        /// </summary>
        private void Apply()
        {
            if (_panelRoot == null || _parent == null)
            {
                return;
            }

            Rect screenBound = _panelRoot.worldBound;
            if (float.IsNaN(screenBound.width) || float.IsNaN(_parent.worldBound.width))
            {
                return;
            }

            Rect cover = KeyVisualCoverLayout.Calculate(screenBound.size, _imageAspectRatio, _focus);

            // 画面の座標で求めた左上を、親要素の中の座標へ直す。
            Vector2 localTopLeft = _parent.WorldToLocal(screenBound.position + cover.position);
            _element.style.left = localTopLeft.x;
            _element.style.top = localTopLeft.y;
            _element.style.width = cover.width;
            _element.style.height = cover.height;
        }
    }
}
