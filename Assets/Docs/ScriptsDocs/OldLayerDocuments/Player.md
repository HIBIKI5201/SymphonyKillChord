# Player 機能構造

このカテゴリは、プレイヤーが保持するスキル定義データをまとめています。現在の実装では「進行データ全般」ではなく、主にスキル定義とその保存元が対象です。

## モジュール詳細

- **[Player-Skill](./Modules/Player-Skill.md)**: スキル定義データとリポジトリ。

## レイヤー構造

### 1. Domain
- **SkillData**: ScriptableObject などに載せるための設定用データ。
- **ISkillEffect / ISkillVisual**: スキル発動時の処理と演出の抽象。

### 2. Application
- このカテゴリ直下の Application はありません。
- スキル判定や利用側のロジックは `InGame/Skill` にあります。

### 5. InfraStructure
- **SkillRepository**: `SkillData` 配列を保持し、`SkillDefinition` を返す ScriptableObject 実装。

## 現在の実装メモ

- `SkillData` 自体は `KillChord.Runtime.Domain.Player` 名前空間にありますが、変換先は `InGame/Skill` の `SkillDefinition` です。
- `TestSkillEffect` と `SkillVisualTest` は具象実装ですが、カテゴリとしては `Player` よりも「スキル利用側」に近い位置づけです。
