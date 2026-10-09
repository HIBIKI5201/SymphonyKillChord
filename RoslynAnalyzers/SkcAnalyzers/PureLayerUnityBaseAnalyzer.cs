using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     ピュア層(Domain/Application/Adaptor)の型がUnityのオブジェクト基底を継承していないかを検査する。
    ///     DesignPhilosophy.md: ピュア層でMonoBehaviour継承は許容されない。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PureLayerUnityBaseAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string[] PureLayers = { "Domain", "Application", "Adaptor" };
        private static readonly string[] ForbiddenBases = { "MonoBehaviour", "ScriptableObject" };

        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.PureLayerUnityBase,
            "ピュア層はUnityのオブジェクトを継承しない",
            "{0} 層の型 '{1}' が {2} を継承しています。ピュアクラスにするか、View/InfraStructure層へ移してください");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        /// <summary> 型の基底クラス列にUnityのオブジェクト基底が含まれるかを調べる。 </summary>
        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (!SkcSyntax.IsTarget(type))
            {
                return;
            }

            var ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

            if (!SkcSyntax.TryParseRuntimeNamespace(ns, out var layer, out _) ||
                System.Array.IndexOf(PureLayers, layer) < 0)
            {
                return;
            }

            for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                if (System.Array.IndexOf(ForbiddenBases, baseType.Name) < 0)
                {
                    continue;
                }

                // UnityEngine名前空間、または参照解決できていない(エラー型)場合のみ対象とする。
                var baseNs = baseType.ContainingNamespace?.ToDisplayString();
                if (baseNs != "UnityEngine" && baseType.TypeKind != TypeKind.Error)
                {
                    continue;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, type.Locations[0], layer, type.Name, baseType.Name));
                return;
            }
        }
    }
}
