# InGame-Enemy

敵の移動・攻撃・音楽同期予約を扱うモジュールです。

## 構造概要

### 1. Domain
- **EnemyMoveDecision / EnemyMoveSpec**: 移動判断と移動設定。
- **EnemyMusicSpec / EnemyAttackMusicSpec**: 攻撃予約に使うタイミング定義。

### 2. Application
- **EnemyMoveUsecase**: 移動ロジック。
- **EnemyAttackUsecase**: 攻撃ロジック。
- **EnemyAttackReservationUsecase**: `IMusicActionScheduler` を使った予約管理。
- **IEnemyNavigationAgent**: ナビゲーション抽象。

### 3. Adaptor
- **EnemyAIController**: 敵の状態を見て移動や予約を指示。
- **EnemyBattleState**: 戦闘状態管理。
- **EnemyMoveInstruction**: View 向けの移動指示。

### 4. View
- **EnemyMoveView**: 移動と見た目の反映。

### 5. InfraStructure
- **EnemyFactory / EnemyMoveData / EnemyMusicData**: 設定と生成。

### 6. Composition
- **EnemyTestSpawner**: テスト用スポーンと参照注入。
- **EnemyMoveDebugInitializer**: デバッグ用初期化。

## 現在の実装メモ

- `EnemyAttackReservationUsecase` は `ReserveEncounter()` と `ReserveBattle()` を持ち、既存予約をキャンセルしてから新規予約を入れます。
- 名前空間は `KillChord.Runtime.Application.InGame.Player` になっており、ファイル配置と一致していない点に注意が必要です。
