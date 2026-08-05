using System;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using UnityEngine;

namespace KillChord.Editor.AssetManagement
{
    /// <summary>
    /// Service Account JSON 鍵ファイルから認証に必要なフィールドを抽出する DTO。
    /// </summary>
    [Serializable]
    internal class ServiceAccountKeyFile
    {
        /// <summary> Service Account のメールアドレス。 </summary>
        public string client_email;
        /// <summary> 秘密鍵 (PEM 形式)。 </summary>
        public string private_key;
    }

    /// <summary>
    /// Google Drive API v3 への認証処理を行うユーティリティ。Service Account 認証に対応。
    /// </summary>
    internal static class DriveAuthProvider
    {
        /// <summary> Drive API へのアクセス権限。読み取り専用。 </summary>
        private static readonly string[] Scopes =
        {
            "https://www.googleapis.com/auth/drive.readonly"
        };

        /// <summary>
        /// Service Account JSON 鍵を使用して Google Drive API の認証情報を取得する。
        /// </summary>
        /// <param name="serviceAccountJsonKey"> Service Account JSON 鍵の文字列。 </param>
        /// <returns> Service Account 認証情報。 </returns>
        /// <exception cref="InvalidOperationException"> JSON 解析失敗またはフィールド不足。 </exception>
        public static ServiceAccountCredential GetCredential(string serviceAccountJsonKey)
        {
            if (string.IsNullOrEmpty(serviceAccountJsonKey))
            {
                throw new InvalidOperationException("Service Account JSON鍵が設定されていません。");
            }

            ServiceAccountKeyFile keyFile;
            try
            {
                keyFile = JsonUtility.FromJson<ServiceAccountKeyFile>(serviceAccountJsonKey);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Service Account JSON鍵の解析に失敗しました: {e.Message}");
            }

            if (keyFile == null
                || string.IsNullOrEmpty(keyFile.client_email)
                || string.IsNullOrEmpty(keyFile.private_key))
            {
                throw new InvalidOperationException(
                    "Service Account JSON鍵の形式が不正です(client_email/private_keyを取得できません)。");
            }

            var initializer = new ServiceAccountCredential.Initializer(keyFile.client_email)
            {
                Scopes = Scopes
            }.FromPrivateKey(keyFile.private_key);

            return new ServiceAccountCredential(initializer);
        }
    }
}
