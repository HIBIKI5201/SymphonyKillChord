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
                environmentSettings.Brightness);
            _environmentSettingsViewModel.Apply(in dto);
        }

        private const string SCREEN_MODE_FULL_SCREEN_LABEL = "フルスクリーン";
        private const string SCREEN_MODE_WINDOWED_LABEL = "ウィンドウ";

        private readonly IEnvironmentSettingsViewModel _environmentSettingsViewModel;
        private readonly IQualityApplier _qualityApplier;

        /// <summary>
        ///     解像度の表示ラベルを組み立てる。
        /// </summary>
        private static string FormatResolutionLabel(int width, int height)
        {
            return $"{width} x {height}";
        }
    }
}
