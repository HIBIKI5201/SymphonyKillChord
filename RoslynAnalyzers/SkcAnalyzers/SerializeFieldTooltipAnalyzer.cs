using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     SerializeField属性を持つフィールドにTooltipAttributeが付いているかを検査する。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SerializeFieldTooltipAnalyzer : DiagnosticAnalyzer
    {
        private const string SerializeFieldName = "UnityEngine.SerializeField";
        private const string TooltipName = "UnityEngine.TooltipAttribute";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            SkcDiagnosticIds.SerializeFieldTooltip,
            "SerializeFieldにはTooltipが必要",
            "フィールド '{0}' にTooltipAttributeによる説明を付けてください",
            SkcDiagnosticIds.Category,
            DiagnosticSeverity.Warning,
            true);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        }

        /// <summary> フィールドシンボルの属性を検査する。 </summary>
        private static void AnalyzeField(SymbolAnalysisContext context)
        {
            var field = (IFieldSymbol)context.Symbol;

            if (!SkcSyntax.IsTarget(field))
            {
                return;
            }

            var attributes = field.GetAttributes();

            // SerializeFieldが無ければ対象外。
            if (!attributes.Any(a => a.AttributeClass?.ToDisplayString() == SerializeFieldName))
            {
                return;
            }

            if (attributes.Any(a => a.AttributeClass?.ToDisplayString() == TooltipName))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, field.Locations[0], field.Name));
        }
    }
}
