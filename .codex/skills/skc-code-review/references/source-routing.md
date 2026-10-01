# 参照元ルーティング

## 設計・規約の正本

- 設計思想とレイヤー責務: `Assets/Scripts/DesignPhilosophy.md`
- コード規約: `Assets/Scripts/CodeGuidelines.md`
- 実装から抽出された補助資料: `Assets/Docs/ScriptsDocs/Architecture.txt`、`Assets/Docs/ScriptsDocs/CodingConventions.txt`

正本と補助資料が矛盾する場合は、`Assets/Scripts`配下を優先して矛盾を報告する。

## ユビキタス言語

- 索引: `Docs/NotionSpecifications/Symphony Kill Chord/用語/用語リスト/_database.md`
- 個別定義: `Docs/NotionSpecifications/Symphony Kill Chord/用語/用語リスト/*.md`

対象コードのDomain名、画面名、操作名、ゲームルール名を`rg`で索引と個別定義から検索する。説明が空、またはファイル名に`【要確認】`がある場合は、関連仕様で補完できなければ未確定語として扱う。

## 機能仕様

- エクスポートされた仕様: `Docs/NotionSpecifications/Symphony Kill Chord/`
- コードモジュール資料: `Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/`
- 実装計画書: `Assets/Docs/*.md`

対象機能と同名・類義のファイルを`rg --files`で絞り、最新の計画書と関係する仕様だけを読む。議事録は、仕様書と計画書だけでは決定理由や新旧判定ができない場合に限って参照する。

## 仕様の優先順位

1. 現在の依頼で明示されたユーザー決定
2. 対象機能の最新計画書にある確定事項
3. Notionの機能仕様と用語定義
4. モジュール資料
5. 現在のコード挙動

下位資料を上位資料より優先しない。矛盾は黙って解釈せず、レビュー結果へ記載する。
