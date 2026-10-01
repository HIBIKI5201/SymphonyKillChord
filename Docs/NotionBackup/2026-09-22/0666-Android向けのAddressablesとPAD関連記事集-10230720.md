# Android向けのAddressablesとPAD関連記事集

- id: 3337c2c6-cc02-80f2-9e57-fdb210230720
- path: Symphony Kill Chord / システム概要 / Android向けのAddressablesとPAD関連記事集
- last_edited: 2026-04-05T19:43:58.560Z

- 概要: Android向けのAddressablesとPADの関連記事をまとめる。
- カテゴリー: 開発用


## 説明
PADはPlay Asset Deliveryの略。

### 参考資料


## 詳細

#### 関連記事
  - ▶ **公式：**
    ‣
    ‣
    ‣
    ‣
    ‣
  - ▶ **個人：**
    ‣
    ‣
    ‣
    ‣
  - ▶ **AIの回答：**
    ‣

#### メモ書き
  [header_4] 【名詞】ベースモジュールとアセットパック
    - ▶ **ベースモジュール**
      最初のシーンの実行ファイル (Java とネイティブ)、プラグイン、スクリプト、アセットを含む。最初のシーンとは、ビルドインデックス が 0 のシーン。
    - ▶ **アセットパック**
      ベースモジュール以外のシーン、リソース、ストリーミングアセット などのことを指す。
  [header_4] 【設定項目】Build & Load Paths
    　AddressableのGroupを選択時、Inspectorに出る設定項目。
　PADを利用する場合、PADの内部処理で自動的に設定するので、手動設定は不要。
  [header_4] 【設定項目】Build App Bundle(Google Play)
    　ビルド設定 → プラットフォーム設定の項目。PADを利用するにはチェックが必須。
　これをチェックしてビルドするとapkではなくaab(Android App Bundle)が出力される。
  [header_4] 【設定項目】Split Application Binary
    　Player Settings → Publishing Settingsにあるの項目。PADを利用するにはチェックが必須。
  [header_4] Google Play Consoleについて
    　Google Playにゲームを配布する開発者向けの管理ツール。ビルドデータはここでアップロードする。
　アカウント作成するだけでも料金（$25）かかり、本人確認書類が必要。
  [header_4] AddressableのBuildについて
    　Addressablesのビルド結果に、「重複するAssetを検知した」的なメッセージが出ることがある。多分その場合、Group構成にはまだ改善する余地がある。
  [header_4] Delivery Typeについて（Install-Time、Fast-Followとか）
    ユーザー側の体感で説明していくと：
    - ▶ Install-Time
      　Google Playの「インストール」ボタンを押して、インストールを待つ間でダウンロードされる。「本体と一緒にダウンロードされる部分」だと認識しても良い。初期内容、ゲームの実行を最低限保証するリソースなどを入れることが多い。
    - ▶ Fast-Follow
      　インストールが完了した後、Google Playが裏で**自動的に**Fast Followのリソースをダウンロードし始める。このタイミングでゲームは起動できるが、開発者の実装次第、基本的なコンテンツが遊べるとか、ダウンロード進捗が確認できるとかの状況になる。
    - ▶ On-Demand
      　ゲーム内で何らかのコードでダウンロードをトリガーする部分。「ゲーム起動時の自動アップデート」「追加ダウンロードボタンを押す」「ガチャで新しいキャラを入手した」などの場合でダウンロードを起動する場合が多い。
  [header_4] PADの「得意分野じゃない」所
    　PADを使って配布されたゲームは、更新する度に、新しいビルドデータを丸ごとGoogle Playにアップロードしなければならない。今のソシャゲーのような、高い頻度で更新するゲームなら、PADは不向き。
　高い頻度で更新したりコンテンツ追加したりするゲームを作るなら、PAD以外のCDN（Contents Delivery Network）を構築してコンテンツを配布するのが一般的らしい。
  [header_4] Android Studioでのビルドデータ分析
    ‣