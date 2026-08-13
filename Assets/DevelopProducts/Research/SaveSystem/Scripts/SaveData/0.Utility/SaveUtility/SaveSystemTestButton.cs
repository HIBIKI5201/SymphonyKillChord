using SymphonyFrameWork.System.SceneLoad;
using UnityEngine;

namespace DevelopProducts.SaveSystem
{
    public class SaveSystemTestButton : MonoBehaviour
    {
        [SerializeField] private int health = 100;
        public async void Save()
        {
            var playerData = await SaveSystem.LoadAsync<PlayerSaveData>();
            playerData.Health = health;
            await SaveSystem.SaveAsync(playerData);
            Debug.Log($"Playerのデータがセーブされました。HP: {playerData.Health}");
        }
        public async void Load()
        {
            var playerData = await SaveSystem.LoadAsync<PlayerSaveData>();
            Debug.Log($"Playerのデータがロードされました。HP:{playerData.Health}");
        }
        public async void Unload()
        {
            SaveSystem.Unload<PlayerSaveData>();
            var playerData = await SaveSystem.LoadAsync<PlayerSaveData>();
            Debug.Log($"Playerのデータが初期化されました。HP:{playerData.Health}");
        }

        public void SceneLoad(string sceneName)
        {
            SceneLoader.LoadSceneAsync(sceneName);
        }
    }
}
