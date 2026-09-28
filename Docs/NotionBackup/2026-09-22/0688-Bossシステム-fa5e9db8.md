# Bossシステム

- id: 37a7c2c6-cc02-8022-ae1c-c799fa5e9db8
- path: Symphony Kill Chord / システム概要 / Bossシステム
- last_edited: 2026-06-09T06:35:00.737Z

- カテゴリー: 開発用


## ボスシステム 開発ドキュメント
> 💡 
  **ブランチ:** `feature/research/boss/miyamoto`

  **担当:** 　宮本

  **作業期間:** 2026-06-05 〜 2026-06-09
---


### 結論
そもそも既存のEnemyLifeCycleだと仕様どおりに動かないので新たに色々実装した。
原因 : **主に攻撃パターンが一つしか持てない。**

### 注意事項
現状のコードだけだとテストができないので
InGameCompositon.csに ServiceLocator.RegisterInstance(targetManager);
 ServiceLocator.RegisterInstance(targetEntityRegistry);
を追加してほしい
且つInGameSceneにBossInitializerとBossプレファブを追加しないといけない
BossTestSceneだと何にも動きません

### 機能の概要
通常敵の構造を踏襲しつつ、ボス専用の拡張を加えた **ボスAIシステム** を新規実装。  
主な特徴は以下の3点：
- **複数攻撃パターン対応** — 通常1 / 通常2 / 特殊（三方向弾）を差し替えなしで保持し、予約ごとにランダム選択
- **音楽同期攻撃予約** — `IMusicActionScheduler` を利用して拍子単位で攻撃タイミングを制御。攻撃の1拍前・2拍前の予兆も自動スケジュール
- **Behavior Graph 新規作成** — Unity Behavior Graph 上で動作する専用ノード群（Action / Condition）でボスの行動ツリー既存のEnemyAIGraph を参考に制御
---

### 変更ファイル一覧

#### 新規作成（Scripts）
| ファイル | 役割 |
|---|---|
| `BossAIController.cs` | ボス行動の中核。移動・攻撃予約・パターン選択を統括 |
| `BossLifeCycle.cs` | 依存関係の構築・初期化・Activate/Deactivate管理 |
| `BossAttackReservationUsecase.cs` | 音楽同期の攻撃予約ユースケース（ボス専用） |
| `BossAttackPattern.cs` | 攻撃定義・タイミング・Controllerをセットで保持するVO |
| `BossAttackEntry.cs` | Inspectorから攻撃種別・タイミングを設定するデータ入力用 |
| `BossTestInitializer.cs` | ボスのテスト用初期化クラス |
| `EnemyTripleShotAttackController.cs` | 三方向同時攻撃を実行するController（特殊攻撃） |
| `EnemyTripleShotAttackControllerGenerator.cs`  | TripleShotControllerを生成するFactory |

#### 新規作成（BehaviorGraphNode）
| ファイル | 役割 |
|---|---|
| `BossAIController.cs` | ボス行動の中核。移動・攻撃予約・パターン選択を統括 |
| `BossLifeCycle.cs` | 依存関係の構築・初期化・Activate/Deactivate管理 |
| `BossAttackReservationUsecase.cs` | 音楽同期の攻撃予約ユースケース（ボス専用） |
| `BossAttackPattern.cs` | 攻撃定義・タイミング・Controllerをセットで保持するVO |
| `BossAttackEntry.cs` | Inspectorから攻撃種別・タイミングを設定するデータ入力用 |
| `BossTestInitializer.cs` | ボスのテスト用初期化クラス |
| `EnemyTripleShotAttackController.cs` | 三方向同時攻撃を実行するController（特殊攻撃） |
| `EnemyTripleShotAttackControllerGenerator.cs` | TripleShotControllerを生成するFactory |

#### 新規作成（View）
| ファイル | 役割 |
|---|---|
| `BossMoveView.cs` | NavMeshAgentを通じてボスの移動を毎フレーム更新するView |

### 実装されたコンポーネント説明

#### BossLifeCycle
`MonoBehaviour` かつ `IGameplayControllable` を実装するボスのルートクラス。  
Inspector から攻撃エントリ（種別・タイミング）を配列で設定し、`Initialize()` 時に攻撃パターンを組み立てて `BossAIController` へ渡す。
```Plain Text
依存関係の組み立て順：
1. RaycastDetectService（射線判定）
2. AttackPositionSearchService（移動目標探索）
3. EnemyMoveUsecase / EnemyAttackUsecase
4. BossAttackReservationUsecase（音楽同期予約）
5. EnemyBattleState（AI判定用・パターンごとに個別生成）
6. BossAttackPattern[] → BossAIController
7. 各Facade・View の初期化
```
---

#### BossAIController
ボスの「思考」を担うPOCO。`MonoBehaviour` に依存しない純粋なC#クラス。
- `ReserveAttack()` — ランダムにパターンを選び、音楽同期で攻撃を予約する
- `GetMoveInstruction()` — 移動意思を評価し、射程内外の状態を `EnemyBattleState` に反映する
- `CancelAttack()` — 硬直時など攻撃をキャンセルする
- イベント: `OnAttackReserved` / `OnAttack` / `On2BeatBefore` / `On1BeatBefore`
> 攻撃パターン選択ロジックは `SelectPattern()` に集約。現状はランダム。HPフェーズ制・順番制への切り替えはここのみ修正すればよい。
---

#### BossAttackReservationUsecase
`IMusicActionScheduler` を直接使い、任意の拍子・拍目に攻撃をスケジュールするユースケース。
- 攻撃の **2拍前・1拍前** も自動でスケジュールされ、演出・予兆に利用可能
- 前小節に巻き込む補正ロジック（拍が1未満になった場合に `BarFlag - 1` へ繰り上げ）を内包

#### 攻撃パターン構造
```Plain Text
BossAttackEntry（Inspector設定）
  ├── Kind（Infantry / Artillery / TripleShot）
  ├── AttackIndex（CharacterDataから攻撃定義を引くIndex）
  └── TimingData（BarFlag / TimeSignature / TargetBeat）
         ↓ BossLifeCycle.Initialize() で組み立て
BossAttackPattern（VO）
  ├── Definition（AttackDefinition）
  ├── Timing（EnemyMusicSpec）
  └── Controller（IEnemyAttackController）
```
攻撃種別と Controller の対応：
| Kind | Controller |
|---|---|
| `Infantry` | `EnemyInfantryAttackController` |
| `Artillery` | `EnemyArtilleryAttackController` |
| `TripleShot` | `EnemyTripleShotAttackController`（今回新規） |

#### TripleShot（三方向攻撃）
正面・右30度・左30度の三方向に `EnemyAttackUsecase.ExecuteAttack` を呼び出す特殊攻撃。
> **TODO:** 現状の `EnemyRaycastDetectView` は単一方向前提のため、各方向ごとの射線判定・警告ラインへの対応が別途必要。
---

#### Behavior Graph ノード
Behavior Graph のブラックボードに Facade を登録し、各ノードが Facade 経由でボスを操作する構成。
[header_4] Actionノード
| ノード | Blackboard変数 | 処理 |
|---|---|---|
| `BossMoveToAttackAction` | Movement, State | 攻撃可能位置へ移動。射程内かつ射線クリアで Success |
| `BossStopMovingAction` | Movement | 移動停止 |
| `BossAttackTargetAction` | Battle | 攻撃予約を発行 |
| `BossGetStunnedAction` | State | 硬直処理 |
[header_4] Conditionノード
| ノード | 判定内容 |
|---|---|
| `BossIsTargetInAttackRangeCondition` | ターゲットが射程内か |
| `BossIsAimSightClearCondition` | 射線に遮蔽物がないか |
| `BossIsAttackingCondition` | 攻撃予約中か |
| `BossIsStunnedCondition` | 硬直中か |

### 使用例

#### Inspector での攻撃パターン設定
`BossLifeCycle` の `_attackEntries` 配列に攻撃パターンを設定する。
```Plain Text
_attackEntries[0]
  Kind        : Infantry
  AttackIndex : 0（CharacterDataの0番攻撃定義）
  TimingData  : BarFlag=1, TimeSignature=4, TargetBeat=3   // 4拍子3拍目

_attackEntries[1]
  Kind        : TripleShot
  AttackIndex : 2（CharacterDataの2番攻撃定義）
  TimingData  : BarFlag=0, TimeSignature=2, TargetBeat=1   // 2拍子1拍目
```

#### 攻撃パターンの拡張
新しい攻撃種別を追加する場合：
1. `BossAttackKind` に新しいenumを追加
2. `IEnemyAttackController` を実装した新Controllerを作成
3. 対応する `IEnemyAttackControllerGenerator` を作成
4. `BossLifeCycle` の `generators` ディクショナリに登録
攻撃選択ロジックの変更
`BossAIController.SelectPattern()` を修正する。
```C#
// 現状: ランダム選択
private BossAttackPattern SelectPattern()
{
    int index = UnityEngine.Random.Range(0, _patterns.Count);
    return _patterns[index];
}

// HPフェーズ制にしたい場合はここのロジックを差し替えるだけでよい
```
---
---

### 既知のTODO / 制限事項
- `EnemyTripleShotAttackController` の三方向射線判定が未実装（現状は正面方向のみで判定）
- 攻撃選択ロジックはランダムのみ。HPフェーズ制・順番制は `SelectPattern()` の差し替えで対応可能
- `BossGetStunnedAction` のスタン演出は未実装
- 今のままのInGameCompositon.csだとBossがターゲットされない