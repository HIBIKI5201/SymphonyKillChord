# RoslynAnalyzers

`Assets/Scripts/CodeGuidelines.md` と `Assets/Scripts/DesignPhilosophy.md` の規約をコンパイル時に検査するRoslynアナライザ。

## ビルドとテスト

```
dotnet build RoslynAnalyzers/SkcAnalyzers -c Release   # DLLを Assets/Editor/Roslyn/ へ自動コピー
dotnet test  RoslynAnalyzers/SkcAnalyzers.Tests
```

- DLLと`.meta`はコミットする。`.meta` には `RoslynAnalyzer` ラベルを付け、全プラットフォームを無効にしている(プレイヤービルドに含めないため)。
- コピーは Release ビルドのときだけ行う(Debug の DLL が Assets に入らないようにするため)。
- アナライザのソースを変えたら、Release ビルドしてDLLもコミットする。CI(`RoslynAnalyzers.yml`)がテストを実行し、DLLが古いと警告する。

## ルール

| ID | 種別 | 内容 | 出典 |
|---|---|---|---|
| SKC0001 | 規約 | `[SerializeField]` のフィールドに `Tooltip` が無い | CodeGuidelines |
| SKC0002 | 規約 | `Debug.LogError/LogWarning` が `$"[{nameof(ClassName)}] ..."` で始まらない | CodeGuidelines |
| SKC0003 | 設計 | Domain/Application/Adaptor の型が `MonoBehaviour` / `ScriptableObject` を継承している | DesignPhilosophy |
| SKC0004 | 規約・設計 | プロパティに `public set` がある | 両方 |
| SKC0005 | 規約 | 名前空間が(レイヤー番号を除いた)フォルダ構成と一致しない。`Assets/Scripts/Runtime` 配下のみ | CodeGuidelines |
| SKC0006 | 規約 | 1ファイルに複数の公開型がある、またはファイル名が型名と一致しない | CodeGuidelines |
| SKC0007 | 規約 | フィールドが `_camelCase` でない | CodeGuidelines |
| SKC0008 | 規約 | `const` が `UPPER_SNAKE` でない | CodeGuidelines |
| SKC0009 | 規約 | インターフェース名が `I` で始まらない | CodeGuidelines |
| SKC0010 | 規約 | イベント名が `On` で始まらない | CodeGuidelines |
| SKC0011 | 規約 | `bool` プロパティが `Is` / `Has` で始まらない | CodeGuidelines |
| SKC0012 | 規約 | 型・メソッド・プロパティがPascalCaseでない、引数がcamelCaseでない | CodeGuidelines |
| SKC0013 | 規約 | フィールドを直接公開している(`const` / `static readonly` / `readonly` は除く) | CodeGuidelines |
| SKC0014 | 規約 | `if` / `else` / `for` / `foreach` / `while` / `do` の本体が波カッコで囲まれていない | CodeGuidelines |
| SKC0015 | 規約 | アクセス修飾子が明示されていない | CodeGuidelines |
| SKC0016 | 規約 | `async void` の本体が単一の `try/catch` ではない、または catch で `Debug.LogException` を呼んでいない | CodeGuidelines |
| SKC0017 | 規約 | メソッド、公開プロパティ・イベントにサマリーが無い | CodeGuidelines |
| SKC0018 | 規約 | 日本語のサマリーが「。」で終わっていない | CodeGuidelines |
| SKC0019 | 規約 | `using` がファイルの先頭にまとまっていない | CodeGuidelines |
| SKC0020 | 設計 | Adaptor/Composition 以外が他モジュール(InGame/OutGame/Persistent など)へ直接 `using` している | DesignPhilosophy |
| SKC0021 | 設計 | レイヤーの参照方向に反する `using` がある(Utility は全レイヤーから参照可) | DesignPhilosophy |
| SKC0022 | 規約 | メンバーの並びが「順序」(22段階)に合わない。直前のメンバーより順位が前のものを報告する | CodeGuidelines |

### 判定の割り切り

- SKC0020/0021 は `using` ディレクティブだけを見る。完全修飾名での参照は検出しない。
- SKC0022 は `#if` で囲まれたメンバーと、順位を決められないメンバー(非公開プロパティなど)を飛ばす。「内部メソッド・抽出メソッド」は private メソッドと同じ順位として扱う。
- 次の規約は機械的に判定できないため対象外: メソッド名が動詞で始まること、マジックナンバー、コメントの量と質、`.uxml`/`.uss` の命名、AIの編集範囲の制限。
- 次の設計は未実装: Entity/ValueObject/DTO の型の種類(`class` / `readonly struct` / `readonly ref struct`)の検査。レイヤー間のアセンブリ参照はasmdefで守られている。

## 深刻度

アナライザはUnityの全アセンブリに適用されるため、`.editorconfig` で既定を `none` にし、
`Assets/Scripts/{Runtime,Develop,Demo}` と `Assets/Editor/{Scripts,AIDebugPlay,ProjectSetup}` だけを有効にしている。
`DevelopProducts`・`Plugins`・`AssetStoreTools`・`SymphonyFrameWork` は対象外。

導入時点の `Assets/Scripts/Runtime` の違反件数(構文中心の簡易計測)と、既定の深刻度:

| 深刻度 | ルール(件数) |
|---|---|
| warning | SKC0003(0) / 0004(19) / 0006(8) / 0007(8) / 0008(24) / 0010(9) / 0011(25) / 0012(17) / 0015(1) / 0016(19) / 0017(7) / 0019(6) / 0021(1) |
| suggestion | SKC0005(90) / 0013(154) / 0014(154) / 0018(181) / 0020(166) / 0022(502) |

SKC0001/0002 は Unity の型を解決できる環境でないと件数を出せないため未計測。
違反を解消したルールから `.editorconfig` の `dotnet_diagnostic.SKC00xx.severity` を `warning` → `error` へ引き上げる。

## 注意

- `Microsoft.CodeAnalysis.CSharp` はUnityが読み込める 4.0.1 に固定している。上げるとUnityで読み込めない可能性がある。
- 新規ルール追加時は `SkcDiagnosticIds.cs` にIDを追加し、`SkcAnalyzers.Tests` にテストを足し、`.editorconfig` に深刻度を足す。
