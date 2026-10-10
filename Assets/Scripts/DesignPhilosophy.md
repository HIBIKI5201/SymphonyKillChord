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

# クラス設計の参照

変更・レビュー対象の型に対応する [DesignClassRoles.md](DesignClassRoles.md) のレイヤー節を読む。実装例が必要な場合だけ [DesignExamples.md](DesignExamples.md) を読む。クラス責務の規則も設計思想の正本の一部である。
