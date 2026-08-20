# Stage 5: 仕様・ドメイン・コード対応表

**ステータス: complete（静的仕様35/35について完了）。ランタイム配線はpartial（部分完了）。**

| Source rule | Domain | Implementation evidence | Wiring state |
|---|---|---|---|
| 6拍種、All、Just、1.5小節timeout | Rhythm Input and Kill Chord Resolution | `BeatType.cs` は1/2/3/4/6/8、`MusicSyncService` はOne fallback | All/timeoutは矛盾、12/16は仕様内矛盾 |
| リズムで武器／攻撃を選ぶ | Player Action and Combat Targeting | `PlayerAttackController`, `CharacterCombatSpec` | implemented |
| 障害物がなければ命中 | Player Action and Combat Targeting | `PlayerAttackController` にline-of-sight TODO | contradictory |
| 敵が拍に同期し攻撃予告 | Enemy Encounter and Stage Simulation / Guidance | scheduling、ray/range viewあり | partial; runtime可読性未検証 |
| Kill Chordのpattern＋action条件 | Rhythm Input / Combat State | `SkillDefinition`, `SkillCheckService`, execution controller | implemented |
| 装備スキルで2小節BGMを編成 | Musical Time and Adaptive Arrangement | initializerはserialized `_testCue` を再生 | specification-only |
| 初期2枠→条件で3枠、末尾note競合禁止 | Progression, Research and Loadout | initial count 2、拡張処理あり、競合validation未発見 | partial |
| node解放、parameter増加、reset | Progression, Research and Loadout | unlock/spend/saveあり、effect/status TODO | partial |
| 勝敗result、time/combo/rank/tips/retry | Mission Evaluation and Result / Guidance | DTO/view model/controllerに経路あり | implemented; navigation runtime未検証 |
| reward pointを一度だけ付与 | Mission Evaluation and Result / Progression | detail表示はあるがcrediting参照未発見 | specification-only |
| Home hubと画面遷移 | Narrative and Game Flow | screen/scene controllers、Unity views 74 | partial; serialized graph未解決 |
| 設定draftの決定／取消 | Persistence and Player Settings | settings UI/pathあり | partial;全項目・永続化未検証 |
| 5分類save、暗号化、reset | Persistence and Player Settings | stage/tree/equipmentあり、skill level/options/encryption不足 | contradictory |
| tutorial再プレイ、prompt fade、通常stageへの転移 | Guidance, Feedback and Recovery | tutorial pageとbeta guidanceあり | fade/transfer criteriaはspec gap |
| pause、resume countdown、loading progress | Guidance, Feedback and Recovery | 関連view/controllerを検出 | runtime behavior unknown |
| PC60/Android30、load≤3秒、15分安定 | 横断quality constraint | 自動performance gateなし | unknown/unverified |

仕様からコードへのexplicit linksは68 clauses中8 linksに留まり、792 spec-gap candidatesを生んでいる。これらは「実装がない」件数ではなく、命名・属性・テストIDを通じたtraceability不足を含む。優先要件には `GS-*` IDをtest名またはasset metadataへ接続する。
