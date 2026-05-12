# InGame-Music

ゲーム中のビート同期を扱うモジュールです。再生中の経過時間に対して、予約されたアクションを所定のタイミングで実行します。

## 構造概要

### 1. Domain
- **RhythmDefinition**: BPM などの基礎定義。
- **RhythmState**: ビート履歴とタイミング計算。
- **ScheduledAction**: 実行予約されたアクション。
- **ExecuteRequestTiming**: どのタイミングで発火するかの指定。

### 2. Application
- **MusicSyncService**: スケジュール実行と履歴保存の中核。
- **IMusicSyncService**: 利用側向けインターフェース。
- **IMusicActionScheduler**: 予約用インターフェース。

### 3. Adaptor
- **MusicSyncController**: 再生時間を service に渡す。
- **MusicSchedulerAdaptor**: `EnemyMusicSpec` などから予約しやすい形へ変換。
- **IMusicSyncViewModel / IMusicViewModel**: 表示用データの抽象。

### 4. View
- **MusicSyncView / MusicSyncViewModel**: 同期状態の可視化。
- **MusicViewModel**: 再生中 cue 名などを保持。

### 6. Composition
- **MusicSyncInitializer**: `MusicPlayer` と同期サービスの接続。

## 現在の実装メモ

- `MusicSyncService` は `PriorityQueue<ScheduledAction, double>` を使って実行時刻順に予約を保持します。
- スキル判定用の行動履歴も `MusicSyncService.RegisterBattleActionHistory()` 経由でここに蓄積されます。
