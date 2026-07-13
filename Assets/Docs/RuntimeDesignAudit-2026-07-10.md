# Runtime 設計・拡張性監査レポート（2026-07-10）

## 1. 監査概要

`Assets/Scripts/Runtime` 全体を対象に、`Assets/Scripts/DesignPhilosophy.md` と現在の実装状態を照らし合わせて、設計上・拡張上の問題点を再監査した。

今回の結論は次の通りです。

- asmdef によるレイヤ分割自体は整っており、**名前空間・アセンブリの物理分離は概ねできている**。
- ただし、実運用上の依存は `ServiceLocator`・シーン検索・`async void Start` に強く寄っており、**設計思想の「Composition が依存性解決を担う」状態にはまだ到達していない**。
- その結果、モジュール追加時に「どこで誰が生成・登録・解放するのか」が読み取りにくく、初期化順・シーン構成・非同期処理の組み合わせで壊れやすい。

## 2. 良い点

- `Assets/Scripts/Runtime/0.Utility` 〜 `Assets/Scripts/Runtime/6.Composition` に asmdef があり、コンパイル単位は分割されている。
- `View` から `Application` / `Domain` を直接 `using` している箇所は、今回の機械検索では目立って見つからなかった。
- InGame / OutGame の初期化をフェーズ化する方向は正しく、今後の改善の軸として有効。

## 3. 主要な問題点

### A-1. グローバル依存がレイヤ境界を迂回している

設計思想では、他モジュール依存は Adaptor を介し、依存解決は Composition が担当する想定です。  
しかし実際には、`Application` / `View` / `InfraStructure` が直接 `ServiceLocator` に触れており、レイヤ境界を実質的に迂回しています。

#### 根拠

- `Assets/Scripts/Runtime/2.Application/OutGame/SkillTree/SkillTreeService.cs:21`
- `Assets/Scripts/Runtime/2.Application/OutGame/SkillBuild/SkillBuildUseCase.cs:25`
- `Assets/Scripts/Runtime/4.View/Persistent/Input/PlayerInputView.cs:20`
- `Assets/Scripts/Runtime/4.View/OutGame/Scenario/ScenarioInputView.cs:22`
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/SkillBuild/OwnedSkillRepository.cs:41`
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/SkillBuild/SkillBuildRepository.cs:43`

#### 問題

- `Application` が永続化基盤を自分で解決しており、純粋なユースケースとして差し替えにくい。
- `View` が自分で登録・取得しているため、View 同士の暗黙結合が生まれる。
- `InfraStructure` が `ServiceLocator` に依存すると、データ取得実装の再利用性が下がる。

#### 推奨

- `Application` には `ISavedataGateway` / `ISkillUnlockRepository` などをコンストラクタ注入する。
- `View` の自己登録をやめ、必要な `IViewModel` / `ISignal` / `Controller` は Composition から注入する。
- `InfraStructure` は `ServiceLocator` を見ず、必要な依存を引数で受け取る。

---

### A-2. 初期化アーキテクチャがまだ統一されていない

InGame / OutGame ともにフェーズ初期化へ寄せ始めていますが、まだ旧式の `async void Start` ベース初期化が複数残っています。

#### 根拠

- `Assets/Scripts/Runtime/6.Composition/InGame/Bootstrap/IngameComposition.cs:23`
- `Assets/Scripts/Runtime/6.Composition/OutGame/OutGameSceneInitializer.cs:26`
- `Assets/Scripts/Runtime/6.Composition/OutGame/Title/TitleSceneInitializer.cs:24`
- `Assets/Scripts/Runtime/6.Composition/OutGame/Title/TitleSceneInitializer.cs:57`
- `Assets/Scripts/Runtime/6.Composition/OutGame/Scenario/ScenarioCom.cs:25`
- `Assets/Scripts/Runtime/6.Composition/OutGame/Scenario/ScenarioCom.cs:54`
- `Assets/Scripts/Runtime/6.Composition/Persistent/PersistentEntryPoint.cs:21`

#### 問題

- モジュールごとに「どのフェーズで依存が保証されるか」が異なる。
- 初期化順不整合が発生すると、実行時まで壊れ方が見えない。
- OutGame 側は四段階初期化へ寄せている最中だが、`TitleSceneInitializer` のように旧方式が残っており、一貫性が崩れている。

#### 推奨

- `Persistent` / `OutGame` / `ScenarioScene` も含めて、初期化は `Init -> ResourceLoadAsync -> Build -> Ready -> Shutdown` に統一する。
- `Start` / `Awake` は「Coordinator 起動のみ」に制限する。
- フェーズ外で `ServiceLocator.GetInstanceAsync<T>()` を呼ぶ実装は段階的に廃止する。

---

### A-3. Composition がシーン検索に依存しており、モジュール契約が弱い

最近の方針では ModuleContainer を登録して依存を受け取る構造へ進んでいますが、Composition 内にはまだ `FindFirstObjectByType` / `FindAnyObjectByType` / `Camera.main` が散在しています。

#### 根拠

- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyInitializer.cs:53`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyInitializer.cs:124`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyInitializer.cs:277`
- `Assets/Scripts/Runtime/6.Composition/InGame/Sequence/SequenceInitializationModule.cs:31`
- `Assets/Scripts/Runtime/6.Composition/InGame/Sequence/SequenceInitializationModule.cs:34`
- `Assets/Scripts/Runtime/6.Composition/InGame/Bootstrap/SceneDependencyInitializationModule.cs:75`
- `Assets/Scripts/Runtime/6.Composition/InGame/Camera/CameraSystemInitializer.cs:50`
- `Assets/Scripts/Runtime/6.Composition/InGame/Player/PlayerInitializer.cs:119`

#### 問題

- ヒエラルキー構成が少し変わるだけで初期化が壊れる。
- Prefab 再利用時に「シーンに存在する前提」が埋め込まれる。
- 依存関係がコードから追いにくく、モジュール単位の拡張契約にならない。

#### 推奨

- シーン検索は `Bootstrap` 起点の一時的な収集だけに限定する。
- 収集した参照は `ModuleContainer` として登録し、他モジュールはそれだけを見る。
- `Find*` を使う場合でも、モジュール境界の入口 1 箇所へ閉じ込める。

---

### A-4. Runtime に開発用コード・一時コード・Editor 依存が残っている

DesignPhilosophy では Runtime にテスト機能や Editor 専用コードを含めない方針ですが、現在は複数残っています。

#### 根拠

- `Assets/Scripts/Runtime/6.Composition/InGame/Player/BuffSystemInit.cs:17`
- `Assets/Scripts/Runtime/2.Application/Player/SkillVisual/SkillVisualTest.cs:9`
- `Assets/Scripts/Runtime/2.Application/Player/SkillEffect/TestSkillEffect.cs:11`
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/SkillTree/SkillTreeTestInputData.cs:10`
- `Assets/Scripts/Runtime/6.Composition/OutGame/SkillBuild/SkillBuildDebugger.cs:11`
- `Assets/Scripts/Runtime/6.Composition/Persistent/Input/InputDebugLogger.cs:10`
- `Assets/Scripts/Runtime/4.View/OutGame/Setting/SettingBase.cs:2`

#### 問題

- Runtime ビルドに本番不要コードが混ざる。
- クラス名や API から「本番コードか開発補助か」が判断しづらい。
- `UnityEditor.SearchService` のような Editor 依存は、ビルドや将来の asmdef 整理時に事故要因になる。

#### 推奨

- 動作確認系は `Assets/Scripts/Develop` へ移す。
- Inspector 可視化専用デバッガーも `Develop` へ分離する。
- `SettingBase` の `UnityEditor.SearchService` は Runtime から除去する。

---

### B-1. 巨大クラスが責務を抱えすぎている

監査時点の行数上位を見ると、複数モジュールが「ロード・生成・イベント接続・状態更新・後始末」を 1 クラスに集約しています。

#### 行数の大きいクラス

- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs` : 721行
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs` : 537行
- `Assets/Scripts/Runtime/6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs` : 497行
- `Assets/Scripts/Runtime/6.Composition/OutGame/StageSelect/StageSelectInitializer.cs` : 465行
- `Assets/Scripts/Runtime/6.Composition/OutGame/Screen/ScreenInitializer.cs` : 437行
- `Assets/Scripts/Runtime/4.View/OutGame/Scenario/ScenarioView.cs` : 441行

#### 具体例

- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs:37`
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs:17`

#### 問題

- 小さな変更でも影響範囲が広い。
- 再利用可能な拡張点がクラス内部に埋もれる。
- テスト単位が大きくなり、段階的改善が難しい。

#### 推奨

- `EnemyLifeCycle` は「アセットロード」「戦闘初期化」「ターゲット登録」「演出/死亡処理」で分割する。
- `ScenarioRepository` は「ファイル選択」「CSV 読み込み」「Authoring 解析」「Normalized 解析」「EventFactory」に分割する。
- `Initializer` は「参照収集」「UseCase 生成」「View 配線」「購読開始」に分割する。

---

### B-2. Scenario 系が特に過密で、拡張点が閉じている

Scenario 周辺は、OutGame の中でも特に 1 ファイルあたりの責務密度が高いです。

#### 根拠

- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs:17`
- `Assets/Scripts/Runtime/6.Composition/OutGame/Scenario/ScenarioCom.cs:25`
- `Assets/Scripts/Runtime/4.View/OutGame/Scenario/ScenarioInputView.cs:13`

#### 問題

- シナリオフォーマットを追加すると `ScenarioRepository` の変更が大きくなる。
- 再生シーンの初期化が `ScenarioCom` 1 本に集中している。
- 入力・UI表示・シナリオ進行が連動しており、個別差し替えが難しい。

#### 推奨

- `ScenarioSceneInitializationModule` を作り、OutGame と同じフェーズ初期化へ統一する。
- `IScenarioDefinitionParser` を分け、CSV 形式ごとの差し替えを可能にする。
- `ScenarioInputView` は `PlayerInputView` を Locator 取得せず、Composition 注入へ寄せる。

---

### B-3. Addressables キーが文字列散在で、変更耐性が弱い

現在の Runtime は、Addressables キー文字列を多数の MonoBehaviour が直接持っています。

#### 根拠

- `Assets/Scripts/Runtime/6.Composition/OutGame/SkillTree/SkillTreeInitializer.cs:47`
- `Assets/Scripts/Runtime/6.Composition/OutGame/SkillBuild/SkillBuildInitializer.cs:39`
- `Assets/Scripts/Runtime/6.Composition/OutGame/Scenario/ScenarioCom.cs:32`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs:351`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/Boss/BossLifeCycle.cs:306`

#### 問題

- キー変更時の影響箇所が多い。
- 文字列 typo がコンパイル時に検出されない。
- 「どのキーがどのモジュールの契約か」がファイル単位で散る。

#### 推奨

- モジュール単位で `AssetKeySet` / `AddressableKeyConfig` を持たせる。
- 可能なら 1 モジュール 1 Config にまとめ、Composition は Config だけを受け取る。
- デバッグ用キーも同じ設定モデルにぶら下げる。

---

### B-4. 非同期 fire-and-forget が多く、停止条件が弱い

`async void` と `CancellationToken.None` がまだ複数残っています。

#### 根拠

- `Assets/Scripts/Runtime/6.Composition/OutGame/Title/TitleSceneInitializer.cs:57`
- `Assets/Scripts/Runtime/6.Composition/OutGame/Scenario/ScenarioCom.cs:54`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs:431`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyInfantrySpawner.cs:81`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyArtillerySpawner.cs:81`
- `Assets/Scripts/Runtime/6.Composition/OutGame/Scenario/ScenarioCom.cs:150`
- `Assets/Scripts/Runtime/2.Application/OutGame/Scenario/ScenarioUsecase.cs:71`

#### 問題

- 例外伝播が見えにくい。
- シーン破棄後も処理が残る危険がある。
- 再入や多重実行の制御が局所実装に寄る。

#### 推奨

- ライフサイクル起点以外の `async void` は `Awaitable` / `Task` へ寄せる。
- 画面遷移・シーン終了に関わる箇所で `CancellationToken.None` を避ける。
- fire-and-forget が必要な箇所は専用ランナーを用意する。

## 4. 優先度付き対応案

### 最優先

1. `Application` / `View` / `InfraStructure` からの `ServiceLocator` 直接利用を止める。
2. `TitleSceneInitializer` / `ScenarioCom` / `PersistentEntryPoint` をフェーズ初期化へ統一する。
3. Runtime に残っているテスト・デバッグ・Editor 依存コードを `Develop` / `Editor` へ移す。

### 次点

4. `FindFirstObjectByType` 系を ModuleContainer 契約へ置き換える。
5. Scenario 系と Enemy 系の巨大クラスを責務分割する。
6. Addressables キーをモジュール設定へ集約する。

### 最後にやるとよいもの

7. 非同期 fire-and-forget の整理。
8. ログや暫定コメントの棚卸し。

## 5. 総括

現在の Runtime は、**レイヤの見た目は整っているが、実運用の依存解決はまだグローバル参照とシーン前提に強く依存している**状態です。  
つまり、問題は「フォルダ構造」よりも **依存の取り方と初期化の統一不足** にあります。

今後の方針としては、以下の順が最も効果的です。

1. 低層の `ServiceLocator` 依存を止める。
2. 初期化ライフサイクルを全シーンで統一する。
3. 巨大クラスを契約単位へ割る。

この 3 点を進めると、設計思想にある「Composition が依存性解決を担い、他モジュール結合は Adaptor を介す」状態にかなり近づきます。
