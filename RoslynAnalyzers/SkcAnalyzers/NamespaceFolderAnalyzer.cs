using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     Assets/Scripts/Runtime配下の名前空間が、レイヤー番号を除いたフォルダ構成と一致するかを検査する。
    ///     例: <c>1.Domain/InGame/Skill</c> は <c>KillChord.Runtime.Domain.InGame.Skill</c>。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamespaceFolderAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.NamespaceFolder,
            "名前空間はフォルダ構成と一致させる",
            "名前空間 '{0}' がフォルダ構成と一致しません。期待値は '{1}' です");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNamespace,
                SyntaxKind.NamespaceDeclaration,
                SyntaxKind.FileScopedNamespaceDeclaration);
        }

        /// <summary> 名前空間宣言を、ファイルのフォルダから求めた期待値と比べる。 </summary>
        private static void AnalyzeNamespace(SyntaxNodeAnalysisContext context)
        {
            var declaration = (BaseNamespaceDeclarationSyntax)context.Node;

            // 入れ子の名前空間宣言は外側と合成されるため、最も外側のみを検査する。
            if (declaration.Parent is BaseNamespaceDeclarationSyntax)
            {
                return;
            }

            if (!SkcSyntax.TryGetRuntimeFolders(context.Node.SyntaxTree.FilePath, out var folders) ||
                folders.Length == 0)
            {
                return;
            }

            var expected = SkcSyntax.RuntimeNamespace + "." + string.Join(".", folders);
            var actual = declaration.Name.ToString();
            if (actual == expected)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Name.GetLocation(), actual, expected));
        }
    }
}
