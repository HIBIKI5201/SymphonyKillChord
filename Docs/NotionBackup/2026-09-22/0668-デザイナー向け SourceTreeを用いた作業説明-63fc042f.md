# デザイナー向け SourceTreeを用いた作業説明

- id: 33d7c2c6-cc02-801c-898a-c32163fc042f
- path: Symphony Kill Chord / システム概要 / デザイナー向け SourceTreeを用いた作業説明
- last_edited: 2026-04-09T19:10:19.199Z

- 概要: デザイナー向けのSourceTree操作方法と作業内容を説明する。
- カテゴリー: 開発用


## 説明
デザイナー向けのSourceTree操作方法と作業内容を説明する。
作業ブランチ名について、[[キャラモデラ― Unity導入会]] を参照する。
他のGit運用関連の情報は、[[GitHub 運用規定]] を参照する。

### 参考資料
‣
‣
‣

## 詳細

#### 【重要】作業時の大まかな流れ
  　資材をリモートとの同期を保つことが大事なため、作業時、下記の流れを遵守しましょう：
  1. 作業前に、feature/designer/masterからプルする
  2. 更新があった場合、一回プッシュする
  3. 担当作業を行う
  4. コミットの前に、feature/designer/masterからプルする
  5. 更新があった場合、一回プッシュする
  6. コミットを行う
  7. 自分のブランチへプッシュする
  8. PR(Pull request)を作る
  　エラーなどが発生した時、後述の「エラーが発生した場合」の通り、他のメンバーに連絡しましょう。

#### 【操作手順】リモートからチェックアウト
  リモートのブランチをローカルに持ってくる操作です。
  左側メニューの「リモート」を展開し、origin/feature/designer/masterを右クリックして、「origin/feature/designer/masterをチェックアウト…」をクリックする。
  [image: image.png] attachment:c2b0c13d-e089-4bf8-b25e-9d4b1676b391:image.png
  表示されたチェックアウト画面で「OK」をクリックする。変更は不要。
  [image: image.png] attachment:3aa79b77-1a92-45bc-b006-72216e8d79d5:image.png
  チェックアウトしたら、自動的にローカルのfeature/designer/masterを追跡中（太字で表示）状態になる。
  [image: image.png] attachment:505c4bc5-10fb-41a4-93b3-58679af0a6f5:image.png

#### 【操作手順】ブランチを作る
  　最初に、`feature/designer/master`追跡中状態であることを確認する。
　そうでない場合、`feature/designer/master`をダブルクリックして追跡ブランチを切り替える。
  [image: image.png] attachment:0cc2fa75-eee8-4e3a-82c0-c8cc261c143a:image.png
  　問題なければ、ブランチボタンをクリックする。
  [image: image.png] attachment:ebb06a16-4023-4d04-8ee6-979bb5426427:image.png
  　ブランチ画面の「新規ブランチ」に、自分のブランチ名を入力して、ブランチ作成ボタンを押す。
※「現在ブランチ」に表示されているブランチは間違いないか確認しておきましょう。
  [image: image.png] attachment:9a6f87e9-2d01-4aad-b458-bc03e5494e80:image.png
  　ブランチを作成した後、自動的に追跡中状態になる。
  [image: image.png] attachment:35a824ec-a4ce-4525-99fd-3c58c5b12fb4:image.png

#### 【操作手順】プルを行う
  　デザイナーの方は、基本的に`feature/designer/master`ブランチから最新の資材をプルする。
  　自分のブランチを追跡中状態にして、プルボタンを押す。
  [image: image.png] attachment:bacb96bc-7664-48ee-8a95-14f73e5877a7:image.png
  [image: image.png] attachment:06c42a02-d854-4c11-aff4-3a0099996d71:image.png
  　プル画面の「プルするリモートブランチ」で`feature/designer/master`を選択して、プルボタンを押下する。
  [image: image.png] attachment:0854eeb6-a8cd-49c1-a221-217cab74f174:image.png
  　プルして、更新があった場合、プッシュボタンに数字が表示される。この場合、変更をリモートにマージすることが必要なので、一回プッシュを行う必要がある。
  [image: image.png] attachment:58ca43a7-c6d3-489e-b1e4-20bc6de6053e:image.png

#### 【操作手順】コミット
  　作業で変更した資材は、SourceTreeの「作業ツリーファイル」に表示される。ここの内容は一定時間で自動更新されるが、F5で手動更新もできる。
　なお、下記画像と違う画面が表示されている場合、コミットボタンか、「ワークスペース」の「ファイルステータス」をクリックすると良い。
  　「作業ツリーのファイル」に表示されるファイルに付くアイコンは、ローカルでどんな変更を加わったかを示す：
  - 黄色の鉛筆アイコンは「リモートに既存するファイルを変更した」
  - 青色のはてなアイコンは「リモートに存在していないファイル」
  - 赤色のマイナスアイコンは「リモートに既存するファイルを削除した」
  [image: image.png] attachment:601331e1-b473-4020-84d0-bbbbc1a50fde:image.png
  　作業ツリーのファイルにて、コミット**対象となるファイルだけ**選択して、追加する。
　追加の方法は、ファイル選択後、「選択をインデックスに追加」ボタンか、右クリックして「追加」かどちらでも良い。
　1件だけ追加する場合、ファイルリストの「+」ボタンも追加できる。
  **注意：**作業で新規ファイルを作成した場合、Unityは自動的にそのファイルに関わる情報を格納する「.meta」ファイルも作成される。この「.meta」ファイルも、コミット対象となるので、忘れずに追加しておきましょう。
  [image: image.png] attachment:dfa97f68-d6ec-485a-b1b2-8d254d3d8257:image.png
  追加されたファイルは上の「Indexにステージしたファイル」に表示される。
  [image: image.png] attachment:4dcfd0e3-5e3b-437e-a6b3-415091c87003:image.png
  追加後、下のテキスト枠にコミット内容が分かるようなメッセージを入力し、コミットボタンを押す。
  [image: image.png] attachment:f9e98dbc-7fd1-4c08-822b-15acacc4a675:image.png
  コミット後、履歴にて自分のコミットが表示される。
が、これではリモートに反映されることにならないので、続けてプッシュを行う必要がある。
  [image: image.png] attachment:e9955092-2278-4287-aefc-01e007ae6d01:image.png

#### 【操作手順】プッシュ
  　コミット後、変更をリモートに反映するために、プッシュを行うことが必要。
  　プッシュボタンを押してプッシュを行う。
  [image: image.png] attachment:48b5b415-33b9-48d1-91ba-5aed43e474d0:image.png
  　プッシュ画面の「対象」にて、自分のブランチをチェックして、「リモートブランチ」にてプッシュ先を選択して、プッシュボタンを押す。
※通常、**自分のブランチのみ**プッシュすれば良い。プッシュする前に、余分なチェックがないか、ブランチ名に間違いがないか、確認しておきましょう。
  [image: image.png] attachment:6e8d57e3-c597-4ae4-bc69-90a8aaef67ff:image.png

#### 【操作手順】PR(Pull request)を作る
  　プッシュ後、Githubのサイトを開いて、プルリクエストを作成する。
　SourceTree画面のリモートボタンでもリポジトリのGithubページに移動できる。
  [image: image.png] attachment:34e7916d-aa27-42a5-bc71-1d182687416a:image.png
  　リポジトリ画面の「Pull requests」をクリックする。
※プッシュ後の一定時間内なら、リポジトリ画面に自分のプッシュ情報が表示される。そこの「Compare & pull request」ボタンからでも、PRを作ることができる。
  [image: image.png] attachment:ebce4204-1afd-4517-bb9a-9f537d6961bf:image.png
  Pull requests画面にて、New pull requestボタンを押す。
  [image: image.png] attachment:b2cdc98b-0767-4e12-ba78-79feced3a311:image.png
  　マージ先とマージ元を選択して、Create pull requestボタンを押す。
　**選択が間違わないように**注意しよう。
  [image: image.png] attachment:8c9d461e-c1dd-4f92-b89d-872dff864f59:image.png
  　タイトルと説明文に適宜な内容を記載して、Create pull requestを押す。
  [image: image.png] attachment:43702a7a-28f2-4b25-86c2-f69b6cc58aee:image.png
  　PRを作ったら、リードに連絡してマージを依頼しよう。

#### エラーが発生した場合
  　pull、push、ブランチ切替などの時にエラーが発生した場合、ローカルの変更と他メンバーの変更が衝突した可能性が高いので、表示されているエラー画面を閉じず、**【エラーメッセージの内容】**と**【自分が行った操作】**を伝えて、メンバーに連絡ましょう。
  [image: image.png] attachment:e9ae9296-1f5d-4afc-afe6-26cc7b7a8f5c:image.png
  
  
