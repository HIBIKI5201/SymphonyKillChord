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
        ///    画面遷移結果を適用します。
        /// </summary>
        /// <param name="screenViewDTO"></param>
        public void Apply(in ScreenViewDTO screenViewDTO)
        {
            var hideId = screenViewDTO.ScreenToHideId;
            var showId = screenViewDTO.ScreenToShowId;
            var targetSceneName = screenViewDTO.TargetSceneName;
            
            if (hideId.HasValue)
            {
                _screenViewRegistry.HideImmediately(hideId.Value);
            }

            _screenViewRegistry.ShowImmediately(showId, targetSceneName);
        }

        /// <summary>
        ///     画面遷移結果を適用します。
        /// </summary>
        public Task Apply(in ScreenViewDTO screenViewDTO, CancellationToken token)
        {
            var hideId = screenViewDTO.ScreenToHideId;
            var showId = screenViewDTO.ScreenToShowId;
            var targetSceneName = screenViewDTO.TargetSceneName;
            return ApplyInternalAsync(hideId, showId, token, targetSceneName);
        }

        private async Task ApplyInternalAsync(ScreenId? hideId, ScreenId showId, CancellationToken token, string targetSceneName = null)
        {
            if (hideId.HasValue)
            {
                await _screenViewRegistry.Hide(hideId.Value, token);
            }

            await _screenViewRegistry.Show(showId, token, targetSceneName);
        }

        private readonly IScreenViewRegistry _screenViewRegistry;
    }
}
