# InGame-Battle

InGame における戦闘処理のモジュールです。現在の主フローは、攻撃入力を `AttackController` が受けて `AttackExecutor` でダメージを計算し、`AttackResultPresenter` で View に渡す流れです。

## 構造概要

### 1. Domain
- **AttackId / AttackDefinition**: 攻撃種別と攻撃性能。
- **AttackResult / Damage**: 計算結果。
- **AttackStepContext / IAttackPipeline**: パイプライン処理用の文脈と抽象。
- **IAttacker / IDefender**: 攻撃側と被攻撃側の抽象。

### 2. Application
- **AttackCalculator**: ダメージ計算。
- **AttackExecutor**: `AttackDefinition`, `IAttacker`, `IDefender` を受けて攻撃を実行。
- **AttackPipeline / IAttackStep / CriticalStep**: 段階的な攻撃処理を表現する仕組み。

### 3. Adaptor
- **AttackController**: 現行の主な攻撃入口。スキル判定と攻撃実行を担当。
- **AttackCommandState**: 選択中の `AttackId` を保持。
- **AttackBattleState**: 攻撃者とターゲットを保持。
- **BattleController**: 旧フロー寄りの補助コントローラー。
- **AttackResultPresenter / DamagePresenter**: ViewModel へ変換。

### 4. View
- **AttackResultView / AttackResultViewModel**: 攻撃結果の表示。

### 5. InfraStructure
- **AttackPipelineResolver**: `AttackId` ごとの `AttackPipeline` を引く。
- **AttackDefinitionData / AttackDefinitionFactory / AttackParameterSetData**: 攻撃定義のデータソース。
- **AttackPilpelineAsset**: パイプライン生成用 Asset。

### 6. Composition
- **BattleCompositionInitializer**: キャラ生成、戦闘状態、Presenter、Controller の結線。

## 現在の実装メモ

- `BattleCompositionInitializer` では `AttackPipelineResolver` を生成していますが、`AttackController` の実行フローにはまだ接続されていません。
- `AttackController.ExecuteAttack()` は `AttackBattleState.Attacker.CombatSpec.GetAttackDifinition(0)` を用いて固定的に攻撃定義を取得しています。
