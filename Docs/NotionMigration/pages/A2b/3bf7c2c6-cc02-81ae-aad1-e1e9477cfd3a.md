# ① レティクル表示フロー（毎フレーム）

- page_id: 3bf7c2c6-cc02-81ae-aad1-e1e9477cfd3a
- Notion パス: Symphony Kill Chord / システム概要 / レティクル / ① レティクル表示フロー（毎フレーム）
- スナップショットの最終更新: 2026-08-17T18:11:18.695Z
- 書き込み許可: 可
- 処置: 本文置き換え

## 判定

- 信頼性: 一部古い — 絞り込みと投影の中身は正しいが、呼び出しの向きが逆である。図では Presenter が View へ反映を押し込んでいるが、実装は `ReticleHudView.LateUpdate` が `ReticleHudPresenter.Tick(buffer)` を呼び、返ってきた一覧でマーカーを更新する（`Assets/Scripts/Runtime/4.View/InGame/Reticle/ReticleHudView.cs:40-47,60-100`）。投影で画面外・カメラの後ろの敵を落とす点も図に無い（`CameraScreenProjector.cs:30-56`）。確認コミット 73fefeb45、確認日 2026-09-22
- 整合性: 親ページ `システム概要 / レティクル`（3bf7c2c6-cc02-81b8-ad8c-cc400c63704e）の原稿（View → Adaptor の向き）とそろえた
- 変更点:
  - 冒頭の説明: `LateUpdate` 起点であること、画面外・カメラの後ろの敵を表示しないこと、マーカーを使い回すことを追記した
  - シーケンス図: 起点を `ReticleHudView` の `LateUpdate` に直し、`Tick(buffer)` の呼び出しと戻りの形にした。投影の失敗（画面外・カメラの後ろ）で除外する分岐と、表示対象から外れたマーカーを非表示にしてプールへ戻す手順を追加した
- 織り込んだ反映項目: CM-14（レティクル側・フロー部分）
- 出典: 実装 `ReticleHudView.cs`、`Assets/Scripts/Runtime/3.Adaptor/InGame/Reticle/ReticleHudPresenter.cs`、`Assets/Scripts/Runtime/4.View/InGame/Reticle/CameraScreenProjector.cs`
- 要確認: なし

## 適用する本文

`ReticleHudView`が毎フレーム（`LateUpdate`）Presenterへ表示対象を問い合わせ、登録済みの敵から表示対象を絞り、スクリーン座標へ投影して並べる。カメラの後ろにある敵と画面外の敵は表示しない。表示対象から外れたマーカーは非表示にして使い回す。

```Mermaid
sequenceDiagram
    autonumber
    participant View as ReticleHudView
    participant Presenter as ReticleHudPresenter
    participant TargetVM as ITargetSystemViewModel (Target)
    participant Projector as CameraScreenProjector
    participant Marker as ReticleMarkerView

    View ->> Presenter: Tick(buffer)（LateUpdate）
    Presenter ->> TargetVM: 注目中・候補のIDと、登録済みターゲットを取得
    TargetVM -->> Presenter: 対象の一覧
    Presenter ->> Presenter: 注目中・候補のIDを除外し、生存している敵に絞る
    loop 対象ごと
        Presenter ->> Projector: ワールド座標をスクリーン座標へ投影
        Projector -->> Presenter: スクリーン座標（カメラの後ろ・画面外なら失敗し、除外）
    end
    Presenter -->> View: ReticleMarker の一覧（buffer）
    View ->> Marker: 対象IDごとに割り当て、位置を更新
    View ->> Marker: 一覧から外れたマーカーを非表示にしてプールへ戻す
    Note over View: 注目中・候補は HUDEnemyHealthView が別途強調表示する
```
