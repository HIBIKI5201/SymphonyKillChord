using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     SaveStoreを使用して音量設定を永続化するリポジトリ。
    /// </summary>
    public sealed class AudioSettingsRepository : IAudioSettingsRepository
    {
        /// <summary>
        ///     保存済みの音量設定を読み込む。
        /// </summary>
        public async ValueTask<AudioSettingsData> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>(cancellationToken);
            return saveData.AudioSettings.Copy();
        }

        /// <summary>
        ///     指定した音量設定を保存する。
        ///     保存に失敗した場合はSaveStoreのキャッシュを変更前へ戻す。
        /// </summary>
        public async ValueTask SaveAsync(
            AudioSettingsData audioSettings,
            CancellationToken cancellationToken = default)
        {
            if (audioSettings == null)
            {
                throw new ArgumentNullException(nameof(audioSettings));
            }

            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>(cancellationToken);
            AudioSettingsData previousSettings = saveData.AudioSettings.Copy();

            saveData.AudioSettings.SetVolumes(
                audioSettings.BgmVolume,
                audioSettings.SoundEffectVolume,
                audioSettings.VoiceVolume);

            try
            {
                await SaveStore.SaveAsync<SaveData>(cancellationToken);
            }
            catch
            {
                saveData.AudioSettings.SetVolumes(
                    previousSettings.BgmVolume,
                    previousSettings.SoundEffectVolume,
                    previousSettings.VoiceVolume);
                throw;
            }
        }
    }
}
