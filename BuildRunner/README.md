# BuildRunner

自分のPCを、SymphonyKillChord の自動ビルド（GitHub Actions の `[CI/CD] Unity Build and Release`）を引き受ける**セルフホストランナー**にするためのフォルダ。

- develop へのマージなどでビルドが予約されると、起動中のランナーのどれか1台が Unity でビルドし、GitHub のリリースにアップロードする。
- ビルドは全体で同時に1つまで。ランナーが何台あっても、空いている1台が順番に引き受ける。
- ランナー本体と登録情報はこのフォルダの中にできるが、`.gitignore` でリポジトリの管理対象から外している。
- ビルドの作業フォルダは `BuildRunner\_work`。ランナーは `_work\SymphonyKillChord\SymphonyKillChord` という固定の形でリポジトリを取り出すのでパスが長くなりやすいが、Unity は 260 文字を超えるパスを扱えない。そこでワークフローが、ビルド先のパスが長いとき（70文字超）に短い別名 `<ドライブ>:\SKCBuild\<ランナー名>`（ジャンクション）を作り、そこから Unity を起動する。

| ファイル | 役割 |
|---|---|
| `setup-runner.ps1` | このPCをランナーにする（環境確認 → ランナー本体のダウンロードとハッシュ照合 → 登録 → 起動） |
| `start-runner.bat` / `start-runner.ps1` | 起動前に状態をスキャンして警告を出し、そのウィンドウでランナーを起動する |
| `runner-menu.bat` / `runner-menu.ps1` | 管理メニュー（起動・状態確認・CPU切り替え・ログ確認） |
| `SetBuildCpuMode.ps1` | 実行中のビルドの CPU 使用率を切り替える |

## 初めてランナーを作る人へ

### 必要なもの

| もの | 内容 |
|---|---|
| Windows のPC | ビルド中は CPU とディスクを多く使う。ノートPCは電源につないでおく |
| このリポジトリのクローン | ランナーは、クローンしたリポジトリの `BuildRunner` フォルダの中に作る |
| Unity Hub と Unity | `ProjectSettings/ProjectVersion.txt` と同じバージョン。モジュールは **Android Build Support**（OpenJDK、Android SDK & NDK Tools を含む）と **Windows Build Support (IL2CPP)** |
| Unity のライセンス | Unity Hub にサインインし、有効なライセンス（Personal でよい）があること |
| 空き容量 | 80GB 以上（Unity の Library とビルド成果物で数十GB使う） |
| GitHub CLI（`gh`） | なくてもよい。あると登録トークンの発行や、メニューでのビルド状況の表示に使える |
| 登録トークン | リポジトリのオーナーに発行してもらう（下記） |

足りないものは、`setup-runner.ps1` が確認して入れ方を案内する（Unity は Unity Hub のインストール画面を開ける。`gh` は `winget` で入れられる）。

### 手順

1. **オーナーに登録トークンを発行してもらう**（有効期限は1時間なので、作業の直前に頼む）。
   オーナーは次のコマンドで発行し、Discord の DM などで個別に渡す（公開のチャンネルには貼らない）。

   ```powershell
   gh api -X POST repos/HIBIKI5201/SymphonyKillChord/actions/runners/registration-token --jq .token
   ```

   自分がリポジトリの管理者で `gh` にログインしていれば、この手順はいらない（スクリプトが自動で発行する）。

2. **セットアップを実行する。** リポジトリのフォルダで PowerShell を開いて、次を実行する。

   ```powershell
   .\BuildRunner\setup-runner.ps1
   ```

   画面の案内に沿って進める。途中でトークンを聞かれたら、1で受け取ったものを貼り付ける。最後に「今すぐ起動しますか？」で `y` を押すと、ランナーが別ウィンドウで起動する。

3. **動いているか確かめる。** GitHub の Settings → Actions → Runners に、自分のPC名が「Idle」で出ていれば完了。

スクリプトの実行が止められる（「このシステムではスクリプトの実行が無効になっている」）ときは、次のどちらかで実行する。

```powershell
powershell -ExecutionPolicy Bypass -File .\BuildRunner\setup-runner.ps1
```

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

## 普段の使い方

### 起動する

`BuildRunner\start-runner.bat` をダブルクリックする。起動前に次の項目をスキャンし、問題がなければ**そのウィンドウがランナーになる**。

| 種類 | 項目 |
|---|---|
| 起動を止める | 未セットアップ、すでにランナーが起動している |
| 警告して確認する | 空き容量が 80GB 未満（40GB 未満は強い警告）、Unity やモジュールがない、ランナーの作業フォルダのプロジェクトを Unity で開いている、バッテリー駆動、一定時間でスリープする電源設定、GitHub に接続できない、前回 Ctrl+C などで止められた |

警告が出たときは `Y`（そのまま起動）か `n`（取りやめ）を選ぶ。

### 管理メニュー

`BuildRunner\runner-menu.bat` をダブルクリックすると管理メニューが開く。

| 番号 | 操作 |
|---|---|
| 1 | 状態を見る（ランナー・CPU切り替え・最近のビルド） |
| 2 | ランナーを起動する（`start-runner.bat` を別ウィンドウで開く） |
| 3〜6 | 実行中ビルドの CPU を 25 / 50 / 75 / 100% にする |
| 7 | CPU の切り替えを解除する |
| 8 | 実行中のビルドを見守る |
| 9 | ランナーの診断ログ（最後の20行）を見る |
| S | セットアップ（`setup-runner.ps1` を実行） |

ターミナルから直接実行もできる（例: `BuildRunner\runner-menu.bat cpu 100`、`status` / `start` / `runs` / `watch` / `log` / `setup`）。

### 気をつけること

- **ランナーのウィンドウで Ctrl+C を押さない・閉じない。** ランナーが止まり、実行中のビルドがキャンセルされる（診断ログに `Runner will be shutdown for UserCancelled` と出る）。コマンドは別のターミナルで実行する。
- **ランナーを止めたいときは、ビルドが動いていないとき（メニューの状態が「待機中」）に閉じる。**
- **`git clean -xdf` や `git clean -X` を実行しない。** 管理対象外のファイルを消すので、ランナー本体と登録情報も消える。消してしまったら、`-Remove` で登録を外してからセットアップをやり直す。
- PC がスリープするとビルドが止まる。ビルド中はスリープしない設定にしておく。

### CPU 使用率

- 自動ビルドは CPU の 50%、手動実行は実行時に選んだ割合（`cpu_usage_percent`）を使う。
- メニューの 3〜6（または `SetBuildCpuMode.ps1 100` など）で、実行中のビルドだけ割合を変えられる。30秒以内に反映され、そのジョブが終わると元に戻る。
- Unity の並列スレッド数（`-job-worker-count`）は起動時に決まるので、実行中の切り替えは「使ってよいコア」だけに効く。スレッド数まで変わるのは、次のプラットフォームのビルドで Unity が起動し直すときから。

## 手動でビルドを流す

```powershell
# 例: Development / demo / Windows だけ / CPU 100%
gh workflow run BuildAndRelease.yml --repo HIBIKI5201/SymphonyKillChord --ref develop -f build_mode=Development -f build_variant=demo -f profile_android=false -f profile_windows=true -f profile_macos=false -f profile_ios=false -f cpu_usage_percent=100%
```

- `clean_build=true` を付けると Unity の Library を消して作り直す。トラブルのときだけ使う（Windows でも1時間以上延びる）。
- タイムアウト: Android は 180 分、ほかは 120 分。

## 作業フォルダを短い場所に移す

ふだんは不要（パスが長くても、ワークフローが短い別名を作って Unity を起動する）。ビルドが「短い別名（…）を作ろうとしましたが、失敗しました」というエラーで止まったときだけ、作業フォルダを短い場所に移す。登録し直す必要はない。

1. ビルドが動いていないとき（メニューの状態が「待機中」）に、ランナーのウィンドウを閉じる。
2. `BuildRunner\.runner` をメモ帳で開き、`"workFolder": "_work"` を短い場所に書き換える（例: `"workFolder": "C:\\SKCWork"`。`\` は2つ重ねる）。
3. 今の作業フォルダの中身（`_work\SymphonyKillChord`）を新しい場所に移す。21GB ほどあり、同じドライブでも10分ほどかかる。移さなくてもよいが、その場合は最初のビルドで Library を作り直すので1時間ほど長くかかる。

   ```powershell
   New-Item -ItemType Directory C:\SKCWork -Force
   robocopy .\BuildRunner\_work\SymphonyKillChord C:\SKCWork\SymphonyKillChord /E /MOVE /R:1 /W:1 /MT:16 /NFL /NDL /NP
   ```

4. `start-runner.bat` で起動する。

## ランナーをやめる

```powershell
.\BuildRunner\setup-runner.ps1 -Remove
```

ランナーのウィンドウを閉じてから実行する。リポジトリの管理者でなければ、登録解除用のトークンをオーナーに発行してもらう（`.../actions/runners/remove-token`）。そのあと `BuildRunner` の管理対象外のファイル（`bin`、`externals`、`_work`、`_diag` など）を消せば元に戻る。

## 困ったとき

| 症状 | 見るところ |
|---|---|
| ビルドがずっと「待機」のまま | 起動中のランナーがあるか（GitHub の Settings → Actions → Runners、またはメニューの 1） |
| ジョブが急に「The operation was canceled」で止まった | `_diag\Runner_*.log` の最後に `UserCancelled` があれば、ランナーのウィンドウで止められている |
| Unity のビルドが失敗した | 実行結果の Artifacts にある `unity-build-log` をダウンロードし、`Exception:` や `Error building Player` を探す |
| `Unable to build with the current configuration` | Library の状態が原因のことがある。`clean_build=true` で流し直す |
| セットアップで登録に失敗した | トークンの期限（1時間）が切れていないか。切れていたら発行し直してもらう |

## セキュリティ

- このリポジトリは公開されている。ランナーのPCでは、ワークフローに書かれたコマンドが実行される。今のワークフローは「マージ後」と「手動実行」でしか動かないので、外部の人の PR がそのままランナーで実行されることはない。
- 登録トークンと、このフォルダにできる `.credentials` などの登録情報は秘密情報。共有したりコミットしたりしない（`.gitignore` で除外している）。
