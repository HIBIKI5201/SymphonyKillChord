using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.Utility;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータをAES-256-CBCで暗号化し、永続化領域の「型名.sav」へ保存するローダー。
    ///     以前のローダー（JsonUtilitySaveDataLoaderStrategy）がPlayerPrefsへ平文で保存したデータは、
    ///     暗号化ファイルが無い場合に読み込み、次の保存で暗号化ファイルへ移す。
    /// </summary>
    [Serializable]
    public sealed class EncryptedFileSaveDataLoaderStrategy : SaveDataLoaderStrategy
    {
        /// <summary> 暗号化ファイルか、移行前の平文データがあるか確認する。 </summary>
        protected override bool ExistsCore(Type dataType)
        {
            return File.Exists(GetFilePath(dataType)) || PlayerPrefs.HasKey(GetLegacyKey(dataType));
        }

        /// <summary> 暗号化ファイルを復号してJSONを返す。無い場合は移行前の平文データを返す。 </summary>
        protected override Awaitable<string> LoadJsonAsync(Type dataType, CancellationToken token)
        {
            return SymphonyAwaitable.FromTask(LoadJsonFromFileAsync(dataType), token);
        }

        /// <summary> JSONを暗号化し、一時ファイル経由で保存する。 </summary>
        protected override Awaitable SaveJsonAsync(Type dataType, string json, CancellationToken token)
        {
            return SymphonyAwaitable.FromTask(SaveJsonToFileAsync(dataType, json), token);
        }

        /// <summary> 暗号化ファイルと移行前の平文データを削除する。 </summary>
        protected override Awaitable DeleteCoreAsync(Type dataType, CancellationToken token)
        {
            string filePath = GetFilePath(dataType);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            DeleteLegacyData(dataType);
            return SymphonyAwaitable.Completed();
        }

        /// <summary> セーブデータをJSONへ変換する。 </summary>
        protected override string SerializeToJson(Type dataType, SaveDataContent data)
        {
            return JsonUtility.ToJson(data);
        }

        /// <summary> JSONをセーブデータへ上書きする。 </summary>
        protected override void OverwriteFromJson(Type dataType, string json, SaveDataContent data)
        {
            JsonUtility.FromJsonOverwrite(json, data);
        }

        private const string FILE_EXTENSION = ".sav";

        /// <summary> 指定した型の暗号化ファイルのパスを取得する。 </summary>
        private static string GetFilePath(Type dataType)
        {
            // Application は KillChord.Runtime.Application と衝突するため完全修飾する。
            return Path.Combine(UnityEngine.Application.persistentDataPath, dataType.Name + FILE_EXTENSION);
        }

        /// <summary> 以前のローダーがPlayerPrefsへ保存したときのキーを取得する。 </summary>
        private static string GetLegacyKey(Type dataType)
        {
            return dataType.FullName;
        }

        /// <summary> 移行前の平文データを削除する。 </summary>
        private static void DeleteLegacyData(Type dataType)
        {
            string legacyKey = GetLegacyKey(dataType);
            if (!PlayerPrefs.HasKey(legacyKey))
            {
                return;
            }

            PlayerPrefs.DeleteKey(legacyKey);
            PlayerPrefs.Save();
        }

        /// <summary>
        ///     暗号化ファイルを読み込んで復号する。
        ///     復号できない場合は空文字列を返し、既定値のセーブデータで始める。
        /// </summary>
        private static async Task<string> LoadJsonFromFileAsync(Type dataType)
        {
            string filePath = GetFilePath(dataType);
            if (!File.Exists(filePath))
            {
                // 暗号化ファイルが無ければ、以前のローダーの平文データを読み込む。無ければ空文字列になる。
                return PlayerPrefs.GetString(GetLegacyKey(dataType), string.Empty);
            }

            byte[] encryptedBytes = await File.ReadAllBytesAsync(filePath);
            try
            {
                return Encoding.UTF8.GetString(SaveDataCrypto.Decrypt(encryptedBytes));
            }
            catch (Exception exception) when (exception is CryptographicException || exception is ArgumentException)
            {
                Debug.LogWarning(
                    $"[{nameof(EncryptedFileSaveDataLoaderStrategy)}] {filePath} を復号できないため、既定値で始めます。{exception.Message}");
                return string.Empty;
            }
        }

        /// <summary> JSONを暗号化して一時ファイルへ書き込み、保存先へ差し替える。 </summary>
        private static async Task SaveJsonToFileAsync(Type dataType, string json)
        {
            string filePath = GetFilePath(dataType);
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            byte[] encryptedBytes = SaveDataCrypto.Encrypt(Encoding.UTF8.GetBytes(json));
            string tempPath = filePath + ".tmp";
            await File.WriteAllBytesAsync(tempPath, encryptedBytes);

            // 削除してから移動すると、その間に異常終了した場合セーブデータが失われる。
            // 既存ファイルがある場合は原子的に置換する。
            if (File.Exists(filePath))
            {
                File.Replace(tempPath, filePath, null);
            }
            else
            {
                File.Move(tempPath, filePath);
            }

            // 暗号化ファイルへ移せたので、以前のローダーの平文データを消す。
            DeleteLegacyData(dataType);
        }
    }
}
