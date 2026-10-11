# DemoExperienceConfig 設定メモ

体験版の動き（タイマー・終了ステージ・セーブ削除）は、同じフォルダの `DemoExperienceConfig.asset` で切り替える。
Inspector の「Timer」「End」欄で設定する。

## 項目

| 項目 (Inspector) | フィールド | 既定値 | 内容 |
|---|---|---|---|
| Is Timer Enabled | `_isTimerEnabled` | オン | オフにすると、ホームタイマーと全体タイマーの表示・計時を止める。時間切れによる強制出撃と体験版終了も起きない |
| Home Time Limit Seconds | `_homeTimeLimitSeconds` | 90 | ホームに入ってから強制出撃するまでの秒数。ホームに戻るたびにリセットされる |
| Overall Time Limit Seconds | `_overallTimeLimitSeconds` | 300 | 全体タイマーの開始地点から体験終了までの秒数。ホームでも戦闘中でも進む |
| Overall Timer Start Point | `_overallTimerStartPoint` | チュートリアル | 全体タイマーを開始する地点（シナリオ = 0 / チュートリアル = 1 / ホーム = 2） |
| End Stage Id | `_endStageId` | 未設定 | クリアすると DemoEnd へ進むバトルステージ。未設定ならステージツリー上の最終バトル（現在は stage_03） |
| Is Save Data Reset On End | `_isSaveDataResetOnEnd` | オン | オフにすると、DemoEnd に着いてもセーブを削除しない。次回はタイトルから続きを再開できる |

タイマーを無効にしている間は、時間に関する3項目（Home / Overall / Start Point）は使われない。

## 設定A: タイマーあり・終了時にデータリセット（元の設定）

展示会などで、1人ずつ時間を区切って遊んでもらうときの設定。

- Is Timer Enabled: **オン**
- Home Time Limit Seconds: 90
- Overall Time Limit Seconds: 300
- Overall Timer Start Point: チュートリアル
- End Stage Id: 未設定
- Is Save Data Reset On End: **オン**

全体タイマーが切れるか、最終バトルをクリアすると DemoEnd へ進む。DemoEnd に着いた時点でセーブを削除するので、次の人は最初から遊ぶ。

```yaml
  _isTimerEnabled: 1
  _homeTimeLimitSeconds: 90
  _overallTimeLimitSeconds: 300
  _overallTimerStartPoint: 1
```

## 設定B: タイマーなし（現在の設定）

時間制限なしで遊んでもらうときの設定。

- Is Timer Enabled: **オフ**
- そのほかは設定Aと同じ（End Stage Id は未設定、Is Save Data Reset On End はオン）

タイマーの表示・強制出撃・時間切れ終了は起きない。最終バトル（stage_03）をクリアすると DemoEnd へ進み、セーブを削除する。

```yaml
  _isTimerEnabled: 0
```

途中のデータから再開できるようにしたい場合は、さらに **Is Save Data Reset On End をオフ**にする。DemoEnd に着いてもセーブが残り、タイトルからホームへ続きで再開できる。ただし体験版ツリーには stage_03 より先が無いので、再開後に stage_03 をもう一度クリアすると、また DemoEnd へ進む。

## 注意

- アセットに書かれていない項目は既定値になる。たとえば `_isTimerEnabled` の行が無ければタイマーはオン。#2448 で設定項目を追加したときにアセットの値を変えていなかったため、main がタイマーありのまま動いていた（#2455 で修正）。
- 項目の定義は `Assets/Scripts/Demo/DemoExperienceConfig.cs`。
