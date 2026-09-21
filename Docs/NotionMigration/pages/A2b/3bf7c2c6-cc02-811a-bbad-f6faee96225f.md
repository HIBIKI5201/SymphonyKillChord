# ② ロックオン開始フロー（入力・被弾時）

- page_id: 3bf7c2c6-cc02-811a-bbad-f6faee96225f
- Notion パス: Symphony Kill Chord / システム概要 / カメラ / ② ロックオン開始フロー（入力・被弾時）
- スナップショットの最終更新: 2026-08-17T13:19:22.520Z
- 書き込み許可: 可
- 処置: 本文置き換え

## 判定

- 信頼性: 一部古い — 「被弾時は EOnTakeDamage の攻撃者IDで対象を指定」が誤り。実装は `EOnTakeDamage.DefenderId`（自分の攻撃が命中した敵）で付け替える（`Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs:412-435`）。ロックオン入力はトグル動作で、ロック中に押すと解除する（:580-593）。左右切替（PR #1852、2026-09-18）が図に無い。確認コミット 73fefeb45、確認日 2026-09-22
- 整合性:
  - 実装との矛盾: 上記の「攻撃者ID」。「対象を設定（失敗時はFreeへ）」は、対象が見つからなかったときに次のフレームで Free に戻るのが正しい（`BuildFrame` で位置が取れず `ClearLockOn`、:643-648）
  - 他ページとの矛盾: `システム概要 / カメラ`（3957c2c6-cc02-809c-9b9d-f97c42fd9b3b）の「攻撃してきた相手」と同じ誤り（同ページの原稿で修正）
  - ページ名「（入力・被弾時）」は実装の「命中時」と合わない。ただし子ページはタイトルで突き合わせるため（`sync_module.py`）、タイトルは変えず本文で説明する
- 変更点:
  - 冒頭の説明: 攻撃入力は押下開始時、ロックオン入力は確定時に処理すること、ロックオン入力はトグルであること、左右切替と命中時の付け替えを追記した。「被弾時」は「自分の攻撃が命中したとき」の意味であると明記した
  - シーケンス図: 旧: 攻撃入力 / ロックオン入力を 1 本にまとめ、Note で「攻撃者ID」 → 新: ①攻撃入力（オート）、②ロックオン入力（トグル）、③左右切替（`TrySwitchTarget`）、④攻撃の命中（`EOnTakeDamage.DefenderId` で `TrySetCurrentTarget`、ロックオン SE は鳴らさない）の 4 つの分岐に分けた
- 織り込んだ反映項目: CM-05、CM-06（システム概要側・フロー部分）、CM-17（命中時の付け替えで SE を鳴らさない件）
- 出典: 実装 `CameraSystemView.cs:357-435, 561-593, 689-719`、`Assets/Scripts/Runtime/6.Composition/InGame/Camera/CameraSystemInitializer.cs:95-106`、`Assets/Scripts/Runtime/4.View/InGame/Target/TargetingSystem.cs:276-285`（`EOnLockOnAcquired`）。PR #1852
- 要確認: なし

## 適用する本文

攻撃入力（押下開始時）ではオートロックオン、ロックオン入力（押下確定時）ではマニュアルロックオンへ遷移する。マニュアル中はオートによる上書きが起きない。ロックオン入力はトグルであり、オート・マニュアルどちらのロック中でも押すと解除する（オート中に押してもマニュアルへは切り替わらない）。

ロック中は左右切替入力で対象を選び直せる。また、自分の攻撃が敵に命中するたびに（`EOnTakeDamage`）、オートロックオンの対象をその敵へ付け替える。ページ名の「被弾時」は、この「敵が自分の攻撃で被弾したとき」を指す。プレイヤー自身の被弾（`EOnPlayerTakeDamage`）ではロックオンは変わらない。

```Mermaid
sequenceDiagram
    autonumber
    actor Player as プレイヤー
    participant CSView as CameraSystemView
    participant TargetVM as ITargetSystemViewModel

    alt 攻撃入力（押下開始）
        Player ->> CSView: 攻撃入力
        Note over CSView: マニュアル中は何もしない
        CSView ->> CSView: 状態をオートにし、解除タイマーを初期化
        CSView ->> TargetVM: ChangeTarget（カメラ前方で対象を選択）
    else ロックオン入力（押下確定）
        Player ->> CSView: ロックオン入力
        alt 未ロック
            CSView ->> CSView: 状態をマニュアルにする
            CSView ->> TargetVM: ChangeTarget（カメラ前方で対象を選択）
        else ロック中（オート・マニュアル）
            CSView ->> TargetVM: ClearTarget（Freeへ）
        end
    else 左右切替入力（ロック中のみ）
        Player ->> CSView: 左 / 右
        CSView ->> TargetVM: TrySwitchTarget（カメラ前方＋右方向×符号で選択）
        TargetVM -->> CSView: 別の敵が選ばれたときだけ切り替え
        Note over CSView: オート中は画面外猶予を与え直す
    else 攻撃の命中（EOnTakeDamage）
        Note over CSView: マニュアル中は何もしない
        CSView ->> TargetVM: TrySetCurrentTarget（DefenderId、ロックオンSEは鳴らさない）
        TargetVM -->> CSView: 成功したらオートにし、命中なしタイマーを0へ、画面外猶予を与える
    end
    Note over CSView: 対象が見つからなかった場合は、次のフレームで位置が取れずFreeへ戻る
```
