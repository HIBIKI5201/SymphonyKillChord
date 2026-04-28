using KillChord.Runtime.Application.OutGame.Screen;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
        public Task Present(ScreenTransitionResult result, CancellationToken token)
        {
            ScreenViewDTO screenViewDTO = new(
                result.ScreenToHideId,
                result.ScreenToShowId);

             return _screenViewModel.Apply(in screenViewDTO, token);
        }

        private readonly IScreenTransitionApplicable _screenViewModel;
    }
}
