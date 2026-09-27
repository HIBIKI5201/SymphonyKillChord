# Stage 6: メカニクスと内部経済

**ステータス: partial（部分完了）。正式仕様と静的コードのみで、プレイテストとtelemetryは未実施。**

## 意図されたループ

1. 音楽、phrase、敵予告を読む。
2. 移動・camera・lock-onで標的と安全位置を選ぶ。
3. 攻撃／回避のcadenceでnote列を作り、武器とKill Chordを選び分ける。
4. attack/buff/debuffを解決し、waveとmissionを進める。
5. resultでmission、time、max combo、rank、tipを受け取り、retryまたは次のnodeへ進む。
6. rewardを研究／強化／loadoutに使い、より複雑なcommandとBGM phraseへ進む。

Ludusの `genre:rhythm` × `genre:action` × `genre:shooter` では、時間判定・即応性・命中feedbackを同時に閉じる必要がある。現状はbeat-selected attackとpattern skillは強いが、All recovery、line-of-sight、adaptive BGM、reward creditが欠け、loopの学習と成長が途中で切れる。

## 難易度と回復

仕様は初心者に拍外行動を許し、tutorial敵damageを0にし、序盤を4/8拍、後半を3/6/1拍にする。研究は操作が苦手でもstoryを完遂するための恒久的救済であり、単なるmeta economyではない。上級者には複雑なKill Chordとendless missionを置く。これは一つのdifficulty sliderではなく、入力自由度・pattern複雑度・成長量の三軸である。

## 経済台帳

| Resource | Source | Sink / meaning | Static assessment |
|---|---|---|---|
| Research points | mission/stage reward | tree path/node、parameter、slot | sink/saveあり、source crediting未発見 |
| Skill level points | stage reward | individual Kill Chord level | aggregate pointのみ、個別level schema不足 |
| Unlocked skills/nodes | tree purchase/tutorial grant | loadout variety、difficulty access | persistenceあり、effect delivery一部TODO |
| Equipped Kill Chords | grant/tree | command set、intended BGM phrases | combat使用あり、BGM arrangementなし |
| Stage rank / combo | battle evaluation | mastery feedback、reward候補 | C/B/A/S pathあり、reward式未定義 |

commercial price、課金、randomized paid rewardは仕様にないため monetization audit は not-applicable とする。無料／買い切り等を推測しない。

## 効果の大きい実施順序

rhythm source統一 → All/timeoutとline-of-sight → multimodal HUD → exactly-once rewardとsave migration → skill effect／slot rule → build-directed BGM → deterministic balance simulation → representative playtestとtelemetry、の順がよい。stage duration、unaided Kill Chord成立、retry理由、tip後改善、resource収支を観測する。
