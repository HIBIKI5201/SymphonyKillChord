# ID統一移行計画書

- 作成日: 2026-07-15（最終改訂: カテゴリを非ジェネリック化した確定版）
- 対象: リポジトリで集約され、IDで個別データを取得する仕組み全般
- 前提: `DataID`の解決にはSourceDataProviderのカテゴリ登録設定が必要なため、SourceDataProviderの最小構成（設定の永続化＋カテゴリ登録）をPhase 0に含める
- セーブデータ: **リセットを許容する**（未リリースのため互換性対応は行わない）

---

## 1. 目的

現在、個別データ（キャラクター・スキル・ステージ等）をリポジトリで一括管理し、IDで取得する仕組みが複数存在するが、ID型はバラバラである。

- 生の`int`をそのまま使う（`SkillTemplateAsset._id`、`SkillNodeData.NodeId`）
- `int`をラップした値オブジェクト（`StageId`）
- `string`をラップした値オブジェクト（`EnemyMissionKey`）
- 生の`string`をそのまま使う（Scenarioのアニメーション/背景/立ち絵カタログID、Stage演出ID）

これを次の2層構造に統一する。

| 層 | ID表現 | 役割 |
| --- | --- | --- |
| **Domain** | `int`をラップしたValueObject（`StageId`、`SkillId`等） | 実行時の比較・Dictionary検索。プリミティブで高速、Unityに非依存 |
| **Infrastructure / View** | `DataID`（非ジェネリック） | プランナーが文字列でIDとカテゴリを設定し、エディタ上でハッシュ（int）へ焼き込む。Domain VOの生成材料、またはView側での対応付けに使用 |

**Domainのデータはプリミティブなint表現のまま**とし、文字列→ハッシュの解決は`DataID`を持つ側（Infrastructure層の`*Asset`クラス、および一部View層）で完結させる。

あわせて、**現状ラップされていないID系にはDomain ValueObjectを新設し、ID系VOを拡充する**。

---

## 2. 設計概要

### 2.1 `DataID`（非ジェネリック・カテゴリ非保持）

当初、カテゴリをジェネリック型パラメータ（`DataID<TCategory>`）で表現する設計を検討したが、**View層はAdaptor層のみ、Infrastructure層はDomain/Application/View層のみを参照する**という制約により、両層から共通して参照できる「カテゴリ用マーカー型の置き場所」が存在しないことが判明した。

このため、**カテゴリをC#の型ではなく文字列にし、SourceDataProviderの設定に登録する**方式に変更した。さらに検討を進め、**その文字列を`DataID`のインスタンスごとにシリアライズして持たせる必要はない**という結論に至った。カテゴリはフィールドが「何のIDを表すか」という宣言時点で決まるものであり、フィールド側に付与する属性（後述の`[DataCategory]`）だけで表現すれば十分である。個別データのインスタンスごとにカテゴリ文字列を重複して持たせると、シリアライズ容量が無駄になるだけでなく、理論上は同じフィールドの別インスタンスに違うカテゴリを設定できてしまうという不整合の余地も生む。属性で固定すれば、この余地自体が無くなる。

```csharp
[Serializable]
public struct DataID
{
    /// <summary> 実行時に使用する数値ID。ビルドにはこの値のみが含まれる。 </summary>
    public int Id => _hashId;

#if UNITY_EDITOR
    /// <summary> プランナーが設定する、人間が読めるID文字列。エディタ限定フィールド。カテゴリは持たない（宣言側フィールドの[DataCategory]属性から得る）。 </summary>
    [SerializeField]
    private string _id;
#endif

    /// <summary> 焼き込み済みハッシュ値。PropertyDrawer側で_id変更時に再計算して書き込む。Viewのセレクターが使えない箇所向けの手動フォールバックとして、Inspector上は読み取り専用＋コピーボタンで表示する（2.1節末尾を参照）。 </summary>
    [SerializeField, ReadOnly]
    private int _hashId;
}
```

- **`_category`フィールドは廃止**。カテゴリはフィールド宣言側の`[DataCategory("Stage")]`属性（2.6節）で指定し、`DataID`構造体自体はカテゴリを一切保持しない。
- **ハッシュの焼き込みは`PropertyDrawer`側の責務に変更**: `_category`が構造体の外（属性）にある以上、`ISerializationCallbackReceiver.OnBeforeSerialize()`はカテゴリ文字列を参照できず自己完結できない。そこで`DataID`用の`CustomPropertyDrawer`が、描画対象フィールドの`[DataCategory]`属性から取得したカテゴリと`_id`の変更を検知し、`SerializedProperty`経由で`_hashId`を再計算・書き込みする。共通のハッシュ計算ロジックは`DataIDHasher.Compute(category, id)`という静的メソッドに切り出し、PropertyDrawerと後述の一括再焼き込みコマンドの双方から使う。
- **スクリプトによる一括編集時の注意**: PropertyDrawerのOnGUIを経由しない（Inspectorを開かずスクリプトで`_id`を書き換えるような）編集では自動焼き込みが走らない。このケースのために、`DataIDHasher.Compute`を使ってプロジェクト内の全`DataID`を再スキャンし`_hashId`を再計算する「Rebuild All DataID Hashes」エディタメニューコマンドを用意し、一括編集後に手動実行してもらう運用とする（5章のリスクにも記載）。
- **ハッシュ関数**: `Animator.StringToHash`。`string.GetHashCode()`はプロセス間・バージョン間の安定性が無いため使用しない。
- **`_hashId`はReadOnly＋コピーボタン**: 既存パッケージ（SymphonyFrameWork）に`ReadOnlyAttribute`/`ReadOnlyDrawer`が既に存在するため、これをそのまま付与し誤編集を防ぐ。加えて`DataID`用PropertyDrawerが`_hashId`の隣にコピー用ボタンを描画する。これは、何らかの理由で`[DataCategory]`のセレクターUIが機能しない箇所（例: 既存コードが素の`int`定数しか受け付けない一時的な呼び出し箇所など）向けの手動フォールバックであり、Inspectorに表示されたハッシュ値をコピーしてそのまま貼り付けられるようにするためのものである。**ただしこれは値のスナップショットに過ぎず、後から`_id`やカテゴリが変わってもコピー先には反映されない**（5章のリスクに記載）。恒常的な使用ではなく、あくまで例外的な代替手段として位置づける。
- **カテゴリ名はC#の型・ファイルに一切紐付かない**: 以前検討したMonoScript GUID方式（スクリプトファイルの`.meta`GUIDをソルトにする案）は不要になった。カテゴリ名は`[DataCategory]`属性の引数として書かれた文字列であり、SourceDataProviderの設定に登録された一覧に対してPropertyDrawerが描画時に検証する（2.6節）。

### 2.2 カテゴリ登録（SourceDataProvider側の責務）

カテゴリの一覧は、SourceDataProviderが`ProjectSettings`ディレクトリへ`ScriptableSingleton`として永続化する設定に登録する（Git共有前提）。この設定は、SourceDataProviderの要素①（リポジトリクラスの登録）と同じ設定画面の一部として扱う。

```csharp
#if UNITY_EDITOR
[FilePath("ProjectSettings/SourceDataProviderSettings.asset", FilePathAttribute.Location.ProjectFolder)]
internal sealed class SourceDataProviderSettings : ScriptableSingleton<SourceDataProviderSettings>
{
    [SerializeField] private List<RepositoryMapping> _repositoryMappings = new();

    [Serializable]
    private sealed class RepositoryMapping
    {
        public string Category;             // 例: "Stage"、"Skill"、"StageEffect"
        public string AddressableKey;        // リポジトリのAddressableキー
        public string ArrayPropertyPath;     // 個別データ配列のプロパティパス
    }
}
#endif
```

この設定の`Category`一覧は、次の2箇所から参照される。

1. **`[DataCategory]`属性のPropertyDrawer**: フィールドに書かれたカテゴリ文字列がこの一覧に存在するかを描画時に検証し、存在しなければInspector上で警告表示する（未登録カテゴリの typo を防ぐ唯一の砦になる。前節の通り`DataID`インスタンス側にはカテゴリを持たせないため、検証ポイントは「フィールド宣言＝属性の引数」の1箇所に集約される）。
2. **Viewなど参照側のセレクターUI**（2.6節）: 該当カテゴリのリポジトリ・配列プロパティをリフレクションで辿り、登録済みインスタンスの`_id`一覧をドロップダウンとして表示する。

この関係上、**`DataID`が正しく機能するには、SourceDataProviderの設定の一部（カテゴリ登録機能）が先に存在している必要がある**ため、4章のPhase 0にSourceDataProviderの最小構成を含める。

### 2.3 Domain層のID ValueObject（プリミティブint）

Domain VOは既存の`StageId`と同じ形（`int`ラップ、`IEquatable`、Unity非依存）に統一する。

```csharp
public readonly struct SkillId : IEquatable<SkillId>
{
    public SkillId(int value) { _value = value; }
    public int Value => _value;
    public bool Equals(SkillId other) => _value == other._value;
    public override bool Equals(object obj) => obj is SkillId other && Equals(other);
    public override int GetHashCode() => _value;
    private readonly int _value;
}
```

- 既存の`StageId`は**変更不要**（現状のままこの形）。
- `EnemyMissionKey`は`string`ラップから`int`ラップへ変更する。
- ラップの無いID（スキル、Scenarioカタログ3種）には新規VOを追加する。

### 2.4 InfrastructureからDomainへの解決フロー

```csharp
// Infrastructure層（StageNodeAssetの例）
[SerializeField, Tooltip("ステージを一意に識別するID。")]
[DataCategory("Stage")]
private DataID _stageId;

public StageNode Create()
{
    // DataIDのハッシュ値からDomain VOを生成する（ここがID解決の唯一の境界）
    var definition = new StageDefinition(
        new StageId(_stageId.Id),
        ...);
}
```

Domain層・Application層・Adaptor層は`DataID`の存在を一切知らない。

### 2.5 InfraStructureとViewが同じIDで対応付けるケース

`StageEffectAssetBase._effectId`（Infra）と`StageEffectView`内部の`EffectBinding._effectId`（View）のように、**同じIDをInfra側とView側の双方が独自に保持し、文字列一致で対応付けている箇所が既存コードに存在する**（`StageEffectView.cs`の`Matches`メソッド）。

`DataID`が非ジェネリック・カテゴリ非保持になったことで、この種のケースは単純にInfra側・View側の両方が同じ`DataID`型のフィールドを持ち、**両方のフィールドに同じ`[DataCategory("StageEffect")]`属性を付けるだけ**で対応付けが揃うようになった。View側が`typeof(StageEffectAssetBase)`のようなInfra層の具象型を参照する必要は一切ない。属性はフィールド宣言時にコードとして固定されるため、旧設計（インスタンスごとにInspectorのドロップダウンでカテゴリを選ぶ方式）で懸念していた「Infra側とView側で誤って別カテゴリを選んでしまう」という運用ミスの余地自体が構造的に無くなる。

### 2.6 `[DataCategory]`属性（SourceDataProvider要素④、生成側・参照側で共通）

`DataID`型のフィールドには必ず`[DataCategory("カテゴリ名")]`を付与する。この属性はInfrastructure側（IDを発行する側）・View側（IDを参照する側）のどちらでも同じ型・同じ書き方で使う（旧`[DataSelector(typeof(Data))]`案は、Infra層の具象型を`typeof`で直接参照してしまいView側のコードにInfra型への参照が発生するため不採用）。

```csharp
// View層の例。Infra層の型を一切参照しない
[SerializeField]
[DataCategory("StageEffect")]
private DataID _effectId;
```

`DataID`用の`CustomPropertyDrawer`（`Assets/Editor/`配下、レイヤー制約の対象外）は、フィールドの`[DataCategory]`属性からカテゴリ文字列を読み取り、宣言側の用途に応じて表示を出し分ける。

- **生成側（Infrastructure）のフィールド**: `_id`のテキスト入力＋読み取り専用ハッシュ表示＋コピーボタン（2.1節）。`SourceDataProviderSettings`から同カテゴリの既存`_id`一覧を取得し、重複していないかを警告表示する。
- **参照側（View等）のフィールド**: `SourceDataProviderSettings`から該当カテゴリのリポジトリ・配列プロパティをリフレクションで辿り、登録済みインスタンスの`_id`一覧をドロップダウンのセレクターとして表示する。選択すると同じハッシュ計算（`DataIDHasher.Compute`）で`_hashId`が書き込まれる。

いずれの場合もView層のC#コード自体はInfra層の型を一切知らずに済む。

### 2.7 ハッシュ衝突対策

`Animator.StringToHash`は32bitのため衝突しうる。各リポジトリのビルド時（既存の`SkillRepository`重複ID警告と同じ位置）に、**同一カテゴリ内で異なる`_id`文字列が同じハッシュになっていないか**を検証する。あわせて、エディタ専用の「Rebuild All DataID Hashes」コマンド（2.1節）実行時に「保存済み`_hashId`と、宣言側`[DataCategory]`＋`_id`から`DataIDHasher.Compute`で再計算した値との不一致」を検出し、焼き込み漏れ（PropertyDrawerを経由しないスクリプト編集等）を洗い出せるようにする。

### 2.8 配置場所

- `DataID`・`DataIDHasher`: `Assets/Scripts/Runtime/0.Utility/Identity/`（新規フォルダ）。非ジェネリックの単純な構造体・静的クラスになったため、View・Infrastructure双方から問題なく参照できる（`KillChord.View.asmdef`/`KillChord.InfraStructure.asmdef`はいずれも既に`KillChord.Utility`を参照済み）。
- `SourceDataProviderSettings`・カテゴリ登録UI・`[DataCategory]`属性とその`PropertyDrawer`・Rebuildコマンド: `Assets/Editor/Scripts/SourceDataProvider/`（エディタ拡張、レイヤー制約の対象外）。
- 新設するDomain VO（`SkillId`等）: 各モジュールのDomain層（既存の`StageId`等と同じ場所）。

---

## 3. 移行対象一覧（実装調査済み）

| 対象 | 現状 | 変更後（Domain） | 変更後（Infrastructure/View） |
| --- | --- | --- | --- |
| ステージ | `StageId`（intラップ） | **変更なし** | `StageNodeAsset._stageId`等を`int`→`DataID`（カテゴリ="Stage"）へ |
| 敵ミッションキー | `EnemyMissionKey`（stringラップ） | `int`ラップへ変更 | `EnemyMissionKeyAsset._id`を`string`→`DataID`（カテゴリ="EnemyMissionKey"）へ |
| スキル | 生の`int`（`SkillTemplateAsset._id`） | `SkillId`（intラップ）を**新設** | `SkillTemplateAsset._id`を`DataID`（カテゴリ="Skill"）へ |
| スキルツリーノード | 生の`int`（`SkillNodeData.NodeId`）。Domain層に未使用の`SkillNodeId`あり | 既存`SkillNodeId`を正式採用（intラップのまま） | `SkillNodeData.NodeId`を`DataID`（カテゴリ="SkillNode"）へ |
| Scenarioアニメーション | 生の`string` | `AnimationId`（intラップ）を**新設** | `AnimationCatalogEntry.Id`を`DataID`（カテゴリ="ScenarioAnimation"）へ |
| Scenario立ち絵 | 生の`string` | `PortraitId`（intラップ）を**新設** | `PortraitCatalogEntry.Id`を`DataID`（カテゴリ="ScenarioPortrait"）へ |
| Scenario背景 | 生の`string` | `BackgroundId`（intラップ）を**新設** | `BackgroundCatalogEntry.Id`を`DataID`（カテゴリ="ScenarioBackground"）へ |
| Stage演出ID | 生の`string`×2箇所（Infra: `StageEffectAssetBase._effectId` / View: `StageEffectView.EffectBinding._effectId`）。文字列一致で対応付け | 該当なし（Domain層のデータではなく、Infra⇔View間の対応付け専用ID） | 両方とも`DataID`（カテゴリ="StageEffect"）へ変更。同じカテゴリ文字列を選ぶことで両側のハッシュが一致する（2.5節） |

### セーブデータへの影響（リセット前提で単純化）

- `StageClearData._stageId`（int）、`SkillUnlockData._unlockedSkillIds`/`_unlockedSkillNodeIds`（int[]）、`SkillBuildData._equipmentSkillIDs`（List<int>）は、**int型のまま変更不要**。保存される値の意味が「手書きの連番」から「ハッシュ値」に変わるだけで、スキーマは同じ。
- 旧セーブデータとの互換は取らない（リセット許容）。`SaveData.INITIAL_UNLOCKED_SKILL_IDS`/`INITIAL_EQUIPPED_SKILL_IDS`（`{ 0, 13 }`）は、新IDのハッシュ値へ更新が必要（5章の注意点参照）。
- Scenarioの`TextTimingTrigger`等がCSV内で参照するID文字列は、CSV→`ScenarioDefinition`のパース時にハッシュへ変換する形にする（Scenarioカタログの解決タイミングと同じ）。

### 移行対象外（調査済み・意図的に除外）

> **2026-07-28追記**: `MissionId`は下記の除外理由が解消されたため、対象外リストから除外し実際に移行済み。`MissionId`をintラップへ変更し、`MissionDefinitionAsset._missionId`を`DataID`化（カテゴリ="Mission"）、`MissionDefinitionRepository`をID検索可能なリポジトリ化した。あわせて`BattleStageAsset._missionDefinitionAsset`（直接参照）も`DataID`（カテゴリ="Mission"）へ変更し、`MissionDefinition`の生成をOutGame選択時からInGame側（`InGameMissionInitializer`）へ遅延させた（`EnemyWaveDefinitionId`と同じ「OutGame→InGameをIDで受け渡し、InGame側で解決」方式）。

| 対象 | 除外理由 |
| --- | --- |
| `MissionEvaluationId` | 重複チェックにのみ使用され、IDからインスタンスを取得する用途がない |
| `SkillNodeBindData.NodeName` | UI Toolkit要素名としてのキーであり、ゲームデータのIDではない |
| `BossAttackEntryAsset.AttackIndex` | 配列の位置参照であり、authoring IDではない |
| `CharacterAnimationPlaybackMap._oneShotIndices` | View層の再生インデックス管理 |
| `ScreenId` | enumのため対象外 |

---

## 4. フェーズ構成

### Phase 0: 共通基盤の実装（SourceDataProviderの最小構成を含む）
1. `SourceDataProviderSettings`（`ScriptableSingleton`、カテゴリ登録機能）を実装する。この時点ではリポジトリの自動登録UI等は不要で、カテゴリ名の登録・一覧取得ができれば十分。
2. `DataID`を`0.Utility/Identity/`に実装する（`_id`のエディタ限定化、`_hashId`への`ReadOnly`付与。カテゴリは保持しない）。
3. `DataIDHasher`（`Compute(category, id)`の静的ハッシュ計算）を`0.Utility/Identity/`に実装する。
4. `[DataCategory]`属性と`DataID`の`PropertyDrawer`を実装する（`SourceDataProviderSettings`のカテゴリ一覧に対する検証、`_id`変更時の`_hashId`焼き込み、生成側のテキスト入力＋コピーボタン、参照側の登録済みインスタンスセレクター）。
5. ハッシュ衝突検出ヘルパーと、「Rebuild All DataID Hashes」エディタメニューコマンド（スクリプト一括編集で焼き込みが漏れた`_hashId`を再計算する救済策）を実装する。

### Phase 1: セーブデータに関与しない対象（低リスク・設計検証を兼ねる）
1. `EnemyMissionKey`を`int`ラップへ変更し、`EnemyMissionKeyAsset`を`DataID`化する（カテゴリ="EnemyMissionKey"）。
2. Scenarioカタログ3種にDomain VO（`AnimationId`/`PortraitId`/`BackgroundId`）を新設し、`*CatalogEntry`と`CatalogRepositoryBase`のキーを`string`→`int`へ変更する。CSVパース時のID解決もあわせて対応する。
3. Stage演出ID（`StageEffectAssetBase._effectId`/`StageEffectView.EffectBinding._effectId`）を、Infra・View双方とも同じ`DataID`（カテゴリ="StageEffect"）へ変更する。**InfraとViewの両方から`DataID`が使えることを確認する、最初の実地検証として適したケース。**
4. 各リポジトリのビルド処理にハッシュ衝突検出を組み込む。

### Phase 2: セーブデータに関与する対象
1. `StageNodeAsset`/`StageTreeAsset`の`_stageId`・接続データを`DataID`（カテゴリ="Stage"）化する（Domain `StageId`は無変更）。
2. `SkillId`を新設し、`SkillTemplateAsset`・`SkillRepository`・`OwnedSkillRepository`・`SkillBuildRepository`・セーブデータ変換箇所を`SkillId`経由に統一する。
3. `SkillNodeId`（既存Domain VO）を正式採用し、`SkillNodeData`/`SkillNodeDataRepo`/`SkillNodeBindRepo`/`SkillNodePhaseBindDataRepo`/`SkillTreeService`の生`int`を置き換える。
4. `SaveData.INITIAL_UNLOCKED_SKILL_IDS`等の初期値を新ハッシュIDへ更新する。
5. 既存アセットの`_id`文字列を設定し直す（このタイミングで`"stage_forest_01"`のような人間可読な命名を最初から採用してよい。セーブリセット前提のため旧int値に縛られる必要がない）。

### Phase 3: SourceDataProviderツール本体の実装
全対象が`DataID`でIDを入力する状態になったら、要素①（リポジトリ自動登録UI）・要素②（個別データInspectorへの登録状態表示）・要素④の`[DataCategory]`本実装に着手する。型ごとのID取得ロジックが不要なため、以前検討した`IIdExtractor`のような拡張ポイントは不要。

---

## 5. リスク・注意点

- **ハッシュ衝突は理論上ゼロにできない**: 2.7節の衝突検出をリポジトリビルド時に必ず組み込む。
- **`_hashId`の焼き込みは`PropertyDrawer`のOnGUI経由に一本化**: `DataID`はカテゴリを保持しないため、自己完結する`OnBeforeSerialize`では焼き込めない。Inspectorを介さずスクリプトで`_id`を一括編集した場合は自動焼き込みが走らないため、必ず「Rebuild All DataID Hashes」コマンド（4章Phase 0）を実行してもらう運用を徹底する。
- **`[DataCategory]`の文字列引数自体のtypoは属性側で防ぎきれない**: カテゴリを`DataID`インスタンスごとに持たせないことで「同じフィールドの別インスタンスで違うカテゴリを選んでしまう」リスクは構造的に無くなったが、`[DataCategory("Stage")]`と`[DataCategory("stage")]`のような属性間の表記ゆれは`PropertyDrawer`の描画時検証（`SourceDataProviderSettings`のカテゴリ一覧との照合）で警告する以外に防ぐ手段がない。将来的にカテゴリ名を定数化するアナライザ等の追加は本計画のスコープ外とする。
- **`DataID`はSourceDataProviderのカテゴリ登録設定に依存する**: カテゴリが1件も登録されていない状態では`DataID`が正しく機能しない。Phase 0でカテゴリ登録機能を先に用意する必要がある（4章参照）。
- **`_hashId`のコピーボタンはスナップショットに過ぎない**: セレクターが使えない箇所向けの手動フォールバックとしてコピーした数値は、コピー後に`_id`やカテゴリが変わっても追従しない。恒常的な使用ではなく例外的な代替手段として扱い、使用箇所はコードコメント等で分かるようにしておくことが望ましい。
- **`INITIAL_UNLOCKED_SKILL_IDS`の更新漏れに注意**: コード内に直書きされたint IDは、grep等で洗い出して新ハッシュへ更新する必要がある（`SaveData.cs`の他、テストコード・デバッグコードも対象）。
- **デバッグ時のID可読性**: ビルド後はハッシュ値しか存在しないため、ログにIDを出す場合は数値になる。エディタ実行時はSourceDataProvider側でハッシュ→文字列の逆引き表示を提供する想定。

---

## 6. SourceDataProviderツールとの関係

- 個別データ型が持つ「`DataID`フィールド」をリフレクションで探すだけで、型ごとの分岐なしにIDを取得できる。
- 参照側は`[DataCategory("カテゴリ名")]`を付けた`DataID`フィールドを持てば、登録済みインスタンスに絞ったセレクターUIから選択でき、実体はハッシュ（int）として保存される。View層のコードにInfra層の型参照は一切発生しない（2.6節）。
- `[DataCategory]`は生成側（Infrastructure）・参照側（View）のどちらでも同じ属性・同じ書き方であるため、SourceDataProviderのPropertyDrawer実装も1種類で両方の用途を賄える（表示内容は宣言側の用途に応じて出し分ける）。
- カテゴリが文字列ベースになったことで、以前検討していた`IIdExtractor`のような型ごとの拡張ポイントは不要になった。
- **SourceDataProviderのカテゴリ登録機能自体が`DataID`の前提条件になったため、両者はもはや「後で作る別ツール」ではなく、最小構成をPhase 0から一緒に育てていく関係になる。**

---

## 7. 主な関連ファイル

### 変更対象

- `Assets/Scripts/Runtime/1.Domain/InGame/Mission/EnemyMissionKey.cs`（string→intラップへ）
- `Assets/Scripts/Runtime/1.Domain/InGame/Mission/EnemyKillRecord.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Mission/EnemyMissionKeyAsset.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/StageSelect/StageNodeAsset.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/StageSelect/StageTreeAsset.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/Player/SkillTemplateAsset.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/Player/SkillRepository.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/SkillBuild/OwnedSkillRepository.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/SkillBuild/SkillBuildRepository.cs`
- `Assets/Scripts/Runtime/1.Domain/OutGame/SkillTree/ValueObject/SkillNodeId.cs`（正式採用）
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/SkillTree/SkillNodeData.cs` ほかSkillTree系リポジトリ
- `Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/CatalogRepositoryBase.cs` ほかScenarioカタログ系
- `Assets/Scripts/Runtime/1.Domain/Persistent/Savedata/SaveData.cs`（初期スキルIDの更新）
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Stage/StageEffectAssetBase.cs`（Infra側`_effectId`）
- `Assets/Scripts/Runtime/4.View/InGame/Stage/StageEffectView.cs`（View側`EffectBinding._effectId`）

### 新規作成

- `Assets/Scripts/Runtime/0.Utility/Identity/DataID.cs`
- `Assets/Scripts/Runtime/0.Utility/Identity/DataIDHasher.cs`（`Compute(category, id)`の静的ハッシュ計算。PropertyDrawerとRebuildコマンドが共用）
- `Assets/Editor/Scripts/SourceDataProvider/SourceDataProviderSettings.cs`（`ScriptableSingleton`、カテゴリ登録）
- `Assets/Editor/Scripts/SourceDataProvider/DataIDPropertyDrawer.cs`（`_hashId`のReadOnly表示＋コピーボタン、生成側/参照側の描画出し分け）
- `Assets/Editor/Scripts/SourceDataProvider/DataCategoryAttribute.cs`（旧`DataSelectorAttribute.cs`。文字列カテゴリを持つ属性本体）
- `Assets/Editor/Scripts/SourceDataProvider/DataIDRebuildMenu.cs`（「Rebuild All DataID Hashes」コマンド）
- 新設Domain VO: `SkillId.cs`、`AnimationId.cs`、`PortraitId.cs`、`BackgroundId.cs`

### 変更不要（今回の設計により影響が消えた箇所）

- `Assets/Scripts/Runtime/1.Domain/OutGame/StageSelect/StageId.cs`（現状のまま）
- `Assets/Scripts/Runtime/1.Domain/OutGame/StageSelect/StageTree.cs`（Dictionaryキーは`StageId`のまま）
- セーブデータのスキーマ（`StageClearData`/`SkillUnlockData`/`SkillBuildData`のint型フィールド）
- カテゴリ用マーカー型・`CategorySaltCache<TCategory>`（前設計で予定していたが、非ジェネリック化・属性ベースのカテゴリ指定により不要になった）
