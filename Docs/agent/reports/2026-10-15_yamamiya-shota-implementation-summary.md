# YamamiyaShota の実装実績まとめ

調査日: 2026-10-15

## 調査範囲と読み方

- ローカル `develop`（`a004571b9030ee71c6ffe11f027db71a7e67a953`）に含まれる、Author 名が `YamamiyaShota` のコミットを対象にした。
- マージを除く **547コミット**、Author 日付の範囲は **2026-04-22〜2026-09-16**。別途マージコミットは126件ある。
- 調査時点のローカル全参照を対象にした場合も、該当する非マージコミットは547件だった。リモート全履歴の最新状態をfetchして集計したものではない。
- 全対象コミットの件名を確認し、主要機能の代表差分・変更パスで内容を補った。以下は履歴から確認できる担当実績であり、現在の動作保証や、機能全体の単独開発を意味しない。
- 「新規実装」は追加コミットで確認できる範囲、「拡張・改修」は既存機能に対する変更を指す。画像の追加・適用は、画像そのものを制作した証拠とは区別した。

## 担当内容の要約

**アウトゲームのUI・画面遷移、ステージ選択から出撃、スキル編成と育成結果の戦闘への反映を中心に担当。加えて、音量設定とUI操作音、モバイル・ゲームパッド操作、画面外の敵方向表示を実装し、カメラのロックオン挙動やシナリオ表示・スキップ処理を改修した。**

## 1. アウトゲームUIと画面遷移の基盤（新規実装・継続改修）

- UI ToolkitのUXML・USSでホーム、作戦、改造、スキルツリー、設定画面の構成を作成。
- 画面ID、遷移ルール、現在の画面状態、表示・非表示・ホーム復帰の処理を実装。
- Controller・Presenter・View・リポジトリ・初期化処理を用意し、画面操作と表示を接続。
- フェードの待機、戻る操作、画面表示直前・表示完了の通知を整備。
- 画面比率に応じたスケーリング、キャラクター画像やボタン画像の適用を調整。

代表根拠: [画面表示ユースケース追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/b8626e8f1)、[画面Viewの基底クラス追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/960c1e417)、[フェード待機修正](https://github.com/HIBIKI5201/SymphonyKillChord/commit/a2aea295d)、[画面比率対応](https://github.com/HIBIKI5201/SymphonyKillChord/commit/2917f87ae)。

## 2. ステージ選択・進行管理・出撃（新規実装・拡張）

- ステージツリー、ノード、接続、ステージ状態、戦闘・シナリオそれぞれの詳細情報を定義。
- ステージ選択・詳細画面を作成し、ミッションとクリア状態を表示。
- ステージクリア後の進行と、接続線・ノードの解放アニメーションを実装。複数の解放演出を順番に再生するシーケンサーも追加。
- 選択したステージの種類に応じて、戦闘準備・戦闘・シナリオへ進む出撃処理を実装。
- 戦闘準備画面にキャラクター立ち絵、装備スキル、効果説明を表示。

代表根拠: [ステージツリー追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/2b2bac108)、[進行管理・接続線演出](https://github.com/HIBIKI5201/SymphonyKillChord/commit/ab938d238)、[出撃ユースケース追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/b1b47420b)、[演出シーケンサー追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/25b299e36)、[戦闘準備のスキル情報表示](https://github.com/HIBIKI5201/SymphonyKillChord/commit/9a4d6353d)。

## 3. 改造画面・スキル編成（新規実装・拡張）

- 入手済みスキル一覧、装備スロット、スキルの詳細表示と編成状態の管理を実装。
- ドラッグ＆ドロップによる装備変更、スロット拡張、クリックとドラッグの判別を実装・調整。
- 編集中の編成と保存済みの編成を分け、保存・破棄の確認画面を追加。
- 入手済みスキル・装備構成をセーブデータと接続し、アウトゲームで編成したスキルを戦闘へ反映。
- スキル名、種類、レベル、アイコン、効果説明と数値の表示を整備。
- 不正・削除済みデータや未所持スキルがあっても、後続スロットが勝手に前詰めされないよう修正。

代表根拠: [編成ユースケース追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/5457431be)、[ドラッグ＆ドロップ追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/bfde236a5)、[編成の戦闘反映](https://github.com/HIBIKI5201/SymphonyKillChord/commit/ddf2d5b60)、[未保存時の確認](https://github.com/HIBIKI5201/SymphonyKillChord/commit/fac3e0515)、[スロット維持の修正](https://github.com/HIBIKI5201/SymphonyKillChord/commit/a4c2a9360)。

## 4. スキルツリーと育成結果の連携（拡張）

- 解放済みノード・ポイント・取得スキルをセーブデータから読み込むように接続。
- ノード解放によるステータスボーナスの定義・集計を追加し、ステータス表示と戦闘開始時のプレイヤーデータへ反映。
- スキルツリーのリセット、確認ダイアログ、ノードと接続線の表示復元を実装。
- リセット後に装備スキルを再評価する処理を追加。
- 最も近い未解放ノードを初期フォーカスに使い、表示領域をその位置に合わせる処理を追加。

代表根拠: [セーブデータ接続](https://github.com/HIBIKI5201/SymphonyKillChord/commit/f8ff28342)、[ステータスボーナス集計](https://github.com/HIBIKI5201/SymphonyKillChord/commit/97702947d)、[戦闘データへの反映](https://github.com/HIBIKI5201/SymphonyKillChord/commit/1e2f23351)、[ツリーリセット](https://github.com/HIBIKI5201/SymphonyKillChord/commit/71e06abaf)、[初期フォーカスの表示調整](https://github.com/HIBIKI5201/SymphonyKillChord/commit/0abc9ece9)。

スキルツリー全体を一から実装したとは断定せず、確認できる上記の追加・改修を担当実績とする。

## 5. タイトル・設定・セーブ連携（新規実装・既存基盤改修）

- タイトル、メニュー、オプション、クレジット画面と遷移処理を作成。
- セーブ状態に応じた開始先の切り替え、チュートリアル進行データ、データリセットを追加。
- スキル関連データの保存・読み込み、セーブデータの集約クラスを整備。
- リセットとロードの競合を待機制御と世代番号で対処し、古いキャッシュの適用を防止。
- ロード中の入力無効化、タイトル開始時の多重操作やアンロード中断への対策を追加。

代表根拠: [タイトル初期化追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/0f14f0b72)、[開始先の切り替え](https://github.com/HIBIKI5201/SymphonyKillChord/commit/4ec9ff574)、[SaveData集約](https://github.com/HIBIKI5201/SymphonyKillChord/commit/c281e46da)、[ロード・リセット競合の修正](https://github.com/HIBIKI5201/SymphonyKillChord/commit/e8a185eaf)、[ロード中の操作無効化](https://github.com/HIBIKI5201/SymphonyKillChord/commit/01aeecb0b)。

セーブ基盤全体の新規開発ではなく、画面・育成データとの連携と既存処理の改修を含む実績として扱う。

## 6. 共通音量設定・BGM・UI操作音（新規実装・組み込み）

- BGM・効果音・ボイスの音量データ、設定サービス、リポジトリ、ViewModel、設定UIを作成。
- 音量の読み込みと変更要求の直列保存を実装し、セーブデータへ永続化。
- タイトル・アウトゲームのBGM通知とUI操作音の監視・再生を接続。
- UI操作の意味に対応する効果音設定とAddressables登録を追加。
- セーブデータリセット時の音量維持と、クリック成立時に操作SEを鳴らす修正を実施。

代表根拠: [音量設定サービス追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/d6bee3ce2)、[音量の永続化](https://github.com/HIBIKI5201/SymphonyKillChord/commit/7877e1e64)、[UI操作音イベント登録](https://github.com/HIBIKI5201/SymphonyKillChord/commit/c83ea1dca)、[シーンのBGM・SE接続](https://github.com/HIBIKI5201/SymphonyKillChord/commit/70d02d540)、[リセット時の音量維持](https://github.com/HIBIKI5201/SymphonyKillChord/commit/384f6fd99)。

## 7. モバイル・ゲームパッドの操作対応（新規実装・改修）

- 仮想スティック上のポインター軌跡からフリック方向を検出し、方向付き回避へ接続。
- フリック判定条件をScriptableObject化し、Addressables経由の取得に対応。
- ゲームパッドの決定操作とボタンクリック処理を共通の作動イベントに接続。
- 設定・タブ画面の階層ナビゲーションとスキル編成操作を改善。
- シナリオ操作のゲームパッド割り当てを追加。

代表根拠: [フリック検出追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/93b76b2c8)、[方向付き回避への接続](https://github.com/HIBIKI5201/SymphonyKillChord/commit/35a9b90c3)、[決定操作の共通化](https://github.com/HIBIKI5201/SymphonyKillChord/commit/dd15c5f67)、[階層ナビゲーション追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/e1ffb3604)、[スキル編成の操作改善](https://github.com/HIBIKI5201/SymphonyKillChord/commit/7a0666f7d)。

## 8. 画面外の敵方向表示（新規実装）

- 登録済みの敵から表示対象を選択し、マーカー用スロットに割り当てるPresenterを作成。
- プレイヤー周辺に画面外の敵方向を示す3Dマーカーを描画。
- ターゲットのBoundsを利用した表示判定、表示数・位置・距離・フェード設定を実装。
- Config・初期化処理・Addressablesへの登録を追加し、GC発生を抑える改修も実施。

代表根拠: [表示対象の選択](https://github.com/HIBIKI5201/SymphonyKillChord/commit/ad9793ddf)、[3Dマーカー描画](https://github.com/HIBIKI5201/SymphonyKillChord/commit/772047033)、[表示設定追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/912a3e476)、[GC抑制の改修](https://github.com/HIBIKI5201/SymphonyKillChord/commit/4dcb8b13f)。性能測定値は今回確認していない。

## 9. カメラ・ロックオン挙動（既存機能の改修）

- 手動ロックオンが移動・視点操作で解除される問題と、自動ロックオンの解除条件を修正。
- カメラ位置の1フレーム遅延、ターゲットからのずれ、マージン内で回転が更新されない問題を修正。
- プレイヤーモデルの中心を考慮した注視位置、軸反転設定を修正。
- 回転処理のFixedUpdateへの集約と、移動入力による水平回転開始の角度閾値廃止を実施。

代表根拠: [手動ロックオン解除の修正](https://github.com/HIBIKI5201/SymphonyKillChord/commit/4e5e8549d)、[1フレーム遅延の修正](https://github.com/HIBIKI5201/SymphonyKillChord/commit/dfe2546fd)、[注視位置の修正](https://github.com/HIBIKI5201/SymphonyKillChord/commit/71dbaf4b5)、[FixedUpdateへの集約](https://github.com/HIBIKI5201/SymphonyKillChord/commit/c4634f97a)、[回転閾値の廃止](https://github.com/HIBIKI5201/SymphonyKillChord/commit/68165a48a)。

## 10. シナリオ表示・演出・スキップ（既存機能の拡張）

- 固定会話枠、話者名と本文の分離、話者名が空欄の地の文表示を拡張。
- 背景・各立ち絵・会話枠・黒オーバーレイを対象にするフェードと、立ち絵の黒化を追加。
- CSV側から対象・方法を指定する形式とガイドを更新し、機能確認用CSVを追加。
- 再実行時の表示初期化、レイヤー、画像・立ち絵参照を調整。
- スキップ確認画面を追加。9月13日に導入した長押しスキップは9月16日に解除しているため、最終的な実績は「確認画面付きスキップへの改修」とする。
- シナリオ後のチュートリアル戦闘への出撃を修正。

代表根拠: [会話表示・フェード拡張](https://github.com/HIBIKI5201/SymphonyKillChord/commit/3743b15d8)、[確認用CSV追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/1428a4c45)、[スキップ確認追加](https://github.com/HIBIKI5201/SymphonyKillChord/commit/57c6f1b0f)、[長押し設定解除](https://github.com/HIBIKI5201/SymphonyKillChord/commit/88d794a9d)、[チュートリアル出撃修正](https://github.com/HIBIKI5201/SymphonyKillChord/commit/180d02f2c)。

## 実績紹介に使える文章

> Unity/C#によるゲーム開発で、アウトゲームのUI・画面遷移、ステージ選択と出撃、スキル編成機能を担当しました。UI Toolkitでの画面作成に加え、編成・育成データの保存と戦闘への反映、共通音量設定、BGM・UI操作音、モバイルのフリック回避、画面外の敵方向表示を実装しました。また、ゲームパッドのUI操作、カメラのロックオン挙動、シナリオの会話表示・フェード・スキップ処理を改善しました。

## 月別の履歴件数

| Author年月 | 非マージコミット数 |
| --- | ---: |
| 2026-04 | 50 |
| 2026-05 | 94 |
| 2026-06 | 151 |
| 2026-07 | 117 |
| 2026-08 | 109 |
| 2026-09 | 26 |
| 合計 | 547 |

コミットはクラス単位の追加、修正、アセット更新も含む。件数は機能数や貢献割合を表さない。

## 検証と限界

- `git log develop --author=YamamiyaShota --no-merges` で対象抽出。抽出されたAuthor名は `YamamiyaShota` のみであることを確認。
- `git log --all` と `develop` の対象件数を比較し、どちらも547件であることを確認。
- 件名による分類に加え、シナリオ拡張、スキルデータ連携、UIナビゲーション、フリック入力、敵方向表示、音量保存等の代表変更パス・差分を確認。
- 他のAuthor名、共同著者欄、別リポジトリ、Git管理外の作業は集計対象外。AI支援の有無や共同作業の分担はGit Author情報だけでは判定できない。
- 実行時の動作確認・Unityコンパイル・テストは未実施。今回は履歴調査とMarkdown資料の追加のみ。
