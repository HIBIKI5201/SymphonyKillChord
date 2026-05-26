using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     戦闘準備画面 View。
    /// </summary>
    public class BattlePreparationScreen : ScreenViewBase
    {
        /// <summary> View を初期化します。 </summary>
        public BattlePreparationScreen(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACKBUTTON_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(BattlePreparationScreen)}] {BACKBUTTON_NAME} が見つかりませんでした。");

            _startButton = rootElement.Q<Button>(STARTBUTTON_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(BattlePreparationScreen)}] {STARTBUTTON_NAME} が見つかりませんでした。");

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
            _startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButton.UnregisterCallback<ClickEvent>(OnBackButtonClicked);
            _startButton.UnregisterCallback<ClickEvent>(OnStartButtonClicked);
        }

        /// <summary>
        ///     画面を閉じるボタンがクリックされたときの処理です。
        /// </summary>
        private void OnBackButtonClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     ゲーム開始ボタンがクリックされたときの処理です。
        /// </summary>
        private void OnStartButtonClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnStartGame?.Invoke();
        }

        private const string BACKBUTTON_NAME = "BackButton";
        private const string STARTBUTTON_NAME = "StartButton";

        private readonly Button _backButton;
        private readonly Button _startButton;
    }
}
