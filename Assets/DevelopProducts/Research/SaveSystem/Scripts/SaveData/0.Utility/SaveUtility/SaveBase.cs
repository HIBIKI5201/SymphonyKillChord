using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace DevelopProducts.SaveSystem
{
    /// <summary>
    ///    セーブデータの基底クラス。
    /// </summary>
    public abstract class SaveBase
     {
        /// <summary>
        ///   セーブデータの基底クラスを初期化します。
        /// </summary>
        protected SaveBase()
        {
            _filePath = Path.Combine(
                Application.persistentDataPath,
                $"{SaveKey}.json");
        }
        /// <summary>
        ///  セーブデータを非同期で読み込みます。 
        /// </summary>
        /// <returns></returns>
        internal async ValueTask ReadAsync()
        {
            if (!File.Exists(_filePath))
                return;

            var json = await File.ReadAllTextAsync(_filePath);
            JsonUtility.FromJsonOverwrite(json, this);
        }
        /// <summary>
        /// セーブデータを非同期で保存します。
        /// </summary>
        /// <returns></returns>
        internal async ValueTask WriteAsync()
        {
            var json = JsonUtility.ToJson(this, true);
            await File.WriteAllTextAsync(_filePath, json);
        }
        /// <summary> セーブデータのキーを取得します。</summary>
        protected virtual string SaveKey => GetType().Name;
        /// <summary> セーブデータのファイルパスを取得します。</summary>
        private readonly string _filePath;
    }
}