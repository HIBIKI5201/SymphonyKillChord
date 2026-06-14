using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using KillChord.Editor.AssetImporter.Settings;

namespace KillChord.Editor.AssetImporter
{
    /// <summary>
    ///     指定されたディレクトリ内のUnityPackageを順番にサイレントインポートするためのクラス。
    /// 
    ///     AssetDatabase.ImportPackageはインポートするアセットにスクリプトが含まれる場合、
    ///     ドメインリロードが発生するため、複数のパッケージを連続してインポートするには、
    ///     インポート完了イベントをトリガーにして次のインポートを開始する必要がある。
    /// </summary>
    [InitializeOnLoad]
    public static class AssetPackageImporter
    {
        /// <summary>
        ///     インポート待ちのパッケージのキューを管理するためのクラス。SessionStateにJSON化して保存される。
        /// </summary>
        [Serializable]
        private class QueueData
        {
            public int totalCount;
            public List<string> packagePaths;
        }

        /// <summary>
        ///     静的コンストラクタ。エディタがロードされたときに一度だけ呼び出される。
        /// </summary>
        static AssetPackageImporter()
        {
            // イベントハンドラーの購読。
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            AssetDatabase.importPackageFailed -= OnImportPackageFailed;
            AssetDatabase.importPackageFailed += OnImportPackageFailed;

            if (SessionState.GetBool(AssetImportSettings.WAITING_KEY, false))
            {
                SessionState.SetBool(AssetImportSettings.WAITING_KEY, false); // フラグを回収
                ContinueImportQueue();
            }
        }

        /// <summary>
        ///     パッケージのインポートが完了したときに呼び出されるイベントハンドラー。次のパッケージのインポートを続行する。
        /// </summary>
        /// <param name="packageName"></param>
        private static void OnImportPackageCompleted(string packageName)
        {
            if (SessionState.GetBool(AssetImportSettings.WAITING_KEY, false))
            {
                SessionState.SetBool(AssetImportSettings.WAITING_KEY, false); // フラグを回収
                ContinueImportQueue();
            }
        }

        /// <summary>
        ///     パッケージのインポートが失敗したときに呼び出されるイベントハンドラー。エラーメッセージをログに出力し、次のパッケージのインポートを続行する。
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="errorMessage"></param>
        private static void OnImportPackageFailed(string packageName, string errorMessage)
        {
            Debug.LogError($"[{nameof(AssetPackageImporter)}] インポート失敗: {packageName} - {errorMessage}");
        
            if (SessionState.GetBool(AssetImportSettings.WAITING_KEY, false))
            {
                SessionState.SetBool(AssetImportSettings.WAITING_KEY, false);
                ContinueImportQueue();
            }
        }

        /// <summary>
        ///     指定されたディレクトリ内のすべての .unitypackage を対象に、サイレントインポートを開始する
        /// </summary>
        /// <param name="directoryPath">.unitypackageファイルが格納されているフォルダパス</param>
        public static void ResetAndStartImportQueue(string directoryPath)
        {
            string fullPath = Path.GetFullPath(directoryPath);
            if (!Directory.Exists(fullPath))
            {
                Debug.LogError($"[{nameof(AssetPackageImporter)}] 保存先パスが存在しません。インポートを中断します。: {fullPath}");
                return;
            }

            // ディレクトリ内のすべての .unitypackage ファイルを検索。
            string[] files = Directory.GetFiles(fullPath, $"*{AssetImportSettings.EXT_UNITYPACKAGE}", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                Debug.Log($"[{nameof(AssetPackageImporter)}] インポート対象の .unitypackage が見つかりませんでした。インポートを中断します。");
                return;
            }
            
            Debug.Log($"[{nameof(AssetPackageImporter)}] インポート対象の .unitypackage を {files.Length} 件見つけました。インポートを開始します...");

            // AssetDatabase.ImportPackage が要求する「Assets/」から始まる相対パスに変換。
            List<string> projectRelativePaths = new();
            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));

            foreach (var file in files)
            {
                string relativePath = Path.GetFullPath(file).Replace(projectPath, "").Replace("\\", "/");
                projectRelativePaths.Add(relativePath);
            }

            // キューデータをJSON化してSessionStateに保存 + 進捗状況の更新。
            QueueData data = new QueueData
            {
                totalCount = projectRelativePaths.Count,
                packagePaths = projectRelativePaths
            };
            SessionState.SetString(AssetImportSettings.SESSION_KEY, JsonUtility.ToJson(data));
            SessionState.SetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, 0.0f);
            SessionState.SetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, $"インポート対象の .unitypackage を {files.Length} 件見つけました。インポートを開始します...");

            ContinueImportQueue();
        }

        /// <summary>
        ///     セッションからキューを読み込み、次のパッケージのインポートを実行する。
        /// </summary>
        private static void ContinueImportQueue()
        {
            string json = SessionState.GetString(AssetImportSettings.SESSION_KEY, "");
            if (string.IsNullOrEmpty(json)) return;

            QueueData data = JsonUtility.FromJson<QueueData>(json);
            if (data?.packagePaths == null || data.packagePaths.Count == 0)
            {
                // すべてのインポートが終了したらセッションキーを削除。
                ClearAllSessionKeys();
                Debug.Log($"[{nameof(AssetPackageImporter)}] すべてのパッケージのインポートが完了しました。");
                SessionState.SetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, 1f);
                SessionState.SetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, "すべてのパッケージのインポートが完了しました。");
                return;
            }

            // 先頭のパスを1つ取り出して残りを保存
            string currentPackage = data.packagePaths[0];
            data.packagePaths.RemoveAt(0);
            SessionState.SetString(AssetImportSettings.SESSION_KEY, JsonUtility.ToJson(data));
            
            Debug.Log($"[{nameof(AssetPackageImporter)}] インポート中: {currentPackage}... 残り {data.packagePaths.Count} 件");
            SessionState.SetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, 1f - (float)data.packagePaths.Count / data.totalCount);
            SessionState.SetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, $"インポート中: {Path.GetFileName(currentPackage)}... 残り {data.packagePaths.Count} 件");

            // アセット内にスクリプトが含まれる場合、この処理の直後、または処理中にドメインリロードが発生する
            SessionState.SetBool(AssetImportSettings.WAITING_KEY, true);
            AssetDatabase.ImportPackage(currentPackage, interactive: false);
        }
        
        /// <summary>
        ///     セッションに保存されたすべてのキーを削除する。インポートキューが空になったときに呼び出される。
        /// </summary>
        private static void ClearAllSessionKeys()
        {
            SessionState.EraseString(AssetImportSettings.SESSION_KEY);
            SessionState.EraseBool(AssetImportSettings.WAITING_KEY);
        }
    }
}