using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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
        /// GASにリクエストを送ってチケットの最新一覧を取得し、ローカルのticketListを更新する。通信中はisLoadingをtrueにしてUIに反映させる。
        /// </summary>
        public static async Task RefreshList()
        {
            if (CachedTicketDataSingleton.instance == null || CachedTicketDataSingleton.instance.CachedTickets == null)
            {
                Debug.LogError("CachedTicketDataSingletonのインスタンスがありません。");
                return;
            }
            
            CachedTicketDataSingleton.instance.CachedTickets.Clear();

            if (TicketSystemSettings.instance == null)
            {
                Debug.LogError("TicketSystemSettingsのインスタンスがありません。");
                return;
            }

            var url = TicketSystemSettings.instance.gasUrl;
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("GASのURLが指定されていません。[Edit > ProjectSettings > TicketSystemSettings]からURLを設定してください。");
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                Debug.LogError("無効なGAS URLです。URLは有効なHTTPS形式である必要があります。");
                return;
            }

            var request = UnityWebRequest.Get(url);
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

                foreach (var ticketData in wrapper.items)
                {
                    CachedTicketDataSingleton.instance.CachedTickets.Add(ticketData);
                }
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
        public static async Task CreateTicket(string sceneName, string path, string currentUserName)
        {
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
        public static async Task UpdateTicketStatus(TicketData ticket, string currentUserName)
        {
            ticket.userName = currentUserName;
            ticket.isInUse = !ticket.isInUse;
            await SendPost("update", ticket);
        }

        /// <summary>
        /// GASに対してチケットの作成・更新リクエストを送る共通関数。レスポンスの内容に応じてダイアログを出したり、一覧を更新したりする。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        private static async Task SendPost(string action, TicketData data)
        {
            var json = JsonUtility.ToJson(data);
            var hashedKey = GetSha256Hash(TicketSystemSettings.instance.apiKey);
            json = json.Insert(1, $"\"action\":\"{action}\",\"key\":\"{hashedKey}\",");

            var request = new UnityWebRequest(TicketSystemSettings.instance.gasUrl, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            var response = request.downloadHandler.text;

            if (response.StartsWith("ERROR_ALREADY_IN_USE"))
            {
                var occupant = response.Replace("ERROR_ALREADY_IN_USE:", "");
                EditorUtility.DisplayDialog("発行失敗",
                    $"このシーンは現在 {occupant} さんが使用中です。\n作業を始める前に本人に確認してください。", "了解");
            }
            else if (response == "SUCCESS")
            {
                EditorUtility.DisplayDialog("完了", "チケットの更新が完了しました。", "OK");
            }
            else
            {
                Debug.Log(response);
            }

            await RefreshList();
        }

        /// <summary>
        /// 文字列を受け取って、その文字列のSHA256ハッシュを返す。
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