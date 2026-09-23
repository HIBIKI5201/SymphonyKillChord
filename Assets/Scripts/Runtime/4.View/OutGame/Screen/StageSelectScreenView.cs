using KillChord.Runtime.View.OutGame.Navigation;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     作戦画面 View。
    /// </summary>
    public sealed class StageSelectScreenView : ScreenViewBase
    {

        /// <summary> View を初期化します。 </summary>
        public StageSelectScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACKBUTTON_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageSelectScreenView)}] {BACKBUTTON_NAME} が見つかりませんでした。");

            RegisterButtonCallback();
        }

        /// <summary>
        ///     画面を表示状態にします。フェード完了後に OnStageSelectScreenCompleted を発火します。
        /// </summary>
        public override async ValueTask Show(CancellationToken cancellationToken = default)
        {
            try
            {
                await base.Show(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            OutGameUIEvent.OnStageSelectScreenCompleted?.Invoke();
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
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButtonActivation?.Dispose();
        }

        /// <summary>
        ///     画面を閉じるボタンが作動したときの処理です。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        private const string BACKBUTTON_NAME = "BackButton";

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        /// <inheritdoc />
        /// <remarks> 起点ノード(マップ最左)が無い場合は戻るボタンへフォールバックします。 </remarks>
        protected override VisualElement InitialFocusElement =>
            RootElement.Q<VisualElement>(className: UINavigationExtensions.INITIAL_FOCUS_CLASS_NAME)
            ?? _backButton;

        private readonly Button _backButton;
        private IDisposable _backButtonActivation;
    }
}
