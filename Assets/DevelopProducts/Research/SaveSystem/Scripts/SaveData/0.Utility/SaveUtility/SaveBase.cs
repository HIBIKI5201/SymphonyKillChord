using Codice.Utils;
using System;
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
        ///     セーブデータを非同期で読み込みます。 
        /// </summary>
        /// <returns></returns>
        internal async ValueTask ReadAsync()
        {
            if (!File.Exists(FilePath))
                return;

            var json = await File.ReadAllTextAsync(FilePath);
            JsonUtility.FromJsonOverwrite(json, this);
        }
        /// <summary>
        ///     セーブデータを非同期で保存します。
        /// </summary>
        /// <returns></returns>
        internal async ValueTask WriteAsync()
        {
            try
            {
                var json = JsonUtility.ToJson(this, true);
                var tempPath = FilePath + ".tmp";
                await File.WriteAllTextAsync(tempPath, json);

                if (File.Exists(FilePath))
                    File.Delete(FilePath);

                File.Move(tempPath, FilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write save file: {ex}");
                throw;
            }
        }
        /// <summary> セーブデータのキーを取得します。</summary>
        private string SaveKey => GetType().Name;
        /// <summary> セーブデータのファイルパスを取得します。</summary>
        private string FilePath => _filePath ??= Path.Combine(Application.persistentDataPath, $"{SaveKey}.json");
        private string _filePath;
    }
}