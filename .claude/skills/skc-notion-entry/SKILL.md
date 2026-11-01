---
name: skc-notion-entry
description: "Add a row to the Notion タスクリスト・議事録・仕様検討フォーラム from a small JSON file with scripts/notion/apply_entry.py. Use whenever the user asks to add a task, record meeting minutes (議事録), or post a proposal to the 仕様検討フォーラム in Notion. Do NOT use for editing spec pages (use skc-notion-spec-write)."
---

# Notion への定型投稿（タスク・議事録・フォーラム）

本文の Markdown や `--set` は自分で組み立てない。**JSON だけを書いて** `scripts/notion/apply_entry.py` に渡す。
スクリプトがテンプレート（`scripts/notion/templates/*.json`）で検証し、本文とプロパティを組み立てて
`NotionMarkdownWriter create` を呼ぶ。書き込みの規則は [skc-notion-spec-write](../skc-notion-spec-write/SKILL.md) と同じ。

## 絶対に守ること

- **`--confirm` は、その会話でユーザーが明示的に承認してから付ける。** 先に dry-run の結果を見せる。
- 書いたら、skc-notion-spec-write の「報告する（必須）」の表で報告する。
- テンプレートにないプロパティ・節を足したいときは、JSON で無理に渡さず、テンプレートを直す（レビュー対象）。

## 手順

1. 作業用の JSON を書く（1件ならオブジェクト、まとめて登録するなら配列）。置き場所は Git の外（一時フォルダ）。
2. 変換だけ確かめる: `python scripts/notion/apply_entry.py <JSON> --offline`
3. dry-run（relation を解決し、Writer が作成先と本文を表示する）: `python scripts/notion/apply_entry.py <JSON>`
4. ユーザーの承認後に作成: `python scripts/notion/apply_entry.py <JSON> --confirm`

どれか1件でも誤りがあれば、全件を送信前に止める。`--confirm` の途中で失敗したら、何件目まで作成済みかが表示される。
選択肢がNotion側で変わった疑いがあるときは `python scripts/notion/apply_entry.py --check-templates` で比べる。

## JSON の形

共通: `type`（`task`・`minutes`・`forum`）、`title`（ページ名）、`properties`、`sections`。
値の配列はカンマ区切りの文字列でもよい。節の値は文字列か、行の配列。空の節は空行になる。

```json
[
  {
    "type": "task",
    "title": "ロックオン切り替えの実装",
    "properties": { "ステータス": "未着手", "担当者": "郷音ヒビキ", "タグ": ["プログラマー"], "親タスク": "親タスクの名前" },
    "sections": { "担当": "郷音さん", "内容": "…", "バックログ": "- [ ] …" }
  },
  {
    "type": "minutes",
    "title": "10/30 定例",
    "properties": { "議題": "進捗の共有" },
    "sections": { "決定": "- …", "ログ": "- …" }
  },
  {
    "type": "forum",
    "title": "ロックオン時のカメラ補正",
    "properties": { "担当者": ["八幡"], "優先度": 5 },
    "sections": { "経緯": "- …", "初期案の要約": "補正を弱める", "初期案": "# 仕様意図\n…" }
  }
]
```

| 種類 | プロパティ（既定値） | 節（必須は★） |
| --- | --- | --- |
| task | ステータス（未着手）・日付・担当者（people、Notionの表示名）・タグ・スプリント（現在）・親タスク・次のタスクにより保留中： | 担当・内容★・バックログ |
| minutes | 日付★（今日）・議題★ | 決定（なし）・ログ★ |
| forum | ステータス（作業中）・担当者（選択肢の名前）・優先度 | 記述指針・仕様概要・仕様意図・仕様詳細・経緯★・初期案の要約★（1行）・初期案★ |

- relation は、ページ名（完全一致で1件だけ見つかるもの）・URL・ID で書ける。スプリントは `現在`・`次` も使える。
- フォーラムの「結論」側の節（記述指針〜仕様詳細）は、採用後に書く。新規投稿では空でよい。
