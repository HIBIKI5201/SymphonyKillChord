using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary> 型とメンバーにアクセス修飾子が明示されていることを検査する。 </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ExplicitAccessibilityAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.ExplicitAccessibility,
            "アクセス修飾子を明示する",
            "'{0}' のアクセス修飾子を明示してください");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeMember,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.RecordStructDeclaration,
                SyntaxKind.DelegateDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.ConstructorDeclaration);
        }

        /// <summary> アクセス修飾子が必要な宣言に修飾子が無ければ報告する。 </summary>
        private static void AnalyzeMember(SyntaxNodeAnalysisContext context)
        {
            if (!SkcSyntax.IsTarget(context.Node.SyntaxTree))
            {
                return;
            }

            var member = (MemberDeclarationSyntax)context.Node;
            var modifiers = member.Modifiers;

            if (SkcSyntax.HasAccessModifier(modifiers) || !RequiresModifier(member))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, GetNameToken(member).GetLocation(), GetNameToken(member).Text));
        }

        /// <summary> 修飾子を省略できない宣言か。インターフェースの要素・明示実装・staticコンストラクタは対象外。 </summary>
        private static bool RequiresModifier(MemberDeclarationSyntax member)
        {
            if (member.Parent is InterfaceDeclarationSyntax)
            {
                return false;
            }

            switch (member)
            {
                case MethodDeclarationSyntax method:
                    return method.ExplicitInterfaceSpecifier == null && !IsPartialDefinition(method);
                case PropertyDeclarationSyntax property:
                    return property.ExplicitInterfaceSpecifier == null;
                case EventDeclarationSyntax evt:
                    return evt.ExplicitInterfaceSpecifier == null;
                case ConstructorDeclarationSyntax constructor:
                    return !SkcSyntax.HasModifier(constructor.Modifiers, SyntaxKind.StaticKeyword);
                default:
                    return true;
            }
        }

        /// <summary> 実装を持たないpartialメソッド定義は、アクセス修飾子を持たないことがある。 </summary>
        private static bool IsPartialDefinition(MethodDeclarationSyntax method)
        {
            return method.Body == null &&
                   method.ExpressionBody == null &&
                   SkcSyntax.HasModifier(method.Modifiers, SyntaxKind.PartialKeyword);
        }

        /// <summary> 診断を付ける位置の名前トークンを返す。 </summary>
        private static SyntaxToken GetNameToken(MemberDeclarationSyntax member)
        {
            switch (member)
            {
                case BaseTypeDeclarationSyntax type: return type.Identifier;
                case DelegateDeclarationSyntax del: return del.Identifier;
                case MethodDeclarationSyntax method: return method.Identifier;
                case PropertyDeclarationSyntax property: return property.Identifier;
                case ConstructorDeclarationSyntax constructor: return constructor.Identifier;
                case EventDeclarationSyntax evt: return evt.Identifier;
                case BaseFieldDeclarationSyntax field:
                    return field.Declaration.Variables[0].Identifier;
                default: return member.GetFirstToken();
            }
        }
    }
}
