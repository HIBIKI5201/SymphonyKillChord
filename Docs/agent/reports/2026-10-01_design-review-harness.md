# ハーネスエンジニアリング（AIエージェント運用基盤）設計レビュー（2026-10-01, Codex）

## 要約

全体評価は **C（A〜Eの5段階：実用的な道具は揃うが、委任の資源制御と共通契約が弱い）**。
Unityの状態観測、拍指定入力、有限時間QA、Notion書き込みの保護は具体的で、学生・有志チームでも反復作業を任せられる基盤がある。
一方、Codex残量保護は古い記録を現在の残量へ読み替え、同時実行を予約管理しないため、依頼者報告の上限到達事故を防げない設計である。
規約のAI編集制限、スキルの出力先、Notionの正本説明が食い違い、同じ作業でもエージェントごとに判断が変わる。
今週は残量判定・完了判定・指示競合を直し、次にスキル共有と検証証跡を整える。大規模な新基盤への置換は不要である。

## 現状の構造

### 調査条件・根拠の扱い

- 表題日は指定どおり2026-10-01。実際のローカル読取日は2026-10-02である。
- ソース、設定、スキル、仕様ミラー、個人メモリの索引、Drive保存記録、既存レビューを静的に照合した。
- Git操作、Unity起動、CLI委任、外部サービスの更新は行っていない。
- 書き込みは本ファイルだけ。本依頼の出力先指定を通常の`Docs/agent/`・コミット規則より優先した。
- `.claude/settings.local.json`は存在だけ確認し、内容を読んでいない。秘密設定の値も読んでいない。
- 残量関数だけをASTから取り出し、人工入力でメモリ内実行した。runner全体や個人セッションログは実行・取得していない。
- 「28時間前・5時間枠100%・並列10本」の事故条件は依頼者の報告。実ログからの独立した再現確認ではない。
- 以下の`N/`は`Docs/NotionSpecifications/Symphony Kill Chord/`を表す。
- `C/`は`.claude/skills/`、`X/`は`.codex/skills/`、`S/`は`.agents/skills/`を表す。
- パス末尾の`:数値`は今回読み取ったファイルの行番号。Notionはページ名でも参照する。

### 指示・実行・記録の流れ

```text
依頼者の現在の指示
  ├─ AGENTS.md ─ 設計思想 / コード規約 / 仕様ミラー / 出力・PR規則
  ├─ Claude Code
  │    ├─ .claude/settings.json ─ startup時に仕様submoduleを非同期更新
  │    ├─ .claude/skills ─ 28スキル
  │    └─ skc-codex-implement → scripts/codex_runner.py → Codex CLI
  └─ Codex
       └─ .codex/skills ─ 21スキル
            ↘ .agents/skills ─ 共通QA本体 + uloopスキル（計19）

道具
  ├─ uloop CLI / Unity側パッケージ → Editor操作・compile・tests・PlayMode
  ├─ 共有JS → uloop → Assets/Editor/AIDebugPlay → JSON観測・攻撃キュー
  └─ SinfoniaOperator → Notion取得 / Discord取得 / Notion更新

記録と統合
  ├─ .uloop/qa/<runId>/ ─ ローカル生証跡
  ├─ Docs/agent/ ─ 共有用の解析・報告・引継ぎ
  └─ agent → 同階層master → develop向けDraft PR → 人の受入判断
```

### 指示層の実測とトークン概算

文字数はUTF-8をデコードした本文の文字数であり、バイト数ではない。
トークンは使用モデルのtokenizerを実行していないため、日本語主体は約1〜2文字/token、英語主体は約3〜4文字/tokenで概算した。
全スキル本文を常時読む前提ではない。起動時のdescriptionと、発火後に読む本文を分けて評価する。

| 対象 | 実測 | 概算トークン | 読み込み方・評価 |
|---|---:|---:|---|
| `AGENTS.md` | 23行・1,937文字 | 約1,000〜1,900 | 短いが大半がPR運用。権限・証跡・復旧の共通契約は少ない |
| `Assets/Scripts/DesignPhilosophy.md` | 298行・5,787文字 | 約2,900〜5,800 | レビュー系は全文参照。実装例を含む |
| `Assets/Scripts/CodeGuidelines.md` | 80行・2,161文字 | 約1,100〜2,200 | レビュー系・委任時に参照 |
| Claudeの28 description | 合計9,234文字 | 約2,300〜3,100 | 長い操作範囲の説明を含む |
| Codexの21 description | 合計5,405文字 | 約1,400〜1,800 | audit・reviewの境界が一部重複 |
| 共通19 description | 合計4,564文字 | 約1,100〜1,600 | Codex側とuloopが重複登録され得る |
| Claude全SKILL本文 | 合計143,443文字 | 約36,000〜72,000 | 一括読込には大きい。オンデマンドなら許容可能 |
| Codex全SKILL本文 | 合計96,934文字 | 約24,000〜48,000 | 同上。references本文は含めない |
| 共通全SKILL本文 | 合計98,157文字 | 約25,000〜49,000 | 18本のuloop複製を含む |

主要3文書だけで約5,000〜10,000 token相当。まず削るべきは規約の根拠ではなく、二重登録・転載・無関係なスキル発火である。
ルートの`CLAUDE.md`は存在しない。調べた主要ソース・スキル配下にも追加の`CLAUDE.md`・`AGENTS.md`は見つからなかった。
Claudeが常に`AGENTS.md`を自動読込することは、このリポジトリのファイルだけでは保証できない。
一方、`C/skc-feature-flow/SKILL.md:9`と`C/skc-codex-implement/SKILL.md:29`は明示的に同文書へ誘導している。

### スキル一覧・重複・片側のみの機能

`*/SKILL.md`の列挙と同名ファイルのバイト比較では、Claude 28本、Codex 21本、同名20本はすべて一致した。
そのうち18本のuloopは`.agents/skills/`側とも一致する。共通QA本体を含め、`.agents`は19本である。
同名スキルの現時点の本文差分はない。重複が将来の同期負担を作ることと、既に不一致であることは区別する。

| スキル名 | Claude | Codex | 共通側・補足 |
|---|---|---|---|
| skc-ai-debug-qa | 有 | 有・一致 | 共通attack-queueへ転送する7行の入口 |
| skc-architecture-audit | 有 | 有・一致 | Codex側でもClaude専用review名へ誘導 |
| skc-code-guideline-check | 有 | 無 | 規約・設計思想の詳細チェックリスト |
| skc-code-review | 無 | 有 | 仕様・用語・責務まで含む差分レビュー |
| skc-codex-implement | 有 | 無 | Claude設計→Codex生成→Claude確認 |
| skc-feature-flow | 有 | 無 | ブランチ作成から二段階PRまで |
| skc-notion-spec-diff-check | 有 | 無 | 仕様と実装の差分調査 |
| skc-notion-spec-write | 有 | 無 | pull・編集・dry-run・確認・push |
| skc-planner-masterdata-workflow | 有 | 無 | DataID・SourceDataProvider・編集画面 |
| skc-sinfonia-importers | 有 | 無 | 仕様・Discordミラーの取得 |
| skc-unity-autobuilder | 有 | 無 | ローカルビルドとCIの対応 |
| uloop-clear-console | 有 | 有・一致 | 共通側にも一致するコピー |
| uloop-compile | 有 | 有・一致 | 同上 |
| uloop-control-play-mode | 有 | 有・一致 | 同上 |
| uloop-execute-dynamic-code | 有 | 有・一致 | 同上 |
| uloop-find-game-objects | 有 | 有・一致 | 同上 |
| uloop-get-hierarchy | 有 | 有・一致 | 同上 |
| uloop-get-logs | 有 | 有・一致 | 同上 |
| uloop-hot-reload | 有 | 有・一致 | 同上 |
| uloop-launch | 有 | 有・一致 | 同上 |
| uloop-pause-point | 有 | 有・一致 | 同上 |
| uloop-record-video | 有 | 有・一致 | 同上 |
| uloop-replay-input | 有 | 有・一致 | 同上 |
| uloop-run-tests | 有 | 有・一致 | 同上 |
| uloop-screenshot | 有 | 有・一致 | 同上 |
| uloop-set-game-view-size | 有 | 有・一致 | 同上 |
| uloop-simulate-keyboard | 有 | 有・一致 | 同上 |
| uloop-simulate-mouse-input | 有 | 有・一致 | 同上 |
| uloop-simulate-mouse-ui | 有 | 有・一致 | 同上 |
| skc-ai-debug-attack-queue | 入口経由 | 入口経由 | 共通側のみ。実装と制約を集約 |

片側専用がすべて欠陥というわけではない。ClaudeからCodexへ委任する入口がClaude専用なのは自然である。
ただしPR手順・ビルド検証・マスターデータ規約は実行者の種類に依存しないため、共通化する価値が高い。

### descriptionと参照パスの確認結果

| 対象 | 実測・具体例 | 判定 |
|---|---|---|
| uloop-compile / run-tests | C#編集後／EditMode・PlayModeという対象を明記 | 発火条件が具体的 |
| uloop-launch | 未起動か再試行後もfreezeの場合だけ、と限定 | 不要な再起動を抑える良い境界 |
| uloop-hot-reload | descriptionに型追加・署名変更・assembly境界を長文で列挙 | 判定には有用だが、詳細は本文へ移せる |
| skc-architecture-audit | diffは`skc-code-guideline-check`へ、と両側で記載 | Codex側の実在名`skc-code-review`と不一致 |
| skc-code-review | `architecture checks`まで含む | 全体監査と差分レビューの境界を明記したい |
| skc-notion-spec-diff-check | descriptionにexport更新も含む。本文19行は先にpull、48行は依頼がなければ省略 | 調査と同期の条件が曖昧 |
| skc-sinfonia-importers | 仕様に答えがある質問全般を例示。ただし本文19行は既存ミラーを先に読む | descriptionだけでは発火範囲が広い |
| plannerスキルの参照 | `C/skc-planner-masterdata-workflow/SKILL.md:11` | `SourceDataProviderSettings.Editor.cs`の`Core/`が欠落 |
| Notion出力の誤配置例 | `C/skc-sinfonia-importers/SKILL.md:54`、diff-check:34 | 非存在パスは失敗例の説明であり参照切れではない |
| reviewのreferences | `X/skc-code-review/references/source-routing.md` | ファイルは存在。優先順位の内容に問題がある |
| Exporterの設定手順 | `SinfoniaOperator/NotionMarkdownExporter/README.md:12` | 案内された秘密設定のsampleファイルは存在しない |

Markdownリンクとコード表記の静的なパス候補を抽出し、リポジトリ基点とスキル基点で存在確認した。
プレースホルダ、glob、URL、コマンド引数は非存在判定から除外した。動的に組み立てる全パスの検査ではない。
単純抽出の非存在候補はClaude側3件、Codex側0件で、前者の2件は上記の意図的な誤配置例だった。
`Assets/Docs/AGENTS.md`も存在しないが、現行`.coderabbit.yaml:7`は実在する2規約を参照している。
Notion「AIエージェント向け参照先一覧」の旧警告を、現行CodeRabbitの欠陥として再掲してはいけない。

### 道具・権限・検証ループ

| 層 | 自己検証できること | 限界・副作用 |
|---|---|---|
| codex_runner | 残量ログ探索、終了コード分岐、出力ファイルの更新確認 | 現在の残量・内容の正しさ・並列予約を保証しない |
| uloop compile | Unityのコンパイル結果、エラー・警告、結果不明の区別 | Play状態やpatchを破棄し得る。結果不明は合格ではない |
| uloop run-tests | Test Runner結果、XML、対象なしの診断 | 既定で未保存Scene・Prefab Stageを保存する |
| hot-reload / pause-point | 実行中の変更試行、到達経路・変数観測 | 一時patchの挙動だけでは保存ソースの受入にならない |
| 共有QA | project一致確認、JSON、攻撃履歴、有限観測、UI callback | `qaVerdict`はUnverified。実機・物理入力・聴感は別 |
| NotionMarkdownWriter | allowlist、dry-run、更新日時による競合検知 | 共有サービスへの変更。承認範囲と対象を親が保持する必要 |
| Notion/Discord Exporter | 情報取得・差分更新、空ログ上書き防止等 | ファイルを生成・更新するため、読取専用調査には実行不可 |
| GitHub PR checks | 宛先規則、develop PRのチェック欄 | 実際のコンパイル・PlayMode証跡を検証するものではない |

`.claude/settings.json:2`にはSessionStart hookだけがあり、共有の`permissions`定義はない。
これは「すべて許可」を意味しない。実効権限は今回未読の個人設定・実行環境にも依存するため判定不能である。
hookはstartup時に仕様submoduleをバックグラウンド更新し、標準出力・エラーを捨てる（同:9）。
ルート`.mcp.json`は存在しない。共通QAはMCP登録を前提とせず、既存uloop CLIを発見して使用する（`S/.../scripts/qa-transport.mjs:60`）。
`Packages/manifest.json:52`はuloopのGit参照を持ち、`packages-lock.json:722`以下には解決済みhashがある。
したがってUnity側まで「完全に未固定」とは言えない。一方、外部CLIとスキル記述の対応版は共有手順から確定できない。

### 記憶・成果物・worktree

| 種類 | 現物 | 評価 |
|---|---|---|
| 個人記憶の索引 | `.design-review/claude_memory_index.md`、21項目 | 失敗経験が蓄積。リンク先本文は今回の提供対象外 |
| 共有出力規約 | `Docs/agent/README.md:3`、`:9`、`:38` | 追跡対象、命名、秘密・生ログの除外を明文化 |
| 過去成果物 | Docs直下16 md、旧AI資料31 md、Docs/agent配下22 md | 数は全てローカル実在数。Git追跡数ではない |
| 引継ぎの散在例 | `Docs/作戦画面UI刷新_引継ぎ資料_2026-09-08.md`等 | 過去資料を新規違反とは断定しないが、正本への誘導が必要 |
| 追加の実装レポート | `SinfoniaOperator/GeminiSummarizer実装レポート.md`等 | ツールREADMEとAI作業記録が混在 |
| QA生証跡 | `.uloop/qa/<runId>/` | `.gitignore:113`で除外。共有用要約への昇格が必要 |
| worktree | `.claude/worktrees/`は存在し、今回列挙時は空 | 過去の並列編集やcleanupの安全性までは分からない |
| 一時出力 | QAは`.uloop/qa-input/`、runnerは出力横のlast-message | Temp依存ではないが、寿命・公開可否の共通表がない |

## 良い点

- 規約を要約だけで判断せず、正本を全文読む手順がある。`X/skc-code-review/SKILL.md:12`、`C/skc-code-guideline-check/SKILL.md:13`。
- 仕様検索を対象機能へ絞り、未確定語を確定扱いしない。`X/skc-code-review/SKILL.md:18`、`:57`。
- QA入口は薄い共通参照で、実装をClaudeとCodexで共有する。`X/skc-ai-debug-qa/SKILL.md:6`。
- QAの通信成功とゲーム上の合格を分けている。`S/skc-ai-debug-attack-queue/SKILL.md:25`。
- QA通信はshellを介さず、15秒・4MiBの上限を持つ。未知のJSONを成功扱いしない。`qa-transport.mjs:31`、`:35`、`:45`。
- Editorの`Application.dataPath`を毎回照合する。別worktreeのUnityを誤操作しにくい。`qa-transport.mjs:76`。
- QAの排他ファイルは`wx`で作成し、runIdとPIDを残す。`S/.../scripts/qa-aging.mjs:12`。
- 攻撃再送は同一IDを使い、通信結果不明時の二重入力を避ける。`S/skc-ai-debug-attack-queue/SKILL.md:71`。
- AIデバッグassemblyはEditor限定で、製品Runtimeと分離される。`Assets/Editor/AIDebugPlay/KillChord.Editor.AIDebugPlay.asmdef:16`。
- Notion Writerには許可対象・dry-run・競合検知・書込再試行抑制がある。`SinfoniaOperator/NotionMarkdownWriter/README.md:8`。
- 委任後は生成物を読み、compileとレビューを行い、修正を再委任し続けない。`C/skc-codex-implement/SKILL.md:85`。
- PR本文の確認済み・未確認を区別し、元PRからdevelop Draftへ伝える。`C/skc-feature-flow/SKILL.md:65`、`:95`。

## 問題点

重大度はハーネスの誤動作・誤判定・共有再現性への影響で付けた。ゲーム本体の品質採点とは独立である。

| ID | 重大度 | 問題 | 根拠 | 影響 |
|---|---|---|---|---|
| H01 | 高 | 古い残量記録のreset時刻を過ぎると現在残量を100%と扱う。ageは表示だけで許否に使わない | `scripts/codex_runner.py:186`、`:198`、`:271` | 他セッションの消費を見ず、大量委任を許可する |
| H02 | 高 | 残量チェックと起動の間に共有予約・同時実行制限がない。判定不能は既定fail-open | 同:205、`:212`、`:460`、`:511` | 複数プロセスが同じ残量を使えると判断。並列10本事故の条件を抑止できない |
| H03 | 高 | runnerの成功条件が非空ファイルのmtime更新。最大コードブロックの救出で別内容を出力する可能性もある | 同:382、`:390` | 書けたことを完成と誤認。Markdown報告では図だけを救出する可能性（推測） |
| H04 | 中 | 出力先のworkspace外参照を拒否せず、fallbackはPython親プロセスが直接書く。失敗時last-messageを次回前に消さない | 同:306、`:337`、`:395`、`:404`、`:505` | 子sandboxだけでは親の書込範囲を制限できない。古い失敗文の誤検出もあり得る（推測） |
| H05 | 高 | 調査用batchは非空出力だけで完了スキップし、一般エラーでも次へ進みALL_DONEを出す | `.design-review/run_batch.sh:5`、`:8`、`:12` | 途中出力や古い結果を完成と誤認。直列化だけでは復旧が不十分 |
| H06 | 高 | 「AI編集はコメント・タイポまで」と、実装を委任する手順が衝突 | `Assets/Scripts/CodeGuidelines.md:4`対`C/skc-codex-implement/SKILL.md:8` | 担当ごとに停止・実装の判断が分かれる。現在の依頼による許可と通常規則の関係が不明瞭 |
| H07 | 中 | 正本・成果物の優先順位が競合。auditはDocs/agentをgitignore扱い、reviewは最新計画をNotionより上位とする | `X/skc-architecture-audit/SKILL.md`末尾、`X/skc-code-review/references/source-routing.md:28`、`Docs/agent/README.md:3` | 未承認計画や古い保存方針が正式判断に混入する |
| H08 | 中 | 共通uloopの三重配置、片側だけのPR手順、Codexにないスキル名への誘導がある | スキル比較、`X/skc-architecture-audit/SKILL.md:3`、`C/skc-feature-flow/SKILL.md` | 発火の重複、更新漏れ、実行者ごとの手順差 |
| H09 | 中 | 実在しない参照と個人記憶への依存が残る | `C/skc-planner-masterdata-workflow/SKILL.md:11`、`C/skc-sinfonia-importers/SKILL.md:60`、Exporter README:12 | 新しい環境では復旧手順を辿れない |
| H10 | 中 | startup hookが無条件に背景更新し、成否・完了を通知しない | `.claude/settings.json:5`、`:9` | 読取専用依頼との衝突、調査途中の版変更、失敗の見落とし（発生状況は未確認） |
| H11 | 高 | 汎用動的コードは状態注入を推奨するが、プロジェクト共通の証跡汚染・復元規則がない | `X/uloop-execute-dynamic-code/SKILL.md:15`、`AGENTS.md`全体 | 障害を迂回したセッションで合格を出すおそれ（推測）。共有QA側の区別を全体へ広げたい |
| H12 | 中 | テスト・Play・compileの副作用と権限を結ぶ共通事前確認がない | `X/uloop-run-tests/SKILL.md:13`、`X/uloop-compile/SKILL.md:50` | 未保存の人間作業が保存され、対象外差分や状態破棄が起き得る |
| H13 | 中 | ツールはあるが自動assert・証跡に結び付いた受入が薄い | `qa-aging.mjs:18`、`Docs/QA/AI_QAツール対応表.md:63`、`ValidateDevelopPR.yml:33` | JSON取得成功やチェック欄だけでは回帰を防げない |
| H14 | 中 | 生証跡・一時原稿・共有報告の昇格手順と寿命が統一されていない | `.gitignore:6`、`:113`、`Docs/agent/README.md`、ローカル配置計数 | セッション終了で証拠が失われる、個人環境に引継ぎが残る |
| H15 | 中 | worktree単位の所有・親子間契約が委任プロンプトの必須項目にない | `C/skc-codex-implement/SKILL.md:37`、`.claude/worktrees/` | 同じbranch・ファイル・Editorを複数担当が操作し得る（推測） |
| H16 | 中 | 外部CLI・Unity package・スキルの互換版とMCPの必要条件が一枚にない | `Packages/manifest.json:52`、`qa-transport.mjs:60`、`X/uloop-launch/SKILL.md:60` | 他メンバーの環境で同じコマンドが同じ意味とは限らない |
| H17 | 中 | 秘密を置かない規則はあるが、公開する証跡・runner診断への適用手順が弱い | `Docs/agent/README.md:40`、`scripts/codex_runner.py:285`、Drive保存記録#2141 | 個人絶対パスや外部識別情報を生ログごと公開しやすい。新たな漏えいは確認していない |
| H18 | 中 | master生成完了を待たない高速な後続pushで、空pushの意図を失う | `AutoCreateMasterBranch.yml:58`、`:80`、`C/skc-feature-flow/SKILL.md:41` | 自動エージェントほど差分なし例外へ入りやすい。既存OPS-02との接点 |

### 残量事故の原因と、確認できた範囲

`_latest_rate_limits()`はログ内のイベント時刻ではなくファイルmtimeを記録時刻として返す（186行）。
古い残量イベントの後ろに別のログが追記されれば、見かけのageが短くなる可能性がある。
さらに`_window_remaining()`はresetを過ぎた値を100へ変換し、その後の消費を観測しない（198〜199行）。
人工入力`used_percent=100, resets_at=1000, now=1001`で戻り値`100.0`を確認した。
これはAPIへの問い合わせではなく、該当関数をメモリ内で実行した結果である。
`--fail-closed`を付けても、この古いデータは「判定不能」にならないためH01を解消しない。
最も逼迫したwindowを使う処理（270行）は良いが、入力の鮮度が保証されなければ保護として不十分である。
依頼者報告の事故を説明できる実装上の欠陥と、当時の実際の消費量・順序を独立に立証できたことは分ける。
Discord「プログラマー」ログ5645行にも5時間制限で実装が進まない旨の記録があり、残枠は実際の運用制約である。
ただし、このDiscord記録を今回の並列10本事故そのものの証拠とは扱わない。

### 検証の穴の解釈

`Assets`内の自作側asmdefを調べた範囲では`TestAssemblies`指定が見つからなかった。Plugins・AssetStoreToolsは対象外とした。
これは全テストコードの不存在を証明しない。外部テスト、独自実行基盤、暗黙的な配置は未確認である。
`Docs/QA/AI_QAツール対応表.md:11`には導入時の依存解決失敗と未実行項目が残るが、現在もcompile不能だとは断定しない。
NotionのQAページは追加13項目も定める一方、ローカル対応表の主表は旧範囲が中心で、期待値の版を結ぶ必要がある。
実機・音声・OS・保存破損復旧をEditorだけで合格にしない設計は正しい。自動化不能な領域を欠陥として数えない。
不足は、未確認を残しつつ、どの版のどの受入条件へ証跡を提出するかの接続である。

## 改善提案

工数はS＝半日〜1日、M＝2〜5日、L＝1〜2週程度の作業量目安。外部承認待ち・実機検証待ちは含まない。
以下は変更案であり、今回これらのファイルを修正していない。

### すぐやる（今週）

#### P1. 古い残量を「不明」とし、まず直列実行へ固定する

- **何を**：`scripts/codex_runner.py`のCapacityReportにイベント取得時刻・観測期限・判定状態を追加する。
- **なぜ**：reset後の現在残量を過去ログから確定できず、`--fail-closed`だけでは事故を防げないため。
- **手順**：イベント時刻を使用し、古い・時刻不明・reset後の観測をUnknownへ落とす。100%へ推定回復しない。
- **手順**：既定をUnknown時停止にし、理由をFresh/Unknown/Exhausted等で返す。暫定期限は設定可能にし、実測で調整する。
- **手順**：`C/skc-codex-implement/SKILL.md`に「当面同時1本、Unknownを残量不足の実数として報告しない」を書く。
- **受入**：28時間前、reset直後、時刻欠落、ログmtimeだけ新しいケースをAPI不使用の単体テストで拒否できる。
- **工数感**：M。**関連問題ID**：H01、H02。

#### P2. 出力の存在と完了を分ける

- **何を**：runnerに成果物種別と検証段階を追加し、`.design-review/run_batch.sh`の非空スキップを廃止する。
- **なぜ**：今回の報告生成では、途中ファイル・図のコードブロック・前回の成功記録が完成扱いになるため。
- **手順**：Markdownでは必要節・行数・禁止情報の確認を行い、code-fence救出を無効にする。ソースは生成済みと検証済みを分ける。
- **手順**：runId・入力hash・出力hash・終了状態を結んだ完了記録を作り、hash一致と検証成功の両方で再実行を省略する。
- **手順**：一般エラーを非ゼロで返し、batch最終結果に失敗一覧を残す。旧statusだけで新しい出力を成功にしない。
- **手順**：出力のresolve後に許可ディレクトリ内か検査し、親のfallbackも同じ検査を使う。last-messageをrunId別にする。
- **受入**：空・途中・旧成果物・図だけの応答・workspace外出力を、成功として受け入れない。
- **工数感**：M。**関連問題ID**：H03、H04、H05。

#### P3. AI編集権限と正本の競合を解消する

- **何を**：`Assets/Scripts/CodeGuidelines.md:4`と`AGENTS.md`に、通常作業と現在の依頼による許可範囲の関係を書く。
- **なぜ**：日常の自律実装を認める運用とコメント限定規則を両立させ、不要な確認と黙った逸脱を減らすため。
- **手順**：規約担当が「明示された実装依頼で変更可能、レビューだけなら読取専用」等の現行方針を確定する。
- **手順**：出力先・Git操作・Unity操作・外部送信の指定を独立した制約として保持し、委任先にも渡すと`AGENTS.md`へ記載する。
- **手順**：両auditスキルのgitignore記述を訂正し、`source-routing.md`では承認済み計画と未承認草案を区別する。
- **手順**：ルート`CLAUDE.md`を薄い参照入口として置く案を採用し、`AGENTS.md`を読むことだけを記す。規則本文は複製しない。
- **受入**：実装・レビュー・本件のような単一レポート出力で、両エージェントが同じ許可範囲を答えられる。
- **工数感**：S。**関連問題ID**：H06、H07、H08。

#### P4. 調査開始時に入力版と副作用を固定する

- **何を**：`.claude/settings.json`の更新hookを見直し、`AGENTS.md`に読取専用と同期の分離を記す。
- **なぜ**：依頼を理解する前の背景更新では、版固定・Git禁止・通信失敗の扱いを守りにくいため。
- **手順**：startupは既存ミラーの状態表示に限定し、更新は明示的な同期作業へ移す。少なくとも更新完了・失敗を隠さない。
- **手順**：`skc-notion-spec-diff-check`の「先にpull」と「更新依頼がなければ省略」を一つの条件分岐へ整理する。
- **手順**：レビュー冒頭に入力版・取得日・同期有無を記録する。Git禁止時は与えられたスナップショット日を使う。
- **受入**：読取専用起動でsubmodule更新が走らず、同じ報告の途中で参照版が変わらない。
- **工数感**：S。**関連問題ID**：H10、H07。

#### P5. 検証前の人間作業保護と証跡区分を共通化する

- **何を**：`AGENTS.md`から参照する`Docs/agent/handoffs/2026-10-01-agent-verification-contract.md`案を作る。
- **なぜ**：一般uloopの便利な既定値を、プロジェクトの変更権限・受入判定へ接続するため。
- **手順**：対象checkout、Editorのproject、未保存Scene、Play状態、patch有無を確認し、テストは原則`--unsaved-changes fail`を指定する。
- **手順**：動的な状態注入をassistedと記録し、変更値・目的・復元方法を書く。受入は保存ソースからの新規セッションで再確認する。
- **手順**：clear-console前に必要な障害ログを保存する。compileのUnknown、NoTestsFound、途中終了を合格へ変換しない。
- **手順**：PRの「確認済み」に方法・対象版・証跡を、「未確認」に残る実機確認を書く。不要と未実施を区別する。
- **受入**：未保存作業を勝手に保存せず、状態注入ありの結果を通常E2E合格として提出しない。
- **工数感**：S。**関連問題ID**：H11、H12、H13。

### 次に（1か月）

#### P6. スキルを共通本体と実行者別入口へ整理する

- **何を**：`.agents/skills/`を共通本体にし、`.claude/skills/`・`.codex/skills/`は必要な入口と役割差だけを持つ。
- **なぜ**：現在のQAが既に示している共有方式を、18本のuloopと共通PR手順へ広げられるため。
- **手順**：ランタイムの探索仕様を確認して二重登録を避け、互換性上コピーが必要なら生成元とhash検査で同期する。
- **手順**：Codexのaudit誘導先を`skc-code-review`へ修正し、全体監査／差分レビュー／同期だけのトリガーを分ける。
- **手順**：plannerの`Core/`欠落を修正。機械依存の復旧は共通referencesへ移し、descriptionから細かい実装制約を減らす。
- **受入**：公開SKILLの名前重複・実在参照・共有本体との差分をCIで検査。例示パスは明示的に除外する。
- **工数感**：M。**関連問題ID**：H08、H09、H16。

#### P7. 個人記憶を「既に共有済み／昇格／保持」に仕分ける

- **何を**：`.design-review/claude_memory_index.md`を棚卸しの入力にし、下表の対象文書へ必要な知識だけを書く。
- **なぜ**：個人記憶を全転載すると肥大する。失敗を防ぐ条件と復旧手順だけを全実行者に届けるため。
- **手順**：索引しかない項目はコード・実行記録を担当者が再確認してから規則化する。古い作業状態を恒久規則にしない。
- **工数感**：M。**関連問題ID**：H09、H14、H15、H16。

| 索引の知識 | 扱い | 具体的な昇格先・追記内容 |
|---|---|---|
| PRテンプレ・二段階PR・空push | 既に共有済み | `AGENTS.md`を正本とし、記憶は参照だけに縮める |
| Stage2だけIssue close | 一部共有済み | 共通feature-flowへ例と判定条件を集約 |
| Python実体・Codex CLI探索 | Claude側共有済み | 共通環境手順に確認コマンドと機械依存である旨を書く |
| DataID形式 | 部分共有済み | plannerスキルと`CodeGuidelines.md`から型付きIDの既存規約へ誘導 |
| NavMesh APIのfreeze経験 | 昇格候補 | プロジェクト用uloop補足に対象API・再試行条件・一度の隔離検証を記す |
| dotnet buildとScriptAssembliesの鮮度 | 昇格 | 検証契約に代替検証の限界、Unity確認を未実施とする条件を書く |
| CRLF一括置換 | Claude側共有済み | 共通編集手順へ移し、バイナリまたはnewline指定と差分確認を残す |
| PR状態確認後に追加push | 昇格 | 共通feature-flowに既存PRのhead・state確認とマージ後の扱いを書く |
| Scene保存のYAML再配置 | 昇格 | 検証契約に保存副作用と対象差分確認を記す。手編集を無条件の標準にはしない |
| NotionExporter停止・watchdog | 昇格 | importerの`references/troubleshooting.md`に症状・切分け・公開exe更新確認を書く |
| ZString / Unsafe依存 | 昇格 | Unityセットアップ手順に復元・PluginImporter・初回worktreeの確認を記す |
| Notionの不変・可変ページ、変更履歴 | 昇格 | notion-spec-writeのreferencesに現行ガイドへの参照と更新単位を書く |
| Drive所有者・ID依存 | 要約のみ昇格 | importer/運用補足に操作前提を書く。識別情報は一切転載しない |
| 素材待ち・前セッションの未完了 | 個人記憶から恒久規則へは移さない | 関連Issueまたは`Docs/agent/handoffs/`へ期限付き状態として記録 |

#### P8. 委任に資源予約と作業所有権を追加する

- **何を**：`scripts/codex_runner.py`と`skc-codex-implement`に、同時実行枠・停止共有・作業契約を追加する。
- **なぜ**：残量の鮮度だけを直しても、同じ枠を複数の子が消費すれば保護にならないため。
- **手順**：同じ利用枠を使うプロセス間で排他を取り、runId・PID・開始時刻・所有checkout・出力を登録する。
- **手順**：上限応答を受けたら新規起動を止める回路を共有し、進行中の成果物を確認してから親へ戻す。
- **手順**：推定消費予約は保証値と呼ばず、安全余裕として扱う。信頼できる残量がない場合は小さい実行数を維持する。
- **手順**：子プロンプトに許可ファイル、branch、Git可否、Unity可否、外部送信可否、検証・終了条件を必須化する。
- **手順**：親は設計・統合・最終受入、子は限定実装を担当。worktreeとEditorは1所有者とし、別checkoutを再利用しない。
- **受入**：複数同時要求でも上限数を超えて起動せず、他runのlock・Editor・成果物を回収しない。
- **工数感**：M。**関連問題ID**：H02、H04、H15。

#### P9. QAの証跡を保存ソースと受入条件へ結ぶ

- **何を**：共有`qa-aging.mjs`のsummary、`Docs/QA/AI_QAツール対応表.md`、PR記載手順を連結する。
- **なぜ**：観測基盤の価値を回帰検証へ変え、Unverifiedを人が判断できる状態にするため。
- **手順**：summaryへ対象版・dirtyの有無・仕様版・シーン・入力方式・patch/状態注入・中断理由を記録する。
- **手順**：現在のrunId・時刻・環境区分を保持し、ローカル絶対パス等は共有版から除く。
- **手順**：まず六拍種の成立、ミッション終了、再入場でイベント二重登録がないこと等、仕様が確定した少数ケースをassert化する。
- **手順**：実機が必要なケースは自動合格にしない。QA期待値の競合はゲーム・仕様レビュー側の決定を待って更新する。
- **受入**：一つのPRから、対象ソース・実施方法・生証跡の要約・未確認項目へ辿れる。
- **工数感**：M。**関連問題ID**：H11、H13、H14。

#### P10. 一時出力・共有出力・公開前処理を定める

- **何を**：`Docs/agent/README.md`に寿命表と公開前確認を追記し、共通環境手順にCLI互換情報を書く。
- **なぜ**：Temp消失、ignoreされた証跡の未共有、個人情報の生ログ転載を同じ保存設計で防ぐため。
- **手順**：Unityの`Temp/`へ再利用する計画・PR原稿・引継ぎを置かないと明記する。終了時消失の前提は運用注意として扱う。
- **手順**：`.uloop/`は再生成可能な生証跡、`Docs/agent/reports/`は匿名化した確定報告、`handoffs/`は次作業への契約とする。
- **手順**：既存の散在物は一括移動せず、新しいREADME索引から現行／履歴を明示して参照する。
- **手順**：秘密・Drive識別情報・個人連絡先・不要な絶対パスを公開版へ含めない。機密値入りの設定やログそのものを記憶へ転載しない。
- **手順**：Unity packageの解決版、uloop CLIの確認済み版、Node/Python前提、必要なMCPの有無と代替CLIを記す。
- **受入**：別担当が同じツール組合せを準備でき、Unity終了後も共有すべき結論が残る。
- **工数感**：M。**関連問題ID**：H14、H16、H17。

### 将来

#### P11. 小さなハーネス回帰テストをCIへ組み込む

- **何を**：`scripts/tests/`と`.github/workflows/`に、API不要で走るrunner・パス・スキル契約の検査を追加する。
- **なぜ**：説明文だけの再発防止から、事故条件を固定した継続検査へ進めるため。
- **手順**：古い残量、並列予約、途中出力、誤った完了記録、許可外出力、参照切れ、プロンプト制約伝達をfixture化する。
- **手順**：副作用のあるUnity・Notion実行は別レーンにし、ここではmockで親子間の契約だけを検証する。
- **手順**：事故率、無駄な再試行、Unknown率、検証証跡欠落を計測する。生成行数や子の本数を成果指標にしない。
- **受入**：今回の残量誤判定とbatchの誤完了が再導入されるとCIが失敗する。
- **工数感**：M。**関連問題ID**：H01〜H05、H08、H09。

#### P12. PR自動化とエージェントの待機を一つの状態機械へ寄せる

- **何を**：共通feature-flowと`AutoCreateMasterBranch.yml`に、作成要求・生成確認・編集開始・PR作成の境界を明記する。
- **なぜ**：エージェントの高速pushと背景workflowの競合を、毎回の例外復旧で吸収しないため。
- **手順**：OPS-01/02の修正を運用担当と共同で行い、生成基点のSHAを固定し、条件不成立時は後続処理を実行しない。
- **手順**：生成完了と対象branchを確認してから続行する。別作業の最新workflow runを拾わないよう実行を特定する。
- **手順**：PRのReady化とdevelopマージは現在の依頼の権限で判断し、自動生成Draftの存在を受入完了と見なさない。
- **受入**：高速連続push・生成失敗・既存PRマージ済みから、重複PRや直接更新に逃げず復旧できる。
- **工数感**：M。**関連問題ID**：H15、H18。

## 他観点との接点

既存レビューは`C:/Users/takut/GameProject/SKC-design-review/Docs/agent/reports/`の2026-10-01版を参照した。
本稿は内容を再監査せず、エージェントへ渡す判断条件・道具・証跡への接続を追加する。

| 既存レビュー | 接点 | 本稿で追加した責任 |
|---|---|---|
| `2026-10-01_design-review-ops.md` OPS-01/02 | master生成の条件・SHA競合 | 高速エージェントの待機・再開契約。H18/P12 |
| 同 OPS-11/12 | 正本説明と成果物配置の競合 | スキル内の古い出力指示、個人記憶からの昇格。H07/H14 |
| `2026-10-01_design-review-notion-spec.md` | 正本・キャッシュと過去資料の混在 | 調査入力版の固定、計画書の承認状態、description発火条件 |
| `2026-10-01_design-review-code.md` | 自動回帰検証と開発機能の分離 | Editor限定asmdefを評価し、assertと証跡の契約を提案 |
| `2026-10-01_design-review-game.md` G01/G12 | Just等の期待値と旧QAの競合 | エージェントが独断で期待値を選ばず、未確認を維持する |
| `2026-10-01_design-review-google-drive.md` D01 | 認証更新・所有者・取り込み確認 | 公開報告への匿名化と操作前提。認証復旧そのものは同レビューへ委ねる |

Drive記録#2141では秘密設定のゴミ箱移動と、認証更新・実動確認の残課題が分かれている。
本稿でも「移動済み」を「秘密無効化済み」と読み替えない。フォルダID・リンク・アカウントは掲載しない。
Discordの開発ログでは、Codexへの実装・レビュー対応委任と利用枠制約が実運用として現れる。
したがってハーネスは補助的な実験ではなく、開発の待ち時間と受入判断に直接影響する基盤として扱うべきである。

## 未確認事項

- 実際の28時間前スナップショット、並列10本の起動順、各実行の消費量。依頼者報告と静的実装の照合までである。
- 現在のCodex残量API・契約枠・アカウント間の共有範囲。個人セッションや認証設定は調べていない。
- Claude Codeの実効permissions、個人hook、MCPサーバー、外部の管理設定。個人設定は存在のみ確認した。
- 各メンバーのCLI実体・バージョン、Unity側packageとの互換性、接続中のEditor。起動・接続検査は行っていない。
- Unity compile、Test Runner、PlayMode、長時間QA、実機、製品buildの成否。今回の静的調査では合格を付けていない。
- `.claude/worktrees/`以外のworktree、過去の所有権競合、異常終了後のcleanup実績。Git操作をしていないため網羅していない。
- 個人メモリ索引のリンク先本文。昇格候補は索引記述から選び、恒久規則化前の再確認を条件にした。
- GitHubの現行ruleset・Issue状態・Actions実行結果。今回はローカルworkflowと既存レビュー・保存記録を使った。
- 秘密値の漏えい有無、トークン失効、外部サービスの実際の権限。既存記録以上の安全性を断定しない。
- Tempの終了時消去挙動は今回Unityを実行して再現していない。保存先設計上の注意として提案した。
- スキル参照検査は静的候補の存在確認。外部URL、動的パス、実行例の全組合せ、全references本文の意味検査は未実施。
- トークン数は文字数からの概算。実際の自動読込量・tokenizer・キャッシュ効果を計測していない。
