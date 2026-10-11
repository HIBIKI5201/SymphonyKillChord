# DemoExperienceConfig 設定メモ

`DemoExperienceConfig.asset` の設定A（元の設定）と設定B（現在の設定）の違い。違うのは次の2項目だけで、ほかの項目は同じ値。

| 項目 (Inspector) | フィールド | 設定A: タイマーあり・データリセットあり | 設定B: タイマーなし・データリセットなし |
|---|---|---|---|
| Is Timer Enabled | `_isTimerEnabled` | オン (`1`) | オフ (`0`) |
| Is Save Data Reset On End | `_isSaveDataResetOnEnd` | オン (`1`) | オフ (`0`) |

- 設定A: タイマーが切れるか最終バトルをクリアすると DemoEnd へ進み、セーブを削除する。
- 設定B: タイマーは表示も計時もしない。最終バトルをクリアすると DemoEnd へ進むが、セーブは残るので、タイトルから続きを再開できる。
