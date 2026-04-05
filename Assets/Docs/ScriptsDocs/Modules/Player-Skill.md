# Player-Skill

Player カテゴリーにおけるスキルシステムのモジュール詳細。

## 構造概要

スキルシステムは、スキルのデータ定義（Domain）、実際の効果（Application）、およびデータの保存・取得（InfraStructure）で構成されています。

### 1. Domain
- **SkillData**: スキルの名前、説明、威力、必要なリソースなどの基本データ。
- **ISkillEffect**: スキルが実行された際の効果を定義するインターフェース。
- **ISkillVisual**: スキルの発動時に再生される演出を定義するインターフェース。

### 2. Application
- **TestSkillEffect**: デバッグや初期実装用のスキル効果クラス。
- **SkillVisualTest**: スキルの視覚演出を行うためのテスト用クラス。

### 5. InfraStructure
- **SkillRepository**: ScriptableObject や保存データからスキル情報をロードするリポジトリ。

## クラス間連携図 (Mermaid)

```mermaid
graph TD
    subgraph Domain
        SD[SkillData]
        ISE[ISkillEffect]
        ISV[ISkillVisual]
    end

    subgraph Application
        TSE[TestSkillEffect]
        SVT[SkillVisualTest]
    end

    subgraph InfraStructure
        SR[SkillRepository]
    end

    SD --> ISE
    SD --> ISV
    ISE <|-- TSE
    ISV <|-- SVT
    SR --> SD
```
