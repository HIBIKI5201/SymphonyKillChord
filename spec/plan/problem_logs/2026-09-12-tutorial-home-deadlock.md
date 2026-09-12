# チュートリアル後のホーム初期化失敗と操作不能

## 作業段階と範囲

- 2026-09-12、投稿者より「初期化失敗の線で調査と修正を進めて」と指示。
- 作業段階: 一度終了後、投稿者からの具体的な例外ログと修正方針の提示を受けて再開。BGM初期化順序を修正、実機検証待ち。
- branch: `agent/tutorial-home-initialization`。既存のQA変更を含めないよう、ローカル `develop` (`8f67a3361`) からプロジェクト内worktreeを作成した。
- 最新の自動確認の指示に従い、Unity起動、PlayMode、テスト、push、mergeは実施しない。
- Cc上の段階・branch登録は未更新。外部資料へのアクセスは禁止されているため、ここに実際の作業段階を記録する。

## 症状と確認した欠陥

チュートリアルステージ終了後、ホームのボタンが表示されず操作できなくなる。

## 終了時の確認

投稿者「PA24_八幡拓音」より「再発が確認されなかったため、終了していい」と指示されたため、追加調査・検証は行わず終了する。確認したビルド、修正適用の有無、再現手順は未提示であり、根本原因の確定や本修正の実機検証完了を意味するものではない。以下の未検証項目は履歴として残す。

実装修正はローカルcommit `dfa98cce9` に保存済み。push・PR作成・mergeは未実施。専用branchとworktreeを保持する。

## 初期化失敗の経路

`ScreenInitializer.Build` は画面を構築して全画面を非表示にし、`Ready` でホームを表示する。`InitializationCoordinator` は他モジュールも含めて各フェーズを完了してから次へ進む。初期化がfalseまたは例外で終わると、ホーム表示まで到達しない経路がある。

`OutGameSceneInitializer.Start` は失敗をログとシーン初期化結果へ通知するだけで、復帰操作を表示しなかった。また、例外で中断したフェーズにはモジュール名・フェーズ名の失敗ログがなく、未構築モジュールのShutdown例外が以降の後始末も中断し得た。

発生環境で最初に失敗したモジュールは未確定。本変更は初期化失敗後の操作不能に対する復帰経路と診断情報の追加であり、個別の初期化エラーが解消したとは判断していない。

## 再開後に特定・修正したBGM初期化不具合

投稿者が `C:\Users\takut\AppData\LocalLow\Sinfonia Studio\Symphony Kill Chord\Player-prev.log`（投稿内で「今日18:08、該当ビルドの実行分」と説明）に次の例外が2回あると報告した。ファイル自体はこのセッションでは開かず、投稿された抜粋と現行コードを照合した。

```text
R3 UnhandleException: System.NullReferenceException: Object reference not set to an instance of an object
  at KillChord.Runtime.View.Persistent.Music.MusicPlayer.ChangeBgm (System.String cueName) [0x00000]
```

`MusicPlayerInitializer.Build` は `Bind` → `Initialize` の順だった。`Bind` が `MusicViewModel.CueName` を購読すると、ReactivePropertyの現在値である空文字が即時通知される。`ChangeBgm` は空文字の判定より先に `_cri.cueName` を読むが、`_cri` は後続の `Initialize` で初めて設定されるため、この順序でNullReferenceExceptionが発生する。

- `Initialize` → `Bind` に変更し、即時通知前にCriAtomSourceを取得する。
- `ChangeBgm` と `StopBgm` の先頭にUnityオブジェクトのnullガードを追加する。
- 新規クラスやBGMコンテナの変更は不要。以前の復帰画面追加とは別commitに分ける。

このNREがホーム非表示を引き起こしたかは未確定。投稿者によると同ログにはCoordinatorのフェーズ失敗ログがない。R3の未処理例外ログがあることから、Buildの中断やMusicPlayerModuleContainerの未登録を直接推論することはできない。初期化失敗を成功扱いにしたり、BGMモジュールのfail-fastを外したりはしない。

確認範囲は呼び出し順・空Cueの停止経路・nullガード位置・差分の静的確認まで。Unityコンパイル、起動、テスト、発生ビルドの再検証は未実施。修正適用後に同じ起動経路で当該NREが消えること、BGMの開始・停止、チュートリアル帰還時のホーム操作を確認する必要がある。

## 修正

- 初期化失敗時は当該シーンのUIDocumentを無効にし、独立した復帰画面を表示する。
- 復帰画面はIMGUIを使用する。失敗したUXML、PanelSettings、Addressables、フェード、通常UIのフォーカスに依存しない。マウスとEnterで操作できる。
- ホームからはタイトルへ戻る。タイトル自身の初期化失敗時は再読み込みできる。
- 復帰は既存SceneTransitionUsecaseを通し、元のロードセッション終了前の再入と連打を防ぐ。復帰元のdestroyCancellationTokenは使わず、遷移先初期化とロード画面終了まで継続する。
- 同一シーン再読み込み用Usecaseは以前のreadinessを消して追跡し直す。InGame専用のKeepOpen処理は流用せず、通常のロードセッションを閉じる。
- 各フェーズのfalseに加え、例外にもモジュール名・フェーズ名を記録する。例外は握り潰さず再送出する。
- Shutdownはモジュール単位で例外を記録し、逆順の後始末を継続する。
- セーブデータやチュートリアル完了フラグは変更しない。

## 静的確認

- OutGameシーンの10個のAddressablesキーをアセットGUIDと照合した。
- ホームの4ボタン名と主要UI要素をUXMLと照合した。
- スキルノード48件のBind、定義、親ノード参照、Phase名を照合した。
- IMGUIモジュールがmanifestにあり、Titleがビルド対象シーンにあることを確認した。
- 既存ロード処理の失敗時にセッションが終了すること、再読み込み先の初期化待機、購読解除、初期化失敗時の後始末をコードレビューした。
- Unityコンパイル・実機描画・入力・復帰は未検証。このコピーには既存Editorの接続設定 `UserSettings/UnityMcpSettings.json` がない。

## 実機で残る確認

1. 発生環境のログで最初のエラーとモジュール・フェーズを確認し、個別原因を修正する。
2. 初回チュートリアルから正常にホームへ帰還し、4ボタンを操作する。
3. ResourceLoad / Build / Readyを失敗させ、復帰画面、タイトルへの帰還、連打防止を確認する。
4. タイトル初期化失敗時の再読み込み、および復帰先ロード失敗時の再操作を確認する。
5. 部分初期化後のShutdown例外でも他モジュールが終了することを確認する。

## 未読資料

`Docs/NotionSpecifications` は存在しない。プロジェクト外のCc共通ルール、session-followup、session-work-phase、task protocol、履歴メモリ、共通スキルはアクセス範囲制限により未読。
