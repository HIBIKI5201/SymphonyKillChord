# Oracle Cloudデプロイ手順

- id: 3d47c2c6-cc02-81df-a3fd-ebaaff186a65
- path: Symphony Kill Chord / システム概要 / SinfoniaOperator / Oracle Cloudデプロイ手順
- last_edited: 2026-09-07T17:11:49.620Z


## Oracle Cloudデプロイ手順
仕様検索Bot（`serve`サブコマンド）は、Oracle Cloud Infrastructure（OCI）のAlways Free枠VM上でsystemdサービスとして常駐させる。手順書は`SinfoniaOperator/deploy/oracle-vm-setup.md`にもあるが、このページでは**実際の構築で遭遇した制約と回避策**を優先して記録する。手順書どおりに進まない場合、まずこのページの既知の制約を確認する。

### 想定と実際の差
手順書（`oracle-vm-setup.md`）はAmpere A1（ARM64、4 OCPU/24GB、Always Free）とUbuntuを前提に書かれている。しかし実際の構築（東京リージョン）では、この前提が崩れた。
| 項目 | 当初の想定 | 実際 |
|---|---|---|
| シェイプ | `VM.Standard.A1.Flex`（Ampere、ARM64） | `VM.Standard.E2.1.Micro`（AMD、x86_64） |
| OS | Ubuntu 22.04 | Oracle Linux 9 |
| CPU/メモリ | 4 OCPU/24GB | 1 OCPU/1GB |
以降の手順は、この実際の構成（AMD・Oracle Linux・1GBメモリ）を前提に読み替える。ARM64版の需給が回復すれば、A1へ載せ替える価値はある（後述）。

### 既知の制約と回避策

#### 1. Ampere A1が東京リージョンで容量不足になる
`VM.Standard.A1.Flex`でインスタンス作成を実行すると、`Out of capacity for shape VM.Standard.A1.Flex in availability domain AD-1`のAPIエラーで失敗することがある。Always Free枠のA1は人気が高く、東京リージョンでは恒常的に品薄である。OCPU数・メモリを減らしても（1 OCPU/6GBまで下げても）解消しないことがある。
回避策の候補は次のとおりだが、いずれも制約がある。
- **数分〜数時間おきにリトライする。** 空きが出た瞬間に作成できる。今回はこの方法を試みたが、短時間（数十秒〜数分間隔）のリトライでは解消しなかった。
- **別リージョンへサブスクライブする。** Free Tierアカウントでは、サブスクライブできるリージョン数の上限に達していて追加できない場合がある（`You have exceeded the maximum number of regions allowed for your tenancy`）。ホームリージョンの変更は基本的に不可逆な操作であり、慎重な判断が要る。
- **AMDのAlways Freeシェイプ（**`**VM.Standard.E2.1.Micro**`**）へフォールバックする。** 今回はこれを採用した。即座に作成できる代わりに、1 OCPU/1GBという小さいスペックになる。
A1が確保できたら、後から`E2.1.Micro`のインスタンスを廃止してA1へ載せ替えることを推奨する。手順は本ページの内容をそのままARM64向けに読み替えればよい（`dotnet publish`のRuntime Identifierを`linux-arm64`にする点だけが変わる）。

#### 2. 新規VCN作成ウィザードでPublic IP自動割当のトグルが効かない
インスタンス作成画面で「Networking」ステップの「Create new virtual cloud network」を選び、新規サブネットも同時に作成する構成にすると、「Automatically assign public IPv4 address」のトグルがUI上グレーアウトして操作できず、初期状態のOFFのまま進んでしまう。この状態で作成すると、Public IPが付与されないインスタンスができあがり、SSH接続ができない。
回避策は、**VCN・サブネット・Internet Gateway・ルートテーブルを先に個別作成しておき、インスタンス作成時には「Select existing virtual cloud network」で既存のPublic Subnetを選択する**ことである。既存のPublic Subnetを選んだ場合は、トグルが正しく有効化され、Public IPが自動的に割り当てられる。
個別作成の手順（Networking画面から）は次のとおり。
1. VCNを作成する（IPv4 CIDRブロックのみ指定すればよい。インターネット接続ウィザードは無いので手動で組み立てる）。
2. Internet Gatewayを作成し、VCNへアタッチする。
3. VCNのデフォルトルートテーブルに、宛先`0.0.0.0/0`・ターゲットをそのInternet GatewayとするルートをAdd Route Rulesで追加する。
4. Subnetを作成する。「Subnet Access」を「Public Subnet」にする（既定で選ばれている）。ルートテーブルは手順3で編集したデフォルトのものを使う。
5. インスタンス作成時、Networkingステップで「Select existing virtual cloud network」を選び、手順1〜4で作ったVCN・Subnetを選択する。

#### 3. 1GBメモリのVMでONNX Runtimeの推論がOOM Killerに落とされる
`E2.1.Micro`（1GBメモリ）上で`index`サブコマンドを実行すると、埋め込みモデル（`multilingual-e5-small`、ONNXファイルで470MB）の読み込み・推論中にLinuxのOOM Killerがプロセスを強制終了することがある。`dmesg`に`Out of memory: Killed process ... (SinfoniaOperato)`と記録される。
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

#### 4. VM上で`dotnet build`しない
1GBメモリのVMで`.NET` SDKによるビルドを行うと、コンパイラ自体のメモリ使用量で容易にOOMする。**ビルドは開発機（ローカル）で行い、自己完結（self-contained）publishした成果物をVMへ転送する運用にする。**
```Bash
dotnet publish SinfoniaOperator/SinfoniaOperator/SinfoniaOperator.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --output <出力先>
```
シェイプがAMD（x86_64）の場合は`linux-x64`、ARM64（A1）の場合は`linux-arm64`を指定する。自己完結publishのため、VM側に`.NET`ランタイムをインストールする必要は無い。成果物一式（100MB程度）は`scp`でVMへ転送する。

#### 5. 秘匿情報の転送
BotトークンなどをVMへ配置する際、ローカルの`sinfonia-operator.secrets.json`をそのまま`scp`で転送し、VM上で`jq`を使ってトークンだけを仕様検索Bot用の設定ファイルへ抽出し、元の転送ファイルは削除する。**トークンの値をターミナルの標準出力に表示しない**（コマンド履歴やログに残るリスクを避ける）。

### 動作確認済みの構成（2026年9月時点）
| 項目 | 値 |
|---|---|
| リージョン | ap-tokyo-1（Japan East） |
| シェイプ | `VM.Standard.E2.1.Micro`（1 OCPU/1GB、Always Free） |
| OS | Oracle Linux 9 |
| 追加スワップ | 6GB（`/swapfile2`） |
| Publish設定 | `linux-x64`、自己完結 |
| 埋め込みモデル | `intfloat/multilingual-e5-small`（ONNX、448MB） |

### systemdへの登録
自己完結publishしたバイナリと設定ファイルの配置後、`SinfoniaOperator/deploy/sinfonia-specsearch.service`をsystemdへ登録する。
```Bash
sudo cp sinfonia-specsearch.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now sinfonia-specsearch.service
```
ログは`journalctl --unit sinfonia-specsearch.service --follow`で確認する。`Restart=on-failure`を設定しているため、OOM等でプロセスが落ちた場合は自動再起動する（ただし原因がメモリ不足であれば再起動しても再発する点に注意する）。

### ネットワーク（インバウンド）について
このBotはDiscord Gatewayへのアウトバウンド接続のみで動作し、外部からのインバウンド接続を待ち受けない。そのためOracle側のセキュリティリスト・NSG・Oracle Linuxのファイアウォールで、SSH（22番）以外のポートを開放する必要は無い。