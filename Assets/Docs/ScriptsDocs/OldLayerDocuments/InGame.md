# InGame 機能構造

`InGame` はゲームプレイ中に動く機能群です。現在の実装では、戦闘、カメラ、敵行動、音楽同期、プレイヤー移動、スキル判定が中心です。

## モジュール詳細

- **[InGame-Battle](./Modules/InGame-Battle.md)**: 攻撃計算、戦闘状態、結果表示。
- **[InGame-Camera](./Modules/InGame-Camera.md)**: カメラ追従、フリールック、ロックオン。
- **[InGame-Enemy](./Modules/InGame-Enemy.md)**: 敵の移動・攻撃・攻撃予約。
- **[InGame-Music](./Modules/InGame-Music.md)**: ビート同期、行動履歴、予約実行。
- **[InGame-Player](./Modules/InGame-Player.md)**: プレイヤー移動と回避。

## レイヤー構造

### 1. Domain
- **Battle**: `AttackDefinition`, `AttackId`, `AttackResult`, `AttackStepContext`, `IAttacker`, `IDefender`。
- **Character**: `CharacterEntity`, `CharacterCombatSpec`, `HealthEntity`, `AttackPower` など。
- **Camera**: `CameraParameter`, `CameraSystemParameter`, `ILockOnTarget`。
- **Enemy**: `EnemyMoveDecision`, `EnemyMoveSpec`, `EnemyMusicSpec`, `EnemyAttackMusicSpec`。
- **Music**: `RhythmDefinition`, `RhythmState`, `ScheduledAction`, `ExecuteRequestTiming`。
- **Player / Skill**: `PlayerMoveParameter`, `SkillDefinition`, `SkillPattern`, `SkillId`。

### 2. Application
- **Battle**: `AttackCalculator`, `AttackExecutor`, `AttackPipeline`, `CriticalStep`。
- **Camera**: `CameraSystemApplication` と、追従・回転ごとの小さな Application 群。
- **Enemy**: `EnemyMoveUsecase`, `EnemyAttackUsecase`, `EnemyAttackReservationUsecase`。
- **Music**: `MusicSyncService`, `IMusicSyncService`, `IMusicActionScheduler`。
- **Player**: `PlayerApplication`, `PlayerMovement`, `PlayerDodgeMovementApplication`。
- **Skill**: `SkillCheckService`, `ISkillRepository`。

### 3. Adaptor
- **Battle**: `AttackController`, `BattleController`, `AttackCommandState`, `AttackBattleState`, `AttackResultPresenter`, `DamagePresenter`。
- **Camera**: `CameraSystemController`, `TargetManager`, `LockOnTarget`。
- **Enemy**: `EnemyAIController`, `EnemyBattleState`, `EnemyMoveInstruction`。
- **Music**: `MusicSyncController`, `MusicSchedulerAdaptor`, `IMusicSyncViewModel`, `IMusicViewModel`。
- **Player / Skill**: `PlayerController`, `SkillController`。

### 4. View
- **Battle / UI**: `AttackResultView`, `AttackResultViewModel`, `IngameHudView`。
- **Camera**: `CameraSystemView`。
- **Enemy**: `EnemyMoveView`。
- **Music**: `MusicSyncView`, `MusicSyncViewModel`, `MusicViewModel`。
- **Player**: `PlayerView`, `PlayerAttackInputView`。
- **Scene**: `IngameSceneView`。

### 5. InfraStructure
- **Battle**: `AttackDefinitionData`, `AttackDefinitionFactory`, `AttackParameterSetData`, `AttackPipelineResolver`。
- **Character / Enemy / Player**: `CharacterFactory`, `EnemyFactory`, `EnemyMoveData`, `EnemyMusicData`, `PlayerConfig`。
- **Camera**: `CameraSystemConfig`。

### 6. Composition
- **起動**: `IngameComposition`。
- **機能初期化**: `BattleCompositionInitializer`, `MusicSyncInitializer`, `PlayerInitializer`, `CameraSystemInitializer`, `SkillInitializer`。

## 現在の実装メモ

- 戦闘まわりは `AttackPipeline` や `AttackPipelineResolver` が存在しますが、現行の `AttackController.ExecuteAttack()` は `AttackExecutor.Execute()` を直接呼ぶフローです。
- スキル判定は `SkillController` が `MusicSyncService` に戦闘履歴を積み、`SkillCheckService` でパターン一致を確認する構成です。
