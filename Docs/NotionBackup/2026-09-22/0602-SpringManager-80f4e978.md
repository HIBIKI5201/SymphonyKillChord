# SpringManager

- id: 3207c2c6-cc02-80f7-aa22-dfcc80f4e978
- path: Symphony Kill Chord / システム概要 / SpringBone仕様概要 / SpringManager
- last_edited: 2026-03-11T06:38:21.717Z

[image: image.png] attachment:b2547992-84f6-4ed9-ac88-2d9a9e136bb0:image.png
- 窓を表示
  - SpringBoneWindowを表示する
- SpringBoneを全て選択
  - 指定したSpringManagerに登録されているSpringBoneを全て選択する
- SpringBoneリストを更新
  - 自分自身の子階層からすべての SpringBone コンポーネントを再取得する

#### Properties
- Automatic Update
  - SpringBone を自動で更新するかどうかを設定
  - ONの場合、毎フレーム自動でシミュレーションが実行される
  - OFFの場合、外部から手動で Update を呼び出す必要がある
- Is Paused
  - SpringBone の物理シミュレーションを一時停止する
  - ONにすると現在の姿勢を保持したまま更新が止まる
- Simulation Frame Rate
  - シミュレーションの更新フレームレートを指定
  - 値を下げると計算負荷は軽減されるが、動きが荒くなる
- Dynamic Ratio
  - 物理挙動の適用割合を制御する（0～1）。
    - 0：物理無効（元の姿勢を維持）
    - 1：完全に物理挙動を適用
- Gravity
  - SpringBone に加わる重力ベクトルを指定する
- Bounce
  - 衝突時の反発係数（0～1）
  - 値を上げると揺れの跳ね返りが強くなる
- Friction
  - 衝突時の摩擦係数（0～1）
  - 値を上げると揺れが早く収束する
  - 低いと長く揺れ続ける
    - 通常の揺れの減衰はSpringBoneコンポーネントの空気抵抗で設定する

#### Constraints
- Enable Angle Limits
  - SpringBoneで設定してあるY軸やZ軸の角度制限を適応するかどうかを設定
- Enable Collision
  - SpringBone専用のコライダーとの衝突判定を有効化するかどうかを設定
- Enable Length Limits
  - 長さ制限ターゲットとの距離制約を有効にする

#### Ground Collision
- Collide With Ground
  - 地面との衝突判定を有効にする
- Ground Height
  - 地面のワールド座標
  - この高さよりも下にボーンが配置できなくなる
- Spring Bones
  - 管理するSpringBoneのリスト

#### Gizoms
- Bone Color
  - ボーンラインの表示色
- Collider Color
  - コライダーの表示色
- Collision Color
  - 衝突発生時の表示色
- Ground Collision
  - 地面描画の表示色
- Angle Limit Draw Scale
  - 角度制限ギズモの描画サイズ倍率