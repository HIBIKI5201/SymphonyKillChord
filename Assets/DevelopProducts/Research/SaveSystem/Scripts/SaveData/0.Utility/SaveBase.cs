using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace DevelopProducts.SaveSystem
{
    public abstract class SaveBase
    {
        internal async ValueTask ReadAsync()
        {
            if (!File.Exists(FilePath)) return;

            var json = await File.ReadAllTextAsync(FilePath);
            JsonUtility.FromJsonOverwrite(json, this);
        }
        internal async ValueTask WriteAsync()
        {
            var json = JsonUtility.ToJson(this, prettyPrint: true);
            await File.WriteAllTextAsync(FilePath, json);
        }
        protected virtual string SaveKey => GetType().Name;
        private string FilePath => Path.Combine(Application.persistentDataPath, $"{SaveKey}.json");
    }
}