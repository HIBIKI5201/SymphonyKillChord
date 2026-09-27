using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     SaveStoreを使用して環境設定を永続化するリポジトリ。
    /// </summary>
    public sealed class EnvironmentSettingsRepository : IEnvironmentSettingsRepository
    {
        /// <summary>
        ///     環境設定リポジトリを初期化する。
        /// </summary>
        /// <param name="defaultAsset"> セーブデータが存在しない初回起動時に適用する既定値。未指定の場合はDomainの既定値を使用する。 </param>
        public EnvironmentSettingsRepository(EnvironmentSettingsDefaultAsset defaultAsset = null)
        {
            _defaultAsset = defaultAsset;
        }

        /// <summary>
        ///     保存済みの環境設定を読み込む。セーブデータが存在しない場合は既定値アセットの内容で初期化する。
        /// </summary>
        public async ValueTask<EnvironmentSettingsData> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            bool hasExistingSave = SaveStore.Exists<SaveData>();
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>(cancellationToken);

            if (!hasExistingSave && _defaultAsset != null)
            {
                EnvironmentSettingsData defaults = _defaultAsset.ToEnvironmentSettingsData();

                // 初回起動時は、アセットの固定値ではなく実際のモニタ解像度を優先する。
                // Screen.currentResolutionが0x0など不正な値を返した場合のみアセットの値へフォールバックする。
                Resolution currentResolution = Screen.currentResolution;
                bool hasValidCurrentResolution = currentResolution.width > 0 && currentResolution.height > 0;
                int resolutionWidth = hasValidCurrentResolution ? currentResolution.width : defaults.ResolutionWidth;
                int resolutionHeight = hasValidCurrentResolution ? currentResolution.height : defaults.ResolutionHeight;

                saveData.EnvironmentSettings.SetResolution(
                    resolutionWidth,
                    resolutionHeight,
                    defaults.IsFullScreen);
                saveData.EnvironmentSettings.SetQualityLevel(defaults.QualityLevel);
                saveData.EnvironmentSettings.SetBrightness(defaults.Brightness);
                saveData.EnvironmentSettings.SetLanguage(defaults.Language);
                saveData.EnvironmentSettings.SetVibrationStrength(defaults.VibrationStrength);
                saveData.EnvironmentSettings.SetRhythmOffsetSeconds(defaults.RhythmOffsetSeconds);
                saveData.EnvironmentSettings.SetCameraSensitivity(defaults.CameraSensitivity);
                saveData.EnvironmentSettings.SetCameraInvertMode(defaults.CameraInvertMode);
                saveData.EnvironmentSettings.SetAutoLockOnEnabled(defaults.IsAutoLockOnEnabled);
            }

            return saveData.EnvironmentSettings.Copy();
        }

        /// <summary>
        ///     指定した環境設定を保存する。
        ///     保存に失敗した場合はSaveStoreのキャッシュを変更前へ戻す。
        /// </summary>
        public async ValueTask SaveAsync(
            EnvironmentSettingsData environmentSettings,
            CancellationToken cancellationToken = default)
        {
            if (environmentSettings == null)
            {
                throw new ArgumentNullException(nameof(environmentSettings));
            }

            // セーブデータを取得し、失敗時に戻せるよう現在の設定を控えておく。
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>(cancellationToken);
            EnvironmentSettingsData previousSettings = saveData.EnvironmentSettings.Copy();

            // 新しい設定を反映する。
            saveData.EnvironmentSettings.SetResolution(
                environmentSettings.ResolutionWidth,
                environmentSettings.ResolutionHeight,
                environmentSettings.IsFullScreen);
            saveData.EnvironmentSettings.SetQualityLevel(environmentSettings.QualityLevel);
            saveData.EnvironmentSettings.SetBrightness(environmentSettings.Brightness);
            saveData.EnvironmentSettings.SetLanguage(environmentSettings.Language);
            saveData.EnvironmentSettings.SetVibrationStrength(environmentSettings.VibrationStrength);
            saveData.EnvironmentSettings.SetRhythmOffsetSeconds(environmentSettings.RhythmOffsetSeconds);
            saveData.EnvironmentSettings.SetCameraSensitivity(environmentSettings.CameraSensitivity);
            saveData.EnvironmentSettings.SetCameraInvertMode(environmentSettings.CameraInvertMode);
            saveData.EnvironmentSettings.SetAutoLockOnEnabled(environmentSettings.IsAutoLockOnEnabled);

            // 保存に失敗した場合は、反映前の設定に戻してから例外を投げ直す。
            try
            {
                await SaveStore.SaveAsync<SaveData>(cancellationToken);
            }
            catch
            {
                saveData.EnvironmentSettings.SetResolution(
                    previousSettings.ResolutionWidth,
                    previousSettings.ResolutionHeight,
                    previousSettings.IsFullScreen);
                saveData.EnvironmentSettings.SetQualityLevel(previousSettings.QualityLevel);
                saveData.EnvironmentSettings.SetBrightness(previousSettings.Brightness);
                saveData.EnvironmentSettings.SetLanguage(previousSettings.Language);
                saveData.EnvironmentSettings.SetVibrationStrength(previousSettings.VibrationStrength);
                saveData.EnvironmentSettings.SetRhythmOffsetSeconds(previousSettings.RhythmOffsetSeconds);
                saveData.EnvironmentSettings.SetCameraSensitivity(previousSettings.CameraSensitivity);
                saveData.EnvironmentSettings.SetCameraInvertMode(previousSettings.CameraInvertMode);
                saveData.EnvironmentSettings.SetAutoLockOnEnabled(previousSettings.IsAutoLockOnEnabled);
                throw;
            }
        }

        private readonly EnvironmentSettingsDefaultAsset _defaultAsset;
    }
}
