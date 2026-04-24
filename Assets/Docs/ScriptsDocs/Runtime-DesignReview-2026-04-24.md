# Runtime 設計レビュー

対象: `Assets/Scripts/Runtime`  
レビュー日: 2026-04-24  
観点: レイヤー境界、依存方向、初期化責務、デバッグコード混入、命名整合性

## 総評

`Assets/Scripts/Runtime` は asmdef とレイヤー分割が先に用意されており、構造化しようとする意図は明確です。  
一方で、実コードは `Domain` / `Application` の Unity 依存、`Composition` の過大責務、`ServiceLocator` / `EventBus` による隠れた依存によって再結合しており、見た目ほど層分離の効果が出ていません。

現状は「レイヤー名はあるが、依存の実態は強く結びついている」状態です。  
新機能追加時に修正範囲が広がりやすく、初期化順序やランタイム結合で不具合を埋め込みやすい設計になっています。

## 定量メモ

| レイヤー | C# ファイル数 | Unity 参照あり | MonoBehaviour | ScriptableObject |
| :--- | ---: | ---: | ---: | ---: |
| `0.Utility` | 9 | 4 | 0 | 0 |
| `1.Domain` | 65 | 25 | 0 | 0 |
| `2.Application` | 43 | 27 | 0 | 0 |
| `3.Adaptor` | 44 | 21 | 0 | 0 |
| `4.View` | 35 | 30 | 20 | 0 |
| `5.InfraStructure` | 29 | 25 | 0 | 12 |
| `6.Composition` | 18 | 18 | 18 | 0 |

この数字からも、`Domain` と `Application` がかなり Unity に寄っていること、`Composition` が完全に Unity 実行基盤に乗っていることが読み取れます。

## 主な問題点

### 1. `Domain` が純粋なドメイン層になっていない

優先度: 高

根拠:
- `Assets/Scripts/Runtime/1.Domain/InGame/Camera/CameraParameter.cs`
- `Assets/Scripts/Runtime/1.Domain/Persistent/Input/BufferedInput.cs`
- `Assets/Scripts/Runtime/1.Domain/Player/SkillData.cs`
- `Assets/Scripts/Runtime/1.Domain/InGame/Skill/SkillDefinition.cs`

観測内容:
- `Domain` が `Vector2` / `Vector3` / `InputActionPhase` など Unity 固有型を直接持っています。
- `Domain` 側で `[SerializeField]` や `[SerializeReference]` を使っており、インスペクタ都合のシリアライズ表現がドメイン定義に混ざっています。
- `SkillDefinition` は値定義だけでなく `Effect.Execute()` と `Visual.Execute()` を直接呼びます。

影響:
- ドメインロジック単体のテストがしにくくなります。
- Unity 実装や入力システムを変えたいときに `Domain` まで巻き込みます。
- `InfraStructure` が持つべき「設定の保持」と `Domain` が持つべき「概念の定義」の境界が曖昧です。

改善方針:
- `Domain` は Unity 非依存の値とルールに寄せる。
- シリアライズ用クラスや `ScriptableObject` 変換は `InfraStructure` に寄せる。
- `Vector3` や `InputActionPhase` が必要な箇所は、専用の DTO / 値オブジェクトで境界を作る。

### 2. スキル定義が「データ」ではなく「実行オブジェクト」を抱えている

優先度: 高

根拠:
- `Assets/Scripts/Runtime/1.Domain/Player/SkillData.cs`
- `Assets/Scripts/Runtime/1.Domain/InGame/Skill/SkillDefinition.cs`
- `Assets/Scripts/Runtime/2.Application/Player/SkillEffect/TestSkillEffect.cs`
- `Assets/Scripts/Runtime/2.Application/Player/SkillVisual/SkillVisualTest.cs`
- `Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillController.cs`

観測内容:
- `SkillData` は `[SerializeReference]` で `ISkillEffect` / `ISkillVisual` の実装を保持しています。
- `SkillDefinition` はそのまま `Effect` / `Visual` を持ち、`SkillExecute()` で直接実行します。
- 現在見える実装は `TestSkillEffect` / `SkillVisualTest` で、実行戦略よりテスト用コードが先に乗っています。

影響:
- スキルアセットが「設定データ」ではなく「コード実行コンテナ」になります。
- 実装クラス差し替えがアセット互換性に直結し、保守負担が上がります。
- 本来 `Application` または `Adaptor` に置きたい実行戦略が、`Domain` の定義そのものに食い込んでいます。

改善方針:
- スキル定義は ID、条件、パラメータまでに留める。
- 実際の効果適用や演出再生は `Application` / `Adaptor` 側のディスパッチに分離する。
- 少なくともテスト用実装は Runtime 本体から外す。

### 3. `ServiceLocator` と静的 `EventBus` に依存しており、依存関係がコード上から追いにくい

優先度: 高

根拠:
- `Assets/Scripts/Runtime/0.Utility/Persistent/EventBus.cs`
- `Assets/Scripts/Runtime/4.View/Persistent/Input/PlayerInputView.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Bootstrap/IngameComposition.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Player/PlayerInitializer.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Camera/CameraSystemInitializer.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyTestSpawner.cs`

観測内容:
- `Composition` 内で `ServiceLocator.GetInstance<T>()` / `RegisterInstance()` が広く使われています。
- `PlayerInputView` のような `View` まで Service Locator に登録されます。
- ダメージ通知は `EventBus<EOnTakeDamage>` に流れ、発火元と購読先が離れています。

影響:
- 必要依存がコンストラクタやフィールドから見えず、初期化順序の不具合が起きやすいです。
- シーン内のどこで何が登録されるかを横断して読まないと全体像が掴めません。
- Dispose 漏れや登録漏れが、実行して初めて分かるタイプのバグになりやすいです。

改善方針:
- `Composition` で明示的に組み立て、依存はコンストラクタや初期化引数で渡す。
- 一時的に Service Locator を残す場合でも、機能単位で登録境界を狭める。
- グローバル `EventBus` は用途を限定し、重要なゲームフローは明示的な依存に戻す。

### 4. ダメージ通知が `GetHashCode()` ベースの識別に依存している

優先度: 高

根拠:
- `Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs`
- `Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/EnemyAIController.cs`

観測内容:
- `PlayerAttackController` は `targetEntity.GetHashCode()` をイベントに積んでいます。
- `EnemyAIController` は `_enemyBattleState.Attacker.GetHashCode()` と比較して被弾対象を識別しています。

影響:
- ハッシュコードは業務上の識別子ではなく、意図がコードに表れていません。
- 将来的な実装変更時に「同一個体判定」が壊れても気づきにくいです。
- クリティカル被弾によるスタンのような重要フローが、偶発的な識別方法に依存しています。

改善方針:
- `CharacterEntityId` など明示的な識別子を導入する。
- イベントに必要な最小情報を型として定義し直す。

### 5. Runtime 本体にテスト用・デバッグ用コードが混在している

優先度: 中

根拠:
- `Assets/Scripts/Runtime/2.Application/Player/SkillEffect/TestSkillEffect.cs`
- `Assets/Scripts/Runtime/2.Application/Player/SkillVisual/SkillVisualTest.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyTestSpawner.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyMoveDebugInitializer.cs`
- `Assets/Scripts/Runtime/6.Composition/Persistent/Input/InputDebugLogger.cs`

観測内容:
- テスト用実装やデバッグ用初期化が本番 Runtime asmdef に含まれています。
- `IngameComposition` から `EnemyTestSpawner` が直接呼ばれており、補助機能が主フローに混ざっています。

影響:
- 本番コードと検証コードの境界が曖昧になります。
- 不要な依存が残り、初期化責務が膨らみます。
- 「今は仮置き」のコードが恒久化しやすいです。

改善方針:
- `Develop`, `Debug`, `Editor`, `Sandbox` など別 asmdef に分離する。
- `Composition` からデバッグ機能を直接呼ばない構成にする。

### 6. `Composition` が組み立て以上の責務を持ちすぎている

優先度: 中

根拠:
- `Assets/Scripts/Runtime/6.Composition/InGame/Bootstrap/IngameComposition.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Player/PlayerInitializer.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyTestSpawner.cs`

観測内容:
- `IngameComposition` はシーンロード、入力切替、カーソル制御、初期化順序、リトライ待機まで担当しています。
- `PlayerInitializer` は依存解決だけでなく、エンティティ生成、ミッションイベント接続、敵スポナー接続まで担当しています。
- `EnemyTestSpawner` は `FindFirstObjectByType` / `FindAnyObjectByType` と Service Locator を併用し、別機能の詳細に踏み込んでいます。

影響:
- 変更時に `Composition` 側へ責務が集まり続けます。
- 機能ごとの独立差し替えが難しくなります。
- シーン依存の不具合とアプリケーションロジックの不具合が切り分けにくいです。

改善方針:
- `Composition` は「何を組み立てるか」に絞る。
- 機能固有の初期化はモジュール単位の installer / bootstrap に分割する。
- シーン探索や待機リトライは専用の起動サービスへ切り出す。

### 7. レイヤー名、asmdef 名、namespace が一致しておらず、読み手を迷わせる

優先度: 中

根拠:
- `Assets/Scripts/Runtime/5.InfraStructure/KillChord.InfraStructure.asmdef`
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Camera/CameraSystemConfig.cs`
- `Assets/Scripts/Runtime/3.Adaptor/InGame/Music/IMusicViewModel.cs`
- `Assets/Scripts/Runtime/6.Composition/Persistent/Input/InputDebugLogger.cs`

観測内容:
- フォルダ名は `5.InfraStructure` ですが、asmdef 名は `KillChord.Structure` です。
- `CameraSystemConfig.cs` の namespace は `KillChord.Runtime.Structure.InGame.Camera` です。
- `IMusicViewModel.cs` は `3.Adaptor` 配下にあるのに namespace は `KillChord.Runtime.View` です。
- `InputDebugLogger.cs` は `6.Composition` 配下にあるのに namespace は `KillChord.Runtime.View` です。

影響:
- IDE の namespace 補完と物理配置がズレます。
- 「どの層の責務か」をパスだけで判断できません。
- リファクタ時に取りこぼしや誤移動が起きやすくなります。

改善方針:
- フォルダ、asmdef、namespace の命名規則を統一する。
- 少なくとも `InfraStructure` / `Structure` の揺れは早めに解消する。
- 層を跨いだ namespace ずれを洗い出して是正する。

### 8. Mission 系は拡張のたびに型が増えやすい構成になっている

優先度: 低

根拠:
- `Assets/Scripts/Runtime/1.Domain/InGame/Mission/MissionDefinition.cs`
- `Assets/Scripts/Runtime/1.Domain/InGame/Mission/ClearCondition/IMissionClearCondition.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Mission/MissionDefinitionAsset.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Mission/AndClearConditionGroupAsset.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Mission/EnemyKillCountClearConditionAsset.cs`

観測内容:
- Domain 側の条件型に対して、Infra 側にも対応する Asset 型がほぼ 1 対 1 で存在します。
- 条件を 1 つ追加するたびに Domain, Infra, Inspector 表現をまとめて増やす必要があります。

影響:
- 機能追加時の実装点が増え、設計変更コストが高くなります。
- 条件の組み合わせが増えるほど、表現の重複も増えます。

改善方針:
- 直近では問題ありませんが、条件種別が増える見込みなら共通表現を先に整理した方が安全です。
- `Create()` 連鎖が増えすぎる前に、設定表現と実行表現の中間層を検討したいです。

## 優先度つき改善順

1. `Domain` / `Application` から Unity 依存とシリアライズ都合を引き剥がす。
2. `ServiceLocator` と静的 `EventBus` の使用範囲を縮小し、主要フローを明示依存に戻す。
3. `GetHashCode()` ベースの識別をやめ、明示的な Entity ID を入れる。
4. Runtime からテスト用・デバッグ用コードを別 asmdef へ退避する。
5. フォルダ名、asmdef 名、namespace の命名規則を統一する。
6. `Composition` を「組み立て専用」に寄せ、モジュールごとの初期化責務を分割する。

## ひとことで言うと

今の `Assets/Scripts/Runtime` は「レイヤー構造を持っている」のは強みですが、  
実際には Unity 依存、グローバル依存、デバッグ依存が複数レイヤーをまたいで残っているため、保守しやすい構造としてはまだ途中段階です。

まずは `Domain` の純度回復と、`Composition` / `ServiceLocator` 周りの整理から着手するのが最も効果的です。
