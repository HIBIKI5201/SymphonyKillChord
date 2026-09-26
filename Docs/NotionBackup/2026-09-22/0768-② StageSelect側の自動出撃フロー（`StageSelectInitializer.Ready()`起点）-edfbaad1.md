# ② StageSelect側の自動出撃フロー（`StageSelectInitializer.Ready()`起点）

- id: 3d67c2c6-cc02-81e7-8605-c1c3edfbaad1
- path: Symphony Kill Chord / システム概要 / Tutorial / ② StageSelect側の自動出撃フロー（`StageSelectInitializer.Ready()`起点）
- last_edited: 2026-09-09T03:36:13.024Z

`StageSelectInitializer`（Order 110）は`Ready()`で自動遷移フローを起動する。予約済みの通常ノード遷移（`PendingNodeTransitionState`、チュートリアル専用ではない汎用機構）を優先消化した後、`TutorialPhase`に応じたチュートリアル区間の自動開始を判定する。現状のswitch文で処理されるのは`OpeningScenarioCompleted`のみであり、それ以外のフェーズでは`TryStartTutorialSegment`は何もせず`false`を返す。
```Mermaid
sequenceDiagram
    autonumber
    participant Loading as LoadingScreenController
    participant Init as StageSelectInitializer (Order 110)
    participant Pending as PendingNodeTransitionState
    participant Save as SaveData.Tutorial (TutorialData)
    participant Tree as StageTree
    participant Sortie as BattleSortieSelectionService / OutGameSortieController

    Note over Init: Ready() 呼び出し時
    Init ->> Loading: IsLoading を確認（ロード中ならLoadingCompleted購読後に再入）
    Init ->> Init: ExecuteAutomaticTutorialFlow()
    Init ->> Pending: TryExecutePendingNodeTransitionAfterReturnAsync()
    alt 予約済み通常遷移がある
        Pending -->> Init: 消化して終了
    else 予約済み遷移がない
        Init ->> Init: TryStartTutorialSegment()
        Init ->> Save: Tutorial.Phase を switch
        alt Phase == OpeningScenarioCompleted
            Init ->> Tree: TryGetTutorialNode() でチュートリアル戦闘ノードを取得
            Init ->> Sortie: TryPrepareBattleSortie(戦闘定義)
            Init ->> Sortie: RequestImmediateBattleSortie()
            Note over Init: 戦闘クリア後、CompleteBattle()の呼び出しは<br/>InGame/Sequence側（本ページ対象外）が担う
        else その他のPhase
            Note over Init: 何もしない（falseを返す）
        end
    end
```
> `TutorialData.CompleteOpeningScenario()`と`CompleteBattle()`の実際の呼び出し元（シナリオ再生完了時・戦闘クリア時のフック）は、それぞれScenario/Sequenceモジュール側に存在すると推測されるが、本タスクで確認した範囲のファイルには含まれていないため、本ページでは呼び出し先APIの存在のみを明記し、呼び出し元の詳細は対象外とする。