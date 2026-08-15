using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.Utility;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータを永続化領域のJSONファイルへ保存するローダー。
    /// </summary>
    [Serializable]
    public sealed class PersistentFileSaveDataLoaderStrategy : SaveDataLoaderStrategy
    {
        /// <summary> 指定した型のセーブファイルが存在するか確認する。 </summary>
        protected override bool ExistsCore(Type dataType)
        {
            return File.Exists(GetFilePath(dataType));
        }

        /// <summary> 指定した型のセーブファイルからJSONを読み込む。 </summary>
        protected override Awaitable<string> LoadJsonAsync(Type dataType, CancellationToken token)
        {
            string filePath = GetFilePath(dataType);
            if (!File.Exists(filePath))
            {
                return SymphonyAwaitable.FromResult(string.Empty);
            }

            return SymphonyAwaitable.FromTask(File.ReadAllTextAsync(filePath), token);
        }

        /// <summary> 指定した型のJSONを一時ファイル経由で保存する。 </summary>
        protected override Awaitable SaveJsonAsync(Type dataType, string json, CancellationToken token)
        {
            return SymphonyAwaitable.FromTask(SaveJsonToFileAsync(dataType, json), token);
        }

        /// <summary> 指定した型のセーブファイルを削除する。 </summary>
        protected override Awaitable DeleteCoreAsync(Type dataType, CancellationToken token)
        {
            string filePath = GetFilePath(dataType);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return SymphonyAwaitable.Completed();
        }

        /// <summary> セーブデータを既存形式のJSONへ変換する。 </summary>
        protected override string SerializeToJson(Type dataType, SaveDataContent data)
        {
            return JsonUtility.ToJson(data, true);
        }

        /// <summary> 既存形式のJSONをセーブデータへ上書きする。 </summary>
        protected override void OverwriteFromJson(Type dataType, string json, SaveDataContent data)
        {
            JsonUtility.FromJsonOverwrite(json, data);
        }

        /// <summary> 指定した型のセーブファイルパスを取得する。 </summary>
        private static string GetFilePath(Type dataType)
        {
            // Application は KillChord.Runtime.Application と衝突するため完全修飾する。
            return Path.Combine(UnityEngine.Application.persistentDataPath, $"{dataType.Name}.json");
        }

        /// <summary> JSONを一時ファイルへ書き込み、保存先へ差し替える。 </summary>
        private static async Task SaveJsonToFileAsync(Type dataType, string json)
        {
            string filePath = GetFilePath(dataType);
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string tempPath = filePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json);

            // 削除してから移動すると、その間に異常終了した場合セーブデータが失われる。
            // 既存ファイルがある場合は原子的に置換する。
            if (File.Exists(filePath))
            {
                File.Replace(tempPath, filePath, null);
                return;
            }

            File.Move(tempPath, filePath);
        }
    }
}
