using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevelopProducts.TicketSystem
{
    /// <summary>
    /// 外部APIから取得したチケットデータをキャッシュするクラス
    /// </summary>
    public class CachedTicketDataSingleton : ScriptableSingleton<CachedTicketDataSingleton>
    {
        private readonly List<TicketData> cachedTickets = new();

        /// <summary>
        /// キャッシュされたチケットデータをすべて取得する。
        /// </summary>
        /// <returns></returns>
        public List<TicketData> GetAll()
        {
            return cachedTickets;
        }

        /// <summary>
        /// キャッシュされたチケットデータをすべてクリアする。
        /// </summary>
        public void Clear()
        {
            cachedTickets.Clear();
        }

        /// <summary>
        /// チケットデータのリストをキャッシュにセットする。
        /// </summary>
        /// <param name="ticketDataList"></param>
        public void Set(List<TicketData> ticketDataList)
        {
            cachedTickets.Clear();
            cachedTickets.AddRange(ticketDataList);
            Sort(cachedTickets);
            OnTicketAdded?.Invoke();
        }

        /// <summary>
        /// チケットデータのリストをソートする。
        /// ソートの優先順位は以下の通り。
        /// 1. 自分のチケットを優先
        /// 2. 空きチケットを優先
        /// 3. シーン名でソート
        /// </summary>
        /// <param name="ticketDataList"></param>
        private static void Sort(List<TicketData> ticketDataList)
        {
            var currentUserName = TicketSystemSettings.instance.userName;
            if (string.IsNullOrEmpty(currentUserName))
            {
                Debug.LogWarning($"ユーザー名が設定されていません。");
                return;
            }

            ticketDataList.Sort((a, b) =>
            {
                // 自分のチケットを優先
                var aIsMine = a.userName == currentUserName;
                var bIsMine = b.userName == currentUserName;
                if (aIsMine != bIsMine) return aIsMine ? -1 : 1;

                // 次に空きチケットを優先
                if (a.isInUse != b.isInUse) return a.isInUse ? 1 : -1;

                // 最後にシーン名でソート
                return string.Compare(a.sceneName, b.sceneName, StringComparison.Ordinal);
            });
        }

        /// <summary>
        /// チケットが追加されたときに呼ばれるイベント。
        /// UI側でこのイベントを購読しておけば、チケットが追加されたときに自動的にUIを更新できる。
        /// </summary>
        public event Action OnTicketAdded;
    }

    // --- 内部データ構造 ---
    [Serializable]
    public class TicketData
    {
        public string id;
        public string sceneName;
        public bool isInUse;
        public string userName;
        public string masterPath;
        public string timestamp;
    }
}