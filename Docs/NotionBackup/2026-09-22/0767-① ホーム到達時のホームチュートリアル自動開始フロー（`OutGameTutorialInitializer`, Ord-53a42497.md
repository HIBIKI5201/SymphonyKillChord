# ① ホーム到達時のホームチュートリアル自動開始フロー（`OutGameTutorialInitializer`, Order 150）

- id: 3d67c2c6-cc02-817f-85b3-ef2253a42497
- path: Symphony Kill Chord / システム概要 / Tutorial / ① ホーム到達時のホームチュートリアル自動開始フロー（`OutGameTutorialInitializer`, Order 150）
- last_edited: 2026-09-09T03:36:11.299Z

`BattleCompleted`以上かつ未完了の場合のみ、ホームチュートリアルが開始される。`RunHomeTutorialAsync`は現状プレースホルダであり、呼び出すと即座に完了する。
```Mermaid
sequenceDiagram
    autonumber
    participant Loading as LoadingScreenController
    participant Init as OutGameTutorialInitializer (Order 150)
    participant Save as SaveData.Tutorial (TutorialData)
    participant UIEvent as OutGameUIEvent

    Note over Init: Ready() 呼び出し時
    Init ->> Loading: IsLoading を確認
    alt ロード中
        Init ->> Loading: LoadingCompleted を購読
        Loading -->> Init: HandleLoadingCompleted(isSuccess)
    end
    Init ->> Save: Phase < BattleCompleted または IsTutorialCompleted を判定
    alt 開始条件を満たさない
        Note over Init: 何もせず終了
    else 開始条件を満たす
        Init ->> Save: StartHome()（Phase→HomeStarted、変化時のみ）
        Init ->> Save: SaveAsync<SaveData>()
        Init ->> UIEvent: OnHomeTutorialStarted を発火
        Init ->> Init: RunHomeTutorialAsync()（現状はプレースホルダ、即完了）
        Init ->> Save: Complete()（Phase→Completed）
        Init ->> Save: SaveAsync<SaveData>()
        Init ->> UIEvent: OnHomeTutorialCompleted を発火
    end
```