---
name: skc-feature-flow
description: "Run this repo's full feature-branch flow end to end: create `feature/<stage>/<name>/agent`, push it empty, implement and commit, open the agent→master PR with the project PR template, self-merge it, then check the auto-generated master→develop draft PR (its body is built from the agent→master PR; fix the source PR, or trigger the workflow manually when the automation cannot fire). Use when the user says things like 'ブランチ名○○で実装して', 'masterにセルフマージして', 'DraftPRのメッセージを直して', or asks to take a change from branch creation through to the develop PR. Implementation itself is delegated to the other skc-/uloop- skills; this skill owns only the git/PR choreography."
---

# skc-feature-flow

作業ブランチ作成 → 実装 → `master` へのセルフマージ → `develop` 向け Draft PR の本文修正、までを一続きで行う。
運用ルールの正本は [AGENTS.md](../../../AGENTS.md) の「自律AIエージェントのブランチ・PR運用」。ここにはその手順化と落とし穴だけを書く。ルールが食い違ったら AGENTS.md を優先する。

## 他スキルとの分担

このスキルは Git と PR の段取りだけを受け持つ。中身の作業は既存スキルに任せる。

| 段階 | 使うスキル |
|------|-----------|
| 実装を Codex に任せる指示があるとき | `skc-codex-implement` |
| C# を編集したあとのコンパイル確認 | `uloop-compile`（Unity 未起動なら dotnet build で代替し、PR に「Unity 上では未確認」と書く） |
| C# を編集したあとの規約レビュー | `skc-code-guideline-check` |
| 動作確認 | `uloop-run-tests` / `uloop-control-play-mode` など |
| 仕様書への反映が要るとき | `skc-notion-spec-write`（別作業として扱い、ユーザーに確認してから） |
| 作業中に見つけた、今回は直さない問題・課題 | `skc-issue-report`（Issue にして、PR の「未確認・残論点」から `#N` で参照する） |

## 0. 入力を決める

- **ブランチ名**: `feature/<段階>/<作業名>/agent`。ユーザーが「ブランチ名○○で」と言ったら `○○` を作業名に使う。すでに完全な形（`feature/.../agent`）なら、そのまま使う。
  - 段階は `demo`・`release`・`tools` など。指定がなければ作業内容から選び、選んだ段階を報告する。
  - `agent` は末尾（個人名の位置）に置く。`agent/○○` の形は使わない。
- **Issue 番号**: 関連 Issue があれば控えておく。1段目（`master` 向け）の PR の「クローズするIssue」節に `- #N` で書くと、2段目（develop 向け）の Draft PR に `Closes #N` として自動で引き継がれる。1段目の PR だけではクローズされない（develop へのマージで初めてクローズされる）。
- **マージしてよい範囲**: `feature/**/master` へのセルフマージは、自分の判断で行ってよい。`develop` へのマージはしない（ユーザーの明示的な指示があるときだけ）。

## 1. ブランチを作り、空のまま push する

```bash
git fetch origin
git checkout -b feature/<段階>/<作業名>/agent origin/develop
git push -u origin feature/<段階>/<作業名>/agent
```

編集の**前に** push する。`AutoCreateMasterBranch.yml` が、push 時点の内容で `feature/<段階>/<作業名>/master` を自動作成する。先に編集してから push すると、作業ブランチと `master` が同じコミットになってしまい、手順 5 の例外ケースに入る。

作成されたかどうかの確認（ワークフローは約10秒待ってから作成する）:

```bash
gh run watch $(gh run list --workflow AutoCreateMasterBranch.yml --branch feature/<段階>/<作業名>/agent --limit 1 --json databaseId -q '.[0].databaseId')
git ls-remote --heads origin feature/<段階>/<作業名>/master
```

## 2. 実装してコミットする

- 規約: `Assets/Scripts/DesignPhilosophy.md`、`Assets/Scripts/CodeGuidelines.md`。
- 上の分担表に沿って、ほかのスキルを呼ぶ。
- コミットは作業ブランチ上でだけ行う。`master` を直接編集しない。
- stage するのは作業で触ったパスだけにする（`git add -A` をリポジトリ全体に使わない）。`Assets/AssetStoreTools` などサブモジュールの dirty 状態を巻き込まないため。
- Python でファイルを一括書き換えるときは、`rb`/`wb` か `newline=""` を使う（CRLF が壊れて、ファイル全体が差分になるのを防ぐ）。
- コミットメッセージの末尾には、会話で指定された Co-Authored-By 行を付ける。

```bash
git push
```

## 3. agent → master の PR を作る

本文は必ず [.github/PULL_REQUEST_TEMPLATE.md](../../../.github/PULL_REQUEST_TEMPLATE.md) の構成にする（Summary / Test plan のような汎用の形式にはしない）。

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

### 例外: 自動の Draft PR が作られないとき

- 作業ブランチと `master` が同じコミットだと（先に編集してから push した場合など）、agent→master の PR は「No commits between」で作れない。この場合は手順 3・4 を省き、`master` を作業ブランチに合わせてから、ワークフローを手動で実行する:
  ```bash
  git push origin feature/<段階>/<作業名>/agent:feature/<段階>/<作業名>/master
  gh workflow run AutoCreateDevelopPullRequest.yml --ref develop -f master_branch=feature/<段階>/<作業名>/master
  ```
  元になる PR が無いので、本文はコミット一覧から作られる。作成後に目印の行を消し、手順 3 と同じ粒度で本文を書き直す（`Closes #N` もここで書く）。
- ワークフローが 403（Actions に PR 作成の権限がない）で警告だけ出して終わった場合は、`gh pr create --draft --base develop --head <master> --title "<タイトル>" --body-file <本文ファイル>` で手動で作る。

## 6. 報告する

ユーザーには次の内容を返す。
- 作業ブランチ名、agent→master の PR、develop 向け Draft PR（どちらも `owner/repo#N` 形式のリンクで）
- 確認済みの内容と未確認の内容（PR 本文と同じ粒度で）
- 例外手順を使ったときは、その理由
