using KillChord.Runtime.Domain.Persistent.Savedata;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.Persistent.Savedata
{
    /// <summary>
    ///     音量設定データの永続化操作を定義するリポジトリインターフェース。
    /// </summary>
    public interface IAudioSettingsRepository
    {
        /// <summary>
        ///     保存済みの音量設定を読み込む。
        /// </summary>
        /// <param name="cancellationToken"> 処理を中止するためのトークン。 </param>
        /// <returns> 読み込んだ音量設定。 </returns>
        ValueTask<AudioSettingsData> LoadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     指定した音量設定を保存する。
        /// </summary>
        /// <param name="audioSettings"> 保存する音量設定。 </param>
        /// <param name="cancellationToken"> 処理を中止するためのトークン。 </param>
        /// <returns> 保存処理を表す非同期操作。 </returns>
        ValueTask SaveAsync(
            AudioSettingsData audioSettings,
            CancellationToken cancellationToken = default);
    }
}
