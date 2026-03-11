using DevelopProducts.Persistent.Composition;
using Unity.VisualScripting;
using UnityEngine;

namespace DevelopProducts.Persistent.View
{
    public class InGameSceneInitializer : MonoBehaviour
    {
        private void Start()
        {
            PersistentInputInstaller installer = FindFirstObjectByType<PersistentInputInstaller>();

            if (installer == null)
            {
                Debug.LogError("PersistentInputInstaller が見つかりません。");
                return;
            }

            installer.SwichInputMapUseCase.ToInGame();
            installer.InputBufferWriter.Clear();

            Debug.Log("InGameSceneInitializer: ToInGame 実行");
        }
    }
}
