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
                environmentSettings.IsFullScreen ? SCREEN_MODE_FULL_SCREEN_KEY : SCREEN_MODE_WINDOWED_KEY,
                _qualityApplier.GetQualityLevelName(environmentSettings.QualityLevel),
                environmentSettings.Brightness,
                GetLanguageLabelKey(environmentSettings.Language),
                GetVibrationStrengthLabelKey(environmentSettings.VibrationStrength),
                GetVibrationScale(environmentSettings.VibrationStrength),
                GetRhythmOffsetStep(environmentSettings.RhythmOffsetSeconds),
                GetRhythmOffsetLabel(environmentSettings.RhythmOffsetSeconds),
                (float)environmentSettings.RhythmOffsetSeconds);
            _environmentSettingsViewModel.Apply(in dto);
        }

        // UI共通テキストのString Table Collection「UICommon」のキー。View層でLocalizedElementTextにより解決される。
        private const string SCREEN_MODE_FULL_SCREEN_KEY = "ui.setting.screen_mode_fullscreen";
        private const string SCREEN_MODE_WINDOWED_KEY = "ui.setting.screen_mode_windowed";
        private const string LANGUAGE_JAPANESE_KEY = "ui.setting.language_japanese";
        private const string LANGUAGE_ENGLISH_KEY = "ui.setting.language_english";
        private const string VIBRATION_STRONG_KEY = "ui.setting.vibration_strong";
        private const string VIBRATION_WEAK_KEY = "ui.setting.vibration_weak";
        private const string VIBRATION_OFF_KEY = "ui.setting.vibration_off";
        private const float VIBRATION_STRONG_SCALE = 1f;
        private const float VIBRATION_WEAK_SCALE = 0.5f;
        private const float VIBRATION_OFF_SCALE = 0f;
        private const string RHYTHM_OFFSET_LABEL_FORMAT = "+0.00;-0.00;0.00";
        private const string RHYTHM_OFFSET_LABEL_SUFFIX = "秒";

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
        ///     表示言語のローカライズキーを取得する。
        /// </summary>
        private static string GetLanguageLabelKey(GameLanguage language)
        {
            return language == GameLanguage.English
                ? LANGUAGE_ENGLISH_KEY
                : LANGUAGE_JAPANESE_KEY;
        }

        /// <summary>
        ///     ゲームパッド振動の強さのローカライズキーを取得する。
        /// </summary>
        private static string GetVibrationStrengthLabelKey(VibrationStrength vibrationStrength)
        {
            return vibrationStrength switch
            {
                VibrationStrength.Weak => VIBRATION_WEAK_KEY,
                VibrationStrength.Off => VIBRATION_OFF_KEY,
                _ => VIBRATION_STRONG_KEY,
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

        /// <summary>
        ///     リズム判定オフセットの秒数を、0.05秒刻みのスライダー段階へ変換する。
        /// </summary>
        private static int GetRhythmOffsetStep(double rhythmOffsetSeconds)
        {
            return (int)System.Math.Round(rhythmOffsetSeconds / EnvironmentSettingsData.RHYTHM_OFFSET_STEP_SECONDS);
        }

        /// <summary>
        ///     リズム判定オフセットの表示ラベルを組み立てる。
        /// </summary>
        private static string GetRhythmOffsetLabel(double rhythmOffsetSeconds)
        {
            return rhythmOffsetSeconds.ToString(RHYTHM_OFFSET_LABEL_FORMAT) + RHYTHM_OFFSET_LABEL_SUFFIX;
        }
    }
}
