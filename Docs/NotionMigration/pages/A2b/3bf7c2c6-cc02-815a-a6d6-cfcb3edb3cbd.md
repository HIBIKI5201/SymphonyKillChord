# ② ターゲット選択フロー（ロックオン入力時）

- page_id: 3bf7c2c6-cc02-815a-a6d6-cfcb3edb3cbd
- Notion パス: Symphony Kill Chord / システム概要 / ターゲットシステム / ② ターゲット選択フロー（ロックオン入力時）
- スナップショットの最終更新: 2026-08-17T13:19:32.598Z
- 書き込み許可: 可
- 処置: 本文置き換え

## 判定

- 信頼性: 一部古い — ロックオン入力で呼ぶのは `SwitchTarget` ではなく `ChangeTarget` である。`TrySwitchTarget` は左右切替入力専用（`Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs:580-593, 689-719`）。カメラは `TargetSystemController` を経由せず、`ITargetSystemViewModel`（実体は `TargetingSystem`）を直接呼ぶ（`Assets/Scripts/Runtime/6.Composition/InGame/Camera/CameraSystemInitializer.cs:95-106`）。選定アルゴリズム（内積最大、負なら最も近い敵）は正しい（`Assets/Scripts/Runtime/4.View/InGame/Target/TargetingSystem.cs:322-353`）。確認コミット 73fefeb45、確認日 2026-09-22
- 整合性: 実装との矛盾は上記。`システム概要 / カメラ / ② ロックオン開始フロー`（3bf7c2c6-cc02-811a-bbad-f6faee96225f）の原稿と呼び出し名をそろえた
- 変更点:
  - 冒頭の説明: 選定の対象と条件（生存中の登録済み敵全員、距離・視界・HPの制限なし）、起点と方向（プレイヤー位置とカメラ前方）、ロックオン成立時の `EOnLockOnAcquired` を追記した
  - シーケンス図: 旧: 1 本の図で `Camera → Controller → SwitchTarget` → 新: 「ロックオン入力 → `ChangeTarget`」「左右切替 → `TrySwitchTarget`」の 2 分岐に分け、参加者から `TargetSystemController` を外して `ITargetSystemViewModel (TargetingSystem)` にした
- 織り込んだ反映項目: CM-05、CM-01（選定条件のシステム概要側）、CM-17（`EOnLockOnAcquired`）
- 出典: 上記の実装箇所、`TargetingSystem.cs:194-285`
- 要確認: なし

## 適用する本文

プレイヤーがロックオン入力を行った際、正面方向・距離に基づいて最適な対象を選択する。起点はプレイヤー位置、方向はカメラ前方である。対象は登録済みで生存中の敵全員であり、距離・視界・HPによる制限は無い。カメラ前方との正規化内積が最大の敵を選び、内積が最大でも負（全員がカメラより後ろ）のときは最も近い敵を選ぶ。攻撃入力によるオートロックオンも同じ選定を使う。

ロック中の左右切替入力では、「カメラ前方＋カメラ右方向×符号」の方向で同じ選定をやり直し、現在と別の敵が選ばれたときだけ切り替える。どちらの場合も、現在と異なる敵へ新たにロックオンが成立すると`EOnLockOnAcquired`を発火する（`PlayerView`がロックオンSEを鳴らす）。カメラは`TargetSystemController`を経由せず、`ITargetSystemViewModel`を直接呼ぶ。

```Mermaid
sequenceDiagram
    autonumber
    participant Camera as CameraSystemView
    participant TSys as ITargetSystemViewModel (TargetingSystem)

    alt ロックオン入力（未ロック時）
        Camera ->> TSys: ChangeTarget(playerPosition, cameraForward)
        TSys ->> TSys: 登録済みターゲットのうち正面内積が最大の対象を選択（負の場合は距離優先）
        TSys ->> TSys: 新しい対象なら EOnLockOnAcquired を発火
    else 左右切替入力（ロック中）
        Camera ->> TSys: TrySwitchTarget(playerPosition, cameraForward + cameraRight × 符号)
        TSys ->> TSys: 同じ選定で対象を評価
        TSys -->> Camera: 現在と別の敵が選ばれたときだけ切り替えて true
    end
    Camera ->> TSys: 現在ターゲット位置の取得要求（毎フレーム）
    TSys -->> Camera: ターゲット位置を返却
```
