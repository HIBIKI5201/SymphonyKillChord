using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace SymphonyKillChord.Analyzers.Tests
{
    /// <summary>
    ///     コミット済みの Assets/Editor/Roslyn/SkcAnalyzers.dll が、現在のソースと同じアナライザ集合を持つかを検証する。
    ///     DLLのバイト列はコンパイラのバージョン差で変わるため、バイト比較ではなくアナライザの型と診断IDを比べる。
    ///     メソッド本体だけの変更は検出できない。
    /// </summary>
    public class CommittedDllTests
    {
        private const string RelativeDllPath = "Assets/Editor/Roslyn/SkcAnalyzers.dll";

        [Fact]
        public void コミット済みDLLはソースと同じアナライザとIDを持つ()
        {
            var dllPath = FindCommittedDll();

            // 同名アセンブリは既定のコンテキストに読み込み済みのため、独立したコンテキストへ読み込んで取り違えを防ぐ。
            var context = new AssemblyLoadContext("CommittedSkcAnalyzers", isCollectible: false);
            var committed = Describe(context.LoadFromAssemblyPath(dllPath));
            var source = Describe(typeof(PublicSetterAnalyzer).Assembly);

            Assert.True(
                source.SetEquals(committed),
                "Assets/Editor/Roslyn/SkcAnalyzers.dll がソースと一致しません。" +
                "`dotnet build RoslynAnalyzers/SkcAnalyzers -c Release` を実行してDLLをコミットしてください。" +
                $"\nソースのみ: {string.Join(", ", source.Except(committed))}" +
                $"\nDLLのみ: {string.Join(", ", committed.Except(source))}");
        }

        /// <summary> アセンブリ内の全アナライザについて「型名:診断ID」の集合を返す。 </summary>
        private static HashSet<string> Describe(Assembly assembly)
        {
            var result = new HashSet<string>();
            foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(t)))
            {
                var analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(type)!;
                foreach (var descriptor in analyzer.SupportedDiagnostics)
                {
                    result.Add($"{type.FullName}:{descriptor.Id}");
                }
            }

            return result;
        }

        /// <summary> テスト実行ディレクトリから上へたどってリポジトリのDLLを探す。 </summary>
        private static string FindCommittedDll()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, RelativeDllPath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new FileNotFoundException($"{RelativeDllPath} が見つかりません。");
        }
    }
}
