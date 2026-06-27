using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Utility.OutGame.Savedata
{
    /// <summary>
    ///    セーブデータの基底クラス。
    /// </summary>
    public abstract class SaveBase
    {
        /// <summary>
        ///     セーブデータを読み込んだ後に呼び出されるメソッド。
        ///   <para>    必要に応じてオーバーライドして、読み込んだ後の処理を実装できます。</para>
        /// </summary>
        protected virtual void OnAfterDeserialize() { }

        /// <summary>
        ///     セーブデータを非同期で読み込みます。 
        /// </summary>
        /// <returns></returns>
        internal async ValueTask ReadAsync()
        {
            if (!File.Exists(FilePath))
                return;

            try
            {
                var json = await File.ReadAllTextAsync(FilePath);
                JsonUtility.FromJsonOverwrite(json, this);
                OnAfterDeserialize();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveBase] Failed to read save file: {ex}");
            }
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
                Debug.Log($"[SaveBase] Write Savedata File. Path:{FilePath}");
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
        // TODO セーブデータの格納先が決まったらここを直す。
        private string FilePath => _filePath ??= Path.Combine(Application.persistentDataPath, $"{SaveKey}.json");
        private string _filePath;
    }
}