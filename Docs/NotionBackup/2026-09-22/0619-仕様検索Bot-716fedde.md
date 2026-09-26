# 仕様検索Bot

- id: 3d47c2c6-cc02-8186-b646-c785716fedde
- path: Symphony Kill Chord / システム概要 / SinfoniaOperator / 仕様検索Bot
- last_edited: 2026-09-07T17:10:23.166Z

Discordの`/spec query:<質問文>`スラッシュコマンドから、`Docs/NotionSpecifications`配下の仕様書をセマンティック検索する機能。ローカルの埋め込みモデルだけで動作し、外部AI APIへの課金は発生しない。

### 全体の流れ
```Mermaid
graph LR
    A["仕様書 Markdown"] -->|"MarkdownChunker"| B["チャンク（見出し単位・800文字）"]
    B -->|"OnnxEmbeddingModel"| C["埋め込みベクトル"]
    C -->|"SpecIndex.Save"| D["spec-index.bin"]
    E["/spec query"] -->|"OnnxEmbeddingModel"| F["クエリベクトル"]
    D -->|"SpecIndex.Load"| G["SpecIndex.TopK"]
    F --> G
    G --> H["Discord Embedで返信（抜粋+Notionリンク）"]
```
インデックス生成（`index`サブコマンド）と検索応答（`serve`サブコマンド）は別プロセス・別タイミングで動く。仕様書を更新したら`index`を再実行しない限り、検索結果には反映されない。

### なぜLLMで要約しないか
検索結果は、マッチしたチャンクの抜粋とNotionリンクをそのまま返す。生成AIによる要約は行わない。これは意図的な設計判断であり、次の理由による。
- 要約はハルシネーション（誤った内容の生成）のリスクを持つ。仕様書は正確性が求められるため、原文をそのまま見せるほうが安全である。
- 要約無しであれば埋め込みモデル（軽量）だけで完結し、生成モデル（重量級）が不要になる。Oracle Cloud無料枠の非力なVMでも動かせる規模に収まる。
将来的に要約機能を追加する場合は、`InteractionCreatedHandler`（`DiscordBotManager`）の`FollowupAsync`直前に要約ステップを挟む形になるが、追加の推論コストとハルシネーション対策（出典の明示など）を別途検討すること。

### `MarkdownChunker`（チャンク分割）
`Docs/NotionSpecifications/**/*.md`を再帰的に読み込み、次の規則でチャンクへ分割する。
- `assets/`ディレクトリ配下と`_database.md`は除外する（画像・データベース定義であり検索対象として有用でないため）。
- `#`・`##`・`###`の見出しから、パンくず文字列（例：「設計思想 > バトルシステム > ダメージ計算」）を構築する。検索結果の見出しとして表示するのに使う。
- セクション本文は800文字ごと、100文字のオーバーラップを持たせて分割する。日本語は単語区切りが無いため、文字数ベースの分割を採用している（英語のような単語数ベースの分割は行っていない）。
- 各ファイル冒頭付近にある`[Notionで開く](https://...)`のリンクを正規表現で抽出し、そのファイルのNotion URLとして全チャンクに引き継ぐ。
チャンクサイズ（800文字）とオーバーラップ（100文字）は`MarkdownChunker`のコンストラクタ引数で調整できる。埋め込みモデルの最大トークン長（512トークン）を超えないよう、変更する場合は上限に注意する。

### `OnnxEmbeddingModel`（埋め込み生成）
多言語対応の軽量埋め込みモデル`intfloat/multilingual-e5-small`（384次元、ONNX形式）を`Microsoft.ML.OnnxRuntime`で実行する。トークナイズにはXLM-RoBERTa系のSentencePieceモデルが必要で、`Lokad.Tokenizers`の`XLMRobertaTokenizer`を使っている。
推論結果はトークンごとの隠れ状態を平均プーリングし、L2正規化したベクトルを返す。E5系モデルの慣例に従い、インデックス生成時は`passage: <パンくず>\n<本文>`、検索時は`query: <検索文>`というプレフィックスを付けてからモデルへ渡す（`SpecIndexBuilder`と`DiscordBotManager`でそれぞれ付与）。**このプレフィックスを省略すると検索精度が大きく落ちる。** E5系モデル特有の学習前提であり、他の埋め込みモデルに差し替える場合は前提を確認すること。
`EmbedAsync`の呼び出しは`SemaphoreSlim`で直列化している。ONNX Runtimeのセッションはスレッドセーフだが、非力なVM上で同時に複数推論を走らせるとメモリを圧迫するため、意図的に1件ずつ処理する設計にしている。

### `SpecIndex`（検索インデックス）
チャンクとその埋め込みベクトルを配列として保持し、コサイン類似度によるTopK検索を提供する。件数が数千件規模（現状2000ファイル・数千チャンク）であるため、総当たり比較で十分な速度が出ており、外部のベクトルデータベースは導入していない。件数が数万を超える規模になったら再検討する。
保存形式は独自の単純なバイナリで、ファイル識別子とバージョン番号を先頭に持つ。フォーマットを変更する場合は`FILE_VERSION`を上げ、旧バージョンの読み込み可否を明示すること。

### Discord側の実装（`DiscordBotManager`）
`serve`サブコマンド実行時、`ConfigureSpecSearch`でインデックスと埋め込みモデルを受け取り、`Ready`イベントで`/spec`スラッシュコマンドを登録する。`SPEC_SEARCH_DISCORD_GUILD_ID`を設定していればそのGuildへ即時登録（開発向け、反映が速い）、未設定ならグローバル登録（反映に最大1時間程度かかる）を行う。
`InteractionCreatedHandler`は`DeferAsync()`で応答を保留してから検索を行う。埋め込み推論に数秒かかる場合があるため、Discordの3秒応答タイムアウトを避ける目的である。検索結果は`SPEC_SEARCH_TOP_K`件（既定3件）をDiscord Embedのフィールドとして返し、フィールド名に見出しパンくず、値に本文抜粋（400文字まで）とNotionリンクを入れる。

### 仕様書を更新したときの運用
1. Notion側の仕様書を更新する。
2. `NotionMarkdownExporter`で`Docs/NotionSpecifications`のミラーを最新化する。
3. VM上で`index`サブコマンドを再実行し、`spec-index.bin`を作り直す。
4. `serve`プロセスを再起動する（`SpecIndex`はプロセス起動時に一度だけ読み込むため、再起動しないと新しいインデックスが反映されない）。
手順3・4を自動化するには、systemdタイマーや`ExecStartPre`でのインデックス再生成などが候補になるが、現状は手動運用としている。