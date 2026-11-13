using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.Utility;
using System;
using System.Collections.Generic;
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
    ///     本体JSONには形式のバージョン番号を付けて保存し、保存のたびに1つ前のファイルを「.bak」として残す。
    ///     ファイルを読めないときは「.bak」を読み、それも読めないときは「.corrupt」として残して既定値で始める。
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

        /// <summary> 暗号化ファイル・予備のファイルと、移行前の平文データを削除する。 </summary>
        protected override Awaitable DeleteCoreAsync(Type dataType, CancellationToken token)
        {
            string filePath = GetFilePath(dataType);
            DeleteIfExists(filePath);
            DeleteIfExists(filePath + BACKUP_SUFFIX);

            DeleteLegacyData(dataType);
            NewerVersionTypes.Remove(dataType);
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
        private const string BACKUP_SUFFIX = ".bak";
        private const string CORRUPT_SUFFIX = ".corrupt";

        /// <summary> 今のアプリより新しい形式だったため、読み込まず書き込みもしない型。直列化から作られた場合に備えて、使うときに作る。 </summary>
        [NonSerialized]
        private HashSet<Type> _newerVersionTypes;

        /// <summary> 今のアプリより新しい形式だった型の集合。 </summary>
        private HashSet<Type> NewerVersionTypes => _newerVersionTypes ??= new HashSet<Type>();

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

        /// <summary> ファイルがあれば削除する。 </summary>
        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        ///     暗号化ファイルを読む。読めなければ予備のファイルを読み、それも読めなければ既定値で始める。
        /// </summary>
        private async Task<string> LoadJsonFromFileAsync(Type dataType)
        {
            string filePath = GetFilePath(dataType);
            if (!File.Exists(filePath))
            {
                // 暗号化ファイルが無ければ、以前のローダーの平文データを読み込む。無ければ空文字列になる。
                return PlayerPrefs.GetString(GetLegacyKey(dataType), string.Empty);
            }

            ReadResult result = await TryReadAsync(dataType, filePath);
            if (result.Status == ReadStatus.Succeeded) { return result.Json; }
            if (result.Status == ReadStatus.NewerVersion) { return HandleNewerVersion(dataType, filePath); }

            // 本体が読めないときは、1つ前に保存した予備を読む。
            string backupPath = filePath + BACKUP_SUFFIX;
            if (File.Exists(backupPath))
            {
                ReadResult backup = await TryReadAsync(dataType, backupPath);
                if (backup.Status == ReadStatus.Succeeded)
                {
                    Debug.LogWarning(
                        $"[{nameof(EncryptedFileSaveDataLoaderStrategy)}] {filePath} を読めないため、予備のファイルから読み込みます。");
                    return backup.Json;
                }
                if (backup.Status == ReadStatus.NewerVersion) { return HandleNewerVersion(dataType, backupPath); }
            }

            // 予備も読めないときは、読めなかったファイルを残してから既定値で始め、ホーム画面で知らせる。
            KeepCorruptFile(filePath);
            SaveDataRecoveryRecord.MarkPending();
            Debug.LogWarning(
                $"[{nameof(EncryptedFileSaveDataLoaderStrategy)}] {filePath} と予備を読めないため、既定値で始めます。");
            return string.Empty;
        }

        /// <summary>
        ///     ファイルを復号し、バージョン番号を外して今の形式へ移す。
        /// </summary>
        private static async Task<ReadResult> TryReadAsync(Type dataType, string path)
        {
            try
            {
                byte[] encryptedBytes = await File.ReadAllBytesAsync(path);
                string text = Encoding.UTF8.GetString(SaveDataCrypto.Decrypt(encryptedBytes));
                if (!SaveDataPayloadFormat.TryUnwrap(text, out int version, out string json))
                {
                    return ReadResult.Failed;
                }
                if (version > SaveDataPayloadFormat.CURRENT_VERSION) { return ReadResult.Newer; }

                string current = SaveDataPayloadFormat.MigrateToCurrent(version, json);

                // 本体JSONがその型として読めるかを、ここで確かめる。読めないまま渡すと、黙って既定値に戻されるため。
                JsonUtility.FromJson(current, dataType);
                return ReadResult.Success(current);
            }
            catch (Exception exception) when (exception is CryptographicException
                || exception is ArgumentException
                || exception is IOException)
            {
                Debug.LogWarning(
                    $"[{nameof(EncryptedFileSaveDataLoaderStrategy)}] {path} を読めません。{exception.Message}");
                return ReadResult.Failed;
            }
        }

        /// <summary>
        ///     今のアプリより新しい形式のデータは、読み込まず、上書きもしないようにする。
        /// </summary>
        private string HandleNewerVersion(Type dataType, string path)
        {
            NewerVersionTypes.Add(dataType);
            Debug.LogWarning(
                $"[{nameof(EncryptedFileSaveDataLoaderStrategy)}] {path} は今のアプリより新しい形式のため読み込みません。このデータは保存もしません。");
            return string.Empty;
        }

        /// <summary>
        ///     読めなかったファイルを「.corrupt」の名前で残す。前回の「.corrupt」は置き換える。
        /// </summary>
        private static void KeepCorruptFile(string filePath)
        {
            try
            {
                string corruptPath = filePath + CORRUPT_SUFFIX;
                DeleteIfExists(corruptPath);
                File.Move(filePath, corruptPath);
            }
            catch (IOException exception)
            {
                Debug.LogWarning(
                    $"[{nameof(EncryptedFileSaveDataLoaderStrategy)}] {filePath} を残せませんでした。{exception.Message}");
            }
        }

        /// <summary>
        ///     JSONにバージョン番号を付けて暗号化し、1つ前のファイルを予備に残して置き換える。
        /// </summary>
        private async Task SaveJsonToFileAsync(Type dataType, string json)
        {
            // 新しい形式のデータを古いアプリで上書きして壊さないよう、書き込まない。
            if (NewerVersionTypes.Contains(dataType))
            {
                Debug.LogWarning(
                    $"[{nameof(EncryptedFileSaveDataLoaderStrategy)}] {dataType.Name} は今のアプリより新しい形式のため保存しません。");
                return;
            }

            string filePath = GetFilePath(dataType);
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string payload = SaveDataPayloadFormat.Wrap(json);
            byte[] encryptedBytes = SaveDataCrypto.Encrypt(Encoding.UTF8.GetBytes(payload));
            string tempPath = filePath + ".tmp";
            await File.WriteAllBytesAsync(tempPath, encryptedBytes);

            // 削除してから移動すると、その間に異常終了した場合セーブデータが失われる。
            // 既存ファイルがある場合は原子的に置換し、置き換える前のファイルを予備として残す。
            if (File.Exists(filePath))
            {
                File.Replace(tempPath, filePath, filePath + BACKUP_SUFFIX);
            }
            else
            {
                File.Move(tempPath, filePath);
            }

            // 暗号化ファイルへ移せたので、以前のローダーの平文データを消す。
            DeleteLegacyData(dataType);
        }

        /// <summary> ファイルを読んだ結果の種類。 </summary>
        private enum ReadStatus
        {
            Succeeded,
            Failed,
            NewerVersion,
        }

        /// <summary> ファイルを読んだ結果と、読めたときの本体JSON。 </summary>
        private readonly struct ReadResult
        {
            public ReadResult(ReadStatus status, string json)
            {
                Status = status;
                Json = json;
            }

            /// <summary> 読めなかったことを表す結果。 </summary>
            public static ReadResult Failed => new(ReadStatus.Failed, null);

            /// <summary> 今のアプリより新しい形式だったことを表す結果。 </summary>
            public static ReadResult Newer => new(ReadStatus.NewerVersion, null);

            /// <summary> 結果の種類。 </summary>
            public ReadStatus Status { get; }

            /// <summary> 読めたときの本体JSON。 </summary>
            public string Json { get; }

            /// <summary> 読めたことを表す結果を作る。 </summary>
            public static ReadResult Success(string json) => new(ReadStatus.Succeeded, json);
        }
    }
}
