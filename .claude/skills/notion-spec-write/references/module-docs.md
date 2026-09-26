# モジュール文書の運用

Notionの「システムリスト」データベースにあるモジュール文書（カメラ、プレイヤー、ミッション等）の
書き方と反映手順。

## 正本はリポジトリ側

| 場所 | 役割 |
| --- | --- |
| `Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/<Module>.md` | 正本。1モジュール1ファイルで完結させる |
| Notion「システムリスト」DBの各ページ | 公開先。正本から生成する |
| `Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/NotionDocsRules.txt` | 節構成のテンプレートと記述ルール |

Notionを直接編集せず、正本を直してから反映する。

## 節の構成

`NotionDocsRules.txt` のテンプレートに従う。加えて次を守る。

- **クラス一覧はトグル見出しにする。** 反映時に`## 🏗️ クラス {toggle="true"}`へ変換される。
  `### 🧩 Composition初期化情報`は畳まない（初期化順が見えなくなるため）。
- **処理フローは1フローにつき1ページの子ページにする。** 正本では`### ①…`として書き、
  反映時に切り出される。
- **「アーキテクチャ上の特徴・既知の課題」の節は作らない。**
  課題はIssueへ、設計上の特徴は概要・依存関係・拡張ポイント・処理フローの文脈で述べる。
- 文体は[writing-rules.md](./writing-rules.md)に従う。

## 反映手順

```bash
python scripts/notion/sync_module.py \
  "Assets/Docs/ScriptsDocs/Modules/NotionModuleDocs/<Module>.md" \
  "Docs/NotionSpecifications/Symphony Kill Chord/システム概要/システムリスト/<ページ名>.md" \
  "<作業ディレクトリ>"
```

スクリプトは pull → 処理フローの切り出し → 未作成の子ページを作成 → 既存の子ページを更新 →
本文を`push --whole`、の順に実行する。子ページはタイトルで突き合わせるため、
**フローの見出しを変えると別ページとして作られる**。改名したときは古いページをNotion上で削除する。

`--confirm`相当の送信をスクリプトが行うため、**実行前にユーザーの承認を得る**こと。

内部で使う`scripts/notion/split_module_doc.py`は、正本を「本文」と「フロー」へ分割し、
H1への背景色付与とクラス一覧のトグル化も行う。単体でも実行できる。

## 実装から書くときの調べ方

1. モジュールのフォルダを層ごとに列挙する（`Assets/Scripts/Runtime/<層>/<カテゴリ>/<モジュール>/`）。
2. 正本のクラス表と突き合わせ、増減したクラスを洗い出す。
3. 新しいクラスは`<summary>`のXMLドキュメントコメントから役割を取る。これが最も安い。
4. `Order`は`Initializer`の`public override int Order`を見る。文書に書く。
5. 依存の向きは、Initializerが`ServiceLocator.GetInstance<T>()`で何を取っているか、
   自分のContainerを誰が参照しているかで確認する。**コミットメッセージから推測して書かない。**
