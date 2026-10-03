---
name: skc-issue-report
description: "Turn problems and open challenges that surfaced while running an skc-/uloop- skill into GitHub Issues: collect the evidence, check for duplicates, draft the body in this repo's Issue format (`[TODO] ...` title, 概要 / 背景・根拠 / やること / 完了条件 / 関連), and create it with `gh issue create`. Use when the user says things like '出てきた問題をIssueにして', '課題をIssue化して', '残論点をIssueに起こして', 'これ後でやるからIssue切って', or when a skill run (audit, QA, spec diff check, feature flow, Codex delegation, importers, autobuilder) ends with findings or blockers that are out of scope for the current change. Does not fix the problem itself; it only records it."
---

# skc-issue-report

skc 系スキルの実行中に見つかった問題・課題のうち、**今の変更では直さないもの**を GitHub Issue として残す。
直すことはしない。記録して、あとから拾える形にするのが仕事。

## 使う場面

| 元のスキル | Issue にする対象の例 |
|-----------|--------------------|
| `skc-architecture-audit` / `skc-code-guideline-check` | 指摘のうち今回の差分の外にあるもの |
| `skc-notion-spec-diff-check` | 仕様と実装のずれ（「仕様のみ」「実装のみ」「どちらも不足」） |
| `skc-ai-debug-qa` | QA で再現した不具合、観測できなかった項目 |
| `skc-feature-flow` | PR の「未確認・残論点」に書いた項目 |
| `skc-codex-implement` | Codex が止まった・誤った実装を出した原因、運用上の課題 |
| `skc-sinfonia-importers` / `skc-unity-autobuilder` / `skc-notion-spec-write` | ツールの失敗、手順書と実際の食い違い |
| スキル自体 | SKILL.md の手順が古い・足りない・間違っている |

次のものは Issue にしない。
- 今の変更の中で直せる小さなもの（その場で直す）。
- 確証のない憶測。再現条件か根拠（`file:line`、ログ、コマンド出力）が書けないものは、書けるところまで調べる。
- すでに Issue がある問題（手順 2 で確かめる）。

## 1. 候補を集める

会話・レポート・ログから、Issue にする候補を一覧にして、それぞれに次を控える。

- 何が起きたか（1 文）
- 根拠（`file:line`、ログの該当行、実行したコマンドと結果、レポートのパス）
- 影響（誰が・何に困るか）
- 直し方の案（分かる範囲で）
- 種別: 不具合 / 改善 / ドキュメント

関連する問題は 1 つにまとめ、無関係なものは分ける。1 Issue 1 テーマにする。大きい解析レポートの指摘が多数あるときは、観点ごとに親 Issue を 1 つ作り、中をチェックリストにする（例: #2359〜#2363）。

## 2. 重複を確かめる

作る前に必ず検索する。

```bash
gh issue list --state all --search "<キーワード>" --limit 20 --json number,title,state
```

- 同じ問題が**開いている**: 新規は作らず、コメントで根拠を足す（`gh issue comment <番号> --body-file <ファイル>`）。
- 同じ問題が**閉じている**: 再発として、新しい Issue の「関連」に番号を書く。
- 近いが別物: 新規に作り、「関連」で相互に結ぶ。

## 3. 本文を書く

本文はファイルに書き、`--body-file` で渡す。置き場所は scratchpad。リポジトリ内に置かない。
公開リポジトリなので、認証情報・Drive の ID やリンク・メンバーのメールアドレス・個人のローカルパスは書かない。

タイトルは `[TODO] <やることを一文で>`（既存の Issue と同じ形）。

```markdown
## 概要
<何を、どうしたいか。2〜3 文>

## 背景・根拠
- <どのスキルの、どの作業で見つかったか>
- <根拠: `file:line` / ログ / コマンド結果 / レポートへのリンク>
- <影響>

## 進め方の案
- <分かる範囲で。決めきれない場合は選択肢と、それぞれの利点・欠点>

## やること
- [ ] <1 つ 1 つが完了を判定できる粒度で>

## 完了条件
- <確かめ方まで書く。例: 「○○を実行して△△が出なくなる」>

## 関連
- <関連 Issue / PR は `#N`。リポジトリ内のファイルやレポートは相対パスではなく GitHub のリンクで>

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

書き方の約束:
- 事実と推測を分ける。確かめていないことは「未確認」と書く。
- 「やること」は着手できる粒度にする。「改善する」だけの行は書かない。
- 解析レポートが `Docs/agent/` にあるときは、その GitHub 上のリンクを「背景・根拠」に入れる。
- 本文中の `#N` は参照扱いになる。この Issue が別の PR でクローズされる前提のときは、PR 側の「クローズするIssue」節に書く（[skc-feature-flow](../skc-feature-flow/SKILL.md) 参照）。

## 4. 作る

ラベルは種別に合わせて 1 つ付ける。既存のラベルだけを使う（新しいラベルは作らない）。

| 種別 | ラベル |
|------|--------|
| 不具合 | `bug` |
| 改善・機能・運用の課題 | `enhancement` |
| ドキュメント・仕様書 | `documentation` |
| コード監査の指摘 | `runtime-audit` |

```bash
gh issue create --title "[TODO] <タイトル>" --label enhancement --body-file <本文ファイル>
```

- 担当者（assignee）は付けない。
- 複数作るときは、1 件ずつ作って番号を控え、あとから「関連」を相互に結ぶ（`gh issue edit <番号> --body-file ...`）。

### 確認の取り方

- ユーザーが「Issue にして」と依頼したとき: 候補の一覧（タイトルと 1 行の要約）を見せたうえで、そのまま作成してよい。件数が多い（5 件以上）場合は、先に一覧だけ見せて確認する。
- スキルの実行中に自分で気づいて提案するとき: 作る前に一覧を見せて確認を取る。勝手に作らない。

## 5. 報告する

作った Issue と、作らなかった候補（重複・対象外・今回直した）を分けて返す。

- 作成した Issue: `owner/repo#N` 形式のリンクとタイトル
- コメントを足した既存 Issue
- 作らなかったものと、その理由

作った Issue を元の PR の「未確認・残論点」から参照する場合は、PR 本文を直して `#N` を書く。
