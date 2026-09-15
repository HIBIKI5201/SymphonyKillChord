namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     環境設定の表示値をViewModelへ渡すDTO。
    /// </summary>
    public readonly ref struct EnvironmentSettingsViewDTO
    {
        /// <summary>
        ///     環境設定の表示値を初期化する。
        /// </summary>
        public EnvironmentSettingsViewDTO(
            string resolutionLabel,
            string screenModeLabel,
            string qualityLevelLabel,
            int brightness)
        {
            ResolutionLabel = resolutionLabel;
            ScreenModeLabel = screenModeLabel;
            QualityLevelLabel = qualityLevelLabel;
            Brightness = brightness;
        }

        /// <summary> 解像度の表示ラベル。 </summary>
        public string ResolutionLabel { get; }

        /// <summary> 画面モードの表示ラベル。 </summary>
        public string ScreenModeLabel { get; }

        /// <summary> 画質プリセットの表示ラベル。 </summary>
        public string QualityLevelLabel { get; }

        /// <summary> 画面の明るさ。 </summary>
        public int Brightness { get; }
    }
}
