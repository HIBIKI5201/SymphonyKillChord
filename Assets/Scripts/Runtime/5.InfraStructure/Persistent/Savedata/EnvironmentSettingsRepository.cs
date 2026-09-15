using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     SaveStoreを使用して環境設定を永続化するリポジトリ。
    /// </summary>
    public sealed class EnvironmentSettingsRepository : IEnvironmentSettingsRepository
    {
        /// <summary>
        ///     保存済みの環境設定を読み込む。
        /// </summary>
        public async ValueTask<EnvironmentSettingsData> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>(cancellationToken);
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

            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>(cancellationToken);
            EnvironmentSettingsData previousSettings = saveData.EnvironmentSettings.Copy();

            saveData.EnvironmentSettings.SetResolution(
                environmentSettings.ResolutionWidth,
                environmentSettings.ResolutionHeight,
                environmentSettings.IsFullScreen);
            saveData.EnvironmentSettings.SetQualityLevel(environmentSettings.QualityLevel);
            saveData.EnvironmentSettings.SetBrightness(environmentSettings.Brightness);

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
                throw;
            }
        }
    }
}
