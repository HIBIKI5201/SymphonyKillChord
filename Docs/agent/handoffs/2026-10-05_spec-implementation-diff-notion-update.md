# 引き継ぎ資料: 仕様書(Notion)と実装の差分チェック・仕様書更新作業

作成日: 2026-10-05
元の作業環境: `C:\Users\kouki\SymphonyKillChord`、ブランチ `feature/demo/outGameUI/master`（HEAD `f0cbf828b`、2026-09-19）

## 1. 依頼の内容と現在地

- 依頼: 「現在の仕様書とゲーム内容を見比べ、仕様書の方を更新していく作業に協力してほしい」。範囲は「全部」（プロジェクト全体）。
- 完了済み:
  1. Notion仕様書エクスポートを 2026-09-24 に最新化（ユーザーが実施）。
  2. `Assets/Scripts/Runtime` 全体を10ドメイン(G1〜G10)に分割して差分調査（並列エージェント）。
  3. 統合レポート作成: `Docs/agent/reports/2026-09-24_spec-implementation-diff.md`（実質的な差分18件）。
  4. 16ページ分の「仕様書への追記・修正内容」を整理（本資料の §3）。
- 未着手: **Notion仕様書への実際の書き込み**。ユーザーからは「各ページに必要な情報を整理してほしい」まで依頼済みで、どのページから書き込むかの指示はまだ無い（直前の質問「優先度高の4件から／ページ単位」は未回答）。

## 2. 別PCで再開するための準備

1. **ファイルの取得**: 本資料と統合差分レポート（`Docs/agent/reports/2026-09-24_spec-implementation-diff.md`、全18件の根拠つき詳細）は `Docs/agent/` 配下にあり、ブランチ `feature/tools/specification-manual/notion` に含まれる。別PCではこのブランチをチェックアウトすれば揃う。前回レポート（`Docs/仕様書と実装の差分分析_2026-08-23.md`、書式の前例）は `Docs/` 直下で git 管理外のため、必要なら手動でコピーする。
2. **仕様書エクスポートの再取得**（`Docs/NotionSpecifications/` は git 管理外で同期されないため）: リポジトリルートから実行する。`SinfoniaOperator/` 配下で実行すると別ディレクトリに出力される。
   ```bash
   ./SinfoniaOperator/NotionMarkdownExporter/bin/Release/net10.0/win-x64/publish/NotionMarkdownExporter.exe --output "Docs/NotionSpecifications"
   ```
   `NOTION_TOKEN` は gitignore 済みの `SinfoniaOperator/sinfonia-operator.settings.json` か環境変数で設定が必要（トークンはこの資料に書かない）。
3. **Notionへの書き込み**: `notion-spec-write` スキル（`SinfoniaOperator/NotionMarkdownWriter.exe`）を使う。流れは pull → 編集 → dry-run → 確認 → push。書き込みには許可リストと競合処理があるため、スキルの手順に従うこと。push前には必ずユーザーに確認する。
4. **ブランチ運用**（`AGENTS.md`）: 編集・コミットは作業ブランチ（`feature/○○/○○/agent`）で行い、`master` で直接編集しない。

## 3. 仕様書ページごとの追記・修正内容（16ページ）

行番号は 2026-09-24 取得のエクスポート時点のもの。再取得後はずれる可能性があるので、書き込み前に必ず該当箇所を再確認すること。パスは `Docs/NotionSpecifications/Symphony Kill Chord/` からの相対。

| # | ページ | 追記・修正内容 | 根拠 |
| --- | --- | --- | --- |
| 1 | `システム概要/システムリスト/スキル効果.md` | :432の「要確認」を解消。スキル2〜10のエフェクトプレハブは約126,025〜126,031バイトでほぼ同一（仮素材の使い回し濃厚）、skill_00/01/13は個別に作り込み済み。エフェクト担当への確認依頼を追記 | `Assets/Level/Prefabs/Master/InGame/SkillEffect/SkillEffect_skill_*.prefab`、`Assets/Level/Data/Master/InGame/Skill/Effect/SkillEffectCatalogConfig.asset` |
| 2 | `システム概要/システムリスト/状態効果（バフ・デバフ）.md` | :347の「要確認」を解消。スキル7のマスター値（`SkillData07.asset:66`、`Ignore`）は使われず、コードで`Replace`が固定されている。マスター項目が死んでいる旨も注記 | `AttackPowerIncreaseBuff.cs:20`、`AttackPowerReductionDebuff.cs:17`（`2.Application/InGame/Buff/`）、`StatusEffectReapplyPolicy.cs:16` |
| 3 | `システム概要/システムリスト/キャラクターアニメーション.md` | :335の「要確認: Reserved枠の用途」を解消。`Reserved`は敵の攻撃予約中（構え）専用のブレンド枠 | `4.View/InGame/Animation/CharacterAnimationView.cs:66-77`、`4.View/InGame/Enemy/EnemyMoveView.cs:269` |
| 4 | `システム概要/システムリスト/音楽.md` | クラス表に`MusicConstants`（BPM・拍・再生速度換算の基礎定数）を追加。`RhythmDefinition`/`MusicTimingCalculator`/`MusicSyncState`/`SkillInitializer`が依存 | `0.Utility/Constant/MusicConstants.cs:5-33`、`6.Composition/InGame/Skill/SkillInitializer.cs:195` |
| 5 | `システム概要/システムリスト/シーンマネージャー.md`（または適切な既存ページ） | `IngameSceneView`（インゲーム中の追加シーンのロード・アンロード）を追記。View層に直置きで「後でレイヤー分けする」と自認された設計上の既知課題として記載 | `4.View/InGame/Scene/IngameSceneView.cs:11-13,22-55` |
| 6 | `仕様概要/仕様リスト/アウトゲーム遷移フロー.md` | :249の「保留（要実機確認）」を解消。Home/StageSelect/SkillTree/SkillBuildの4画面すべてから設定画面を開ける。`作戦画面.md:251`との矛盾も解消 | `4.View/OutGame/Screen/OutGameUIEvent.cs:47`、`ScreenInitializer.cs:543-548`（`6.Composition/OutGame/Screen/`） |
| 7 | `システム概要/システムリスト/UI構築フロー.md`（独立ページ化も検討） | OutGameナビゲーション基盤を追記。`SpatialNavigationResolver`（非格子レイアウト向けの方向解決）、`HierarchicalNavigationScope`、`ModalNavigationScope`、`UIActivationEvent`/`UINavigationExtensions`、`NavigationDebugLog`（一時コード） | `4.View/OutGame/Navigation/` 配下（各 :8〜:18付近） |
| 8 | `仕様概要/仕様リスト/改造画面.md` | 「未実装の項目」にレベルアップ時の効果音欠落を追加（仕様:90は正しいが実装が`Select`/`SkillSet`のみ）。実装側の修正はIssue化を推奨 | `3.Adaptor/OutGame/Audio/UISoundEffectKind.cs:6-13`、`SkillBuildUseCase.cs:56` ほか |
| 9 | `仕様概要/仕様リスト/研究画面.md` | (a) プレビュー動画ボタンが常時非表示になっている点を「未実装・一時無効」に追記（コミット`c6349c4c5`、2026-09-15）。(b) :165-167の「要確認」を解消、全49ノードで`_hasSkillSlotBonus`が未設定のため編成枠は増えない（データ修正の起票が必要）。(c) 任意: Tips表示が改造画面（構造化）と異なり単一テキスト埋め込みである点 | `4.View/OutGame/SkillTree/SkillDetailScreenView.cs:96-97`、`5.InfraStructure/OutGame/SkillTree/SkillNodeData.cs:36`、`Assets/Level/Data/Master/OutGame/SkillTree/SkillNodeData/SkillNodeData-N7.asset` |
| 10 | `仕様概要/仕様リスト/シナリオパート.md` | :396-406の決定（テスト用・旧版CSVを`develop`フォルダへ移動、#2078）に「2026-09-24時点で未実施」の進捗注記を追加 | `Assets/StreamingAssets/ScenarioAuthoring/` に`develop`フォルダなし |
| 11 | `システム概要/システムリスト/入力.md` | :65,237の「読む処理が無い」を「ゲームロジックとしての消費者は無い（Developビルド限定の`InputDebugLogger`が購読）」に精緻化 | `Assets/Scripts/Develop/Composition/Persistent/Input/InputDebugLogger.cs:16-17` |
| 12 | `仕様概要/仕様リスト/ローカライズ.md`（または新規システムリストページ） | `LocalizationInitializer`（初期化完了待ち）と`LocalizedElementText`（UI連携。21ファイルから参照）を追記 | `4.View/Persistent/Localization/LocalizationInitializer.cs:10`、`LocalizedElementText.cs:9` |
| 13 | `システム概要/システムリスト/SourceDataProvider.md` | `ScriptableObjectRepositoryBase<TId,TValue,TEntry>`（9種のRepositoryが継承する共通基底）を追記 | `5.InfraStructure/Repository/ScriptableObjectRepositoryBase.cs:12` |
| 14 | `用語.md` | `DataID`の用語エントリを追加（正式名・別名・コード上の名前） | `0.Utility/Identity/DataID.cs:11` |
| 15 | `システム概要/システムリスト/Behavior Graphについて.md` | :126-186の「作成済みノード一覧」表に`WaitForDiscoveryAction`（雑魚敵の索敵待機。`EnemyAIGraph.asset`から3箇所で参照、ボス用グラフには無し）を追加 | `4.View/InGame/Enemy/BehaviorGraphNode/Action/WaitForDiscoveryAction.cs:11,24-46` |

（実装パスは `Assets/Scripts/Runtime/` からの相対。#1・#2・#9はマスターデータ参照を含む。）

### 仕様書側の変更が不要な項目（実装側の修正が必要）

- `仕様概要/仕様リスト/コンボについて.md`: 仕様は「3コンボ以上で表示（現状の実装は4）」と既に正確。実装側（`6.Composition/InGame/Mission/InGameMissionInitializer.cs:449` と `Assets/Level/Scenes/Master/InGame.unity:57729` の`_comboVisibleCount: 4`）を3に直す作業。

### 参考: 優先度の高い4件

G6-C1（プレビュー動画ボタンの一時無効化。上記#9a）、G6-C2（編成枠拡張フラグ未設定。#9b）、G5-B1（ナビゲーション基盤。#7）、G1-C1（スキル2〜10の仮素材。#1）。

## 4. 調査で分かったこと（再調査の手間を省くため）

- 仕様書の対象ページの多くは 2026-09-22〜23 にチーム側で実装ベースに一斉改訂されていて、**全体の整合性は高い**。差分は「更新をすり抜けた項目」と「更新後の新しい実装変更」に限られる。
- 前回レポート（2026-08-23）の B-1〜B-6、B-10、C-1 は解消済みを確認。C-2（`SkillHitScheduler`）は今回別観点で矛盾なし。B-7〜B-9（カメラシェイク、敵HUDのプール化、ロードTips）は今回個別には再検証していない。
- G3（ミッション・シークエンス・プレイヤー・リザルト）は差分なし。
- 方針・企画系文書（企画概要書、α/β版開発指針など）に実装との矛盾は無し。
- 調査エージェントの出力のうち、コミット番号・行番号などの根拠は各エージェントの報告に基づく。書き込み前に重要なものは再確認すること。

## 5. 再開時の手順（推奨）

1. 上記のファイルをコピーし、エクスポートを再取得する（§2）。
2. 実装がこの間に変わっている可能性があるため、§3の各項目について根拠の行番号・状態を再確認する（`git log --since=2026-09-19 -- Assets/Scripts/Runtime` で変更量の目安を取れる）。
3. ユーザーに着手順を確認する（優先度の高い4件から／ページ単位）。
4. `notion-spec-write` スキルで、1ページずつ pull → 編集 → dry-run → ユーザー確認 → push。
5. 書き込み後、差分レポートの該当項目に対応済みの印を付ける。

## 6. このプロジェクトでユーザーが求めている進め方（メモリより）

- 実装タスクは必ず先にプランを書く。
- 画面の見た目を変える作業は、実装前にビジュアルのモックを出す。
- PlayModeでの自己検証はしない（確認はユーザーに依頼して報告を受ける）。
- OutGame UIをリファレンス画像から作る場合は `Assets/Arts/Images/Sprites/UI/OutGame` 内の既存アセットのみを使う。
