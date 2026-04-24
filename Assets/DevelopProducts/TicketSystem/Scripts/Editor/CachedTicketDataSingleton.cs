using System;
using System.Collections.Generic;
using UnityEditor;

namespace DevelopProducts.TicketSystem
{
    /// <summary>
    /// 外部APIから取得したチケットデータをキャッシュするクラス
    /// </summary>
    public class CachedTicketDataSingleton : ScriptableSingleton<CachedTicketDataSingleton>
    {
        private readonly List<TicketData> cachedTickets = new();

        public List<TicketData> GetAll()
        {
            return cachedTickets;
        }

        public void Clear()
        {
            cachedTickets.Clear();
        }
        
        public void Set(IEnumerable<TicketData> ticketDataList)
        {
            cachedTickets.Clear();
            cachedTickets.AddRange(ticketDataList);
            OnTicketAdded?.Invoke();
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