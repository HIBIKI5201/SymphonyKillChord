# DemoExperienceConfig 設定メモ

`DemoExperienceConfig.asset` の設定A（元の設定）と設定B（現在の設定）の違い。違うのは次の2項目だけで、ほかの項目は同じ値。

| 項目 (Inspector) | フィールド | 設定A: タイマーあり・データリセットあり | 設定B: タイマーなし・データリセットなし |
|---|---|---|---|
| Is Timer Enabled | `_isTimerEnabled` | オン (`1`) | オフ (`0`) |
| Is Save Data Reset On End | `_isSaveDataResetOnEnd` | オン (`1`) | オフ (`0`) |

- 設定A: タイマーが切れるか最終バトルをクリアすると DemoEnd へ進み、セーブを削除する。
- 設定B: タイマーは表示も計時もしない。最終バトルをクリアすると DemoEnd へ進むが、セーブは残るので、タイトルから続きを再開できる。

## スキルツリーのロック（両設定・展示版共通）

| 項目 (Inspector) | フィールド | 値 |
|---|---|---|
| Locked Skill Node Ids | `_lockedSkillNodeIds` | `node_n2`（スキル2）、`node_n3`（スキル3） |

- 体験版では未実装のスキル2・3を解放させないため、この2ノードの解放ボタンを押せなくする。
- このノードを通らないと届かない先のノード（`node_n105` など）も、まとめて解放できなくなる。
- 「全ノード解放」が条件のノードは、ロック中のノードとその先のノードを数えずに判定する。
- `DemoExperienceConfig_Exhibition.asset` にも同じ値を入れている。
