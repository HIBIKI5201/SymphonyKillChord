using KillChord.Runtime.Domain.Persistent.Savedata;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.Persistent.Savedata
{
    /// <summary>
    ///     環境設定データの永続化操作を定義するリポジトリインターフェース。
    /// </summary>
    public interface IEnvironmentSettingsRepository
    {
        /// <summary>
        ///     保存済みの環境設定を読み込む。
        /// </summary>
        /// <param name="cancellationToken"> 処理を中止するためのトークン。 </param>
        /// <returns> 読み込んだ環境設定。 </returns>
        ValueTask<EnvironmentSettingsData> LoadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     指定した環境設定を保存する。
        /// </summary>
        /// <param name="environmentSettings"> 保存する環境設定。 </param>
        /// <param name="cancellationToken"> 処理を中止するためのトークン。 </param>
        /// <returns> 保存処理を表す非同期操作。 </returns>
        ValueTask SaveAsync(
            EnvironmentSettingsData environmentSettings,
            CancellationToken cancellationToken = default);
    }
}
