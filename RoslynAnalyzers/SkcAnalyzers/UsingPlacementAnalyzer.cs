using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary> using ディレクティブをファイルの先頭にまとめることを検査する。 </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UsingPlacementAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.UsingPlacement,
            "using はファイルの先頭にまとめる",
            "using ディレクティブ '{0}' をファイルの先頭にまとめてください");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
        }

        /// <summary> 名前空間の内側にある、または型宣言より後ろにあるusingを報告する。 </summary>
        private static void AnalyzeUsing(SyntaxNodeAnalysisContext context)
        {
            if (!SkcSyntax.IsTarget(context.Node.SyntaxTree))
            {
                return;
            }

            var directive = (UsingDirectiveSyntax)context.Node;

            if (directive.Parent is CompilationUnitSyntax unit)
            {
                // 名前空間や型の宣言より後ろに書かれたusingを指摘する。
                var firstMember = unit.Members.FirstOrDefault();
                if (firstMember == null || directive.SpanStart < firstMember.SpanStart)
                {
                    return;
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, directive.GetLocation(), directive.Name?.ToString() ?? string.Empty));
        }
    }
}
