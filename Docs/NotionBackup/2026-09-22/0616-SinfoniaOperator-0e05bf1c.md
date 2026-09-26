# SinfoniaOperator

- id: 31b7c2c6-cc02-80ab-b731-fd020e05bf1c
- path: Symphony Kill Chord / システム概要 / SinfoniaOperator
- last_edited: 2026-09-09T03:24:59.311Z

- 概要: スタジオのタスクやスプリントの管理を行うボットについて
- カテゴリー: 開発用


## 説明
Discord及びNotionの管理を行うボット群。
SinfoniaOperatorディレクトリに、ゲーム本体とは別の`.NET`ソリューションとして設計されている。
毎朝のタスク・スプリント通知に加え、Oracle Cloud上に常駐させた仕様検索Bot（Discordの`/spec`コマンド）も同じソリューションに含まれる。
また同ディレクトリには、Discord/Notion通知ボット以外に以下3つの独立したツールが同居している。
| ツール | 役割 |
|---|---|
| NotionMarkdownExporter | Notionの仕様書ページ以下をEnhanced Markdown APIで再帰取得し、Markdownとしてローカル（`Docs/NotionSpecifications`）へ保存する。子ページ参照は相対リンクへ変換される。再実行時はNotionページの最終更新日時を前回エクスポート完了日時と比較し、更新・新規ページのみを再取得する差分エクスポートに対応している。 |
| NotionMarkdownWriter | ローカルで編集したMarkdownをNotionへ書き戻す。書き込み対象は`sinfonia-operator.env.json`の`NOTION_WRITE_ALLOWED_ROOTS`に列挙した許可ページの配下に限定され、それ以外への送信は拒否される。更新は部分置換（`update_content`）のみに対応し、全文置換や子ページ削除は実装されていない。`--confirm`を付けるまで実際の送信は行われない。 |
| DiscordLogExporter | 設定したDiscordチャンネル・フォーラムの全メッセージをテキストファイルへ出力する。通常チャンネルはチャンネルごと、フォーラムはページ（スレッド）ごとに1ファイルを生成し、`Docs/DiscordLog/`へ保存する。 |
いずれもWindows向けの単体exeとして配布されており、Notion/Discordの通知ボット本体とは別の実行系として動作する。

### 参考資料
‣
‣

## 詳細
機能ごとの詳細は子ページに分けている。まずここで全体像をつかんでから、目的の子ページを開く。

### 全体構成
SinfoniaOperatorは3つの`.NET`プロジェクトから成る。
- `SinfoniaOperator.Core`：Notion連携・Discord送信・設定管理の共通ライブラリ。Unity Editor拡張からも参照される。
- `SinfoniaOperator`（Exe）：Bot本体。CLIサブコマンドで「日次通知」「検索インデックス生成」「常駐検索Bot」の3つの動作を切り替える。
- `SinfoniaOperator.SpecSearch`：仕様検索専用ライブラリ。Markdownのチャンク分割・埋め込みベクトル生成・検索インデックスを担う。

### 主な機能
- 毎日朝7時（JST）、GitHub Actionsから実行され、Notionのタスク・スプリントデータベースを読み込んでDiscordへ通知する。
- Oracle Cloud上でsystemdサービスとして常駐し、Discordの`/spec query:<質問文>`コマンドから仕様書をセマンティック検索できる。ローカルの埋め込みモデルのみで動作し、外部AI APIへの課金は発生しない。

### 子ページ
- 📄 [[アーキテクチャ]] (3d47c2c6-cc02-8122-b5c1-e06b79d59012)
- 📄 [[日次通知機能]] (3d47c2c6-cc02-8102-8c10-fb2512a3fa72)
- 📄 [[仕様検索Bot]] (3d47c2c6-cc02-8186-b646-c785716fedde)
- 📄 [[設定・環境変数リファレンス]] (3d47c2c6-cc02-81e6-93d5-fdca6adc65f9)
- 📄 [[Oracle Cloudデプロイ手順]] (3d47c2c6-cc02-81df-a3fd-ebaaff186a65)