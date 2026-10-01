# ③ リズムアクション予約フロー（Enemy 等の外部モジュールから）

- id: 3bf7c2c6-cc02-8125-a337-cc26fa0cbb3f
- path: Symphony Kill Chord / システム概要 / 音楽 / ③ リズムアクション予約フロー（Enemy 等の外部モジュールから）
- last_edited: 2026-08-17T13:21:11.176Z

Enemy などの外部モジュールが `IMusicActionScheduler` を使って、指定ビート後にコールバックを登録する処理フローである。
```Mermaid
sequenceDiagram
    autonumber
    participant Caller as 呼び出し元 (EnemyAttackReservationUsecase 等)
    participant Scheduler as IMusicActionScheduler
    participant MSService as IMusicSyncService

    Caller ->> Scheduler: 指定ビート数後のコールバックを予約 (Schedule)
    Note over MSService: 毎フレーム of Update ループで拍の到達を検知
    MSService ->> Scheduler: 予約済みアクションの実行チェック (毎フレーム)
    alt 指定ビートタイミングに到達
        Scheduler -->> Caller: 登録済みコールバックを発火
    end
```