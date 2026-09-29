# apps

CI で使う GitHub App の設定を記録したもの。

- 正本は GitHub の設定（App の所有者の Settings → Developer settings → GitHub Apps）。このファイルを編集しても設定には反映されない。
- 設定を変更したら、ここの JSON と README を更新してコミットする。
- **秘密鍵（.pem）は絶対にコミットしない。** このリポジトリは公開されている。秘密鍵は Environment の Secret にだけ置く。

## sinfonia-variable-updater

`BuildAndRelease.yml` がリポジトリ変数（`VERSION_*`）を読み書きするための App。
`GITHUB_TOKEN` にはリポジトリ変数を書き換える権限がないため、以前は個人の Fine-grained token（`VAR_UPDATE_TOKEN`、2026-09-29 に削除）を使っていた。
App に切り替えると、トークンの期限切れや、発行した個人のアカウントへの依存がなくなる。

| 項目 | 値 |
| --- | --- |
| 設定の記録 | [sinfonia-variable-updater.json](sinfonia-variable-updater.json)（App Manifest 形式） |
| 所有者 | `HIBIKI5201` |
| インストール先 | `HIBIKI5201/SymphonyKillChord` のみ |
| 権限 | Repository permissions → Variables: Read and write（Metadata: Read-only は自動で付く） |
| Webhook | 使わない |
| App ID | `5119490` |
| Client ID | Environment `AutoBuildAndRelease` の変数 `SINFONIA_VARIABLE_UPDATER_CLIENT_ID` |
| 秘密鍵 | Environment `AutoBuildAndRelease` の Secret `SINFONIA_VARIABLE_UPDATER_PRIVATE_KEY` |

### ワークフローでの使い方

変数を読み書きするステップの直前で `actions/create-github-app-token` を使い、トークンを発行する。
発行されるトークンは1時間で失効するため、ジョブの最初で発行してビルド後まで使い回さない。

`create-github-app-token` には Variables 用の `permission-*` 入力がないため、権限の絞り込みは App 側の権限設定で行う。
App に Variables 以外の権限を足すと、ワークフローのトークンにもその権限が付く点に注意する。

### 作成手順

1. GitHub 右上のアイコン → Settings → Developer settings → GitHub Apps → **New GitHub App** を開く。
2. [sinfonia-variable-updater.json](sinfonia-variable-updater.json) と同じ内容で設定する。
   - GitHub App name: `sinfonia-variable-updater`
   - Homepage URL: `https://github.com/HIBIKI5201/SymphonyKillChord`
   - Webhook: **Active のチェックを外す**
   - Repository permissions → **Variables: Read and write**（ほかは No access のまま）
   - Where can this GitHub App be installed?: **Only on this account**
3. **Create GitHub App** を押す。作成後の画面に表示される **Client ID** を控える。
4. 同じ画面の Private keys → **Generate a private key** を押し、.pem ファイルをダウンロードする。
5. 左メニューの Install App → `HIBIKI5201` の **Install** → **Only select repositories** → `SymphonyKillChord` を選んでインストールする。
6. Client ID と秘密鍵を Environment に登録する。
   ```bash
   gh variable set SINFONIA_VARIABLE_UPDATER_CLIENT_ID --env AutoBuildAndRelease --repo HIBIKI5201/SymphonyKillChord --body "<Client ID>"
   gh secret set SINFONIA_VARIABLE_UPDATER_PRIVATE_KEY --env AutoBuildAndRelease --repo HIBIKI5201/SymphonyKillChord < <ダウンロードした.pem>
   ```
7. 登録できたら、手元の .pem ファイルを削除する（必要になったら新しい鍵を発行すればよい）。

### 引き継ぎ

- **所有者を変える**: App の設定画面 → Advanced → **Transfer ownership of this GitHub App** で、別のユーザーまたは Organization に譲渡する。Client ID と秘密鍵はそのまま使えるので、ワークフローも Environment も変更しなくてよい。
- **作り直す**（元の所有者に連絡が取れないなど）: 上の作成手順で同じ設定の App を作り、新しい Client ID と秘密鍵で Environment の変数と Secret を上書きする。名前はGitHub全体で重複できないため、元の App が残っている場合は別の名前にし、この README と JSON も更新する。

### 秘密鍵の入れ替え

1. App の設定画面で新しい秘密鍵を発行する（1つの App に複数の鍵を持てる）。
2. `SINFONIA_VARIABLE_UPDATER_PRIVATE_KEY` を新しい鍵で上書きし、ビルドが通ることを確かめる。
3. 古い鍵を App の設定画面で削除する。

鍵が漏れた疑いがあるときは、先に古い鍵を削除してから新しい鍵を登録する。
