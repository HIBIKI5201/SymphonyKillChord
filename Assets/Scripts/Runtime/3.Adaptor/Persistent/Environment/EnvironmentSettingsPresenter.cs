using KillChord.Runtime.Domain.Persistent.Savedata;
using System;

namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     環境設定を表示用DTOへ変換してViewModelへ反映するPresenter。
    /// </summary>
    public sealed class EnvironmentSettingsPresenter
    {
        /// <summary>
        ///     環境設定Presenterを初期化する。
        /// </summary>
        public EnvironmentSettingsPresenter(
            IEnvironmentSettingsViewModel environmentSettingsViewModel,
            IQualityApplier qualityApplier)
        {
            _environmentSettingsViewModel = environmentSettingsViewModel
                ?? throw new ArgumentNullException(nameof(environmentSettingsViewModel));
            _qualityApplier = qualityApplier
                ?? throw new ArgumentNullException(nameof(qualityApplier));
        }

        /// <summary>
        ///     現在の環境設定をViewModelへ反映する。
        /// </summary>
        public void Push(EnvironmentSettingsData environmentSettings)
        {
            if (environmentSettings == null)
            {
                throw new ArgumentNullException(nameof(environmentSettings));
            }

            EnvironmentSettingsViewDTO dto = new EnvironmentSettingsViewDTO(
                FormatResolutionLabel(environmentSettings.ResolutionWidth, environmentSettings.ResolutionHeight),
                environmentSettings.IsFullScreen ? SCREEN_MODE_FULL_SCREEN_LABEL : SCREEN_MODE_WINDOWED_LABEL,
                _qualityApplier.GetQualityLevelName(environmentSettings.QualityLevel),
                environmentSettings.Brightness,
                GetLanguageLabel(environmentSettings.Language),
                GetVibrationStrengthLabel(environmentSettings.VibrationStrength),
                GetVibrationScale(environmentSettings.VibrationStrength));
            _environmentSettingsViewModel.Apply(in dto);
        }

        private const string SCREEN_MODE_FULL_SCREEN_LABEL = "フルスクリーン";
        private const string SCREEN_MODE_WINDOWED_LABEL = "ウィンドウ";
        private const string LANGUAGE_JAPANESE_LABEL = "日本語";
        private const string LANGUAGE_ENGLISH_LABEL = "英語";
        private const string VIBRATION_STRONG_LABEL = "強い";
        private const string VIBRATION_WEAK_LABEL = "弱い";
        private const string VIBRATION_OFF_LABEL = "オフ";
        private const float VIBRATION_STRONG_SCALE = 1f;
        private const float VIBRATION_WEAK_SCALE = 0.5f;
        private const float VIBRATION_OFF_SCALE = 0f;

        private readonly IEnvironmentSettingsViewModel _environmentSettingsViewModel;
        private readonly IQualityApplier _qualityApplier;

        /// <summary>
        ///     解像度の表示ラベルを組み立てる。
        /// </summary>
        private static string FormatResolutionLabel(int width, int height)
        {
            return $"{width} x {height}";
        }

        /// <summary>
        ///     表示言語のラベルを取得する。
        /// </summary>
        private static string GetLanguageLabel(GameLanguage language)
        {
            return language == GameLanguage.English
                ? LANGUAGE_ENGLISH_LABEL
                : LANGUAGE_JAPANESE_LABEL;
        }

        /// <summary>
        ///     ゲームパッド振動の強さのラベルを取得する。
        /// </summary>
        private static string GetVibrationStrengthLabel(VibrationStrength vibrationStrength)
        {
            return vibrationStrength switch
            {
                VibrationStrength.Weak => VIBRATION_WEAK_LABEL,
                VibrationStrength.Off => VIBRATION_OFF_LABEL,
                _ => VIBRATION_STRONG_LABEL,
            };
        }

        /// <summary>
        ///     ゲームパッド振動へ適用する強さの倍率を取得する。
        /// </summary>
        private static float GetVibrationScale(VibrationStrength vibrationStrength)
        {
            return vibrationStrength switch
            {
                VibrationStrength.Weak => VIBRATION_WEAK_SCALE,
                VibrationStrength.Off => VIBRATION_OFF_SCALE,
                _ => VIBRATION_STRONG_SCALE,
            };
        }
    }
}
