using System.Linq;
using Xunit;

namespace SymphonyKillChord.Analyzers.Tests
{
    /// <summary> 各アナライザの陽性(検出)・陰性(非検出)を確かめる。 </summary>
    public class AnalyzerTests
    {
        private const string RuntimeRoot = "/repo/Assets/Scripts/Runtime/";

        [Fact]
        public void SerializeFieldTooltip_検出と非検出()
        {
            const string source = @"
using UnityEngine;
public class A
{
    [SerializeField] private int _bad;
    [SerializeField, Tooltip(""説明"")] private int _ok;
}";
            Assert.Equal(new[] { "SKC0001" }, AnalyzerTestHelper.Run<SerializeFieldTooltipAnalyzer>(source));
        }

        [Fact]
        public void LogMessagePrefix_検出と非検出()
        {
            const string source = @"
using UnityEngine;
public class A
{
    public void F()
    {
        Debug.LogError(""bad"");
        Debug.LogError($""[{nameof(A)}] ok"");
    }
}";
            Assert.Equal(new[] { "SKC0002" }, AnalyzerTestHelper.Run<LogMessagePrefixAnalyzer>(source));
        }

        [Theory]
        [InlineData("KillChord.Runtime.Domain.InGame", "MonoBehaviour", 1)]
        [InlineData("KillChord.Runtime.Application.InGame", "ScriptableObject", 1)]
        [InlineData("KillChord.Runtime.Adaptor.InGame", "MonoBehaviour", 1)]
        [InlineData("KillChord.Runtime.View.InGame", "MonoBehaviour", 0)]
        [InlineData("KillChord.Runtime.Domain.InGame", "object", 0)]
        public void PureLayerUnityBase(string ns, string baseType, int expected)
        {
            var source = $"namespace {ns} {{ public class A : {(baseType == "object" ? "object" : "UnityEngine." + baseType)} {{ }} }}";
            Assert.Equal(expected, AnalyzerTestHelper.Run<PureLayerUnityBaseAnalyzer>(source).Length);
        }

        [Fact]
        public void PublicSetter_検出と非検出()
        {
            const string source = @"
public class A
{
    public int Bad { get; set; }
    public int Ok1 { get; private set; }
    public int Ok2 { get; init; }
    public int Ok3 { get; }
}";
            Assert.Equal(new[] { "SKC0004" }, AnalyzerTestHelper.Run<PublicSetterAnalyzer>(source));
        }

        [Theory]
        [InlineData("KillChord.Runtime.Domain.InGame.Skill", "1.Domain/InGame/Skill", 0)]
        [InlineData("KillChord.Runtime.Domain.InGame", "1.Domain/InGame/Skill", 1)]
        [InlineData("Whatever", "../Other/Folder", 0)]
        public void NamespaceFolder(string ns, string folder, int expected)
        {
            var source = $"namespace {ns} {{ public class A {{ }} }}";
            var path = folder.StartsWith("..") ? "/repo/Other/A.cs" : RuntimeRoot + folder + "/A.cs";
            Assert.Equal(expected, AnalyzerTestHelper.Run<NamespaceFolderAnalyzer>(source, path).Length);
        }

        [Fact]
        public void OneTypePerFile_複数型とファイル名不一致()
        {
            Assert.Empty(AnalyzerTestHelper.Run<OneTypePerFileAnalyzer>("public class A { private class B { } }", "A.cs"));
            Assert.Equal(new[] { "SKC0006" },
                AnalyzerTestHelper.Run<OneTypePerFileAnalyzer>("public class A { } public class B { }", "A.cs"));
            Assert.Equal(new[] { "SKC0006" },
                AnalyzerTestHelper.Run<OneTypePerFileAnalyzer>("public class A { }", "Other.cs"));
            Assert.Empty(AnalyzerTestHelper.Run<OneTypePerFileAnalyzer>("public partial class A { }", "A.Part.cs"));
        }

        [Fact]
        public void Naming_各規則()
        {
            const string source = @"
public interface Foo { }
public class A
{
    private int badField;
    private int _ok;
    public const int Bad = 1;
    public const int GOOD_VALUE = 2;
    public int PublicField;
    public event System.Action Changed;
    public event System.Action OnChanged;
    public bool Ready { get; }
    public bool IsReady { get; }
    public void doIt(int Bad) { }
}";
            var ids = AnalyzerTestHelper.Run<NamingConventionAnalyzer>(source);
            Assert.Contains("SKC0007", ids);
            Assert.Contains("SKC0008", ids);
            Assert.Contains("SKC0009", ids);
            Assert.Contains("SKC0010", ids);
            Assert.Contains("SKC0011", ids);
            Assert.Contains("SKC0013", ids);
            Assert.Equal(2, ids.Count(i => i == "SKC0012")); // doIt と 引数 Bad
            Assert.Single(ids, "SKC0007");
        }

        [Fact]
        public void Braces_検出と非検出()
        {
            const string source = @"
public class A
{
    public void F(bool a)
    {
        if (a) return;
        if (a) { return; } else if (!a) { return; } else { return; }
        foreach (var x in new int[0]) x.ToString();
    }
}";
            Assert.Equal(new[] { "SKC0014", "SKC0014" }, AnalyzerTestHelper.Run<BraceAnalyzer>(source));
        }

        [Fact]
        public void ExplicitAccessibility_検出と非検出()
        {
            const string source = @"
class Bad { int _x; void M() { } }
public interface I { void M(); }
public class Ok : I { static Ok() { } public void M() { } }";
            Assert.Equal(new[] { "SKC0015", "SKC0015", "SKC0015" },
                AnalyzerTestHelper.Run<ExplicitAccessibilityAnalyzer>(source));
        }

        [Fact]
        public void AsyncVoid_検出と非検出()
        {
            const string source = @"
using System;
using System.Threading.Tasks;
using UnityEngine;
public class A
{
    public async void Bad() { await Task.Yield(); }
    public async void Good()
    {
        try { await Task.Yield(); }
        catch (OperationCanceledException) { }
        catch (Exception e) { Debug.LogException(e); }
    }
    public async Task Fine() { await Task.Yield(); }
}";
            Assert.Equal(new[] { "SKC0016" }, AnalyzerTestHelper.Run<AsyncVoidAnalyzer>(source));
        }

        [Fact]
        public void Documentation_サマリー有無と句点()
        {
            const string source = @"
public class A
{
    /// <summary> 説明です。 </summary>
    public int Ok { get; }
    public int NoSummary { get; }
    /// <summary>
    ///     句点がありません
    ///     <para> 補足です。 </para>
    /// </summary>
    private void Bad() { }
    /// <summary>
    ///     補足です。
    ///     <para> 続きも句点で終わります。 </para>
    /// </summary>
    private void Good() { }
    private void Missing() { }
}";
            var ids = AnalyzerTestHelper.Run<DocumentationAnalyzer>(source);
            Assert.Equal(2, ids.Count(i => i == "SKC0017"));
            Assert.DoesNotContain("SKC0018", ids);
        }

        [Fact]
        public void Documentation_句点が無い日本語()
        {
            const string source = @"
public class A
{
    /// <summary> 説明です </summary>
    public void F() { }
}";
            Assert.Equal(new[] { "SKC0018" }, AnalyzerTestHelper.Run<DocumentationAnalyzer>(source));
        }

        [Fact]
        public void UsingPlacement_検出と非検出()
        {
            const string source = @"
using System;
namespace N
{
    using System.Linq;
    public class A { }
}";
            Assert.Equal(new[] { "SKC0019" }, AnalyzerTestHelper.Run<UsingPlacementAnalyzer>(source));
        }

        [Fact]
        public void Dependency_レイヤーとモジュール()
        {
            const string layer = "using KillChord.Runtime.Adaptor.InGame;\nnamespace KillChord.Runtime.Domain.InGame { public class A { } }";
            Assert.Equal(new[] { "SKC0021" }, AnalyzerTestHelper.Run<DependencyDirectionAnalyzer>(layer));

            const string module = "using KillChord.Runtime.Domain.OutGame;\nnamespace KillChord.Runtime.Domain.InGame { public class A { } }";
            Assert.Equal(new[] { "SKC0020" }, AnalyzerTestHelper.Run<DependencyDirectionAnalyzer>(module));

            const string adaptor = "using KillChord.Runtime.Domain.OutGame;\nnamespace KillChord.Runtime.Adaptor.InGame { public class A { } }";
            Assert.Empty(AnalyzerTestHelper.Run<DependencyDirectionAnalyzer>(adaptor));

            const string utility = "using KillChord.Runtime.Utility.InGame;\nnamespace KillChord.Runtime.Domain.OutGame { public class A { } }";
            Assert.Empty(AnalyzerTestHelper.Run<DependencyDirectionAnalyzer>(utility));
        }

        [Fact]
        public void MemberOrder_検出と非検出()
        {
            const string ok = @"
public class A : System.IDisposable
{
    public A() { }
    public event System.Action OnX;
    public int P { get; }
    public void Pub() { }
    public void Dispose() { }
    private const int C = 1;
    private int _f;
    private void Awake() { }
    private void Priv() { }
}";
            Assert.Empty(AnalyzerTestHelper.Run<MemberOrderAnalyzer>(ok));

            const string bad = @"
public class A
{
    private void Priv() { }
    public void Pub() { }
    public A() { }
}";
            Assert.Equal(new[] { "SKC0022", "SKC0022" }, AnalyzerTestHelper.Run<MemberOrderAnalyzer>(bad));
        }

        [Theory]
        [InlineData("Assets/AssetStoreTools/CRIMW/CriWare/CriAtomExOutputAnalyzer.cs", 0)]
        [InlineData("Assets\\AssetStoreTools\\CRIMW\\Foo.cs", 0)]
        [InlineData("/repo/Assets/Plugins/Foo.cs", 0)]
        [InlineData("Assets/DevelopProducts/Research/Foo.cs", 0)]
        [InlineData("Assets/Scripts/SymphonyFrameWork/Foo.cs", 0)]
        [InlineData("Assets/Scripts/Runtime/1.Domain/InGame/Foo.cs", 1)]
        [InlineData("Assets/Scripts/Develop/Foo.cs", 1)]
        [InlineData("Assets/Editor/Scripts/Foo.cs", 1)]
        [InlineData("/repo/Assets/Scripts/Runtime/Foo.cs", 1)]
        [InlineData("Test.cs", 1)]
        public void 検査対象のパスだけを検査する(string path, int expected)
        {
            // 外部ライブラリ由来のようなPascalCaseでないメソッド名。
            const string source = "public class Foo { public void bad_name() { } }";
            var ids = AnalyzerTestHelper.Run<NamingConventionAnalyzer>(source, path);
            Assert.Equal(expected, ids.Count(i => i == "SKC0012"));
        }

        [Fact]
        public void Unityの相対パスでも名前空間とフォルダの一致を検査する()
        {
            const string source = "namespace KillChord.Runtime.Domain.InGame { public class A { } }";
            Assert.Equal(
                new[] { "SKC0005" },
                AnalyzerTestHelper.Run<NamespaceFolderAnalyzer>(source, "Assets/Scripts/Runtime/1.Domain/InGame/Skill/A.cs"));
            Assert.Empty(
                AnalyzerTestHelper.Run<NamespaceFolderAnalyzer>(source, "Assets/Scripts/Runtime/1.Domain/InGame/A.cs"));
        }

        [Fact]
        public void 件数の多いルールは既定でInfo()
        {
            var infoIds = new[] { "SKC0005", "SKC0013", "SKC0014", "SKC0018", "SKC0020", "SKC0022" };
            var descriptors = typeof(PublicSetterAnalyzer).Assembly.GetTypes()
                .Where(t => typeof(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
                .SelectMany(t => ((Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer)System.Activator.CreateInstance(t)!)
                    .SupportedDiagnostics)
                .ToDictionary(d => d.Id);

            foreach (var id in infoIds)
            {
                Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Info, descriptors[id].DefaultSeverity);
            }

            Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Warning, descriptors["SKC0012"].DefaultSeverity);
        }
    }
}
