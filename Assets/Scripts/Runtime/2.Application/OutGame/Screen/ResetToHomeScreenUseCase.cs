using KillChord.Runtime.Domain.OutGame.Screen;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     ホーム画面へ復帰するユースケース。
    /// </summary>
    public sealed class ResetToHomeScreenUseCase
    {
        /// <summary>
        ///     ユースケースを初期化します。
        /// </summary>
        public ResetToHomeScreenUseCase(
            IScreenStateRepository screenStateRepository,
            IScreenPresenter screenPresenter)
        {
            _screenStateRepository = screenStateRepository;
            _screenPresenter = screenPresenter;
        }

        /// <summary>
        ///     ホーム画面へ復帰します。
        /// </summary>
        public async Task Execute(CancellationToken token)
        {
            ScreenTransitionState transitionState = _screenStateRepository.TransitionState;
            ScreenId? previousScreenId = transitionState.CurrentScreenId;

            transitionState.Reset(ScreenId.Home);

            ScreenTransitionResult result = new(
                previousScreenId,
                ScreenId.Home,
                clearHistory: true);

            await _screenPresenter.Present(result, token);
        }

        private readonly IScreenPresenter _screenPresenter;
        private readonly IScreenStateRepository _screenStateRepository;
    }
}