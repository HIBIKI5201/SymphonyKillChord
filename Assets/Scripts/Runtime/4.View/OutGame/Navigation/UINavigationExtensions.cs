using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     UI Toolkit の要素をマウスとコントローラーの両方で操作できるようにする拡張メソッド群です。
    ///     <para>
    ///         UI Toolkit はフォーカスを持つ要素にしか <see cref="NavigationSubmitEvent"/> を配送しないため、
    ///         コントローラー対応には要素を focusable にしたうえで Submit を購読する必要があります。
    ///         クリック経路はそのまま残すため、マウス操作の挙動は変わりません。
    ///     </para>
    /// </summary>
    public static class UINavigationExtensions
    {
        /// <summary> フォーカス可能にした要素へ付与するUSSクラスです。フォーカスリングの装飾に使用します。 </summary>
        public const string NAVIGABLE_CLASS_NAME = "navigable";

        /// <summary>
        ///     画面を開いたときに最初にフォーカスさせる要素へ付与するUSSクラスです。
        ///     <para>
        ///         ノードのように実行時に生成される要素では、生成側がこのクラスを付け、
        ///         画面側はクラス名で引くことで互いに直接参照せずに済みます。
        ///     </para>
        /// </summary>
        public const string INITIAL_FOCUS_CLASS_NAME = "initial-focus";

        /// <summary>
        ///     要素をクリックとコントローラーの決定操作の両方で作動するようにします。
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <param name="onActivate"> クリックまたは決定操作で呼び出す処理です。 </param>
        /// <returns> 呼び出しを連結できるよう、対象の要素をそのまま返します。 </returns>
        /// <exception cref="ArgumentNullException"> いずれかがnullの場合にスローされます。 </exception>
        public static VisualElement RegisterActivation(this VisualElement element, Action onActivate)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (onActivate == null)
            {
                throw new ArgumentNullException(nameof(onActivate));
            }

            element.MakeNavigable();
            element.RegisterCallback<ClickEvent>(_ => onActivate());
            element.RegisterCallback<NavigationSubmitEvent>(navigationEvent =>
            {
                onActivate();
                navigationEvent.StopPropagation();
            });

            return element;
        }

        /// <summary>
        ///     要素をフォーカス可能にし、コントローラーの決定操作をクリックとして扱えるようにします。
        ///     <para>
        ///         既存の <see cref="ClickEvent"/> ハンドラをそのまま利用したい要素に使用します。
        ///         決定操作を受けると自身へ <see cref="ClickEvent"/> を送出するため、
        ///         マウスで押した場合と同じ経路を通ります。
        ///     </para>
        ///     <para>
        ///         Buttonは決定操作を標準でクリックへ変換するため、
        ///         Buttonが渡された場合はフォーカス可能化だけを行い、イベントを追加しません。
        ///     </para>
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <returns> 呼び出しを連結できるよう、対象の要素をそのまま返します。 </returns>
        /// <exception cref="ArgumentNullException"> elementがnullの場合にスローされます。 </exception>
        public static VisualElement EnableSubmitAsClick(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.MakeNavigable();

            // Buttonは標準処理でNavigationSubmitEventをクリックへ変換するため、
            // 追加のClickEventを送ると同じ操作が二重に実行される。
            if (element is Button)
            {
                return element;
            }

            element.RegisterCallback<NavigationSubmitEvent>(navigationEvent =>
            {
                using ClickEvent clickEvent = ClickEvent.GetPooled();
                clickEvent.target = element;
                element.SendEvent(clickEvent);
                navigationEvent.StopPropagation();
            });

            return element;
        }

        /// <summary>
        ///     要素をコントローラーのフォーカス移動の対象にします。
        ///     <para>
        ///         既にフォーカス可能な要素(Buttonなど)にも呼び出せます。USSクラスの付与のみ行います。
        ///     </para>
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <returns> 呼び出しを連結できるよう、対象の要素をそのまま返します。 </returns>
        /// <exception cref="ArgumentNullException"> elementがnullの場合にスローされます。 </exception>
        public static VisualElement MakeNavigable(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.focusable = true;

            // tabIndex が負の要素はフォーカス移動の探索対象から外れる。
            if (element.tabIndex < 0)
            {
                element.tabIndex = 0;
            }

            // フォーカス中の要素はクリックを透過させないようにしておく。
            if (element.pickingMode == PickingMode.Ignore)
            {
                element.pickingMode = PickingMode.Position;
            }

            element.AddToClassList(NAVIGABLE_CLASS_NAME);
            return element;
        }

        /// <summary>
        ///     要素をコントローラーのフォーカス移動の対象から外します。
        ///     <para>
        ///         キャンセル操作で代替できる「戻る」ボタンなど、
        ///         フォーカスを当てる必要のない要素に使用します。
        ///         マウスでのクリックは従来どおり可能です。
        ///     </para>
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <returns> 呼び出しを連結できるよう、対象の要素をそのまま返します。 </returns>
        /// <exception cref="ArgumentNullException"> elementがnullの場合にスローされます。 </exception>
        public static VisualElement ExcludeFromNavigation(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            // Button など既定でフォーカス可能な要素があるため、明示的に無効化する。
            element.focusable = false;
            element.tabIndex = -1;
            element.RemoveFromClassList(NAVIGABLE_CLASS_NAME);
            return element;
        }

        /// <summary>
        ///     レイアウト確定後に要素へフォーカスを移します。
        ///     <para>
        ///         表示直後の要素は同フレームではまだレイアウトされておらず、
        ///         その場で <see cref="Focus"/> を呼んでも無視されることがあります。
        ///         次のレイアウト後まで遅延させることで確実にフォーカスさせます。
        ///     </para>
        /// </summary>
        /// <param name="element"> フォーカスさせる要素です。nullの場合は何もしません。 </param>
        public static void FocusDeferred(this VisualElement element)
        {
            if (element == null || element.panel == null)
            {
                return;
            }

            element.schedule.Execute(() =>
            {
                // 遅延実行までの間に画面が閉じられている場合があるため、都度確認する。
                if (element.panel == null || !element.enabledInHierarchy)
                {
                    return;
                }

                if (element.resolvedStyle.display == DisplayStyle.None)
                {
                    return;
                }

                element.Focus();
            });
        }
    }
}
