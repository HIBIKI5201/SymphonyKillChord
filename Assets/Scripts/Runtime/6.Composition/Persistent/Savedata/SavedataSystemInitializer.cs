using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
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
        ///     セーブシステムを生成して登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _savedataSystem = new SavedataSystem();
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
