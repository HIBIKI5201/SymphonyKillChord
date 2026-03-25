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

ただしUnityフレームワークへの依存は許容する。 Unityライフサイクルへの依存は抑える。

レイヤーを

- Domain
- Application
- Adaptor
- View
- InfraStructure
- Composition

に分ける。

下層への参照を行いたい場合は、自層にintarfaceを追加し、下層がそれを実装してCompositionがDI注入する。

## **各レイヤーの説明**

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

### ValueObject

Domain層にある値が不変な値型オブジェクト。

## Adaptor層

### Presenter

Adaptor層にあるViewとDomainを仲介するクラス。

Query CommandのQueryを担当する。

### Controller

Adaptor層にあるDomainやApplicationの処理を実行するクラス。

Query CommandのCommandを担当する。

### DataTransferObject（DTO）

Adaptor層にあるViewModelへ更新データを送る値型オブジェクト。

型は`readonly ref struct` 。

## InfraStructure層

### Repository

InfraStructure層に実装があり、Application層に抽象がある。

DBの取得処理やScriptableObjectなどを持つ。

### Factory

InfraStructure層に実装があり、Application層とView層に抽象がある。

## View層

### ViewModel

View層にあるUIに表示するデータを保持するクラス。
ReactivePropaty指向で実装する。

DTOを受け取るメソッドを用意する。
DTOは`in` を使用する。

## **Composition層**

### Initializer

Composition層にある初期化やDIを実行するクラス。