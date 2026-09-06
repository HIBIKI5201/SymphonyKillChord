# SinfoniaOperator 仕様検索Bot 実装レポート

## 実装概要

承認済み計画`C:\Users\takut\.claude\plans\squishy-shimmying-trinket.md`のアーキテクチャ1〜4に基づき、SinfoniaOperatorへローカル埋め込みモデルを使う仕様検索機能を追加した。

- `Docs/NotionSpecifications/**/*.md`を見出し単位・文字数単位でチャンク化する。
- `assets`ディレクトリと`_database.md`を検索対象から除外する。
- Markdown冒頭付近の`[Notionで開く](https://...)`を抽出する。
- `intfloat/multilingual-e5-small`をONNX Runtimeで実行し、attention mask付き平均プーリングとL2正規化を行う。
- XLM-RoBERTa固有のトークンIDを正しく生成するため、ARM64で利用可能なマネージド実装`Lokad.Tokenizers`の`XLMRobertaTokenizer`を使用する。
- 索引作成時は`passage: 見出しパンくず\n本文`、検索時は`query: 問い合わせ`をモデルへ入力する。
- インデックスを単純な独自バイナリ形式で保存・復元し、総当たりのコサイン類似度でTop Kを取得する。
- Discordの`/spec query:<文字列>`へ、上位結果の見出し、約400文字の原文抜粋、NotionリンクをEmbedで返す。LLMによる要約は行わない。

## ステップ1: SpecSearchライブラリ

`SinfoniaOperator.SpecSearch`を`net10.0`単独ターゲットのclass libraryとして追加した。Unity向け`CopyToUnity`ターゲットは持たない。

主な実装は次のとおり。

- `SpecChunkRecord`: 相対ソースパス、見出しパンくず、Notion URL、本文、埋め込みベクトルを保持する。
- `MarkdownChunker`: 800文字、100文字オーバーラップを既定値として仕様書を分割する。
- `IEmbeddingModel`: 非同期埋め込み生成を抽象化する。
- `OnnxEmbeddingModel`: XLM-RoBERTaトークナイズ、ONNX推論、平均プーリング、L2正規化を行う。同時呼び出しはセマフォで直列化する。
- `SpecIndex`: Top K検索とバイナリ保存・読込を提供する。ファイル識別子と形式バージョンを検証する。
- `SpecIndexBuilder`: チャンク生成から順次埋め込み、保存までを調停する。

`SinfoniaOperator.slnx`を新設し、Core、SpecSearch、Botの3プロジェクトを登録した。

## ステップ2: Bot統合

既存の`send`分岐および引数なしのタスク・スプリント通知フローは保持し、その前段へ次のサブコマンドを追加した。

- `index`: 設定を読み込み、仕様書から検索インデックスを生成する。
- `serve`: 起動時にインデックスをロードし、Discord Gatewayへ接続したまま常駐する。SIGINTとSIGTERMを`PosixSignalRegistration`で処理し、接続を停止・ログアウトして終了する。

`DiscordBotManager`には次を追加した。

- Botトークンのみで起動できるコンストラクタ。
- Guild scopedまたはglobalの`/spec`登録。
- `InteractionCreatedHandler`による遅延応答、埋め込み検索、Embed返信。
- イベント購読解除を含む非同期破棄。

`SPEC_SEARCH_TOP_K`は未設定時3件で、Discord Embedの上限を安全に守るため1〜10の範囲を受け付ける。

追加した設定キーは次のとおり。

- `SPEC_SEARCH_INDEX_PATH`
- `SPEC_SEARCH_EMBEDDING_MODEL_PATH`
- `SPEC_SEARCH_DISCORD_GUILD_ID`（任意）
- `SPEC_SEARCH_TOP_K`（任意、既定3）

トークナイザはONNXモデルと同じディレクトリの`sentencepiece.bpe.model`を使用する。

## ステップ3: Oracle VMデプロイ

`deploy`配下へ次を追加した。

- Oracle Cloud Always Free Ampere A1、Ubuntu 22.04 ARM64向けセットアップ手順。
- partial cloneとsparse-checkoutで`Docs/NotionSpecifications`と`SinfoniaOperator`だけを展開する手順。
- .NET 10 ARM64 SDK・ランタイム導入、linux-arm64自己完結publish、インデックス生成、systemd登録手順。
- 特定リビジョンの`intfloat/multilingual-e5-small` ONNXモデルとSentencePieceモデルを取得するスクリプト。
- `serve`引数、`Restart=on-failure`を設定したsystemdユニット。

BotはDiscord Gatewayへのアウトバウンド接続だけを使用するため、Oracleのセキュリティリスト、NSG、Ubuntuファイアウォールでインバウンドポートを追加開放する必要がないことを手順書へ明記した。

## 変更ファイル

- `SinfoniaOperator/.gitignore`
- `SinfoniaOperator/SinfoniaOperator.slnx`
- `SinfoniaOperator/SinfoniaOperator.Core/OperatorConfigKeys.cs`
- `SinfoniaOperator/SinfoniaOperator.SpecSearch/SinfoniaOperator.SpecSearch.csproj`
- `SinfoniaOperator/SinfoniaOperator.SpecSearch/SpecChunkRecord.cs`
- `SinfoniaOperator/SinfoniaOperator.SpecSearch/MarkdownChunker.cs`
- `SinfoniaOperator/SinfoniaOperator.SpecSearch/IEmbeddingModel.cs`
- `SinfoniaOperator/SinfoniaOperator.SpecSearch/OnnxEmbeddingModel.cs`
- `SinfoniaOperator/SinfoniaOperator.SpecSearch/SpecIndex.cs`
- `SinfoniaOperator/SinfoniaOperator.SpecSearch/SpecIndexBuilder.cs`
- `SinfoniaOperator/SinfoniaOperator/SinfoniaOperator.csproj`
- `SinfoniaOperator/SinfoniaOperator/SinfoniaOperator.cs`
- `SinfoniaOperator/SinfoniaOperator/DiscordBotManager.cs`
- `SinfoniaOperator/deploy/oracle-vm-setup.md`
- `SinfoniaOperator/deploy/sinfonia-specsearch.service`
- `SinfoniaOperator/deploy/download-embedding-model.sh`
- `SinfoniaOperator/SpecSearchBot実装レポート.md`

`.github/workflows/SinfoniaOperator.yml`は変更していない。開始時から存在した`Assets/Resources/PerformanceTestRun*.json`および`.meta`の未追跡ファイルにも変更を加えていない。

## 検証結果

### 成功した確認

- `dotnet sln SinfoniaOperator/SinfoniaOperator.slnx list`: Core、SpecSearch、Botの3プロジェクトを正常に認識した。
- `SinfoniaOperator.Core`: `netstandard2.1`と`net10.0`の両方で0警告・0エラーでビルド成功した。環境のRoslyn共有コンパイラ用名前付きパイプが拒否されたため、`UseSharedCompilation=false`と単一MSBuildノードを指定した。
- SpecSearchの外部パッケージ非依存部分: ONNXアダプターだけを検証時に除外し、0警告・0エラーでコンパイル成功した。検証後に正規のプロジェクト定義へ戻した。
- Bot統合部分: 一時的な同形SpecSearch契約を使い、Discord.Net 3.19.0のスラッシュコマンド、Interaction、Followup APIを含めて0コンパイルエラーを確認した。検証用ファイルは削除済みである。
- `git diff --check`: 空白エラーなし。
- `.github/workflows/SinfoniaOperator.yml`: 差分なし。

### 環境制約により完了できなかった確認

最終の実構成ビルドはNuGet復元で停止した。この実行環境では`https://api.nuget.org/v3/index.json`への通信が`127.0.0.1:9`で拒否され、ローカルキャッシュにも新規依存の`Microsoft.ML.OnnxRuntime 1.29.0`と`Lokad.Tokenizers 0.1.1`が存在しない。

最終実行コマンド:

```powershell
dotnet build SinfoniaOperator\SinfoniaOperator.slnx --configuration Release --nologo --maxcpucount:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

結果は0警告、2件の`NU1301`復元エラーであり、コンパイラには到達していない。このため、依頼された「3プロジェクトすべての実構成で警告なしビルド成功」は当環境では未確認である。NuGetへ接続できる環境で上記コマンドを再実行する必要がある。

Git Bashによる`bash -n`もサンドボックスの`CreateFileMapping`権限拒否で起動できなかったため、ダウンロードスクリプトは目視による構文確認までとしている。

## 実運用前の確認

NuGet接続可能なARM64 VMで次を実施する。

1. ソリューションを警告なしでビルドする。
2. 配布スクリプトでONNXモデルと`sentencepiece.bpe.model`を取得する。
3. `index`を実行し、実データのインデックス件数とファイル生成を確認する。
4. 開発Guildで`serve`を起動し、`/spec`の登録、原文抜粋、Notionリンクを確認する。
5. SIGTERMでsystemd停止時のグレースフルシャットダウンを確認する。
