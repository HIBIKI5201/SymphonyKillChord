using System;

namespace KillChord.Editor.TicketSystem
{
    /// <summary>
    ///     シーンの編集チケット1件のデータ。
    /// </summary>
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
