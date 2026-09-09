# Symphony Kill Chord — 仕様起点ビジネスドメイン提案とコード突合

作成日: 2026-08-24

## 1. 結論

ビジネスドメインの正本候補は、2026-07-18 の公開仕様35ページを基に定義済みの次の10境界を
継承するのが妥当である。

1. Musical Time and Adaptive Arrangement
2. Rhythm Input and Kill Chord Resolution
3. Player Action and Combat Targeting
4. Combat State and Effect Resolution
5. Enemy Encounter and Stage Simulation
6. Mission Evaluation and Result
7. Progression, Research and Loadout
8. Narrative and Game Flow
9. Persistence and Player Settings
10. Guidance, Feedback and Recovery

2026-08-24 のコード起点13候補は、この10境界を置き換えるものではない。コード配置を探すための
実装スライスとして有用だが、複数の仕様責務を同居させる候補と、製品ドメインではない技術区分を
含むためである。ビジネスドメインは上記10件、プログラム構造は `.anatomia/layers.json`、
コード探索用スライスは13候補、と三者を分けて管理する。

## 2. 使用した証拠

### 2.1 現在の公開状態（2026-08-24 Canalis再クロール）

起点:
`https://lying-foxglove-81a.notion.site/Symphony-Kill-Chord-27d7c2c6cc02801d9648fbe2769f1971`

- ルートから同一Notionサイト内のページ候補32件を発見した。
- 一度の完走で取得できた本文は3ページだった。
  - `Symphony Kill Chord`
  - `トンマナ`
  - `キャラクタービジュアル`
- 29件は `.notion-page-content` が表示されず、非公開・権限外・Notion内部参照・一時challengeを
  区別できない状態だった。
- 既知の公開ページ `仕様概要` は単独アクセスでは表示できたため、29件すべてを非公開とは
  判定できない。連続クロール時のNotion側challengeも含まれる。
- 現在取得できた本文は、仕様カテゴリの目次とビジュアル方針が中心であり、戦闘・成長・保存・
  ミッション等の境界を単独で確定するには不足している。

現在の公開ルートは、仕様体系を次のカテゴリへ分けていることを確認できる。

- システム: 仕様概要、システム概要、用語、マスターデータ
- ビジュアル: デザイン、トンマナ、キャラクタービジュアル
- ストーリー: メイン/サイドシナリオ、背景/世界観/キャラクター設定
- サウンド: BGM、SE、キャラクターボイス
- 管理用: アセットデータベース

`トンマナ` は「音楽に乗って戦う体験を最優先し、視覚負担を減らす」「必要な時だけ情報を
提示する」と規定している。この内容は独立した製品ドメインというより、
`Guidance, Feedback and Recovery` に課す表示ポリシー/品質属性である。

### 2.2 完全性のある既存スナップショット

`Docs/G-Lab/Symphony Kill Chord-Spec/spec/plan/00-source-manifest.md` は、2026-07-18 に公開ルート
`仕様概要` から発見した35/35ページをCanalis `NotionPublicSource`で取得した記録を持つ。
同スナップショットから仕様起点10ドメインを再定義した正本候補が
`Docs/G-Lab/Symphony Kill Chord-Spec/spec/plan/04-domain-model.md` である。

現行公開クロールが部分取得のため、本提案では次の証拠順位を採用する。

1. 2026-07-18の公開仕様35/35スナップショット: 境界定義の主要証拠
2. 2026-08-24の全体クロールで取得できた3ページ（確定保存2ページ）: 仕様体系とビジュアル方針の現行確認
3. 2026-08-24のAnatomiaコード解析: 実装存在・規模・配置の突合証拠

## 3. ビジネスドメイン候補

### Musical Time and Adaptive Arrangement

BPM、拍子、小節位置、ビート同期予約、イントロ/ループ/アウトロ、装備中Kill Chordに応じた
BGM編成を所有する。コード側の主な対応は `rhythm-music` だが、入力判定責務は含めない。

### Rhythm Input and Kill Chord Resolution

攻撃/回避入力間隔のノーツ判定、All/Just/timeout、Kill Chord列照合と発動可否を所有する。
`rhythm-music` と `skill-runtime` の双方に実装が分散するため、フォルダ単位で一括所有しない。

### Player Action and Combat Targeting

移動、照準、攻撃、回避、硬直、クールダウン、キャンセル、遮蔽物を含む標的選択を所有する。
`combat-core` の一部と `platform-services` の入力変換部分が対応する。

### Combat State and Effect Resolution

HP、能力値、ダメージ、クリティカル、多段ヒット抑制、バフ/デバフ/回復を所有する。
`combat-core` と `skill-runtime` の効果解決部分が対応する。

### Enemy Encounter and Stage Simulation

敵判断、スポーン、プール、ウェーブ、予告付き攻撃、ボス、ステージギミックを所有する。
コード側の `enemy` と `stage-barrage` を統合して扱う。

### Mission Evaluation and Result

成功/失敗条件、進捗、経過時間、コンボ、ランク、報酬、リザルト確定を所有する。
コード側の `mission` が中心で、`ui-screens` のリザルト表示はadapterとして接続する。

### Progression, Research and Loadout

ステージ解放、報酬ポイント、研究ツリー、スキル獲得/レベル、装備枠、Kill Chord競合、
出撃編成を所有する。`skill-progression` を中心に `sortie-stageselect` の編成部分が対応する。

### Narrative and Game Flow

新規/継続、シナリオ、Home/作戦/準備/戦闘/リザルト遷移、シーン完了の一回性を所有する。
`scenario-narrative`、`sortie-stageselect`、`ui-screens` の画面遷移部分が対応する。

### Persistence and Player Settings

永続データ、保存、暗号化、移行、破損回復、音量/操作/表示設定の確定と取消を所有する。
`platform-services` のSave/Load部分と `ui-screens` のSetting部分が対応する。

### Guidance, Feedback and Recovery

リズム/BGM/敵攻撃/ミッションの可視化、チュートリアル、ポーズ復帰、失敗Tips、ロード進捗を
所有する。`presentation-fx` と `ui-screens` の一部が対応する。現行 `トンマナ` の低彩度・
必要時のみ強調という方針も、この境界の表示ポリシーとして扱う。

## 4. コード起点13候補との突合

| コード起点候補 | 対応する仕様起点ドメイン | 判定 | 調整方針 |
|---|---|---|---|
| combat-core | Player Action / Combat State | 分割必要 | 行動要求と戦闘効果解決を別所有にする |
| enemy | Enemy Encounter | 強一致 | そのまま主要implementorとして使える |
| mission | Mission Evaluation | 強一致 | Result表示はUI adapterとして分離する |
| skill-runtime | Rhythm Input / Combat State | 分割必要 | 成立判定と効果解決を別所有にする |
| skill-progression | Progression | 強一致 | 研究・装備・所有を同じ境界で扱う |
| rhythm-music | Musical Time / Rhythm Input | 分割必要 | 音楽時計と入力判定を分離する |
| stage-barrage | Enemy Encounter | 統合 | encounter/stage simulationの下位機能にする |
| presentation-fx | Guidance | 部分一致 | 技術名ではなくcue/recoveryルールで所有する |
| scenario-narrative | Narrative and Game Flow | 強一致 | シナリオ再生と全体遷移の責務を明示する |
| sortie-stageselect | Progression / Narrative and Game Flow | 分割必要 | 編成確定と画面遷移を別所有にする |
| ui-screens | Guidance / Narrative / Persistence | 横断adapter | UIを単独ビジネスドメインへ昇格しない |
| platform-services | Persistence / Player Action / Narrative | 技術横断 | input/save/sceneを各仕様境界へ割り当てる |
| spec-tooling | 対応なし | 製品外 | `external-tooling`プログラム区分として管理する |

## 5. 仕様から見たギャップ

### コード候補が粗すぎる境界

- `rhythm-music` は音楽時計と入力判定を混在させる。
- `combat-core` はプレイヤー操作と戦闘状態解決を混在させる。
- `skill-runtime` はKill Chord成立と成立後の効果を混在させる。
- `ui-screens` は複数ビジネスドメインのadapterを一つに束ねた技術分類である。

### コード候補だけに現れる区分

- `spec-tooling` は製品のビジネスドメインではない。
- `presentation-fx` は独立ドメイン候補ではなく、Guidanceの実装基盤として扱うのが安全である。
- `platform-services` は複数ビジネスドメインを支える基盤であり、単独の意味境界ではない。

### 仕様側で横断的に要求される境界

- Persistence and Player Settings
- Guidance, Feedback and Recovery
- Narrative and Game Flow

これらは単一フォルダに閉じず、複数のView/Adaptor/Infrastructure/Compositionへ分散する。
Anatomiaのowner割当はパスだけでなく型名・責務・spec clauseを根拠に行う必要がある。

## 6. 推奨する正式化方針

1. `spec/domains/` に仕様起点10ドメインを正式化する。
2. 各定義に、目的・責務・in/out boundary・不変条件・根拠specを記録する。
3. コード起点13候補はドメイン名としてそのまま採用せず、implementor探索用の対応表として残す。
4. `.anatomia/layers.json` で構造レイヤーを別途全域分類する。
5. `presentation-fx`、`ui-screens`、`platform-services`、`spec-tooling` を製品ビジネスドメインへ
   誤昇格させない。
6. 公開取得できなかった29リンクは、公開設定またはCanalisの認証付き取得経路を整備して再取得し、
   2026-07-18スナップショットとの差分を確認する。

本書は提案・突合結果であり、`spec/domains/` への適用や人間承認を代行しない。
