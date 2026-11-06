# 参照元ルーティング

## 設計・規約の正本

- 設計思想とレイヤー責務: `Assets/Scripts/DesignPhilosophy.md`
- クラス責務の正本: `Assets/Scripts/DesignClassRoles.md`（対象レイヤーだけ読む）
- 実装例: `Assets/Scripts/DesignExamples.md`（必要時だけ）
- コード規約: `Assets/Scripts/CodeGuidelines.md`
- 実装から抽出された補助資料: `Assets/Docs/ScriptsDocs/Architecture.txt`、`Assets/Docs/ScriptsDocs/CodingConventions.txt`

正本と補助資料が矛盾する場合は、`Assets/Scripts`配下を優先して矛盾を報告する。

## ユビキタス言語

- Notion「用語リスト」データベース（id `3567c2c6-cc02-80d1-bd4e-e6dea0d41181`）。仕様書の写し `Docs/NotionSpecifications` の索引 `_database.md` のパスは `python scripts/notion/spec_query.py path <id>` で引き、同じフォルダに個別定義がある。写しの更新と報告は[読む手順](../../../../Docs/agent/README.md#必要な資料だけを読む)に従う。

対象コードのDomain名、画面名、操作名、ゲームルール名を`rg`で索引と個別定義から検索する。説明が空、またはファイル名に`【要確認】`がある場合は、関連仕様で補完できなければ未確定語として扱う。

## 機能仕様

- 機能仕様: 仕様書の写し `Docs/NotionSpecifications`。`python scripts/notion/spec_query.py search <語>` で絞り、`get <page id>` で仕様の節だけを読む。
- コードモジュール資料: Notion「システムリスト」データベース（id `27d7c2c6-cc02-81ee-963f-f0e4195b52e0`。パスは `path <id>` で引く。正本はNotion）

対象機能と同名・類義のページを `search` か `rg --files` で絞り、関係する仕様だけを読む。設計判断の理由はシステムリストの各ページの「設計方針」にある。議事録は、仕様書だけでは決定理由や新旧判定ができない場合に限って参照する。

## 仕様の優先順位

1. 現在の依頼で明示されたユーザー決定
2. 対象機能の最新計画書にある確定事項
3. Notionの機能仕様と用語定義
4. モジュール資料
5. 現在のコード挙動

下位資料を上位資料より優先しない。矛盾は黙って解釈せず、レビュー結果へ記載する。
