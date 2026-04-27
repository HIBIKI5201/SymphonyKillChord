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
        public void Apply(in ScreenViewDTO screenViewDTO)
        {
            if (screenViewDTO.ScreenToHideId.HasValue)
            {
                _screenViewRegistry.Hide(screenViewDTO.ScreenToHideId.Value);
            }

            _screenViewRegistry.Show(screenViewDTO.ScreenToShowId);
        }

        private readonly IScreenViewRegistry _screenViewRegistry;
    }
}
