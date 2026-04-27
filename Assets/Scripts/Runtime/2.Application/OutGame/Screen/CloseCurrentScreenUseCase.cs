using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     現在開いている画面を閉じるユースケース。
    /// </summary>
    public sealed class CloseCurrentScreenUseCase
    {
        /// <summary>
        ///     ユースケースを初期化します。
        /// </summary>
        public CloseCurrentScreenUseCase(
            IScreenStateRepository screenStateRepository,
            IScreenPresenter screenPresenter)
        {
            _screenStateRepository = screenStateRepository;
            _screenPresenter = screenPresenter;
        }

        /// <summary>
        ///     現在開いている画面を閉じます。
        /// </summary>
        public void Execute()
        {
            ScreenTransitionState transitionState = _screenStateRepository.TransitionState;
            ScreenId? currentScreenId = transitionState.CurrentScreenId;

            if (currentScreenId.HasValue == false)
            {
                return;
            }

            if (!transitionState.TryGoBack(out ScreenId previousScreenId))
            {
                return;
            }

            ScreenTransitionResult result = new(
                currentScreenId,
                previousScreenId,
                clearHistory: false);

            _screenPresenter.Present(result);
        }

        private readonly IScreenPresenter _screenPresenter;
        private readonly IScreenStateRepository _screenStateRepository;
    }
}
