# Scenario Authoring CSV

Google Spreadsheet で入力したシナリオを、既存ランタイム形式の CSV に変換するためのフォルダです。

- 入力: `*.events.csv`, `*.triggers.csv`
- 出力: `Assets/StreamingAssets/Scenario/<ScenarioId>.csv`

## 使い方

1. Spreadsheet で `events` シートと `triggers` シートを作成する
2. それぞれ CSV で書き出す
3. `Assets/StreamingAssets/ScenarioAuthoring` に配置する
   - `<ScenarioId>.events.csv`
   - `<ScenarioId>.triggers.csv`
4. Unity で `Tools/KillChord/Scenario/Convert Authoring CSV (All)` を実行する

## 入力フォーマット

次の 2 つの形式をサポートしています。

1. 簡易CSV形式（推奨）
2. 列形式（従来）

## ファイル名ルール

- 例:
  - `ScenarioTest.events.csv`
  - `ScenarioTest.triggers.csv`
- `ScenarioId` はファイル名の先頭（例: `ScenarioTest`）から自動判定されます

## ヘッダー行について

- `events.csv` / `triggers.csv` ともに**ヘッダー行は省略可能**です
- ヘッダー行を省略する場合は、下記の列順を必ず守ってください

## events.csv 仕様

### 列順

`Step,Type,Speaker,Text,BackgroundId,AnimationId,FadeStart,FadeEnd,FadeDuration`

### イベント一覧と機能説明

1. `Text`
   - 機能: セリフ表示イベント
   - 使用列: `Step, Type, Speaker, Text`
2. `Background`
   - 機能: 背景切り替えイベント
   - 使用列: `Step, Type, BackgroundId`
3. `Animation`
   - 機能: アニメーション再生イベント
   - 使用列: `Step, Type, AnimationId`
4. `Fade`
   - 機能: フェード制御イベント（開始値・終了値・秒数）
   - 使用列: `Step, Type, FadeStart, FadeEnd, FadeDuration`

### 簡易CSV形式（推奨）

書式:

`<Step>,<Type>,<args...>`

例:

1. `1,Text,Hello world`
2. `2,Background,genki_pose`
3. `3,Text,Hero,Hello world`
4. `4,Fade,1,0,0.5`
5. `5,Animation,anim_hero_idle`

補足:

- `Text` は 3 列なら `Text` 本文のみ、4 列以上なら `Speaker,Text` 扱いです
- `Text` の本文にカンマを含めたい場合は `"Hello, world"` のようにダブルクォートで囲ってください

## triggers.csv 仕様

### 列順

`ParentStep,TriggerType,TriggerIndex,TriggerKeyword,OnTriggerType,Arg1,Arg2,Arg3`

### TriggerType 一覧と機能説明

1. `AtCharIndex`
   - 機能: 文字表示位置が指定 index に達したら発火
   - 必須: `TriggerIndex`
2. `AtKeyword`
   - 機能: 表示中テキストに指定キーワードが含まれたら発火
   - 必須: `TriggerKeyword`
3. `AtSuffix`
   - 機能: 表示中テキストの末尾が指定文字列と一致したら発火
   - 必須: `TriggerKeyword`
4. `AtTextEnd`
   - 機能: テキスト表示の最後で発火
   - 必須: なし

### OnTriggerType 一覧と機能説明

1. `Fade`
   - 機能: フェードイベントを発火
   - 引数: `Arg1=Start, Arg2=End, Arg3=Duration`
2. `Background`
   - 機能: 背景切り替えイベントを発火
   - 引数: `Arg1=BackgroundId`
3. `Animation`
   - 機能: アニメーション再生イベントを発火
   - 引数: `Arg1=AnimationId`

### 簡易CSV形式（推奨）

書式:

`<ParentStep>,<TriggerType>,[triggerArg],<OnTriggerType>,<onArgs...>`

例:

1. `3,AtCharIndex,5,Fade,0,1,0.5`
2. `3,AtKeyword,danger,Background,bg_alert`
3. `7,AtTextEnd,Fade,1,0,0.5`

## 補足

- `*.triggers.csv` が存在しない場合は、トリガーなしで変換されます
- 変換時に必須列不足、型不正、Step 重複、ParentStep 不整合はエラーになります
