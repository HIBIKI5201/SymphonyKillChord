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

### 5. フェードを入れる

```csv
8,Fade,0,1,0.5
```

書き方:

```csv
番号,Fade,開始値,終了値,秒数
```

例:

- `0,1,0.5` で 0.5 秒かけて暗くなる
- `1,0,0.5` で 0.5 秒かけて明るくなる

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
- `Text` なのに話者名かセリフが空
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

## もっとやりたいとき

この最小仕様で足りないものを追加したい場合は、次を広げます。

- `Animation`: アニメを再生する
- `Trigger`: セリフの途中や最後で別の命令を発火する
- ヘッダー付きCSV: 列名ありの詳細形式

必要になったら、その機能だけ別ページとして追加するのがおすすめです。
