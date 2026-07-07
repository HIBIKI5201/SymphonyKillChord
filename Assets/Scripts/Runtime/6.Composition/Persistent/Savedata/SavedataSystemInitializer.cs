using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Savedata
{
    /// <summary>
    ///     セーブシステムの初期化クラス。
    /// </summary>
    public class SavedataSystemInitializer : MonoBehaviour
    {
        private void Awake()
        {
            SavedataSystem savedataSystem = new SavedataSystem();
            ServiceLocator.RegisterInstance(savedataSystem);
        }
    }
}
