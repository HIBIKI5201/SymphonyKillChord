# Stage 10: Di合議結果

**ステータス: complete。** 仕様起点10ドメイン、Unity対応Anatomia、Ludus、Vitiaの証拠を用いて合議し、実装・検証方針を確定した。参照セッションは abba676c-4aaa-4ede-a86d-2911a5f1a4ab。

## 最終判断

- **面白さ:** Cadence→Weapon→Pattern Skill→Kill Chordという操作と結果の連鎖には、面白さを成立させる設計上の根拠がある。
- **奥深さ:** 未証明である。5回・10回の反復で新しい判断が生まれるか、習熟が画面から分かるか、通常ステージへ学習が転移するかを実機で確認する必要がある。
- **市場性:** 条件付きである。リズム判定と三人称アクションの同時成功を、本作固有の即時フィードバックとして短時間で伝えられるかが差別化の中心になる。
- **優先方針:** behavior-first vertical sliceを採用する。architecture-first refactorとadaptive music firstは選ばない。

## 採用する実装束

Core vertical sliceを「All/timeout + line-of-sight + exactly-once reward + recoverable save + multimodal HUD/tutorial transfer」に固定する。

Kill Chord成立時の視覚・音・物理フィードバックは、単独の演出ではなく、weapon selection、pattern skill、reward creditの契約を同時に確認できる計測点として実装する。Scene遷移の分割はこの受入テストを支える範囲に留める。Adaptive BGMはphrase選択イベントと仮表示を先行し、core rule、reward、save、guidanceの成立後に実音源へ接続する。

## 反対論を踏まえた制約

- コアループが接続されていることと、習熟によって深まることは別である。仕様上の循環だけで奥深さを断定しない。
- 一要素だけを磨いても、reward、save、guidanceが切れていれば商品ループは成立しない。
- Vitiaのrepeat value 0.594とchallenge learning 0.522は仕様証拠のcoverage heuristicであり、実ユーザの再訪率やリテンションではない。
- 「売れる」は未検証仮説である。価格、競合、ストア訴求、実プレイ継続率、配信・TGS反応を取得するまで条件付き評価に留める。
- 合議内の期間表現は見積根拠を持たないため、納期として採用しない。

## 最初の受入テスト

1. GS-01: first/silence/hit/resume入力がAllとなり、command先頭以外では成立しない。
2. GS-02: targetとの間にobstacleがあるshotはdamageを与えず、feedbackが理由を示す。
3. GS-05/06: result ID単位でrewardが一度だけ付与され、retry、crash、reopenで二重付与されない。
4. GS-07: versioned saveがatomicに更新され、破損時は既存progressを保持して明示的に回復できる。
5. GS-09: mute条件でもbeat、enemy telegraph、Kill Chord成立が識別でき、tutorial後の通常stageでpromptなし成功を測れる。

ownerは実装着手時に各ドメイン担当へ割り当てる。担当情報がないため本解析では未割当とする。
