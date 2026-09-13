# シナリオCSVの書き方

このページは、非プログラマー向けの最小仕様です。  
まずは `ScenarioAuthoring` に置く `.events.csv` だけ覚えれば大丈夫です。

## いちばん大事なこと

- 1行が1つの命令です
- 1列目は番号です
- 2列目は命令の種類です
- 3列目以降に、その命令に必要な内容を書きます
- 行の後ろにある不要な引数は、省略して構いません

## 保存場所

- `Assets/StreamingAssets/ScenarioAuthoring/ファイル名.events.csv`

例:

- `Assets/StreamingAssets/ScenarioAuthoring/ScenarioTest.events.csv`

## まずはこの形で書けばOK

```csv
1,Background,background
2,Layer,PortraitLeft,2
3,Portrait,Left,hello,-460,-120,1,true
4,Portrait,Right,hello,460,-120,1,true
5,Text,Hero,......Hello sekai?
6,Portrait,Left,good,-460,-120,1,true
7,Text,Tatuki,danger is coming
```

## 使う命令は5つだけ

### 1. 背景を変える

```csv
1,Background,background
```

書き方:

```csv
番号,Background,背景ID
```

### 2. 立ち絵の重なり順を決める

```csv
2,Layer,PortraitLeft,2
```

書き方:

```csv
番号,Layer,対象,順番
```

よく使う対象:

- `PortraitLeft`
- `PortraitCenter`
- `PortraitRight`
- `Background`
- `Text`

### 3. 立ち絵を出す・変える

```csv
3,Portrait,Left,hello,-460,-120,1,true
```

書き方:

```csv
番号,Portrait,位置,立ち絵ID,X,Y,拡大率,表示するか
```

意味:

- `位置`: `Left` / `Center` / `Right`
- `立ち絵ID`: 表示したい絵の名前
- `X`: 左右位置
- `Y`: 上下位置
- `拡大率`: 普通は `1`
- `表示するか`: 普通は `true`

最小形でも使えます。

```csv
3,Portrait,Left,hello
```

この場合は自動で次の値になります。

- `X = 0`
- `Y = 0`
- `拡大率 = 1`
- `表示するか = true`

### 4. 会話を出す

```csv
5,Text,Hero,......Hello sekai?
```

書き方:

```csv
番号,Text,話者名,セリフ
```

話者名を空欄にすると地の文になります。話者名欄だけが隠れ、セリフは表示されます。

### 5. フェードを入れる

```csv
8,Fade,0,1,0.5
```

書き方:

```csv
番号,Fade,開始値,終了値,秒数,対象,方法
```

`対象` と `方法` は省略できます。省略時は `Screen,Alpha` です。

`Alpha` の値は `0` が透明、`1` が不透明です。既定の `Screen,Alpha` では会話枠を残して背景と立ち絵の透明度を変えます。

画面を黒く暗転・明転させる場合は、`Black,Alpha` を指定します。

```csv
8,Fade,0,1,0.5,Black,Alpha
9,Fade,1,0,0.5,Black,Alpha
```

透明度を変える対象:

- `Screen`: 画面全体（会話枠を除く）
- `Background`: 背景
- `PortraitLeft` / `PortraitCenter` / `PortraitRight`: 各立ち絵
- `Text`: 会話枠、話者名、本文
- `Black`: 全画面黒オーバーレイ

立ち絵を輪郭と透明度を維持したまま黒くする場合は、対象の後ろへ `Black` を指定します。

```csv
9,Fade,0,1,0.5,PortraitLeft,Black
10,Fade,1,0,0.5,PortraitLeft,Black
```

`FadeTarget` の `Black` は全画面黒オーバーレイ、`FadeMode` の `Black` は立ち絵RGBの黒化です。立ち絵黒化の対象には `PortraitLeft` / `PortraitCenter` / `PortraitRight` だけを指定できます。

テキスト末尾のTriggerから立ち絵を黒くする場合は、開始値、終了値、秒数の後ろへ対象と方法を追加します。

```csv
20,Text,案内役,この本文の表示完了時に左の立ち絵が黒くなります
21,Trigger,20,AtTextEnd,,,Fade,0,1,0.5,PortraitLeft,Black
```

ヘッダー付きCSVでは通常Fade行に `FadeMode` 列を追加します。Triggerでは `OnTriggerArg4` が対象、`OnTriggerArg5` が方法です。

## 省略してよい書き方

後ろの引数は、省略できます。

たとえばこの2つはどちらも有効です。

```csv
3,Portrait,Left,hello,-460,-120,1,true
3,Portrait,Left,hello
```

ただし、途中を飛ばして後ろだけ書くときはカンマが必要です。

例:

```csv
3,Portrait,Left,hello,-460,,1,true
```

これは `Y` だけ空欄にして、その後ろの `拡大率` と `表示するか` を書いています。

## ルール

- 文字の間にカンマを入れたいときは `"..."` で囲みます
- `"` を文字として入れたいときは `""` と書きます
- 小数は `0.5` のように `.` を使います
- `true` と `false` が使えます
- 空行と `#` で始まる行は無視されます

## よくあるミス

- 行番号が重複している
- `Text` のセリフが空
- `Portrait` の位置が `Left` `Center` `Right` 以外になっている
- 数字を書く場所に文字を書いている

## コピペ用テンプレート

```csv
1,Background,background
2,Layer,PortraitLeft,2
3,Portrait,Left,hero
4,Text,Hero,こんにちは
5,Portrait,Right,friend
6,Text,Friend,やあ
```

## 表示拡張の確認用CSV

固定会話枠、話者名と本文の分離、地の文、立ち絵の黒化などをまとめて確認する専用データです。

- `Assets/StreamingAssets/ScenarioAuthoring/ScenarioDisplayExtensionTest.events.csv`
- 再生時のシナリオID: `ScenarioDisplayExtensionTest`

## もっとやりたいとき

この最小仕様で足りないものを追加したい場合は、次を広げます。

- `Animation`: アニメを再生する
- `Trigger`: セリフの途中や最後で別の命令を発火する
- ヘッダー付きCSV: 列名ありの詳細形式

必要になったら、その機能だけ別ページとして追加するのがおすすめです。
