---
name: skc-feature-flow
description: "Run this repo's full feature-branch flow end to end: create `feature/<stage>/<name>/agent`, push it empty, implement and commit, open the agent→master PR with the project PR template, self-merge it, then rewrite the auto-generated master→develop draft PR body (or create it manually when the automation cannot fire). Use when the user says things like 'ブランチ名○○で実装して', 'masterにセルフマージして', 'DraftPRのメッセージを直して', or asks to take a change from branch creation through to the develop PR. Implementation itself is delegated to the other skc-/uloop- skills; this skill owns only the git/PR choreography."
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

## 0. 入力を決める

- **ブランチ名**: `feature/<段階>/<作業名>/agent`。ユーザーが「ブランチ名○○で」と言ったら `○○` を作業名に使う。すでに完全な形（`feature/.../agent`）なら、そのまま使う。
  - 段階は `demo`・`release`・`tools` など。指定がなければ作業内容から選び、選んだ段階を報告する。
  - `agent` は末尾（個人名の位置）に置く。`agent/○○` の形は使わない。
- **Issue 番号**: 関連 Issue があれば控えておく。`Closes #N` は **develop 向けの PR（2段目）にだけ**書く。`master` 向けの PR（1段目）に書いてもクローズされない。
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
- 「確認済みの内容」には、確認の方法（uloop-compile / dotnet build / PlayMode / テスト名）を項目ごとに書く。
- 本文の末尾に、会話で指定された PR 用の署名行を付ける。
- この PR には `Closes #N` を書かない。

```bash
gh pr create --base feature/<段階>/<作業名>/master --head feature/<段階>/<作業名>/agent \
  --title "<作業内容の要約>" --body-file <本文ファイル>
```

本文ファイルは scratchpad に書く。`ValidateFeaturePRTarget.yml` が、マージ先が同じ階層の `master` かどうかを検査する（commit status `feature-pr-target`）。

## 4. master へセルフマージする

```bash
gh pr checks <PR番号>          # feature-pr-target が成功していること
gh pr merge <PR番号> --merge   # マージコミットで取り込む（squash しない）
```

マージすると `AutoCreateDevelopPullRequest.yml` が `master` → `develop` の Draft PR を自動作成する。完了を待つ:

```bash
gh run watch $(gh run list --workflow AutoCreateDevelopPullRequest.yml --limit 1 --json databaseId -q '.[0].databaseId')
gh pr list --head feature/<段階>/<作業名>/master --base develop --state open --json number,url,isDraft
```

## 5. develop 向け Draft PR の本文を直す

自動生成された本文は定型文（「自動生成Draft PRです」「動作確認は未実施です」）なので、そのままにしない。手順 3 の本文をもとに、次の点を変えて書き直す。

- 概要・原因・対処・確認済みの内容・未確認・残論点を、実際の変更内容で埋める。
- 関連 Issue があれば、概要の直後などに `Closes #N` を書く（ここで初めて書く）。
- タイトルが `feature/.../master → develop` のままなら、変更内容がわかるタイトルに変える。
- Draft のまま残す。Ready for review にするか、develop へマージするかはユーザーが決める。

```bash
gh pr edit <develop向けPR番号> --title "<タイトル>" --body-file <本文ファイル>
```

### 例外: 自動の Draft PR が作られないとき

- 作業ブランチと `master` が同じコミットだと（先に編集してから push した場合など）、agent→master の PR は「No commits between」で作れない。この場合は手順 3・4 を省き、`master` を作業ブランチに合わせてから develop 向け Draft PR を**手動で**作る:
  ```bash
  git push origin feature/<段階>/<作業名>/agent:feature/<段階>/<作業名>/master
  gh pr create --draft --base develop --head feature/<段階>/<作業名>/master --title "<タイトル>" --body-file <本文ファイル>
  ```
- ワークフローが 403（Actions に PR 作成の権限がない）で警告だけ出して終わった場合も、同じく手動で作る。
- すでに開いている develop 向け PR があるときは、ワークフローは何もしない。その既存 PR の本文を更新する。

## 6. 報告する

ユーザーには次の内容を返す。
- 作業ブランチ名、agent→master の PR、develop 向け Draft PR（どちらも `owner/repo#N` 形式のリンクで）
- 確認済みの内容と未確認の内容（PR 本文と同じ粒度で）
- 例外手順を使ったときは、その理由
