# Player-Skill

スキル定義データのモジュールです。実行時の判定や発動は `InGame/Skill` 側が担当し、このモジュールは主にデータ供給を担います。

## 構造概要

### 1. Domain
- **SkillData**: ID、入力パターン、`ISkillEffect`、`ISkillVisual` を保持する設定データ。
- **ISkillEffect / ISkillVisual**: スキル発動時の処理と演出の抽象。

### 2. Application
- このモジュール固有の Application はありません。
- 利用側には `SkillCheckService` と `SkillController` があります。

### 5. InfraStructure
- **SkillRepository**: `SkillData[]` を保持し、要求された ID を `SkillDefinition` に変換して返す ScriptableObject。

## 現在の実装メモ

- `SkillData.ToSkillDefinition()` で `SkillId`、`SkillPattern`、`ISkillEffect`、`ISkillVisual` を束ねた `SkillDefinition` を生成します。
- `TestSkillEffect` と `SkillVisualTest` はドメイン抽象の具象実装で、テスト用途の色が強いクラスです。
