using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     研究画面 View。
    /// </summary>
    public sealed class SkillTreeScreenView : ScreenViewBase
    {
        /// <summary> View を初期化します。 </summary>
        public SkillTreeScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACKBUTTON_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(SkillTreeScreenView)}] {BACKBUTTON_NAME} が見つかりませんでした。");

            RegisterButtonCallback();
        }

        public override void Dispose()
        {
            base.Dispose();
            UnregisterButtonCallback();
        }

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButton.UnregisterCallback<ClickEvent>(OnBackButtonClicked);
        }

        /// <summary>
        ///     画面を閉じるボタンがクリックされたときの処理です。
        /// </summary>
        private void OnBackButtonClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        private const string BACKBUTTON_NAME = "BackButton";

        private readonly Button _backButton;
    }
}
