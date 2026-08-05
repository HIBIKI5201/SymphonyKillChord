using System.Collections.Generic;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AssetManagement
{
    /// <summary>
    ///     機密情報を保持するシングルトン設定。
    ///     Service Account JSON Key と取得元フォルダ ID を管理。
    ///     UserSettings 配下に保存され Git 管理対象外となる。
    /// </summary>
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(DriveImportSecrets), FilePathAttribute.Location.ProjectFolder)]
    internal class DriveImportSecrets : ScriptableSingleton<DriveImportSecrets>
    {
        /// <summary> Google Service Account の JSON 鍵 (JSON 文字列)。機密情報。 </summary>
        public string serviceAccountJsonKey = "";

        /// <summary> 取得元フォルダ ID と配置先パスの組。フォルダ ID も機密扱い。 </summary>
        [Tooltip("取得元フォルダIDと配置先パスの組。フォルダIDにはGoogleDriveのフォルダIDを指定する。配置先パスはAssets/配下を指定する必要がある。")]
        public List<DriveSourceFolder> sourceFolders = new();

        /// <summary>
        ///     設定をディスクに永続化する。
        /// </summary>
        public void Persist()
        {
            Save(true);
        }
    }
}
