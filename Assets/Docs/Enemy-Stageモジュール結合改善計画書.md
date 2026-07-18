# Enemy-Stageモジュール結合改善計画書

- 作成日: 2026-07-16
- 対象: InGameの`Enemy`モジュールと`Stage`モジュール（実体はStageEffectシステム）の間の結合を整理する
- 前提: `Assets/Scripts/DesignPhilosophy.md`のレイヤー参照ルール（他モジュールへの依存はAdaptor層のみが持ち、Composition層が依存解決を行う）を判断基準とする

---

## 1. 目的

「EnemyとStageのモジュールが絡み合っている」という指摘を受け、実際のコードを調査した。結論として、**Domain層で双方向の型参照が発生している箇所が実在し、これがレイヤールール違反かつ「絡み合い」の正体**であることを確認した。あわせて、「ステージ」という言葉がコード上で複数の意味（StageEffect演出／StageSelectの出撃データ／物理的なレベル配置）に分裂して使われていることも、混乱の一因になっていると考えられる。

本計画書では、実際に検出した違反を解消する変更と、名称の整理を段階的に行う。

---

## 2. 現状整理（調査済み）

### 2.1 InGameの「Stage」モジュールの実体

プレイヤーが「ステージ」と聞いてイメージする物理的なレベル・アリーナは、実は`InGame/Stage`モジュールには存在しない。`InGame/Stage`モジュールの中身は**StageEffect（ウェーブ演出）システムのみ**（`IStageEffectDefinition`、`StageEffectAssetBase`、`StageEffectView`等、Domain〜Composition全層で約15ファイル）であり、Applicationレイヤーは存在しない。

一方、物理的なレベル配置に関する概念は次のように分裂している。

| 概念 | 実際の置き場所 |
| --- | --- |
| ステージシーンそのもの（`StageSceneObjects`/`IStageSceneInstance`） | `6.Composition/InGame/Player/`（**Playerモジュール**の中） |
| 敵の配置スポーン地点（`SpawnPositionPair`/`EnemySpawnPositionSearcher`） | `4.View/InGame/Enemy/`（**Enemyモジュール**が`FindObjectsByType`でシーンから直接収集） |
| どの敵Waveアセットを使うか（`EnemyWaveDefinitionAssetKey`） | `1.Domain/OutGame/StageSelect/StageDefinition.cs`（**StageSelectモジュール**、出撃前選択データ） |

「Stage」という名前を持つモジュールが実際には演出専用であり、本当の「ステージ」概念はPlayer/Enemy/StageSelectに分散している、という**命名と実態のズレ**が、そもそもの「絡み合っている」という印象の一因になっていると考えられる。

### 2.2 検出した実際の結合ポイント

| # | 場所 | レイヤー | 内容 | 深刻度 |
| --- | --- | --- | --- | --- |
| ① | `1.Domain/InGame/Enemy/EnemyWaveDefinition.cs`（`StageEffects: IReadOnlyList<IStageEffectDefinition>`）↔ `1.Domain/InGame/Stage/StageEffectDefinition.cs`・`IStageEffectDefinition.cs`（`MusicSpec: EnemyMusicSpec`） | **Domain ↔ Domain** | EnemyのDomainがStageのDomain型を持ち、StageのDomainがEnemyのDomain型を持つ、**循環したモジュール間依存** | 高（レイヤールール違反の中心） |
| ② | `5.InfraStructure/InGame/Enemy/EnemyWaveDefinitionAsset.cs`（`List<StageEffectAssetBase>`）↔ `5.InfraStructure/InGame/Stage/StageEffectAssetBase.cs`（`EnemyMusicSpec`生成） | **Infrastructure ↔ Infrastructure/Domain** | ①の波及。Infra層でも同じ相互参照が発生 | 中 |
| ③ | `1.Domain/OutGame/StageSelect/StageDefinition.cs`（`MissionDefinition`を直接保持） | **Domain**（StageSelect側） | Enemy/Stageとは別モジュールの組だが同種の違反。「Stage」という名前に引きずられて混同しやすい | 中（本計画では付随事項として記載） |
| ④ | `6.Composition/InGame/Enemy/AssignedEnemyManager.cs` | Composition | 「ステージに事前配置されている敵を管理する」コメント付きだが、**どこからも参照されていない未使用コード** | 低 |
| ⑤ | `EnemyInitializer`（Order 700）/`StageEffectInitializer`（Order 800） | Composition | `StageEffectInitializer.Ready()`が`EnemyModuleContainer`を読む依存が、Orderの数値と一部のコードコメント以外に明文化されていない | 低〜中 |
| ⑥ | `EnemyLifeCycle`/`BossLifeCycle` → `ServiceLocator.GetInstance<MissionEventController>()` | Composition | `MissionModuleContainer`を介さない直接取得（`Mission.md`にも既知課題として記載済み）。Stageとは無関係だが①と同根の"Containerを介さないモジュール間参照"パターン | 中（付随事項） |

②〜④は①の直接の帰結、または①と同じ「モジュール間参照はAdaptor層のみ」というルールの逸脱パターンなので、①を解消すれば②は自動的に解消され、④〜⑥は同じ考え方で個別に対応する。

Adaptor/View層では、`StageEffectView`/`StageEffectPresenter`が`int effectId`＋`enum StageEffectViewKind`という**IDベースの疎結合**で通信しており、Enemy側の型を一切参照していない。この部分は既に理想的な形になっている。

### 2.3 なぜこの形になったか（削除済み設計資料より）

過去に作成され削除済みの`ステージ実装レポート.md`（gitログから復元）には、次の設計方針が明記されていた。

> 敵生成、背景演出、障害物、音楽同期を一つのWaveControllerへ集約せず、フェーズIDを介して各モジュールが独立して反応する構造が望ましい。

つまり「IDを介して各モジュールが独立して反応する」という**意図自体は正しかった**が、実装時に`EnemyWaveDefinition`が`IStageEffectDefinition`を直接運ぶ形になってしまい、意図と実装がズレた状態になっている。3章の方針は、この元々の意図に実装を合わせ直すものである。

---

## 3. 設計方針

### 3.1 ①の解消: EnemyのWave定義からStageのDomain型を追い出す

`EnemyWaveDefinition.StageEffects`（`IReadOnlyList<IStageEffectDefinition>`）を、**Stage演出のIDリスト**に置き換える。

```csharp
// 変更前（1.Domain/InGame/Enemy/EnemyWaveDefinition.cs）
public IReadOnlyList<IStageEffectDefinition> StageEffects { get; }

// 変更後
public IReadOnlyList<int> StageEffectIds { get; }
```

- `EnemyWaveDefinitionAsset`（Infrastructure）は、現状通り`List<StageEffectAssetBase>`をInspector上で保持してよい（Infra層でのアセット参照はAuthoring上の利便性として許容する）。ただし`ToDefinition()`でDomainへ変換する際は、各`StageEffectAssetBase`の**ID（`_effectId`）だけを抽出**して`StageEffectIds`に詰める形にし、`IStageEffectDefinition`そのものは渡さない。
- `Assets/Docs/ID統一移行計画書.md`で計画済みの「Stage演出ID」の`DataID`化（カテゴリ="StageEffect"）と噛み合わせられるため、両計画を同時期に進める場合はEnemy側のIDも`DataID`型に揃えるとよい（本計画単独でも、暫定的に生の`int`のままで進めて問題ない）。
- `EnemyWaveSpawnerState`が発行する`OnWaveStarted`イベントのペイロードから`IStageEffectDefinition`が消え、代わりに`StageEffectIds`（またはWaveインデックスのみ）が乗る形になる。
- `StageEffectInitializer`（Stage側）は、渡されたIDを使って**自分自身が管理しているStageEffectのカタログ**（既存の`StageEffectAssetBase`群）から一致するものを検索して発火する。「Enemyから渡された演出定義をそのまま実行する」のではなく、「Enemyから通知されたIDを見て、Stageが自分の持ち物の中から該当する演出を選んで実行する」という、各モジュールが独立して反応する形に変える。これは2.3節で確認した本来の設計意図と一致する。

この変更により、**EnemyのDomain層からStageのDomain型への参照が完全に消える**。

### 3.2 ①の解消: StageのDomain型からEnemyのDomain型を追い出す

`IStageEffectDefinition`/`StageEffectDefinition`が持つ`MusicSpec: EnemyMusicSpec`が、Stage→Enemy方向の依存を作っている。`EnemyMusicSpec`は名前こそ"Enemy"だが、実態は**音楽同期用のタイミング仕様（BPM・拍オフセット等）** であり、Enemy固有のデータではない。すでに`Adaptor/InGame/Music`に音楽同期の窓口（`MusicSyncState`）が存在することからも、これは本来Musicモジュールが持つべき概念である。

- `EnemyMusicSpec`を`1.Domain/InGame/Enemy/`から**Musicモジュールの Domain層**（例: `1.Domain/InGame/Music/MusicSyncSpec.cs`）へ移設し、名称も実態に合わせて`MusicSyncSpec`のように変更する。
- Musicモジュールに現状Domain層が存在しない場合は、この1つの値オブジェクトのためだけに新設する（小さく低リスク）。
- Enemy・Stageの双方は、この移設後の型をMusicモジュールから参照する形にする。これにより「EnemyとStageが直接依存し合う」構造から、「EnemyとStageがそれぞれ独立してMusicへ依存する」という一方向の構造に変わり、循環が解消される。

3.1・3.2の両方を行うことで、①（および波及先の②）が解消される。

### 3.3 ④の解消: 未使用コードの削除

`AssignedEnemyManager.cs`はどこからも参照されていないことを確認済みなので、単純に削除する。

### 3.4 ⑤の解消: Composition依存関係の明文化

`EnemyInitializer`（Order 700）と`StageEffectInitializer`（Order 800）の依存関係は、`InitializationCoordinator`のフェーズバリア設計（全モジュールの`Build`完了後に全モジュールの`Ready`が始まる）のおかげで実害はないが、「なぜ800が700より後でなければならないか」がコード上に明示されていない。

- `StageEffectInitializer`のクラスXMLコメントに「`EnemyModuleContainer`（`EnemyInitializer`の`Build`フェーズで登録）に依存するため、Orderは`EnemyInitializer`より大きい値である必要がある」旨を明記する。
- `EnemyInitializer`側のコメントにも、`StageEffectInitializer`から参照されている旨を一言添えておく（相互参照であることが片方を見ただけで分かるように）。

コード構造自体の変更は不要で、コメント追記のみの軽微な対応とする。

### 3.5（付随事項）⑥の解消: Mission連携もContainer経由に揃える

`EnemyLifeCycle`/`BossLifeCycle`が`ServiceLocator.GetInstance<MissionEventController>()`を直接呼んでいる箇所は、Stageとは無関係だが①〜②と同じ「Containerを介さないモジュール間アクセス」というパターンであり、`Mission.md`にも既知課題として記載済みである。3.1〜3.4でEnemy↔Stage間の参照規律を揃えるのと合わせて、Enemy↔Mission間も`MissionModuleContainer`経由に変更しておくと、Enemy周りのモジュール間アクセス方法が全て統一される。

本計画のスコープの中心ではないため、任意対応（Phase 4）として扱う。

### 3.6 スコープ外とする事項

- **StageSelectの`StageDefinition`が`MissionDefinition`を直接持つ件（2.2節③）**: Enemy/Stageとは別のモジュールの組み合わせであり、対応する場合は別の計画書として切り出すのが適切。本計画では検出事実の記録のみに留める。
- **「本当のステージ（レベル/アリーナ）」概念の統合**（`StageSceneObjects`、`SpawnPositionPair`等をひとつのモジュールへ集約すること）: 2.1節で触れた分裂は事実だが、統合には新規モジュール設計を要する大きめの変更になる。本計画は「循環依存の解消」を主目的とするため、これは将来検討事項として書き残すのみとし、フェーズには含めない。
- **`BossInitializer`が`InitializationCoordinator`の管理外でテスト専用ドライバとして残っている件**: `Enemy.md`に既知の課題として記載済みだが、Stageとは無関係のため対象外。

---

## 4. フェーズ構成

### Phase 1: EnemyのWave定義からStageのDomain型を追い出す（3.1節）
1. `EnemyWaveDefinition.StageEffects`を`StageEffectIds`（`IReadOnlyList<int>`）に変更する。
2. `EnemyWaveDefinitionAsset.ToDefinition()`を、`StageEffectAssetBase`から`_effectId`のみを抽出する形に変更する。
3. `EnemyWaveSpawnerState`の`OnWaveStarted`イベントペイロードを、新しい`StageEffectIds`に対応させる。
4. `StageEffectInitializer`側を、受け取ったIDから自身のカタログを検索して発火する形に変更する。
5. 実機確認: Wave開始時に、これまでと同じタイミング・内容でStage演出が発火することを確認する（外部から見た挙動は変化しないことがゴール）。

### Phase 2: StageのDomain型からEnemyのDomain型を追い出す（3.2節）
1. `EnemyMusicSpec`をMusicモジュールのDomain層へ移設し、`MusicSyncSpec`等へリネームする（Musicモジュールに Domain層が無ければ新設）。
2. `IStageEffectDefinition`/`StageEffectDefinition`・Enemy側の参照元を、移設後の型へ差し替える。
3. ビルドが通ることを確認し、Phase 1と合わせてEnemy⇔StageのDomain層に相互参照が残っていないことをgrepで確認する。

### Phase 3: モジュール名を実態に合わせて整理する（任意、Phase 1/2完了後）
1. Phase 1/2でEnemyとStageEffectの間にDomain層の型参照が残っていないことを確認した上で、`1.Domain/InGame/Stage/`等のフォルダ・名前空間を`StageEffect`へリネームする（クラス名は既に`StageEffect*`のため、フォルダ・名前空間の変更のみで済む）。
2. 併せてNotionドキュメント側にも`Stage`（実体はStageEffect）というモジュールページを新設するか、既存のEnemy.mdの記述を見直す（現状`Stage.md`は存在しない）。

### Phase 4（任意）: 周辺クリーンアップ
1. `AssignedEnemyManager.cs`を削除する（3.3節）。
2. `EnemyInitializer`/`StageEffectInitializer`のクラスコメントに依存関係を明記する（3.4節）。
3. `EnemyLifeCycle`/`BossLifeCycle`の`ServiceLocator.GetInstance<MissionEventController>()`を`MissionModuleContainer`経由に変更する（3.5節）。

---

## 5. リスク・注意点

- **Phase 1の`StageEffectIds`移行は、Wave開始時の演出発火タイミングが変わらないことの確認が肝心**。Enemyが「定義を渡す」からStageが「IDを見て自分で選ぶ」に変わるため、IDの取り違えがあると演出が発火しない/別の演出が発火するといった不具合になりやすい。移行直後は実機で全Waveパターンを確認する。
- **Phase 2で`EnemyMusicSpec`を移設する際、既存のシリアライズ済みアセット（`EnemyWaveDefinitionAsset`等）への影響を確認する**。型の名前空間・アセンブリが変わると、Unityのシリアライズ参照（`m_Script`のGUID等）が壊れる可能性があるため、移設は「新しい型を作って参照を差し替える」のではなく、同一の型定義をファイル移動する形で慎重に行う（Unity C#スクリプトはクラス自体のGUIDが変わらない限り問題ないが、`[Serializable]`なプレーンC#クラス/構造体はアセンブリ内での型のフルネームが変わるとJSON/YAMLシリアライズの互換性に影響する場合があるため、移設前後で该当アセットの再シリアライズ確認を行う）。
- **Phase 3のリネームは、Phase 1/2の完了を前提とする**。循環参照が残ったままリネームすると、単に「EnemyとStageEffectが絡み合っている」という同じ問題が名前だけ変わって残ることになるため、順序を守る。
- **Phase 4の`MissionModuleContainer`経由化は、Missionモジュール側のContainer実装が現状どこまで揃っているかによって工数が変わる**可能性がある。着手前に`MissionModuleContainer`の現状を確認する。

---

## 6. 主な関連ファイル

### 変更対象（Phase 1）

- `Assets/Scripts/Runtime/1.Domain/InGame/Enemy/EnemyWaveDefinition.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Enemy/EnemyWaveDefinitionAsset.cs`
- Enemy側のWave開始イベント発行箇所（`EnemyWaveSpawnerState`関連）
- `Assets/Scripts/Runtime/6.Composition/InGame/Stage/StageEffectInitializer.cs`

### 変更対象（Phase 2）

- `Assets/Scripts/Runtime/1.Domain/InGame/Enemy/EnemyMusicSpec.cs`（移設・リネーム元）
- `Assets/Scripts/Runtime/1.Domain/InGame/Stage/IStageEffectDefinition.cs`
- `Assets/Scripts/Runtime/1.Domain/InGame/Stage/StageEffectDefinition.cs`
- `Assets/Scripts/Runtime/5.InfraStructure/InGame/Stage/StageEffectAssetBase.cs`
- Musicモジュールの新規/既存Domain層フォルダ（例: `1.Domain/InGame/Music/`）

### 変更対象（Phase 3、任意）

- `Assets/Scripts/Runtime/1.Domain/InGame/Stage/` 以下のフォルダ・名前空間全体
- `Assets/Scripts/Runtime/3.Adaptor/InGame/Stage/`、`4.View/InGame/Stage/`、`5.InfraStructure/InGame/Stage/`、`6.Composition/InGame/Stage/` 以下も同様

### 変更対象（Phase 4、任意）

- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/AssignedEnemyManager.cs`（削除）
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyInitializer.cs`（コメント追記）
- `Assets/Scripts/Runtime/6.Composition/InGame/Stage/StageEffectInitializer.cs`（コメント追記）
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/EnemyLifeCycle.cs`
- `Assets/Scripts/Runtime/6.Composition/InGame/Enemy/Boss/BossLifeCycle.cs`

### 変更不要

- `Assets/Scripts/Runtime/3.Adaptor/InGame/Stage/`、`4.View/InGame/Stage/`（`StageEffectView`/`StageEffectPresenter`のID＋enumベースの通信は既に理想形のため、Phase 3のリネーム以外は変更不要）
- Adaptor/View層のEnemy・Stage双方（Domain層の問題であり、Adaptor/View層には元々クロスモジュール参照が存在しない）
