# レビュー・テスト計画

ステータス: partial（提案済み・未実行）。今回の静的解析ではruntime testを実行していない。

## 現在の証拠

追跡対象の `Assets` treeには `[Test]`、`[TestCase]`、`[UnityTest]` 属性がない。ファイル名に `Test` を含むものは15件あるが、test assembly definitionは確認できない。NUnit参照は研究prototype内の未使用constraints namespaceだけである。release workflowはbuildするがtestを実行しない。

## 優先characterization test

1. 正常payloadを保存し、対象置換の途中で中断しても、旧saveまたは新saveのどちらかが読み取れることを確認する。
2. 壊れたJSONと旧version JSONを読み込み、progressを暗黙に初期化せず、失敗を明示することを確認する。
3. 全enemy attack sourceが意図したattack definitionから攻撃力を読み、固定inputに対して同一のdeterministic damageを出すことを確認する。
4. stage完了時、build／unlock rewardが一度だけ付与・保存され、transition再実行で複製されないことを確認する。
5. 複数nodeのskill-tree pathを解放し、cost、point減算、unlocked node、unlocked skillがatomicに保存されることを確認する。
6. beat境界inputを与え、judgment window、scheduled attack、pause／resume動作を確認する。
7. Play Modeへ入る前に全stage、mission、skill、attack asset referenceを検証する。
8. 指定PC／Android device profileで、PC ≥60 FPS、Android ≥30 FPS、in-game load ≤3秒、15分間errorなし・重大なmemory増加なしを測定する。

純粋なdomain ruleはUnity EditMode test、scene composition、input timing、asset loading、persistence integrationはPlayMode testで扱う。いずれかが失敗した場合、packaging前にCIを失敗させる。
