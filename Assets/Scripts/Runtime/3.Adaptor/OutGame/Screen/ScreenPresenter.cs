using KillChord.Runtime.Application.OutGame.Screen;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面遷移結果を ViewModel へ橋渡しする Presenter。
    /// </summary>
    public sealed class ScreenPresenter : IScreenPresenter
    {
        /// <summary>
        ///     Presenter を初期化します。
        /// </summary>
        public ScreenPresenter(IScreenTransitionApplicable screenViewModel)
        {
            _screenViewModel = screenViewModel;
        }

        /// <summary>
        ///     画面遷移結果を出力します。
        /// </summary>
        public void Present(ScreenTransitionResult result)
        {
            ScreenViewDTO screenViewDTO = new(
                result.ScreenToHideId,
                result.ScreenToShowId);

            _screenViewModel.Apply(in screenViewDTO);
        }

        private readonly IScreenTransitionApplicable _screenViewModel;
    }
}
