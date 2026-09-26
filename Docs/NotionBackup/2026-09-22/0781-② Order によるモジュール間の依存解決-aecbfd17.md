# ② Order によるモジュール間の依存解決

- id: 3bf7c2c6-cc02-81e1-836b-d675aecbfd17
- path: Symphony Kill Chord / システム概要 / 初期化ライフサイクル / ② Order によるモジュール間の依存解決
- last_edited: 2026-08-17T18:19:25.307Z

`Build`で登録し`Ready`で取得するため、取得したい相手より大きい`Order`が必要になる。
```Mermaid
sequenceDiagram
    autonumber
    participant Target as TargetSystemInitializationModule (Order 100)
    participant Camera as CameraSystemInitializer (Order 600)
    participant Locator as ServiceLocator

    Note over Target,Camera: Build フェーズ
    Target ->> Locator: TargetSystemModuleContainer を登録
    Camera ->> Locator: 自分の依存を登録
    Note over Target,Camera: Ready フェーズ
    Camera ->> Locator: TargetSystemModuleContainer を取得
    Locator -->> Camera: 取得成功（Order 100 が先に Build 済みのため）
    Note over Camera: Order を逆にすると取得に失敗し、初期化エラーになる
```