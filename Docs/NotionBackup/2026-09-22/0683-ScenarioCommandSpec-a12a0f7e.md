# ScenarioCommandSpec

- id: 3af7c2c6-cc02-8079-a505-e997a12a0f7e
- path: Symphony Kill Chord / システム概要 / シナリオコマンド使い方 / ScenarioCommandSpec
- last_edited: 2026-08-04T10:38:22.281Z


## シナリオコマンド仕様
シナリオ（線形の会話・カットシーン）を定義する CSV コマンドの仕様書です。
実装（`ScenarioRepository` のオーサリングCSVパーサ）から抽出しています。
- 配置場所: `Assets/StreamingAssets/ScenarioAuthoring/<シナリオID>.csv`
- StreamingAssets 上のため、**再ビルド不要**（Play を入れ直せば反映）。
---

### 1. 基本ルール
- **1行 = 1コマンド**。カンマ区切り。
- **1列目 = ステップ番号（Step）**、**2列目 = コマンド種別（Type）**、3列目以降がコマンド固有の引数。
- **Step は一意**であること。表示順は Step の昇順に整列される（連番でなくてよい）。Step は `Trigger` の親参照にも使う。
- 後ろの**省略可能な引数は省いてよい**。途中だけ空にする場合は**カンマを残す**（例: `3,Portrait,Left,hello,-460,,1,true`）。
- **空行**と `**#**`** で始まる行**は無視される。
- 値の中にカンマを入れたいときは `"..."` で囲む。`"` を文字として入れたいときは `""`。
- 小数は `.`（例 `0.5`）。真偽値は `true` / `false`（`0` / `1` も可）。

#### 進行（クリック待ち）
| コマンド | クリック待ち |
|---|---|
| `Text` | **あり**（表示後にプレイヤーの送り入力を待つ） |
| `Background` / `Animation` / `Fade` / `Portrait` / `Layer` | なし（自動で次へ） |
---

### 2. コマンド一覧

#### Text — 会話・地の文
```Plain Text
Step,Text,話者名,セリフ
```
| 引数 | 必須 | 説明 |
|---|---|---|
| 話者名 | 任意 | 空にすると話者なし（ナレーション）。空欄でも `Step,Text,,セリフ` とカンマは残す |
| セリフ | 必須 | 本文。1文字ずつ表示され、末尾でクリック待ち |

#### Background — 背景切り替え
```Plain Text
Step,Background,背景ID
```
| 引数 | 必須 | 説明 |
|---|---|---|
| 背景ID | 必須 | 背景カタログ（`BackgroundCatalogAsset`）に登録された ID |

#### Animation — アニメーション再生
```Plain Text
Step,Animation,アニメID
```
| 引数 | 必須 | 説明 |
|---|---|---|
| アニメID | 必須 | アニメカタログ（`AnimationCatalogAsset`）に登録された ID |

#### Portrait — 立ち絵の表示／変更
```Plain Text
Step,Portrait,位置,立ち絵ID,X,Y,拡大率,表示
```
| 引数 | 必須 | 既定値 | 説明 |
|---|---|---|---|
| 位置 | 必須 | — | `Left` / `Center` / `Right` |
| 立ち絵ID | 必須 | — | 立ち絵カタログ（`PortraitCatalogAsset`）に登録された ID |
| X | 任意 | `0` | 横位置 |
| Y | 任意 | `0` | 縦位置 |
| 拡大率 | 任意 | `1` | スケール |
| 表示 | 任意 | `true` | `true` で表示、`false` で非表示 |

#### Fade — フェード演出
```Plain Text
Step,Fade,開始値,終了値,秒数,対象
```
| 引数 | 必須 | 既定値 | 説明 |
|---|---|---|---|
| 開始値 | 必須 | — | 開始 alpha（0〜1） |
| 終了値 | 必須 | — | 終了 alpha（0〜1） |
| 秒数 | 必須 | — | フェードにかける時間（秒） |
| 対象 | 任意 | `Screen` | 下表参照 |
**対象（Fade Target）**
| 値 | 対象 |
| — | — |
| `Screen`（既定） | 画面全体（`Canvas` / `All` も同義） |
| `Background` | 背景のみ |
| `PortraitLeft` / `PortraitCenter` / `PortraitRight` | 各立ち絵 |
| `Text` | テキストボックス |
| `Black` | 演出用の全画面黒オーバーレイ（暗転/明転） |
補足:
- Fade は**秒数分だけ待ってから次へ進む**ため、連続する Fade は順番に再生される。
- `Screen` フェードは**テキストを対象から除外**する（テキストは常に残る）。また完全な 0 にはせず僅かに残す（描画カリング回避）。
- `Black` は**最前面**の黒オーバーレイ。テキストも覆う暗転になり、**全黒に達した時点で下のテキストは消去**される（明転時に前テキストが残らない）。
- `0,1,...`（開始0→終了1）で暗くなる／`1,0,...` で明るくなる。

#### Layer — 重なり順の変更
```Plain Text
Step,Layer,対象,順番
```
| 引数 | 必須 | 説明 |
|---|---|---|
| 対象 | 必須 | `Background` / `PortraitLeft` / `PortraitCenter` / `PortraitRight` / `Text` / `Canvas` |
| 順番 | 必須 | 兄弟内のインデックス（0〜子要素数-1にクランプ）。`Canvas` 指定時は Canvas の sortingOrder |
補足: 通常の重なり順は**設定アセット（**`**ScenarioSettingsAsset**`** の Layer リスト、背面→前面）**で定義され、要素生成のたびに適用される。`Layer` コマンドは手動の上書き。

#### Trigger — テキスト表示中の副次イベント発火
```Plain Text
Step,Trigger,親Step,発火条件,文字位置,キーワード,発火種別,引数1,引数2,引数3
```
指定した `Text` コマンド（親Step）の表示進行中に、条件を満たしたら別イベントを発火する。
| 引数 | 説明 |
|---|---|
| 親Step | 対象となる `Text` コマンドの Step（Text 以外は不可） |
| 発火条件 | `AtCharIndex` / `AtKeyword` / `AtSuffix` / `AtTextEnd` |
| 文字位置 | `AtCharIndex` のとき: 何文字目で発火するか |
| キーワード | `AtKeyword`（含む）/ `AtSuffix`（末尾一致）のとき: 判定文字列 |
| 発火種別 | 発火するイベント: `Fade` / `Background` / `Animation` / `Portrait` / `Layer` |
| 引数1〜3 | 発火種別ごとの引数（下表） |
**発火種別ごとの引数**
| 発火種別 | 引数1 | 引数2 | 引数3 |
| — | — | — | — |
| `Fade` | 開始値 | 終了値 | 秒数（対象は `Screen` 固定） |
| `Background` | 背景ID | — | — |
| `Animation` | アニメID | — | — |
| `Portrait` | 位置 | 立ち絵ID | X |
| `Layer` | 対象 | 順番 | — |
補足: `AtTextEnd` は本文の末尾で発火（文字位置・キーワード不要）。
---

### 3. 値の参照
| 種別 | 値 |
|---|---|
| 立ち絵の位置（PortraitSlot） | `Left` / `Center` / `Right` |
| Fade 対象 | `Screen` / `Background` / `PortraitLeft` / `PortraitCenter` / `PortraitRight` / `Text` / `Black` |
| Layer 対象 | `Background` / `PortraitLeft` / `PortraitCenter` / `PortraitRight` / `Text` / `Canvas` |
| Trigger 発火条件 | `AtCharIndex` / `AtKeyword` / `AtSuffix` / `AtTextEnd` |
| Trigger 発火種別 | `Fade` / `Background` / `Animation` / `Portrait` / `Layer` |
| 背景ID / アニメID / 立ち絵ID | 各カタログアセットに登録された ID 文字列 |
---

### 4. 記述例
```Plain Text
# 背景と立ち絵を出して会話
1,Background,Outdoor
2,Portrait,Left,face,-460,-120,1,true
3,Portrait,Center,body,0,-120,1,true
4,Portrait,Right,face,460,-120,1,true
5,Text,主人公,ここが約束の場所か。
6,Text,,（誰もいない……）           # 話者名を空にするとナレーション

# テキストだけをフェードアウト→フェードイン
7,Fade,1,0,1,Text
8,Fade,0,1,1,Text

# 黒フェードで暗転→明転（演出）
9,Text,,……行こう。
10,Fade,0,1,1,Black
11,Fade,1,0,1,Black

# 背景だけフェード（テキスト・立ち絵は残る）
12,Fade,1,0,1.5,Background
13,Text,主人公,景色が変わっていく。

# トリガー: 5文字目で背景を切り替え
14,Text,主人公,いま何かが動いた。
15,Trigger,14,AtCharIndex,5,,Background,Outdoor
```
---

### 5. 補足: ヘッダー付き（正規化）CSV
先頭の有効行が `Type,` で始まる場合は、**列名ベースのヘッダー付き形式**として解釈される（`Type` / `Step` / `Speaker` / `Text` / `FadeStart` / `FadeTarget` / `ParentStep` などの列名で指定）。列順に依存せず書ける詳細形式で、本仕様のオーサリング形式（位置ベース）と自動判別される。
---

### 関連ドキュメント
- `Assets/Scripts/ScenarioCsvGuide.md`: 非プログラマー向けの最小ガイド。
- `Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/Scenario.md`: モジュール設計ドキュメント。