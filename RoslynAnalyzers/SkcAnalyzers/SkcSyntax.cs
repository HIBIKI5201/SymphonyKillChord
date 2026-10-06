using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SymphonyKillChord.Analyzers
{
    /// <summary> 複数のアナライザで共有する構文・パス・名前空間の補助。 </summary>
    internal static class SkcSyntax
    {
        /// <summary> ランタイムコードの名前空間の接頭辞。 </summary>
        public const string RuntimeNamespace = "KillChord.Runtime";

        private const string RuntimeFolder = "/Assets/Scripts/Runtime/";

        /// <summary> 規約の検査対象にする名前空間(とその配下)。サードパーティ・研究用コードは含めない。 </summary>
        private static readonly string[] TargetNamespaces = { "KillChord.Runtime", "KillChord.Editor" };

        private static readonly ConditionalWeakTable<SyntaxTree, StrongBox<bool>> TargetTreeCache =
            new ConditionalWeakTable<SyntaxTree, StrongBox<bool>>();

        private static readonly Regex LayerPrefix = new Regex(@"^\d+\.", RegexOptions.Compiled);

        /// <summary> パス区切りを / に揃える。 </summary>
        public static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

        /// <summary>
        ///     パス区切りを揃え、先頭を / にする。Unityはコンパイラへ <c>Assets/...</c> の相対パスを渡すため、
        ///     絶対パスと同じ形(/Assets/...)で扱えるようにする。
        /// </summary>
        public static string NormalizeWithLeadingSlash(string path)
        {
            var normalized = NormalizePath(path);
            return normalized.StartsWith("/", StringComparison.Ordinal) ? normalized : "/" + normalized;
        }

        /// <summary>
        ///     検査の対象の名前空間か。<c>KillChord.Runtime</c> と <c>KillChord.Editor</c>(とその配下)だけを対象にする。
        ///     アナライザはUnityの全アセンブリ(Packages・AssetStoreTools・研究用コードなど)に適用され、
        ///     Unityのコンパイラは.editorconfigの除外を反映しない場合があるため、除外ではなく対象をここで絞る。
        /// </summary>
        public static bool IsTargetNamespace(string? ns)
        {
            return ns != null &&
                   TargetNamespaces.Any(t => ns == t || ns.StartsWith(t + ".", StringComparison.Ordinal));
        }

        /// <summary> 構文木が検査対象か。対象の名前空間を1つでも宣言していれば対象。名前空間の無いファイルは対象外。 </summary>
        public static bool IsTarget(SyntaxTree? tree)
        {
            if (tree == null)
            {
                return false;
            }

            return TargetTreeCache.GetValue(tree, t => new StrongBox<bool>(ComputeIsTarget(t))).Value;
        }

        /// <summary> シンボルが検査対象の名前空間に属するか。 </summary>
        public static bool IsTarget(ISymbol symbol)
        {
            var ns = symbol.ContainingNamespace;
            return ns != null && IsTargetNamespace(ns.ToDisplayString());
        }

        /// <summary> 構文木内の名前空間宣言に、対象の名前空間があるか。 </summary>
        private static bool ComputeIsTarget(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            return root
                .DescendantNodes(n => n is CompilationUnitSyntax || n is BaseNamespaceDeclarationSyntax)
                .OfType<BaseNamespaceDeclarationSyntax>()
                .Any(n => IsTargetNamespace(GetFullNamespace(n)));
        }

        /// <summary> 入れ子の名前空間宣言を連結した完全名を返す。 </summary>
        private static string GetFullNamespace(BaseNamespaceDeclarationSyntax declaration)
        {
            var parts = new System.Collections.Generic.List<string>();
            for (SyntaxNode? node = declaration; node != null; node = node.Parent)
            {
                if (node is BaseNamespaceDeclarationSyntax ns)
                {
                    parts.Insert(0, ns.Name.ToString());
                }
            }

            return string.Join(".", parts);
        }

        /// <summary> ファイルがAssets/Scripts/Runtime配下なら、そこからのフォルダ区切り(番号除去済み)を返す。 </summary>
        public static bool TryGetRuntimeFolders(string filePath, out string[] folders)
        {
            folders = Array.Empty<string>();
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var normalized = NormalizeWithLeadingSlash(filePath);
            var index = normalized.IndexOf(RuntimeFolder, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return false;
            }

            var relative = normalized.Substring(index + RuntimeFolder.Length);
            var segments = relative.Split('/');
            folders = segments
                .Take(segments.Length - 1)
                .Select(s => LayerPrefix.Replace(s, string.Empty))
                .ToArray();
            return true;
        }

        /// <summary> 名前空間をレイヤー名とモジュール名に分解する。Runtime配下でなければfalse。 </summary>
        public static bool TryParseRuntimeNamespace(string ns, out string layer, out string? module)
        {
            layer = string.Empty;
            module = null;
            var prefix = RuntimeNamespace + ".";
            if (!ns.StartsWith(prefix, StringComparison.Ordinal))
            {
                return false;
            }

            var parts = ns.Substring(prefix.Length).Split('.');
            layer = parts[0];
            module = parts.Length > 1 ? parts[1] : null;
            return true;
        }

        /// <summary> 属性名が指定の名前(Attribute接尾辞の有無を問わない)か。 </summary>
        public static bool IsAttributeNamed(AttributeData attribute, string name)
        {
            var attributeName = attribute.AttributeClass?.Name;
            return attributeName == name || attributeName == name + "Attribute";
        }

        /// <summary> 構文上の属性名が指定の名前か。 </summary>
        public static bool IsAttributeNamed(AttributeSyntax attribute, string name)
        {
            var text = attribute.Name.ToString();
            var dot = text.LastIndexOf('.');
            if (dot >= 0)
            {
                text = text.Substring(dot + 1);
            }

            return text == name || text == name + "Attribute";
        }

        /// <summary> 宣言に指定の属性が付いているか。 </summary>
        public static bool HasAttribute(MemberDeclarationSyntax member, string name)
        {
            return member.AttributeLists.SelectMany(l => l.Attributes).Any(a => IsAttributeNamed(a, name));
        }

        /// <summary> 修飾子に指定の種類が含まれるか。 </summary>
        public static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind)
        {
            return modifiers.Any(m => m.IsKind(kind));
        }

        /// <summary> アクセス修飾子(public/private/protected/internal)が含まれるか。 </summary>
        public static bool HasAccessModifier(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m =>
                m.IsKind(SyntaxKind.PublicKeyword) ||
                m.IsKind(SyntaxKind.PrivateKeyword) ||
                m.IsKind(SyntaxKind.ProtectedKeyword) ||
                m.IsKind(SyntaxKind.InternalKeyword));
        }

        /// <summary> シンボルがオーバーライドまたはインターフェース実装で、名前を自由に決められないか。 </summary>
        public static bool IsNameFixedBySignature(ISymbol symbol)
        {
            if (symbol.IsOverride)
            {
                return true;
            }

            var type = symbol.ContainingType;
            if (type == null)
            {
                return false;
            }

            if (type.TypeKind == TypeKind.Interface)
            {
                return false;
            }

            foreach (var iface in type.AllInterfaces)
            {
                foreach (var member in iface.GetMembers())
                {
                    var impl = type.FindImplementationForInterfaceMember(member);
                    if (SymbolEqualityComparer.Default.Equals(impl, symbol))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary> 生成コードのファイルか。 </summary>
        public static bool IsGeneratedPath(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var name = NormalizePath(path!);
            return name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
                   name.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}
