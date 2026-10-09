using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary> 一行のブロックでも波カッコを使うことを検査する。 </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BraceAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.Braces,
            "制御文は波カッコで囲む",
            "{0} の本体は波カッコ {{ }} で囲んでください",
            DiagnosticSeverity.Info);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeStatement,
                SyntaxKind.IfStatement,
                SyntaxKind.ElseClause,
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.WhileStatement,
                SyntaxKind.DoStatement);
        }

        /// <summary> 制御文の本体がブロックでなければ報告する。 </summary>
        private static void AnalyzeStatement(SyntaxNodeAnalysisContext context)
        {
            if (!SkcSyntax.IsTarget(context.Node.SyntaxTree))
            {
                return;
            }

            StatementSyntax body;
            string name;
            switch (context.Node)
            {
                case IfStatementSyntax ifStatement:
                    body = ifStatement.Statement;
                    name = "if";
                    break;
                case ElseClauseSyntax elseClause:
                    // else if は連鎖として許容する。
                    body = elseClause.Statement;
                    name = "else";
                    if (body is IfStatementSyntax)
                    {
                        return;
                    }

                    break;
                case ForStatementSyntax forStatement:
                    body = forStatement.Statement;
                    name = "for";
                    break;
                case ForEachStatementSyntax forEach:
                    body = forEach.Statement;
                    name = "foreach";
                    break;
                case WhileStatementSyntax whileStatement:
                    body = whileStatement.Statement;
                    name = "while";
                    break;
                case DoStatementSyntax doStatement:
                    body = doStatement.Statement;
                    name = "do";
                    break;
                default:
                    return;
            }

            if (body is BlockSyntax)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, body.GetLocation(), name));
        }
    }
}
