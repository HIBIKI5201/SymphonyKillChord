using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     サマリーの有無(SKC0017)と、日本語サマリーが「。」で終わるか(SKC0018)を検査する。
    ///     全メソッドと、公開のプロパティ・イベントにサマリーが必要。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DocumentationAnalyzer : DiagnosticAnalyzer
    {
        private static readonly Regex Japanese = new Regex(@"[぀-ヿ一-鿿]", RegexOptions.Compiled);
        private static readonly Regex SummaryBody =
            new Regex(@"<summary>(?<body>.*?)</summary>", RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly DiagnosticDescriptor MissingRule = SkcDescriptors.Create(
            SkcDiagnosticIds.MissingSummary,
            "メソッドと公開プロパティ・イベントにサマリーを付ける",
            "'{0}' にサマリー(/// <summary>)を付けてください");

        private static readonly DiagnosticDescriptor PeriodRule = SkcDescriptors.Create(
            SkcDiagnosticIds.SummaryPeriod,
            "日本語のサマリーは「。」で終える",
            "'{0}' のサマリーが「。」で終わっていません");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(MissingRule, PeriodRule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeMember,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.ConstructorDeclaration);
        }

        /// <summary> サマリーの有無と末尾の句点を検査する。 </summary>
        private static void AnalyzeMember(SyntaxNodeAnalysisContext context)
        {
            var member = (MemberDeclarationSyntax)context.Node;
            var comment = GetDocComment(member);
            var name = GetName(member);
            var location = GetNameLocation(member);

            if (comment == null)
            {
                if (RequiresSummary(member))
                {
                    context.ReportDiagnostic(Diagnostic.Create(MissingRule, location, name));
                }

                return;
            }

            // <inheritdoc/> は継承元のサマリーを引くため、句点の検査から除く。
            var match = SummaryBody.Match(comment);
            if (!match.Success)
            {
                return;
            }

            // 行頭の /// と、<para> や <see/> などのタグを除いた本文の末尾を見る。
            var withoutPrefix = Regex.Replace(match.Groups["body"].Value, @"^\s*///", string.Empty, RegexOptions.Multiline);
            var body = Regex.Replace(withoutPrefix, @"<[^>]*>", string.Empty).Trim();
            if (body.Length > 0 && Japanese.IsMatch(body) && !body.EndsWith("。"))
            {
                context.ReportDiagnostic(Diagnostic.Create(PeriodRule, location, name));
            }
        }

        /// <summary> サマリーが必須の宣言か。メソッドは全て、プロパティ・イベントは公開のみ。 </summary>
        private static bool RequiresSummary(MemberDeclarationSyntax member)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    // partial定義と、インターフェース実装の明示実装は継承元で説明される。
                    return method.ExplicitInterfaceSpecifier == null;
                case PropertyDeclarationSyntax property:
                    return IsPublic(property.Modifiers, property.Parent);
                case EventDeclarationSyntax evt:
                    return IsPublic(evt.Modifiers, evt.Parent);
                case EventFieldDeclarationSyntax eventField:
                    return IsPublic(eventField.Modifiers, eventField.Parent);
                default:
                    return false;
            }
        }

        /// <summary> publicか、インターフェース内の要素か。 </summary>
        private static bool IsPublic(SyntaxTokenList modifiers, SyntaxNode? parent)
        {
            return SkcSyntax.HasModifier(modifiers, SyntaxKind.PublicKeyword) || parent is InterfaceDeclarationSyntax;
        }

        /// <summary> 宣言直前の /// コメントを返す。無ければnull。 </summary>
        private static string? GetDocComment(MemberDeclarationSyntax member)
        {
            // 構造化されたドキュメントコメントだけでなく、通常コメントとして解析された /// も拾う。
            var trivia = member.GetLeadingTrivia();
            var text = string.Concat(
                trivia.Where(t => t.ToFullString().TrimStart().StartsWith("///")).Select(t => t.ToFullString()));
            return text.Length == 0 ? null : text;
        }

        /// <summary> 診断用の名前。 </summary>
        private static string GetName(MemberDeclarationSyntax member)
        {
            switch (member)
            {
                case BaseTypeDeclarationSyntax type: return type.Identifier.Text;
                case MethodDeclarationSyntax method: return method.Identifier.Text;
                case PropertyDeclarationSyntax property: return property.Identifier.Text;
                case ConstructorDeclarationSyntax constructor: return constructor.Identifier.Text;
                case EventDeclarationSyntax evt: return evt.Identifier.Text;
                case BaseFieldDeclarationSyntax field: return field.Declaration.Variables[0].Identifier.Text;
                default: return member.ToString();
            }
        }

        /// <summary> 診断を付ける位置。 </summary>
        private static Location GetNameLocation(MemberDeclarationSyntax member)
        {
            switch (member)
            {
                case BaseTypeDeclarationSyntax type: return type.Identifier.GetLocation();
                case MethodDeclarationSyntax method: return method.Identifier.GetLocation();
                case PropertyDeclarationSyntax property: return property.Identifier.GetLocation();
                case ConstructorDeclarationSyntax constructor: return constructor.Identifier.GetLocation();
                case EventDeclarationSyntax evt: return evt.Identifier.GetLocation();
                case BaseFieldDeclarationSyntax field: return field.Declaration.Variables[0].Identifier.GetLocation();
                default: return member.GetLocation();
            }
        }
    }
}
