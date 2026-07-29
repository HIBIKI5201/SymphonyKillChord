# 02. Null参照リスクとアサート漏れ

## 調査サマリ

| 指標 | 値 |
| --- | --- |
| `ArgumentNullException` によるガードを持つファイル | 110 |
| `4.View` の `[SerializeField]` 総数 | 315 |
| `4.View` でSerializeFieldを持つがnullガードが一切ないファイル | 42 |

`3.Adaptor`(37) と `4.View`(26) はガードが厚い一方、`5.InfraStructure`(7) と
`6.Composition`(6) は薄い。**Composition層は依存を組み立てる場所であり、
ここでのnullは最も原因追跡が難しい**ため、本来は最も厚くすべき層である。

---

## 【確定・最優先】LogError後にreturnがなくNullReferenceExceptionへ進む

**場所**: [SkillTreeController.cs:98-102](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillTree/SkillTreeController.cs)

```csharp
if (_nodesOnPath == null || _nodesOnPath.Count == 0)
{
    Debug.LogError($"[SkillTreeController] 解放対象ノードの取得に失敗しました。");
    // ← return が無い
}
foreach (SkillNodeEntity entity in _nodesOnPath)   // _nodesOnPath が null ならここで落ちる
{
```

`_nodesOnPath` が null の場合、エラーログを出した直後の `foreach` で
`NullReferenceException` が発生する。ログを出す意図があるのに処理を止めていないため、
「ログが出た上でクラッシュする」という最悪の挙動になる。

さらに悪いのは、`Count == 0` のケースでは `foreach` は正常に0回転し、
その後 line 117 の `_skillTreeStatusEntity.ModifyPoint(-_costToUnlock)` で
**何も解放していないのにスキルポイントだけ消費される**点。

**修正方針**: `return` を追加し、null と空を分けて扱う。

```csharp
if (_nodesOnPath == null || _nodesOnPath.Count == 0)
{
    Debug.LogError($"[SkillTreeController] 解放対象ノードの取得に失敗しました。");
    return;
}
```

---

## 【確定】要素数を検証せずに配列末尾へアクセス

**場所**: [HUDEnemyHealthView.cs:92-98](../../Assets/Scripts/Runtime/4.View/InGame/UI/HUDEnemyHealthView.cs)

```csharp
private void Awake()
{
    _lockedOnSize  = _healthImage.rectTransform.sizeDelta;  // _healthImage 未検証
    _lockedOnColor = _healthImage.color;
    _index = _sprites.Length - 1;                            // _sprites 未検証
    _healthImage.sprite = _sprites[^1];                      // 空配列なら例外
}
```

`_healthImage` と `_sprites` はどちらも `[SerializeField]` で、Inspector未設定または
空配列の場合に `Awake` の時点で例外となる。このクラスはnullガードもログも一切持たない
（前掲の「SerializeFieldを持つがガードなし」42ファイルの1つ）。

加えて `RatioToIndex`（line 108-111）に軽微な設計上の歪みがある。

```csharp
return Mathf.Clamp(Mathf.RoundToInt(ratio * _sprites.Length), 0, _sprites.Length - 1);
```

`ratio = 1.0`（満タン）のとき `RoundToInt(1.0 * Length) = Length` となり、
Clampで `Length - 1` に丸められる。つまり**満タンと満タン直下が同じスプライトになる**。
`_sprites.Length - 1` を掛けるのが本来の意図と思われる。

**修正方針**: 対比として、同じ `4.View` の
[ReticleHudView.cs:27-38](../../Assets/Scripts/Runtime/4.View/InGame/Reticle/ReticleHudView.cs) は
`Awake` で `_markerPrefab` / `_markerRoot` を個別に `Debug.LogError` 付きで検証しており、
これが本プロジェクトの模範パターンになっている。同じ形式を横展開するのが良い。

---

## 【要検証】毎フレーム13個のnullチェックで初期化状態を判定している

**場所**: [CameraSystemView.cs:383-392](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs)

```csharp
private void Tick(float deltaTime)
{
    if (_changeTargetAction == null || _clearTargetAction == null || _getCurrentTargetPositionFunc == null
        || _updateCandidateAction == null || _trySetTargetByIdFunc == null || _followCalculator == null
        || _lockOnRotationCalculator == null || _freeLookRotationCalculator == null
        || _lookAtRotationCalculator == null || _lockOnRangeChecker == null || _lockOnBreakTracker == null
        || _playerT == null || _cameraT == null)
    {
        return;
    }
```

これらは全て `Initialize`（line 45-96）でまとめて代入される。したがって実質的に
判定したいのは「Initialize済みか」の1点であり、**13個の個別チェックは
毎フレーム実行される冗長な処理**になっている（`_playerT` / `_cameraT` は
UnityEngine.Object のためnullチェック自体がネイティブ呼び出しを伴う）。

また、この書き方では**どの依存が欠けていたのかが分からない**まま黙って
returnするため、カメラが動かない不具合の原因追跡が困難になる。

**修正方針**: `Initialize` の末尾で各依存を検証してログを出し、
`_isInitialized` フラグを立てる。`Tick` は `if (!_isInitialized) return;` だけにする。

---

## 【設計指摘】ArgumentNullExceptionによる防御の層別偏り

Presenter層は模範的にガードしている。

```csharp
// 3.Adaptor/InGame/UI/HUDEnemyHealthPresenter.cs:22-28
if (targetingSystem == null)
    throw new ArgumentNullException(nameof(targetingSystem), "TargetingSystemがNULL。");
if (viewModel == null)
    throw new ArgumentNullException(nameof(viewModel), "敵HPのViewModelがNULL。");
```

一方 `6.Composition` は 84ファイル中わずか6ファイルしかガードを持たない。
DIの組み立て役である以上、ここで検出できなかったnullは実行時の遠い場所で顕在化する。

**修正方針**: Composition層の各 `Initialize` / コンストラクタに
`ArgumentNullException` ガードを追加する。[08](08_緊密結合とレイヤー違反.md) で触れる
`ServiceLocator.TryGetInstance` の定型ブロック（85箇所）をヘルパー化する際に、
同時にnull検証も集約できる。

---

## 【設計指摘】Unity固有のnull意味論を跨いだ判定

**場所**: [VoiceSource.cs:65](../../Assets/Scripts/Runtime/4.View/Persistent/Voice/VoiceSource.cs)、
[SoundEffectSource.cs:72](../../Assets/Scripts/Runtime/4.View/Persistent/Music/SoundEffectSource.cs)

```csharp
_volumeRegistryView ??= FindAnyObjectByType<PersistentAudioVolumeRegistryView>();
```

`??=` はC#のnull合体代入であり、**Unityの「破棄済みオブジェクト」判定を通さない**。
`_volumeRegistryView` が破棄済み（Unity的にはnull相当だがCLR的には非null）の場合、
`??=` は代入をスキップし、破棄済み参照を使い続けて `MissingReferenceException` になる。

**修正方針**: `if (_volumeRegistryView == null) { _volumeRegistryView = ...; }` と書く。
Unityの `==` オーバーロードが効くため破棄済みも正しく検出できる。
なお [EnemyHealthBillboardView.cs:12-20](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyHealthBillboardView.cs)
と [CameraSystemView.cs:394-397](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs)
は `if (x == null)` 形式で書かれており、こちらが正しい。

---

## 【設計指摘】ShellLifeCycleの条件式によるnull補完

**場所**: [ShellLifeCycle.cs:63-64](../../Assets/Scripts/Runtime/6.Composition/InGame/Enemy/ShellLifeCycle.cs)

```csharp
if (!_musicSyncInitializer) _musicSyncInitializer = FindFirstObjectByType<MusicSyncInitializer>();
if (!_musicSyncView)        _musicSyncView        = FindAnyObjectByType<MusicSyncView>();
```

`!obj` による暗黙のbool変換はUnityの破棄済み判定が効くため動作としては正しい。
ただし、**取得に失敗した場合（シーンに存在しない場合）のログが無い**ため、
以降の処理で静かにnull参照する。`Find*` は毎回シーン全体を走査するため、
失敗時は毎回フルスキャンのコストも払う（[05](05_不必要な繰り返し処理.md) 参照）。

---

# 付録: 該当箇所の全列挙

## 付録A. `4.View` でSerializeFieldを持つがガードが無いファイル（全42件）

`[SerializeField]` を持ちながら `LogError` / `LogWarning` / `ArgumentNullException` /
`Assert` のいずれも含まないファイル。SerializeField数の降順。

**重要な注意**: 42件すべてが問題というわけではない。以下の3分類で扱いが異なる。

| 分類 | 対応 |
| --- | --- |
| **A. ScriptableObject設定クラス**（`*Config` 等） | Inspector上で必ず値が入る前提の設定値。ガードより `[Min]` / `[Range]` 属性による入力制約が適切 |
| **B. MonoBehaviour View**（参照が必須） | **ガード追加が必要**。[ReticleHudView.cs:27-38](../../Assets/Scripts/Runtime/4.View/InGame/Reticle/ReticleHudView.cs) が模範 |
| **C. データ保持のみ**（`*Entry` / `*Pair`） | 影響が小さい。優先度低 |

### 分類B（ガード追加を推奨）— 20件

| # | SF数 | ファイル | 備考 |
| --- | --- | --- | --- |
| 1 | 10 | [4.View/InGame/Enemy/EnemyMoveView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyMoveView.cs) | 571行の大型View |
| 2 | 9 | [4.View/InGame/Music/RhythmGuideView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Music/RhythmGuideView.cs) | 旧実装の可能性（[04](04_GCAllocとメモリリーク.md) 参照） |
| 3 | 7 | [4.View/OutGame/Scenario/CommandBarToggleView.cs](../../Assets/Scripts/Runtime/4.View/OutGame/Scenario/CommandBarToggleView.cs) | |
| 4 | 6 | [4.View/InGame/Result/StageResultMissionItemView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Result/StageResultMissionItemView.cs) | |
| 5 | 5 | [4.View/InGame/Sequence/StageSequenceMessageView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Sequence/StageSequenceMessageView.cs) | |
| 6 | 5 | [4.View/InGame/Mission/MissionEvaluationItemView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Mission/MissionEvaluationItemView.cs) | |
| 7 | 5 | [4.View/InGame/Camera/CameraSystemView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraSystemView.cs) | **毎フレーム13個のnullチェック**（本文参照） |
| 8 | 4 | [4.View/InGame/UI/HUDEnemyHealthView.cs](../../Assets/Scripts/Runtime/4.View/InGame/UI/HUDEnemyHealthView.cs) | **`Awake` で `_sprites[^1]` に無検証アクセス**（本文参照） |
| 9 | 4 | [4.View/InGame/Stage/StageEffectView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Stage/StageEffectView.cs) | |
| 10 | 4 | [4.View/InGame/Enemy/NearestAttackPositionSearchView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/NearestAttackPositionSearchView.cs) | `_agent` / `_raycastView` 未検証 |
| 11 | 3 | [4.View/Persistent/Load/LoadingScreenView.cs](../../Assets/Scripts/Runtime/4.View/Persistent/Load/LoadingScreenView.cs) | |
| 12 | 3 | [4.View/Persistent/Input/MobileInput.cs](../../Assets/Scripts/Runtime/4.View/Persistent/Input/MobileInput.cs) | |
| 13 | 3 | [4.View/InGame/Skill/SkillView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Skill/SkillView.cs) | |
| 14 | 3 | [4.View/InGame/Mission/MissionStepPopupView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Mission/MissionStepPopupView.cs) | |
| 15 | 2 | [4.View/InGame/UI/ParticleController.cs](../../Assets/Scripts/Runtime/4.View/InGame/UI/ParticleController.cs) | |
| 16 | 2 | [4.View/InGame/Skill/SkillUI/SkillCrosshairStepView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Skill/SkillUI/SkillCrosshairStepView.cs) | |
| 17 | 2 | [4.View/InGame/Music/RhythmGuideLabelView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Music/RhythmGuideLabelView.cs) | |
| 18 | 2 | [4.View/InGame/Enemy/Boss/BossMoveView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/Boss/BossMoveView.cs) | |
| 19 | 1 | [4.View/OutGame/Scenario/ScenarioUIHideView.cs](../../Assets/Scripts/Runtime/4.View/OutGame/Scenario/ScenarioUIHideView.cs) | |
| 20 | 1 | [4.View/InGame/UI/IngameHudView.cs](../../Assets/Scripts/Runtime/4.View/InGame/UI/IngameHudView.cs) | `_healthBarImage` 未検証 |

### 分類A（設定クラス。`[Min]`/`[Range]` 属性で対応）— 12件

| # | SF数 | ファイル |
| --- | --- | --- |
| 21 | 25 | [4.View/InGame/Camera/CameraConfig.cs](../../Assets/Scripts/Runtime/4.View/InGame/Camera/CameraConfig.cs) |
| 22 | 19 | [4.View/InGame/Music/ACLikeRhythmGuideEffectConfig.cs](../../Assets/Scripts/Runtime/4.View/InGame/Music/ACLikeRhythmGuideEffectConfig.cs) |
| 23 | 18 | [4.View/InGame/Sequence/StageStartSequenceConfig.cs](../../Assets/Scripts/Runtime/4.View/InGame/Sequence/StageStartSequenceConfig.cs) |
| 24 | 12 | [4.View/InGame/Skill/SkillUI/SkillInputProgressUIConfig.cs](../../Assets/Scripts/Runtime/4.View/InGame/Skill/SkillUI/SkillInputProgressUIConfig.cs) |
| 25 | 4 | [4.View/InGame/Skill/SkillUI/SkillBeatVisualSettingConfig.cs](../../Assets/Scripts/Runtime/4.View/InGame/Skill/SkillUI/SkillBeatVisualSettingConfig.cs) |
| 26 | 3 | [4.View/InGame/Animation/CharacterAnimationCatalogConfig.cs](../../Assets/Scripts/Runtime/4.View/InGame/Animation/CharacterAnimationCatalogConfig.cs) |
| 27 | 2 | [4.View/OutGame/Setting/ScreenConfig.cs](../../Assets/Scripts/Runtime/4.View/OutGame/Setting/ScreenConfig.cs) |
| 28 | 2 | [4.View/InGame/Player/PlayerAttackWeaponConfig.cs](../../Assets/Scripts/Runtime/4.View/InGame/Player/PlayerAttackWeaponConfig.cs) |
| 29 | 1 | [4.View/InGame/Player/PlayerAttackAnimationConfig.cs](../../Assets/Scripts/Runtime/4.View/InGame/Player/PlayerAttackAnimationConfig.cs) |
| 30 | 3 | [4.View/InGame/Character/VisualEffectGraphOneShotVisualEffect.cs](../../Assets/Scripts/Runtime/4.View/InGame/Character/VisualEffectGraphOneShotVisualEffect.cs) |
| 31 | 2 | [4.View/InGame/Stage/GameObjectActivationOneShotVisualEffect.cs](../../Assets/Scripts/Runtime/4.View/InGame/Stage/GameObjectActivationOneShotVisualEffect.cs) |
| 32 | 2 | [4.View/InGame/Stage/AnimatorTriggerOneShotVisualEffect.cs](../../Assets/Scripts/Runtime/4.View/InGame/Stage/AnimatorTriggerOneShotVisualEffect.cs) |

`ACLikeRhythmGuideEffectConfig`（#22）に `[Min(0.01f)]` を付ければ、
[09. マジックナンバー](09_マジックナンバーと定数化.md) で指摘した
`Mathf.Max(0.01f, ...)` のクランプ4箇所が実装から消える。

### 分類C（データ保持のみ。優先度低）— 10件

| # | SF数 | ファイル |
| --- | --- | --- |
| 33 | 4 | [4.View/InGame/Player/PlayerAttackAnimationEntry.cs](../../Assets/Scripts/Runtime/4.View/InGame/Player/PlayerAttackAnimationEntry.cs) |
| 34 | 2 | [4.View/InGame/Enemy/SpawnPositionPair.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/SpawnPositionPair.cs) |
| 35 | 2 | [4.View/InGame/Character/SoundEffectOneShotVisualEffect.cs](../../Assets/Scripts/Runtime/4.View/InGame/Character/SoundEffectOneShotVisualEffect.cs) |
| 36 | 2 | [4.View/InGame/Character/ParticleSystemPoolView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Character/ParticleSystemPoolView.cs) |
| 37 | 1 | [4.View/InGame/Enemy/EnemyHealthBillboardView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyHealthBillboardView.cs) | 
| 38 | 1 | [4.View/InGame/Character/ReusableParticleSystemView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Character/ReusableParticleSystemView.cs) |
| 39 | 1 | [4.View/InGame/Character/PooledParticleSystemOneShotVisualEffect.cs](../../Assets/Scripts/Runtime/4.View/InGame/Character/PooledParticleSystemOneShotVisualEffect.cs) |
| 40 | 1 | [4.View/InGame/Character/ParticleSystemRingBufferView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Character/ParticleSystemRingBufferView.cs) |
| 41 | 1 | [4.View/InGame/Character/ParticleSystemOneShotVisualEffect.cs](../../Assets/Scripts/Runtime/4.View/InGame/Character/ParticleSystemOneShotVisualEffect.cs) |
| 42 | 1 | [4.View/InGame/Animation/CharacterAnimationView.cs](../../Assets/Scripts/Runtime/4.View/InGame/Animation/CharacterAnimationView.cs) |

**着手順の推奨**: 分類Bの #7・#8・#10（既知の不具合または大型View）→ 残りの分類B →
分類A（属性化）→ 分類C。

## 付録B. `??=` によるUnity非対応のnull補完（全2件）

Unityの「破棄済みオブジェクト」判定を通さないため、
`if (x == null)` 形式へ書き換えるべき箇所。

| # | 場所 | コード |
| --- | --- | --- |
| 1 | [4.View/Persistent/Voice/VoiceSource.cs:65](../../Assets/Scripts/Runtime/4.View/Persistent/Voice/VoiceSource.cs) | `_volumeRegistryView ??= FindAnyObjectByType<PersistentAudioVolumeRegistryView>();` |
| 2 | [4.View/Persistent/Music/SoundEffectSource.cs:72](../../Assets/Scripts/Runtime/4.View/Persistent/Music/SoundEffectSource.cs) | `_volumeRegistryView ??= FindAnyObjectByType<PersistentAudioVolumeRegistryView>();` |

なお `SaveBase.cs:101` の `_filePath ??= Path.Combine(...)` は
`string` に対する `??=` であり **Unityのnull意味論と無関係なので問題ない**。
