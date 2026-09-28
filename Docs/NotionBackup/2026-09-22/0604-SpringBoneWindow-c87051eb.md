# SpringBoneWindow

- id: 3207c2c6-cc02-805c-bd12-d5d3c87051eb
- path: Symphony Kill Chord / システム概要 / SpringBone仕様概要 / SpringBoneWindow
- last_edited: 2026-03-11T06:39:36.236Z

[image: スクリーンショット 2026-03-01 112340.png] attachment:a67f969b-8d38-4004-8dec-8c245b484b4b:スクリーンショット_2026-03-01_112340.png

#### **Show**
-  Show Only Selected Bones
  - 選択中のオブジェクトに含まれる SpringBone のみを表示する
- Show Only Selected Colliders
  - 選択中オブジェクトのコライダーのみ表示する
  - 大規模モデルで視認性を上げるための機能
- Show Bone Collisions
  - ボーン先端の当たり判定（半径）を SceneView に表示する
  - 衝突範囲の確認に使用する
---

#### Dynamics CSV
[image: スクリーンショット 2026-03-01 112722.png] attachment:1a495779-777e-4781-92bd-8a41d36906f2:スクリーンショット_2026-03-01_112722.png
- Load
  - CSV ファイルから SpringBone / Collider 設定を読み込む
[image: スクリーンショット 2026-03-01 112729.png] attachment:b352b302-b343-4b58-961f-7638d0b3477c:スクリーンショット_2026-03-01_112729.png
- Save
  - 現在の SpringBone / Collider 設定を CSV として保存する
---

#### SpringBone
- Add SpringBone
  - 選択中のオブジェクトすべてに SpringBone コンポーネントを追加する
- Create Origin
  - 選択しているすべてのオブジェクトに回転基準となる基点を生成する
  - 生成された基点は SpringBone の回転基準として使用される
  - 基点は SpringBone の現在位置を基準に作成される
  - 既に基点が存在する場合は再利用または更新される
- Create or Update Manager
  - 選択オブジェクト配下に SpringManager を作成
  - 既に存在する場合は SpringBone リストを更新する
- Mirror SpringBones
  - Mirror SpringBones を押すとウィンドウが開く
  [image: スクリーンショット 2026-03-01 115333.png] attachment:7f4e20d0-d383-47bd-b1e8-53536176a14f:スクリーンショット_2026-03-01_115333.png
SpringBone Mirror Window
  - Get from selection
    - 現在選択中の GameObject に含まれる SpringBone を取得する
    - 自動でミラー先（反対側）の SpringBone を探索してペアを作成する
    - Source → Destination の対応リストを生成する
  - Set bone where X < 0
    - X 座標が 0 未満の SpringBone を Source として自動取得する
    - 反対側（X > 0 側）の SpringBone を Destination として自動設定する
  - Set bone where X > 0
    - X 座標が 0 より大きい SpringBone を Source として取得する
    - 反対側（X < 0 側）を Destination として設定する
  - Select all copy source
    - リスト内の Source 側 SpringBone をすべて選択する
  - Select all copy destination 
    - リスト内の Destination 側 SpringBone をすべて選択する
  - Select All
    - Source / Destination 両方の SpringBone をすべて選択する
  - Do Mirror
    - リストに設定された Source → Destination のペアに対して実際にミラーコピー処理を実行する。

- Select This and Child Bones
  - 選択中のオブジェクト配下の SpringBone をすべて選択する
- Delete SpringBone
  - 選択中の SpringBone を削除する
- Delete Managers and Bones of Selection and Children
  - 選択オブジェクト配下の
    - SpringManager
    - SpringBone
    をすべて削除する
---

#### Collision
- Sphere/Capsule/Quad
  - 選択オブジェクト配下にコライダーを作成する
- Fit Capsule Place to Parent
  - 選択中のカプセルコライダーを親ボーンに合わせて位置調整する
- Exclude Collision from SpringBone
  - 選択中のSpringBoneからコライダー参照を削除する
  - コライダー自体は削除されない
- Delete Collider of Selection and Children
  - 選択オブジェクト配下のコライダーを削除する
- Clean Up
  - 不要な参照、重複、無効コライダーなどを削除する

### Wind Volume
[image: image.png] attachment:84e82f33-4a2c-4f26-ba00-494f057ad791:image.png
-  weight（0～1）
  - 風の有効度。
  - 0 → 無効
  - 1 → フル適用
- strength
  - 風の強さ
- amplitude
  - 揺らぎ具合(横方向)
  - 値を上げると風が大きく左右に揺れる
  - 0 にすると一定方向の風になる
- period
  - 風の時間変化周期（秒）
  - 値を下げる → 速く変化する風
  - 値を上げる → ゆっくりした風
- spinPeriod
  - 揺らぎ方向が回転する周期
  - 有効にすると、横揺れ方向がぐるぐる回る
  - 0 に近い値で無効
- peakDistance
  - 空間的な波の間隔
  - 小さい → ボーンごとに風がバラける
  - 大きい → 全体が同じように揺れる