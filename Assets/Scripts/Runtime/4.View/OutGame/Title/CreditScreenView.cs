using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     クレジット画面の View クラス。
    /// </summary>
    public class CreditScreenView : ScreenViewBase
    {
        /// <summary>
        ///    クレジット画面の View を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <param name="outGameUIEvent"></param>
        public CreditScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement);
            RegisterButtonCallbacks();
        }

        /// <summary>
        ///   クレジット画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            UnregisterButtonCallbacks();
        }

        private const string BACK_BUTTON_NAME = "BackButton";

        private Button _backButton;

        /// <summary>
        ///     クレジット画面の UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void Initialize(VisualElement rootElement)
        {
            if (rootElement == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(CreditScreenView)}: Root VisualElementがnullです。");
#endif
                return;
            }

            _backButton = rootElement.Q<Button>(BACK_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {BACK_BUTTON_NAME}が見つかりません。");
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            _backButton.clicked += OnBackButtonClicked;
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _backButton.clicked -= OnBackButtonClicked;
        }


        /// <summary>
        ///     戻るボタンがクリックされたときの処理。
        /// </summary>
        private void OnBackButtonClicked()
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }
    }
}
