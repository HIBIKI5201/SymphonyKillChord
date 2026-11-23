using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.Diagnostics;
using SymphonyFramework.SaveData;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータを読み込み、旧IDの移行を行うクラス。
    /// </summary>
    [AddComponentMenu("KillChord/Composition/Savedata/SavedataSystemInitializer")]
    public sealed class SavedataSystemInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SavedataSystemInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 10;

        /// <summary>
        ///     保存済みデータを読み込み、旧IDが含まれている場合は統一IDへ移行する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveData saveData = await Manuscript.LoadAsync<SaveData>(cancellationToken);

            if (LegacyDataIdMigration.TryMigrate(saveData))
            {
                await Manuscript.SaveAsync<SaveData>(cancellationToken);
                DevLog.Log($"[{nameof(SavedataSystemInitializer)}] 旧IDを統一IDへ移行しました。", this);
            }

            return true;
        }
    }
}
