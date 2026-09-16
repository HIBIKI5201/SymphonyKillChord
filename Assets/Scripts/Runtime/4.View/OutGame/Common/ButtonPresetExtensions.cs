using System;
using KillChord.Runtime.View.OutGame.Navigation;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Common
{
    /// <summary>
    ///     「見た目」「アニメーション」「クリック/フォーカス等の挙動」をまとめて組み立てた、
    ///     完成品のボタンを要素へ付与するための拡張メソッド群です。「ボタンの実装(全部)」に属します。
    /// </summary>
    public static class ButtonPresetExtensions
    {
        /// <summary> 見た目(<c>Button.uss</c> の off/on スキン切り替え)を担うUSSクラス名です。 </summary>
        public const string BTN_SKIN_CLASS_NAME = "btn-skin";

        /// <summary> アニメーション(<c>Button.uss</c> のクリック時縮小演出)を担うUSSクラス名です。 </summary>
        public const string BTN_SCALE_FEEDBACK_CLASS_NAME = "btn-scale-feedback";

        /// <summary>
        ///     要素へ、基本ボタンの見た目・アニメーション・クリック/フォーカス挙動を一括で付与します。
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <param name="onActivate"> クリックまたは決定操作で呼び出す処理です。 </param>
        /// <returns> 付与した内容をまとめて解除するための <see cref="IDisposable"/> です。 </returns>
        /// <exception cref="ArgumentNullException"> elementまたはonActivateがnullの場合にスローされます。 </exception>
        public static IDisposable ApplyBasicButtonPreset(this VisualElement element, Action onActivate)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (onActivate == null)
            {
                throw new ArgumentNullException(nameof(onActivate));
            }

            return new BasicButtonPresetRegistration(element, onActivate);
        }

        /// <summary>
        ///     基本ボタンプリセットの付与内容を所有し、一括解除するクラスです。
        /// </summary>
        private sealed class BasicButtonPresetRegistration : IDisposable
        {
            /// <summary>
            ///     見た目・アニメーションのUSSクラスと、クリック/フォーカス挙動を要素へ付与します。
            /// </summary>
            /// <param name="element"> 対象の要素です。 </param>
            /// <param name="onActivate"> クリックまたは決定操作で呼び出す処理です。 </param>
            public BasicButtonPresetRegistration(VisualElement element, Action onActivate)
            {
                _element = element;

                _element.AddToClassList(BTN_SKIN_CLASS_NAME);
                _element.AddToClassList(BTN_SCALE_FEEDBACK_CLASS_NAME);
                _element.MakeNavigable();

                _activationRegistration = _element.RegisterActivation(onActivate);
                _pulseManipulator = _element.EnableButtonPulseAnimation();
            }

            /// <summary>
            ///     付与したクラス・挙動をすべて解除します。
            /// </summary>
            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _activationRegistration.Dispose();
                _element.RemoveManipulator(_pulseManipulator);
                _element.RemoveFromClassList(BTN_SKIN_CLASS_NAME);
                _element.RemoveFromClassList(BTN_SCALE_FEEDBACK_CLASS_NAME);
                _isDisposed = true;
            }

            private readonly VisualElement _element;
            private readonly IDisposable _activationRegistration;
            private readonly IManipulator _pulseManipulator;
            private bool _isDisposed;
        }
    }
}
