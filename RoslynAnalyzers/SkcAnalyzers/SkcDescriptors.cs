using Microsoft.CodeAnalysis;

namespace SymphonyKillChord.Analyzers
{
    /// <summary> DiagnosticDescriptorの生成を共通化する。 </summary>
    internal static class SkcDescriptors
    {
        /// <summary>
        ///     診断を作る。既定の深刻度はwarning。
        ///     既存コードの違反が多いルールはInfoを渡す(Unityのコンソールに出さず、IDEにだけ表示する)。
        ///     Unityのコンパイラは.editorconfigの深刻度を反映しない場合があるため、既定値をここで決めておく。
        /// </summary>
        public static DiagnosticDescriptor Create(
            string id, string title, string message, DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        {
            return new DiagnosticDescriptor(
                id, title, message, SkcDiagnosticIds.Category, severity, true);
        }
    }
}
