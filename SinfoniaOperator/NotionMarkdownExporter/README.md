# Notion Markdown Exporter

Notionのルートページ以下をEnhanced Markdown APIで再帰取得するWindows向けツールです。
子ページ、データベース行、画像、添付ファイルをローカルへ保存し、子ページ参照を相対リンクへ変換します。
ブロック背景色の装飾マーカー（例: `**{color="gray\_bg"}**` や見出し末尾の `{color="gray_bg"}`）は出力時に削除します。

## 初期設定

1. Notionで内部インテグレーションを作成し、コンテンツの読み取り権限を有効にします。
2. ルートページの`•••`メニューから`接続`を開き、作成したインテグレーションを追加します。
3. 既存の`SinfoniaOperator/sinfonia-operator.settings.json`へ次の2項目を追加します。設定ファイルがない場合は、`SinfoniaOperator/sinfonia-operator.settings.sample.json`をコピーして作成します。
4. `NOTION_EXPORT_ROOT_PAGE`と、必要に応じて`NOTION_EXPORT_OUTPUT`を設定します。`NOTION_TOKEN`は既存のSinfoniaOperatorと共用します。

```json
{
  "NOTION_EXPORT_ROOT_PAGE": "https://www.notion.so/ルートページのURL",
  "NOTION_EXPORT_OUTPUT": "Docs/NotionSpecifications"
}
```

`sinfonia-operator.settings.json`はGitの除外対象です。トークンをリポジトリへコミットしないでください。

## 実行

設定ファイルを用意した場合は、`SinfoniaOperator/NotionMarkdownExporter.exe`を実行します。
`NOTION_TOKEN`は共有設定ファイルまたは環境変数からのみ読み取ります。未設定の場合はエラーとして終了します。
ルートページと出力先が不足している場合は対話入力できます。

コマンドラインから明示的に指定する例:

```powershell
./NotionMarkdownExporter.exe `
  --root "https://www.notion.so/your-root-page-id" `
  --output "../Docs/NotionSpecifications"
```

画像や添付ファイルを取得しない場合:

```powershell
./NotionMarkdownExporter.exe --root "PAGE_ID" --no-assets
```

## メモリ使用量

取得したページ本文は、ページ単位でOSの一時ディレクトリへ直ちに退避します。
全ページの本文をRAMへ保持せず、リンク先の確定後に1ページずつ読み戻して出力します。
データベース行も25件ずつ逐次処理し、一時ファイルは正常終了時とエラー終了時のどちらでも削除します。

## 出力の更新

出力先の`.notion-export-manifest.json`には、このツールが生成したファイルだけが記録されます。
再実行時は、新しいNotionデータを一時領域へ取得できたことを確認した後、前回マニフェストに記録されたMarkdown、添付ファイル、空フォルダーを削除してから再生成します。
出力先に手動で追加したファイルは削除しません。
マニフェストが破損している場合は、古いデータと新しいデータの混在を防ぐためエクスポートを停止します。
