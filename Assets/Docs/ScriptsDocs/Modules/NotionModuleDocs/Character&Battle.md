
---

# 📌 基本情報
インゲーム中のキャラクターパラメータ管理、戦闘時の攻撃力・防御力・クリティカル計算、ダメージパイプライン、およびバフ（ステータス効果）システムを司るモジュールです。
| 項目 | 内容 |
| --- | --- |
| **モジュール名** | Character & Battle |
| **カテゴリ** | InGame / Core |
| **アーキテクチャ** | クリーンアーキテクチャ (Domain, Application, Adaptor, View) |

---

## 🏗️ クラス（レイヤー構成）

| クラス名 | レイヤー | 役割・機能 |
| --- | --- | --- |
| **`CharacterEntity`** | Domain | キャラクター（プレイヤー/敵共通）の基底 Entity。`IAttacker` / `IDefender` を実装し HP や攻撃力などのコアデータを保持 |
| **`HealthEntity`** | Domain | HP 変化の状態管理クラス。`OnHealthChanged` イベントを発行して変化を通知 |
| **`Health`** | Domain | HP 値を表す readonly struct（ValueObject） |
| **`AttackPower`** | Domain | 攻撃力を表す readonly struct（ValueObject） |
| **`CriticalChance`** | Domain | クリティカル確率を表す readonly struct（ValueObject） |
| **`ActionParams`** | Domain | ダメージ計算に必要なパラメータをまとめた readonly struct |
| **`AttackDefinition`** | Domain | 攻撃定義クラス。`IAttackPipeline` を保持し、ステップの構成を管理 |
| **`AttackResult`** | Domain | 攻撃計算の結果（最終ダメージ・クリティカル有無）を保持する readonly struct |
| **`AttackStepContext`** | Domain | 各パイプラインステップに渡されるコンテキスト（攻撃者/防御者/定義/isJustHit等）の readonly ref struct |
| **`IAttackPipeline`** | Domain | ダメージ計算ステップのパイプラインを定義するインターフェース |
| **`AttackCalculator`** | Application | `IAttackPipeline.Execute` を呼び出し `AttackResult` を返すピュア静的クラス |
| **`AttackExecutor`** | Application | `AttackCalculator` を通じて計算し、`IDefender.TakeDamage` でダメージを適用するピュア静的クラス |
| **`IAttackStep`** | Application | パイプラインの1ステップを表すインターフェース |
| **`ConfirmedDamage`** | Application | 最終ダメージを確定するパイプラインステップ（`IAttackStep` を実装） |
| **`CriticalStep`** | Application | クリティカル発生を抽選するパイプラインステップ（`IAttackStep` を実装） |
| **`BuffBase`** | Application | バフの基底クラス（`IBuff` を実装）。`DamageDownBuff` / `LifeStealBuff` がこれを継承 |
| **`DamageDownBuff`** | Application | 防御バフの適用ロジック（`BuffBase` を継承） |
| **`LifeStealBuff`** | Application | 与ダメージ回復バフの適用ロジック（`BuffBase` を継承） |
| **`PlayerAttackController`** | Adaptor | プレイヤーの攻撃アクションを制御する Adaptor。`AttackExecutor` を呼び出してダメージを適用し、`AttackResultPresenter` へ結果を送る |
| **`PlayerBattleState`** | Adaptor | プレイヤーの戦闘中のバフ・ステータスを保持する状態クラス |
| **`EnemyBattleState`** | Adaptor | 敵の戦闘中のバフ・ステータスを保持する状態クラス |
| **`AttackResultPresenter`** | Adaptor | 攻撃結果（ダメージ・クリティカル）を View に通知する Presenter |
| **`AttackResultDTO`** | Adaptor | ダメージ通知・クリティカル発生などを View へ伝えるための readonly struct DTO |
| **`PlayerHealthHudPresenter`** | Adaptor | `IDefender.OnHealthChanged` イベントを購読し、HP 変化を `IHealthHudViewModel` へ伝達する Presenter |
| **`EnemyHealthHudPresenter`** | Adaptor | 敵の HP 変化を `IHealthHudViewModel` へ伝達する Presenter |
| **`IHealthHudViewModel`** | Adaptor | HP バーの表示バインド用インターフェース |
| **`HealthHudDTO`** | Adaptor | HP バー更新用のデータを保持する readonly struct DTO |
| **`AttackResultView`** | View | 攻撃結果（ダメージ数値・クリティカルエフェクト等）を画面に表示する MonoBehaviour |
| **`AttackResultViewModel`** | View | 攻撃結果の表示状態を保持する ViewModel（`IAttackResultViewModel` を実装） |

---

## 🔗 モジュール間依存関係

このモジュールはダメージパイプラインの提供元として、Player・Enemy 両モジュールから依存されます（外部と接続のない内部レイヤーは省略）。

```mermaid
graph TD
    subgraph CharacterBattleModule [Character & Battle モジュール]
        CB_Domain["Domain\n(CharacterEntity, IAttackPipeline, ActionParams 等)"]
        CB_App["Application\n(AttackCalculator, AttackExecutor, CriticalStep 等)"]
        CB_Adaptor["Adaptor\n(PlayerAttackController, PlayerBattleState 等)"]
        CB_Domain --> CB_App
        CB_App --> CB_Adaptor
    end

    subgraph PlayerModule [Player モジュール]
        P_Adaptor["Adaptor\n(PlayerController 等)"]
    end

    subgraph EnemyModule [Enemy モジュール]
        E_Adaptor["Adaptor\n(EnemyAIController 等)"]
    end

    subgraph MusicModule [Music モジュール]
        M_Domain["Domain\n(BeatType)"]
        M_Adaptor["Adaptor\n(MusicSyncState)"]
    end

    P_Adaptor -->|"CharacterEntity, IAttackPipeline 等を使用"| CB_Domain
    P_Adaptor -->|"PlayerAttackController, AttackExecutor を利用"| CB_Adaptor
    E_Adaptor -->|"CharacterEntity, Health, AttackPower を参照"| CB_Domain
    E_Adaptor -->|"EnemyBattleState, AttackExecutor を利用"| CB_Adaptor
    CB_Adaptor -->|"BeatType によるジャスト判定評価"| M_Domain
    CB_Adaptor -->|"MusicSyncState から BPM 取得"| M_Adaptor
```

---

# 🔄 処理の流れ（シークエンス図）

主要な処理フローごとに分けて記述します。

### ① プレイヤー攻撃実行フロー（入力イベント時）
プレイヤーが攻撃ボタンを押した際、ターゲット取得・バフ適用・パイプライン計算・ダメージ反映までの一連の処理です。

```mermaid
sequenceDiagram
    autonumber
    actor Player as プレイヤー入力
    participant PAC as PlayerAttackController
    participant TSC as TargetSelectorController
    participant PBState as PlayerBattleState
    participant AExec as AttackExecutor
    participant ACalc as AttackCalculator
    participant Pipeline as IAttackPipeline
    participant ARP as AttackResultPresenter

    Player ->> PAC: 攻撃ボタン押下 (ExecuteAttack)
    PAC ->> TSC: 現在のロックオンターゲット取得 (TryGetCurrentTargetEntity)
    TSC -->> PAC: ターゲット Entity 返却
    PAC ->> PBState: ターゲットを戦闘ステートに設定 (ChangeTarget)
    PAC ->> AExec: 攻撃実行要求 (Execute: attackDefinition, attacker, defender, isJustHit, baseDamage)
    AExec ->> ACalc: ダメージ計算要求 (Calculate)
    ACalc ->> Pipeline: パイプライン実行 (Execute: AttackStepContext)
    Pipeline -->> ACalc: AttackResult 返却 (CriticalStep → DamageDownBuff → ConfirmedDamage)
    ACalc -->> AExec: AttackResult 返却
    AExec ->> AExec: defender.TakeDamage にダメージを適用
    AExec -->> PAC: AttackResult 返却
    PAC ->> ARP: 結果を Push (AttackResultPresenter.Push)
    ARP -->> ARP: AttackResultDTO に変換して View へ通知
```

### ② HP 変化とHUDへの反映フロー（ダメージ被弾時）
ダメージを受けた際に Entity の HP が変化し、HUD（HPバー）へ即時反映される処理フローです。

```mermaid
sequenceDiagram
    autonumber
    participant Defender as CharacterEntity (IDefender)
    participant HEntity as HealthEntity
    participant PHPresenter as PlayerHealthHudPresenter
    participant HHudVM as IHealthHudViewModel
    participant HHudView as HealthHudView

    Defender ->> HEntity: TakeDamage (ダメージ値)
    HEntity ->> HEntity: 現在 HP を更新
    HEntity -->> Defender: OnHealthChanged イベントを発火 (currentHp, maxHp, amountChanged)
    Defender -->> PHPresenter: OnHealthChanged イベントコールバック
    PHPresenter ->> HHudVM: UpdateHealth (HealthHudDTO)
    HHudVM ->> HHudView: ReactiveProperty 経由で HP バーを更新
```

### ③ バフ適用フロー（攻撃前後タイミング）
攻撃実行の前後でバフシステムが起動し、ダメージ増幅やライフスティールを処理する流れです。

```mermaid
sequenceDiagram
    autonumber
    participant PAC as PlayerAttackController
    participant BuffSys as BuffSystem (CharacterEntity)
    participant DABuff as DamageDownBuff / LifeStealBuff

    PAC ->> BuffSys: 攻撃前バフ実行 (Execute: Attack_Logic_Before)
    BuffSys ->> DABuff: 各バフのロジック実行
    DABuff -->> BuffSys: 変換後コンテキストを返却
    Note over PAC: ここで AttackExecutor によるコアダメージ計算を実施
    PAC ->> BuffSys: 攻撃後バフ実行 (Execute: Attack_Logic_After)
    BuffSys ->> DABuff: ライフスティール等の後処理バフを実行
    DABuff -->> PAC: 最終 BuffContext (AttackResult) を返却
```
