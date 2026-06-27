using System;
using System.IO;
using System.Text;
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
        ///     セーブデータを非同期で読み込み、暗号化された文を復号して読み込みます。
        /// </summary>
        /// <returns></returns>
        internal async ValueTask ReadAsync()
        {
            if (!File.Exists(FilePath))
            {
                return;
            }

            try
            {
                byte[] encrypted = await File.ReadAllBytesAsync(FilePath);
                var json = Encoding.UTF8.GetString(SaveCryptoUtility.Decrypt(encrypted));
                JsonUtility.FromJsonOverwrite(json, this);
            }
            catch (Exception ex) 
            {
                throw new InvalidDataException("セーブデータの復号に失敗しました。", ex);
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
                //  データを暗号化する。
                byte[] encrypted = SaveCryptoUtility.Encrypt(Encoding.UTF8.GetBytes(json));

                var tempPath = FilePath + ".tmp";
                await File.WriteAllBytesAsync(tempPath, encrypted);

                if (File.Exists(FilePath))
                {
                    //  既存があるなら原子的に置き換える。
                    File.Replace(tempPath, FilePath, null);
                }
                else
                {
                    //  初回は置き換え先が無いので単純に移動する。
                    File.Move(tempPath, FilePath);
                }
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
        private string FilePath => _filePath ??= Path.Combine(Application.persistentDataPath, $"{SaveKey}.sav");
        private string _filePath;
    }
}