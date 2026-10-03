# RoslynAnalyzers

`Assets/Scripts/CodeGuidelines.md` の規約をコンパイル時に検査するRoslynアナライザ。

## ビルド

```
dotnet build RoslynAnalyzers/SkcAnalyzers -c Release
```

ビルド後、`SkcAnalyzers.dll` が `Assets/Editor/Roslyn/` へ自動コピーされる。DLLと`.meta`はコミットする。
`.meta` には `RoslynAnalyzer` ラベルを付け、全プラットフォームを無効にしている(プレイヤービルドに含めないため)。

## ルール

| ID | 内容 |
|---|---|
| SKC0001 | `[SerializeField]` のフィールドに `Tooltip` が無い |
| SKC0002 | `Debug.LogError/LogWarning` が `$"[{nameof(ClassName)}] ..."` で始まらない |

深刻度は `.editorconfig` の `dotnet_diagnostic.SKC000x.severity` で変更する。

## 注意

- `Microsoft.CodeAnalysis.CSharp` はUnityが読み込める 4.0.1 に固定している。上げるとUnityで読み込めない可能性がある。
- 新規ルール追加時は `SkcDiagnosticIds.cs` にIDを追加する。
