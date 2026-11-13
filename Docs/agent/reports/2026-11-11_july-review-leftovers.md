# 2026-07 コードレビューの残り 8 件の処置（Issue #2757）

2026-07 のコードレビュー（Notion「7月β大改修リファクタリング / 疑問点」）で未解決だった 8 件を、2026-11-11 の develop（4af8ef28）と照らして処置した。作業ブランチは `feature/release/july-review-leftovers/agent`。

## 一覧

| # | 項目 | 処置 |
| --- | --- | --- |
| 1 | シーンのオブジェクトが混ざっている | **PC で行う**（未着手。下の「PC で行う作業」） |
| 2 | インゲームシーンの Stage オブジェクト（NavMeshData が missing） | **解決済み**。直すものは無い |
| 3 | `RhythmState` の名前 | **直した**。`RhythmInputBuffer` に改名 |
| 4 | `HealthHudView`・`HealthHudViewModel`（DTO をそのまま View へ渡す・イージング） | **直した** |
| 5 | セーブデータの定義がドメインにある | **直さない（提案）**。ユーザーの判断待ち |
| 6 | シーンのモジュールの層 | **直さない（提案）**。ユーザーの判断待ち |
| 7 | `SelectedBattleStageState` が無いとき | **直さない**。DebugHub で代わりになっている |
| 8 | テスト用の using が Runtime に残っている | **直した** |

## 直したもの

### 3. `RhythmState` → `RhythmInputBuffer`

- `Assets/Scripts/Runtime/1.Domain/InGame/Music/RhythmInputBuffer.cs`（`.meta` も `git mv` したので GUID は変わらない）。中身はリズム入力（`RhythmInputRecord`）を一定数だけ持つリングバッファなので、「状態」ではなく「入力バッファ」と名付けた。
- 使っているのは `MusicSyncService` だけ。フィールド名も `_rhythmInputBuffer` にした。シリアライズされない純粋クラスなので、シーン・プレハブへの影響は無い。
- 同じ形の `SkillRhythmState`（スキルの入力履歴）は今回の指摘の対象外なので残した。そろえるなら同じ手順で `SkillRhythmInputBuffer` にできる。

### 4. HP HUD の ViewModel

- `IHealthHudViewModel` は `ReactiveProperty<HealthHudDTO>` をやめ、表示に使う値を `CurrentHealth`・`MaxHealth`・`HealthRatio`（いずれも `ReadOnlyReactiveProperty<float>`）として出す。DTO は Adaptor（Presenter）から ViewModel への受け渡し（`UpdateHealth(in HealthHudDTO)`）にだけ使う。設計思想のクラス責務（ViewModel は DTO を受け取り、ReactiveProperty で表示データを持つ）に合わせた。
- 割合の計算（最大 HP が 0 以下なら 0）は ViewModel に寄せた。`UpdateHealth` は最大 HP → 現在 HP → 割合の順に更新するので、割合を購読する View は最新の最大 HP を読める。
- 書き換えた View: `HealthHudView`（敵の頭上 HP）、`HealthBarView`・`HealthTextView`（プレイヤーの HUD）。
- `HealthHudView` の赤いバーのイージングを `[SerializeField] Ease _healthBarEase`（既定 `OutExpo`、今までと同じ）にした。デバッグ用キー操作のアニメーションも同じ処理を通す。

### 8. 不要な using

- `TripleShotRaycastDetectView.cs` から `using UnityEngine.Assertions.Must;` を消した。ファイル内で使っていない。

## 直さないもの

### 2. Stage オブジェクトの NavMeshData（解決済み）

- 今の `Assets/Level/Scenes/Master/InGame.unity` には Stage オブジェクトも NavMeshSurface も無い（ルートは SceneManagersRoot・Canvases・EnemySpawning など 15 個）。`m_NavMeshData: {fileID: 0}` はシーン既定の NavMeshSettings で、コンポーネントではない。
- NavMeshSurface はステージのシーン（`Stages/Stage_01.unity`・`Stage_02.unity`）の `Map` にあり、それぞれ焼いた `NavMesh-Map.asset` を参照している（GUID が一致）。

### 5. セーブデータの定義がドメインにある（提案: 今のまま）

- `1.Domain/Persistent/Savedata/` の型は、Application（8 ファイル）と Adaptor（6 ファイル）が直接使っている。いちばん外側（InfraStructure）へ移すと、内側の層が外側を参照することになり、設計思想の依存の向きに反する。
- 保存の形式・暗号化・読み書き（`SaveDataPayloadFormat`・`SaveDataCrypto`・`EncryptedFileSaveDataLoaderStrategy`・各 Repository）は、すでに `5.InfraStructure/Persistent/Savedata/` にある。外部に保存するための処理は外側、保存する中身の定義は内側、という分け方になっている。
- `[Serializable]`・`[SerializeField]` が付いている点は、設計思想が「ピュア層での UnityEngine への依存は許容する」としているので違反ではない。

### 6. シーンのモジュールの層（提案: 今のまま）

- シーンのロードの実体は `5.InfraStructure/Persistent/SceneManagement/SceneTransitionService.cs` にあり、Application の `ISceneTransitionService` を実装して Composition が注入している。設計思想の「下層への参照は自層に interface を置き、下層が実装して Composition が注入する」どおりである。
- View 層は「ゲームオブジェクトやレンダリングの操作・入力の受け取り」の層で、シーンやアセットの読み込みはデータを取ってくる InfraStructure の役割に近い。View へ寄せる理由は見当たらない。

### 7. `SelectedBattleStageState` が無いとき（DebugHub で代替）

- 指摘の目的は「テストのため、ステージ未選択でもインゲームを始めたい」こと。今は DebugWorld の DebugHub（`Assets/Scripts/DebugWorld/Hub/DebugHubInitializer.cs`）が、チュートリアル以外の最初のバトルステージを選んで出撃できる。
- インゲームのシーンを直接開いて再生しても、常駐シーンの起動後にアンロードされる（`PersistentBootState` の注記）。Composition に既定ステージを入れても、この経路では使われない。
- 設計思想は「Runtime はマスタービルド向けで、テスト機能を含めない」としている。本番で未選択のまま始まるのは不具合なので、今のエラーで止める動きを残す。

## PC で行う作業（1. シーンの整理）

Unity エディタでの確認が要るので、クラウドでは手を付けていない。

- `InGame.unity` のルートに、エフェクトのプールなど（`ArtilleryExplosionEffectPool`・`DamageEffeectPool`・`ArtilleryExplosionSoundPool`・`Effect`・`Effect_1`）や、カメラ（`Dolly CinemachineCamera`・`Dolly Spline`）が並んでいる。空のオブジェクト（例: `Pools`・`Cameras`・`Effects`）でまとめる。
- 同じシーンのルートに、非表示の `EnemyArtillery(Clone)` が 2 つ保存されている（#2253 のマージ以降）。再生中に生成されたものが保存されたとみられる。参照されていないことを確かめて消す。
- まとめる前に、`transform.root`・`GetRootGameObjects()`・`DontDestroyOnLoad` に頼る処理が無いかを確かめる。
- OutGame（ルート 11 個）・Persistent（15 個）も同じ方針で見る。

## 未確認

- Unity でのコンパイル、PlayMode での HP 表示の確認はしていない（PC での一括確認に回す）。
