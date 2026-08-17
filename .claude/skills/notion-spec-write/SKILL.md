---
name: notion-spec-write
description: "Write to the Notion game specification from Claude — create a new spec page or update an existing one — using the NotionMarkdownWriter CLI (SinfoniaOperator/NotionMarkdownWriter.exe). Use whenever the user asks to add, edit, update, fix, or document something in the Notion spec (仕様書), reflect an implementation into the spec, write up a system's documentation to Notion, or act on the findings of a spec/implementation diff report. Covers the pull → edit → dry-run → confirm → push loop, the write allowlist, and conflict handling. Do NOT use for read-only spec questions — read Docs/NotionSpecifications directly for those."
---

# Notion 仕様書への書き込み

このリポジトリはNotionの仕様書を`Docs/NotionSpecifications/`へミラーしているが、そちらは**読み取り専用のコピー**。
Notion本体を更新するには`SinfoniaOperator/NotionMarkdownWriter.exe`を使う。ツールの詳細は
[SinfoniaOperator/NotionMarkdownWriter/README.md](../../../SinfoniaOperator/NotionMarkdownWriter/README.md)。

`notion-spec-diff-check`（差分の発見）の続きとして使うことが多い。差分レポートの指摘を仕様書側へ反映するのがこのスキル。

## 絶対に守ること

- **`--confirm`は、その会話でユーザーが明示的に承認してから付ける。** Notionは共有ワークスペースであり、
  書き込みは他人から見える不可逆な変更。dry-runの結果を提示し、承認を得てから送信する。
- **`Docs/NotionSpecifications/`配下の`.md`を編集しても、Notionには反映されない。** ミラーを直接書き換えて
  「更新しました」と報告しないこと。
- **エクスポート済みの`.md`を編集の材料にしない。** リンク変換と装飾マーカー除去で原文と食い違うため、
  必ず`pull`で取得した原文を編集する。
- Notion APIの`replace_content`と既存ページの削除はツールが実装していない。ページの削除を要求されたら
  Notion上での手作業を案内する。
- 本文を全面的に書き換える場合は`push --whole`を使う。`update_content`の枠内で、本文全体を1件の置換として
  送る。部分差分は組み立てないため、一意な`old_str`を作れない編集でも通り、適用の成否がページ単位に揃う。
  逆に、部分更新（`--whole`なし）は置換が一意にならないと送信前に中断する。

## 書き込める範囲

`sinfonia-operator.env.json`の`NOTION_WRITE_ALLOWED_ROOTS`に列挙されたページの**子孫だけ**が対象。
許可ページ自身の本文は編集できず、作成先の親としてのみ指定できる。

範囲外のページを頼まれた場合は、勝手に許可リストへ追記せず、ユーザーへ確認する
（この設定はGit共有で、追加はリポジトリの変更としてレビューされる前提のもの）。

## 既存ページを更新する

```bash
./SinfoniaOperator/NotionMarkdownWriter.exe pull "<Docs/NotionSpecifications配下の.mdパス|URL|ページID>"
```

1. リポジトリルートから実行する。出力された作業ファイルのパスを控える。
2. 作業ファイルをEditツールで普通に編集する。`old_str`/`new_str`を自分で組み立てる必要はない
   （一意になるまでの文脈付与はツールが行う）。書き換えは必要な箇所だけにとどめ、
   本文全体を書き直さない — 差分が大きいほど事故も確認コストも増える。
3. 送信せずに差分を確認する。

   ```bash
   ./SinfoniaOperator/NotionMarkdownWriter.exe push "<作業ファイル>"
   ```

4. 出力された差分をユーザーへ提示し、承認を得る。
5. 承認後に送信する。

   ```bash
   ./SinfoniaOperator/NotionMarkdownWriter.exe push "<作業ファイル>" --confirm
   ```

送信後、ツールは本文を取得し直して送信内容どおりか確認する。「完全には一致しません」と出た場合は
Notion側の書式正規化が入っている。作業ファイルが最新本文で更新されているので、意図した内容になっているか読んで判断し、
必要なら追加の編集をして再度pushする。

## 新しいページを作る

本文のMarkdownファイルを用意し（先頭行は`# ページ名`。この見出しがタイトルになり本文からは消える）、
親ページを指定して作成する。

```bash
./SinfoniaOperator/NotionMarkdownWriter.exe create "<本文.md>" --parent "<親ページの.mdパス|URL|ID>"
```

同じくdry-run → 承認 → `--confirm`の順。本文は作業用ディレクトリかスクラッチパッドに置き、
`Docs/NotionSpecifications/`の中には作らない（エクスポーターの管理対象と混ざる）。

## つまずいたとき

| 症状 | 意味と対応 |
| --- | --- |
| `pull以降にNotion側が更新されています` | 他の人が編集した。`pull`し直して編集をやり直す。作業ファイルを上書きするので、必要なら編集内容を退避してから。 |
| `一意に特定できる置換範囲を作れませんでした` | 同一文面の繰り返し。編集箇所を分けて複数回pushするか、周辺の文面ごと書き換える。 |
| `書き込み許可ページの配下にありません` | 範囲外。許可リストの追加はユーザーの判断を仰ぐ。 |
| `ページが大きすぎて…分割しました` | APIの上限。そのページはNotion上で直接編集してもらう。 |
| 403 / `insert_content`などの権限エラー | インテグレーションに書き込み権限が無い。Notion側の設定変更が必要なので、ユーザーに依頼する。 |

## 反映後

`push`や`create`は`Docs/NotionSpecifications/`のミラーを更新しない。
ミラーを合わせるには`NotionMarkdownExporter`の再実行が必要だが、全ページ再取得の重い処理なので、
1ページ書き換えるたびに回さない。差分チェックなど次の作業でミラーの鮮度が問題になるときだけ、
`notion-spec-diff-check`の手順に従って実行する。
