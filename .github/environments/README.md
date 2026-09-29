# environments

GitHub Actions の Environment の構成を記録したもの。

- 正本は GitHub の設定（Settings → Environments）。このファイルを編集しても設定には反映されない。
- 記録するのは Secret・変数の**名前と用途**と保護ルールだけ。**値は書かない。** このリポジトリは公開されている。
- Secret や変数を追加・削除したら、ここの JSON を更新してコミットする。現在の構成は次のコマンドで確認できる。
  ```bash
  gh api repos/HIBIKI5201/SymphonyKillChord/environments/<名前>
  gh secret list --env <名前> --repo HIBIKI5201/SymphonyKillChord
  gh variable list --env <名前> --repo HIBIKI5201/SymphonyKillChord
  ```
- 同じ名前の Secret がリポジトリと Environment の両方にあると、Environment 側が優先される。CI 用の Secret は Environment にだけ置き、リポジトリ側に同じ名前を作らない。

| Environment | 用途 |
| --- | --- |
| [AutoBuildAndRelease](AutoBuildAndRelease.json) | Unity の自動ビルドとリリース（`BuildAndRelease.yml`） |
