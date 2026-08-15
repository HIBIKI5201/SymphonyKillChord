---
name: codex-implement
description: "Delegate an implementation task to Codex CLI with rate-limit protection, then review the result. Use when the user asks to implement a feature via Codex, or wants to offload coding to Codex/an external model while keeping design and review in Claude. Falls back to implementing directly when Codex quota is low."
---

# codex-implement

設計・計画は Claude が行い、**実装だけを Codex CLI に委任**する。
Codex の残枠が足りない場合は委任をスキップし、Claude 自身が実装まで完遂する。

ラッパー: [scripts/codex_runner.py](../../../scripts/codex_runner.py)

## 重要: このリポジトリでの実行コマンド

Windows 環境のため `python3` は Microsoft Store のスタブ（exit 49）で **動かない**。
必ず `python` を使うこと。

```bash
python scripts/codex_runner.py "<プロンプト>" "<出力先パス>"
```

## ワークフロー

### 1. 設計・プロンプト策定

Codex を呼ぶ前に、Claude が責任を持って設計する。

- 対象コードベースを読み、既存のアーキテクチャ・命名・アセンブリ定義を把握する
- リポジトリ規約を必ず確認する（[AGENTS.md](../../../AGENTS.md) 参照）
  - `Assets/Scripts/DesignPhilosophy.md`
  - `Assets/Scripts/CodeGuidelines.md`
- 変更対象ファイル・公開 API・依存関係を確定する

そのうえで **Codex 用プロンプト**を書く。Codex は会話履歴を持たないので、
プロンプト単体で完結させること。

プロンプトに必ず含める要素:

| 要素 | 内容 |
|------|------|
| 目的 | 何を実装するか（1〜2 行） |
| 配置 | namespace / アセンブリ / クラス名 |
| API 仕様 | メソッドシグネチャ、プロパティ、イベント |
| 参照すべき既存コード | 具体的なファイルパス（模倣させる規約の実例） |
| 制約 | 使用禁止 API、Unity バージョン、パフォーマンス要件 |
| 非対象 | 触ってはいけないファイル |

プロンプトが長い場合は一時ファイルに書き、`--prompt-file` を使う。

### 2. Codex ラッパーの呼び出し

```bash
python scripts/codex_runner.py "<プロンプト>" "<出力先パス>"
```

長文プロンプトの場合:

```bash
python scripts/codex_runner.py --prompt-file <プロンプトファイル> "<出力先パス>"
```

事前に残量だけ確認したい場合（Codex は消費しない）:

```bash
python scripts/codex_runner.py --check-only --json
```

主なオプション:

| オプション | 既定 | 説明 |
|-----------|------|------|
| `--threshold <float>` | `10.0` | 残量しきい値パーセント |
| `--check-only` | – | 残量チェックのみ |
| `--json` | – | 残量レポートを JSON で stdout に出力 |
| `--fail-closed` | – | 残量判定不能時に実行を許可しない |
| `--model <name>` | config.toml | 使用モデル |
| `--sandbox <mode>` | `workspace-write` | Codex のサンドボックス |
| `--cd <dir>` | カレント | 作業ルート |
| `--timeout <sec>` | `1800` | タイムアウト |

### 3. 条件分岐・フォールバックハンドリング

終了コードで**必ず**分岐する。出力テキストではなく exit code を見ること。

#### Exit Code 0 — Codex 生成成功

1. 生成されたファイルを **必ず Read して中身を確認する**（無検証で受け入れない）
2. 型チェック / コンパイル確認
   - Unity C# の場合は `uloop-compile` skill でコンパイルし、
     `uloop-get-logs` skill でエラー・警告を確認する
3. レビュー観点
   - リポジトリのコーディング規約に沿っているか
   - 既存の設計パターン・命名と整合しているか
   - 対象外ファイルが変更されていないか（`git status` で確認）
   - null 安全性・境界条件・例外処理
4. 問題があれば Claude が直接修正する（再委任でループしない）
5. ユーザーに「Codex が生成 → Claude がレビュー済み」と結果を報告する

#### Exit Code 2 — 残量不足（レートリミット保護）

1. ユーザーに次を通知する:
   > **Codex の残枠不足のため、Claude 自身で実装します。**
2. 残量の実数を添える（`--check-only --json` の `remaining_percent` / `resets_at`）
3. **Claude 自身が手順 1 の設計どおりに実装を完遂する**
   - Codex 用に書いたプロンプトがそのまま実装仕様書になる
   - 通常どおり Edit / Write で実装し、コンパイル・レビューまで行う
4. 実装後、Codex を使わなかった旨を報告に明記する

#### Exit Code 1 — 一般エラー

1. stderr のメッセージを読んで原因を切り分ける
   - `codex CLI が見つかりません` → 環境変数 `CODEX_BIN` を設定（下記参照）
   - タイムアウト → `--timeout` を延ばすか、タスクを分割する
   - 生成物を確認できませんでした → プロンプトの出力先指示が曖昧
2. **同じ条件でのリトライは 1 回まで**。解消しなければ Claude 自身で実装に切り替える

## 判断基準: いつ Codex に委任するか

| 委任する | Claude が自分でやる |
|---------|-------------------|
| 仕様が確定した単一ファイルの新規実装 | 複数ファイルにまたがるリファクタリング |
| ボイラープレートの多いクラス生成 | 既存コードの微修正 |
| 定型的なデータ構造・ユーティリティ | 対話的な調査・デバッグ |
| テストコードの雛形生成 | 設計判断そのものを含むタスク |

Codex はこのリポジトリの文脈を持たないため、**曖昧な指示ほど品質が落ちる**。
プロンプトを書くコストが自分で実装するコストを上回るなら、委任しない。

## 環境メモ

- Codex CLI は **PATH に載っていない**。Codex Desktop 同梱の
  `%LOCALAPPDATA%\OpenAI\Codex\bin\<hash>\codex.exe` を
  `codex_runner.py` が自動解決する（`config.toml` の `CODEX_CLI_PATH` 経由）。
  解決に失敗する場合のみ `CODEX_BIN` に絶対パスを設定する。
- 認証は ChatGPT ログイン済み。再認証が必要な場合のみ `codex login`。
- 残量は `~/.codex/sessions/**/rollout-*.jsonl` の `rate_limits` から読む。
  Codex を一度も実行していないと情報が無く、既定では **実行許可 (fail-open)** になる。
