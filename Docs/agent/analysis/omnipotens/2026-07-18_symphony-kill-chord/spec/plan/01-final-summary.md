# Symphony Kill Chord Omnipotens 最終まとめ

## 最終判断

Symphony Kill Chordの中核である「入力間隔が武器を選び、ノーツ列がKill Chordを成立させ、編成が戦術と音楽を変える」という構造には、独自性と反復価値の根拠がある。一方、現時点で商品価値を証明する実機証拠は不足している。最優先はコンテンツ追加ではなく、Cadence→Weapon→Pattern Skill→Kill Chord→Rewardを、初見でも理解でき、失敗から回復でき、テストで再現できる一本のvertical sliceとして閉じることである。

実装順はbehavior-firstを採用する。All/timeout、line-of-sight、rewardのexactly-once確定、recoverable save、multimodal HUD/tutorial transferを先に固定し、Scene遷移の分割はその受入テストを支える範囲に留める。Adaptive BGMはイベント契約と仮表示を先行し、コアループ成立後に実音源へ接続する。

## 最優先の5課題

1. All note、1.5小節timeout、被弾・pause復帰を一つの回復契約へ統一する。
2. 遮蔽物を含むline-of-sightと、攻撃不成立理由のフィードバックを確定する。
3. result ID単位で報酬を一度だけ付与し、retry・crash・再起動でも重複させない。
4. versioned・atomic・recoverableなsaveと、破損・移行・reset確認を一体で設計する。
5. beat、weapon変化、Kill Chord、敵予告を音・視覚・振動で対応付け、通常ステージへの学習転移を測る。

## 8月末までの開発アプローチ

以下は2026年7月18日時点の静的解析から導いた推奨順であり、担当人数を前提にした工数見積ではない。

### 7月後半: ルール固定と回帰防止

- 6拍種を正本候補として、All/timeout、reward確定時点、result遷移先を決定する。
- GS-01、GS-02、GS-05/06、GS-07、GS-09のcharacterization testを先に置く。
- TGS版の対象scene、約2分のプレイ範囲、使用可能build、対応入力端末を固定する。
- Kill Chord成立時の視覚・音・物理フィードバックについて、HUD・combat・reward間のイベント契約を定義する。

### 8月前半: 計測可能なvertical slice

- Cadence→Weapon→Pattern Skill→Kill Chord→Result/Rewardを一本のプレイ経路として閉じる。
- 敗北Tips、retry、pause復帰、tutorial prompt fadeを実装し、初見・失敗・再挑戦を同じbuildで確認できるようにする。
- Adaptive BGMは実音源の完成を待たず、phrase選択イベントとUI仮表示で入力との因果を検証する。
- PC 60 FPS、Android 30 FPS、ロード3秒、15分安定性の計測手順を自動化する。

### 8月後半: TGS候補版の固定

- 初見者プレイテストで、開始5秒後の目標理解、最初のKill Chordまでの時間、mute条件の理解、retry後改善を測る。
- 配信・会場騒音を想定し、字幕、visual beat、enemy telegraph、Kill Chord成立表示を音なしでも読める状態にする。
- 2分デモの開始、終了、即時reset、controller再接続、save隔離、異常終了復帰を通しで確認する。
- 8月末に機能を固定し、それ以降はTGS blocker、可読性、性能、運用性を優先する。

## 9月中旬TGSまでの課題

### 9月上旬: 安定化と展示運用

- TGS対象端末で連続プレイ、熱、メモリ、入力切断、音声出力切替、scene reloadを検証する。
- 操作説明を短縮し、スタッフ説明なしでもattack、cadence差、Kill Chordの目的が伝わるか確認する。
- デモ終了後の自動reset、ログ採取、クラッシュ時の復帰、予備buildへの切替手順を用意する。
- 未完成のadaptive BGM、boss、成長要素を完成済みとして見せず、体験範囲と製品予定を明確に分ける。

### TGS直前: 体験と発信の整合

- 会場版、配信用capture、説明パネル、短尺動画で、同じCadence→Weapon→Kill Chordの因果を示す。
- 会場騒音、mute配信、小画面視聴でも成立するUIサイズとcaptionを最終確認する。
- 待機列を含むセッション時間、離脱地点、初回成功率、スタッフ介入回数を記録できるようにする。
- blocker以外の新機能を止め、既知問題、回避策、再起動不要の復旧手順を共有する。

## TGS時点の合格条件

- 初見者が説明なし、または最小説明で4拍／8拍の武器変化を試せる。
- 一回のセッション中にKill Chord成立とその効果を本人・観客の双方が識別できる。
- 失敗理由と次の行動が画面から分かり、retry後に同じ誤りが減る。
- 連続展示で進行不能、報酬重複、save破損、入力不能が発生しない。
- 配信映像だけを見た人が「撃つ速さが武器と必殺パターンを変える」と説明できる。

## 証拠境界

本まとめは公開仕様、commit 0cca5d536257fd5a2f7951f7d3a0756dfc0d5788の静的コード、Unity対応Anatomia、Ludus、Vitia、Di合議を統合した判断である。実機プレイ、来場者観察、販売データ、配信視聴データは未取得であり、面白さ、売上、TGSでの反応は予測として扱う。
