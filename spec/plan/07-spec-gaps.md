# Stage 5: 仕様ギャップ (2026-09-06 再作成)

**ステータス: 仕様側は 296ページ版で再照合済み。コード側は Anatomia の記号検索で確認できた範囲のみ更新。**

- 仕様の母集団: 公開Notion 仕様本体 **296ページ / 6910ブロック** (`spec/notion/spec-2026-09-06/`)
- 仕様の整理結果: `spec/feature/game-spec.md` (GS-01〜GS-15)
- コード観測: Anatomia `project analyze symphonykillchord` (1704 files / 5470 functions / 入口 1253)
- 旧版 (35ページ / 7-18 時点): `07-spec-gaps.prev-2026-07-18.md`

コード側の判定は **Anatomia の記号名検索**に基づく。名前が違うだけで実装が存在する可能性を
排除できないものは「記号名では確認できず」と書き、解消済みとは扱わない。

---

## 1. 旧ギャップの再判定

| 旧 Severity | Gap | 2026-09-06 の判定 | 根拠 |
|---|---|---|---|
| High | All ノーツ / 1.5小節 timeout 契約が実装経路に見つからない | **継続 (記号名では確認できず)** | 仕様は GS-01 で明確。コード側は `AllNote` / `AllNotes` / `RhythmTimeout` / `Timeout` のいずれもヒット無し。判定基盤 (`RhythmJudgmentDefinition` / `RhythmState.GetHistoryBeatType` / `TryResolveBeatType`) は Domain に存在するので、**別名で実装されているか未実装かの切り分けが要る** |
| High | player shot の line-of-sight が TODO | **継続** | 仕様は GS-02 で「障害物が無ければ必ず当たる」。`LineOfSight` はヒット無し、`Obstacle` は `DevelopProducts/Mocks/**` のカメラ処理のみ。`EnemyRaycastDetectService` は**敵側の探知**であって射線判定ではない |
| High | 装備連動 interactive BGM の wiring が無く test cue 再生 | **継続 (根拠が強化)** | `EquipmentBgmService` は `Assets/DevelopProducts/Research/EquipmentBGM/Scripts/` にのみ存在し、**research-prototype 層で本編 Runtime に統合されていない**。仕様は GS-04 で 2小節ブロック差し替えと装備数別の並び順まで確定している |
| High | stage reward crediting 経路が見つからない | **解消** | `StageProgressSaveDataService.GrantReward` (`2.Application/Persistent/Savedata/`) と `SaveAndLogRewardAsync` が Runtime に存在 |
| High | save の skill level / options / encryption / atomic recovery が不足 | **一部継続** | `SaveData` (`1.Domain/Persistent/Savedata/`) と `SkillBuildData.SetEquipmentSkillIDs` は Runtime にある。ただし **暗号化は `DevelopProducts/Research/SaveSystem/**/SaveCryptoUtility.Encrypt` の research 層のみ**で本編未統合。migration / 破損復旧は仕様側にも無い (GS-07 未解決) |
| High | enemy damage に `new Damage(10)` 経路 | **仕様は解消・コードは要再確認** | 仕様は GS-02 に敵側ダメージ式が明記された (`攻撃力 ×(1+攻撃バフ-デバフ)×(1+防御バフ-デバフ)`、Just 倍率なし)。ハードコードの有無は記号検索では判定できない |
| High | PC/Android FPS、load、memory の自動証拠が無い | **継続** | 仕様は GS-08 で数値確定 (PC 60 / Android 30 / ロード 3秒 / 15分無エラー)。計測を残す仕組みが無い。受け皿は Augur の `kind: guardrail` と media-based testing |
| High | HUD 仕様が見出しのみ | **継続** | 296ページ版でも `インゲームHUD` は「戦闘UI」「リズムUI」の見出しのみで本文が無い。一方 GS-04 は「再生中の音楽要素を識別できる UI」を要求しており、**受入条件が書けない** |
| Medium | 同末尾ノーツの同時装備禁止が未強制 | **経路あり・強制内容は要確認** | `SkillBuildDefinition.ValidateEquipment` (`1.Domain/OutGame/SkillBuild/`) が存在。仕様は GS-03 に具体例つきで明記 (`A A B B` と `A C B` は不可) |
| Medium | rhythm source が 6拍種と 8拍種で矛盾 | **解消** | `コアメカニクスについて` から 12/16 拍の記述が消え、専用ページと同じ **6拍種 (1/2/3/4/6/8)** で統一された |
| Medium | result 完了後の遷移先が Home / Operation で矛盾 | **継続** | `リザルト` ページは「完了 = ホーム画面に移動」で統一。`ステージルール` ページに「ホームに戻る」と「作戦画面に戻る」が併記されたまま |
| Medium | skill-tree 効果・slot unlock に TODO | **一部解消** | `1.Domain/OutGame/SkillTree/` に `CriticalChanceAdditionEffect` / `CriticalDamageMultiplierAdditionEffect` 等の効果実装が存在。slot unlock (6個解放後の特別ノードで枠 2→3) の強制は未確認 |
| Medium | tutorial は順序列挙のみで prompt fade / skip / help / 転移判定が無い | **一部解消** | 296ページ版でフローが具体化 (移動 → 攻撃(ロックオン確認 / 1・2・4・8拍子 / 3・6拍子) → 回避 → キルコード確認 → 敵撃破)、**再プレイ可**も明記。ただし prompt の fade、skip、完了基準は依然として無い |
| Medium | title の data reset に確認 / backup / 取消 / 結果通知が未記載 | **継続** | 296ページ版にも記載が無い |
| Medium | accessibility (字幕・色覚・振動・remap・文字可読性) が未定義 | **継続** | コンフィグに `判定調整機能` の見出しが増えたが本文が無い。音 cue の視覚代替は GS-04 の「BGM 要素表示 UI」だけで、accessibility として体系化されていない |
| Medium | auto lock の優先規則に「HP または距離」の未決定が残る | **一部解消・核心は継続** | `カメラワーク` ページに規則が書かれていた — オートは「ダメージを与えた敵」を対象、無ければ直線に弾が飛ぶ、視点操作/移動操作で解除。マニュアルは専用ボタンで発動しマニュアル優先。**ただしマニュアルの対象選択が「最も HP が低い or 距離が近い」の or のままで未決定**。射程・画面外の扱いも未記載 |
| Medium | mutable 外部 ZIP を checksum 無しで import | **要再確認** | 記号検索の対象外。ビルド設定側の確認が要る |

## 2. 296ページ版で新たに出たギャップ

| Severity | Gap | Impact |
|---|---|---|
| **High** | **ダメージ統一式が 2 ページで矛盾**。`攻撃アクション` (プレイヤー→敵) は攻撃力バフで**除算**、`敵` (敵→プレイヤー) は**乗算**。両方が「統一式に準拠」と書いている | バフの符号が逆になり、バランス調整とテスト期待値が定まらない |
| High | `ボス戦闘` と `ゲーム起動時機能` のページが**見出しのみで本文が空** | ボス固有仕様と起動時の受入条件が書けない。CBT のステージにボスが含まれる |
| **High** | **装備枠拡張 (2→3) の条件が 3 系統に割れている** — ①`スキル`「6 個解放後の特別なノード」/ ②QAシート §15 の照合結果「バフ・デバフ 1,2 と攻撃 2 を取得」/ ③`研究画面`「ノードをすべて獲得した時にスロット拡張へ繋がるパス」 | 3 枠になる条件が確定せず、成長導線と QA の期待値が決まらない |
| Medium | 回避の性能値が `0.x 秒` / `0.x m` のまま未定 (無敵時間・移動距離)。用語に `DodgeDuration` / `DodgeCooldown` の定義はある | コードには `1.Domain/InGame/Character/DodgeCooldown.cs` があるため、**値の正本がコード側にしか無い** |
| Medium | 歩兵 / 砲兵 / ボスの**個別パラメータが未記載** (見出しのみ) | 敵ごとの挙動を検証できない |
| Medium | 「収集リスト」(スキル図鑑・収集率) が `シーン` 定義に現れるが専用仕様ページが無い | 実装範囲が確定しない |
| Medium | ゴア表現がリリース版と TGS 版で異なる (鮮血・血だまり・死体配置 / 血が少し舞う程度) | **IARC 年齢レーティングの申告根拠が版で変わる**。どちらで申告するかを決める必要がある |
| Low | `n` / `m` のパラメータ変数が未定義のまま (プランナーが実装後に変更する方針) | ミッション条件・スキル効果の数値がテストで固定できない |

## 3. 決定が必要な事項

1. **ダメージ統一式の所有元を決め、除算/乗算のどちらかに揃える** (新規・最優先)。
1. **装備枠拡張 (2→3) の条件を 1 つに確定する** (新規・最優先。現在 3 系統)。
2. result 遷移先を `リザルト` ページ (Home) に寄せ、`ステージルール` の併記を消す。
   併せて reward 確定時点と再試行時の idempotency を一つの状態機械で定める。
3. save の version / per-skill level / 暗号化境界 / atomic replace / 破損復旧 /
   destructive reset 確認を一体で設計する。**暗号化は research 層から本編 Runtime へ移す**。
4. tutorial の prompt fade / skip / 完了基準を受入条件化する (フロー自体は確定済み)。
5. beat / hit / enemy telegraph の視覚・聴覚・触覚の冗長化と、設定・端末ごとの代替を定義する。
   HUD ページの本文が無い限り受入条件が書けない。
6. マニュアルロックオンの対象選択を「HP 最小」「距離最短」のどちらかに確定する (現在 or のまま)。射程・画面外の扱いも決める。
7. 回避の無敵時間・移動距離・クールタイムの値を仕様へ書き戻す (現在はコードが正本)。
8. ゴア表現をリリース版 / TGS 版のどちらで IARC 申告するか決める。
9. 装備連動 BGM と セーブ暗号化を **research 層から本編 Runtime へ統合**する期限を決める。

## 4. 検証の受け皿

- **コード側の未確認項目** (All ノーツ / line-of-sight / enemy damage ハードコード /
  slot unlock 強制 / ZIP checksum) は、記号名検索では判定できない。
  Anatomia の `where` / `context` か実コード読解で確定させる。
- **仕様が決まっていない項目**は QA の合否判定ができないため、
  `spec/analysis/2026-09-06/qa-testcases.md` の `保留理由` 列に落としてある。
- **GS-08 の性能要件**は Augur の `kind: guardrail` として台帳に載せ、
  media-based testing (外部キャプチャ + フレーム解析) で計測を残す。
