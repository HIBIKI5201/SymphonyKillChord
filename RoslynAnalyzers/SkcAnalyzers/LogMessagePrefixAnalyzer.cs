using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     Debug.LogError/LogWarningのメッセージが <c>[{nameof(ClassName)}]</c> で始まるかを検査する。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LogMessagePrefixAnalyzer : DiagnosticAnalyzer
    {
        private const string DebugTypeName = "UnityEngine.Debug";
        private const string LogErrorName = "LogError";
        private const string LogWarningName = "LogWarning";
        private const string ExpectedPrefix = "[{nameof(";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            SkcDiagnosticIds.LogMessagePrefix,
            "ログは [{nameof(ClassName)}] 形式で始める",
            "Debug.{0} のメッセージは $\"[{{nameof(ClassName)}}] ...\" の形式で書いてください",
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
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        /// <summary> 呼び出し式がDebug.LogError/LogWarningなら第1引数を検査する。 </summary>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!(context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is IMethodSymbol method))
            {
                return;
            }

            if (method.ContainingType?.ToDisplayString() != DebugTypeName ||
                (method.Name != LogErrorName && method.Name != LogWarningName))
            {
                return;
            }

            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                return;
            }

            var message = invocation.ArgumentList.Arguments[0].Expression;

            // 補間文字列の先頭が "[{nameof(" で始まっていれば規約通り。
            if (message is InterpolatedStringExpressionSyntax interpolated &&
                interpolated.Contents.Count >= 2 &&
                interpolated.Contents[0] is InterpolatedStringTextSyntax head &&
                head.TextToken.ValueText == "[" &&
                interpolated.Contents[1] is InterpolationSyntax interpolation &&
                interpolation.Expression.ToString().StartsWith("nameof(") &&
                interpolated.ToString().StartsWith("$\"" + ExpectedPrefix))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, message.GetLocation(), method.Name));
        }
    }
}
