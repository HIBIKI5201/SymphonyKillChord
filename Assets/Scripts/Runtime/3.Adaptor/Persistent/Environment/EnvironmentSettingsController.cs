using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Domain.Persistent.Savedata;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     UIから受け取った環境設定をDomainと各適用先へ反映するController。
    ///     変更内容は即座にプレビュー適用するが、保存は確定操作があるまで行わない。
    /// </summary>
    public sealed class EnvironmentSettingsController : IEnvironmentSettingsCommand
    {
        /// <summary>
        ///     環境設定Controllerを初期化する。
        /// </summary>
        public EnvironmentSettingsController(
            EnvironmentSettingsData initialSettings,
            EnvironmentSettingsService environmentSettingsService,
            EnvironmentSettingsPresenter environmentSettingsPresenter,
            IResolutionApplier resolutionApplier,
            IQualityApplier qualityApplier,
            IBrightnessApplier brightnessApplier)
        {
            if (initialSettings == null)
            {
                throw new ArgumentNullException(nameof(initialSettings));
            }

            _committedSettings = initialSettings.Copy();
            _workingSettings = initialSettings.Copy();
            _environmentSettingsService = environmentSettingsService
                ?? throw new ArgumentNullException(nameof(environmentSettingsService));
            _environmentSettingsPresenter = environmentSettingsPresenter
                ?? throw new ArgumentNullException(nameof(environmentSettingsPresenter));
            _resolutionApplier = resolutionApplier
                ?? throw new ArgumentNullException(nameof(resolutionApplier));
            _qualityApplier = qualityApplier
                ?? throw new ArgumentNullException(nameof(qualityApplier));
            _brightnessApplier = brightnessApplier
                ?? throw new ArgumentNullException(nameof(brightnessApplier));

            _availableResolutions = _resolutionApplier.GetAvailableResolutions();
            _availableQualityLevels = _qualityApplier.GetAvailableQualityLevels();

            ApplyToDevice(_workingSettings);
        }

        /// <summary>
        ///     解像度を前後に切り替える。プレビュー適用のみ行い、保存はしない。
        /// </summary>
        public void CycleResolution(int direction)
        {
            if (_availableResolutions.Count == 0)
            {
                return;
            }

            int currentIndex = FindResolutionIndex(_workingSettings.ResolutionWidth, _workingSettings.ResolutionHeight);
            int nextIndex = Wrap(currentIndex, direction, _availableResolutions.Count);
            ResolutionOption option = _availableResolutions[nextIndex];

            _workingSettings.SetResolution(option.Width, option.Height, _workingSettings.IsFullScreen);
            _resolutionApplier.Apply(option.Width, option.Height, _workingSettings.IsFullScreen);
            _environmentSettingsPresenter.Push(_workingSettings);
        }

        /// <summary>
        ///     フルスクリーンとウィンドウを切り替える。プレビュー適用のみ行い、保存はしない。
        /// </summary>
        public void ToggleScreenMode()
        {
            _workingSettings.SetResolution(
                _workingSettings.ResolutionWidth,
                _workingSettings.ResolutionHeight,
                !_workingSettings.IsFullScreen);
            _resolutionApplier.Apply(
                _workingSettings.ResolutionWidth,
                _workingSettings.ResolutionHeight,
                _workingSettings.IsFullScreen);
            _environmentSettingsPresenter.Push(_workingSettings);
        }

        /// <summary>
        ///     画質プリセットを前後に切り替える。プレビュー適用のみ行い、保存はしない。
        /// </summary>
        public void CycleQualityLevel(int direction)
        {
            if (_availableQualityLevels.Count == 0)
            {
                return;
            }

            int currentPosition = FindQualityLevelPosition(_workingSettings.QualityLevel);
            int nextPosition = Wrap(currentPosition, direction, _availableQualityLevels.Count);
            QualityLevelOption option = _availableQualityLevels[nextPosition];

            _workingSettings.SetQualityLevel(option.QualityLevelIndex);
            _qualityApplier.Apply(option.QualityLevelIndex);
            _environmentSettingsPresenter.Push(_workingSettings);
        }

        /// <summary>
        ///     画面の明るさを設定する。プレビュー適用のみ行い、保存はしない。
        /// </summary>
        public void SetBrightness(int brightness)
        {
            int previousBrightness = _workingSettings.Brightness;
            _workingSettings.SetBrightness(brightness);
            if (previousBrightness == _workingSettings.Brightness)
            {
                return;
            }

            _brightnessApplier.SetBrightness(ToNormalizedBrightness(_workingSettings.Brightness));
            _environmentSettingsPresenter.Push(_workingSettings);
        }

        /// <summary>
        ///     すべての環境設定を既定値へ戻す。プレビュー適用のみ行い、保存はしない。
        /// </summary>
        public void ResetToDefaults()
        {
            _workingSettings.SetResolution(
                EnvironmentSettingsData.DEFAULT_RESOLUTION_WIDTH,
                EnvironmentSettingsData.DEFAULT_RESOLUTION_HEIGHT,
                EnvironmentSettingsData.DEFAULT_IS_FULL_SCREEN);
            _workingSettings.SetQualityLevel(EnvironmentSettingsData.DEFAULT_QUALITY_LEVEL);
            _workingSettings.SetBrightness(EnvironmentSettingsData.DEFAULT_BRIGHTNESS);
            ApplyToDevice(_workingSettings);
            _environmentSettingsPresenter.Push(_workingSettings);
        }

        /// <summary>
        ///     プレビュー中の変更を保存として確定する。
        /// </summary>
        public void ConfirmChanges()
        {
            _committedSettings.SetResolution(
                _workingSettings.ResolutionWidth,
                _workingSettings.ResolutionHeight,
                _workingSettings.IsFullScreen);
            _committedSettings.SetQualityLevel(_workingSettings.QualityLevel);
            _committedSettings.SetBrightness(_workingSettings.Brightness);
            _environmentSettingsService.QueueSave(_committedSettings);
        }

        /// <summary>
        ///     プレビュー中の変更を破棄し、直前に保存された状態へ戻す。
        /// </summary>
        public void CancelChanges()
        {
            _workingSettings.SetResolution(
                _committedSettings.ResolutionWidth,
                _committedSettings.ResolutionHeight,
                _committedSettings.IsFullScreen);
            _workingSettings.SetQualityLevel(_committedSettings.QualityLevel);
            _workingSettings.SetBrightness(_committedSettings.Brightness);
            ApplyToDevice(_workingSettings);
            _environmentSettingsPresenter.Push(_workingSettings);
        }

        private const float NORMALIZED_BRIGHTNESS_SCALE = 0.1f;

        private readonly EnvironmentSettingsData _committedSettings;
        private readonly EnvironmentSettingsData _workingSettings;
        private readonly EnvironmentSettingsService _environmentSettingsService;
        private readonly EnvironmentSettingsPresenter _environmentSettingsPresenter;
        private readonly IResolutionApplier _resolutionApplier;
        private readonly IQualityApplier _qualityApplier;
        private readonly IBrightnessApplier _brightnessApplier;
        private readonly IReadOnlyList<ResolutionOption> _availableResolutions;
        private readonly IReadOnlyList<QualityLevelOption> _availableQualityLevels;

        /// <summary>
        ///     指定した環境設定を各適用先へ反映する。
        /// </summary>
        private void ApplyToDevice(EnvironmentSettingsData settings)
        {
            _resolutionApplier.Apply(settings.ResolutionWidth, settings.ResolutionHeight, settings.IsFullScreen);
            _qualityApplier.Apply(settings.QualityLevel);
            _brightnessApplier.SetBrightness(ToNormalizedBrightness(settings.Brightness));
        }

        /// <summary>
        ///     現在の解像度と一致する選択肢のインデックスを探す。見つからない場合は先頭を返す。
        /// </summary>
        private int FindResolutionIndex(int width, int height)
        {
            for (int i = 0; i < _availableResolutions.Count; i++)
            {
                if (_availableResolutions[i].Width == width && _availableResolutions[i].Height == height)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        ///     現在の画質プリセットと一致する選択肢の位置を探す。見つからない場合は先頭を返す。
        /// </summary>
        private int FindQualityLevelPosition(int qualityLevelIndex)
        {
            for (int i = 0; i < _availableQualityLevels.Count; i++)
            {
                if (_availableQualityLevels[i].QualityLevelIndex == qualityLevelIndex)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        ///     インデックスを指定方向へ1つ進め、範囲外は循環させる。
        /// </summary>
        private static int Wrap(int index, int direction, int count)
        {
            return ((index + direction) % count + count) % count;
        }

        /// <summary>
        ///     0～10の表示値を0～1の正規化した明るさへ変換する。
        /// </summary>
        private static float ToNormalizedBrightness(int brightness)
        {
            return Mathf.Clamp(brightness, EnvironmentSettingsData.MIN_BRIGHTNESS, EnvironmentSettingsData.MAX_BRIGHTNESS)
                * NORMALIZED_BRIGHTNESS_SCALE;
        }
    }
}
