# Symphony Kill Chord — ドメイン突合 (2026-09-06)

Notion 公開仕様書 / `spec/feature/game-spec.md` (GS ベースライン) / Anatomia ビジネスドメイン /
Anatomia プログラムドメイン (コード) の 4 層を突き合わせた結果。

- 解析: Anatomia `project analyze symphonykillchord` — **1704 files / 5470 functions / 入口 1253**
- 仕様: Notion 公開ページ再クロール (`spec/notion/`、Canalis `notion-public` 使用)
- コード側の生データ: `./anatomia/` (program-domains / business-domains / screens / entrypoints)

---

## 1. 4 層の対応表

| Notion カテゴリ | 主な Notion ページ | GS | Anatomia ビジネスドメイン | コード (層別ファイル数) |
|---|---|---|---|---|
| システム | リズム判定 | GS-01 | Rhythm Input and Kill Chord Resolution | 25 (domain 10 / app 4 / adaptor 5 / view 4 / infra 1 / comp 1) |
| システム | 攻撃アクション・回避アクション・移動方法・カメラワーク | GS-02 | Player Action and Combat Targeting | 35 (domain 2 / app 4 / adaptor 12 / view 6 / infra 2 / comp 9) |
| システム | プレイヤー・敵・キャラクターステータス | GS-02 | Combat State and Effect Resolution | 85 (domain 31 / app 34 / adaptor 4 / view 10 / infra 6) |
| システム | スキル・改造画面・研究画面 | GS-03 / GS-06 | Progression, Research and Loadout | 95 (domain 23 / app 11 / adaptor 22 / view 18 / infra 16 / comp 5) |
| サウンド | BGM・SE・キャラクターボイス | GS-04 | Musical Time and Adaptive Arrangement | **17** (shared 1 / domain 5 / app 1 / adaptor 3 / view 3 / comp 4) |
| システム | ステージルール・ミッション・マップについて | GS-05 | Enemy Encounter and Stage Simulation | 118 (domain 11 / app 9 / adaptor 18 / view 55 / infra 10 / comp 15) |
| システム | ミッション・リザルト | GS-05 | Mission Evaluation and Result | 110 (domain 38 / app 8 / adaptor 19 / view 11 / infra 29 / comp 5) |
| ストーリー | メイン/サイドシナリオ・シナリオパート | GS-06 | Narrative and Game Flow | 100 (domain 19 / app 12 / adaptor 24 / view 20 / infra 17 / comp 8) |
| システム | 設定画面 | GS-07 | Persistence and Player Settings | 42 (shared 3 / domain 9 / app 4 / adaptor 4 / view 14 / infra 3 / comp 5) |
| システム | ホーム画面・作戦画面・インゲームHUD | GS-09 | Guidance, Feedback and Recovery | 94 (app 3 / adaptor 18 / view 64 / comp 9) |
| — | (品質・プラットフォーム制約) | GS-08 | **対応ドメインなし** | — |

---

## 2. 突合で出た不整合

### 2.1 ドメイン定義の `specRefs` が 18 本すべて解決しなかった → **修正済み**

`spec/domains/*.domain.json` の `specRefs` は英語見出し向けアンカーを指していたが、
`spec/feature/game-spec.md` の見出しは日本語で、生成されるアンカーが一致しなかった
(10 ドメイン × 計 18 参照が全滅)。**日本語見出しに合わせて 18 本すべて書き換え、
再突合で 18/18 解決を確認した。**

| 旧アンカー (解決しない) | 新アンカー (解決する) |
|---|---|
| `#gs-01-rhythm-judgment` | `#gs-01-リズム判定` |
| `#gs-02-rhythm-command-combat` | `#gs-02-リズムコマンド戦闘` |
| `#gs-03-kill-chord-skills-and-build` | `#gs-03-キルコードと編成` |
| `#gs-04-music-and-encounter-synchronization` | `#gs-04-音楽と遭遇の同期` |
| `#gs-05-stage-mission-and-result` | `#gs-05-ステージミッションリザルト` |
| `#gs-06-out-game-and-progression` | `#gs-06-アウトゲームと成長` |
| `#gs-07-persistence` | `#gs-07-永続化` |
| `#gs-09-navigation-guidance-and-recovery` | `#gs-09-導線ガイダンス回復` |

再突合の残件は 1 つだけ: **`gs-08-品質プラットフォーム制約` はどのドメインからも
参照されていない** (§2.4 と同じ理由 — 横断制約に所有ドメインが無い)。

> 見出しが日本語である以上、アンカーも日本語になる。今後 game-spec.md を英語見出しへ
> 戻す場合は domain.json 側も同時に戻さないと再び全滅する。片側だけの変更を防ぐため、
> specRefs の解決チェックを CI (Anatomia `spec_linkage`) の対象にすることを勧める。

### 2.2 ビジネスドメイン未所属の 169 ファイル → **二層のいずれかに所属していれば充足 (指摘取り下げ)**

Runtime の 842 ファイルのうち 169 (20%) はビジネスドメインに属さないが、
**プログラムドメインには全件が所属している** (`unclassifiedModules: 0` /
`unclassifiedSymbols: 0`、313 モジュール・1294 ファイル)。二層ドメインは
どちらか一方に属していれば規約上は充足するため、これは欠陥ではない。

参考として、ビジネスドメインを持たない側の分布 (QA の担当割当を決めるときの材料):

| プログラムドメイン | ファイル数 |
|---|---|
| `view:4.View/InGame` | 68 |
| `composition:6.Composition/InGame` | 19 |
| `adaptor:3.Adaptor/InGame` | 16 |
| `domain:1.Domain/InGame` | 8 |
| `application:2.Application/InGame` | 8 |
| `composition:6.Composition/Persistent` | 8 |
| `view:4.View/Persistent` | 7 |
| `shared:0.Utility/Persistent` | 6 |
| `adaptor:3.Adaptor/OutGame` / `view:4.View/OutGame` / `composition:6.Composition/OutGame` | 6 / 6 / 6 |
| その他 (`0.Utility/*`, `5.InfraStructure/*`, `6.Composition/Bootstrap`) | 11 |

中心はスキルエフェクト演出 (`4.View/InGame/Skill/Effect/*` 28)、アニメーション (10)、
カメラ計算 (8)、シーケンス (13)。**QA テストケースはビジネスドメインだけを見る**方針のため、
これらを対象にするケース (QA-VF-01〜03) は表示・演出を所有する機能側の
ビジネスドメイン (Combat State and Effect Resolution / Player Action and Combat Targeting /
Narrative and Game Flow) に寄せて起票した。

> なお全ドメインの `membership` は `Assets/Scripts/Runtime/[1-6]\.` を前提にしており、
> `0.Utility` 配下はビジネスドメイン側のパターンに構造的に当たらない。ビジネス側へ
> 寄せたくなった場合はここが効く。

### 2.3 48 ファイルが 2 ドメインに重複所属

HUD と リズムガイドが `Guidance, Feedback and Recovery` と機能ドメインの両方に入る。

| ファイル (`Assets/Scripts/Runtime/` 以下) | 所属 |
|---|---|
| `3.Adaptor/InGame/Mission/MissionHud{DTO,Presenter}.cs` | Guidance / Mission Evaluation |
| `4.View/InGame/Mission/MissionHud{View,ViewModel}.cs` | Guidance / Mission Evaluation |
| `3.Adaptor/InGame/Music/RhythmGuide{Dto,Presenter,ZoneDto}.cs` | Guidance / Rhythm Input |
| `4.View/InGame/Music/RhythmGuide{View,LabelView,UpdateView}.cs` | Guidance / Rhythm Input |

「HUD 表示は Guidance が所有し、値の正しさは機能ドメインが所有する」なら妥当だが、
Augur の quota はビジネスドメイン単位なので、**重複所属は同じテストが 2 ドメインの枠を
食う**。テスト登録時にどちらを主とするか決めておく。

### 2.4 GS-08 (品質・プラットフォーム制約) に対応するドメインが無い

60/30 FPS、ロード 3 秒以内、15 分無エラー・メモリリーク無し は、コード上の所有者が
いない横断制約。Augur では `kind: guardrail` (退役免除) として扱い、ドメインは
`(unowned)` ではなく計測対象ドメインへ紐づける運用にする。

### 2.5 Musical Time and Adaptive Arrangement のコード実体が薄い (17 ファイル)

仕様側は BGM を 4 小節 intro / 16 小節 loop / 4 小節 outro、CRI による 2 小節ブロック
差し替え、装備スキル数によるアレンジ順序と定義する (GS-04) のに対し、コードは 17 本。
`Assets/Scripts/Runtime/*/Persistent/Music` (adaptor 3 / view 7 / composition 5 の計 15 本) が
ドメイン未所属で残っており、**音楽同期の実装がドメイン境界の外に散っている**可能性が高い。

### 2.6 仕様側に残る矛盾・未確定 (game-spec.md より)

| ID | 内容 | QA への影響 |
|---|---|---|
| GS-02 矛盾 | ダメージ統一式が 2 ページで割れている。`攻撃アクション` (プレイヤー→敵) は攻撃力バフで**除算**、`敵` (敵→プレイヤー) は**乗算** | バフ・デバフを含む条件のダメージ期待値が決まらない (バフ無しなら判定できる) |
| GS-03 矛盾 | 装備枠拡張 (2→3) の条件が 3 系統 — 「6 個解放後の特別なノード」/「バフ・デバフ 1,2 と攻撃 2 を取得」/「ノードをすべて獲得した時」 | 枠拡張テストの前提が決まらない |
| GS-05 矛盾 | リザルト完了後の戻り先が Home / 作戦 で記述が割れる | 遷移テストの期待値が決まらない |
| GS-07 未解決 | schema version / migration / cloud save / 複数 profile / 破損復旧 が未指定 | セーブ破損・移行の QA 範囲が定義できない |
| GS-09 未解決 | data reset 確認、音 cue の代替、remap 範囲、可読性目標、チュートリアル完了基準 | アクセシビリティ QA の合否基準が無い |
| GS-10 未解決 | 回避の無敵時間・移動距離・クールタイムが `0.x` のまま | 回避テストの閾値が決まらない |
| GS-11 未解決 | `ボス戦闘` ページが本文空 | ボス戦の受入条件が書けない |
| GS-12 未解決 | マニュアルロックオンの対象選択が「HP 最小 **or** 距離最短」の or のまま | 同じ入力で結果が変わりうる |

> 旧 35 ページ版で挙げていた **GS-01 の拍種 6 vs 8 矛盾**と **GS-06 のランク計算式未取得**は、
> 296 ページ版で解消した (6 拍種で統一 / サブミッション達成数で C・B・A・S が確定)。

### 2.7 Anatomia 側の欠落

- **scene projection 未生成** — `spec/data/generated/anatomia/scene-manifest.json` が無く
  `anatomia scenes` が失敗する。シーン単位 (Unity Scene) の QA 割り当てができない。
- `screens` は 1253 entry から抽出できているが、Editor ツール
  (`AutoBuildWindow` 等) が `kind: dialog` として混ざる。QA 対象の画面一覧として使うには
  `Assets/Editor/**` の除外が要る。

---

## 3. 対応の優先順位

1. **2.1 specRefs のアンカー統一** — 対応済み (18/18 解決)。今後の再発防止として
   `spec_linkage` を CI ゲートに入れる。
2. **§4 GS ベースラインの更新** — 35 ページ由来のまま止まっており、仕様本体 296 ページとの
   差がドメイン↔仕様の被覆漏れになっている。最優先の残件。
3. **2.6 の矛盾 3 件の裁定** — 拍種・戻り先・ランク式。QA の期待値が決まらないので、
   テストケースは「保留」で起票してある (`qa-testcases.md` の `保留理由` 列)。
4. **§5 の 13 項目を CBT QA シートへ追加** — 対応済み。
5. **2.7 scene manifest 生成** — シーン単位の QA 割当のため。

> 2.2 (ビジネスドメイン未所属 169 ファイル) は、二層のいずれかに所属していれば充足という
> 方針により対応不要。QA 側はプログラムドメインキーで紐づける。

---

## 4. 仕様本体 (Notion 296 ページ) と GS ベースラインの被覆 → **更新済み**

仕様本体カテゴリ (システム / ビジュアル / ストーリー / サウンド) の再クロールで
**296 ページ / 6910 ブロック**を取得した (`spec/notion/spec-2026-09-06/`)。
`spec/feature/game-spec.md` は 35 ページ由来のままだったため、**296 ページ版で作り直した**
(旧版は `spec/feature/game-spec.prev-2026-07-18.md` に退避)。

### 更新内容

- GS-01〜GS-09 を現行仕様で書き直し
- **GS-10 プレイヤーの移動と回避** / **GS-11 敵と遭遇** / **GS-12 ターゲットとカメラ** /
  **GS-13 レベルデザインと難易度対応** / **GS-14 表現レーティング (ゴア表現)** /
  **GS-15 体験版 (TGS)** を新設
- ドメイン定義の `specRefs` に新節を配線 (18 → **23 参照、23/23 解決**)

### 母集団から除外したもの

Notion の `システム概要` 配下 (ターゲットシステム / カメラ / 音楽 / スキル効果 /
シークエンス / セーブ 等) は**実装モジュール設計書**であって遊びの規則ではないため、
GS には取り込まない。モジュール構成は Anatomia のプログラムドメインを正とする。
コード規定 / GitHub 運用規定 / 研修資料 / 最適化記事まとめ も同様に除外。

### 更新で解消したこと

| 旧版の記載 | 現行 |
|---|---|
| GS-01 矛盾「6拍種 vs 8拍種」 | **解消**。`コアメカニクスについて` から 12/16 拍の記述が消え 6 拍種で統一 |
| GS-06「ランク計算式が仕様に無い」 | **解消**。サブミッション達成数で メインのみ=C / +1=B / +2=A / +3=S |
| GS-02「非criticalダメージ = 攻撃力 × 攻撃時補正」 | ダメージ統一式 (Just 倍率・クリティカル倍率・攻撃/防御バフデバフ) に置換 |

### 更新で新たに出た矛盾・未解決

| ID | 種別 | 内容 |
|---|---|---|
| GS-02 | **矛盾 (新規)** | プレイヤー式は攻撃力バフで**除算**、敵式は**乗算**。「統一式に準拠」と書かれているのに一致していない |
| GS-05 | 矛盾 (継続) | リザルト戻り先。`リザルト` は Home 統一、`ステージルール` に両方が残る |
| GS-10 | 未解決 | 回避の無敵時間・移動距離・クールタイムが `0.x` のまま。用語には `DodgeDuration` / `DodgeCooldown` があるが値が無い |
| GS-11 | 未解決 | `ボス戦闘` ページが見出しのみで本文が空。歩兵・砲兵の個別パラメータも無い |
| GS-12 | 未解決 | ロックオン対象の選択規則 (優先順位・切替順・射程・画面外) |
| GS-06 | 未解決 | 「収集リスト」(スキル図鑑) に専用仕様ページが無い |

### 参照ドメインが無い GS

`GS-08 品質・プラットフォーム制約` と `GS-14 表現レーティング (ゴア表現)` は
どのビジネスドメインからも参照されていない。どちらも横断制約で、コード上の所有者が
いないため。QA では `kind: guardrail` として扱う。

---

## 5. 既存 CBT QA シートとの突合

`Docs/QA/CBT_QAシート_2026-09-06.md` (14 節 / 約 130 項目) は GS-01〜09 と
ビルド設定から作られており、本ドキュメントの QA ケースと大きく重なる。
**ドメイン側から見て、既存シートに対応項目が無いもの**は以下。

| 本書のケース | ドメイン | 既存シートの状況 |
|---|---|---|
| QA-CS-03 多段ヒット抑制 | Combat State and Effect Resolution | 無し (ドメイン定義に「多段ヒット抑制」と明記があるのに項目が無い) |
| QA-CS-04 バフ/デバフ/回復の一回限り適用 | 同上 | 6-4/6-5 は「効果を出す」のみで**重複適用の抑制**を見ていない |
| QA-PA-03 回避のクールダウン / キャンセル | Player Action and Combat Targeting | 無し (4-2 は回避ノーツの記録のみ) |
| QA-EN-02 ウェーブ / プール枯渇 / 二重スポーン | Enemy Encounter and Stage Simulation | 無し |
| QA-EN-03 ボス戦の一連 (出現〜ギミック〜撃破) | 同上 | 3-4 予告 / 5-14 モデル確認のみ。Notion「Bossシステム」82 ブロックに対して薄い |
| QA-EN-04 ステージ尺 約2分 | 同上 | 無し (GS-05 の目標値が検証されない) |
| QA-MT-04 長時間戦闘での拍ずれ蓄積 | Musical Time and Adaptive Arrangement | 無し (7-9 は Bluetooth 音ズレのみ) |
| QA-NA-03 シーン完了の一回性 | Narrative and Game Flow | 無し (ドメイン定義の明示責務。8-8/8-9 は報酬側のみ) |
| QA-NA-04 サイドシナリオの読了保存 | 同上 | 無し |
| QA-PR-04 スキルレベル上限 | Progression, Research and Loadout | 6-10 は表示一致のみで**上限で頭打ちになるか**を見ていない |
| QA-VF-01 スキルエフェクトの表示 | (ドメイン未所有 4.View/InGame/Skill/Effect 28 本) | 6-2 の「フィードバックがある」のみ |
| QA-VF-02 カメラワーク | (ドメイン未所有 4.View/InGame/Camera 11 本) | 5-13 の揺れ確認のみ |
| QA-VF-03 シーケンス演出の中断 | (ドメイン未所有 4.View/InGame/Sequence 13 本) | 無し |

逆に、既存シートにあって本書に無いもの (ストア掲載・法務、端末条件、テキスト表示、
バッテリー/発熱、画面回転など) は**ドメインを持たない運用・提出物側の項目**であり、
本書の対象外。両者は排他ではなく、**既存シートに上表の 13 項目を追加する**のが早い。
