# Stage 4: Unity 対応 Anatomia アーキテクチャ解析

**ステータス: complete（決定的静的解析について完了）。ランタイムとUnityのserialized配線は対象外。**

## 解析方法

Anatomia commit `d0cc4d6e77d1a10d1b4b1ca911026a1bec304cbb` を固定し、`Assets/Scripts/Runtime` 799 C# files を解析した。tree-sitter WASM で構文木を作り、型・関数・呼出・継承・Unity lifecycle を規則ベースでグラフ化する。LLM、clang、Unity Editor は使わない。Unity profile は `unity`、既定表示は class graph となり、74 Unity views / 22 screen edges を検出した。

解析器の build と Unity向け focused tests は成功した（3 files / 5 tests）。プロジェクト本体の起動やサービス起動はしていない。

## 全体結果

| 指標 | 最終値 | 解釈 |
|---|---:|---|
| Functions | 2,311 | `Assets/Scripts/Runtime`の解析対象 |
| Call edges | 7,780 | 1,954 unresolved callsを別途保持 |
| Generic violations | 11 | 候補であり、欠陥確定ではない |
| Hotspots | 20 | scene transition系が上位 |
| Cycles | 201 | 同名／overload誤結合の確認が必要 |
| Orphans | 715 | Unity lifecycle認識後の値 |
| Cross-domain coupling | 107 | 仕様起点10境界間の静的結合 |
| Spec gaps | 792 | 未配線候補。未実装確定値ではない |
| Spec links | 8 | 68 clausesに対して少なく、網羅性不足 |

上位hotspotは `SceneTransitionUsecase.LoadSceneAndWaitForReadyAsync`、`UnloadAndSetActiveAsync`、`ScenePriorityResolver.Resolve`、`SceneTransitionService.ReloadSceneAsync` / `ChangeSceneAsync` で、coupling 98–102、cyclomatic 47–50。これはフロー境界の集中を示すが、Scene遷移の性質上必要な協調も含む。

## 仕様起点10ドメイン

| Domain | Implementors | Cohesion | Isolated |
|---|---:|---:|---:|
| Musical Time and Adaptive Arrangement | 42 | 0.480 | 22 |
| Rhythm Input and Kill Chord Resolution | 78 | 0.295 | 33 |
| Player Action and Combat Targeting | 128 | 0.727 | 49 |
| Combat State and Effect Resolution | 175 | 0.307 | 111 |
| Enemy Encounter and Stage Simulation | 424 | 0.837 | 149 |
| Mission Evaluation and Result | 198 | 0.537 | 89 |
| Progression, Research and Loadout | 317 | 0.505 | 107 |
| Narrative and Game Flow | 418 | 0.384 | 166 |
| Persistence and Player Settings | 145 | 0.720 | 36 |
| Guidance, Feedback and Recovery | 323 | 0.617 | 129 |

`Rhythm Input`、`Combat State`、`Narrative` の低いcohesionは境界漏れ／adapter混入の調査優先度を上げる。重複所属は Music/Skill/UI の意図的横断も含むため、単純にゼロを目指さない。

集計の99.78% assignment coverageは built-in `state-machine` が2,271 functionsを取得するため、手動10ドメインの強さとして使わない。`Enemy Encounter`、`Guidance`、`Mission`、`Narrative`、`Player Action`、`Rhythm Input` の spec-integrity warning は「specRefがあるがspec-linked implementorが0」の警告であり、未実装の確定ではない。

## 制約と確認方法

- Reflection、Service Locator、DI、UnityEvent、serialized Prefab/Scene、Addressables、CRIの実行時経路は静的call graphだけでは完全に解決できない。
- generic violation例（`SkillTreeInitializer`、`AttackIntervalEvaluator`）はソース確認後に修正判断する。
- raw data: `spec/data/anatomia-unity-*.json`
- interactive graph: `report/architecture-review-unity.html`
