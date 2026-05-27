# InGame Battle Code Review 2026-05-04

対象: `Assets/Scripts/Runtime/*/InGame/Battle` と、その主フローに直接関係する呼び出し元。

## 問題1: プレイヤー攻撃が常に命中扱いになる

### 対象
- `Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs`
- `Assets/Scripts/Runtime/2.Application/InGame/Battle/AttackExecutor.cs`

### 内容
`PlayerAttackController.ExecuteAttack()` は `AttackExecutor.Execute(..., true)` を固定で渡しています。

そのため、遮蔽物や射程外などの不成立条件があっても、`AttackExecutor` 側で `defender.TakeDamage()` まで進みます。

敵側の攻撃では命中判定サービスを介しているため、プレイヤー側だけ戦闘ルールが不整合です。

### 影響
- 壁越しヒットが発生する。
- 射程外でもダメージ適用される。
- 敵とプレイヤーで戦闘ルールが一致しない。

## 問題2: 攻撃結果 UI が View に接続されていない

### 対象
- `Assets/Scripts/Runtime/6.Composition/InGame/Player/PlayerInitializer.cs`
- `Assets/Scripts/Runtime/4.View/InGame/Battle/AttackResultView.cs`
- `Assets/Scripts/Runtime/4.View/InGame/Battle/AttackResultViewModel.cs`

### 内容
`PlayerInitializer` では `AttackResultViewModel` と `AttackResultPresenter` を生成していますが、`AttackResultView.Bind()` を呼んでいません。

`AttackResultPresenter.Push()` 自体は実行されても、View 側が `OnChanged` を購読していないため、画面表示に反映されません。

### 影響
- 攻撃結果表示が機能しない。
- Presenter と ViewModel の生成コストだけが残る。
- 実装者が「表示される前提」でデバッグすると誤認しやすい。

## 問題3: 被弾イベントの対象識別に GetHashCode を使っている

### 対象
- `Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs`
- `Assets/Scripts/Runtime/0.Utility/Persistent/EOnTakeDamage.cs`
- `Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyAIController.cs`

### 内容
プレイヤー攻撃時の `EOnTakeDamage` は、対象識別子として `targetEntity.GetHashCode()` を送っています。

受信側の `EnemyAIController` はこの値だけで自分への被弾かどうかを判定し、クリティカル時にスタン遷移を実行します。

`GetHashCode()` は永続識別子ではなく、衝突回避も保証されません。

### 影響
- ハッシュ衝突時に別個体へ誤ってスタンが入る可能性がある。
- 戦闘状態遷移が不安定になる。
- TODO のまま本線ロジックに入っている。

## 問題4: 無効な BeatType 設定が静かに握りつぶされる

### 対象
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Battle/AttackDefinitionFactory.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Battle/AttackDefinitionData.cs`
- `Assets/Scripts/Runtime/1.Domain/InGame/Character/CharacterCombatSpec.cs`

### 内容
`AttackDefinitionFactory.Create()` は `UseBeatType` が有効でも、`BeatType` が enum に存在しない場合に例外を投げず `null` として扱います。

その結果、`CharacterCombatSpec.GetAttackDefinitionByBeatType()` の検索にヒットせず、実行時に「対応する攻撃定義が見つからない」とだけ失敗します。

設定ミスの原因がアセット側にあるのに、その場で検出されません。

### 影響
- 攻撃不能が実行時まで潜伏する。
- 原因特定に余計な時間がかかる。
- データ不正とロジック不正の切り分けが難しくなる。
