using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AssetManagement
{
    internal static class DriveImportSync
    {
        private static bool isRunning;
        private static CancellationTokenSource cts;

        /// <summary>
        ///     実行中の同期をキャンセルする。
        /// </summary>
        public static void Cancel()
        {
            if (isRunning && cts != null)
            {
                cts.Cancel();
            }
        }

        [MenuItem(ToolConst.TOOLS_PATH + "/" + nameof(DriveImportSync) + "/Sync Now")]
        public static async void SyncNow()
        {
            if (isRunning)
            {
                EditorUtility.DisplayDialog(
                    "Drive Import",
                    "同期は既に実行中です。",
                    "OK");

                return;
            }

            isRunning = true;
            cts = new CancellationTokenSource();
            DriveImportSyncWindow.StartSync();
            DriveImportSyncWindow.Log("同期開始");
            var importedAssets = new HashSet<string>();
            int totalUpdated = 0, totalSkipped = 0, totalFailed = 0;

            try
            {
                var secrets = DriveImportSecrets.instance;
                var settings = DriveImportSettings.instance;
                var manifest = DriveImportManifest.instance;

                if (string.IsNullOrEmpty(secrets.serviceAccountJsonKey))
                {
                    const string msg =
                        "[DriveImport] Service AccountのJSON鍵が未設定です。Project Settings > Drive Import で設定してください。";
                    Debug.LogError(msg);
                    DriveImportSyncWindow.Error(msg);
                    return;
                }

                if (secrets.sourceFolders == null || secrets.sourceFolders.Count == 0)
                {
                    const string msg = "[DriveImport] 取得元フォルダが未設定です。Project Settings > Drive Import で追加してください。";
                    Debug.LogError(msg);
                    DriveImportSyncWindow.Error(msg);
                    return;
                }

                ServiceAccountCredential credential;
                try
                {
                    credential = DriveAuthProvider.GetCredential(secrets.serviceAccountJsonKey);
                }
                catch (InvalidOperationException e)
                {
                    const string prefix = "[DriveImport]";
                    Debug.LogError($"{prefix} 認証情報の取得に失敗しました: {e.Message}\n{e.StackTrace}");
                    DriveImportSyncWindow.Error($"{prefix} 認証情報の取得に失敗しました: {e.Message}");
                    return;
                }
                catch (Exception e)
                {
                    const string prefix = "[DriveImport]";
                    Debug.LogError($"{prefix} 予期しないエラーで認証に失敗しました: {e.Message}\n{e.StackTrace}");
                    DriveImportSyncWindow.Error($"{prefix} 予期しないエラーが発生しました: {e.Message}");
                    return;
                }

                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                foreach (var sourceFolder in secrets.sourceFolders)
                {
                    if (string.IsNullOrEmpty(sourceFolder.folderId))
                    {
                        const string msg = "[DriveImport] folderId未設定の項目をスキップしました。";

                        Debug.LogWarning(msg);
                        DriveImportSyncWindow.Warning(msg);
                        continue;
                    }

                    if (!IsValidDestination(sourceFolder.destinationPath))
                    {
                        var msg =
                            $"[DriveImport] 配置先パスが不正です(Assets/配下を指定してください): {sourceFolder.destinationPath}";

                        Debug.LogError(msg);
                        DriveImportSyncWindow.Error(msg);
                        continue;
                    }

                    var destinationAbsRoot = Path.GetFullPath(Path.Combine(projectRoot, sourceFolder.destinationPath));
                    var queue = new List<(DriveApiClient.DriveNode node, string absPath)>();
                    DriveImportSyncWindow.Log(
                        $"フォルダ取得 : {sourceFolder.destinationPath}");

                    try
                    {
                        await CollectFilesAsync(sourceFolder.folderId, destinationAbsRoot, settings, credential, queue, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        const string prefix = "[DriveImport]";
                        Debug.LogWarning($"{prefix} 同期がキャンセルされました。");
                        DriveImportSyncWindow.Warning($"{prefix} 同期がキャンセルされました。");
                        break;
                    }
                    catch (Exception e)
                    {
                        const string prefix = "[DriveImport]";
                        Debug.LogError($"{prefix} フォルダ取得中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
                        DriveImportSyncWindow.Error($"{prefix} フォルダ取得中にエラーが発生しました: {e.Message}");
                        continue;
                    }

                    DriveImportSyncWindow.Log($"取得対象 : {queue.Count}件");

                    int total = queue.Count;
                    int done = 0;

                    foreach (var (node, absPath) in queue)
                    {
                        done++;
                        DriveImportSyncWindow.SetProgress(
                            done,
                            total,
                            node.Name);

                        if (manifest.TryGetModifiedTime(node.Id, out var lastModified)
                            && lastModified == node.ModifiedTime
                            && File.Exists(absPath))
                        {
                            totalSkipped++;
                            DriveImportSyncWindow.Warning(
                                $"Skip : {node.Name}");
                            continue;
                        }

                        try
                        {
                            cts.Token.ThrowIfCancellationRequested();
                            await DriveApiClient.DownloadFileAsync(node.Id, credential, absPath, cts.Token);
                            manifest.SetModifiedTime(node.Id, node.ModifiedTime);
                            string assetPath = FileUtil.GetProjectRelativePath(absPath);

                            if (!string.IsNullOrEmpty(assetPath))
                            {
                                importedAssets.Add(assetPath.Replace('\\', '/'));
                            }

                            totalUpdated++;
                            DriveImportSyncWindow.Log(
                                $"Download : {node.Name}");
                        }
                        catch (OperationCanceledException)
                        {
                            const string prefix = "[DriveImport]";
                            Debug.LogWarning($"{prefix} ダウンロード中断: {node.Name}");
                            DriveImportSyncWindow.Warning($"{prefix} ダウンロード中断: {node.Name}");
                            totalFailed++;
                        }
                        catch (Exception e)
                        {
                            totalFailed++;
                            const string prefix = "[DriveImport]";
                            Debug.LogError($"{prefix} ダウンロード失敗 [{node.Name}]: {e.Message}\n{e.StackTrace}");
                            DriveImportSyncWindow.Error($"{prefix} ダウンロード失敗 [{node.Name}]: {e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                const string prefix = "[DriveImport]";
                var msg = $"{prefix} 同期中に予期しないエラーが発生しました: {e.Message}";
                Debug.LogError($"{msg}\n{e.StackTrace}");
                DriveImportSyncWindow.Error(msg);
            }

            finally
            {
                isRunning = false;
                cts?.Dispose();
                cts = null;

                DriveImportManifest.instance.Persist();
                
                if (importedAssets.Count > 0)
                {
                    AssetDatabase.StartAssetEditing();
                    try
                    {
                        foreach (var assetPath in importedAssets)
                        {
                            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                        }
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                    }
                    
                    AssetDatabase.Refresh();
                }

                DriveImportSyncWindow.Log("");
                DriveImportSyncWindow.Log($"更新 : {totalUpdated}");
                DriveImportSyncWindow.Log($"スキップ : {totalSkipped}");
                DriveImportSyncWindow.Log($"失敗 : {totalFailed}");
                DriveImportSyncWindow.Finish();
            }
        }

        /// <summary>
        ///     配置先パスが妥当か検証する (Assets/ 配下であることを確認)。
        /// </summary>
        /// <param name="destinationPath"> 検証対象のパス。 </param>
        /// <returns> 妥当なパスであれば true。 </returns>
        private static bool IsValidDestination(string destinationPath)
        {
            if (string.IsNullOrEmpty(destinationPath))
            {
                return false;
            }

            return destinationPath.Replace('\\', '/').TrimStart('/').StartsWith("Assets");
        }

        /// <summary>
        ///     Google Drive フォルダを再帰的に走査し、ダウンロード対象ファイルをキューに追加する。
        /// </summary>
        /// <param name="folderId"> 走査開始フォルダの Drive ID。 </param>
        /// <param name="localFolderAbsPath"> ローカル保存先の絶対パス。 </param>
        /// <param name="settings"> 除外パターンなどの同期設定。 </param>
        /// <param name="credential"> Drive API 認証情報。 </param>
        /// <param name="queue"> ダウンロード対象ファイル (ノード, ローカルパス) のリスト。 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        private static async Task CollectFilesAsync(
            string folderId,
            string localFolderAbsPath,
            DriveImportSettings settings,
            ServiceAccountCredential credential,
            List<(DriveApiClient.DriveNode, string)> queue,
            CancellationToken ct = default)
        {
            List<DriveApiClient.DriveNode> children = await DriveApiClient.ListChildrenAsync(folderId, credential, ct);

            foreach (var node in children)
            {
                if (node.IsFolder)
                {
                    if (IsFolderExcluded(node.Name, settings))
                    {
                        continue;
                    }

                    var subPath = Path.Combine(localFolderAbsPath, SanitizeLocalName(node.Name));
                    await CollectFilesAsync(node.Id, subPath, settings, credential, queue, ct);
                }
                else
                {
                    if (IsFileExcluded(node.Name, settings))
                    {
                        continue;
                    }

                    var filePath = Path.Combine(localFolderAbsPath, SanitizeLocalName(node.Name));
                    queue.Add((node, filePath));
                }
            }
        }

        /// <summary>
        ///     フォルダ名が除外リストに含まれるか判定する。
        /// </summary>
        /// <param name="folderName"> 判定対象のフォルダ名。 </param>
        /// <param name="settings"> 除外設定。 </param>
        /// <returns> 除外対象であれば true。 </returns>
        private static bool IsFolderExcluded(string folderName, DriveImportSettings settings)
        {
            foreach (var excludedName in settings.excludeFolderNames)
            {
                if (!string.IsNullOrEmpty(excludedName)
                    && string.Equals(excludedName, folderName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     ファイル名が除外対象か判定する (拡張子・パターンで判定)。
        /// </summary>
        /// <param name="fileName"> 判定対象のファイル名。 </param>
        /// <param name="settings"> 除外設定。 </param>
        /// <returns> 除外対象であれば true。 </returns>
        private static bool IsFileExcluded(string fileName, DriveImportSettings settings)
        {
            var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            foreach (var e in settings.excludeExtensions)
            {
                if (!string.IsNullOrEmpty(e) && e.TrimStart('.').ToLowerInvariant() == ext)
                {
                    return true;
                }
            }

            foreach (var p in settings.excludeFilePatterns)
            {
                if (string.IsNullOrEmpty(p.pattern))
                {
                    continue;
                }

                var regexPattern = p.type == FilePatternType.Wildcard
                    ? WildcardToRegex(p.pattern)
                    : p.pattern;

                try
                {
                    if (Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase))
                    {
                        return true;
                    }
                }
                catch (ArgumentException e)
                {
                    Debug.LogWarning($"[DriveImport] 無効なパターンをスキップしました: {p.pattern} ({e.Message})");
                }
            }

            return false;
        }

        /// <summary>
        ///     ワイルドカードパターンを正規表現に変換する。
        /// </summary>
        /// <param name="wildcard"> ワイルドカードパターン (* と ? を使用)。 </param>
        /// <returns> 対応する正規表現文字列。 </returns>
        private static string WildcardToRegex(string wildcard)
        {
            return "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
        }

        /// <summary>
        ///     ファイル/フォルダ名から無効な文字を削除し、ローカルファイルシステムに適合させる。
        /// </summary>
        /// <param name="name"> サニタイズ前の名前。 </param>
        /// <returns> 無効な文字が '_' に置換された名前。 </returns>
        private static string SanitizeLocalName(string name)
        {
            return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
        }
    }
}