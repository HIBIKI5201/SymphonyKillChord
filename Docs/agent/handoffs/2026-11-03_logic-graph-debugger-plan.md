# ロジックのグラフ表示デバッグウィンドウ 実装計画（検討段階）

- 作成日: 2026-11-03
- 状態: 計画のみ。コードは未変更。実装するときは feature-flow スキルで進める。
- 対象: ゲームのロジック（攻撃・スキル・初期化など）の流れを、Play中にノードとエッジのグラフで表示するエディタ用デバッグウィンドウ。

## 1. 前提と確認できなかったこと

- 依頼にあった X の動画は、この作業環境から x.com に接続できず見られなかった。「実行中のロジックがノードとして並び、処理が通ったノードやエッジが光る・値が見える」ような表示を想定して計画した。動画の内容と違う点があれば、そこから直す。
- Unity 公式ドキュメント（docs.unity3d.com）とフォーラムも直接は開けず、Web 検索の要約から確認した。下の表で「確認済み」は検索結果に公式ページの記述として出たもの、「推測」は裏付けを取れていないもの。
- プロジェクトの Unity は `6000.3.10f1`（Unity 6.3 LTS）。

## 2. Unity 6.x で使えるグラフ表示の手段

| 手段 | 使えるバージョン | 内容 | 確度 |
| --- | --- | --- | --- |
| Graph Toolkit（モジュール） | 6.4 以降 | 6.4 でエディタ組み込みのモジュールになり、パッケージ導入が不要になった。カスタムのグラフ編集ツールを作るための「前面（UI）」の枠組み。 | 確認済み |
| Graph Toolkit（実験版パッケージ `com.unity.graphtoolkit` 0.x） | 6.2〜6.3 | 実験版で本番利用は非推奨。6.4 以降に入れるとモジュールと衝突してコンパイルエラーになる。 | 確認済み |
| Graph Toolkit のコードからのグラフ生成 | 6.5 以降 | `GraphDatabase.CreateGraph<T>`、`AddNode`、`Connect` などでグラフをコードから作れるようになった（6.5 alpha の告知）。 | 確認済み（メソッド名は 6.5 正式版で再確認が必要） |
| Graph Toolkit の実行時デバッグ表示 | 未定 | 6.5 alpha の告知で「ワイヤーのアニメーションやノードの進捗バーなどのデバッグ・視覚フィードバックに取り組む」と予告。6.5・6.6 で出たという記述は見つからなかった。 | 推測（予告のみ） |
| Graph Toolkit の実行環境 | - | 公式ドキュメントに「実行時の実行バックエンドは含まない」とある。実行時の動きは自前で作る。 | 確認済み |
| GraphView（`UnityEditor.Experimental.GraphView`） | 6.3 で利用可 | Shader Graph などで使われてきた旧来の UI Toolkit 製グラフ UI。パン・ズーム・ミニマップ・ノード移動が揃っている。Experimental 名前空間のまま。 | 6.3 で使えることは確認済み。将来 Graph Toolkit に置き換わり削除される時期は推測 |
| UI Toolkit の自作（`VisualElement` ＋ `Painter2D`） | 6.3 で利用可 | ノードを VisualElement、エッジを Painter2D で描く。部品はすべて自作。 | 確認済み |
| 6.6 の状況 | - | 6.6 は検索時点で Alpha のドキュメントしか確認できず、LTS かどうかも確認できなかった。 | 推測 |

要点: Graph Toolkit は「グラフをアセットとして編集するツール」を作るための枠組みで、Play 中に変わり続けるロジックを読み取り専用で映す用途とは向きが違う。6.5 のコード生成 API とデバッグ表示が揃えば使いやすくなるが、プロジェクトは 6.3 LTS なので今すぐは使えない。

## 3. プロジェクト側の現状（調べた範囲）

- デバッグ窓口: `Assets/Editor/Scripts/KillChordDebugger/KillChordDebuggerWindow.cs`（UI Toolkit。現在はセーブデータのパネルだけ）。ここにパネルを足す形がそのまま使える。
- グラフ表示の前例: `Assets/Editor/Scripts/Inspectors/SourceData/StageTreeGraphView.cs` が IMGUI の `Handles` でステージツリーをグラフ表示している（GraphView・Graph Toolkit は未使用）。
- グラフにしやすいロジック:
  - 攻撃パイプライン: `AttackPipeline` が `IAttackStep[]`（`WeaponDamageStep`、`CriticalStep`、`OutOfRangeDamageStep` など）を順に実行する。一直線のグラフで、ステップごとのダメージ値を出せる。
  - スキル → スキル効果 → 状態異常・バフ（`2.Application/InGame/Skill`、`SkillEffect`、`StatusEffect`、`Buff`）: Just 判定やコンボ発動からダメージまでの因果。リズムの利点を強める議論（10-30 議事録）の確認に一番役立つ。
  - 初期化: `InitializationCoordinator` と `IInitializationModule.Order`。起動順の依存グラフ。
  - 敵 AI は Unity Behavior のグラフで、Behavior 側に実行時のデバッグ表示があるため対象外。
- 設計上の制約: 設計思想により Runtime にデバッグ機能を入れてはいけない。データの取り出し口は Runtime 側にインターフェースだけ置き、実装はエディタ・開発用アセンブリに置いて Composition で差し込む必要がある。

## 4. 構成案（どの表示手段でも共通の部分）

表示とデータ収集を分けて、表示手段を後で差し替えられるようにする。

1. トレース口（Runtime）: Application 層に `ILogicTraceSink` のようなインターフェースを置く。既定は何もしない実装で、リリースビルドでは呼び出しが消えるか何もしない。
2. 記録（Develop またはエディタ側）: 実装はリングバッファに「ノード ID・発生時刻・拍位置・値」を構造体で積む。毎フレームの割り当てを出さない。
3. 差し込み（Composition）: `UNITY_EDITOR || DEVELOPMENT_BUILD` のときだけ記録の実装を注入する。
4. 表示（Editor）: KillChordDebugger に「ロジック」パネルを追加し、記録を読んで、通ったノード・エッジを光らせ、選んだノードの直近の値を出す。一時停止とコマ送りで過去の実行を追えるようにする。

## 5. 決めてほしいこと

### Q1. グラフの表示に何を使うか
1. ⭐ GraphView で作る（6.3 のまま。パン・ズーム・ミニマップがすぐ使える。将来 Graph Toolkit に移すときは表示部分だけ差し替える）
2. UI Toolkit で自作する（API の廃止リスクがない。パン・ズーム等を自作する分、工数が増える）
3. Unity を 6.5 以降に上げて Graph Toolkit で作る（公式の新しい枠組み。ただしエンジン更新の影響が大きく、実行時デバッグ表示は未提供の可能性が高い）

### Q2. 最初にグラフにするロジック
1. ⭐ 攻撃パイプライン（小さく一直線で、仕組みの確認に向く）
2. スキル → 効果 → ダメージの因果（価値は一番高いが範囲が広い。2 段階目に回す案）
3. 初期化モジュールの起動順

### Q3. どこで見られるようにするか
1. ⭐ Unity エディタの Play 中だけ
2. 開発ビルド（実機）から記録を送ってエディタで見る（通信部分が増える）

### Q4. 動画で一番真似したい点
動画を見られなかったので、「処理が流れる様子のアニメーション」「ノードに値が出る」「グラフの形そのもの（自動生成）」など、真似したい点を一言もらえると計画を合わせられる。

## 6. 段階案（Q1〜Q3 が推奨どおりの場合）

| 段階 | 内容 | 完了条件 |
| --- | --- | --- |
| 1 | トレース口・記録・差し込みを作り、攻撃パイプラインに付ける。テキスト一覧で記録を確認 | EditMode テストで記録内容を確認。リリースビルドで何もしないこと |
| 2 | KillChordDebugger に GraphView のパネルを追加し、攻撃パイプラインをグラフ表示・ハイライト | エディタで Play して表示を確認 |
| 3 | スキル → 効果 → ダメージの因果を追加 | Just 判定から最終ダメージまで追える |
| 4 | Unity を 6.5 以降に上げる判断が出たら Graph Toolkit への置き換えを検討 | - |

各段階は feature-flow で 1 PR ずつ進める。エディタで確認できる内容はエディタで確認する（2026-11-01 の方針）。

## 7. 出典（検索結果で確認したページ）

- Graph Toolkit（Unity 6.4 マニュアル）: https://docs.unity3d.com/Manual/gtk/gtk-index.html
- Graph Toolkit 実験版 0.4 の紹介: https://docs.unity3d.com/Packages/com.unity.graphtoolkit@0.4/manual/introduction.html
- Graph Toolkit Update in Unity 6.5 alpha（フォーラム）: https://discussions.unity.com/t/graph-toolkit-update-in-unity-6-5-alpha/1712169
- `GraphDatabase.CreateGraph<T>` の API: https://docs.unity.com/en-us/engine/6000.7/script-reference/unity/graphtoolkit/editor/graphdatabase/creategraph
- 6.4 に実験版パッケージを入れたときの衝突: https://issuetracker.unity.com/issues/17919/multiple-errors-thrown-in-the-console-after-installing-graph-toolkit-package
- Unity 6.6 Alpha の Graph Toolkit マニュアル: https://docs.unity3d.com/6000.6/Documentation/Manual/gtk/gtk-index.html
