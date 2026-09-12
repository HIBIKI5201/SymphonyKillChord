# Notion Markdown Writer

Notionの仕様書ページへMarkdownで書き込むWindows向けツールです。
`NotionMarkdownExporter`が読み取り専用なのに対して、こちらは作成と部分更新を担当します。

Notion API `2026-03-11` のMarkdown Content APIを使うため、ブロックJSONを組み立てる必要はありません。

## 安全のための制限

- **書き込めるのは許可ページの配下だけ**です。`sinfonia-operator.env.json`の
  `NOTION_WRITE_ALLOWED_ROOTS`に列挙したページIDの子孫以外は、送信前に拒否します。
  許可ページ自身の本文は編集できません（作成先の親としてだけ指定できます）。
- **更新は部分置換（`update_content`）だけ**です。全文置換（`replace_content`）と
  子ページ削除（`allow_deleting_content`）は実装していません。
- `--confirm`を付けるまで**何も送信しません**。既定は差分表示のみです。
- pull以降にNotion側が更新されていた場合、`last_edited_time`の比較で中断します。
- APIが本文を分割して返す巨大ページは、原文が欠けた状態で差分を作らないよう編集を拒否します。
- 書き込み系リクエスト（POST・PATCH）はサーバーエラーで再試行しません。二重適用を避けるためです。

## 初期設定

1. Notionの内部インテグレーションに、コンテンツの**挿入・更新権限**を付与します
   （読み取りだけではページ作成・更新が403になります）。
2. 書き込みを許可するページのIDを`sinfonia-operator.env.json`へ列挙します。

```json
{
  "NOTION_WRITE_ALLOWED_ROOTS": [
    "27d7c2c6-cc02-818d-95ec-c1dc9a3c6761",
    "27d7c2c6-cc02-819a-9547-f211f355dfec"
  ]
}
```

`NOTION_TOKEN`は`sinfonia-operator.secrets.json`または環境変数から読み取ります。
この公開設定はGitで共有されるため、許可ページの追加はレビュー対象になります。

## 使い方

エクスポート済みのMarkdownはリンク変換や装飾マーカーの削除を経ており、Notionの原文とは一致しません。
そのため編集は、`pull`で取得した**原文**に対して行います。

```powershell
# 1. 編集の基準になる原文を取得する（ローカルの.mdパス、URL、IDのいずれでも指定できる）
./NotionMarkdownWriter.exe pull "Docs/NotionSpecifications/Symphony Kill Chord/システム概要/システムリスト/Bossシステム.md"

# 2. 表示された作業ファイルを普通に編集する

# 3. 差分を確認する（送信しない）
./NotionMarkdownWriter.exe push "SinfoniaOperator/.notion-work/xxxxxxxx-Bossシステム.md"

# 4. 送信する
./NotionMarkdownWriter.exe push "SinfoniaOperator/.notion-work/xxxxxxxx-Bossシステム.md" --confirm
```

子ページの作成:

```powershell
./NotionMarkdownWriter.exe create draft.md --parent "Docs/NotionSpecifications/Symphony Kill Chord/システム概要.md" --confirm
```

本文の最初の行は`# ページ名`にしてください。この見出しがページタイトルになり、本文からは取り除かれます。

## 作業ファイル

`pull`は作業ファイルと、同じ場所に`<ファイル名>.notion-pull.json`（pull時点の原文・ページID・最終更新日時）を出力します。
既定の出力先は`SinfoniaOperator/.notion-work/`で、Gitの除外対象です。
`push`の成功後は反映後の本文で作業ファイルとサイドカーを上書きし、送信内容どおりに反映されたかを確認します。

## 差分の作り方

`push`は作業ファイルとpull時点の原文を行単位で比較し、変更区間を`old_str`/`new_str`へ変換します。
`old_str`はページ内で一意でなければならないため、一意になるまで前後の行を文脈として自動的に足します。
繰り返しの多い文面などで一意にできない場合は、送信せずエラーになります。

## ローカルミラーとの関係

`push`はNotionを更新しますが、`Docs/NotionSpecifications/`のエクスポート結果は更新しません。
ミラーを最新化するには`NotionMarkdownExporter`を実行してください（全ページの再取得になります）。

## 実装メモ

- `NotionIdentifier`・`RequestRateLimiter`・`NotionApiException`は`NotionMarkdownExporter`とソースを共有しています（csprojのリンク参照）。
- 設定ファイルとリポジトリルートの探索は、エクスポーターの`ExporterOptions`と同じ規則を`WriterEnvironment`へ実装しています。
