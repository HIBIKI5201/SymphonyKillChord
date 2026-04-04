# Player 機能構造

Player は、プレイヤーの永続的なデータ（スキル、成長要素など）や、プレイヤーに関連する抽象的な概念を担当します。

## モジュール詳細

- **[Player-Skill](./Modules/Player-Skill.md)**: スキルデータの定義、効果の実装、リポジトリ。

## レイヤー構造

### 1. Domain
- **Skill**: `SkillData` (スキルの基本情報), `ISkillEffect` (スキル効果の抽象), `ISkillVisual` (スキル演出の抽象)。

### 2. Application
- **SkillEffect**: スキル効果の具象クラス (`TestSkillEffect` など)。
- **SkillVisual**: スキル演出の具象クラス (`SkillVisualTest` など)。

### 5. InfraStructure
- **Repository**: `SkillRepository` (スキルの保存・取得ロジック)。

## 構造図 (Mermaid)

### スキルシステムの抽象構造

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
