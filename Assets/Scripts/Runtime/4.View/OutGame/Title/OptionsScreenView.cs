using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     オプション画面の View クラス。
    /// </summary>
    public class OptionsScreenView : ScreenViewBase
    {
        /// <summary>
        ///    オプション画面の View を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <param name="outGameUIEvent"></param>
        public OptionsScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement);
            RegisterButtonCallbacks();
        }

        /// <summary>
        ///   オプション画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            UnregisterButtonCallbacks();
        }

        private const string BACK_BUTTON_NAME = "BackButton";
        private const string BACK_GROUND_NAME = "BackGround";

        private Button _backButton;
        private VisualElement _backGround;

        /// <summary>
        ///     オプション画面の UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void Initialize(VisualElement rootElement)
        {
            if (rootElement == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(OptionsScreenView)}: Root VisualElementがnullです。");
#endif
                return;
            }

            _backButton = rootElement.Q<Button>(BACK_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {BACK_BUTTON_NAME}が見つかりません。"); 

            _backGround = rootElement.Q<VisualElement>(BACK_GROUND_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {BACK_GROUND_NAME}が見つかりません。");
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            _backButton.clicked += OnBackButtonClicked;

            _backGround.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _backButton.clicked -= OnBackButtonClicked;
            _backGround.UnregisterCallback<PointerDownEvent>(OnPointDownEvent);
        }


        /// <summary>
        ///     戻るボタンが押されたときの処理。
        /// </summary>
        private void OnBackButtonClicked()
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     バックグラウンドが押されたときの処理。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointDownEvent(PointerDownEvent evt)
        {
            // バックグラウンドの子要素が押された場合は処理を行わない
            if (evt.target != evt.currentTarget) { return; }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }
    }
}
