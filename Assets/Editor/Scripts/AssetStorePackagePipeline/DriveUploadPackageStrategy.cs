using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using KillChord.Editor.AssetManagement;
using SymphonyFrameWork.Debugger.Logger;
using SymphonyFrameWork.Editor;
using UnityEngine;

namespace KillChord.Editor.AssetStorePackagePipeline
{
    /// <summary>
    ///     出力したパッケージを Google Drive のフォルダへアップロードする手順。
    /// </summary>
    /// <remarks>
    ///     Execute段階の手順。
    ///     <b>出力を行う手順より後ろへ置くこと。</b>先頭へ置くとアップロード対象が存在しない。
    ///     ZIP化の手順と併用する場合は、ZIPの後ろへ置くとZIPだけをアップロードする。
    ///     認証には <see cref="DriveImportSecrets" /> の Service Account JSON 鍵を使用するため、
    ///     アップロード先フォルダを Service Account のメールアドレスへ編集権限付きで共有しておく必要がある。
    /// </remarks>
    [Serializable]
    public sealed class DriveUploadPackageStrategy : AssetStoreToolsPackageStepStrategy
    {
        /// <inheritdoc />
        public override string DisplayName => "Upload To Google Drive";

        /// <summary> アップロードに必要なアクセス権限。 </summary>
        private static readonly string[] Scopes =
        {
            DriveAuthProvider.SCOPE_FILE
        };

        private const string LOG_PREFIX = "[" + nameof(DriveUploadPackageStrategy) + "]";

        [SerializeField]
        [Tooltip("アップロード先のGoogleDriveフォルダID。Service Accountのメールアドレスへ編集権限で共有しておく。")]
        private string _folderId;

        /// <inheritdoc />
        protected override void Execute(AssetStoreToolsPackageExportContext context)
        {
            // フォルダIDが未設定のままアップロードするとDrive上のルートへ散らばるため、先に弾く。
            if (string.IsNullOrWhiteSpace(_folderId))
            {
                SymphonyDebugLogger.LogDirect(
                    $"{LOG_PREFIX}\nアップロード先のフォルダIDが設定されていないため、アップロードを行いませんでした。",
                    LogKindEnum.Error);
                return;
            }

            string[] targets = CollectUploadTargets(context);
            if (targets.Length == 0)
            {
                SymphonyDebugLogger.LogDirect(
                    $"{LOG_PREFIX}\nアップロード対象のファイルが存在しませんでした: {context.ExportFullPath}",
                    LogKindEnum.Error);
                return;
            }

            ServiceAccountCredential credential =
                DriveAuthProvider.GetCredential(DriveImportSecrets.instance.serviceAccountJsonKey, Scopes);

            foreach (string target in targets)
            {
                // 1ファイルの失敗で残りのアップロードを止めない。
                try
                {
                    // Execute段階は同期実行のため、完了まで待ってから次のファイルへ進む。
                    string fileId = DriveApiClient
                        .UploadFileAsync(target, _folderId, credential)
                        .GetAwaiter()
                        .GetResult();

                    SymphonyDebugLogger.LogDirect(
                        $"{LOG_PREFIX}\nアップロード完了: {Path.GetFileName(target)} (id: {fileId})");
                }
                catch (Exception e)
                {
                    SymphonyDebugLogger.LogDirect(
                        $"{LOG_PREFIX}\nアップロード失敗: {Path.GetFileName(target)}\n{e}", LogKindEnum.Error);
                }
            }
        }

        /// <summary>
        ///     アップロードするファイルを決定する。
        /// </summary>
        /// <remarks>
        ///     ZIP化の手順が先に走っている場合はZIPだけを送る。
        ///     フォルダの中身を1ファイルずつ送るより転送が速く、Drive上でも1件にまとまるため。
        /// </remarks>
        /// <param name="context"> 出力先と確定済みの計画を保持するコンテキスト。 </param>
        /// <returns> アップロード対象ファイルの絶対パス。 </returns>
        private string[] CollectUploadTargets(AssetStoreToolsPackageExportContext context)
        {
            string zipFullPath = Path.Combine(context.ExportRoot, $"{context.PackageName}.zip");
            if (File.Exists(zipFullPath)) { return new[] { zipFullPath }; }

            // ZIPが無い場合は、出力フォルダ内のファイルをそのまま送る。
            if (!Directory.Exists(context.ExportFullPath)) { return Array.Empty<string>(); }

            IEnumerable<string> files = Directory.EnumerateFiles(
                context.ExportFullPath, "*", SearchOption.AllDirectories);

            return files
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }
    }
}
