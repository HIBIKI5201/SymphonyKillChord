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
- 「未参照」と書いたものは、記録した時点でどのワークフローからも渡していない。消す前に、ワークフローやツールで使う予定がないか確かめる。
- 同じ名前の Secret がリポジトリと Environment の両方にあると、Environment 側が優先される。CI 用の Secret は Environment にだけ置き、リポジトリ側に同じ名前を作らない。

| Environment | 用途 |
| --- | --- |
| [AutoBuildAndRelease](AutoBuildAndRelease.json) | Unity の自動ビルドとリリース（`BuildAndRelease.yml`） |
| [Sinfonia Operator](SinfoniaOperator.json) | Sinfonia Operator の定期実行と配備（`SinfoniaOperator.yml` / `DeploySinfoniaOperator.yml`） |
| [github-pages](github-pages.json) | ホームページの GitHub Pages への配備（`HomePageDeploy.yml`） |
| [(リポジトリ全体)](repository.json) | Environment に属さない、リポジトリの Secret・変数 |
