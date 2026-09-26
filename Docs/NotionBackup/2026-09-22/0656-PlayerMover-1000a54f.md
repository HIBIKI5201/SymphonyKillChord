# PlayerMover

- id: 2b07c2c6-cc02-80b0-b5ba-e0b11000a54f
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / プレイヤー / PlayerMover
- last_edited: 2025-12-01T14:42:09.632Z


## クラス概要
プレイヤーキャラクターの「移動処理」を担当する。
カメラ基準の移動、ジャンプ、回転補間、Rigidbody への速度反映などを行う。

## API

### コンストラクタ
PlayerStatus / Rigidbody / player Transform / camera Transform を受け取り、
内部フィールドとして保持する。

### メソッド

#### CalcPlayerVelocityByInputDirection
入力方向からプレイヤーの速度を計算する。
引数をVector2の方向を受け取り、カメラ基準の移動方向に変換し、移動速度Vector3を返す。

#### SetPlayerVelocity
変換した移動速度ベクトルを引数として受け取りフィールド変数のvelocityに代入している。

#### SetIsGround
引数でbool値をもらいフィールド変数の_isGroundに代入する。

#### Update
MonoBehaviourを継承していないのでそのまま毎フレーム呼ばれるわけではないが、PlayerManagerのUpdateで呼ばれているからUpdateとして扱っている。
プレイヤーの向きを移動方向に向ける処理。
Dampingによる回転補間している。

#### FixedUpdate
こちらもUpdateと同じくPlayerManagerで呼ばれている。
RigidbodyのX/Zの速度をフィールド変数のvelocityに合わせて更新。
Yの速度は自然落下を維持。

## 内部実装
カメラ方位に基づく移動ベクトル生成。
指数減衰による回転補間。
Rigidbody の Y は保持し、X/Z のみ直接制御。
