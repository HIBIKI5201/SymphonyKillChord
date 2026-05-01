using KillChord.Runtime.Domain.OutGame.Screen;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面遷移結果を View へ適用する Applicator。
    /// </summary>
    public sealed class ScreenViewApplicator : IScreenTransitionApplicable
    {
        /// <summary>
        ///     Applicator を初期化します。
        /// </summary>
        public ScreenViewApplicator(IScreenViewRegistry screenViewRegistry)
        {
            _screenViewRegistry = screenViewRegistry;
        }

        /// <summary>
        ///     画面遷移結果を適用します。
        /// </summary>
        public Task Apply(in ScreenViewDTO screenViewDTO, CancellationToken token)
        {
            var hideId = screenViewDTO.ScreenToHideId;
            var showId = screenViewDTO.ScreenToShowId;
            return ApplyInternalAsync(hideId, showId, token);
        }

        private async Task ApplyInternalAsync(ScreenId? hideId, ScreenId showId, CancellationToken token)
        {
            if (hideId.HasValue)
            {
                await _screenViewRegistry.Hide(hideId.Value, token);
            }

            await _screenViewRegistry.Show(showId, token);
        }

        private readonly IScreenViewRegistry _screenViewRegistry;
    }
}
