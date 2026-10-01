# ③ タイトル復帰フロー（ESC長押し）

- id: 3bf7c2c6-cc02-81f7-8423-d89bf8c0ffd6
- path: Symphony Kill Chord / システム概要 / シークエンス / ③ タイトル復帰フロー（ESC長押し）
- last_edited: 2026-08-17T13:19:51.949Z

プレイ中にESCを長押しすると、リザルトを経由せずタイトルシーンへ戻る。
```Mermaid
sequenceDiagram
    autonumber
    actor Player as プレイヤー
    participant Init as ReturnToTitleInitializer
    participant Ctrl as ReturnToTitleController
    participant SceneUC as SceneTransitionUsecase

    Player ->> Init: ESCを長押し
    Init ->> Ctrl: タイトル復帰要求
    Ctrl ->> SceneUC: インゲームをアンロードしてタイトルへ遷移
```