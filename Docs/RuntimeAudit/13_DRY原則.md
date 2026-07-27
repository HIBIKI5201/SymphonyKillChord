# 13. DRY原則（Don't Repeat Yourself）

DRYは「コードが似ている」ことではなく、**「知識が重複している」**ことを問題にする原則。
同じ判断・同じルールが複数箇所に書かれていると、片方だけ直して不整合になる。

大規模なファイル単位のコピペは [10. コード重複と過剰抽象化](10_コード重複と過剰抽象化.md) に
まとめてあるため、ここでは**「知識の重複」**に絞る。

---

## 【重大】敵の基礎攻撃力が3ファイルにハードコード重複

**場所**:

```csharp
// 2.Application/InGame/Enemy/EnemyAttackUsecase.cs:49
private Damage _baseDamage = new Damage(10); // TODO敵の基礎攻撃力があるはずなので、それを使用するようにする。

// 2.Application/InGame/Enemy/Boss/EnemyTripleShotAttackUsecase.cs:48
private Damage _baseDamage = new Damage(10); // TODO敵の基礎攻撃力があるはずなので、それを使用するようにする。

// 2.Application/InGame/Enemy/ShellAttackUsecase.cs:24
private Damage _baseDamage = new Damage(10);// TODO敵の基礎攻撃力があるはずなので、それを使用するようにする。
```

**同じTODOコメントごと3箇所にコピーされている**。これはDRY違反の典型例で、
- 「敵の基礎ダメージは10」というゲームバランス上の知識が3箇所に散在
- バランス調整時に1箇所直し忘れると、雑魚・ボス・砲弾でダメージが食い違う
- 3箇所とも「本来は敵データから取るべき」と分かっていながら放置されている

**修正方針**: 敵の `CombatSpec` / `EnemyDefinition` から取得する。
それが間に合わないなら、せめて共有の定数に一本化して重複を消す。

```csharp
// 1.Domain/InGame/Enemy/EnemyCombatDefaults.cs（暫定）
public static class EnemyCombatDefaults
{
    /// <summary> 敵データ未整備時の暫定基礎ダメージ。EnemyDefinition 実装後に削除する。 </summary>
    public static readonly Damage FallbackBaseDamage = new(10);
}
```

---

## 【重大】原点判定の閾値が2ファイルに複製

**場所**:
[EnemyRaycastDetectView.cs:342-345](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyRaycastDetectView.cs)、
[TripleShotRaycastDetectView.cs:334-337](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/Boss/TripleShotRaycastDetectView.cs)

```csharp
private bool IsEnemyOrigin(Vector3 sourcePosition)
{
    return (sourcePosition - transform.position).sqrMagnitude <= 0.0001f;
}
```

メソッド名・実装・マジックナンバーまで完全一致。
この2クラスは `FindClosestHit` / `CastAndGetHitCount` / `CreateRay` /
`ShouldUseLockedDirection` / `FreezeCurrentRayDirection` / `IsReadyForRaycast` など、
**ほぼすべてのprivateメソッドが重複**している。

同時に、両者には**片方だけ直されている差分**もある。

| 項目 | EnemyRaycastDetectView | TripleShotRaycastDetectView |
| --- | --- | --- |
| マテリアル管理 | `new Material` + `OnDestroy` で破棄 | `.material` 暗黙クローン・**破棄なし** |
| 警告表示 | DecalProjector | LineRenderer × 3 |

まさにDRY違反が引き起こす典型的な事故（片方だけ修正され、もう片方にバグが残る）が
既に発生している（[04. GC Alloc](04_GCAllocとメモリリーク.md) 参照）。

**修正方針**: レイキャスト判定部分を `RaycastTargetDetector` として抽出し、
警告表示（Decal / LineRenderer）だけをサブクラスまたはStrategyで差し替える。

---

## 【中】勝敗リザルトのDTO構築が重複

**場所**: [StageResultPresenter.cs:55-90](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Result/StageResultPresenter.cs)

`PresentVictory` と `PresentDefeat` が、9引数の `StageResultDTO` を
それぞれ組み立てている。共通なのは以下。

```csharp
_selectedBattleStageState.StageName,
_missionRuntimeService.MissionDefinition.MainMissionText,
FormatBattleTime(_missionRuntimeService.MissionProgress.ElapsedTime.Value),
_missionRuntimeService.MissionProgress.MaxCombo.Value.ToString(),
```

異なるのは `StageResultType`、クリア文言、サブミッション、ランク、Tipsの5つ。

**修正方針**: 共通部分を組み立てるprivateメソッドを切り出す。

```csharp
private StageResultDTO BuildDto(
    StageResultType type, string missionStateText,
    StageResultMissionItemDTO[] subMissions, string rank, string tip)
```

これによりDemeter違反のチェーン（[17. デメテルの法則とCQS](17_デメテルの法則とCQS.md) 参照）も
1箇所に集約される。

---

## 【中】音量マネージャの完全重複

**場所**:
[VoiceVolumeManager.cs](../../Assets/Scripts/Runtime/4.View/Persistent/Voice/VoiceVolumeManager.cs)、
[SoundEffectVolumeManager.cs](../../Assets/Scripts/Runtime/4.View/Persistent/Music/SoundEffectVolumeManager.cs)

`Register` / `UnRegister` / `SetVolume` / `GetVolume` と `List<T>` 管理が一字一句同一。
両ソース（`VoiceSource` / `SoundEffectSource`）は既に `IVolumeApplicable` を実装済み。

さらに [PersistentAudioVolumeRegistryView.cs](../../Assets/Scripts/Runtime/4.View/Persistent/PersistentAudioVolumeRegistryView.cs) の
Register/Unregisterメソッド4つも同じ理由で半減できる。

**修正方針**:
```csharp
public sealed class VolumeManager<T> where T : IVolumeApplicable { ... }
```

---

## 【中】ミッション条件グループの4重複

**場所**: `1.Domain/InGame/Mission/`

`AndClearConditionGroup` / `OrClearConditionGroup` / `AndFailConditionGroup` /
`OrFailConditionGroup`（各50行台）が、**ログ文言以外完全一致**。

「複数条件をANDで結合する」という知識が2箇所（Clear用とFail用）に、
「ORで結合する」という知識も2箇所に重複している。

**修正方針**: `CompositeCondition<TCondition>(bool requireAll)` に統合。
`IsSatisfied` の手書きforループも `_conditions.All(...)` / `.Any(...)` で置換できる。

---

## 【中】ServiceLocator解決の定型ブロックが79箇所

「サービス解決に失敗したらエラーログを出してfalseを返す」という知識が
25ファイル・79箇所に手書きで複製されている。

```csharp
if (!ServiceLocator.TryGetInstance(out SomeService x))
{
    Debug.LogError("...");
    return false;
}
```

[StageSelectInitializer.cs:262-315](../../Assets/Scripts/Runtime/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs) では
**6回連続で**この形が並んでいる。

詳細と修正方針は [08. 緊密結合とレイヤー違反](08_緊密結合とレイヤー違反.md) を参照。

---

## 【中】Setting系UIのバインド処理

**場所**: `4.View/OutGame/Setting/`

`SettingToggle` / `SettingSlider` / `SettingDropDown` が、
「`Q<T>()` で取得 → nullチェック → `SetValueWithoutNotify` → `RegisterValueChangedCallback`」
という**手順の知識**をそれぞれ再実装している。

**修正方針**: `SettingBase` に共通ヘルパーを置く。

```csharp
protected void BindControl<TControl, TValue>(
    string name, Func<TValue> getter, Action<TValue> setter)
    where TControl : VisualElement, INotifyValueChanged<TValue>
```

---

## 【小】画面UseCaseのsync/async二重実装

**場所**:
[CloseCurrentScreenUseCase.cs:28-73](../../Assets/Scripts/Runtime/2.Application/OutGame/Screen/CloseCurrentScreenUseCase.cs)、
[ShowScreenUseCase.cs:28-83](../../Assets/Scripts/Runtime/2.Application/OutGame/Screen/ShowScreenUseCase.cs)

`Execute()` と `Execute(CancellationToken)` で、結果DTOの構築ロジックが重複している。
「どういう結果を返すか」という知識が2箇所にある。

**修正方針**: `BuildResult()` をprivateに切り出し、2つのオーバーロードから呼ぶ。

---

## 【小】ScreenControllerの1行委譲4連

**場所**: [ScreenController.cs:29-56](../../Assets/Scripts/Runtime/3.Adaptor/OutGame/Screen/ScreenController.cs)

```csharp
public void ShowTitle()   => _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Title));
public void ShowMenu()    => _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Menu));
public void ShowOptions() => _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Options));
public void ShowCredit()  => _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Credit));
```

呼び出し元（[TitleSceneInitializer.cs](../../Assets/Scripts/Runtime/6.Composition/OutGame/Title/TitleSceneInitializer.cs)）は
C#コードから直接呼んでいるため、`Show(ScreenId id)` 1つで足りる。

---

## DRY適用時の注意: これらは重複ではない

過剰適用を避けるため、**見た目が似ていても統合すべきでないもの**を挙げる。

| 対象 | 統合すべきでない理由 |
| --- | --- |
| 値オブジェクト13種（`DodgeCooldown` / `MoveSpeed` 等） | 型で意味を分けることが目的。統合すると原則が崩れる。定型部分はSource Generator等で生成すべき |
| `IOneShotVisualEffect` の6実装 | 正当なStrategyパターン。各実装の中身は本質的に異なる |
| Mission系 `ConditionAsset` 群 | `SerializeReference` によるデータ駆動設計。Unity側の都合として妥当 |
| `Initialize` / `Build` / `Ready` の3フェーズ | 構造は似るが実行タイミングという異なる知識を表す（ただし [10](10_コード重複と過剰抽象化.md) のCoordinator実装重複は別問題） |
