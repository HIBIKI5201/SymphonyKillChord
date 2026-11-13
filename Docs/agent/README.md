# Docs/agent

自律AIエージェントが生成するファイル（解析記録・監査結果・調査レポート・引継ぎ資料）の置き場。
ここは git の追跡対象で、生成物は下記の構成に沿って置き、コミットする。

## 必要な資料だけを読む

| 作業 | 最初に読む | 必要になったときに追加 |
| --- | --- | --- |
| C#実装・差分レビュー | `Assets/Scripts/DesignPhilosophy.md`、`CodeGuidelines.md` | `DesignClassRoles.md` の対象レイヤー、対象型・呼び出し元、対象機能の仕様・用語。`DesignExamples.md` は実装例が必要なときだけ |
| 全体アーキテクチャ監査 | 上記と `skc-architecture-audit` | モジュール単位でコードを読む。調査済み・未調査を区別する |
| feature-flow | `.agents/skills/skc-feature-flow/SKILL.md` | 作業開始・PR・復旧の各段階で指定されたreferenceだけ |
| Unity操作・QA | `.agents/skills/` の該当スキル | JSON観測を先に使い、画像・ログ・階層は必要な範囲だけ取得する |
| 資料・CI・運用の修正 | 対象ファイルとその参照元 | C#・Unity挙動を変更しなければUnity起動・全規約・全仕様読込は不要 |
| 調査・引継ぎ | このREADMEの配置規則 | 対象テーマの既存レポートだけ。過去の全レポートを読まない |

- C#の例: 敵の攻撃処理を直すなら、`rg -n` で対象の型と呼び出し元を特定し、そのファイルの層（Domain・Application・Adaptor など）に当たる `DesignClassRoles.md` の節と、`spec_search` で絞った敵・攻撃の仕様の節だけを読む。ほかの機能の仕様や過去レポートは読まない。
- 探索はファイル名 → 見出し・一致行 → 該当節の順に進める。`Docs/NotionSpecifications` はまず MCP サーバー `spec` の `spec_search`（使えないときは `python scripts/notion/spec_query.py search <語>`）でページを絞り、`spec_get`（`get <ID・タイトル>`）で仕様の節だけを読む（経緯・却下案は `rationale`、実装状態は `status`）。窓口を使えないときはルートの目録 `_catalog.md`（1ページ1行。同期CIが生成）を `rg` で引き、目録がない版では機能に対応するサブフォルダから絞る。大量一致は `rg -l`、必要な一致行は `rg -n -m` を使う（`-m` はファイルごとの上限）。
- スキル・設定が読む決まったNotionページは、パスではなくpage idで書く（#1955）。一覧は参照表 `SinfoniaOperator/notion-cache-map.json`（page id・呼び名・`readers`）。読む手順: ① `git submodule update --init --remote --depth 1 -- Docs/NotionSpecifications` で写しを最新にする（セッション開始時に済んでいれば省く） ② 作業に要るページを参照表か `search` で選ぶ ③ `python scripts/notion/spec_query.py get <page id>`（データベースは `path <id>` でパスを引いて読む） ④ 読んだページと `git -C Docs/NotionSpecifications log -1 --format=%cI` の日時を報告に書く。pullできなければ手元の写しで続けてその旨を伝え、写しも無ければ止めて仕様書リポジトリの閲覧権限を頼む。ページを消す・統合する前に参照表の `readers` を確かめる。運用の正本はNotion「Notion プロジェクト ドキュメントの運用」（`3e37c2c6-cc02-815b-9dac-f6b834c1e56b`）と「AIエージェント向け参照先一覧」（`3e37c2c6-cc02-812f-94e0-dbf07db926f8`）である。
- 同じセッションで読んだ同じ版の資料は再読しない。編集・ブランチ変更・同期があれば対象資料を再確認する。
- ログや階層は件数・期間・ルートを絞る。出力が切れたら上限を増やす前に探索範囲を狭める。
- 外部サービスの読取・更新は依頼に必要なときだけ。報告用の巨大な生ログはコミットしない。
- 引継ぎには目的、現在のブランチ・PR、変更したパス、検証結果、未解決点、次の操作だけを残す。会話・全ログ・全文仕様の転載はしない。

## ハーネスの保守

- Codex共通スキルは `.agents/skills/`、Codex専用は `.codex/skills/`。同名を両方に置かない。Claudeの独自入口は `.claude/skills/` に置き、共有可能な手順は共通本体へリンクする。
- 現在のセッションで削除前のスキル一覧が残っている場合は `.agents/skills/<name>/SKILL.md` を参照する。次のセッションで一覧の重複解消を確認する。
- 正本の規則をスキル本文へ転載しない。短い入口に対象・必須条件・条件付きの参照先を置き、モード固有の詳細をreferencesへ分ける。
- `python scripts/harness/check_harness.py` で入口の文字数上限、リンク、スキル名の再重複を確認する。CIでも同じチェックを実行する。数字はトークン実測ではなくUTF-8本文の文字数である。

## ディレクトリ構成

```
Docs/agent/
├── README.md                              … このファイル（資料の選び方・構成・命名規則）
├── index.md                               … 収録済みの成果物と旧置き場からの移動の一覧
├── analysis/                              … 解析ツールが一括生成する成果物
│   └── <tool>/
│       └── <YYYY-MM-DD>_<target>/         … 1回の解析実行 = 1フォルダ
│           ├── report/                    … 閲覧用（HTML など）
│           └── spec/                      … 機械可読の中間成果物（md / json など）
├── reports/                               … 単発の調査・監査レポート
│   └── <YYYY-MM-DD>_<topic>.md
└── handoffs/                              … 作業の引継ぎ資料・作業計画
    └── <YYYY-MM-DD>_<topic>.md
```

| フォルダ | 置くもの | 置かないもの |
| --- | --- | --- |
| `analysis/` | 解析ツール（Omnipotens など）の1回分の出力一式 | 手書きのメモ、単発の調査結果 |
| `reports/` | 1テーマの調査・監査・比較の結果（1ファイルで完結するもの） | 複数ファイルにまたがるツール出力 |
| `handoffs/` | 次の担当（人・エージェント）に渡す引継ぎ資料、作業計画 | 確定した仕様（仕様は `Docs/NotionSpecifications` / Notion が正本） |

## 命名規則

- 日付は作成日を `YYYY-MM-DD` で先頭に付ける（一覧で時系列に並ぶように）。
- `<tool>` `<target>` `<topic>` は英小文字のケバブケース（例: `omnipotens`, `symphony-kill-chord`, `save-data-audit`）。
- 同じテーマを更新する場合は新しい日付で別フォルダ・別ファイルを作り、古いものは消さない（差分を追えるようにする）。

## 置かないもの

- 秘密情報（認証情報、Drive の ID やリンク、メンバーのメールアドレスなど）。公開リポジトリのため。
- 再生成できる大きな生ログ・キャッシュ。必要な部分だけをレポートに要約して残す。
- Unity プロジェクトから参照されるアセット（`Assets/` 配下に置く）。

収録済みの成果物の一覧は [index.md](index.md)。新しい成果物を置いたら、そちらに1行足す。
