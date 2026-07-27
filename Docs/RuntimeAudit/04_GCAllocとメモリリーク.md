# 04. GC Allocとメモリリーク（ロード画面以外）

ゲームプレイ中（ロード画面を除く）に発生するヒープ確保と、破棄されないリソースを対象とする。

## 前提: 本プロジェクトの良い点

指摘の前に評価すべき点を明記する。

- DTOの多くが `readonly ref struct` で定義されており、毎フレーム生成してもGCが発生しない
  （[HUDEnemyHealthDTO.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/UI/HUDEnemyHealthDTO.cs)、
  [RhythmGuideDto.cs](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Music/RhythmGuideDto.cs)）
- `Physics.RaycastNonAlloc` + 事前確保した `RaycastHit[]` を使用
  （[EnemyRaycastDetectView.cs:153](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyRaycastDetectView.cs)）
- `List` / `Dictionary` / `HashSet` / `Stack` を `readonly` フィールドとして事前確保し使い回す
  （[ReticleHudView.cs:126-130](../../Assets/Scripts/Runtime/4.View/InGame/Reticle/ReticleHudView.cs)）
- LINQは `6.Composition` の3ファイルのみで、ホットパスに存在しない

以下は、その水準から外れている箇所。

---

## 【確定・最優先】LineRenderer.materialの暗黙クローンが破棄されない

**場所**: [TripleShotRaycastDetectView.cs:254-267](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/Boss/TripleShotRaycastDetectView.cs)

```csharp
_lineRenderers[AIM_LINE_INDEX_LEFT].material.SetColor("_EmissionColor", _currentLineColor);
_lineRenderers[AIM_LINE_INDEX_CENTER].material.SetColor("_EmissionColor", _currentLineColor);
_lineRenderers[AIM_LINE_INDEX_RIGHT].material.SetColor("_EmissionColor", _currentLineColor);
```

`Renderer.material` は **初回アクセス時にマテリアルを複製する**（`sharedMaterial` と異なる）。
このクラスには `OnDestroy` が存在しないため、複製された3つのMaterialインスタンスは
**ボスが破棄されても解放されない**。ボスの出現回数だけリークが累積する。

決定的なのは、**同じ役割の姉妹クラスが正しく実装されている**こと。

```csharp
// 4.View/InGame/Enemy/ShellView.cs:33-34, 104-110  ← 正しい
_material = new Material(_indicator.material);
_indicator.material = _material;
...
private void OnDestroy()
{
    if (_material != null) { Destroy(_material); _material = null; }
}
```

```csharp
// 4.View/InGame/Enemy/EnemyRaycastDetectView.cs:219-220, 192-199  ← 正しい
_decalMaterial = new Material(_attackWarningDecal.material);
_attackWarningDecal.material = _decalMaterial;
...
private void OnDestroy()
{
    if (_decalMaterial != null) { Destroy(_decalMaterial); _decalMaterial = null; }
}
```

**修正方針**: `TripleShotRaycastDetectView` に `Initialize` でのマテリアル複製保持と
`OnDestroy` での `Destroy` を追加し、既存2クラスと同じ形に揃える。

---

## 【確定・最優先】NavMeshPath.cornersを探索ループ内で繰り返し参照

**場所**: [NearestAttackPositionSearchView.cs:64-85](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/NearestAttackPositionSearchView.cs)

```csharp
if (_agent.CalculatePath(_positionSamples[i], _path))
{
    if (_path.status == NavMeshPathStatus.PathComplete)
    {
        float pathLength = 0;
        for (int j = 1; j < _path.corners.Length; j++)          // ← 毎回プロパティアクセス
        {
            pathLength += Vector3.Distance(_path.corners[j - 1], _path.corners[j]);  // ← さらに2回
        }
```

`NavMeshPath.corners` は**プロパティであり、内部で `Vector3[]` を確保して返す**。
Unityが `GetCornersNonAlloc(Vector3[])` を用意しているのはこのため。

このコードでは、
- `CalculatePath` を `_samplingCount` 回呼ぶたびにパスが再計算され、`corners` が再確保される
- さらにループ条件と本体で**1イテレーションあたり3回** `corners` にアクセスしている

`_searchInterval`（0〜1秒、SerializeField）ごとに、敵1体あたり `_samplingCount` 個の
配列確保が発生する。敵が同時に10体いれば無視できない量になる。

**修正方針**: 事前確保したバッファに `GetCornersNonAlloc` で書き出す。

```csharp
// フィールド
private Vector3[] _cornerBuffer = new Vector3[MAX_CORNERS];

// ループ内
int cornerCount = _path.GetCornersNonAlloc(_cornerBuffer);
float pathLength = 0f;
for (int j = 1; j < cornerCount; j++)
{
    pathLength += Vector3.Distance(_cornerBuffer[j - 1], _cornerBuffer[j]);
}
```

なお、そもそもこの探索処理自体が重い（[05. 不必要な繰り返し処理](05_不必要な繰り返し処理.md) 参照）。

---

## 【確定】IReadOnlyListに対するforeachで毎フレーム列挙子がボクシングされる

**場所**: [RhythmGuidePresenter.cs:43](../../Assets/Scripts/Runtime/3.Adaptor/InGame/Music/RhythmGuidePresenter.cs)

```csharp
foreach (RhythmJudgmentRange range in _rhythmGuideUsecase.RhythmJudgmentDefinition.JudgmentRanges)
```

`JudgmentRanges` の型は `IReadOnlyList<RhythmJudgmentRange>` である
（[RhythmJudgmentDefinition.cs:26](../../Assets/Scripts/Runtime/1.Domain/InGame/Music/RhythmJudgmentDefinition.cs)）。

`List<T>` を直接 `foreach` すると構造体列挙子が使われGCは発生しないが、
**インターフェース経由だと `IEnumerator<T>` にボクシングされヒープ確保が起きる**。

`CreateDto()` は毎フレーム呼ばれる（[ACLikeRhythmGuideViewModel.cs:36](../../Assets/Scripts/Runtime/4.View/InGame/Music/ACLikeRhythmGuideViewModel.cs)、
[RhythmGuideUpdateView.cs:59](../../Assets/Scripts/Runtime/4.View/InGame/Music/RhythmGuideUpdateView.cs)）ため、
**毎フレーム確実にGC Allocが発生する**。

**修正方針**: `RhythmJudgmentDefinition` に `List<RhythmJudgmentRange>` を返す内部プロパティを
追加するか、`for` ループ + インデクサに変更する。後者が最も影響が小さい。

```csharp
IReadOnlyList<RhythmJudgmentRange> ranges = _rhythmGuideUsecase.RhythmJudgmentDefinition.JudgmentRanges;
for (int i = 0; i < ranges.Count; i++) { ... }
```

同種の `foreach` は以下にもあるが、いずれも毎フレームではないため優先度は低い。

- [StageTree.cs:271](../../Assets/Scripts/Runtime/1.Domain/OutGame/StageSelect/StageTree.cs) — `_nodes.Keys`
- [ScenarioView.cs:355](../../Assets/Scripts/Runtime/4.View/OutGame/Scenario/ScenarioView.cs) — `_portraitBySlot.Values`
- [SkillTreeInitializer.cs:334,483](../../Assets/Scripts/Runtime/6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs)
- [ScreenViewRegistry.cs:86,97](../../Assets/Scripts/Runtime/6.Composition/OutGame/Screen/ScreenViewRegistry.cs)

---

## 【要検証】シェーダプロパティを毎フレーム文字列で解決

**場所**: [EnemyRaycastDetectView.cs:328-336](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyRaycastDetectView.cs)

```csharp
if (_decalMaterial.HasProperty("_BaseColor")) { _decalMaterial.SetColor("_BaseColor", appliedColor); }
if (_decalMaterial.HasProperty("_Color"))     { _decalMaterial.SetColor("_Color", appliedColor); }
```

このメソッドは `UpdateWarningLine()`（line 242）から呼ばれ、
`UpdateWarningLine` は `LateUpdate`（line 185-191）から**警告表示中は毎フレーム**呼ばれる。

文字列版のAPIは内部で `Shader.PropertyToID` によるハッシュ計算と辞書引きを行う。
1フレームあたり `HasProperty` × 2 + `SetColor` × 2 = 4回の文字列解決。

さらに `HasProperty` を毎フレーム評価しているが、**マテリアルは変わらないので結果も変わらない**。
初期化時に一度判定してフラグに持てば済む。

**修正方針**: `static readonly int` にIDをキャッシュし、対応プロパティも初期化時に確定する。

```csharp
private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");
private static readonly int COLOR_ID      = Shader.PropertyToID("_Color");
private int _activeColorPropertyId;   // InitializeWarningDecal で決定
```

同様の文字列指定は以下にもある。

- [TripleShotRaycastDetectView.cs:255,260,265](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/Boss/TripleShotRaycastDetectView.cs) — `"_EmissionColor"`（毎フレーム × 3）
- [ShellView.cs:84](../../Assets/Scripts/Runtime/4.View/InGame/Enemy/ShellView.cs) — `"_Circle"`（イベント駆動のため影響小）

---

## 【要検証】毎フレーム呼び出されるリズムガイド更新が二重に存在する

`RhythmGuidePresenter.CreateDto()` を毎フレーム呼ぶ経路が2つある。

| 呼び出し元 | 駆動方法 |
| --- | --- |
| [ACLikeRhythmGuideViewModel.cs:36](../../Assets/Scripts/Runtime/4.View/InGame/Music/ACLikeRhythmGuideViewModel.cs) | `ACLikeRhythmGuideView.Update()` → `OnUpdate` イベント |
| [RhythmGuideUpdateView.cs:59](../../Assets/Scripts/Runtime/4.View/InGame/Music/RhythmGuideUpdateView.cs) | 自身の `Update()` |

両方がシーン上でアクティブな場合、`CreateDto()` が**毎フレーム2回**実行され、
上記の列挙子ボクシングも2倍になる。旧実装（`RhythmGuideView`）と
新実装（`ACLikeRhythmGuideView`）の移行途中である可能性が高い。

**要確認**: シーン上で両方が有効になっていないか。片方が旧実装なら削除対象。

---

## 【設計指摘】ロード時のGCは許容範囲内

`Debug.Log` に伴う文字列補間が422箇所中226箇所ある（[12. ログ運用](12_ログ運用とビルド影響.md) 参照）が、
毎フレーム実行される箇所は確認されなかった。`EnemyWaveTimerView.FixedUpdate` 内の
`Debug.Log`（line 68）もWaveタイムアウト時のみで、毎フレームではない。

---

## 【設計指摘】プール実装の一貫性

`ParticleSystemPoolView` は `UnityEngine.Pool.ObjectPool<T>` を使用、
`ReticleHudView` は自前の `Stack<T>` プール（line 99-116）を使用している。

後者は `Instantiate` のフォールバックとShow/Hideの切り替えのみで簡潔なため、
必ずしも `ObjectPool` へ統一する必要はない。ただし規約として
どちらを標準とするかは決めておくのが望ましい。
