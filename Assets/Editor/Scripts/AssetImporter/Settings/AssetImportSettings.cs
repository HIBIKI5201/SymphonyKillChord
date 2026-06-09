using KillChord.Editor.Utility;
using UnityEditor;

#if UNITY_EDITOR
namespace KillChord.Editor.AssetImporter.Settings
{
    /// <summary>
    /// Google Driveからのインポートに関する設定や、他で使用する定数を保持・管理するScriptableSingletonクラス。
    /// </summary>
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(AssetImportSettings) + ProviderConst.ASSET_EXT,
        FilePathAttribute.Location.ProjectFolder)]
    public class AssetImportSettings : ScriptableSingleton<AssetImportSettings>
    {
        public string clientId = "";
        public string clientSecret = "";
        public string accessToken = "";
        public string refreshToken = "";
        public string folderId = "";
        public string lastDownloadedVersion = "";
        public bool deleteAfterImport = true;
        
        // Projectのパスを保存するための定数。
        public const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + nameof(AssetImportWindow);
        
        // GoogleDriveからインポートしたファイルを処理する一時フォルダのパス。
        public const string TEMP_EXTRACT_PATH = "Library/GoogleDriveDownloaderTemp";

        // アセンブリロードを跨いで各種パラメーターを保持するためのSessionStateキー群。
        // SessionStateはエディタのセッション中のみ有効で、ドメインリロードやアセンブリリロードを跨いでデータを保持できる仕組み。

        // SESSION_KEYは、インポート待ちのパッケージのキューをJSON化して保存するためのキー。QueueDataクラスをJSON化して保存する。
        // WAITING_KEYは、インポート処理が完了するまで次のインポートを開始しないためのboolフラグ。trueの場合はインポート待ち状態。
        // PROGRESS_VALUE_KEYとPROGRESS_MESSAGE_KEYは、インポートの進捗状況を管理ウィンドウで表示する用。値はfloatとstringをとる。
        public const string SESSION_KEY = "CustomPackageImporter_SessionQueue";
        public const string WAITING_KEY = "CustomPackageImporter_Waiting";
        public const string PROGRESS_VALUE_KEY = "CustomPackageImporter_ProgressValue";
        public const string PROGRESS_MESSAGE_KEY = "CustomPackageImporter_ProgressMessage";

        // Google Drive OAuth認証用のURL群。
        // LOCALHOST_URLはOAuthのリダイレクトURIとして使用。ローカルで認証コードを受け取るためのURL。
        // TOKEN_URLはアクセストークンを取得するためのエンドポイント。
        // OAUTH_AUTH_URLは認証ページへのリダイレクト先。
        // DRIVE_READONLY_SCOPEは要求する権限スコープ。
        public const string LOCALHOST_URL = "http://localhost:5000/";
        public const string TOKEN_URL = "https://oauth2.googleapis.com/token";
        public const string OAUTH_AUTH_URL = "https://accounts.google.com/o/oauth2/v2/auth";
        public const string DRIVE_READONLY_SCOPE = "https://www.googleapis.com/auth/drive.readonly";

        // Google Drive API のエンドポイント群。
        // DRIVE_API_FILES_ENDPOINTはファイル検索・一覧取得用。
        // DRIVE_DOWNLOAD_ENDPOINT_TEMPLATEはファイルダウンロード用（{0}をプレースホルダーとして使用）。
        public const string DRIVE_API_FILES_ENDPOINT = "https://www.googleapis.com/drive/v3/files";
        public const string DRIVE_DOWNLOAD_ENDPOINT_TEMPLATE = "https://www.googleapis.com/drive/v3/files/{0}?alt=media";

        // ファイル拡張子関連の定数。
        // UNITYPACKAGE_EXTENSIONはUnityで使用するパッケージファイルの拡張子。
        public const string EXT_UNITYPACKAGE = ".unitypackage";

        public void Save()
        {
            // データをディスクに強制書き込み
            Save(true);
        }
    }
}
#endif