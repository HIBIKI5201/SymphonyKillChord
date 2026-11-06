using System;
using System.IO;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータを読めず既定値で始めたことを、永続化領域の目印ファイルで記録する。
    ///     ホーム画面で1回だけ知らせるために使い、知らせたら消す。
    /// </summary>
    public static class SaveDataRecoveryRecord
    {
        /// <summary> 知らせていない記録があるか。 </summary>
        public static bool HasPending => File.Exists(GetMarkerPath());

        /// <summary>
        ///     既定値で始めたことを記録する。
        /// </summary>
        public static void MarkPending()
        {
            try
            {
                File.WriteAllText(GetMarkerPath(), DateTime.UtcNow.ToString("o"));
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"[{nameof(SaveDataRecoveryRecord)}] 目印ファイルを書けませんでした。{exception.Message}");
            }
        }

        /// <summary>
        ///     知らせ終えた記録を消す。
        /// </summary>
        public static void Clear()
        {
            try
            {
                File.Delete(GetMarkerPath());
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"[{nameof(SaveDataRecoveryRecord)}] 目印ファイルを消せませんでした。{exception.Message}");
            }
        }

        private const string MARKER_FILE_NAME = "save_recovered.flag";

        /// <summary>
        ///     目印ファイルのパスを返す。
        /// </summary>
        private static string GetMarkerPath()
        {
            // Application は KillChord.Runtime.Application と衝突するため完全修飾する。
            return Path.Combine(UnityEngine.Application.persistentDataPath, MARKER_FILE_NAME);
        }
    }
}
