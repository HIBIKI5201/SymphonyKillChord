# クラス設計

[設計思想](DesignPhilosophy.md) のクラス責務に関する正本。対象レイヤーの節を読む。


## Domain層

### Entity

Domain層にある値が可変な参照型オブジェクト。

型は`class` 。

[実装例](DesignExamples.md#entity)は必要なときだけ参照する。


公開プロパティは読み取り専用にし、変更は`ChangeValue`のように意図が伝わるメソッド経由で行う。オートプロパティの`public set`は使用しない。

### ValueObject（VO）

Domain層にある値が不変な値型オブジェクト。

型は`readonly struct` 。

[実装例](DesignExamples.md#valueobject)は必要なときだけ参照する。


## Application層

### Factory

Application層にあるApplicationとDomainのインスタンスを生成するクラス。
Factoryパターンを使用する。

### IRepository

Repository の抽象クラス。

## Adaptor層

### Presenter

Adaptor層にあるViewとDomainを仲介するクラス。

Query CommandのQueryを担当する。

### Controller

Adaptor層にあるDomainやApplicationの処理を実行するクラス。

Query CommandのCommandを担当する。

### State

Adaptor層にあるデータ状態の永続性を持つクラス。

### DataTransferObject（DTO）

Adaptor層にあるViewModelへ更新データを送る値型オブジェクト。

型は`readonly ref struct` 。

### Registory

AdaptorそいうにあるDomainとViewで対になるデータのペアを保存するクラス。

### IViewModel

ViewModel（VM） の抽象クラス。

### ISignal

Signal の抽象クラス。

## View層

### ViewModel（VM）

View層にあるUIに表示するデータを保持するクラス。
ReactivePropaty指向で実装する。

DTOを受け取るメソッドを用意する。
DTOは`in` を使用する。

### Signal

View層にあるイベントバス。

DTOを受け取るメソッドを用意する。
DTOは`in` を使用する。

### Spawner

View層にあるViewのオブジェクトを生成するクラス。
Factoryパターンを使用する。

### Config

View層にある、Viewの設定データを格納するクラス。
ScriptableObjectで実装する。

Viewのみで完結する設定データがある場合に使用する。
ドメインロジックに関連する場合はこれではなく **Asset** を使用する。

## InfraStructure層

### Asset

InfraStructure層にある、データを入力するScriptableObjectクラス。

Domain層などに対となるクラスが存在し、それのパラメータを導入できるようにする。

種類が増えていくデータ（判定条件、演出など）は、抽象基底クラス（`XxxAssetBase`）を用意し、`[SerializeReference, SubclassSelector]` を付けたフィールドでインスペクタから多態的に選択できるようにする。基底クラスは`abstract Create()`を持ち、対応するDomain層の型へ変換する。

### Repository

InfraStructure層に実装があり、Application層に抽象がある。

DBの取得処理やScriptableObjectなど。

## **Composition層**

### Initializer

Composition層にある初期化やDIを実行するクラス。

`InGameInitializationModuleBase` / `OutGameInitializationModuleBase` / `PersistentInitializationModuleBase` のいずれかを継承し、`Init()` → `ResourceLoadAsync(CancellationToken)` → `Build()` → `Ready()` の順で呼ばれるライフサイクルを実装する。`ModuleName` と `Order`（実行順）を持ち、`InitializationCoordinator<TModule>` が `Order` の昇順で全モジュールをフェーズごとに実行する。いずれかのフェーズが失敗した場合、以降のフェーズは実行されない。シーン終了時は`Shutdown()`を登録順の逆順で呼び出す。

### Container

Composition層にあるモジュールのサービスを包括して保持し、他サービスに伝達するクラス。

`ServiceLocator.RegisterInstance`で登録し、他モジュールは`ServiceLocator.TryGetInstance<T>()`で取得する。モジュール間連携はコンストラクタDIではなく、この Container を介した ServiceLocator 経由の公開が標準になっている。

### Debugger

Composition層にあるエディタ向け機能のクラス。

### 非同期処理の型

Composition層のモジュールライフサイクル（`ResourceLoadAsync`等）はUnityネイティブの`Awaitable` / `Awaitable<T>`を返す。Application層・Adaptor層のユースケースやシーン遷移などのAPIは`Task<T>` / `ValueTask<T>`を返す。両者の橋渡しが必要な箇所に限り、Application層でも`Awaitable`を使用してよい。
