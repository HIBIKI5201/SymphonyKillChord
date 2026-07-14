# InGame スキルモジュール設計監査レポート（2026-07-11）

## 1. 監査概要

`Assets/Scripts/Runtime` 内の InGame スキルモジュールを対象に、現在の責務分割・依存方向・拡張点の作り方を調査した。

今回の結論は次の通りです。

- 現在のスキルモジュールは、**見た目上は `InGame.Skill` という層構造を持っているが、実際の所有者は `PlayerInitializer` に偏っている**。
- そのため、スキル機能は「独立モジュール」ではなく、**プレイヤー初期化の一部実装として埋め込まれている**。
- また、**効果実装のデータ化方式と依存注入方式が噛み合っておらず**、新スキル追加時に安全に拡張しにくい。

## 2. 全体評価

### 良い点

- `SkillDefinition` / `SkillPattern` / `SkillCooldownTime` など、基礎的な値型・定義型は比較的小さく分かれている。  
  `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\1.Domain\InGame\Skill\SkillDefinition.cs:8`
- 入力進捗 UI は `State -> Presenter -> View` に分かれており、この部分は拡張の土台になりうる。  
  `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\3.Adaptor\InGame\Skill\SkillUI\SkillInputProgressController.cs:9`

### 総評

ただし、現状は「土台の一部がある」段階で、**モジュール全体としての拡張契約は未整理**です。  
特に問題なのは次の 4 点です。

- モジュール境界が曖昧
- スキル効果のデータ化と実行方式が不整合
- 実行失敗時の意味論が不安定
- Dead code / 仮コード / 未使用契約が多い

## 3. 主な問題点

### A-1. スキルモジュールの所有者が曖昧で、実際には `PlayerInitializer` に吸収されている

スキル専用の Composition として `SkillInitializer` が存在する一方で、中身は未実装に近く、実際の構築処理は `PlayerInitializer` 側に集中している。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Skill\SkillInitializer.cs:18`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Skill\SkillInitializer.cs:32`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Player\StageSceneInitializer.cs:16`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Player\StageSceneInitializer.cs:22`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Player\PlayerInitializer.cs:227`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Player\PlayerInitializer.cs:280`

#### 問題

- スキルを追加・差し替えしたい時に、専用モジュールを触るのではなく `PlayerInitializer` を編集する必要がある。
- プレイヤー専用実装になっており、将来「敵も同じ入力型スキルを使う」「NPC が同一ロジックを使う」時に再利用しづらい。
- `StageSceneObjects` が `SkillInitializer` を公開しているが、現実にはそこが機能していないため、設計と実装が乖離している。

#### 推奨

- `SkillInitializer` を本当にスキルモジュールの初期化単位に戻す。
- `PlayerInitializer` は `SkillModuleContainer` を受け取る側に寄せる。
- `StageSceneObjects` にぶら下がっている旧式参照は整理する。

---

### A-2. リポジトリ契約があるのに、InGame 側では使われていない

`ISkillRepository` と `SkillRepository` は存在するが、InGame の実装フローではほぼ使われていない。  
実際のスキル構築は `SkillBuildDefinition` またはテスト用の `_equippedSkills` から直接行われている。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\InGame\Skill\ISkillRepository.cs:9`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\5.InfraStructure\Player\SkillRepository.cs:14`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Player\PlayerInitializer.cs:65`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Player\PlayerInitializer.cs:76`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Player\PlayerInitializer.cs:289`

#### 問題

- スキルデータ取得経路が 1 つに定まっていない。
- 「本番の装備スキル」「テスト用スキル」「OutGame 編成結果」の優先順位がコード上で暗黙化している。
- 将来ロード元を増やすたびに `PlayerInitializer` が肥大化する。

#### 推奨

- InGame のスキル構築入口は `ISkillLoadoutRepository` のような 1 契約に集約する。
- `SkillRepository` は「スキルマスター」、`LoadoutRepository` は「今装備しているスキル」という責務に分離する。
- `_equippedSkills` のようなテスト用経路は `Develop` へ退避する。

---

### A-3. スキル効果実装の配置が `InGame.Skill` に閉じておらず、モジュール境界がぶれている

スキルの実行本体は `InGame.Skill` にある一方、効果本体は `Application.Player.SkillEffect`、テンプレートは `Domain.Player` / `InfraStructure.Player` にあり、モジュール単位の追跡が難しい。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\InGame\Skill\SkillUseCase.cs:7`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\SkillBase.cs:7`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\1.Domain\Player\SkillTemplate.cs:10`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\5.InfraStructure\Player\SkillTemplateAsset.cs:13`

#### 問題

- 「Player のスキル」なのか「InGame のスキル」なのか責務境界が曖昧。
- スキル改修時の探索コストが高い。
- 将来プレイヤー以外の主体がスキルを使う場合、名称と依存が障害になる。

#### 推奨

- InGame で使うスキル実行系は `InGame.Skill` 配下へ寄せる。
- `Player` に残すのは「プレイヤー固有の発動入力」や「プレイヤー専用 View」だけに限定する。

---

### B-1. `SkillTemplateAsset` のデータモデルと、効果実装の生成方式が噛み合っていない

`SkillTemplateAsset` は `ISkillEffect` を `SerializeReference` で持つが、実際の具象クラスは引数付きコンストラクタに依存しており、データとして安全に扱いにくい。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\5.InfraStructure\Player\SkillTemplateAsset.cs:36`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\Skill_07.cs:13`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\Skill_02.cs:14`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\Skill_07.cs:88`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\Skill_02.cs:29`

#### 問題

- `ISkillEffect` 実装が `IBuff` や `IAttackController` をコンストラクタで要求しており、アセット定義時の managed reference と相性が悪い。
- スキル効果の一部は、生成時に必要な依存が満たされないまま残る可能性が高い。
- `Skill_02` のように `_attackController` を使うが初期化経路が見当たらない実装がある。

#### 推奨

- `SkillEffect` は「定義」と「実行器」を分ける。
- Asset に持たせるのは `SkillEffectSpec` のような純データにし、実行時に `Factory` で `ISkillEffectExecutor` を組み立てる。
- `IBuff` や `IAttackController` のようなランタイム依存は Asset に直接埋め込まない。

---

### B-2. `SkillEffectContext` が View/検索責務を持ち込み、しかも現在の実装では `null` が渡されている

`SkillEffectContext` は `IViewRepository` を持つが、この抽象は Domain 側に置かれており、しかも現状の `SkillUsecase` は `null` を渡している。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\1.Domain\Player\SkillEffectContext.cs:10`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\1.Domain\Player\IViewRepository.cs:10`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\InGame\Skill\SkillUseCase.cs:32`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\Skill_02.cs:23`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\Skill_03.cs:23`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\Skill_07.cs:22`

#### 問題

- Domain 側に「条件に合うキャラを探す」という View / Query 的責務が混ざっている。
- `context.Repository` 依存のスキルは、現状では `NullReferenceException` リスクを抱える。
- 範囲選択・直線選択・扇形選択などのターゲット取得戦略が、正式なモジュール契約になっていない。

#### 推奨

- `IViewRepository` は削除し、`ITargetQueryService` や `IAreaTargetResolver` のような Application 契約へ置き換える。
- `SkillUsecase` は対象解決サービスを受け取り、効果実行前に必要なターゲット群を整形して渡す。
- `SkillEffectContext` には「実行に必要な値」だけを残す。

---

### B-3. パターン一致後に実行失敗しても、クールダウン消費・履歴クリア・UI更新が走る

`SkillExecutionController` は、入力パターンが一致した時点で `SkillUsecase` の戻り値に関わらず後続状態を進めてしまう。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\3.Adaptor\InGame\Skill\SkillExecutionController.cs:62`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\3.Adaptor\InGame\Skill\SkillExecutionController.cs:70`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\3.Adaptor\InGame\Skill\SkillExecutionController.cs:72`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\InGame\Skill\SkillUseCase.cs:39`

#### 問題

- ターゲットが不在でスキル不発でも、クールダウンが入る。
- 入力履歴が消え、再試行もできない。
- UI 的には「発動した」ように見える余地がある。

#### 推奨

- `TryExecuteSkill` は `PatternMatched` と `SkillExecuted` を分けて扱う。
- 実行失敗時にクールダウン消費するかどうかを、スキルポリシーとして明示する。
- 返り値を `enum SkillExecutionResult` へ変えると拡張しやすい。

---

### B-4. `BattleActionType` を記録しているのに、判定には使っていない

履歴には `BattleActionType` を保存しているが、実際のマッチ判定は `BeatType` のみ。さらに呼び出し元は `Attack` だけです。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\1.Domain\InGame\Skill\SkillRhythmState.cs:39`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\1.Domain\InGame\Skill\SkillRhythmState.cs:90`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\InGame\Skill\SkillCheckService.cs:17`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\3.Adaptor\InGame\Battle\PlayerAttackController.cs:108`

#### 問題

- 「攻撃→攻撃→回避」のような行動種別込みのコンボスキルへ拡張できない。
- 履歴に持っている情報と判定規則がズレており、設計意図が不明瞭。

#### 推奨

- 本当に拍種だけで良いなら `BattleActionType` の保存をやめる。
- 将来行動種別込みにしたいなら、`SkillPattern` 自体を `BeatType + ActionType` の複合シグネチャへ拡張する。

---

### B-5. スキル View との対応付けが `int Id` とシーン配置に依存している

各スキルの visual は `SkillView[]` から `Id` で線形検索されており、Scene 上の配置と ID 手合わせに依存しています。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\4.View\InGame\Skill\SkillView.cs:7`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\6.Composition\InGame\Player\PlayerInitializer.cs:312`

#### 問題

- スキル追加ごとに Scene 上の View 配置と ID 同期が必要になる。
- 見つからないスキルは静かに `continue` され、欠落が実行時まで見えにくい。
- スキル演出をデータ側から差し替える余地が少ない。

#### 推奨

- `SkillViewRegistry` または `SkillVisualConfig` を作る。
- `SkillDefinition` / `SkillTemplate` 側が直接 `AnimationKey` と `VisualKey` を持ち、Composition で対応付ける。

---

### C-1. Dead code / 未使用契約 / 仮コードが多い

スキル周辺には、使われていない契約や暫定コードが残っている。

#### 根拠

- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\InGame\Skill\IViewAction.cs:8`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\1.Domain\InGame\Skill\SkillType.cs:6`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillVisual\SkillVisualTest.cs:9`
- `C:\Users\takut\GameProject\SymphonyKillChord\Assets\Scripts\Runtime\2.Application\Player\SkillEffect\TestSkillEffect.cs:11`

#### 問題

- 何が正式設計で、何が試作なのか判別しにくい。
- 新規実装者が誤って旧経路へ乗る可能性がある。

#### 推奨

- 未使用契約は削除する。
- 試作コードは `Develop` へ移す。
- 空 enum や死んだ interface は「今後使う予定」ではなく、必要時に追加する。

## 4. 優先度付き改善案

### 最優先

1. スキル構築責務を `PlayerInitializer` から分離し、専用 `SkillModuleContainer` を作る。
2. `SkillEffect` のデータ定義と実行器を分ける。
3. `SkillEffectContext` から `IViewRepository` を除去し、対象解決を Application 契約へ寄せる。
4. `SkillExecutionController` の実行結果を多値化し、不発時ポリシーを明示する。

### 次点

5. `ISkillRepository` / `LoadoutRepository` の役割を分け、スキル取得経路を一本化する。
6. `BattleActionType` を本当に使うか捨てるか決める。
7. `SkillView` / UI の対応付けを Registry 化する。

### 最後にやるとよいもの

8. Dead code を整理する。
9. `Skill_00` のような ID 命名をやめ、意味名へ改名する。
10. テスト用 `_equippedSkills` と本番用編成データの境界を明確にする。

## 5. 総括

現在の InGame スキルモジュールが拡張しにくい主因は、**スキルそのもののロジックよりも、モジュール境界と生成方式が整理されていないこと**です。

特に大きいのは次の 3 点です。

- スキルが独立モジュールとして閉じていない。
- 効果実装がデータと依存注入の両方を抱えている。
- 発動判定・対象解決・視覚演出の責務境界が曖昧。

今後は、

1. モジュール初期化の独立
2. 効果定義の純データ化
3. 対象解決サービスの正式契約化

の順で手を入れると、かなり拡張しやすくなるはずです。
