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

        public List<TicketData> CachedTickets => cachedTickets;
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
