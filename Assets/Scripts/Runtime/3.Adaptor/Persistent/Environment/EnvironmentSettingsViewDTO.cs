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
            int brightness,
            string languageLabel,
            string vibrationStrengthLabel,
            float vibrationScale,
            int rhythmOffsetStep,
            string rhythmOffsetLabel,
            float rhythmOffsetSeconds)
        {
            ResolutionLabel = resolutionLabel;
            ScreenModeLabel = screenModeLabel;
            QualityLevelLabel = qualityLevelLabel;
            Brightness = brightness;
            LanguageLabel = languageLabel;
            VibrationStrengthLabel = vibrationStrengthLabel;
            VibrationScale = vibrationScale;
            RhythmOffsetStep = rhythmOffsetStep;
            RhythmOffsetLabel = rhythmOffsetLabel;
            RhythmOffsetSeconds = rhythmOffsetSeconds;
        }

        /// <summary> 解像度の表示ラベル。 </summary>
        public string ResolutionLabel { get; }

        /// <summary> 画面モードの表示ラベル。 </summary>
        public string ScreenModeLabel { get; }

        /// <summary> 画質プリセットの表示ラベル。 </summary>
        public string QualityLevelLabel { get; }

        /// <summary> 画面の明るさ。 </summary>
        public int Brightness { get; }

        /// <summary> 表示言語の表示ラベル。 </summary>
        public string LanguageLabel { get; }

        /// <summary> ゲームパッド振動の強さの表示ラベル。 </summary>
        public string VibrationStrengthLabel { get; }

        /// <summary> ゲームパッド振動へ適用する強さの倍率。 </summary>
        public float VibrationScale { get; }

        /// <summary> リズム判定オフセットの段階（0.05秒刻み、-6～6）。 </summary>
        public int RhythmOffsetStep { get; }

        /// <summary> リズム判定オフセットの表示ラベル。 </summary>
        public string RhythmOffsetLabel { get; }

        /// <summary> リズム判定タイミングへ加算するオフセット秒数。 </summary>
        public float RhythmOffsetSeconds { get; }
    }
}
