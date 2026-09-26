# Oracle Cloud x86-64 VM セットアップ

## 1. VMを用意する

Oracle Cloud InfrastructureでUbuntu 22.04 x86-64 Computeを作成します。BotはDiscord Gatewayへアウトバウンド接続するだけで、HTTPサーバーなどのインバウンド待受は行いません。Oracle側のセキュリティリスト、NSG、Ubuntuのファイアウォールで追加ポートを開放する必要はありません。

以降の例では配置先を`/opt/sinfonia-specsearch`、サービスユーザーを`sinfonia`とします。

```bash
sudo apt-get update
sudo apt-get install --yes ca-certificates curl file git python3
sudo useradd --system --create-home --shell /usr/sbin/nologin sinfonia
sudo mkdir -p /opt/sinfonia-specsearch
sudo chown -R sinfonia:sinfonia /opt/sinfonia-specsearch
```

## 2. .NET 10 x64を導入する

自己完結publishしたBotの実行自体には共有ランタイムは不要ですが、VM上でのビルドと保守用に.NET 10 SDKを導入します。SDKにはx64ランタイムも含まれます。

```bash
curl --fail --location https://dot.net/v1/dotnet-install.sh --output /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
sudo /tmp/dotnet-install.sh --channel 10.0 --architecture x64 --install-dir /opt/dotnet
sudo ln -s /opt/dotnet/dotnet /usr/local/bin/dotnet
dotnet --info
```

## 3. 必要なディレクトリだけ取得する

`REPOSITORY_URL`は実際のGitリポジトリURLへ置き換えてください。partial cloneとsparse-checkoutを組み合わせ、Unityの`Assets/`全体を取得せず、仕様書とBotだけを作業ツリーへ展開します。

```bash
sudo -u sinfonia git clone --filter=blob:none --no-checkout --sparse REPOSITORY_URL \
  /opt/sinfonia-specsearch/repository
cd /opt/sinfonia-specsearch/repository
sudo -u sinfonia git sparse-checkout set Docs/NotionSpecifications SinfoniaOperator
sudo -u sinfonia git checkout main
```

更新時も同じ作業ツリーで`sudo -u sinfonia git pull --ff-only`を実行します。

## 4. 埋め込みモデルを取得する

選定モデルは`intfloat/multilingual-e5-small`です。ダウンロードスクリプトはモデルの特定リビジョンを固定し、ONNXモデルと対応するSentencePieceファイルを同じディレクトリへ配置します。

```bash
cd /opt/sinfonia-specsearch/repository
chmod +x SinfoniaOperator/deploy/download-embedding-model.sh
sudo -u sinfonia SinfoniaOperator/deploy/download-embedding-model.sh
```

## 5. publishとインデックス生成を行う

```bash
cd /opt/sinfonia-specsearch/repository
sudo -u sinfonia dotnet publish SinfoniaOperator/SinfoniaOperator/SinfoniaOperator.csproj \
  --configuration Release \
  --runtime linux-arm64 \
  --self-contained true \
  --output /opt/sinfonia-specsearch/publish
```

公開設定を作成します。Guild IDを設定すると`/spec`がそのGuildへ即時登録され、省略するとグローバル登録されます。`SPEC_SEARCH_TOP_K`を省略した場合は3件です。

```bash
sudo install --mode=600 --owner=root --group=root /dev/null /etc/sinfonia-specsearch.env
sudo editor /etc/sinfonia-specsearch.env
```

```dotenv
DISCORD_BOT_TOKEN=Discord Botトークン
SPEC_SEARCH_INDEX_PATH=/opt/sinfonia-specsearch/spec-index.bin
SPEC_SEARCH_EMBEDDING_MODEL_PATH=/opt/sinfonia-specsearch/models/multilingual-e5-small/model.onnx
SPEC_SEARCH_DISCORD_GUILD_ID=任意のGuild ID
SPEC_SEARCH_TOP_K=3
```

環境ファイルを読み込み、初回インデックスを生成します。

```bash
sudo bash -c 'set -a; source /etc/sinfonia-specsearch.env; set +a; exec runuser -u sinfonia -- /opt/sinfonia-specsearch/publish/SinfoniaOperator index'
```

仕様書を更新した場合は、Botを再起動する前に同じ`index`コマンドを再実行してください。

## 6. systemdへ登録する

```bash
sudo cp SinfoniaOperator/deploy/sinfonia-specsearch.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now sinfonia-specsearch.service
sudo systemctl status sinfonia-specsearch.service
```

ログと停止状態は次のコマンドで確認します。

```bash
sudo journalctl --unit sinfonia-specsearch.service --follow
sudo systemctl stop sinfonia-specsearch.service
```

## 7. GitHub Actionsから自動デプロイする

`[CI/CD] Deploy Sinfonia Operator`ワークフローは、`develop`のSinfoniaOperator関連ファイルが更新されたとき、または手動実行されたときに次の処理を行います。

1. Oracle VMのCPUに合う自己完結バイナリを発行する。
2. Oracle VMへ成果物を転送する。
3. `/opt/sinfonia-specsearch/releases/<commit SHA>-<run ID>-<attempt>`へ展開する。
4. VMの既存仕様キャッシュと既設モデルから、新リリース専用の索引を生成する。失敗した場合は稼働中のリリースを維持する。
5. `publish`シンボリックリンクを新リリースへ切り替える。
6. systemdサービスを再起動し、最大3分間、稼働状態と`/spec`・`/branches`両方の登録ログを確認する。
7. 稼働確認に失敗した場合は直前のリリースへ戻し、成功時は新しい3リリースだけを保持する。

GitHubの`Sinfonia Operator` Environmentに以下のSecretsを登録してください。

| Secret | 内容 |
|---|---|
| `ORACLE_HOST` | Oracle VMのホスト名またはIPアドレス。 |
| `ORACLE_SSH_USER` | 配備先を所有し、対象serviceへの`sudo systemctl restart/stop/start/is-active/status`と`sudo journalctl`をパスワードなし・非対話で実行できるユーザー。 |
| `ORACLE_SSH_PRIVATE_KEY` | 上記ユーザーのOpenSSH秘密鍵。 |
| `ORACLE_KNOWN_HOSTS` | `ssh-keyscan`などで事前に検証したOracle VMのhost key行。 |

VMはx86-64なので、Environment Variableの`ORACLE_RUNTIME`には`linux-x64`を設定します。未設定時も`linux-x64`が使われます。配備スクリプトも実機とバイナリのCPU形式を照合し、不一致の場合はサービスを止める前に中断します。SSHで`sudo -n systemctl status sinfonia-specsearch.service`と`sudo -n journalctl --unit sinfonia-specsearch.service --lines 1 --no-pager`が成功することを事前に確認してください。

公開envの`SPEC_SEARCH`だけをデプロイ対象に含めます。DiscordやGitHubのトークンはOracle VMにだけ保存します。新リリースの`spec-search.release.json`には検索設定・専用索引の絶対パス・モデルの絶対パスを保存し、serve時はVM側の同名設定より優先します。自動デプロイは`/spec`と`/branches`の登録ログをヘルスチェックに使うため、Botへ渡すJSONまたは環境ファイルに`GITHUB_REPOSITORY`を必ず設定してください。

索引生成はSSHユーザー権限で実行します。既存キャッシュ（`repository/Library/NotionSpecifications`優先、旧`repository/Docs/NotionSpecifications`へフォールバック）と`models/multilingual-e5-small`のモデル・トークナイザーを読み取れることが必要です。キャッシュは自動取得しません。更新手順と検索設定は[仕様検索README](../SinfoniaOperator.SpecSearch/README.md)を参照してください。

```json
"GITHUB_REPOSITORY": "HIBIKI5201/SymphonyKillChord"
```

`BRANCH_CLEANUP_DISCORD_GUILD_ID`を省略した場合は`SPEC_SEARCH_DISCORD_GUILD_ID`が使われます。GitHub APIの匿名レート制限を避ける場合は、読み取り権限だけを持つ`GITHUB_TOKEN`もOracle VM側へ設定します。
