# 17. デメテルの法則・CQS・引数設計

「オブジェクト間のやり取りの作法」に関する原則をまとめる。

- **デメテルの法則**（最小知識の原則）— 直接の友達とだけ話す
- **CQS**（Command Query Separation）— 状態を変えるか値を返すか、どちらか一方
- **Tell, Don't Ask** — データを取り出して判断せず、判断ごと依頼する
- **Long Parameter List / Data Clump** — 引数の塊はオブジェクトにする

---

## デメテルの法則（Law of Demeter）

> メソッド内で呼んでよいのは、自分自身・引数・自分のフィールド・自分が生成したオブジェクトのメソッドのみ。

### 【違反】3段以上のメンバチェーン

| 場所 | コード |
| --- | --- |
| [StageResultPresenter.cs:66,67,85,86](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Result/StageResultPresenter.cs) | `_missionRuntimeService.MissionProgress.ElapsedTime.Value`<br>`_missionRuntimeService.MissionProgress.MaxCombo.Value` |
| [MissionHudPresenter.cs:40,62](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Mission/MissionHudPresenter.cs) | `_missionRuntimeService.MissionProgress.EndReason.ToString()`<br>`_missionRuntimeService.MissionDefinition.ClearCondition.GetStep(...)` |
| [PlayerAttackController.cs:203](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs) | `_battleState.Attacker.CombatSpec.GetAttackDefinitionByBeatType(...)` |
| [SceneDependencyInitializationModule.cs:88](../../Assets/Scripts/Runtime/6.Composition/InGame/Bootstrap/SceneDependencyInitializationModule.cs) | `_container.InputComposition.GetInputMapController.EnableOnly(...)` |
| [InGameMissionInitializer.cs:89,97](../../Assets/Scripts/Runtime/6.Composition/InGame/Mission/InGameMissionInitializer.cs) | `_moduleContainer.MissionRuntimeService.MissionDefinition.ClearCondition` |
| [SkillInputProgressState.cs:24](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillUI/SkillInputProgressState.cs) | `_skillDefinition.SkillPattern.Signatures.Length` |

### なぜ問題か

`_missionRuntimeService.MissionProgress.ElapsedTime.Value` は、
Presenterが **`MissionRuntimeService`・`MissionProgress`・`ElapsedTime` の
3つの型の内部構造を知っている**ことを意味する。

`MissionProgress` のプロパティ名が変わるだけで、Presenter が壊れる。
`MissionProgress` が `null` になりうる場合、この行のどこでNullReferenceExceptionが
起きたかも分からない。

`_battleState.Attacker.CombatSpec.GetAttackDefinitionByBeatType(...)` は特に深刻で、
**Adaptor層が Domain の内部構造を3段掘っている**。

### 修正方針

チェーンの終端で欲しい値を、**最初のオブジェクトが直接提供する**。

```csharp
// MissionRuntimeService 側
public float ElapsedSeconds => MissionProgress.ElapsedTime.Value;
public int   MaxCombo       => MissionProgress.MaxCombo.Value;

// Presenter 側
FormatBattleTime(_missionRuntimeService.ElapsedSeconds)
```

`PlayerAttackController` の場合は Tell, Don't Ask がより適切。

```csharp
// 現状（Ask: 取り出して自分で判断）
var def = _battleState.Attacker.CombatSpec.GetAttackDefinitionByBeatType(beatType);

// 改善（Tell: 判断ごと依頼）
var def = _battleState.ResolveAttackDefinition(beatType);
```

---

## CQS（コマンド・クエリ分離）

> メソッドは「状態を変える（コマンド）」か「値を返す（クエリ）」のどちらか一方であるべき。

### 【違反・作者も認識済み】ExecuteAttack

**場所**: [PlayerAttackController.cs:81](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs)

```csharp
public bool ExecuteAttack(out int resultBeatType) //TODO : outでBeatTypeを返す構造を修正する
```

「攻撃を実行する」というコマンドが、成否（bool）と拍種別（out int）という
**2つのクエリ結果**を返している。コメントで作者自身が問題と認識している。

**修正方針**: 結果を表す `readonly struct AttackExecutionResult` を返す。
プロジェクトには既に `readonly ref struct` のDTOが多数あるため、この方針と整合する。

```csharp
public readonly struct AttackExecutionResult
{
    public readonly bool Succeeded;
    public readonly int  BeatType;
}
```

### 【違反】ref + out を併用する Update

**場所**:
[PlayerApplication.cs:25](../../Assets/Scripts/Runtime/2.Application/InGame/Player/PlayerApplication.cs)、
[PlayerController.cs:44](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Player/PlayerController.cs)、
[PlayerMovementApplication.cs:20](../../Assets/Scripts/Runtime/2.Application/InGame/Player/PlayerMovementApplication.cs)、
[PlayerDodgeMovementApplication.cs:57](../../Assets/Scripts/Runtime/2.Application/InGame/Player/PlayerDodgeMovementApplication.cs)

```csharp
public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
```

`void` を返しながら、`ref` で回転を書き換え、`out` で速度を返す。
呼び出し側は「このメソッドが何を変更するのか」をシグネチャから読み取りにくい。

同じシグネチャが `IPlayerApplication` / `IPlayerController` の
インターフェースにも定義されており、4箇所に波及している。

**修正方針**: 入出力をまとめた構造体を返す。

```csharp
public readonly struct PlayerMovementResult
{
    public readonly Quaternion Rotation;
    public readonly Vector3    Velocity;
}

public PlayerMovementResult Update(in PlayerMovementInput input);
```

`readonly struct` なのでGCは発生しない。

### 【違反】CollectEquippedSkillIds

**場所**: [EquipmentBgmInitializer.cs:240](../../Assets/Scripts/Runtime/6.Composition/InGame/Music/EquipmentBgmInitializer.cs)

```csharp
private IReadOnlyList<int> CollectEquippedSkillIds(out string skillSource)
```

スキルID一覧と「どこから取得したか」という診断情報を同時に返している。
診断情報はログ用と思われるため、メソッド内でログを出せば `out` は不要になる。

### 【許容】Try* パターン

`TryGetCurrentTarget(out ...)` 等の `Try*` メソッドは、
**.NET標準の慣用句**であり CQS の例外として広く受け入れられている。
本プロジェクトでも一貫して使われており問題ない。

---

## Long Parameter List / Data Clump

### 【最重大】引数15個のInitialize

**場所**: [CameraSystemView.cs:45-60](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs)

```csharp
public void Initialize(
    Action<Vector3, Vector3> changeTargetAction,
    Action clearTargetAction,
    Func<(bool HasTarget, Vector3 TargetPosition)> getCurrentTargetPositionFunc,
    Action<Vector3, Vector3> updateCandidateAction,
    Func<Vector3, Vector3, bool> trySwitchTargetFunc,
    Func<Guid, bool> trySetTargetByIdFunc,
    CameraFollowCalculator followCalculator,
    CameraLockOnRotationCalculator lockOnRotationCalculator,
    CameraFreeLookRotationCalculator freeLookRotationCalculator,
    CameraLookAtRotationCalculator lookAtRotationCalculator,
    CameraLockOnRangeChecker lockOnRangeChecker,
    CameraLockOnBreakTracker lockOnBreakTracker,
    CameraConfig viewSettings,
    Transform playerT,
    PlayerInputView playerInputView)
```

**15引数**。しかも明確な2つのData Clumpに分かれている。

| 塊 | 引数 | 置き換え先 |
| --- | --- | --- |
| ターゲット操作（6デリゲート） | `changeTargetAction` 〜 `trySetTargetByIdFunc` | `ITargetCommand` + `ITargetQuery`（[16. SOLID](16_SOLID原則.md) 参照） |
| カメラ計算（6クラス） | `followCalculator` 〜 `lockOnBreakTracker` | `CameraCalculatorSet` |

これを適用すると引数は **15 → 5** になる。

```csharp
public void Initialize(
    ITargetCommand targetCommand,
    ITargetQuery targetQuery,
    CameraCalculatorSet calculators,
    CameraConfig viewSettings,
    Transform playerT,
    PlayerInputView playerInputView)
```

さらに、この15引数が [02. Null参照リスク](02_Null参照リスクとアサート漏れ.md) で指摘した
「毎フレーム13個のnullチェック」の直接の原因になっている。
引数を減らせばそのチェックも自然に消える。

### その他の引数過多

| 場所 | 引数数 | 備考 |
| --- | --- | --- |
| [PlayerAttackController.cs:29](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Battle/PlayerAttackController.cs) | 約10 | コンストラクタ |
| [CharacterAnimationPlaybackMap.cs:21](../../Assets/Scripts/Runtime/4.View/InGame/Animation/CharacterAnimationPlaybackMap.cs) | 約10 | コンストラクタ |
| [SkillInputProgressAnimationSetting.cs:25](../../Assets/Scripts/Runtime/4.View/InGame/Skill/SkillUI/SkillInputProgressAnimationSetting.cs) | 約11 | 設定値の塊 → ScriptableObject化が適切 |
| [ACLikeRhythmGuideView.cs:570](../../Assets/Scripts/Runtime/4.View/InGame/Music/ACLikeRhythmGuideView.cs) | 約11（うち`out`が7） | `InitBeatGUI`。7つのout引数は返却用構造体にまとめるべき |
| [EnemyLifeCycle.cs:95](../../Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs) | 約9 | `Initialize` |
| [BossLifeCycle.cs:74](../../Assets/Scripts/Runtime/6.Composition/InGame/Enemy/Boss/BossLifeCycle.cs) | 約8 | `Initialize` |
| [PlayerView.cs:153](../../Assets/Scripts/Runtime/4.View/InGame/Player/PlayerView.cs) | 約8 | `Initialize` |
| [MissionRuntimeService.cs:23](../../Assets/Scripts/Runtime/2.Application/InGame/Mission/MissionRuntimeService.cs) | 約8 | コンストラクタ |
| [BossAIController.cs:25](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Enemy/Boss/BossAIController.cs) | 約8 | コンストラクタ |
| [SkillExecutionController.cs:21](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Skill/SkillExecutionController.cs) | 約8 | コンストラクタ |

`InitBeatGUI` の**7つの`out`引数**は特に読みにくい。

```csharp
private void InitBeatGUI(
    in GameObject parent, float beatWidth, float beatHeight, float scale,
    out int totalBeatBoxCount,
    out Image[] leftBeatImages, out Image[] rightBeatImages,
    out RectTransform[] leftBeatRT, out RectTransform[] rightBeatRT,
    out MotionHandle[] handles, out int[] justTimingBeatBoxIndex)
```

冒頭でoutを全てnullに初期化してから（583-589行）、失敗時は
`totalBeatBoxCount = 0` を設定して早期returnする（591-602行）という
**手続き的なエラー処理**になっている。

**修正方針**: 生成結果を保持する `BeatGuiElements` クラスを返す。
これは [16. SOLID](16_SOLID原則.md) で指摘した `BeatGuiBuilder` 切り出しと同じ作業になる。

### 【良い例】DTOによる引数集約

一方で、以下は既に正しく集約されている。

```csharp
// 1.Domain/InGame/Battle/AttackStepContext.cs
public AttackStepContext(AttackDefinition attackDefinition, IAttacker attacker,
                         IDefender defender, bool isJustHit = false, Damage baseDamage = default)

// 1.Domain/InGame/Player/PlayerMoveSpec.cs
public PlayerMoveSpec(MoveSpeed moveSpeed, AttackRotationSpeed attackRotationSpeed,
                      DodgeSpeed dodgeSpeed, DodgeDuration dodgeDuration,
                      DodgeCooldown dodgeCooldown, AttackCooldown attackCooldown)
```

`PlayerMoveSpec` は6引数だが、**すべてが型付き値オブジェクト**であり、
引数の取り違えがコンパイルエラーになる。これは良い設計。
`CameraSystemView.Initialize` の15引数もこの方向へ寄せるべき。
