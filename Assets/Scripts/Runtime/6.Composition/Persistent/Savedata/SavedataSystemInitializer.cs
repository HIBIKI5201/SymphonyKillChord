using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Savedata
{
    /// <summary>
    ///     セーブシステムの初期化クラス。
    /// </summary>
    public sealed class SavedataSystemInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SavedataSystemInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 10;

        /// <summary>
        ///     セーブシステムを生成する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Init()
        {
            _savedataSystem = new SavedataSystem();
            return true;
        }

        /// <summary>
        ///     保存済みデータを読み込み、旧IDが含まれている場合は統一IDへ移行する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            cancellationToken.ThrowIfCancellationRequested();

            if (LegacyDataIdMigration.TryMigrate(saveData))
            {
                await _savedataSystem.SaveAsync(saveData);
                Debug.Log($"[{nameof(SavedataSystemInitializer)}] 旧IDを統一IDへ移行しました。", this);
            }

            return true;
        }

        /// <summary>
        ///     セーブシステムを生成して登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            ServiceLocator.RegisterInstance(_savedataSystem);
            return true;
        }

        /// <summary>
        ///     登録済みセーブシステムを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (ServiceLocator.TryGetInstance(out SavedataSystem registeredSavedataSystem)
                && ReferenceEquals(registeredSavedataSystem, _savedataSystem))
            {
                ServiceLocator.UnregisterInstance<SavedataSystem>();
            }

            _savedataSystem = null;
        }

        private SavedataSystem _savedataSystem;
    }
}
