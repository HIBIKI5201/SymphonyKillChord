using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace DevelopProducts.TicketSystem
{
    /// <summary>
    /// GASと通信してチケットの取得・作成・更新を行うクラス。
    /// GASのURLやAPIキーはTicketSystemSettingsから取得する。
    /// </summary>
    public static class TicketSystemWebClient
    {
        /// <summary>
        /// GASからのレスポンスをパースするためのラッパークラス。
        /// GASからはチケットの配列が返ってくるので、その配列をitemsフィールドに入れる。
        /// </summary>
        [Serializable]
        public class TicketListWrapper
        {
            public List<TicketData> items;
        }

        /// <summary>
        /// GASにリクエストを送ってチケットの最新一覧を取得し、ローカルのticketListを更新する。
        /// </summary>
        public static async UniTask RefreshList()
        {
            if (CachedTicketDataSingleton.instance == null)
            {
                Debug.LogError("CachedTicketDataSingletonのインスタンスがありません。");
                return;
            }

            CachedTicketDataSingleton.instance.Clear();

            if (TicketSystemSettings.instance == null)
            {
                Debug.LogError("TicketSystemSettingsのインスタンスがありません。");
                return;
            }

            var url = TicketSystemSettings.instance.gasUrl;
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("GASのURLが指定されていません。[Edit > ProjectSettings > TicketSystem]からURLを設定してください。");
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                Debug.LogError("無効なGAS URLです。URLは有効なHTTPS形式である必要があります。");
                return;
            }

            using var request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = "{\"items\":" + request.downloadHandler.text + "}";
                var wrapper = JsonUtility.FromJson<TicketListWrapper>(json);
                if (wrapper?.items == null)
                {
                    Debug.LogError("JSONのパースに失敗しました。レスポンスが正しい形式ではありません。");
                    return;
                }

                if (wrapper.items.Count == 0)
                {
                    Debug.LogWarning("取得したチケットはありませんでした。");
                }

                CachedTicketDataSingleton.instance.Set(wrapper.items);
            }
            else
            {
                Debug.LogError($"チケットの取得に失敗しました。HTTPエラーコード: {request.responseCode}");
            }
        }

        /// <summary>
        /// チケットを新規作成して使用開始する。GASにリクエストを送る前に、ローカルのticketオブジェクトの状態を使用中にしておく。
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="path"></param>
        /// <param name="currentUserName"></param>
        public static async UniTask CreateTicket(string sceneName, string path, string currentUserName)
        {
            if (string.IsNullOrEmpty(currentUserName))
            {
                Debug.LogWarning(
                    "ユーザー名が設定されていません。チケットを作成する前に、[Edit > ProjectSettings > TicketSystem]からユーザー名を設定してください。");
                return;
            }

            var ticket = new TicketData
            {
                id = Guid.NewGuid().ToString(),
                sceneName = sceneName,
                isInUse = true,
                userName = currentUserName,
                masterPath = path
            };

            await SendPost("create", ticket);
        }

        /// <summary>
        /// チケットの使用開始・解放を切り替える。GASに更新リクエストを送る前に、ローカルのticketオブジェクトの状態を切り替えておく。
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="currentUserName"></param>
        public static async UniTask UpdateTicketStatus(TicketData ticket, string currentUserName)
        {
            if (string.IsNullOrEmpty(currentUserName))
            {
                Debug.LogWarning(
                    "ユーザー名が設定されていません。チケットを作成する前に、[Edit > ProjectSettings > TicketSystem]からユーザー名を設定してください。");
                return;
            }

            ticket.userName = currentUserName;
            ticket.isInUse = !ticket.isInUse;
            await SendPost("update", ticket);
        }

        /// <summary>
        /// 指定されたチケットを一覧から完全に削除する。
        /// </summary>
        /// <param name="sceneName"></param>
        public static async UniTask DisposeTicket(string sceneName)
        {
            var ticket = new TicketData
            {
                sceneName = sceneName,
            };
            await SendPost("dispose", ticket);
        }

        /// <summary>
        /// GASに対してチケットの作成・更新リクエストを送る共通関数。レスポンスの内容に応じてダイアログを出したり、一覧を更新したりする。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        private static async UniTask SendPost(string action, TicketData data)
        {
            var json = JsonUtility.ToJson(data);
            var hashedKey = GetSha256Hash(TicketSystemSettings.instance.apiKey);
            json = json.Insert(1, $"\"action\":\"{action}\",\"key\":\"{hashedKey}\",");

            var url = TicketSystemSettings.instance.gasUrl;
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("GASのURLが指定されていません。[Edit > ProjectSettings > TicketSystem]からURLを設定してください。");
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                Debug.LogError("無効なGAS URLです。URLは有効なHTTPS形式である必要があります。");
                return;
            }

            // GASにアクセスする直前に、チケット情報のキャッシュをクリアする。
            CachedTicketDataSingleton.instance.Clear();

            try
            {
                using var request = new UnityWebRequest(url, "POST");
                var bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                var response = request.downloadHandler.text;

                if (response.StartsWith("ERROR_ALREADY_IN_USE"))
                {
                    var occupant = response.Replace("ERROR_ALREADY_IN_USE:", "");
                    EditorDialog.DisplayAlertDialog("発行失敗",
                        $"このシーンは現在 {occupant} さんが使用中です。\n作業を始める前に本人に確認してください。", "了解");
                }
                else if (response.StartsWith("ERROR_APIKEY_MISMATCH"))
                {
                    EditorDialog.DisplayAlertDialog("発行失敗",
                        "APIキーが正しくありません。URLとAPIキーの両方が正しいことを確認してください。", "了解");
                }
                else if (response == "SUCCESS")
                {
                    EditorDialog.DisplayAlertDialog("完了", "チケットの更新が完了しました。", "OK");
                }
                else
                {
                    // 未定義のGASからのログ処理をデバッグ用に出力。
                    Debug.Log(response);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"チケットの更新に失敗しました: {e.Message}");
                EditorDialog.DisplayAlertDialog("通信エラー", "サーバーとの通信に失敗しました。ネットワーク状態を確認してください。", "了解");
            }
            finally
            {
                // 成否に関わらず最新の状態を取得し、キャッシュを整合させる。
                await RefreshList();
            }
        }

        /// <summary>
        /// 文字列を受け取って、その文字列のSHA256ハッシュを返す。
        /// 簡易的なセキュリティ対策用。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string GetSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}