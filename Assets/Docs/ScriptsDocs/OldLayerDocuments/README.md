# スクリプト構造ドキュメント

このディレクトリには、`Assets/Scripts/Runtime` 配下の主要モジュールをレイヤー別に整理したドキュメントを置いています。

現在の実装を基準にしているため、将来的な拡張用クラスや未接続の補助クラスは「存在はするが常用フローには未接続」と分けて記載します。

## 機能別ドキュメント

- **[InGame.md](./InGame.md)**: 戦闘、カメラ、敵、音楽同期、プレイヤー操作、スキル判定を含むゲームプレイ中の機能。
- **[Persistent.md](./Persistent.md)**: 入力履歴、常駐BGM、シーン切り替えなど、シーンをまたいで使う機能。
- **[Player.md](./Player.md)**: プレイヤースキルのデータ定義とリポジトリ。
- **[Utility.md](./Utility.md)**: 汎用コレクション、定数、列挙型。

## レビュー / 補足ドキュメント

- **[Runtime-DesignReview-2026-04-24.md](./Runtime-DesignReview-2026-04-24.md)**: `Assets/Scripts/Runtime` 全体の設計レビューと問題点の整理。

## レイヤー定義

| レイヤー名 | 役割 |
| :--- | :--- |
| **0. Utility** | 汎用クラス、定数、列挙型、データ構造。 |
| **1. Domain** | 値オブジェクト、エンティティ、仕様定義。Unity 依存は最小限。 |
| **2. Application** | ドメインを使ったユースケースや計算処理。 |
| **3. Adaptor** | Controller / Presenter / ViewModel 変換。 |
| **4. View** | MonoBehaviour、UI、見た目や Unity コンポーネント操作。 |
| **5. InfraStructure** | ScriptableObject、Factory、外部サービス実装。 |
| **6. Composition** | 初期化、依存関係の組み立て、エントリポイント。 |

## 補足

- `Assets/Scripts/Runtime` にはクリーンアーキテクチャ寄りの分割がありますが、現時点では一部モジュールで「用意済みだが未接続」のクラスが残っています。
- このドキュメントは理想構成ではなく、現行コードで追える依存関係を優先して説明します。
