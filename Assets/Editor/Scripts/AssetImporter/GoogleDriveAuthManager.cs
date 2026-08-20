using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KillChord.Editor.AssetImporter.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace KillChord.Editor.AssetImporter
{
    /// <summary>
    ///     Google Drive APIのOAuth認証を管理するクラス。
    ///     ローカルでHTTPリスナーを立てて、
    ///     GoogleのOAuthリダイレクトをキャッチしてアクセストークンを取得する。
    /// </summary>
    public static class GoogleDriveAuthManager
    {
        /// <summary>
        ///     OAuthフローを開始する。
        ///     ブラウザで認証ページを開き、認証後にローカルでHTTPリスナーがコードを受け取る。
        /// </summary>
        public static async Task StartOAuthFlowAsync()
        {
            var settings = AssetImportSettings.instance;
            string authUrl =
                $"{AssetImportSettings.OAUTH_AUTH_URL}?client_id={settings.clientId}&redirect_uri={UnityWebRequest.EscapeURL(AssetImportSettings.LOCALHOST_URL)}&response_type=code&scope={UnityWebRequest.EscapeURL(AssetImportSettings.DRIVE_READONLY_SCOPE)}&access_type=offline&prompt=consent";

            System.Diagnostics.Process.Start(authUrl);

            using HttpListener listener = new HttpListener();
            listener.Prefixes.Add(AssetImportSettings.LOCALHOST_URL);
            listener.Start();

            HttpListenerContext context = await listener.GetContextAsync();
            string code = context.Request.QueryString["code"];

            byte[] buffer =
                Encoding.UTF8.GetBytes("<html><body>Authentication complete. You can close this window.</body></html>");
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
            listener.Stop();

            if (!string.IsNullOrEmpty(code))
            {
                await ExchangeCodeForTokensAsync(code);
            }
        }

        /// <summary>
        ///     アクセストークンをリフレッシュする。
        ///     リフレッシュトークンを使用して新しいアクセストークンを取得し、設定に保存する。
        /// </summary>
        public static async Task RefreshAccessTokenAsync()
        {
            var settings = AssetImportSettings.instance;
            if (string.IsNullOrEmpty(settings.refreshToken))
            {
                throw new Exception("Refresh token is missing. Please authorize first.");
            }

            WWWForm form = new WWWForm();
            form.AddField("client_id", settings.clientId);
            form.AddField("client_secret", settings.clientSecret);
            form.AddField("refresh_token", settings.refreshToken);
            form.AddField("grant_type", "refresh_token");

            using UnityWebRequest request = UnityWebRequest.Post(AssetImportSettings.TOKEN_URL, form);
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                OAuthTokenResponse response =
                    JsonUtility.FromJson<OAuthTokenResponse>(request.downloadHandler.text);
                settings.accessToken = response.access_token;
                settings.Save();
            }
            else
            {
                throw new Exception($"Failed to refresh access token: {request.downloadHandler.text}");
            }
        }

        /// <summary>
        ///     認証コードをアクセストークンとリフレッシュトークンに交換する。
        ///     認証コードをGoogleのトークンエンドポイントに送信し、レスポンスからトークンを取得して保存する。
        /// </summary>
        /// <param name="code">認証コード</param>
        private static async Task ExchangeCodeForTokensAsync(string code)
        {
            var settings = AssetImportSettings.instance;

            WWWForm form = new WWWForm();
            form.AddField("code", code);
            form.AddField("client_id", settings.clientId);
            form.AddField("client_secret", settings.clientSecret);
            form.AddField("redirect_uri", AssetImportSettings.LOCALHOST_URL);
            form.AddField("grant_type", "authorization_code");

            using UnityWebRequest request = UnityWebRequest.Post(AssetImportSettings.TOKEN_URL, form);
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                OAuthTokenResponse response =
                    JsonUtility.FromJson<OAuthTokenResponse>(request.downloadHandler.text);
                settings.accessToken = response.access_token;
                if (!string.IsNullOrEmpty(response.refresh_token))
                {
                    settings.refreshToken = response.refresh_token;
                }

                settings.Save();
                Debug.Log("OAuth Authentication Successful.");
            }
            else
            {
                throw new Exception($"OAuth Exchange Error: {request.downloadHandler.text}");
            }
        }

        /// <summary> OAuthトークンエンドポイントからのレスポンスを表すクラス。 </summary>
        [Serializable]
        public class OAuthTokenResponse
        {
            public string access_token;
            public string refresh_token;
        }
    }
}