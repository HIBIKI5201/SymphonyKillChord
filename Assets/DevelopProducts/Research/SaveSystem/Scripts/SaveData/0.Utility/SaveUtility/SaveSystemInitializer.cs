using Research.SaveSystem;
using System.Threading.Tasks;
using UnityEngine;

namespace DevelopProducts.SaveSystem
{
    public class SaveSystemInitializer : MonoBehaviour
    {
        private void Awake()
        {
            InitializeData();
        }
        private async void InitializeData()
        {
            var playerData = new PlayerSaveData();
            await SaveSystem.SaveAsync(playerData);

            Debug.Log("SaveSystem initialized with default PlayerSaveData.");
        }
    }
}
