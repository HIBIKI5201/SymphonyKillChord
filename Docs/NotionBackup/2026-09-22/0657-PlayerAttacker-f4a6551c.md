# PlayerAttacker

- id: 2b07c2c6-cc02-8034-941b-cdf6f4a6551c
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / プレイヤー / PlayerAttacker
- last_edited: 2025-11-26T13:03:31.750Z


## クラス概要
プレイヤーの攻撃処理を担当するクラス。
カメラの正面方向に Raycast を飛ばし、攻撃対象を検出する。
無視タグでないオブジェクトが ICharacter を実装していればダメージを与える。

## API

### コンストラクタ
引数をPlayerStatus,PlaeyrConfig,,cameraのTransformとして受け取ってそれぞれフィールド変数に代入している。
- `status` → 攻撃力・攻撃射程などのパラメータ。
- `config` → 無視するタグなどの設定値。
- `camera` → Raycast の origin / direction の参照元。

### メソッド

#### Attack
カメラの正面にRaycastを飛ばし、対象がICaracterのインターフェースを持っていたならダメージを与える。

#### FindAttackTarget
Raycastにより最初にヒットしたオブジェクトを確認。
無視するタグだった場合タグは無視。
HelthEntityがあれば攻撃対象として返す。
見つからなければnull。

#### 攻撃の可視化
Raycast の範囲を赤線で SceneView 上に可視化する。

## 内部実装
Raycast による直線攻撃。
タグによる無視処理。
ICharacter による抽象化（敵/味方どれでも攻撃可能）。
Gizmo によるデバッグ補助。
