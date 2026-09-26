# 機械テストのテストランナー設計 (2026-09-06)

`spec/analysis/2026-09-06/qa-testcases.md` で **実行者 = 機械 / 機械+人間** とした 37 ケースを、
Unity Test Framework で回すための構成。Augur 台帳 (`.augur/tests.config.json`) から
`augur tests run` で起動できる形に合わせる。

## 1. 前提 (すでに揃っているもの)

| 項目 | 現況 |
|---|---|
| `com.unity.test-framework` | **1.6.0** (`Packages/manifest.json`) |
| `com.unity.test-framework.performance` | **3.2.0** — GS-08 の計測に使う |
| `com.unity.ext.nunit` | 2.0.5 |
| Runtime の asmdef | 層ごとに 7 本 (`KillChord.Utility` / `.Domain` / `.Application` / `.Adaptor` / `.View` / `.InfraStructure` / `.Composition`) |
| テストコード | **無し** (`Assets/Tests/` が存在しない) |

層ごとに asmdef が切られているので、**Domain だけを参照するテストアセンブリ**が作れる。
これが EditMode テストを速く保つ前提になる。

## 2. アセンブリ構成

`Assets/Tests/` を新設し、3 本の asmdef を置く。

```
Assets/Tests/
  EditMode/
    KillChord.Tests.EditMode.asmdef      -- references: Domain, Utility, Application, TestFixtures
    InGame/     Rhythm/ Combat/ Music/ Mission/ Enemy/
    OutGame/    SkillBuild/ SkillTree/ StageSelect/
    Persistent/ Savedata/
  PlayMode/
    KillChord.Tests.PlayMode.asmdef      -- references: 上記 + Adaptor, View, Composition, InfraStructure
    InGame/     Sequence/ Music/ Enemy/
    OutGame/    Flow/
    Persistent/ Savedata/
  Fixtures/
    KillChord.Tests.Fixtures.asmdef      -- テストダブルとビルダー。Domain のみ参照
```

| asmdef | `includePlatforms` | 目的 |
|---|---|---|
| `KillChord.Tests.EditMode` | `Editor` | 純粋ロジック。シーンも `MonoBehaviour` も使わない |
| `KillChord.Tests.PlayMode` | (空 = 全プラットフォーム) | シーン・時間・永続化が絡むもの。実機でも走らせる |
| `KillChord.Tests.Fixtures` | (空) | 両方から参照するビルダーとフェイク |

3 本とも `"optionalUnityReferences": ["TestAssemblies"]` (UTF 1.6 では
`"references"` に `UnityEngine.TestRunner` / `UnityEditor.TestRunner`、
`"precompiledReferences": ["nunit.framework.dll"]`、`"defineConstraints": ["UNITY_INCLUDE_TESTS"]`) を付ける。

**EditMode が View / Composition を参照しないこと**を構成で強制する。参照が必要になった
時点で、それは PlayMode のテストである。

## 3. どのケースをどちらで回すか

| 分類 | 置き場所 | 対象ケース |
|---|---|---|
| **EditMode** (純粋ロジック・シーン不要) | `Assets/Tests/EditMode/` | QA-RH-01〜06 (拍種判定 / All ノーツ / Just / 硬直 / コマンド照合 / タイムアウト破棄)、QA-CS-01〜04 (ダメージ式 / クリティカル / 多段ヒット抑制 / 状態効果の重複)、QA-PA-01〜03 (射線 / 武器選択 / 回避 CD)、QA-PR-01/02/04 (末尾ノーツ競合 / 枠拡張 / レベル上限)、QA-ME-03/04 (ランク判定 / 戻り先)、QA-EN-02 (ウェーブ順序) |
| **PlayMode** (シーン・時間・永続化) | `Assets/Tests/PlayMode/` | QA-CS-05 (HP0 で戦闘終了)、QA-MT-01〜04 (BGM 構成 / アレンジ順 / ブロック差し替え / 拍ずれ蓄積)、QA-NA-03/04 (シーン完了の一回性 / 読了保存)、QA-PS-01/03 (5 分類の永続化 / 破損復旧)、QA-PR-03/05 (研究リセット / ステージ解放と報酬)、QA-GU-01 (ポーズと再開)、QA-ME-05 (再出撃) |
| **Performance** (計測、`performance` package) | `Assets/Tests/PlayMode/Performance/` | QA-QP-01〜04 (PC 60FPS / Android 30FPS / ロード 3 秒 / 15 分安定) |

`機械+人間` の 8 ケースは、**値の検証だけを上表のどちらかに置き**、提示 (表示・音) の
確認は CBT QA シート側に残す。二重に数えない。

## 4. テストを書けるようにするための seam

現状のコードのままだと決定的に書けないものがある。**テストの前に seam が要る**もの:

| 必要な seam | 理由 | 影響するケース |
|---|---|---|
| **時刻の注入** (`IClock` 相当) | リズム判定は「前回入力との時間差」で拍種を決める。`Time.time` 直参照だと EditMode で再現できない | QA-RH-01〜04, QA-RH-06 |
| **乱数の seed 固定** | クリティカルは `クリティカル率 %` の確率。seed を固定できないと期待値が書けない | QA-CS-02 |
| **音楽時間の純関数化** | BPM・小節・拍位置の計算を CRI 再生から切り離す。`MusicTimingCalculator` が既にあるので、そこへ寄せる | QA-RH-01, QA-MT-01〜04 |
| **セーブ入出力の抽象** | `SaveStore` は静的 API。テンポラリディレクトリへ差し替えられないと永続化テストが実データを壊す | QA-PS-01, QA-PS-03 |
| **射線判定の分離** | 現状 `LineOfSight` に相当する記号が Runtime に見つからない (`spec/plan/07-spec-gaps.md`)。**実装そのものが先** | QA-PA-01 |

seam が無いものは「テストが書けない」ではなく **`spec/plan/07-spec-gaps.md` の実装課題**として扱う。
テストのためにプロダクトコードを歪めない。

## 5. 命名と台帳の印

- テストクラス: `<対象クラス>Tests` (例 `RhythmJudgmentDefinitionTests`)
- テストメソッド: `<対象>_<条件>_<期待>` (例 `TryResolveBeatType_入力間隔が半拍_8拍子を返す`)
- **Augur 台帳と突き合わせる印**をファイル先頭のコメントに置く:
  `// @augur test:<testId> plan:<planId>` — `augur tests register --from-plan <planId>` が
  これで照合する
- QA ケース ID も併記する: `// QA-RH-01`

## 6. 起動方法 (CI / Augur から)

```
Unity.exe -runTests -batchmode -projectPath . \
          -testPlatform EditMode|PlayMode \
          -testFilter <NUnit フィルタ> \
          -testResults <path>/results.xml \
          -logFile -
```

`.augur/tests.config.json` の runner 定義はこれに合わせてある:

```jsonc
"runners": {
  "unity": {
    "command": ["Unity.exe", "-runTests", "-batchmode", "-projectPath", ".",
                "-testPlatform", "EditMode",
                "-testResults", "TestResults/editmode-results.xml", "-logFile", "-"],
    "selectorFlag": "-testFilter",
    "reporter": "nunit3"
  },
  "command": {}
}
```

`TestRecord.runner` は列挙値 (`vitest` / `cargo` / `gtest` / `unity` / `command`) なので
**`unity-playmode` のような runner 名は足せない**。`unity` を EditMode 既定とし、
**PlayMode と Performance のテストは `runner: "command"` で argv を明示して登録する**
(`-testPlatform PlayMode` と別の `-testResults` を持たせる)。

- `Unity.exe` は PATH に無いので、**バス側で絶対パスに解決する**
  (`augur.config.json` の `buses` に Unity のインストールパスを持つ wrapper バスを足す)。
  Augur はバス越しに shell 無しで spawn するため、`npx` 相当のシムは使えない。
- `-testPlatform` は EditMode / PlayMode で 2 回起動する。Augur のバンドルは
  **プラットフォームごとに分ける** (`domain:<name>` の実行が 2 プロセスになる)。
- 終了コードは UTF の規約 (0 = 全 pass、2 = 失敗あり)。Augur 側は
  `--for-revisor` のときだけ failed → 1 に写像する。

## 7. 結果の取り込み

- UTF は **NUnit3 XML** を出す。Augur の `RunRecord` へは
  `testcase@fullname` → 台帳の `name`、`result` → passed/failed、`duration` → ms で写す。
- 1 実行 = 1 `runId`。`report <runId>` で判断者向けにまとめ、`verdict` で採否を記録する。
- **`--for-revisor` は CBT では使わない**。CBT の合否は人が `verdict` で記録する。

## 8. 段取り

1. `Assets/Tests/` と 3 本の asmdef を作る (テスト 0 本でもビルドが通ることを先に確認)
2. §4 の seam のうち **時刻注入と音楽時間の純関数化**を先に入れる。ここが最多のケースを塞いでいる
3. EditMode の critical から書く — QA-RH-02 (All ノーツ 3 条件)、QA-RH-05 (コマンド照合)、
   QA-CS-01 (ダメージ式)、QA-PR-01 (末尾ノーツ競合)
4. `augur tests register --from-plan <planId>` で台帳へ載せ、`augur tests run --bundle domain:<name>` を通す
5. Performance は最後。実機 (Android) が要るので CI ではなく手元のバスで回す

**QA-PR-02 (装備枠拡張) は仕様側の矛盾が解けるまで書けない** — 枠が 2→3 になる条件が
「6 個解放後の特別なノード」「バフ・デバフ 1,2 と攻撃 2 を取得」「ノードをすべて獲得した時」の
3 系統に割れている (`spec/plan/07-spec-gaps.md` の決定事項 2)。
**QA-CS-01 (ダメージ式) はバフ・デバフ無しの条件なら書ける**。攻撃力バフを含む条件だけが、
プレイヤー式の除算と敵式の乗算の矛盾 (同 決定事項 1) が解けるまで保留になる。
QA-PR-01 (末尾ノーツ競合) は仕様に具体例があるので保留ではない。
