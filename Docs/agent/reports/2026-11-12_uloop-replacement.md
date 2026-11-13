# uloop を Unity CLI と MCP に置き換える調査（2026-11-12）

## 結論

- uloop にしか無い機能は**ある**。コード行で止めて変数を取る `pause-point`、実行中のコード差し替え `hot-reload`、動画の録画 `record-video`、入力の記録再生 `replay-input`、キーボード・マウスの入力再現 `simulate-keyboard` / `simulate-mouse-input` / `simulate-mouse-ui`、UI要素の座標付きスクリーンショットの6系統。
- ただし、このプロジェクトで実際に使ってきたのは compile・launch・run-tests・Play の操作・ログ取得・C# の実行（QA の攻撃キュー）で、これらは Unity CLI（`unity` コマンド＋`com.unity.pipeline`）ですべて代わりが利く。上の6系統は、レポートでスキル一覧として名前が出るだけで、作業に使った記録は見つからなかった。
- 「コンパイル確認は `uloop launch` でエディタを起動する」の方針は、`unity open`（GUIのエディタ）→ `unity status --until-ready` → `unity recompile` に置き換えられる。
- MCP については、Unity AI Assistant（`com.unity.ai.assistant`）に入っているエディタ内の MCP サーバーは、2.20 で非推奨になった。後継は Unity CLI に入っている `unity mcp`。Unity 公式のスキルは「シェルが使えるなら MCP ではなく `unity command` を使う」としていて、Claude と Codex はどちらもシェルが使えるので、主に CLI を使い、MCP は無くても困らない。

## 確認の方法と範囲

- リポジトリ（develop d32024d5）の uloop を含むファイルを `rg -i uloop` で全部洗い出した（98ファイル）。
- Unity CLI の機能は、Unity 公式のエージェント用スキル（GitHub `Unity-Technologies/skills` の `skills/unity-cli`。SKILL.md と references の build-run-test.md、integration-advanced.md、playmode-verification-loop.md）で確かめた。docs.unity.com はこの環境から開けなかったので、公式ドキュメント本体は検索結果の要約だけで確かめた。
- **未確認**: PC で実際に `unity` コマンドを動かしていない（PC は別の作業で使用中）。このプロジェクトの `com.unity.pipeline` は `0.8.0-exp.1` で、下の対応表のコマンド名は最新版のもの。古い版にあるかどうかは、移行時に `unity command --detail full --format json` で確かめる必要がある。

## uloop を使っている場所

| 場所 | 使い方 | 移行で要る作業 |
|---|---|---|
| `Packages/manifest.json` / `packages-lock.json` | Unity 側パッケージ `io.github.hatayama.uloopmcp` | 外す |
| `.claude/skills/uloop-*`（18本）と `.agents/skills/uloop-*`（18本） | uloop ツールが入れたスキル | 消す。代わりに `unity skill install claude-code --local` などで公式の `unity-cli` スキルを入れる |
| `scripts/harness/unity-preflight.mjs`（＋テスト） | Claude の hook。`uloop compile / run-tests / hot-reload / control-play-mode` の前に、`uloop execute-dynamic-code` で未保存の Scene・Prefab と Play 中を調べて止める | 見張るコマンドを `unity recompile` / `unity test` / `unity command test run` / `unity command editor.playmode start` に変え、調べる手段を `unity command script eval_file` に変える |
| `.agents/skills/skc-ai-debug-attack-queue/scripts/qa-transport.mjs` | QA の攻撃キュー・観測。C# を書いたファイルを `uloop execute-dynamic-code --code-file` で送る | `unity command script eval_file <file> --project-path <root>` に変える。呼ぶ先の `Assets/Editor/AIDebugPlay`（プロジェクトのコード）は uloop に依存していないので、そのまま使える |
| `qa-aging.mjs`・`SKILL.md`・`references/json-api.md` | 証跡の置き場 `.uloop/qa/`、ロック `.uloop/qa-session.lock`、`ULOOP_EXECUTABLE` などの説明 | 置き場の名前を変え（例 `.unity-qa/`）、説明を書き直す |
| `.gitignore` | `.uloop/` を無視 | 新しい置き場に合わせる |
| `scripts/harness/check_harness.py`（＋テスト） | `uloop-` で始まるスキルを外部のものとして検査から外す | 外部スキルの扱いを `unity-` 系に合わせる |
| `AGENTS.md` | 「compile・run-tests・hot-reload・Play の前に preflight」「run-tests には `--unsaved-changes fail`」 | `unity` のコマンド名に書き換える。`--unsaved-changes fail` に当たる引数は CLI に無いので、preflight で止める形だけにする |
| `.agents/skills/skc-feature-flow/references/pull-requests.md` | 確認方法の例に `uloop-compile` | `unity recompile` に書き換える |
| `.claude/skills/skc-issue-report/SKILL.md` | description に「skc-/uloop- skill run」 | 文言を直す |
| `Docs/agent/` の過去レポート・引継ぎ（11本） | 当時の記録 | 直さない（記録として残す） |
| Notion「システムリスト」 | 開発ツールの行に「Unity Loop MCP (uloop-*)」 | 移行後に Unity CLI へ書き換える |

CI（`BuildAndRelease.yml`）は Unity を `-batchmode` で直接起動していて、uloop は使っていない。`Assets/` のコードにも uloop への参照は無い。

## 機能ごとの対応

○ は Unity CLI の組み込みで代わりが利く。△ は `script eval` で C# を書けば代わりになるが、組み込みのコマンドは無い。× は代わりが無い。

| uloop | Unity CLI での代わり | 判定 |
|---|---|---|
| `launch` | `unity open <project>`（GUI で開き、開いたまま残る）。再起動は `unity close` → `unity open` | ○ |
| `compile` | `unity recompile --project-path <project> --format json`。`--strict` で警告も失敗にできる | ○ |
| `run-tests` | 開いているエディタには `unity command test run`（長いときは `--detach` と `unity job wait`）。エディタを閉じていれば `unity test <project> --mode EditMode`（バッチで起動） | ○（`--unsaved-changes fail` は無い） |
| `control-play-mode` | `unity command editor.playmode start / pause / stop`、状態は `unity command editor status` | ○（1フレーム送りは版によって確認が要る） |
| `get-logs` / `clear-console` | `unity command console get` / `console clear` | ○ |
| `execute-dynamic-code` | `unity command script eval "<C#>"` / `script eval_file <file>` | ○（公式の説明は「版によっては無い」。0.8.0-exp.1 で要確認） |
| `find-game-objects` / `get-hierarchy` | `unity command gameobject find` / `scene get_hierarchy` | ○ |
| `screenshot` | `unity command capture game_view`（幅・高さ・カメラ・保存先）/ `capture scene_view` | ○（UI要素の座標を書き込む機能は無い → △） |
| `set-game-view-size` | 組み込み無し。撮影時の幅・高さ指定はできる。Game ビューそのものの大きさは `script eval` でリフレクション | △ |
| `simulate-keyboard` / `simulate-mouse-input` / `simulate-mouse-ui` | 組み込み無し。`script eval` で Input System にイベントを積むか `ExecuteEvents` を呼ぶ C# を書けば作れる。QA のボタン操作は既に `AIDebugQaApi` にある | △ |
| `replay-input` | 無し（記録と再生の仕組みを自作する必要がある） | × |
| `record-video` | 無し（Unity Recorder パッケージを入れれば録画はできるが、別の仕組み） | × |
| `pause-point` | 無し（コードを書き換えず、行で止めて変数を取る機能）。代わりは IDE のデバッガか、ログを足して再コンパイル | × |
| `hot-reload` | 無し。`script eval` で実行中の値を変えることはできるが、メソッド本体の差し替えはできない | × |

Unity CLI にしか無いもの: プロジェクトの Unity バージョンを読んで正しいエディタを選ぶ `unity open`、テストの失敗分だけの再実行（`--rerun-failed`）・変更の影響を受けたテストだけの実行（`--affected`）、エディタのモーダルダイアログへの応答（`editor dialog`）、`[CliCommand]` 属性での自作コマンド、ビルド（`unity build`）。

## コンパイル確認の手順（`uloop launch` の代わり）

PowerShell で、PC のメインのクローンに対して行う。

```powershell
$p = 'C:\Users\takut\GameProject\SymphonyKillChord'
unity open $p                                                   # GUI のエディタを起動（開いたまま残る）
unity status --until-ready --project-path $p --format json      # 読み込みが終わるまで待つ
unity recompile --project-path $p --format json                 # 0=成功、6=コンパイルエラー、7=エディタに届かない
```

注意点:

- 起動した時点でコンパイルエラーがあると、エディタはセーフモードで立ち上がり、Pipeline が読み込まれないので `unity recompile` は 7 で終わる。そのときは `unity status --project-path $p --format json` の `safeMode.compileErrors` にエラーの一覧が出るので、それをコンパイル確認の結果にする。
- エディタがフォーカスを持っていないと Play が進まないことがある。Play で確かめるときは公式の手順（`editor set_auto_tick` と `editor.playmode set_run_in_background`）に従う。
- 開いた Play やエディタは、終わったら `unity command editor.playmode stop` と `unity close $p` で閉じる（PC の後片付けの方針どおり）。

## 決めてほしいこと

1. uloop にしか無い6系統（pause-point・hot-reload・record-video・replay-input・入力の再現・座標付きスクショ）を手放してよいか。
   - ⭐ 手放す。作業に使った記録が無い。入力の再現が要る場面が出たら、そのとき `[CliCommand]` で小さなコマンドを足す。
   - 入力の再現（キーボード・マウス）だけ、先に `[CliCommand]` で作ってから消す。
2. 移行の進め方。
   - ⭐ feature-flow で1本にまとめる（preflight と QA の通信の書き換え、check_harness、AGENTS.md などの文言、uloop スキル36本と uloop パッケージの削除、`.gitignore`）。最後に PC の実機で compile・EditMode テスト・Play・QA スナップショットを1回ずつ確かめる。
   - 先に preflight と QA だけ切り替え、uloop の削除はしばらく様子を見てから別の PR で行う。
3. `com.unity.pipeline` を `0.8.0-exp.1` から最新版に上げるか。
   - ⭐ 上げる（`unity pipeline upgrade`）。`script eval`・`console get`・`capture` などが古い版に無いおそれがある。Unity 6000.3.10f1 は対応範囲内。
4. MCP をリポジトリに登録するか。
   - ⭐ 登録しない。Claude・Codex はシェルで CLI を使えるので、`.mcp.json` にツールの定義を増やす必要が無い（起動時のコンテキストも増えない）。使いたい人は各自 `unity mcp configure` で入れる。
   - `unity mcp` を `.mcp.json` に登録する。
   - あわせて、非推奨になったエディタ内の MCP を持つ `com.unity.ai.assistant` は、AI Assistant の他の機能を使っていなければ外せる。使っているかどうかは確認が要る。
