# InGame-Player

プレイヤーの移動と回避を扱うモジュールです。

## 構造概要

### 1. Domain
- **PlayerMoveParameter**: 移動速度、回避時間などのパラメータ。

### 2. Application
- **PlayerApplication**: 通常移動と回避更新の切り替え。
- **PlayerMovement**: 通常移動。
- **PlayerDodgeMovementApplication**: 回避開始と回避中の更新。

### 3. Adaptor
- **PlayerController**: 入力を受けて `PlayerApplication` を呼ぶ。

### 4. View
- **PlayerView**: Rigidbody / Animator / 見た目反映。
- **PlayerAttackInputView**: 攻撃入力を戦闘側へ送る。

### 5. InfraStructure
- **PlayerConfig**: `PlayerMoveParameter` の設定元。

### 6. Composition
- **PlayerInitializer**: View、Controller、Application を組み立てる。
- **PlayerMoveParameterDebug**: Editor デバッグ用。

## 現在の実装メモ

- `PlayerApplication.Update()` は、回避中なら `PlayerDodgeMovementApplication`、それ以外は `PlayerMovement` を使います。
- `PlayerInitializer` はプレイヤーエンティティを生成しつつ、敵スポナー側へターゲット情報も渡しています。
