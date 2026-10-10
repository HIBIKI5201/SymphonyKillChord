# 例外時の復旧

### 例外: 自動の Draft PR が作られないとき

- 作業ブランチと `master` が同じコミットだと（先に編集してから push した場合など）、agent→master の PR は「No commits between」で作れない。この場合は手順 3・4 を省き、`master` を作業ブランチに合わせてから、ワークフローを手動で実行する:
  ```bash
  git push origin feature/<段階>/<作業名>/agent:feature/<段階>/<作業名>/master
  gh workflow run AutoCreateDevelopPullRequest.yml --ref develop -f master_branch=feature/<段階>/<作業名>/master
  ```
  元になる PR が無いので、本文はコミット一覧から作られる。作成後に目印の行を消し、手順 3 と同じ粒度で本文を書き直す（`Closes #N` もここで書く）。
- ワークフローが 403（Actions に PR 作成の権限がない）で警告だけ出して終わった場合は、`gh pr create --draft --base develop --head <master> --title "<タイトル>" --body-file <本文ファイル>` で手動で作る。
