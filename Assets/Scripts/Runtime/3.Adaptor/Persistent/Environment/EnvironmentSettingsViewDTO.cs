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
            float rhythmOffsetSeconds,
            int cameraSensitivity,
            float cameraSensitivityScale,
            string cameraInvertModeLabel,
            bool isCameraInvertVertical,
            bool isCameraInvertHorizontal,
            string autoLockOnLabel,
            bool isAutoLockOnEnabled,
            string buttonLayoutLabel,
            bool isJapaneseButtonLayout)
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
            CameraSensitivity = cameraSensitivity;
            CameraSensitivityScale = cameraSensitivityScale;
            CameraInvertModeLabel = cameraInvertModeLabel;
            IsCameraInvertVertical = isCameraInvertVertical;
            IsCameraInvertHorizontal = isCameraInvertHorizontal;
            AutoLockOnLabel = autoLockOnLabel;
            IsAutoLockOnEnabled = isAutoLockOnEnabled;
            ButtonLayoutLabel = buttonLayoutLabel;
            IsJapaneseButtonLayout = isJapaneseButtonLayout;
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

        /// <summary> カメラ感度（1～10）。 </summary>
        public int CameraSensitivity { get; }

        /// <summary> カメラ入力へ掛ける感度の倍率。 </summary>
        public float CameraSensitivityScale { get; }

        /// <summary> カメラ操作の反転方向の表示ラベル。 </summary>
        public string CameraInvertModeLabel { get; }

        /// <summary> カメラの上下操作を反転するかどうか。 </summary>
        public bool IsCameraInvertVertical { get; }

        /// <summary> カメラの左右操作を反転するかどうか。 </summary>
        public bool IsCameraInvertHorizontal { get; }

        /// <summary> オートロックオンの表示ラベル。 </summary>
        public string AutoLockOnLabel { get; }

        /// <summary> 攻撃時のオートロックオンを使うかどうか。 </summary>
        public bool IsAutoLockOnEnabled { get; }

        /// <summary> ゲームパッドの決定・キャンセルの配置の表示ラベル。 </summary>
        public string ButtonLayoutLabel { get; }

        /// <summary> ゲームパッドの決定・キャンセルが日本式かどうか。 </summary>
        public bool IsJapaneseButtonLayout { get; }
    }
}
