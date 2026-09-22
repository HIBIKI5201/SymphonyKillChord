# 初期化ライフサイクル

- page_id: 3bf7c2c6-cc02-81fc-afe1-dbe0d028cda1
- Notion パス: Symphony Kill Chord / システム概要 / 初期化ライフサイクル
- スナップショットの最終更新: 2026-08-18T15:08:07.074Z
- 書き込み許可: 可
- 処置: 本文置き換え

## 判定

- 信頼性: 一部古い — クラス表とフェーズ定義は実装どおり（`Assets/Scripts/Runtime/6.Composition/Bootstrap/InitializationCoordinator.cs`、各 `~InitializationModuleBase`）。ただし Order の実例が 9 件だけで、実装は 51 モジュール（`override int Order` の全件）。エントリポイントに `PersistentEntryPoint` と `OutGameSceneInitializer` が無い。失敗時の挙動・進捗報告・収集条件・OutGame の復帰画面（#1529）が無い。「取得したい相手より大きい Order を持たせる」の理由付けが、全モジュールで 1 フェーズを終えてから次へ進む実装（`InitializationCoordinator.cs` の `InitializeAsync`）と合わない。最終確認 2026-09-22 / 73fefeb45
- 整合性:
  - 子 ② Order によるモジュール間の依存解決 (3bf7c2c6-cc02-81e1-836b-d675aecbfd17) の例（Target 100 が Build で登録、Camera 600 が Ready で取得）は、Order を逆にしても成功する。子 ② も直した
  - 50_基盤・ツール・規約.md の IF-02 の一覧には `SettingComposition`（OutGame 140）、`InputComposition`（Persistent 50）、`PersistentEntryPoint`（Persistent 1000）、`ScenarioCom`（OutGame 10）、`SkillEffectInitializer`（InGame 440）が抜けていた。本原稿では `git grep "override int Order"` の全件を載せた
  - IF-01 は「Persistent だけ同じシーンのモジュールに絞る」としていたが、OutGame も同じシーンに絞っている（`OutGameSceneInitializer.cs:160-174`）
  - システム概要 / シーンマネージャー (39e7c2c6-cc02-8009-8530-cc10a4396fa5) にも IF-03（復帰画面）の反映先候補がある。こちらを正とし、シーンマネージャー側はリンクだけにするのがよい（他領域）
- 変更点:
  - 概要: 最終更新日を 2026-09-22 にした
  - 🏗️ クラス: `IInitializationModule`、`InitializationCoordinator<T>`、`PersistentEntryPoint`、`OutGameSceneInitializer` の行を追加した
  - 🔁 初期化フェーズ: 旧: 「Order はモジュール間の依存の向きを表す。取得したい相手より大きい値を持たせる。実例として、Targetが100、…」 → 新: 1 フェーズを全モジュールで終えてから次へ進むこと、Order が効くのは同じフェーズの中での登録と取得であること、シーン別の Order 全一覧の表、同じ Order 同士の順序は保証されないこと
  - 🧩 Composition初期化情報: 旧: 「`IngameComposition`（インゲーム）と、各シーンのCoordinator」 → 新: シーン別のエントリポイント 3 つ
  - 🔗 モジュール結合: 旧: 「Coordinator → ISceneInitializationReadiness（初期化完了を通知）」 → 新: 通知するのはエントリポイント（`Complete` を呼ぶのは `IngameComposition` / `OutGameSceneInitializer` / `PersistentEntryPoint`）。エントリポイントの箱に 3 クラスを並べた
  - 🔌 拡張ポイント: 「シーン上に配置されていれば Coordinator が拾う」に、有効なものだけ・同じシーンのものだけの条件を足した
  - ⚠️ 失敗時の挙動と進捗報告（新しい節、末尾）: フェーズの戻り値、例外とキャンセル、進捗、Shutdown の順序、OutGame の復帰画面を追加した
- 決定（2026-09-22 八幡）: R77 同じ Order で問題ないモジュールは許容する（禁止しない）。「🔁 初期化フェーズ」の【要確認】を外し、「同じ Order で問題ないものは同じ値のままでよい。順序に依存するものは Order を分ける」に置き換えた
- 決定（2026-09-22 八幡）: R54 全表示を日英に対応させる（#2067）。「⚠️ 失敗時の挙動と進捗報告」の復帰画面の文言の【要確認】を外した。旧: 「コードに直書き。ローカライズ対象外」 → 新: 「言語設定が English なら英語の文言を出す（日英ともコードに直書き）」。実装は既に日英に対応している（`OutGameSceneInitializer.cs:225-235`。シーンマネージャーの原稿と同じ）
- 織り込んだ反映項目: IF-01, IF-02, IF-03
- 出典: `InitializationCoordinator.cs`（`InitializeAsync`、`RunSynchronousPhase`、`LogPhaseFailure`）、`IngameComposition.cs:140-163,192-206,213-219`、`PersistentEntryPoint.cs:26,39-65,106-118,160-187`、`OutGameSceneInitializer.cs:100-125,160-206,242-295`、`OutGameInitializationFailureView.cs:48-57`、`6.Composition/**` の `override int Order`（51 モジュール）、`ServiceLocator.RegisterInstance` を呼ぶフェーズ（`ScenarioCom.cs:184-217` だけが Ready で登録。Screen / StageSelect / SkillTree / SkillBuild / Title / InGameMission は Build から呼ぶ `Initialize` 系で登録）、PR #1529
- 要確認:
  - 復帰画面の実機での表示（#1529 本文で未実施、実機確認）

## 適用する本文

# 概要 {color="gray_bg"}
> 💡 **モジュール概要**
> 各シーンの初期化ライフサイクルを提供する基盤モジュールである。他モジュールの`~Initializer`はここで定義された基底クラスを継承し、`Order`の順に`Init → ResourceLoadAsync → Build → Ready`の各フェーズが実行される。
| 項目 | 内容 |
|---|---|
| **モジュール名** | Bootstrap |
| **カテゴリ** | 共通（InGame / OutGame / Persistent） |
| **ステータス** | 実装済み |
| **最終更新日** | 2026-09-22 |
---

## 🏗️ クラス
| クラス名 | レイヤー | 役割・機能 |
|---|---|---|
| `**IInitializationModule**` | Composition | 初期化モジュールの共通契約（`ModuleName`・`Order`・各フェーズ） |
| `**IInGameInitializationModule**` / `**IOutGameInitializationModule**` / `**IPersistentInitializationModule**` | Composition | 各シーンの初期化モジュールの共通契約 |
| `**InGameInitializationModuleBase**` | Composition | インゲームの初期化モジュール基底。他モジュールの`~Initializer`はこれを継承する |
| `**OutGameInitializationModuleBase**` / `**PersistentInitializationModuleBase**` | Composition | アウトゲーム・常駐シーンの初期化モジュール基底 |
| `**InitializationCoordinator<T>**` | Composition | フェーズの実行順・失敗時の停止・進捗報告を持つ共通の基底 |
| `**InGameInitializationCoordinator**` / `**OutGameInitializationCoordinator**` / `**PersistentInitializationCoordinator**` | Composition | 各シーンの初期化モジュールをフェーズ順に実行する |
| `**IngameComposition**` | Composition | インゲームの初期化ライフサイクルを実行する入口 |
| `**OutGameSceneInitializer**` | Composition | アウトゲームの初期化ライフサイクルを実行する入口。自身もOrder 0のモジュールで、初期化失敗時は復帰画面を出す |
| `**PersistentEntryPoint**` | Composition | 常駐シーンの初期化ライフサイクルを実行する入口 |
| `**SceneDependencyInitializationModule**` / `**SceneDependencyModuleContainer**` | Composition | シーン依存サービスの待機と公開 |

### 🔁 初期化フェーズ
基底クラスが定義する仮想メソッドを、Coordinatorが`Order`の昇順で呼ぶ。1つのフェーズを全モジュールで終えてから、次のフェーズへ進む。
| フェーズ | 役割 |
|---|---|
| `**Init**` | 自分の中で完結する初期化。他モジュールへ触らない |
| `**ResourceLoadAsync**` | Addressables等の非同期読み込み。`Awaitable`を返す |
| `**Build**` | 自分のオブジェクトを組み立て、ServiceLocatorへ登録する |
| `**Ready**` | 他モジュールをServiceLocatorから取得して結線する |
| `**Shutdown**` | 破棄処理 |
`Order`はモジュール間の依存の向きを表す。取得したい相手より大きい値を持たせる。
全モジュールの`Build`が終わってから`Ready`が始まるため、`Build`で登録したものを`Ready`で取得する場合は`Order`の大小に関係なく取得できる。`Order`が効くのは、同じフェーズの中で登録と取得をする場合である（`Build`の中で他モジュールを取得する、`Ready`の中で登録したものを他モジュールが`Ready`で取得する、など）。現状、`Ready`の中で登録するのは`ScenarioCom`（`PendingNodeTransitionState`が未登録のときだけ）で、ほとんどのモジュールは`Build`で登録して`Ready`で取得している。
同じ`Order`のモジュール同士の実行順は保証されない（収集は`FindObjectsSortMode.None`で、インゲームと常駐は`OrderBy`、アウトゲームは`List.Sort`で並べるため）。同じ`Order`で問題ないモジュールは同じ値のままでよい。順序に依存するものは`Order`を分ける。
現在の`Order`は次のとおりである（コードの`override int Order`。種別は継承する基底クラスで、Persistent / OutGame / InGame の順に並べた）。
| 基底の種別 | Order | モジュール |
|---|---|---|
| Persistent | 0 | SceneTransitionInitializer |
| Persistent | 10 | SavedataSystemInitializer |
| Persistent | 20 | MusicPlayerInitializer / InitialSkillLoadoutInitializer |
| Persistent | 30 | SoundEffectInitializer |
| Persistent | 32 | EnvironmentAppliersInitializer |
| Persistent | 35 | AudioSettingsInitializer |
| Persistent | 36 | UISoundEffectInitializer |
| Persistent | 37 | EnvironmentSettingsInitializer |
| Persistent | 40 | CameraInitializer / EventNotificationInitializer |
| Persistent | 50 | InputComposition |
| Persistent | 1000 | PersistentEntryPoint |
| OutGame | 0 | OutGameSceneInitializer / OutGameScenarioSceneInitializer |
| OutGame | 10 | OutGameScenarioInitializer / ScenarioCom |
| OutGame | 20 | TitleSceneInitializer / OutGameSortieInitializer |
| OutGame | 30 | OutGameBgmInitializer |
| OutGame | 100 | ScreenInitializer |
| OutGame | 110 | StageSelectInitializer / HomeCharacterPreviewInitializer |
| OutGame | 120 | SkillTreeInitializer |
| OutGame | 125 | OutGameUISoundEffectInitializer |
| OutGame | 130 | SkillBuildInitializer |
| OutGame | 140 | SettingComposition |
| OutGame | 150 | OutGameTutorialInitializer |
| InGame | 0 | SceneDependencyInitializationModule |
| InGame | 100 | TargetSystemInitializationModule |
| InGame | 200 | MusicSyncInitializer |
| InGame | 250 | EquipmentBgmInitializer |
| InGame | 400 | StageResultInitializationModule |
| InGame | 440 | SkillEffectInitializer / SkillCrosshairProgressUIInitializer |
| InGame | 441 | SkillListUIInitializer |
| InGame | 442 | SkillInputProgressUIInitializer |
| InGame | 450 | SkillInitializer |
| InGame | 490 | PlayerStatusBonusInitializer |
| InGame | 495 | InGameHudInitializer |
| InGame | 500 | PlayerInitializer |
| InGame | 600 | CameraSystemInitializer / InGameMissionInitializer |
| InGame | 650 | HUDEnemyHealthInitializer |
| InGame | 660 | ReticleHudInitializer / EnemyDirectionIndicatorInitializer |
| InGame | 700 | EnemyInitializer |
| InGame | 800 | StageEffectInitializer / ACLikeRhythmGuideInitializer |
| InGame | 1000 | SequenceInitializationModule |
| InGame | 1010 | PauseWindowInitializer |

### 🧩 Composition初期化情報
| 項目 | 内容 |
|---|---|
| **Initializerクラス** | `IngameComposition`（InGame）、`OutGameSceneInitializer`（OutGame）、`PersistentEntryPoint`（Persistent）と、各シーンのCoordinator |
| **公開する ModuleContainer / ServiceLocator登録型** | `SceneDependencyModuleContainer` |
---

## 🔗 モジュール結合
```Mermaid
graph TD
    %% 定義 (接続のないレイヤーは省略)
    subgraph BootstrapModule [Bootstrap モジュール]
        BS_Base["Composition<br>各InitializationModuleBase"]
        BS_Coord["Composition<br>各InitializationCoordinator"]
        BS_Entry["Composition<br>IngameComposition / OutGameSceneInitializer / PersistentEntryPoint"]
        BS_Entry --> BS_Coord
        BS_Coord --> BS_Base
    end

    subgraph AllModules [各機能モジュール]
        AM_Init["Composition<br>CameraSystemInitializer, EnemyInitializer ほか"]
    end

    subgraph SceneManagementModule [Persistent/SceneManagement モジュール]
        SM_App["Application<br>ISceneInitializationReadiness"]
    end

    %% 依存関係
    AM_Init -->|"基底を継承しOrderを宣言"| BS_Base
    BS_Coord -->|"フェーズ順に実行"| AM_Init
    BS_Entry -->|"初期化完了を通知"| SM_App
```

### 📥 依存しているもの
- `**Persistent/SceneManagement**`
  - *依存箇所*: `ISceneInitializationReadiness`
  - *詳細*: シーンの初期化が完了したことを通知する。通知が来るまで、遷移を要求した側のロード画面は表示され続ける

### 📤 依存されているもの
- **ほぼ全てのモジュール**
  - *参照箇所*: `InGameInitializationModuleBase`, `OutGameInitializationModuleBase`, `PersistentInitializationModuleBase`
  - *詳細*: 各モジュールの`~Initializer`がこれらを継承し、`ModuleName`と`Order`を宣言して各フェーズを実装する
---

# 詳細 {color="gray_bg"}

## 🧅レイヤー情報

### ① Domain
当モジュールでは使用していない。

### ② Application
当モジュールでは使用していない。

### ③ Adaptor
当モジュールでは使用していない。

### ④ View
当モジュールでは使用していない。

### ⑤ Infrastructure
当モジュールでは使用していない。

### ⑥ Composition
初期化の契約・基底クラス・Coordinatorをシーン種別ごとに持つ。Coordinatorは共通の`InitializationCoordinator<T>`を継承し、シーンごとの差分は型引数と名前だけである。

## 🔌 拡張ポイント
| 拡張したいこと | 実装する場所 | 追加登録の要否 |
|---|---|---|
| 新しいモジュールを初期化に載せたい | 対象シーンの`~InitializationModuleBase`を継承し、`ModuleName`と`Order`を宣言して必要なフェーズを`override`する | 不要（シーン上に配置されていればCoordinatorが拾う。ただし拾うのは有効な（`isActiveAndEnabled`）ものだけで、アウトゲームと常駐は同じシーンにあるものだけである）。`Order`の設定を誤ると、同じフェーズの中で取得したい相手がまだ登録されていない |
| 初期化フェーズを増やしたい | 契約と基底クラス、`InitializationCoordinator<T>`の実行順を変更する | 必要（全シーン分の基底へ影響する） |

## 🔄処理フロー
主要な処理フローは、それぞれ子ページに分けている。
- 📄 [[① シーン初期化のフェーズ実行フロー]] (3bf7c2c6-cc02-81a8-809e-cf1c618042fd)
- 📄 [[② Order によるモジュール間の依存解決]] (3bf7c2c6-cc02-81e1-836b-d675aecbfd17)

## ⚠️ 失敗時の挙動と進捗報告
- 各フェーズは成功・失敗を`bool`で返す。1つのモジュールでも失敗したら、その場で以降のモジュールと以降のフェーズを止め、`[Coordinator名] モジュール名 の フェーズ名 フェーズに失敗しました。`をエラーログに出す。
- フェーズ中の例外は、同じ内容をログに出してから再送出する。キャンセル（`OperationCanceledException`）はログを出さずにそのまま通す。
- 進捗は「モジュール数×4フェーズ」を全ステップ数として`IProgress<float>`へ報告し、ロード画面の進捗に使う。
- 結果は`ISceneInitializationReadiness.Complete(シーン名, 成否)`で通知する。
- シーンの破棄時は、`Order`と逆の順番で各モジュールの`Shutdown`を呼ぶ。アウトゲームは1つのモジュールの`Shutdown`で例外が出ても、残りの`Shutdown`を続ける。
- アウトゲームの初期化に失敗すると、シーン内のUIDocumentを無効にし、通常のUIとAddressablesに依存しない復帰画面（`OutGameInitializationFailureView`、560×180px、文字20pt）を出す。
  - 文言: 「画面の読み込みに失敗しました。」「タイトルへ戻る」、復帰に失敗したとき「画面を読み込めませんでした。もう一度お試しください。」、処理中「読み込み中…」。ロード済みの言語設定がEnglishなら英語の文言を出す（日英ともコードに直書きで、Localizationのテーブルは使わない。Localization自体の初期化に失敗しても表示できるようにするため）
  - マウスまたはEnterでタイトルへ戻る。処理中は操作を受け付けない。
  - タイトル自身の初期化に失敗したときは、タイトルを読み込み直す。
  - シナリオ戦闘への出撃中や、常駐の寿命トークンがキャンセル済みのときは、復帰を始めない。
