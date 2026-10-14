# ブランチ作成と作業

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
