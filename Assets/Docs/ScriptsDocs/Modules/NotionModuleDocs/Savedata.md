# 概要
> 💡 **モジュール概要**
> プレイヤーのセーブデータ（スキル解放・装備構成・ステージ進行・チュートリアル完了状態）の読み込み・保存・キャッシュを司る常駐モジュールです。JSONファイルへの永続化と、型ごとのキャッシュ・排他制御を担います。

| 項目 | 内容 |
| --- | --- |
| **モジュール名** | Savedata |
| **カテゴリ** | Persistent |
| **ステータス** | 実装済み |
| **最終更新日** | 2026-08-17 |

---

## 🏗️ クラス

| クラス名 | レイヤー | 役割・機能 |
| --- | --- | --- |
| **`SaveData`** | Domain | セーブデータのルート集約。`SkillUnlock`/`SkillBuild`/`StageProgress`/`Tutorial`の4つのデータを束ねる |
| **`SkillUnlockData`** | Domain | 研究ポイント、解放済みスキルツリーノードID、解放済みスキルIDを保持 |
| **`SkillBuildData`** | Domain | 装備中スキルIDリスト、スキルレベルアップポイントを保持 |
| **`StageProgressData`** | Domain | 全ステージのクリア記録（`StageClearData`のリスト）を保持。`RecordClear`/`IsStageCleared` |
| **`StageClearData`** | Domain | 1ステージ分のクリア記録（StageId＋達成済み評価条件IDリスト、複数プレイ分を和集合でマージ） |
| **`TutorialData`** | Domain | チュートリアル完了フラグ（`IsTutorialCompleted`）。`Complete()`で確定 |
| **`SaveStore`** | SymphonyFrameWork | 型ごとのロード・保存・削除・キャッシュを行うフレームワーク側のAPI。旧`SaveBase`/`SavedataSystem`はこれへ統合され、当リポジトリからは削除済み |
| **`PersistentFileSaveDataLoaderStrategy`** | Infrastructure | セーブデータを永続化領域のJSONファイルへ読み書きするローダー |
| **`StageProgressSaveDataService`** | Application | ステージクリア時の評価結果を`StageProgressData`へ記録し保存する窓口。チュートリアル完了もあわせて記録 |
| **`InitialSkillLoadoutService`** | Application | セーブデータへ初期解放・初期装備スキルを補完する。起動時とセーブデータリセット後の双方で使う |
| **`SavedataSystemInitializer`** | Composition | セーブ機構の初期化とServiceLocatorへの登録（Order 10） |
| **`InitialSkillLoadoutInitializer`** | Composition | `InitialSkillLoadoutService`の構築と初期スキルの補完（Order 20） |
| **`LegacyDataIdMigration`** | Composition | ID統一前の連番IDを現在のハッシュIDへ移行する内部クラス |

### 🧩 Composition初期化情報

| 項目 | 内容 |
| --- | --- |
| **Initializerクラス** | `SavedataSystemInitializer`（保存機構）／`InitialSkillLoadoutInitializer`（初期スキル補完） |
| **Order** | 10（保存機構。Persistentシーン内でほぼ最初期）／20（初期スキル補完） |
| **公開する ModuleContainer / ServiceLocator登録型** | 専用の`ModuleContainer`は無し。`InitialSkillLoadoutService`をServiceLocatorへ登録し、保存の入口はSymphonyFrameWorkの`SaveStore`（静的API）を直接使う |

---

## 🔗 モジュール結合

```mermaid
graph TD
    %% 定義 (接続のないレイヤーは省略)
    subgraph SavedataModule [Savedata モジュール]
        SD_Domain["Domain<br>SaveData, StageProgressData, TutorialData"]
        SD_Store["SymphonyFrameWork<br>SaveStore"]
        SD_App["Application<br>StageProgressSaveDataService"]
        SD_Composition["Composition<br>SavedataSystemInitializer, InitialSkillLoadoutInitializer"]
        SD_Domain --> SD_Store
        SD_App --> SD_Store
        SD_Composition --> SD_Store
    end

    subgraph SequenceModule [Sequence モジュール]
        SQ_Composition["Composition<br>SequenceInitializationModule"]
    end

    subgraph StageSelectModule [StageSelect モジュール]
        SS_Infra["Infrastructure<br>SaveDataClearStageRepository"]
    end

    subgraph TitleModule [Title モジュール]
        T_Composition["Composition<br>TitleSceneInitializer"]
    end

    subgraph SkillModule [Skill モジュール]
        SK_Composition["Composition<br>SkillBuildInitializer, SkillTreeInitializer"]
    end

    %% 依存関係
    SQ_Composition -->|"クリア時の評価結果を保存"| SD_App
    SS_Infra -->|"クリア済みステージID一覧の取得"| SD_Domain
    T_Composition -->|"チュートリアル完了判定・データリセット・初期スキル再適用"| SD_Composition
    SK_Composition -->|"SaveStore経由でセーブデータを読み書き"| SD_Store
```

### 📥 依存しているもの

* なし
  * *詳細*: 本モジュールは他モジュールのDomain/Application型に一切依存しない、独立した基盤モジュールです。

### 📤 依存されているもの

* **`Sequence`**
  * *参照箇所*: `StageProgressSaveDataService.SaveClearAsync`
  * *詳細*: ミッションクリア確定時に評価結果・チュートリアル完了状態を保存します。
* **`StageSelect`**
  * *参照箇所*: `SaveDataClearStageRepository`（`IStageClearRepository`実装）
  * *詳細*: クリア済みステージ一覧をステージマップの解放判定に使用します。
* **`Title`**
  * *参照箇所*: `SaveStore.LoadAsync<SaveData>()` / `SaveStore.DeleteAsync<SaveData>()`, `InitialSkillLoadoutService`
  * *詳細*: 初回起動判定（`TutorialData.IsTutorialCompleted`）とセーブデータリセットで使用します。リセット後は初期スキルの再補完も呼びます。
* **`Skill`**
  * *参照箇所*: `SaveStore`
  * *詳細*: `SkillBuildInitializer`/`SkillTreeInitializer`がスキル解放・装備状態の読み書きに使用します。

---

# 詳細

## 🧅レイヤー情報

### ① Domain
`SaveData`を頂点に、`SkillUnlockData`/`SkillBuildData`/`StageProgressData`/`TutorialData`という4つのサブデータを保持します。いずれも「公開プロパティは読み取り専用、変更は意図が伝わるメソッド経由」という設計規約（`Architecture.txt`参照）に従っています。
### ② Application
`StageProgressSaveDataService`が、ステージクリア時の評価結果を`StageProgressData.RecordClear`へ変換して保存する橋渡しを行います。`InitialSkillLoadoutService`が初期解放・初期装備スキルの補完を担当します。
### ③ Adaptor
当モジュールでは使用していません。
### ④ View
当モジュールでは使用していません。
### ⑤ Infrastructure
`PersistentFileSaveDataLoaderStrategy`が、永続化領域のJSONファイルへの読み書きを担当します。クリア済みステージ情報を提供する`SaveDataClearStageRepository`はStageSelectモジュール側にあります。
### ⑥ Composition
`SavedataSystemInitializer`（Order 10）が保存機構を初期化し、`InitialSkillLoadoutInitializer`（Order 20）が初期解放・初期装備スキルを補完します。`LegacyDataIdMigration`がID統一前の連番IDをハッシュIDへ移行します。

## 🔌 拡張ポイント

> 現状、ポリモーフィックな拡張ポイント（`SubclassSelector`等）はありません。新しい種類のセーブデータを追加する場合は、`SaveBase`を継承した新しいクラスを作成し、`SaveData`のフィールドとして追加するか、独立した型として`SavedataSystem.LoadAsync<T>()`/`SaveAsync<T>()`で扱う形になります（`T : SaveBase, new()`という型制約を満たせば、`SavedataSystem`は特別な登録なしに新しい型を扱えます）。

## 🔄処理フロー

主要な処理フローは、それぞれ子ページに分けています。

### ① セーブデータ読み込みフロー（初回アクセス時）

```mermaid
sequenceDiagram
    autonumber
    participant Caller as 呼び出し元（Title等）
    participant System as SavedataSystem
    participant Data as SaveData (SaveBase)
    participant File as JSONファイル

    Caller ->> System: LoadAsync<SaveData>()
    alt キャッシュ済み
        System -->> Caller: キャッシュされたSaveDataを返却
    else 未読み込み
        System ->> Data: new SaveData() → ReadAsync()
        Data ->> File: {persistentDataPath}/SaveData.json を読み込み
        alt ファイルが存在する
            File -->> Data: JSON文字列
            Data ->> Data: JsonUtility.FromJsonOverwrite
        else ファイルが存在しない
            Note over Data: 既定値のまま（新規プレイヤー扱い）
        end
        System ->> System: キャッシュに格納
        System -->> Caller: SaveDataを返却
    end
```

### ② クリア時の保存フロー

```mermaid
sequenceDiagram
    autonumber
    participant Seq as SequenceInitializationModule (Sequenceモジュール)
    participant Service as StageProgressSaveDataService
    participant Data as StageProgressData / TutorialData
    participant System as SavedataSystem

    Seq ->> Service: SaveClearAsync(stageId, evaluationResult, isTutorial)
    Service ->> Data: StageProgressData.RecordClear(stageId, achievedEvaluationIds)
    alt isTutorial == true
        Service ->> Data: TutorialData.Complete()
    end
    Service ->> System: SaveAsync(saveData)
    System ->> System: 書き込みロック取得 → WriteAsync（一時ファイル経由でアトミックに置換）→ キャッシュ更新
```

