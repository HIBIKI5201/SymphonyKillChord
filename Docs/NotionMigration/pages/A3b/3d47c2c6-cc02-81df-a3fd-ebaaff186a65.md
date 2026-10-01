# Oracle Cloudデプロイ手順

- page_id: 3d47c2c6-cc02-81df-a3fd-ebaaff186a65
- Notion パス: Symphony Kill Chord / システム概要 / SinfoniaOperator / Oracle Cloudデプロイ手順
- スナップショットの最終更新: 2026-09-07T17:11:49.620Z
- 書き込み許可: 可
- 処置: 本文置き換え

## 判定

- 信頼性: 一部古い — 確認コミット 73fefeb45。実構築の制約と回避策（E2.1.Micro・Oracle Linux 9・スワップ 6GB・VM でビルドしない）はリポジトリ側の手順書より新しく、残す価値がある。一方、通常の配備は GitHub Actions で自動化された: `.github/workflows/DeploySinfoniaOperator.yml`（#1538）が `develop` への push（SinfoniaOperator 本体・Core・SpecSearch・`deploy/deploy-release.sh`・ワークフロー自身の変更時）または手動で起動し、`linux-x64`（変数 `ORACLE_RUNTIME` で変更可）の self-contained publish を作って SSH で送る。`deploy/deploy-release.sh` がリリースを切り替えてサービスを再起動し、稼働確認に失敗したら前のリリースへ戻す（:95-184、保持数 `MAX_RELEASE_COUNT=3`）。systemd ユニットは `User=sinfonia`、`EnvironmentFile=/etc/sinfonia-specsearch.env`、`ExecStart=/opt/sinfonia-specsearch/publish/SinfoniaOperator serve`、`RestartSec=10`（`deploy/sinfonia-specsearch.service`）
- 整合性:
  - リポジトリの `SinfoniaOperator/deploy/oracle-vm-setup.md` は 09-14 に見出しを x86-64 に直したが、手順 5 の publish は `--runtime linux-arm64` のまま（:59）。手順 3 の sparse-checkout で `Docs/NotionSpecifications` を取る手順は、同フォルダが `.gitignore` の対象のため機能しない（D-09）
  - 本ページ内で埋め込みモデルの大きさが「470MB」（制約 3）と「448MB」（動作確認済みの構成）で食い違う
  - 「設定・環境変数リファレンス」(3d47c2c6-cc02-81e6-93d5-fdca6adc65f9) の VM 設定ファイル名を `/etc/sinfonia-specsearch.env` に揃えた
- 変更点:
  - 冒頭の段落: 通常は自動配備であり、本ページの手作業は初回構築と障害時のためであることを追加した
  - 「想定と実際の差」節: リポジトリの手順書が見出しだけ x86-64 に直り、publish の指定が arm64 のまま残っていることを追加した
  - 「4. VM上で`dotnet build`しない」節: 通常は GitHub Actions（ubuntu-latest）が publish することを追加した
  - 「5. 秘匿情報の転送」節: 旧「仕様検索Bot用の設定ファイルへ抽出」→ 新「`/etc/sinfonia-specsearch.env`（root 所有・600）へ書き込む」。`GITHUB_REPOSITORY` が必須であることを追加した
  - 「systemdへの登録」節: ユニットの主な設定（実行ユーザー・環境ファイル・作業ディレクトリ・再起動間隔）を追加した
  - 新節「GitHub Actionsによる自動デプロイ」: 起動条件・処理の流れ・Secrets・前提を追加した（既存節の後ろ）
- 織り込んだ反映項目: TL-13
- 出典: `.github/workflows/DeploySinfoniaOperator.yml`、`SinfoniaOperator/deploy/deploy-release.sh`、`SinfoniaOperator/deploy/sinfonia-specsearch.service`、`SinfoniaOperator/deploy/oracle-vm-setup.md` §5・§7、PR #1538、01 棚卸し（oracle-vm-setup.md 行）
- 要確認:
  - 埋め込みモデルの ONNX ファイルの大きさ（470MB か 448MB か。VM 上で `ls -l` で確認）
  - 現在の VM 構成（シェイプ・OS・スワップ）が 2026-09 時点から変わっていないか（要実機確認）
  - VM 上のインデックスの元になる仕様書ミラーの用意方法（sparse-checkout 手順が機能しないため。VM 管理者に確認）

## 適用する本文

# Oracle Cloudデプロイ手順 {color="gray_bg"}
仕様検索Bot（`serve`サブコマンド）は、Oracle Cloud Infrastructure（OCI）のAlways Free枠VM上でsystemdサービスとして常駐させる。手順書は`SinfoniaOperator/deploy/oracle-vm-setup.md`にもあるが、このページでは**実際の構築で遭遇した制約と回避策**を優先して記録する。手順書どおりに進まない場合、まずこのページの既知の制約を確認する。
Botのプログラムの配備は、通常はGitHub Actionsで自動的に行われる（後述「GitHub Actionsによる自動デプロイ」）。本ページの手作業の手順は、VMの初回構築と、自動配備が使えないときのためのものである。

## 想定と実際の差
手順書（`oracle-vm-setup.md`）は当初Ampere A1（ARM64、4 OCPU/24GB、Always Free）とUbuntuを前提に書かれていた。しかし実際の構築（東京リージョン）では、この前提が崩れた。
| 項目 | 当初の想定 | 実際 |
|---|---|---|
| シェイプ | `VM.Standard.A1.Flex`（Ampere、ARM64） | `VM.Standard.E2.1.Micro`（AMD、x86_64） |
| OS | Ubuntu 22.04 | Oracle Linux 9 |
| CPU/メモリ | 4 OCPU/24GB | 1 OCPU/1GB |

以降の手順は、この実際の構成（AMD・Oracle Linux・1GBメモリ）を前提に読み替える。ARM64版の需給が回復すれば、A1へ載せ替える価値はある（後述）。
なお手順書は見出しをx86-64に直してあるが、publishの手順には`--runtime linux-arm64`が残っている。x86_64のVMでは`linux-x64`を指定する。

## 既知の制約と回避策

### 1. Ampere A1が東京リージョンで容量不足になる
`VM.Standard.A1.Flex`でインスタンス作成を実行すると、`Out of capacity for shape VM.Standard.A1.Flex in availability domain AD-1`のAPIエラーで失敗することがある。Always Free枠のA1は人気が高く、東京リージョンでは恒常的に品薄である。OCPU数・メモリを減らしても（1 OCPU/6GBまで下げても）解消しないことがある。
回避策の候補は次のとおりだが、いずれも制約がある。
- **数分〜数時間おきにリトライする。** 空きが出た瞬間に作成できる。今回はこの方法を試みたが、短時間（数十秒〜数分間隔）のリトライでは解消しなかった。
- **別リージョンへサブスクライブする。** Free Tierアカウントでは、サブスクライブできるリージョン数の上限に達していて追加できない場合がある（`You have exceeded the maximum number of regions allowed for your tenancy`）。ホームリージョンの変更は基本的に不可逆な操作であり、慎重な判断が要る。
- **AMDのAlways Freeシェイプ（`VM.Standard.E2.1.Micro`）へフォールバックする。** 今回はこれを採用した。即座に作成できる代わりに、1 OCPU/1GBという小さいスペックになる。

A1が確保できたら、後から`E2.1.Micro`のインスタンスを廃止してA1へ載せ替えることを推奨する。手順は本ページの内容をそのままARM64向けに読み替えればよい（`dotnet publish`のRuntime Identifierを`linux-arm64`にする点と、自動配備のEnvironment Variable`ORACLE_RUNTIME`を`linux-arm64`にする点が変わる）。

### 2. 新規VCN作成ウィザードでPublic IP自動割当のトグルが効かない
インスタンス作成画面で「Networking」ステップの「Create new virtual cloud network」を選び、新規サブネットも同時に作成する構成にすると、「Automatically assign public IPv4 address」のトグルがUI上グレーアウトして操作できず、初期状態のOFFのまま進んでしまう。この状態で作成すると、Public IPが付与されないインスタンスができあがり、SSH接続ができない。
回避策は、**VCN・サブネット・Internet Gateway・ルートテーブルを先に個別作成しておき、インスタンス作成時には「Select existing virtual cloud network」で既存のPublic Subnetを選択する**ことである。既存のPublic Subnetを選んだ場合は、トグルが正しく有効化され、Public IPが自動的に割り当てられる。
個別作成の手順（Networking画面から）は次のとおり。
1. VCNを作成する（IPv4 CIDRブロックのみ指定すればよい。インターネット接続ウィザードは無いので手動で組み立てる）。
2. Internet Gatewayを作成し、VCNへアタッチする。
3. VCNのデフォルトルートテーブルに、宛先`0.0.0.0/0`・ターゲットをそのInternet GatewayとするルートをAdd Route Rulesで追加する。
4. Subnetを作成する。「Subnet Access」を「Public Subnet」にする（既定で選ばれている）。ルートテーブルは手順3で編集したデフォルトのものを使う。
5. インスタンス作成時、Networkingステップで「Select existing virtual cloud network」を選び、手順1〜4で作ったVCN・Subnetを選択する。

### 3. 1GBメモリのVMでONNX Runtimeの推論がOOM Killerに落とされる
`E2.1.Micro`（1GBメモリ）上で`index`サブコマンドを実行すると、埋め込みモデル（`multilingual-e5-small`、ONNXファイルで470MB）の読み込み・推論中にLinuxのOOM Killerがプロセスを強制終了することがある。`dmesg`に`Out of memory: Killed process ... (SinfoniaOperato)`と記録される。
【要確認: ONNXファイルの大きさ。本ページの「動作確認済みの構成」では448MBと書いている】
回避策は、**スワップ領域を増設する**ことである。デフォルトの約500MBのスワップでは不足したため、6GBのスワップファイルを追加した。
```Bash
sudo fallocate -l 6G /swapfile2
sudo chmod 600 /swapfile2
sudo mkswap /swapfile2
sudo swapon /swapfile2
# 再起動後も有効にする
echo '/swapfile2 none swap sw 0 0' | sudo tee -a /etc/fstab
```
スワップを増設すれば処理自体は完走するが、**物理メモリが足りないためディスクI/Oが発生し、非常に遅い。** 2000件を超える仕様書チャンクの初回インデックス生成には数分〜十数分単位の時間がかかる。これは一時的な負荷であり、`index`サブコマンドの実行中だけ発生する。`serve`サブコマンド（常駐時の検索応答）でも1回のクエリごとに埋め込み推論が走るため、同様に応答が遅くなる可能性がある。応答速度が問題になる場合は、A1（メモリに余裕がある）への載せ替えを検討する。

### 4. VM上で`dotnet build`しない
1GBメモリのVMで`.NET` SDKによるビルドを行うと、コンパイラ自体のメモリ使用量で容易にOOMする。**ビルドはVMの外で行い、自己完結（self-contained）publishした成果物をVMへ転送する運用にする。** 通常はGitHub Actions（`ubuntu-latest`）がpublishと転送を行う。手作業で行う場合は開発機で次のコマンドを使う。
```Bash
dotnet publish SinfoniaOperator/SinfoniaOperator/SinfoniaOperator.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --output <出力先>
```
シェイプがAMD（x86_64）の場合は`linux-x64`、ARM64（A1）の場合は`linux-arm64`を指定する。自己完結publishのため、VM側に`.NET`ランタイムをインストールする必要は無い。成果物一式（100MB程度）は`scp`でVMへ転送する。

### 5. 秘匿情報の転送
BotトークンなどをVMへ配置する際、ローカルの`sinfonia-operator.secrets.json`をそのまま`scp`で転送し、VM上で`jq`を使ってトークンだけを環境ファイル`/etc/sinfonia-specsearch.env`（root所有・権限600）へ書き込み、元の転送ファイルは削除する。**トークンの値をターミナルの標準出力に表示しない**（コマンド履歴やログに残るリスクを避ける）。
自動配備は`/branches`の登録ログを稼働確認に使うため、環境ファイルには`GITHUB_REPOSITORY`を必ず設定する。設定ファイルは自動配備の対象に含めない（VMにだけ置く）。

## 動作確認済みの構成（2026年9月時点）
| 項目 | 値 |
|---|---|
| リージョン | ap-tokyo-1（Japan East） |
| シェイプ | `VM.Standard.E2.1.Micro`（1 OCPU/1GB、Always Free） |
| OS | Oracle Linux 9 |
| 追加スワップ | 6GB（`/swapfile2`） |
| Publish設定 | `linux-x64`、自己完結 |
| 埋め込みモデル | `intfloat/multilingual-e5-small`（ONNX、448MB） |

## systemdへの登録
自己完結publishしたバイナリと設定ファイルの配置後、`SinfoniaOperator/deploy/sinfonia-specsearch.service`をsystemdへ登録する。
```Bash
sudo cp sinfonia-specsearch.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now sinfonia-specsearch.service
```
ユニットの主な設定は次のとおりである。
| 項目 | 値 |
|---|---|
| 実行ユーザー | `sinfonia` |
| 作業ディレクトリ | `/opt/sinfonia-specsearch/repository` |
| 環境ファイル | `/etc/sinfonia-specsearch.env` |
| 起動コマンド | `/opt/sinfonia-specsearch/publish/SinfoniaOperator serve` |
| 再起動 | `on-failure`、10秒後 |

ログは`journalctl --unit sinfonia-specsearch.service --follow`で確認する。`Restart=on-failure`を設定しているため、OOM等でプロセスが落ちた場合は自動再起動する（ただし原因がメモリ不足であれば再起動しても再発する点に注意する）。

## ネットワーク（インバウンド）について
このBotはDiscord Gatewayへのアウトバウンド接続のみで動作し、外部からのインバウンド接続を待ち受けない。そのためOracle側のセキュリティリスト・NSG・Oracle Linuxのファイアウォールで、SSH（22番）以外のポートを開放する必要は無い。

## GitHub Actionsによる自動デプロイ
`.github/workflows/DeploySinfoniaOperator.yml`（Deploy Sinfonia Operator）が、Botのプログラムを自動でVMへ配備する。
- 起動条件: `develop`へのpushで、`SinfoniaOperator/SinfoniaOperator/`・`SinfoniaOperator.Core/`・`SinfoniaOperator.SpecSearch/`・`deploy/deploy-release.sh`・ワークフロー自身のいずれかが変わったとき。手動実行（`workflow_dispatch`）もできる。同時に動く配備は1つだけで、後続は待つ。
- 処理の流れ:
  1. `ubuntu-latest`で、`ORACLE_RUNTIME`（未設定なら`linux-x64`）向けの自己完結publishを作る。
  2. 成果物と`deploy-release.sh`をSSHでVMの`/opt/sinfonia-specsearch/incoming/`へ転送する。
  3. `deploy-release.sh`が`/opt/sinfonia-specsearch/releases/<コミットSHA>-<実行ID>-<試行回数>`へ展開する。実機とバイナリのCPU形式が違う場合は、サービスを止める前に中断する。
  4. `publish`シンボリックリンクを新しいリリースへ切り替え、サービスを再起動する。
  5. 最大3分間、サービスが稼働しているかと、`/branches`の登録ログが出たかを確かめる。
  6. 確認に失敗したら直前のリリースへ戻す。成功したら新しい3リリースだけを残し、古いものを消す。
- GitHubのEnvironment「Sinfonia Operator」に登録するSecretsは`ORACLE_HOST`・`ORACLE_SSH_USER`・`ORACLE_SSH_PRIVATE_KEY`・`ORACLE_KNOWN_HOSTS`である。`ORACLE_SSH_USER`は、対象サービスへの`sudo systemctl`と`sudo journalctl`をパスワードなしで実行できる必要がある。
- 自動配備はプログラムの入れ替えだけを行う。仕様書のインデックス（`spec-index.bin`）は作り直さないので、仕様書を更新したときは「仕様検索Bot」ページの手順で`index`を再実行する。
