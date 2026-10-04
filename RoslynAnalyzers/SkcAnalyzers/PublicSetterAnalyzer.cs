using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     公開プロパティのpublic setを禁止する。変更は SetXxx / RecordXxx のようなメソッド経由にする。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PublicSetterAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.PublicSetter,
            "プロパティにpublic setを使わない",
            "プロパティ '{0}' にpublic setがあります。SetXxx/RecordXxx のようなメソッドを介してください");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        }

        /// <summary> publicなsetアクセサを持つプロパティを報告する。 </summary>
        private static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var property = (IPropertySymbol)context.Symbol;

            if (property.DeclaredAccessibility != Accessibility.Public ||
                property.SetMethod == null ||
                property.SetMethod.DeclaredAccessibility != Accessibility.Public ||
                property.SetMethod.IsInitOnly ||
                property.ContainingType.TypeKind == TypeKind.Interface ||
                property.IsIndexer ||
                SkcSyntax.IsNameFixedBySignature(property))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, property.Locations[0], property.Name));
        }
    }
}
