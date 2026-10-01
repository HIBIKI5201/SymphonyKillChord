# StageNodeAssetの使い方

- id: 3797c2c6-cc02-8083-8469-f92524ecff07
- path: Symphony Kill Chord / システム概要 / StageNodeAssetの使い方
- last_edited: 2026-06-10T03:04:42.890Z

- カテゴリー: インゲーム


## 場所
StageNodeAsset は現在 Assets/Level/Scenes/Develop/OutGameTest/StageSelect/ 内に配置されています。
[image: image.png] attachment:60d0ee4b-fb34-4002-baee-549de74f5f57:image.png

## 各パラメーターの意味

### 基本情報

#### Stage ID 
ステージノードの ID を設定します。
数値で設定してください。
UIToolkit上で設定しているノードの名前の数値と一致させてください。 

#### StageType
ステージのタイプを設定します。
タイプによって異なる処理が行われます。
- シナリオの場合
→ ステージノードが選択された時に表示される、ステージ詳細画面の出撃ボタンを押した時に 後述する TargetSceneName に直接遷移します。
- バトルの場合
→ステージノードが選択された時に表示される、ステージ詳細画面の出撃ボタンを押した時に後述する TargetSceneName を戦闘準備画面に渡し、戦闘準備画面に遷移します。
戦闘準備画面から TargetSceneName に遷移します。
- IsInitiallyUnlocked
ステージのノードを最初から解放済みにするかどうかを設定します。
（なので、StageID 1のものに設定されているのが望ましいのではないでしょうか？）

### UI 情報

#### StageName
ステージノードが選択された時に表示されるステージの名前です。
[image: image.png] attachment:34816673-1f7e-4237-860c-846332125480:image.png

#### FlavorText
ステージノードが選択されたときに表示されるステージのフレーバーテキストです。

[image: image.png] attachment:a6caeb71-ef0f-4f32-936b-6600ba4418cc:image.png

### シーン遷移

#### TargetSceneName
そのステージノードがどこのシーンに遷移したいのかを設定するためのものです。

### クリア報酬

#### RewardSkillBuildPoint・RewardSkillUnlockPoint
ステージノードが選択された時に表示されるステージをクリアした時の報酬を設定します。
[image: image.png] attachment:34c31e08-a1b2-4a16-ac2f-890ae3811924:image.png

#### MissionDefinitionAsset
ステージのタイプが Battle の時に設定します。