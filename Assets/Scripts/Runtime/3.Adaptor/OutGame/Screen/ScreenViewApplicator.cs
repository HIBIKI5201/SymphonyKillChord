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

            if (hideId.HasValue)
            {
                _screenViewRegistry.Hide(hideId.Value);
            }

            _screenViewRegistry.Show(showId);
        }

        private readonly IScreenViewRegistry _screenViewRegistry;
    }
}
