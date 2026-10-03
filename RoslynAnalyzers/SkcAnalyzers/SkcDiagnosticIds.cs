namespace SymphonyKillChord.Analyzers
{
    /// <summary> アナライザの診断IDを集約する。 </summary>
    internal static class SkcDiagnosticIds
    {
        /// <summary> SerializeFieldにTooltipが無い。 </summary>
        public const string SerializeFieldTooltip = "SKC0001";

        /// <summary> Debug.LogError/LogWarningのメッセージが規定の接頭辞形式でない。 </summary>
        public const string LogMessagePrefix = "SKC0002";

        /// <summary> 診断のカテゴリ名。 </summary>
        public const string Category = "SymphonyKillChord.CodeGuidelines";
    }
}
