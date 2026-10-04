using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers
{
    /// <summary>
    ///     CodeGuidelines.md の命名規則を検査する。
    ///     フィールドは <c>_camelCase</c>、定数は <c>UPPER_SNAKE</c>、型・メソッド・プロパティはPascalCase、引数はcamelCase、
    ///     インターフェースは <c>I</c> 始まり、イベントは <c>On</c> 始まり、boolプロパティは <c>Is</c>/<c>Has</c> 始まり。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamingConventionAnalyzer : DiagnosticAnalyzer
    {
        private static readonly Regex CamelUnderscore = new Regex(@"^_[a-z][A-Za-z0-9]*$", RegexOptions.Compiled);
        private static readonly Regex UpperSnake = new Regex(@"^[A-Z][A-Z0-9]*(_[A-Z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex Pascal = new Regex(@"^[A-Z][A-Za-z0-9]*$", RegexOptions.Compiled);
        private static readonly Regex Camel = new Regex(@"^[a-z][A-Za-z0-9]*$", RegexOptions.Compiled);

        private static readonly DiagnosticDescriptor FieldRule = SkcDescriptors.Create(
            SkcDiagnosticIds.FieldNaming,
            "フィールド名は _camelCase",
            "フィールド '{0}' は '_' で始まるキャメルケースにしてください");

        private static readonly DiagnosticDescriptor ConstRule = SkcDescriptors.Create(
            SkcDiagnosticIds.ConstNaming,
            "定数名はUPPER_SNAKE",
            "定数 '{0}' はアッパースネークケースにしてください");

        private static readonly DiagnosticDescriptor InterfaceRule = SkcDescriptors.Create(
            SkcDiagnosticIds.InterfaceNaming,
            "インターフェース名は I で始める",
            "インターフェース '{0}' は 'I' で始めてください");

        private static readonly DiagnosticDescriptor EventRule = SkcDescriptors.Create(
            SkcDiagnosticIds.EventNaming,
            "イベント名は On で始める",
            "イベント '{0}' は 'On' で始めてください");

        private static readonly DiagnosticDescriptor BoolRule = SkcDescriptors.Create(
            SkcDiagnosticIds.BoolPropertyNaming,
            "boolプロパティは Is / Has で始める",
            "bool型プロパティ '{0}' は 'Is' または 'Has' で始めてください");

        private static readonly DiagnosticDescriptor CasingRule = SkcDescriptors.Create(
            SkcDiagnosticIds.CasingNaming,
            "型・メソッド・プロパティはパスカルケース、引数はキャメルケース",
            "'{0}' は{1}にしてください");

        private static readonly DiagnosticDescriptor PublicFieldRule = SkcDescriptors.Create(
            SkcDiagnosticIds.PublicField,
            "フィールドを直接公開しない",
            "フィールド '{0}' が公開されています。プロパティを使用してください");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(FieldRule, ConstRule, InterfaceRule, EventRule, BoolRule, CasingRule, PublicFieldRule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(
                AnalyzeSymbol,
                SymbolKind.Field, SymbolKind.NamedType, SymbolKind.Event, SymbolKind.Property, SymbolKind.Method);
        }

        /// <summary> シンボルの種類ごとに命名を検査する。 </summary>
        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            switch (context.Symbol)
            {
                case IFieldSymbol field:
                    AnalyzeField(context, field);
                    break;
                case INamedTypeSymbol type:
                    AnalyzeType(context, type);
                    break;
                case IEventSymbol evt:
                    AnalyzeEvent(context, evt);
                    break;
                case IPropertySymbol property:
                    AnalyzeProperty(context, property);
                    break;
                case IMethodSymbol method:
                    AnalyzeMethod(context, method);
                    break;
            }
        }

        /// <summary> フィールドと定数を検査する。enumの要素と自動プロパティの裏フィールドは対象外。 </summary>
        private static void AnalyzeField(SymbolAnalysisContext context, IFieldSymbol field)
        {
            if (field.IsImplicitlyDeclared || field.ContainingType.TypeKind == TypeKind.Enum)
            {
                return;
            }

            var location = field.Locations[0];

            if (field.IsConst)
            {
                if (!UpperSnake.IsMatch(field.Name))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ConstRule, location, field.Name));
                }

                return;
            }

            // static readonly は定数扱いも許容する。
            if (field.IsStatic && field.IsReadOnly && UpperSnake.IsMatch(field.Name))
            {
                return;
            }

            // 公開フィールドは命名より先に公開自体を指摘する。
            if (field.DeclaredAccessibility == Accessibility.Public && !field.IsStatic && !field.IsReadOnly)
            {
                context.ReportDiagnostic(Diagnostic.Create(PublicFieldRule, location, field.Name));
                return;
            }

            if (field.DeclaredAccessibility == Accessibility.Public)
            {
                return;
            }

            if (!CamelUnderscore.IsMatch(field.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(FieldRule, location, field.Name));
            }
        }

        /// <summary> 型名を検査する。 </summary>
        private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol type)
        {
            if (type.IsImplicitlyDeclared || type.TypeKind == TypeKind.Error || type.IsAnonymousType)
            {
                return;
            }

            var location = type.Locations[0];

            if (type.TypeKind == TypeKind.Interface)
            {
                if (type.Name.Length < 2 || type.Name[0] != 'I' || !char.IsUpper(type.Name[1]))
                {
                    context.ReportDiagnostic(Diagnostic.Create(InterfaceRule, location, type.Name));
                }

                return;
            }

            if (type.TypeKind == TypeKind.Delegate)
            {
                return;
            }

            if (!Pascal.IsMatch(type.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(CasingRule, location, type.Name, "パスカルケース"));
            }
        }

        /// <summary> イベント名を検査する。 </summary>
        private static void AnalyzeEvent(SymbolAnalysisContext context, IEventSymbol evt)
        {
            if (evt.IsImplicitlyDeclared || SkcSyntax.IsNameFixedBySignature(evt))
            {
                return;
            }

            if (!evt.Name.StartsWith("On", System.StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(EventRule, evt.Locations[0], evt.Name));
            }
        }

        /// <summary> プロパティ名とbool接頭辞を検査する。 </summary>
        private static void AnalyzeProperty(SymbolAnalysisContext context, IPropertySymbol property)
        {
            if (property.IsImplicitlyDeclared || property.IsIndexer || SkcSyntax.IsNameFixedBySignature(property))
            {
                return;
            }

            var location = property.Locations[0];

            if (!Pascal.IsMatch(property.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(CasingRule, location, property.Name, "パスカルケース"));
            }

            if (property.Type.SpecialType == SpecialType.System_Boolean &&
                !property.Name.StartsWith("Is", System.StringComparison.Ordinal) &&
                !property.Name.StartsWith("Has", System.StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(BoolRule, location, property.Name));
            }
        }

        /// <summary> メソッド名と引数名を検査する。 </summary>
        private static void AnalyzeMethod(SymbolAnalysisContext context, IMethodSymbol method)
        {
            if (method.IsImplicitlyDeclared ||
                method.MethodKind != MethodKind.Ordinary ||
                SkcSyntax.IsNameFixedBySignature(method))
            {
                return;
            }

            if (!Pascal.IsMatch(method.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(CasingRule, method.Locations[0], method.Name, "パスカルケース"));
            }

            foreach (var parameter in method.Parameters)
            {
                if (!Camel.IsMatch(parameter.Name))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(CasingRule, parameter.Locations[0], parameter.Name, "キャメルケース"));
                }
            }
        }
    }
}
