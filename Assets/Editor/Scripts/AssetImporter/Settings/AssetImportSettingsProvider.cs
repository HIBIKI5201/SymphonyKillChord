using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace KillChord.Editor.AssetImporter.Settings
{
    /// <summary>
    ///     Google DriveからのUnityPackageのダウンロードに必要な設定を管理するSettingsProvider。
    ///     OAuthクライアントID、クライアントシークレット、Google DriveのフォルダID、UnityPackageの保存先パスなどを設定できる。
    /// </summary>
    public class AssetImportSettingsProvider : SettingsProvider
    {
        private AssetImportSettingsProvider(string path, SettingsScope scopes,
            IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateGoogleDriveSettingsProvider()
        {
            return new AssetImportSettingsProvider(AssetImportSettings.SETTINGS_PATH, SettingsScope.Project);
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            EditorGUI.BeginChangeCheck();
            var settings = AssetImportSettings.instance;
            settings.clientId = EditorGUILayout.TextField("OAuth Client ID", settings.clientId);
            settings.clientSecret = EditorGUILayout.PasswordField("OAuth Client Secret", settings.clientSecret);
            settings.folderId = EditorGUILayout.TextField("GoogleDrive Folder ID", settings.folderId);
            settings.deleteAfterImport = EditorGUILayout.Toggle("Delete Package After Import", settings.deleteAfterImport);

            EditorGUILayout.Space(10);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Last Downloaded Version", settings.lastDownloadedVersion);
            }

            if (GUILayout.Button("Authorize via Browser"))
            {
                _ = GoogleDriveAuthManager.StartOAuthFlowAsync();
            }

            if (EditorGUI.EndChangeCheck())
            {
                settings.Save();
            }
        }
    }
}

#endif