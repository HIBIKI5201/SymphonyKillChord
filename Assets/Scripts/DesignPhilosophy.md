# 開発環境

## Runtime

マスタービルド向けのソースコード。
エディタやテスト機能が含まれてはならない。

## Develop

開発ビルド向けのソースコード。
Runtimeモジュールにテスト機能を与える。

## Editor

Unityエディタ用ソースコード。
エディタ拡張やツールを与える。

## DevelopProducts

技術研究向けのソースコード。
先行研究用のデモコードを実装する。

# **基底概念**

- 大規模に拡張性のあるシステムであること
- 仕様の変更に柔軟であること
- 大人数での開発に対応できること

## **指向**

- クリーンアーキテクチャ
- ドメイン駆動設計
- SOLID原則
- GRASP原則
- KISS法則
- クエリ・コマンド原則
- MVVM

# **アーキテクチャ**

基礎概念はクリーンアーキテクチャ。 副次概念はドメイン駆動設計。

## 依存性

ただしUnityフレームワークへの依存は許容する。 
ただしUnityライフサイクルへの依存は抑える。

つまりピュア層で

- using UnityEngine

などは許容されるが

- MonoBehaviour継承

などは許容されない

## レイヤー

- Domain
- Application
- Adaptor
- View
- InfraStructure
- Composition

に分ける。

下層への参照を行いたい場合は、自層にintarfaceを追加し、下層がそれを実装してCompositionがDI注入する。

他モジュールへの依存は、Adaptor層のみが依存し、Composition層が依存性解決を行う。

## **各レイヤーの説明**

!image.png

### **Domain**

データ層。ピュアクラス。 参照レイヤーはなし。

ロジックで使用するデータの保存層。 データに関するロジックを持つ。

### **Application**

処理層。ピュアクラス。 参照レイヤーはDomain。

Domainを操作してロジックを処理する。

### **Adaptor**

伝達層。ピュアクラス。 参照レイヤーはDomain/Application。

Applicationの処理を呼び出したり、Domainのデータを他のApplicationへ橋渡しする。 View層へデータを受け渡すViewからの入力をApplicationに受け渡す。

Viewへの受け渡しはViewModelを使用する。

### **View**

表示層。Unityフレームワーク。 参照レイヤーはAdaptor。

ゲームオブジェクトやレンダリングの操作を行う。 入力を受け取ってAdaptorに受け渡す。

### **InfraStructure**

データ転送層。Unityフレームワーク。参照レイヤーはDomain、Application、View。

ScriptableObjectやDataBaseを使用して取得する実装を行う。

### **Composition**

初期化層。Unityフレームワーク/ピュアクラスのハイブリッド。 参照レイヤーはDomain/Application/Adaptor/View/InfraStructure。

Domain、Application、Adaptor、View、InfraStructureのシステムの依存性注入を行う。

# **クラス設計**

## Domain層

### Entity

Domain層にある値が可変な参照型オブジェクト。

型は`class` 。

- サンプルコード
    
    ```csharp
    public class Entity
    {
        public Entity(string id)
        {
            _id = id;
            _value = 0;
        }
    
        public string ID => _id;
        public float Value => _value;
    
        public void ChangeValue(float value) => _value = value;
    
        private readonly string _id;
        private float _value;
    }
    ```
    

公開プロパティは読み取り専用にし、変更は`ChangeValue`のように意図が伝わるメソッド経由で行う。オートプロパティの`public set`は使用しない。

### ValueObject（VO）

Domain層にある値が不変な値型オブジェクト。

型は`readonly struct` 。

- サンプルコード
    
    ```csharp
    public readonly struct ValueObject : IEquatable<ValueObject>, IComparable<ValueObject>
    {
        public ValueObject(float value = 0)
        {
            if (value < 0) { throw new ArgumentException("value is can't negative", nameof(value)); }
            _value = value;
        }
    
        public float Value => _value;
    
        public static bool operator ==(ValueObject left, ValueObject right) => left.Equals(right);
        public static bool operator !=(ValueObject left, ValueObject right) => !left.Equals(right);
        public static bool operator <(ValueObject left, ValueObject right) => left.CompareTo(right) < 0;
        public static bool operator <=(ValueObject left, ValueObject right) => left.CompareTo(right) <= 0;
        public static bool operator >(ValueObject left, ValueObject right) => left.CompareTo(right) > 0;
        public static bool operator >=(ValueObject left, ValueObject right) => left.CompareTo(right) >= 0;
    
        public int CompareTo(ValueObject other) => _value.CompareTo(other._value);
        public bool Equals(ValueObject other) => _value == other._value;
        public override bool Equals(object obj) => obj is ValueObject other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
    
        private readonly float _value;
    }
    ```
    

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