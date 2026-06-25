using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     タイトルオプション画面の View クラス。
    /// </summary>
    public class TitleOptionScreenView : ScreenViewBase
    {
        /// <summary>
        ///    タイトルオプション画面の View を初期化します。
        /// </summary>
        public TitleOptionScreenView(
            VisualElement rootElement, 
            OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement);
            RegisterButtonCallbacks();
        }

        /// <summary>
        ///   タイトルオプション画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            UnregisterButtonCallbacks();
        }

        private const string OPTION_BUTTON_NAME = "OptionButton";
        private const string CREDIT_BUTTON_NAME = "CreditButton";
        private const string BACK_BUTTON_NAME = "BackButton";

        private Button _optionButton;
        private Button _creditButton;

        private Button _backButton;

        /// <summary>
        ///     タイトルオプション画面の UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void Initialize(VisualElement rootElement)
        {
            if (rootElement == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleOptionScreenView)}: Root VisualElementがnullです。");
#endif
                return;
            }


            _optionButton = rootElement.Q<Button>(OPTION_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(TitleOptionScreenView)}: {OPTION_BUTTON_NAME}が見つかりません。");
            _creditButton = rootElement.Q<Button>(CREDIT_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(TitleOptionScreenView)}: {CREDIT_BUTTON_NAME}が見つかりません。");
            _backButton = rootElement.Q<Button>(BACK_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(TitleOptionScreenView)}: {BACK_BUTTON_NAME}が見つかりません。");
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            _optionButton.clicked += OnOptionButtonClicked;
            _creditButton.clicked += OnCreditButtonClicked;
            _backButton.clicked += OnBackButtonClicked;
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _optionButton.clicked -= OnOptionButtonClicked;
            _creditButton.clicked -= OnCreditButtonClicked;
            _backButton.clicked -= OnBackButtonClicked;
        }

        /// <summary>
        ///     オプションボタンがクリックされたときの処理。
        /// </summary>
        private void OnOptionButtonClicked()
        {
            OutGameUIEvent.OnShowOptionsScreen?.Invoke();
        }

        /// <summary>
        ///     クレジットボタンがクリックされたときの処理。
        /// </summary>
        private void OnCreditButtonClicked()
        {
            OutGameUIEvent.OnShowCreditScreen?.Invoke();
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
