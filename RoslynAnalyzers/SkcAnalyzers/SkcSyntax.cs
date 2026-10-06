using System;
using System.Linq;
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

        private const string AssetsFolder = "/Assets/";
        private const string RuntimeFolder = "/Assets/Scripts/Runtime/";

        /// <summary> 規約の検査対象にするフォルダ。サードパーティ・研究用コードは含めない。 </summary>
        private static readonly string[] TargetFolders =
        {
            "/Assets/Scripts/Runtime/",
            "/Assets/Scripts/Develop/",
            "/Assets/Scripts/Demo/",
            "/Assets/Editor/Scripts/",
            "/Assets/Editor/AIDebugPlay/",
            "/Assets/Editor/ProjectSetup/",
        };
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
        ///     検査の対象ファイルか。Assets配下のファイルは、このプロジェクト自身のコードだけを対象にする。
        ///     (アナライザはUnityの全アセンブリに適用され、.editorconfigの除外がコンパイラに効かない場合があるため、アナライザ側で絞る。)
        ///     Assets配下でないパス(テストなど)は対象にする。
        /// </summary>
        public static bool IsTargetPath(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            var normalized = NormalizeWithLeadingSlash(path!);
            if (normalized.IndexOf(AssetsFolder, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return true;
            }

            return TargetFolders.Any(folder => normalized.IndexOf(folder, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary> 構文木が検査対象のファイルのものか。 </summary>
        public static bool IsTarget(SyntaxTree? tree)
        {
            return tree == null || IsTargetPath(tree.FilePath);
        }

        /// <summary> シンボルの宣言が検査対象のファイルにあるか。ソースの宣言が無いシンボルは対象外。 </summary>
        public static bool IsTarget(ISymbol symbol)
        {
            var location = symbol.Locations.FirstOrDefault(l => l.IsInSource);
            return location != null && IsTarget(location.SourceTree);
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
