using Microsoft.CodeAnalysis;

namespace SymphonyKillChord.Analyzers
{
    /// <summary> DiagnosticDescriptorの生成を共通化する。 </summary>
    internal static class SkcDescriptors
    {
        /// <summary> 既定でwarningの診断を作る。 </summary>
        public static DiagnosticDescriptor Create(string id, string title, string message)
        {
            return new DiagnosticDescriptor(
                id, title, message, SkcDiagnosticIds.Category, DiagnosticSeverity.Warning, true);
        }
    }
}
