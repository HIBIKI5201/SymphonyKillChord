# AIエージェント向け資料とハーネスの最適化（2026-10-10）

## 変更と狙い

大量の資料を毎回読む手順を、短い共通入口と条件付き参照へ変更した。既存規則の削除による削減ではなく、必要な時点で必要な節を読む構成とした。

- AGENTS.md: 共通の作業条件・探索・出力先・ブランチ運用の要点だけを入口に残し、元のブランチ規則全文を共通feature-flowのreferences/branch-rules.mdへ保存。
- DesignPhilosophy.md: 共通原則を残し、クラス責務をDesignClassRoles.md、Entity/ValueObjectの実装例をDesignExamples.mdへ分割。責務も正本の一部として明記。既存のUnity GUIDは保持し、新規2文書にmetaを追加。
- feature-flow: .agents/skills/skc-feature-flowへ共通本体を配置。作業開始・PR・復旧を段階別referenceへ分割。Claude側は共通入口へのリンクとし、Codexにも共通配置から利用可能とした。編集前のmaster生成確認、既存PRのstate/head確認、developをDraftで残す条件を入口に明記。
- Codex側の18個のuloopスキル複製（計40ファイル）を除去し、.agents側へ一本化。Claudeのuloop配置は、Claudeの発見経路を維持するため今回保持。
- レビュー系スキル: 原則の再転載を除去し、分割した正本の対象レイヤーを参照する。Codex監査が存在しないClaude専用レビュー名へ誘導する誤りと、報告先をgitignore対象とする古い説明を修正。
- Docs/agent/README.md: 作業別の参照表、探索・出力範囲、同一版の再読回避、短い引継ぎ、共通スキル配置の保守方法を追加。既存の出力ディレクトリ構成は維持。
- scripts/harness/check_harness.py と AgentHarness.yml: UTF-8本文文字数の上限、管理対象文書のファイル・見出しリンク、Codex共通/専用間の同名スキル、基本frontmatterを検査。Python標準ライブラリだけで実行する。

## 実測

UTF-8デコード後の本文文字数。改行はLFに正規化し、同じ方法で変更前後を比較した。トークン数・課金額・応答時間の実測ではない。

| 対象 | 変更前 | 変更後 | 低減 |
| --- | ---: | ---: | ---: |
| AGENTS.md | 1,937 | 1,077 | 44.4% |
| DesignPhilosophy.mdの共通入口 | 5,787 | 1,661 | 71.3% |
| feature-flow入口（旧Claude本文→共通本体） | 5,941 | 938 | 84.2% |
| Claude feature-flow入口 | 5,941 | 265 | 95.5% |
| Claude code-guideline-check本文 | 6,949 | 1,929 | 72.2% |
| Codexプロジェクト内スキルdescription合計（.codex + .agents） | 9,449 | 5,431 | 42.5% |

Claudeのfeature-flowは265文字の入口に加えて共通本体938文字を読む。実作業では段階別referenceも必要となる。C#作業では対象レイヤーの責務と仕様も必要となるため、上記の低減率をタスク全体のトークン削減率と読み替えない。

## 検証

- 管理対象の入口予算・ローカルリンク・見出しリンク・同名スキルチェック: エラー0件。
- unittest: 正常ケース、同名スキル、リンク切れ/外部パス、相対リンクと見出し、存在しない見出し、文字数超過、入口欠落、frontmatter破損の8件が成功。
- 元のDesignPhilosophy.mdの非空行すべて（置換した最上位見出し以外）が分割後の3文書に残ることを照合。元AGENTS.mdのブランチ規則全文がreferenceに保存されていることを照合。
- 除去した40ファイルすべてが共通側と改行正規化後に同一であることを照合。削除パスへの明示的な呼び出し元は対象スクリプト・設定・スキルの検索で見つからなかった。
- C#およびUnityの実行挙動は変更していない。Unityコンパイル・PlayModeは不要として未実行。
- skill-creator付属quick_validate.pyはPyYAMLが環境にないため実行できなかった。独自の標準ライブラリ検証で名前・description・frontmatter・参照先を検査した。

## 残る範囲

- 新しいセッションでのスキル一覧の表示、Claude経由の共通入口発見、実タスクのトークン使用量は未実測。現在のセッションには削除前のスキル一覧が残り得る。
- CIは対象ファイルのPR変更時に実行する。既存の全資料を常時全文検査するものではなく、管理対象の入口・共通feature-flowを検査する。
- Notion仕様ミラー、旧仕様、過去の監査報告は正本/履歴維持のため今回分割・書換していない。対象機能へ探索を絞る指示で読み込みを抑える。
- Claudeと.agentsのuloop複製統合、codex_runner.pyの資源制御は今回の変更対象外。

## 作業環境

既存チェックアウトのUnity設定など5ファイルの未コミット変更を保持するため、独立した疎チェックアウトを使用した。作業ブランチはfeature/tools/agent-harness/agent。編集前のpush後、feature/tools/agent-harness/masterが開始コミット170116d63で生成されたことを確認した。
