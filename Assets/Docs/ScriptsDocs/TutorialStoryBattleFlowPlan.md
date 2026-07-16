# 特殊ノード連結ルール基盤設計計画

## 概要

現在のステージ選択では、ストーリーノードとバトルノードはそれぞれ単体で再生されます。  
この独立性は維持したまま、一部の特殊ケースだけ「あるノードの再生後に別ノードや別アクションへ自動接続する」仕組みを追加したいです。

当初の要件は「初回チュートリアル時に、特定ストーリー再生後に特定バトルへ進める」ですが、本設計ではそれをチュートリアル専用特例として埋め込まず、他の特殊処理にも再利用できる「特殊ノード連結ルール基盤」として構築します。

## 目的

- 通常時はノードを独立再生できる既存仕様を守る。
- 特定条件を満たした場合のみ、ノード再生後の後続アクションを差し込めるようにする。
- 初回チュートリアル以外にも、イベント、分岐演出、解放演出、将来の時限コンテンツへ流用できる構造にする。

## 現状整理

### 既存の遷移導線

- `StageSelectInitializer` は出撃時に `StageDefinition.StageType` を見て `Scenario` と `Battle` を分岐しています。
- `Battle` ノードは `SelectedBattleStageState` と `SelectedMissionState` を構築して `OutGameSortieController.RequestSortieAsync()` を呼びます。
- `Scenario` ノードは `SelectedScenarioState.SelectScenario()` を行ってから `OutGameSortieController.RequestSortieAsync()` を呼びます。
- シナリオ再生完了後は通常の終了導線へ戻る想定で、後続アクション予約の仕組みはありません。
- 既存の `TutorialSortieRequestState` は「タイトルからチュートリアルバトルへ直行する」専用用途です。

### 現状の問題

- 特殊導線を増やすたびに `StageSelectInitializer` や Scenario 側へ個別条件を埋め込むと、特例分岐が散らばります。
- 「ノード再生後に何をするか」という文脈を保持する共通 State がありません。
- チュートリアルだけを直接実装すると、後でイベント用の特殊導線を追加した際に設計を作り直す可能性が高いです。

## 設計方針

### 方針1: 特殊処理の軸を「チュートリアル」ではなく「ルール」に置く

- 個別機能名ではなく、「条件を満たしたら後続アクションを発火する」という抽象で扱います。
- チュートリアルはそのルールの 1 利用例として実装します。

### 方針2: 通常導線を汚さない

- 通常ノード再生は従来どおり単体で完結します。
- 特殊導線は、ルールが一致したときだけ一時的に有効になる pending action として扱います。

### 方針3: ルール判定と実行予約を分離する

- 「このノードに特別な後続処理があるか」を判定する責務と、
- 「判定結果として何を予約し、終了時にどう消費するか」の責務を分けます。

### 方針4: Action 種別を拡張可能にする

- 初回は `特定バトルへ遷移` だけでよいですが、構造上は以下を追加可能にします。
  - 別シナリオ再生
  - 特定画面表示
  - 特定演出開始
  - 報酬付与後に画面遷移

## 提案アーキテクチャ

### 1. NodeTransitionRule

- 役割: どの条件で、どの後続アクションを予約するかを表すルール。
- 配置候補: `1.Domain/OutGame/NodeTransition`

想定項目:

- `TriggerNodeType`
- `TriggerScenarioId` または `TriggerStageId`
- `ConditionSet`
- `ActionDefinition`
- `Priority`

このルールは「チュートリアル用」ではなく、ゲーム全体の特殊遷移ルールの基本単位として扱います。

### 2. NodeTransitionCondition

- 役割: ルールが有効になる条件を表す値オブジェクト群。
- 配置候補: `1.Domain/OutGame/NodeTransition`

初期実装で想定する条件:

- `TutorialNotCompletedCondition`
- `CurrentNodeScenarioIdCondition`
- `CurrentNodeStageIdCondition`

将来的に追加可能な条件:

- `StageClearedCondition`
- `FlagEnabledCondition`
- `DateRangeCondition`
- `OwnedItemCondition`

### 3. NodeTransitionActionDefinition

- 役割: 条件成立時に予約すべき後続アクション定義。
- 配置候補: `1.Domain/OutGame/NodeTransition`

初期実装で想定するアクション:

- `StartBattleActionDefinition`

将来的に追加可能なアクション:

- `PlayScenarioActionDefinition`
- `ShowScreenActionDefinition`
- `OpenRewardActionDefinition`

### 4. PendingNodeTransitionState

- 役割: 現在再生中ノードの終了後に発火すべき後続アクションを一時保持する State。
- 配置候補: `3.Adaptor/OutGame/NodeTransition`

想定 API:

- `void Reserve(NodeTransitionActionRuntimeData runtimeData)`
- `bool HasPending`
- `bool TryConsume(out NodeTransitionActionRuntimeData runtimeData)`
- `void Clear()`

これは汎用の pending 実行文脈であり、チュートリアル専用状態にはしません。

### 5. NodeTransitionRuleResolver

- 役割: 現在選択されたノード情報とゲーム状態から、適用対象のルールを解決する UseCase。
- 配置候補: `2.Application/OutGame/NodeTransition`

想定責務:

- 候補ルール群から適用可能なルールを 1 件選ぶ。
- 条件群を順に評価する。
- 成功時は ActionDefinition を runtime data に変換して返す。

### 6. NodeTransitionRuleAsset

- 役割: ノード連結ルールをインスペクタで設定する Asset。
- 配置候補: `5.InfraStructure/OutGame/NodeTransition`

構成案:

```text
NodeTransitionRuleAsset
- rules[]
  - triggerNodeType
  - triggerScenarioId / triggerStageReadableId
  - conditions[]
  - actionType
  - targetBattleStageReadableId
  - priority
```

## 初回チュートリアルをこの基盤で表す方法

初回チュートリアルは、以下の 1 ルールとして表現します。

- Trigger: 特定 `ScenarioId`
- Condition:
  - `TutorialNotCompletedCondition`
- Action:
  - `StartBattleActionDefinition(targetBattleStageId)`

つまりチュートリアル専用コードを散らすのではなく、特殊ノード連結ルールの 1 エントリとして定義します。

## 処理フロー案

### 1. ステージ選択でノードを出撃する

`StageSelectInitializer.HandleSortieRequested()` で、選択中ノードに対して `NodeTransitionRuleResolver` を問い合わせます。

- 適用ルールなし:
  - `PendingNodeTransitionState.Clear()`
  - 従来どおり通常出撃
- 適用ルールあり:
  - 返された runtime data を `PendingNodeTransitionState.Reserve(...)`
  - その後、元ノードを通常どおり出撃

重要なのは、特殊ルールがあっても「元ノードの再生自体は通常処理で始める」ことです。

### 2. ノード再生中

- `Scenario` なら `SelectedScenarioState` を従来どおり設定します。
- `Battle` なら `SelectedBattleStageState` と `SelectedMissionState` を従来どおり設定します。

ここでは pending action は実行せず、終了待ちです。

### 3. ノード再生完了時

Scenario 終了、または将来必要なら Battle 終了の完了地点で `PendingNodeTransitionState.TryConsume(...)` を確認します。

- pending なし:
  - 通常の終了導線へ戻る
- pending あり:
  - Action 種別に応じた Executor を呼ぶ

### 4. Action 実行

初期実装の `StartBattleActionDefinition` では以下を行います。

- 対象 `StageDefinition` を取得する
- `SelectedBattleStageState.SelectBattleStage(...)`
- `OutGameMissionSelectController.Select(...)`
- `OutGameSortieController.RequestImmediateBattleSortie(...)` または `RequestSortieAsync(StageType.Battle, ...)`

## 実装責務の割り当て

### Domain

- `NodeTransitionRule`
- `NodeTransitionCondition`
- `NodeTransitionActionDefinition`
- 必要なら `NodeTransitionActionType`

条件とアクションを型で分け、ルールそのものは pure に保ちます。

### Application

- `NodeTransitionRuleResolver`
- 必要なら `NodeTransitionActionRuntimeDataFactory`

Application 層で「現在のゲーム状態に対してどのルールが成立するか」を判定します。

### Adaptor

- `PendingNodeTransitionState`
- 必要なら `NodeTransitionActionExecutor`

Adaptor 層で、Application が返した runtime data を UI 遷移や既存 Controller 呼び出しへ橋渡しします。

### InfraStructure

- `NodeTransitionRuleAsset`
- Asset から Domain ルール群へ変換する Repository

### Composition

- `StageSelectInitializer`
  - 出撃時にルール解決を行う
  - pending action の予約を行う
- `ScenarioCom` または終了制御クラス
  - 再生終了時に pending を消費する
- 必要なら Battle 終了側
  - 将来 Battle 完了後アクションも扱う場合に同基盤へ接続する

## 既存クラスへの変更方針

### `StageSelectInitializer`

- `Scenario` 専用の特例分岐を直接書かない。
- 選択ノード情報を `NodeTransitionRuleResolver` に渡し、返り値があれば `PendingNodeTransitionState` に予約する。
- 予約に失敗した場合はログを出し、通常導線へフォールバックする。

### `ScenarioCom`

- シナリオ再生終了時に `PendingNodeTransitionState` を確認する。
- pending があれば ActionExecutor へ渡す。
- pending がなければ既存終了導線を維持する。

### `StageTree`

- `StageDefinition` 解決が各所へ散るなら、`TryGetDefinition(StageId, out StageDefinition)` の追加を検討する。
- これにより Action 実行時の接続先解決を共通化しやすくなる。

## データ設計案

### 最小構成

```text
NodeTransitionRuleAsset
- rules[]
  - triggerNodeType
  - triggerScenarioId
  - conditions[]
    - type: TutorialNotCompleted
  - action
    - type: StartBattle
    - targetBattleStageReadableId
  - priority
```

### 将来拡張例

```text
NodeTransitionRuleAsset
- rules[]
  - triggerNodeType
  - triggerStageReadableId
  - conditions[]
    - type: StageCleared
      stageReadableId: stage_05
    - type: FlagEnabled
      flagId: event_summer
  - action
    - type: PlayScenario
      scenarioId: scenario_event_opening
  - priority
```

## 衝突解決方針

複数ルールが同時成立する可能性があるため、以下のいずれかを採用します。

- `priority` の最大値を採用する
- 同 priority なら先勝ちにする
- 競合時は警告を出して先頭ルールだけ適用する

初期実装では `priority` 採用が分かりやすいです。

## 失敗時の扱い

- 接続先ノードが存在しない:
  - ログを出して pending を破棄し、元ノード終了後は通常導線へ戻る
- Action 実行に必要なデータが不足している:
  - ログを出して通常導線へフォールバックする
- シナリオ終了イベントが二重発火した:
  - `TryConsume()` により一度だけ処理する
- ノード途中離脱:
  - 画面離脱や再選択時に `PendingNodeTransitionState.Clear()` を呼ぶ

## テスト観点

### 正常系

1. 初回チュートリアル用ルールが成立すると、対象ストーリー終了後に対象バトルへ自動遷移する。
2. 同じルールでもチュートリアル完了後は成立しない。
3. 対象外ノードでは pending が予約されず、通常どおり終了する。
4. 複数ルール定義時に、もっとも高 priority のルールだけが採用される。

### 再利用確認

1. 条件を差し替えるだけで、別イベント用のストーリー→ストーリー遷移を追加できる。
2. Action 種別を増やしたとき、既存チュートリアルルールの実装へ影響が出ない。
3. `StageSelectInitializer` に個別イベント名の if 文を増やさずに特殊導線を追加できる。

### 異常系

1. ルール設定が壊れていても元ノード再生は継続できる。
2. pending が残った状態で別ノード選択を行っても、古い action が暴発しない。
3. 実行先の `StageDefinition` が `Battle` ではない場合、ActionExecutor が拒否して通常導線へ戻る。

## 実装手順

1. `NodeTransitionRule` / `NodeTransitionCondition` / `NodeTransitionActionDefinition` を追加する。
2. `NodeTransitionRuleAsset` と Repository を追加する。
3. `NodeTransitionRuleResolver` を追加し、選択ノードとセーブ状態から適用ルールを解決できるようにする。
4. `PendingNodeTransitionState` と ActionExecutor を追加する。
5. `StageSelectInitializer` に「ルール解決→予約」の流れを追加する。
6. `ScenarioCom` 側に「終了時に pending を消費して実行」の流れを追加する。
7. 初回チュートリアルルールを Asset に 1 件登録して動作確認する。

## 採用理由

- 特殊処理の追加先を「個別クラスの if 文」ではなく「ルール定義」に寄せられます。
- 初回チュートリアル要求を満たしながら、他イベントにも横展開できます。
- 通常導線と特殊導線の責務が分離され、保守時に影響範囲を読みやすくなります。

## 非採用案

### チュートリアル専用 State を追加する案

- 初回要件には最短ですが、次の特殊処理追加時に同じ問題を繰り返します。
- 命名と責務が用途固定になり、拡張性が低いため不採用です。

### `StageSelectInitializer` に特殊ケースを都度追加する案

- 一見簡単ですが、条件分岐が集中して肥大化します。
- 再生完了後の挙動も別クラスに散りやすく、保守コストが高いため不採用です。

### セーブデータに pending action を保存する案

- 今回必要なのは短命な実行予約であり、永続化は過剰です。
- 中断再開時の整合性設計まで必要になり、要件に対して重いため不採用です。
