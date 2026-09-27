# Stage 8: Vitia UX・オンボーディングレビュー

**ステータス: partial（部分完了）。** Vitia commit `3fa33c9e5e7913c35a2a085dbbe49631cdee9eae`、source manifest `spec/data/vitia-ux-source-manifest.json`、仕様35/35、静的コードを使用した。実機観察・ユーザテストは未実施である。

## ラベル中立性の確認

採点時は `[artifact-A]`、`[hybrid-action]` として製品名、ジャンルの評判、Vitiaラベルを無視した。名称を別名へ置換しても「時間入力と即時戦闘を同時に理解し、失敗から通常stageへ転移できるか」という診断は変わらないため rename check は pass。以下の数値は仕様証拠のcoverage heuristicであり、楽しさや売上を測らない。

## 証拠台帳

- **観測済み（仕様／静的コード）:** 一入力でattack、拍により武器が変化、patternでKill Chord、tutorial敵damage 0、4/8拍から開始、tutorial再プレイ、敗北Tips、retry、研究による恒久支援、pause/resume countdown、settings、controller/mobile方針。
- **推論:** tutorialの安全性と段階設計は学習を助ける可能性がある。adaptive BGMと視覚HUDが配線されれば、入力の予測可能性を高めうる。
- **未確認:** 初見の注視点、誤入力、time-to-first-Kill-Chord、音なしでの理解、prompt依存、通常stageでのtransfer、酔い、疲労、mobile操作、設定変更の効果。

## 行動発見の連鎖

| Link | 仕様上の意図 | 静的な問題 | 検証 |
|---|---|---|---|
| user goal | 敵を倒しmissionを達成 | mission HUD詳細が薄い | 開始5秒後に目標を説明できる率 |
| available action | move / attack / dodge / lock-on | platform mappingの完全性未確認 | input device別のunaided action success |
| signifier | music、rhythm UI、telegraph、radio cue | HUD pageが見出しのみ、audio依存 | mute/low-vision条件でcue検出率 |
| attempted action | 拍に合わせattackしpatternを作る | All/timeout不一致で期待と結果がずれる | first attempt、hesitation、confident error |
| feedback | weapon、note、skill、hit、combo | adaptive BGM未配線、hit cue受入条件なし | action→cause attribution、latency |
| recovery | enemy damage 0、retry、Tips、research | prompt fade/transfer criteriaなし | retry後改善率、help使用、resume time |

## チュートリアルと学習転移

チュートリアルは story → move → attack → 1/2/4/8拍 → Kill Chord → lock-on → defeat と多くの関係を一つのstageで扱う。安全な試行（enemy damage 0）と再プレイは強いが、最小の価値単位を「4拍attackを2回成功し、因果を説明できる」に切り、次に8拍、最後にpatternへ組み合わせるべきである。各stepは cue → safe attempt → action-specific feedback → 変形課題 → prompt fade の順にし、通常stage冒頭でunaided transferを測る。skip、control reference、captions、visual beat、振動代替は罰なしで提供する。

## 初期体験とピーク体験の連続性

| Dimension | First meaningful stage | Peak-value stage | Gap |
|---|---|---|---|
| Goal | 基本attackで敵撃破 | mission制約下でcommandを選択 | mission/commandの同時負荷を段階化 |
| Actions | move、attack、4/8 beat | dodge終端、3/6/1 beat、3-slot build | transfer introduction位置が未定義 |
| Decision | cadenceを変える | 敵・HP・build・phraseでpattern選択 | early敵にも意味ある選択を置く |
| Feedback | radio cue、rhythm UI | adaptive BGM、telegraph、combo/rank | BGM配線とmultimodal HUDが未完成 |
| Promise | 音に合わせると戦い方が変わる | 自作buildが音と戦術を変える | tutorial固定buildからownershipへの橋 |
| Failure/recovery | damage 0 | defeat Tips、retry、research | Tipsの診断性と改善を未測定 |

## 4レンズ監査

Deterministic output: `spec/data/vitia-game-experience-audit.json`。

- **Play promise — supported 0.582:** goal/action/rule/outcomeは仕様上つながる。feedback traceabilityとadaptive musicが弱点。
- **Challenge and learning — needs work 0.522:** 安全な試行と研究救済はあるが、later transfer証拠がなくprompt fadeも未定義。
- **Player agency/fairness — exploratory 0.434:** solo中心で強制的social pressureは見当たらないが、coverage 0.45・confidence 0.45で結論不可。
- **Repeat value — supported 0.594:** build、pattern、mission variationに再訪価値がある。endless、satiation、stopping cueは実機未確認。

Monetizationは価格、購入、広告、paid rewardの資料がなく **not-applicable**。不明をゼロ評価しない。

## アクセシビリティ、情報密度、疲労、回復

- beatを音だけに置かず、位相／次beat／成立noteを色だけに依存しない形で表示し、振動強度と簡略表示を選べるようにする。
- combat HUDは「今の目標」「次に入力可能なaction」「成立中pattern」「敵予告」をpriority順で制限する。情報数の一律上限ではなくtask successで調整する。
- camera shake、auto lock、brightness、text speed、caption、remap、hold/toggle、rhythm timing calibrationをglobal settingsとして整理する。
- pause復帰はcountdown中にbeat phaseとtargetを再提示し、最初の入力をAll扱いにするか仕様で決める。
- destructive reset、unsaved loadout、skill-tree resetには対象、結果、取消可能性を明示する。

## 改善案

| Proposal | Impact | Risk / cost | Validation | Guardrail |
|---|---|---|---|---|
| Beat/command HUD contractを先に確定 | core因果を可視化 | 画面密度増 | mute条件のaction attribution | 色・音単独に依存しない |
| Tutorialをcompetence gate＋fadeへ分割 | transfer改善 | 実装・計測追加 | normal stage unaided success | skip/replay/helpを常設 |
| All/timeoutをpause/被弾と統一 | recovery予測性 | balance変更 | resume/被弾後error rate | hidden leniencyにしない |
| 敗北Tipsを原因別に選択 | retry学習 | 誤診断 | tip後の同一error減少 | shame、課金誘導なし |
| Settingsをcombatから試聴・試操作 | calibration短縮 | state rollback複雑化 | adjust→resume time | cancelで完全復元 |

優先順は HUD contract → All/recovery → tutorial transfer → settings/accessibility → first/peak playtest。completion率だけでなく、unaided discovery、later transfer、recovery time、accessibility failureを併記する。

## 配信映え

**評価: 素材は強いが、視聴者へ因果を伝える実装証拠が不足している。** Cadenceによるweapon変化、note列の蓄積、Kill Chord成立、敵への結果が同じ画面内で連続すれば、短いclipでも「操作が音楽と戦術を変えた」と理解できる。反対に、HUD、hit cue、adaptive BGMが分断されたままでは、配信者本人には手応えがあっても視聴者には通常のTPSとして見える。

配信画面では次を独立した受入条件にする。

- 3秒以内に現在のcadence、選ばれたweapon、完成中patternを小画面でも判別できる。
- Kill Chord成立前後を、色だけでなく形、動き、caption、音で区別できる。
- 失敗時も「入力時機」「対象遮蔽」「pattern不一致」のどれかを配信者が説明できる。
- mute autoplayと音声圧縮後の双方で、Cadence→Weapon→Kill Chordの順序が崩れない。
- 配信者向け表示がプレイヤーの照準・敵予告・mission情報を塞がない。

推奨captureは、同一敵・同一cameraで4拍と8拍のweapon差を示し、pattern完了からKill Chord結果までを切らずに映す8～12秒である。検証指標はclip視聴後のrule comprehension、誤説明率、Kill Chord瞬間の視線到達、字幕条件差とする。視聴数やリアクション数だけを成功指標にしない。

## TGSでのゲームプレイ予想

**位置付け: 実機観察前の予測。** 約2分のステージと初見来場者を前提にすると、序盤は馴染みのある移動・照準・attackで参加しやすい一方、会場騒音下では拍の違い、All/timeout、pattern進行を見失う可能性が高い。最も強い瞬間は、入力速度でweaponが変わり、note列完成とKill Chord結果が観客にも同時に見える場面である。

予想されるプレイ推移:

1. **開始0～20秒:** 移動・attackは試せるが、mission目標とリズム入力の関係はHUD次第。スタッフ説明へ依存しやすい。
2. **20～60秒:** 4拍／8拍でweaponが変わることを理解できれば、本作固有の試行が始まる。変化が小さい場合は連打へ固定される。
3. **60～120秒:** patternとKill Chordへ注意を移せるかが分岐点。敵予告、照準、note列の情報競合が強いと、成立前に理由不明の失敗となる。
4. **終了時:** Kill Chordを一度でも自力で成立できれば記憶に残りやすい。未成立でも次の有効行動と再挑戦理由を示せれば、失敗を体験価値へ変えられる。

TGS向けの重点課題は、音なしでも読めるvisual beat、最初の一成功までの短縮、即時retry/reset、controller再接続、長時間連続稼働、スタッフ介入なしの目標理解である。観測項目はtime-to-first-attack、time-to-first-weapon-change、time-to-first-Kill-Chord、スタッフ介入回数、離脱地点、retry率、終了後のルール説明正答とする。

展示版では全システムを見せるより、「Cadenceがweaponを変える」「patternがKill Chordになる」「結果が明確に返る」の3点を確実に完遂させる。Adaptive BGMや成長要素が未完成なら、説明で補って完成済みと誤認させず、会場版の体験範囲を明示する。
