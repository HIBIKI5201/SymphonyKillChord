# 二段階PR

## 3. agent → master の PR を作る

本文は必ず [.github/PULL_REQUEST_TEMPLATE.md](../../../../.github/PULL_REQUEST_TEMPLATE.md) の構成にする（Summary / Test plan のような汎用の形式にはしない）。

- 必須チェックの項目は、**実際に確かめたものだけ** `[x]` にする。PlayMode を通していなければ `[ ]` のままにし、理由を「未確認・残論点」に書く。
  - 例外: 確認が**不要**な変更（C#を変更していない → コンパイル不要、Unity外のワークフロー・ドキュメント等だけ → PlayMode不要）は、`[x]` にして末尾に `（不要: 理由）` を書く。括弧の補足は develop 向け Draft PR にも引き継がれる。
- 「確認済みの内容」には、確認の方法（uloop-compile / dotnet build / PlayMode / テスト名）を項目ごとに書く。
- 本文の末尾に、会話で指定された PR 用の署名行を付ける。
- クローズしたい Issue は「クローズするIssue」節に `- #N` で書く（無ければ「なし」）。ここに書いたものだけが develop 向け PR の `Closes` になる。本文中の `#N` は参照扱いで、クローズされない。
- この本文が、そのまま develop 向け Draft PR の本文の材料になる。各節を省略せずに埋める。

```bash
gh pr create --base feature/<段階>/<作業名>/master --head feature/<段階>/<作業名>/agent \
  --title "<作業内容の要約>" --body-file <本文ファイル>
```

本文ファイルは scratchpad に書く。`ValidatePRTarget.yml` が、すべてのPRについてマージ元とマージ先の組み合わせを検査する（commit status `pr-target`）。作業ブランチのマージ先は同じ階層の `master` だけが通る。

## 4. master へセルフマージする

```bash
gh pr checks <PR番号>          # pr-target が成功していること
gh pr merge <PR番号> --merge   # マージコミットで取り込む（squash しない）
```

マージすると `AutoCreateDevelopPullRequest.yml` が `master` → `develop` の Draft PR を自動作成する。完了を待つ:

```bash
gh run watch $(gh run list --workflow AutoCreateDevelopPullRequest.yml --limit 1 --json databaseId -q '.[0].databaseId')
gh pr list --head feature/<段階>/<作業名>/master --base develop --state open --json number,url,isDraft
```

## 5. develop 向け Draft PR の本文を確かめる

ワークフロー（処理本体は `.github/scripts/BuildDevelopPullRequest.js`）が、`master` に取り込まれていて develop にまだ入っていない PR の本文から、テンプレ構成の本文を組み立てる。

- 各節は元 PR の同じ節を写す。元 PR が複数あれば、PR ごとの小見出しを付けて並べる。
- 必須チェックは、全ての元 PR でチェック済みの項目だけ `[x]` になる。未チェックがあれば「未確認・残論点」に自動で書かれる。
- 元 PR の「クローズするIssue」節の `#N`（と、本文中の `Closes #N` など）は、実在する Issue であることを確かめたうえで `Closes #N` にする。PR 番号や存在しない番号は除外し、理由を書く。
- タイトルは元 PR のタイトル（複数なら「〜 ほかN件」）。
- 同じ `master` へ後から PR がマージされたり、マージ済みの元 PR の本文を直したりすると、本文を作り直す。

したがって、通常は書き直さない。内容を確かめ、足りないところがあれば**元の agent→master PR の本文を直す**（`edited` で作り直される）。

develop 向け PR の本文を直接書き直したいときは、先頭の `<!-- auto-develop-pr ... -->` の行を消す。消すと以降は自動で作り直さず、新しく見つかった Issue の `Closes #N` を末尾に追記するだけになる。タイトルを手で変えた場合は、目印が残っていてもタイトルは上書きしない。

Draft のまま残す。Ready for review にするか、develop へマージするかはユーザーが決める。

## 6. 報告する

ユーザーには次の内容を返す。
- 作業ブランチ名、agent→master の PR、develop 向け Draft PR（どちらも `owner/repo#N` 形式のリンクで）
- 確認済みの内容と未確認の内容（PR 本文と同じ粒度で）
- 例外手順を使ったときは、その理由
