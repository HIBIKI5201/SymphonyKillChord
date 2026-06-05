# Scenario System Commands

シナリオシステムで現在コード上に存在するコマンド一覧です。  
実装根拠は主に `ScenarioRepository` の `CreateAuthoringEventDefinition` / `CreateEventDefinition` / `CreateAuthoringTrigger` / `CreateTrigger` です。

## 対象コマンド一覧

| Type | 概要 | 備考 |
| --- | --- | --- |
| `Text` | 話者名と本文を表示する | `Trigger` を紐づけ可能 |
| `Background` | 背景を切り替える | 背景ID必須 |
| `Animation` | アニメーションを再生する | アニメーションID必須 |
| `Fade` | フェード値を補間する | `start`, `end`, `duration` 必須 |
| `Portrait` | 立ち絵を表示・更新する | 座標、拡大率、表示状態は省略可 |
| `Layer` | 描画順を変更する | 対象レイヤーと順序が必須 |
| `Trigger` | `Text` の途中または末尾で別イベントを発火する | `Text` にのみ紐づけ可能 |

## Authoring CSV 形式

配置先:

```text
Assets/StreamingAssets/ScenarioAuthoring/{ScenarioId}.events.csv
```

基本は `Step,Type,...` 形式で記述します。

### 1. Text

```csv
5,Text,Hero,Hello world
```

書式:

```csv
Step,Text,Speaker,Text
```

### 2. Background

```csv
1,Background,background
```

書式:

```csv
Step,Background,BackgroundId
```

### 3. Animation

```csv
2,Animation,OpeningPose
```

書式:

```csv
Step,Animation,AnimationId
```

### 4. Fade

```csv
8,Fade,0,1,0.5
```

書式:

```csv
Step,Fade,FadeStart,FadeEnd,FadeDuration
```

### 5. Portrait

```csv
3,Portrait,Left,hero,-460,-120,1,true
```

書式:

```csv
Step,Portrait,PortraitSlot,PortraitId,PortraitPosX,PortraitPosY,PortraitScale,PortraitVisible
```

省略時の既定値:

| 項目 | 既定値 |
| --- | --- |
| `PortraitPosX` | `0` |
| `PortraitPosY` | `0` |
| `PortraitScale` | `1` |
| `PortraitVisible` | `true` |

有効な `PortraitSlot`:

- `Left`
- `Center`
- `Right`

### 6. Layer

```csv
4,Layer,PortraitLeft,2
```

書式:

```csv
Step,Layer,LayerTarget,LayerOrder
```

有効な `LayerTarget`:

- `Background`
- `PortraitLeft`
- `PortraitCenter`
- `PortraitRight`
- `Text`
- `Canvas`

### 7. Trigger

`Trigger` は単独表示イベントではなく、既存の `Text` ステップに紐づく追加行です。

```csv
6,Trigger,5,AtKeyword,,Hero,Fade,0,1,0.25
```

書式:

```csv
Step,Trigger,ParentStep,TriggerType,TriggerIndex,TriggerKeyword,OnTriggerType,OnTriggerArg1,OnTriggerArg2,OnTriggerArg3
```

各項目:

| 項目 | 説明 |
| --- | --- |
| `ParentStep` | 発火元になる `Text` の `Step` |
| `TriggerType` | 発火条件 |
| `TriggerIndex` | `AtCharIndex` のときに使用 |
| `TriggerKeyword` | `AtKeyword` / `AtSuffix` のときに使用 |
| `OnTriggerType` | 発火時に生成するイベント種別 |
| `OnTriggerArg1..3` | 発火イベントの引数 |

有効な `TriggerType`:

- `AtCharIndex`
- `AtKeyword`
- `AtSuffix`
- `AtTextEnd`

`OnTriggerType` と引数対応:

| OnTriggerType | 必要引数 |
| --- | --- |
| `Fade` | `OnTriggerArg1=FadeStart`, `OnTriggerArg2=FadeEnd`, `OnTriggerArg3=FadeDuration` |
| `Background` | `OnTriggerArg1=BackgroundId` |
| `Animation` | `OnTriggerArg1=AnimationId` |
| `Portrait` | `OnTriggerArg1=PortraitSlot`, `OnTriggerArg2=PortraitId`, `OnTriggerArg3=PortraitPosX` |
| `Layer` | `OnTriggerArg1=LayerTarget`, `OnTriggerArg2=LayerOrder` |

注意:

- `ParentStep` は `Text` のステップである必要があります。
- `Trigger` 自体は再生イベント列には入らず、親 `Text` に付与されます。
- `OnTriggerType=Portrait` の authoring 形式では `Y`, `Scale`, `Visible` は指定できず、固定で `0`, `1`, `true` です。

## Normalized CSV 形式

先頭データ行が `Type,` で始まる場合、ヘッダー付き normalized CSV として解釈されます。

例:

```csv
Type,Step,Speaker,Text,BackgroundId,AnimationId,FadeStart,FadeEnd,FadeDuration,PortraitSlot,PortraitId,PortraitPosX,PortraitPosY,PortraitScale,PortraitVisible,LayerTarget,LayerOrder,ParentStep,TriggerType,TriggerIndex,TriggerKeyword,OnTriggerType,OnTriggerArg1,OnTriggerArg2,OnTriggerArg3
Background,1,,,background,,,,,,,,,,,,,,,,,,,
Text,2,Hero,Hello world,,,,,,,,,,,,,,,,,,,
Trigger,,,,,,,,,,,,,,,,2,AtTextEnd,,,Fade,0,1,0.25
```

主な列:

| 列名 | 用途 |
| --- | --- |
| `Type` | コマンド種別 |
| `Step` | イベント順序 |
| `Speaker`, `Text` | `Text` 用 |
| `BackgroundId` | `Background` 用 |
| `AnimationId` | `Animation` 用 |
| `FadeStart`, `FadeEnd`, `FadeDuration` | `Fade` 用 |
| `PortraitSlot`, `PortraitId`, `PortraitPosX`, `PortraitPosY`, `PortraitScale`, `PortraitVisible` | `Portrait` 用 |
| `LayerTarget`, `LayerOrder` | `Layer` 用 |
| `ParentStep`, `TriggerType`, `TriggerIndex`, `TriggerKeyword`, `OnTriggerType`, `OnTriggerArg1..3` | `Trigger` 用 |

normalized 形式の補足:

- `Trigger` は独立行として記述します。
- `Text` 行には inline trigger を1つだけ直接持たせることもできます。
- `Step` 未指定時は自動採番されます。

## 実装上の有効値まとめ

### PortraitSlot

- `Left`
- `Center`
- `Right`

### LayerTarget

- `Background`
- `PortraitLeft`
- `PortraitCenter`
- `PortraitRight`
- `Text`
- `Canvas`

### TriggerType

- `AtCharIndex`
- `AtKeyword`
- `AtSuffix`
- `AtTextEnd`

### OnTriggerType

- `Fade`
- `Background`
- `Animation`
- `Portrait`
- `Layer`

## 補足

- 現在の実装では、通常イベントとして解釈されるのは `Text`, `Background`, `Animation`, `Fade`, `Portrait`, `Layer` です。
- `Trigger` は補助コマンドであり、親 `Text` が存在しないとエラーになります。
- `unknown Type` は `FormatException` になります。
