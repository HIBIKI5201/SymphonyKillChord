using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     async void は、本体全体をtry/catchで囲み、catchで <c>Debug.LogException</c> を呼ぶものだけを許可する。
    ///     (Unityのイベント関数やイベントハンドラなど、戻り値をvoidにしなければならない場合のみ使う想定。)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AsyncVoidAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.AsyncVoid,
            "async void は本体全体をtry/catchで囲む",
            "async void メソッド '{0}' は本体全体を try/catch で囲み、catch で Debug.LogException を呼んでください。それ以外は Task / Awaitable / UniTask を返してください");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        /// <summary> async voidメソッドの本体の形を検査する。 </summary>
        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            if (!SkcSyntax.IsTarget(context.Node.SyntaxTree))
            {
                return;
            }

            var method = (MethodDeclarationSyntax)context.Node;

            if (!SkcSyntax.HasModifier(method.Modifiers, SyntaxKind.AsyncKeyword) ||
                !(method.ReturnType is PredefinedTypeSyntax predefined) ||
                !predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
            {
                return;
            }

            if (HasWrappingTryCatch(method))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text));
        }

        /// <summary> 本体が単一のtry文で、catchのいずれかがLogExceptionを呼んでいるか。 </summary>
        private static bool HasWrappingTryCatch(MethodDeclarationSyntax method)
        {
            if (method.Body == null || method.Body.Statements.Count != 1)
            {
                return false;
            }

            if (!(method.Body.Statements[0] is TryStatementSyntax tryStatement) || tryStatement.Catches.Count == 0)
            {
                return false;
            }

            return tryStatement.Catches.Any(c => c.Block.ToString().Contains("LogException"));
        }
    }
}
