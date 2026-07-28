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
`ServiceLocator.TryGetInstance` の定型ブロック（79箇所）をヘルパー化する際に、
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
