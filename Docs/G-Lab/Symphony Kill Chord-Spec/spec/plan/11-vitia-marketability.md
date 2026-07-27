# Stage 9: ラベル中立Vitia市場性分析

**ステータス: partial（探索的評価）。** Stage 8と同じVitia rootを再利用したが、UX結論を採点入力へ逆流させず、公開仕様に記載された体験・対象・証拠だけで独立評価した。store page、price、demo telemetry、creative testは未取得。

## 1. ラベル中立性の確認

製品名、`rhythm`、`TPS`、希少ジャンルという自己記述、Latin domain名を `[artifact-A]`、`[mechanism-X]` に置換して採点した。rename後も「音楽のphraseを戦闘選択に変える瞬間を、短い実演で証明できるか」が主課題で変わらないため pass。

## 2. 真実性台帳

- **Verified:** 一つのattack入力、cadence別weapon、pattern skill、classical×EDM方針、third-person combat、adaptive BGM設計、build/progression、target age 26–35という企画仮説。
- **Assumed:** 代表ユーザはactionの即応性と音楽patternの習得を両方求めうる。短い動画／体験版が説明文より適する可能性がある。
- **Unknown:** 市場規模、認知、実機品質、art/audio完成度、価格、platform/store、demo availability、wishlist/conversion、比較タイトルからのswitch理由。

## 3. 診断

- Objective: 最初の接触で「音に合わせる装飾」ではなく「cadenceがweaponとskillを変える戦闘」を理解させる。
- Bottleneck: 価値が時間的で、静止画やジャンル名だけではrule→action→outcomeを証明しづらい。現状のtrialability signalは0.4、proof strengthは0.45。
- Audience context: 26–35歳という記述はdesigner仮説であり、campaign segmentの実証ではない。年齢やnostalgiaを脆弱性推定に使わない。

## 4. 体験と収益化の監査

play promiseとrepeat valueには仕様上の支持があるが、challenge learningはneeds work。宣伝でadaptive musicや自在なcommand masteryを先行させる前に、実装配線とdemoで証明する。commercial model不明のためmonetizationは **not-applicable**。総額、購入、renewal、random rewardを推測しない。

## 5. Vitiaドメイン選択

Deterministic output: `spec/data/vitia-marketability-score.json`。

- Primary **Luxuria 0.570**: 音、cadence、weapon変化、クラシック×EDM、即時戦闘という感覚的・時間的実演が最も強い。
- Secondary **Gula 0.502**: build/pattern/missionの組合せが反復価値を示す。ただし強制習慣ではなく、新しい判断とmasteryを示す。
- Acedia 0.462: 説明負荷低減は重要だが、主価値ではなくactivation支援。
- Superbia 0.344: masteryは候補だが、実機skill proofが不足。
- Invidia/Avaritia: social comparison、価格・経済価値のcoverage不足で選ばない。
- Luxuria + Gula は mandatory compulsion audit。near miss、expiry、obligation、failure直後offerは使わない。

## 6. 補助メカニズム監査

最優先は **mental simulationではなく直接的rule demonstration**。8–12秒の同一sceneで「遅いcadence→shotgun」「速いcadence→SMG」「pattern完了→Kill Chord」をUIと音で対応させる。readiness gapはadaptive BGM未配線、HUD受入条件不足、実機captureなし。attentionは中間指標であり、理解・trial開始をprimary outcomeにする。

## 7. 戦略カード

- Mechanism: truthful sensory demonstration + meaningful variation
- Proposition hypothesis: 「撃つ速さそのものが、武器と必殺の譜面になる」
- Proof: 同一敵・同一cameraでcadence、note列、weapon、resultを連続表示
- Message: 「一つの攻撃で、リズムを組み替えて戦う」
- CTA: 「30秒で4拍と8拍を試す」—実際に短いdemoが提供できる場合のみ
- Channel: muted autoplayでも理解できるcaptioned short video、store capsuleから同じdemoへ接続
- Boundary: 未実装adaptive BGM、未検証performance、boss内容を完成済みとして見せない

## 8. 実験計画

- Control: art/style中心の15秒video
- Treatment: 同一sceneでcadence→weapon→Kill Chordの因果をcaption付きで示す15秒video
- Primary metric: 視聴後の三択rule comprehension正答率。その後にqualified demo start率を副指標とする。
- Guardrails: expectation mismatch、mute/字幕条件の理解差、negative feedback、demo開始後60秒以内離脱。
- Duration/stopping: segment事前固定、各cellが事前sample sizeへ達するか2週間。途中の年齢／脆弱性後付けtargetingは禁止。
- Disconfirming result: comprehensionが改善しない、またはdemo離脱／期待不一致が悪化したらpropositionとHUD proofを再設計する。

## 9. 倫理確認

fabricated rarity、nostalgia pressure、status shame、near-miss、variable paid reward、false countdownは使わない。字幕、visual beat、easy exitを保持し、conversionと同時にregret、expectation mismatch、accessibility failureを測る。
