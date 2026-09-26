# ④ SkillHitSchedulerによる連撃タイミング制御

- id: 3d67c2c6-cc02-811b-ac59-e928713c9cc2
- path: Symphony Kill Chord / システム概要 / キャラ&バトル / ④ SkillHitSchedulerによる連撃タイミング制御
- last_edited: 2026-09-09T03:31:03.104Z

連撃スキル（例: スキルID13）は、ヒットのタイミングを`SkillHitScheduler`に一任する。エフェクトの再生可否に関わらずダメージ自体は確実に発生させるため、スキル効果側は発動時にヒット内容を予約するだけで、実際の適用はスケジューラの`Tick`が行う。
```Mermaid
sequenceDiagram
    autonumber
    participant SkillEffect as Skill_13 等 (Skillモジュール)
    participant Scheduler as SkillHitScheduler
    participant Controller as SkillHitController (Skillモジュール)
    participant AExec as AttackExecutor

    Note over Scheduler: 音楽同期時にBPMへ応じた再生速度を設定 (SetPlaybackSpeed)
    SkillEffect ->> Scheduler: 連撃を予約 (Schedule: hitCount, delaySeconds, intervalSeconds, onHit)
    loop 毎フレーム
        Controller ->> Scheduler: 経過時間を進める (Tick)
        alt 予約時刻に到達したヒットがある
            Scheduler ->> SkillEffect: onHit コールバックを実行
            SkillEffect ->> AExec: 1ヒット分のダメージ適用要求
        end
    end
    Note over Controller: ゲームプレイ停止等では Clear で予約中の連撃を破棄する
```