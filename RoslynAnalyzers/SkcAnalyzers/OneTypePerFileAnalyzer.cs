using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     1ファイルには1つの公開型のみを定義し、ファイル名を型名と一致させる。
    ///     ネストしたprivate型は例外。partialは <c>型名.補足.cs</c> も許可する。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OneTypePerFileAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.OneTypePerFile,
            "1ファイル1公開型で、ファイル名は型名と一致させる",
            "{0}");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeTree);
        }

        /// <summary> ファイル内の公開型を数え、ファイル名と比べる。 </summary>
        private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
        {
            var path = context.Tree.FilePath;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(SkcSyntax.NormalizePath(path));
            var types = context.Tree.GetRoot(context.CancellationToken)
                .DescendantNodes(n => n is CompilationUnitSyntax || n is BaseNamespaceDeclarationSyntax)
                .OfType<BaseTypeDeclarationSyntax>()
                .Where(t => SkcSyntax.HasModifier(t.Modifiers, SyntaxKind.PublicKeyword))
                .ToList();

            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                if (i > 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule, type.Identifier.GetLocation(),
                        $"1ファイルに複数の公開型があります。'{type.Identifier.Text}' を別ファイルに分けてください"));
                }
                else if (!NameMatches(fileName, type.Identifier.Text))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule, type.Identifier.GetLocation(),
                        $"ファイル名 '{fileName}' を型名 '{type.Identifier.Text}' に一致させてください"));
                }
            }
        }

        /// <summary> ファイル名が型名と一致するか。partialの補足ファイル(型名.補足)も許可する。 </summary>
        private static bool NameMatches(string fileName, string typeName)
        {
            return fileName == typeName ||
                   fileName.StartsWith(typeName + ".", StringComparison.Ordinal) ||
                   fileName.StartsWith(typeName + "`", StringComparison.Ordinal);
        }
    }
}
