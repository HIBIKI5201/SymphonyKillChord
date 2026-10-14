using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     using ディレクティブから、レイヤーの参照方向(SKC0021)と、他モジュールへの直接依存(SKC0020)を検査する。
    ///     DesignPhilosophy.md: 他モジュールへの依存はAdaptor層のみが行い、Composition層が解決する。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DependencyDirectionAnalyzer : DiagnosticAnalyzer
    {
        private const string UtilityLayer = "Utility";

        /// <summary> 各レイヤーが参照してよいレイヤー(Utilityは全レイヤーから参照可)。 </summary>
        private static readonly Dictionary<string, string[]> AllowedLayers = new Dictionary<string, string[]>
        {
            { "Domain", new string[0] },
            { "Application", new[] { "Domain" } },
            { "Adaptor", new[] { "Domain", "Application" } },
            { "View", new[] { "Adaptor" } },
            { "InfraStructure", new[] { "Domain", "Application", "View" } },
        };

        /// <summary> 他モジュールへの依存を許す(依存解決を担う)レイヤー。 </summary>
        private static readonly string[] CrossModuleLayers = { "Adaptor", "Composition" };

        private static readonly DiagnosticDescriptor LayerRule = SkcDescriptors.Create(
            SkcDiagnosticIds.LayerDependency,
            "レイヤーの参照方向を守る",
            "{0} 層から {1} 層への参照 '{2}' は許可されていません。自層にインターフェースを置き、Compositionで注入してください");

        private static readonly DiagnosticDescriptor ModuleRule = SkcDescriptors.Create(
            SkcDiagnosticIds.CrossModuleDependency,
            "他モジュールへの依存はAdaptor層のみ",
            "{0} 層の {1} モジュールが {2} モジュール '{3}' に直接依存しています。Adaptor層を介してください",
            DiagnosticSeverity.Info);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(LayerRule, ModuleRule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
        }

        /// <summary> using の対象名前空間を、所属する型の名前空間と比べる。 </summary>
        private static void AnalyzeUsing(SyntaxNodeAnalysisContext context)
        {
            var directive = (UsingDirectiveSyntax)context.Node;

            if (!SkcSyntax.IsTarget(directive.SyntaxTree))
            {
                return;
            }

            if (directive.Name == null)
            {
                return;
            }

            var ownNamespace = FindOwnNamespace(directive);
            if (ownNamespace == null ||
                !SkcSyntax.TryParseRuntimeNamespace(ownNamespace, out var ownLayer, out var ownModule) ||
                !SkcSyntax.TryParseRuntimeNamespace(directive.Name.ToString(), out var targetLayer, out var targetModule))
            {
                return;
            }

            if (targetLayer == UtilityLayer || (ownLayer == targetLayer && ownModule == targetModule))
            {
                return;
            }

            // レイヤーの参照方向。Compositionは全レイヤーを参照できる。
            if (ownLayer != targetLayer &&
                AllowedLayers.TryGetValue(ownLayer, out var allowed) &&
                Array.IndexOf(allowed, targetLayer) < 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    LayerRule, directive.Name.GetLocation(), ownLayer, targetLayer, directive.Name.ToString()));
                return;
            }

            // 他モジュールへの直接依存。
            if (ownModule != null && targetModule != null && ownModule != targetModule &&
                Array.IndexOf(CrossModuleLayers, ownLayer) < 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ModuleRule, directive.Name.GetLocation(), ownLayer, ownModule, targetModule, directive.Name.ToString()));
            }
        }

        /// <summary> ファイル内の最初の名前空間宣言から、このファイルが属する名前空間を取り出す。 </summary>
        private static string? FindOwnNamespace(SyntaxNode node)
        {
            var root = node.SyntaxTree.GetRoot();
            foreach (var descendant in root.DescendantNodes(n => !(n is BaseNamespaceDeclarationSyntax)))
            {
                if (descendant is BaseNamespaceDeclarationSyntax declaration)
                {
                    return declaration.Name.ToString();
                }
            }

            return null;
        }
    }
}
