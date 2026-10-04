using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     CodeGuidelines.md の「順序」(22段階)に沿ってメンバーが並んでいるかを検査する。
    ///     直前の順位付き(判定できる)メンバーより順位が小さいメンバーを報告する。
    ///     1つの置き間違いが後続の全メンバーの報告に波及しないよう、隣接比較にしている。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MemberOrderAnalyzer : DiagnosticAnalyzer
    {
        private static readonly HashSet<string> UnityMessages = new HashSet<string>
        {
            "Awake", "OnEnable", "Start", "FixedUpdate", "Update", "LateUpdate", "OnDisable", "OnDestroy",
            "OnValidate", "Reset", "OnApplicationQuit", "OnApplicationPause", "OnApplicationFocus",
            "OnTriggerEnter", "OnTriggerStay", "OnTriggerExit", "OnTriggerEnter2D", "OnTriggerStay2D", "OnTriggerExit2D",
            "OnCollisionEnter", "OnCollisionStay", "OnCollisionExit", "OnCollisionEnter2D", "OnCollisionStay2D",
            "OnCollisionExit2D", "OnGUI", "OnDrawGizmos", "OnDrawGizmosSelected", "OnBecameVisible",
            "OnBecameInvisible", "OnRectTransformDimensionsChange", "OnTransformParentChanged",
            "OnTransformChildrenChanged", "OnPreRender", "OnPostRender", "OnRenderObject", "OnMouseDown",
            "OnMouseUp", "OnMouseEnter", "OnMouseExit", "OnMouseOver", "OnMouseDrag",
        };

        private static readonly DiagnosticDescriptor Rule = SkcDescriptors.Create(
            SkcDiagnosticIds.MemberOrder,
            "メンバーはCodeGuidelines.mdの順序で並べる",
            "'{0}'({1}) は直前の '{2}'({3}) より前に置いてください");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeType,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.RecordStructDeclaration);
        }

        /// <summary> 型の直下のメンバーを順に見て、順位が逆転した箇所を報告する。 </summary>
        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            var declaration = (TypeDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            var previousRank = 0;
            var previousName = string.Empty;
            var previousLabel = string.Empty;

            foreach (var member in declaration.Members)
            {
                // #if で囲まれたメンバーはデバッグ機能などの扱いが任意のため、判定に含めない。
                if (HasPreprocessorDirective(member))
                {
                    continue;
                }

                var rank = GetRank(member, semanticModel, context.CancellationToken, out var name, out var label);
                if (rank == 0)
                {
                    continue;
                }

                if (rank < previousRank)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule, GetLocation(member), name, label, previousName, previousLabel));
                }

                previousRank = rank;
                previousName = name;
                previousLabel = label;
            }
        }

        /// <summary> メンバーの順位(1〜21)を返す。判定できないメンバーは0。 </summary>
        private static int GetRank(
            MemberDeclarationSyntax member,
            SemanticModel model,
            System.Threading.CancellationToken token,
            out string name,
            out string label)
        {
            name = string.Empty;
            label = string.Empty;

            switch (member)
            {
                case ConstructorDeclarationSyntax constructor:
                    name = constructor.Identifier.Text;
                    return Rank(1, "コンストラクタ", out label);

                case EventFieldDeclarationSyntax eventField:
                    name = eventField.Declaration.Variables[0].Identifier.Text;
                    return IsPublic(eventField.Modifiers) ? Rank(2, "公開イベント", out label) : 0;

                case EventDeclarationSyntax evt:
                    name = evt.Identifier.Text;
                    return IsPublic(evt.Modifiers) ? Rank(2, "公開イベント", out label) : 0;

                case PropertyDeclarationSyntax property:
                    name = property.Identifier.Text;
                    return RankProperty(property, model, token, out label);

                case FieldDeclarationSyntax field:
                    name = field.Declaration.Variables[0].Identifier.Text;
                    return RankField(field, out label);

                case MethodDeclarationSyntax method:
                    name = method.Identifier.Text;
                    return RankMethod(method, model, token, out label);

                case EnumDeclarationSyntax enumeration:
                    name = enumeration.Identifier.Text;
                    return RankNestedType(enumeration.Modifiers, 8, 19, "Enum", out label);

                case ClassDeclarationSyntax @class:
                    name = @class.Identifier.Text;
                    return RankNestedType(@class.Modifiers, 9, 20, "class", out label);

                case StructDeclarationSyntax @struct:
                    name = @struct.Identifier.Text;
                    return RankNestedType(@struct.Modifiers, 10, 21, "Struct", out label);

                default:
                    return 0;
            }
        }

        /// <summary> プロパティの順位。公開(3)、インターフェース実装(4)。非公開は判定しない。 </summary>
        private static int RankProperty(
            PropertyDeclarationSyntax property, SemanticModel model, System.Threading.CancellationToken token, out string label)
        {
            label = string.Empty;
            if (!IsPublic(property.Modifiers))
            {
                return 0;
            }

            return ImplementsInterface(property, model, token)
                ? Rank(4, "インターフェースプロパティ", out label)
                : Rank(3, "公開プロパティ", out label);
        }

        /// <summary> フィールドの順位。公開定数(5)、非公開定数(11)、SerializeField(12)、その他(13)。 </summary>
        private static int RankField(FieldDeclarationSyntax field, out string label)
        {
            var isConstant = SkcSyntax.HasModifier(field.Modifiers, SyntaxKind.ConstKeyword) ||
                             (SkcSyntax.HasModifier(field.Modifiers, SyntaxKind.StaticKeyword) &&
                              SkcSyntax.HasModifier(field.Modifiers, SyntaxKind.ReadOnlyKeyword));

            if (IsPublic(field.Modifiers))
            {
                // 公開フィールドで定数でないものは、順序の対象にしない(別規則で指摘される)。
                return isConstant ? Rank(5, "公開定数", out label) : Zero(out label);
            }

            if (isConstant)
            {
                return Rank(11, "定数", out label);
            }

            if (SkcSyntax.HasAttribute(field, "SerializeField"))
            {
                return Rank(12, "SerializeField", out label);
            }

            return Rank(13, "プライベートフィールド", out label);
        }

        /// <summary> メソッドの順位。公開(6)、インターフェース実装(7)、Unityメッセージ(14)、Handle(15)、継承用(16)、private/internal(17)。 </summary>
        private static int RankMethod(
            MethodDeclarationSyntax method, SemanticModel model, System.Threading.CancellationToken token, out string label)
        {
            var name = method.Identifier.Text;
            var isPublic = IsPublic(method.Modifiers);

            if (isPublic)
            {
                return ImplementsInterface(method, model, token)
                    ? Rank(7, "インターフェースメソッド", out label)
                    : Rank(6, "公開メソッド", out label);
            }

            if (method.ExplicitInterfaceSpecifier != null)
            {
                return Rank(7, "インターフェースメソッド", out label);
            }

            if (UnityMessages.Contains(name))
            {
                return Rank(14, "Unityメッセージ", out label);
            }

            if (name.StartsWith("Handle", System.StringComparison.Ordinal))
            {
                return Rank(15, "イベントハンドラ", out label);
            }

            if (SkcSyntax.HasModifier(method.Modifiers, SyntaxKind.ProtectedKeyword) ||
                SkcSyntax.HasModifier(method.Modifiers, SyntaxKind.VirtualKeyword) ||
                SkcSyntax.HasModifier(method.Modifiers, SyntaxKind.AbstractKeyword) ||
                SkcSyntax.HasModifier(method.Modifiers, SyntaxKind.OverrideKeyword))
            {
                return Rank(16, "プロテクト/仮想メソッド", out label);
            }

            return Rank(17, "プライベートメソッド", out label);
        }

        /// <summary> ネスト型の順位。公開型と非公開型で順位が異なる。型以外の入れ子は対象外。 </summary>
        private static int RankNestedType(SyntaxTokenList modifiers, int publicRank, int privateRank, string kind, out string label)
        {
            return IsPublic(modifiers)
                ? Rank(publicRank, "公開" + kind, out label)
                : Rank(privateRank, "プライベート" + kind, out label);
        }

        /// <summary> 順位とラベルを返す小さな補助。 </summary>
        private static int Rank(int rank, string text, out string label)
        {
            label = text;
            return rank;
        }

        /// <summary> 判定対象外を表す。 </summary>
        private static int Zero(out string label)
        {
            label = string.Empty;
            return 0;
        }

        /// <summary> publicか。 </summary>
        private static bool IsPublic(SyntaxTokenList modifiers)
        {
            return SkcSyntax.HasModifier(modifiers, SyntaxKind.PublicKeyword);
        }

        /// <summary> メンバーがインターフェースのメンバーを実装しているか。 </summary>
        private static bool ImplementsInterface(
            MemberDeclarationSyntax member, SemanticModel model, System.Threading.CancellationToken token)
        {
            var symbol = model.GetDeclaredSymbol(member, token);
            return symbol != null && SkcSyntax.IsNameFixedBySignature(symbol) && !symbol.IsOverride;
        }

        /// <summary> メンバーの前に #if などのプリプロセッサ指令があるか。 </summary>
        private static bool HasPreprocessorDirective(MemberDeclarationSyntax member)
        {
            return member.GetLeadingTrivia().Any(t => t.IsDirective) ||
                   member.GetTrailingTrivia().Any(t => t.IsDirective);
        }

        /// <summary> 診断を付ける位置。 </summary>
        private static Location GetLocation(MemberDeclarationSyntax member)
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
