using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SymphonyKillChord.Analyzers.Tests
{
    /// <summary> ソースをコンパイルし、指定のアナライザが出した診断のIDを返す。 </summary>
    internal static class AnalyzerTestHelper
    {
        /// <summary> テストで使うUnityの最小スタブ。 </summary>
        private const string UnityStub = @"
namespace UnityEngine
{
    public class Object { }
    public class MonoBehaviour : Object { }
    public class ScriptableObject : Object { }
    public class SerializeField : System.Attribute { }
    public class TooltipAttribute : System.Attribute { public TooltipAttribute(string s) { } }
    public static class Debug
    {
        public static void LogError(object o) { }
        public static void LogWarning(object o) { }
        public static void LogException(System.Exception e) { }
    }
}";

        /// <summary> 名前空間を持たないテスト用ソースを包む、検査対象の名前空間。 </summary>
        public const string TargetNamespace = "KillChord.Runtime.Test";

        /// <summary>
        ///     診断IDの一覧を返す。filePathでフォルダ依存の規則を検証できる。
        ///     アナライザは名前空間が KillChord.Runtime / KillChord.Editor のコードだけを検査するため、
        ///     名前空間を持たないソースは、先頭の using を残して <see cref="TargetNamespace"/> で包む。
        /// </summary>
        public static string[] Run<TAnalyzer>(string source, string filePath = "Test.cs")
            where TAnalyzer : DiagnosticAnalyzer, new()
        {
            var parse = new CSharpParseOptions(LanguageVersion.Latest);
            var trees = new[]
            {
                CSharpSyntaxTree.ParseText(UnityStub, parse, "UnityStub.cs"),
                CSharpSyntaxTree.ParseText(WrapInTargetNamespace(source), parse, filePath),
            };

            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p))
                .ToList();

            var compilation = CSharpCompilation.Create(
                "Test", trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var diagnostics = compilation
                .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new TAnalyzer()))
                .GetAnalyzerDiagnosticsAsync()
                .Result;

            return diagnostics
                .Where(d => d.Location.SourceTree?.FilePath == filePath)
                .Select(d => d.Id)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary> 名前空間の宣言が無いソースを、先頭のusingを残して対象の名前空間で包む。 </summary>
        private static string WrapInTargetNamespace(string source)
        {
            if (source.Contains("namespace "))
            {
                return source;
            }

            var lines = source.Split('\n').ToList();
            var headerCount = lines.TakeWhile(l => l.Trim().Length == 0 || l.TrimStart().StartsWith("using ")).Count();
            var header = string.Join("\n", lines.Take(headerCount));
            var body = string.Join("\n", lines.Skip(headerCount));
            return header + "\nnamespace " + TargetNamespace + "\n{\n" + body + "\n}\n";
        }
    }
}
