using System.Collections.Generic;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AssetManagement
{
    /// <summary>
    ///     機密情報を保持するシングルトン設定。
    ///     取得元フォルダ ID を管理する。UserSettings 配下に保存され Git 管理対象外となる。
    ///     Service Account JSON Key は共通秘密設定ファイル（<see cref="SharedSecrets"/>）から読む。
    /// </summary>
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(DriveImportSecrets), FilePathAttribute.Location.ProjectFolder)]
    internal class DriveImportSecrets : ScriptableSingleton<DriveImportSecrets>
    {
        /// <summary>
        ///     移行前の Google Service Account の JSON 鍵 (JSON 文字列)。
        ///     現在は共通秘密設定ファイルに置く。移行期間中だけ読み、移行したら空にする。
        /// </summary>
        public string serviceAccountJsonKey = "";

        /// <summary> 取得元フォルダ ID と配置先パスの組。フォルダ ID も機密扱い。 </summary>
        [Tooltip("取得元フォルダIDと配置先パスの組。フォルダIDにはGoogleDriveのフォルダIDを指定する。配置先パスはAssets/配下を指定する必要がある。")]
        public List<DriveSourceFolder> sourceFolders = new();

        /// <summary>
        ///     Service Account の JSON 鍵を取得する。共通秘密設定ファイルを優先し、無い場合は移行前の値を使う。
        /// </summary>
        /// <returns> JSON 鍵の文字列。未設定の場合は空文字。 </returns>
        public string GetServiceAccountJsonKey()
        {
            return SharedSecrets.ResolveValue(
                SharedSecrets.GOOGLE_SERVICE_ACCOUNT_JSON_KEY,
                serviceAccountJsonKey,
                "Project Settings > KillChord > Drive Import");
        }

        /// <summary>
        ///     移行前の JSON 鍵を共通秘密設定ファイルへ移し、移せたらこの設定から消す。
        /// </summary>
        /// <returns> 移行前の値が残っていない場合は true。 </returns>
        public bool MigrateToSharedSecrets()
        {
            if (!SharedSecrets.TryMigrate(SharedSecrets.GOOGLE_SERVICE_ACCOUNT_JSON_KEY, serviceAccountJsonKey))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(serviceAccountJsonKey))
            {
                serviceAccountJsonKey = "";
                Persist();
            }

            return true;
        }

        /// <summary>
        ///     設定をディスクに永続化する。
        /// </summary>
        public void Persist()
        {
            Save(true);
        }
    }
}
