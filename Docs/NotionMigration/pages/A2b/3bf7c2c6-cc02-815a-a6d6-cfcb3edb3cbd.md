# ② ターゲット選択フロー（ロックオン入力時）

- page_id: 3bf7c2c6-cc02-815a-a6d6-cfcb3edb3cbd
- Notion パス: Symphony Kill Chord / システム概要 / ターゲットシステム / ② ターゲット選択フロー（ロックオン入力時）
- スナップショットの最終更新: 2026-08-17T13:19:32.598Z
- 書き込み許可: 可
- 処置: 本文置き換え

## 判定

- 信頼性: 一部古い — ロックオン入力で呼ぶのは `SwitchTarget` ではなく `ChangeTarget` である。`TrySwitchTarget` は左右切替入力専用（`Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs:580-593, 689-719`）。カメラは `TargetSystemController` を経由せず、`ITargetSystemViewModel`（実体は `TargetingSystem`）を直接呼ぶ（`Assets/Scripts/Runtime/6.Composition/InGame/Camera/CameraSystemInitializer.cs:95-106`）。対象の選び方は `仕様概要 / カメラワーク`（3257c2c6-cc02-80ee-8950-e36ec0a324d5）の「視界範囲内にいる最もHPが低いor距離が近い敵」が正であり、既存本文の「正面方向・距離に基づいて選択」は実装（内積最大、負なら最も近い敵）を写したもので仕様と食い違う。確認コミット 73fefeb45、確認日 2026-09-22
- 整合性: 実装との矛盾は上記。`システム概要 / カメラ / ② ロックオン開始フロー`（3bf7c2c6-cc02-811a-bbad-f6faee96225f）の原稿と呼び出し名をそろえた
  - 対象の選び方: 実装の不具合（仕様に合わせて直す必要あり。根拠 `Assets/Scripts/Runtime/4.View/InGame/Target/TargetingSystem.cs:322-353` の `EvaluateBestTarget` が正面内積と距離だけで選び、HP と視界範囲を見ていない）
- 決定（2026-09-22 八幡）: ロックオン対象の選び方は仕様（視界範囲内・HP 優先）が正しく、実装が誤り。本文は仕様どおりに書く
- 変更点:
  - 冒頭の説明: 旧: 「正面方向・距離に基づいて最適な対象を選択する」 → 新: 「視界範囲内にいる敵のうち、最もHPが低い、または距離が近い敵を選択する」（仕様に戻した）。起点と方向（プレイヤー位置とカメラ前方）、ロックオン成立時の `EOnLockOnAcquired` を追記した
  - 冒頭の説明: 左右切替の選び方（方向基準）は仕様に定義が無いため、現状の実装を書いて【要確認】を付けた
  - シーケンス図: 旧: 1 本の図で `Camera → Controller → SwitchTarget` → 新: 「ロックオン入力 → `ChangeTarget`」「左右切替 → `TrySwitchTarget`」の 2 分岐に分け、参加者から `TargetSystemController` を外して `ITargetSystemViewModel (TargetingSystem)` にした。選定の Note を仕様の選び方に直した
- 織り込んだ反映項目: CM-05、CM-01（選定条件のシステム概要側。決定により仕様側に合わせた）、CM-17（`EOnLockOnAcquired`）
- 出典: 上記の実装箇所、`TargetingSystem.cs:194-285`、`仕様概要 / カメラワーク` の Notion スナップショット（2026-05-29 更新）
- 要確認:
  - 仕様の「最もHPが低いor距離が近い」は、HP と距離のどちらを優先するかが一意でない（企画）
  - 左右切替の選び方は仕様に定義が無い。現状の方向基準のままでよいか（企画）

## 適用する本文

プレイヤーがロックオン入力を行った際、視界範囲内にいる敵のうち、最もHPが低い、または距離が近い敵を対象に選択する。【要確認: HP と距離のどちらを優先するかを企画に】起点はプレイヤー位置、方向はカメラ前方である。攻撃入力によるオートロックオンも同じ選定を使う。

ロック中の左右切替入力では、「カメラ前方＋カメラ右方向×符号」の方向を基準に対象を選び直し、現在と別の敵が選ばれたときだけ切り替える。【要確認: 左右切替の選び方は仕様に定義が無い。方向基準のままとするかを企画に】どちらの場合も、現在と異なる敵へ新たにロックオンが成立すると`EOnLockOnAcquired`を発火する（`PlayerView`がロックオンSEを鳴らす）。カメラは`TargetSystemController`を経由せず、`ITargetSystemViewModel`を直接呼ぶ。

```Mermaid
sequenceDiagram
    autonumber
    participant Camera as CameraSystemView
    participant TSys as ITargetSystemViewModel (TargetingSystem)

    alt ロックオン入力（未ロック時）
        Camera ->> TSys: ChangeTarget(playerPosition, cameraForward)
        TSys ->> TSys: 視界範囲内の登録済みターゲットのうち、HPが最も低い、または距離が最も近い対象を選択
        TSys ->> TSys: 新しい対象なら EOnLockOnAcquired を発火
    else 左右切替入力（ロック中）
        Camera ->> TSys: TrySwitchTarget(playerPosition, cameraForward + cameraRight × 符号)
        TSys ->> TSys: 指定方向を基準に対象を評価
        TSys -->> Camera: 現在と別の敵が選ばれたときだけ切り替えて true
    end
    Camera ->> TSys: 現在ターゲット位置の取得要求（毎フレーム）
    TSys -->> Camera: ターゲット位置を返却
```
