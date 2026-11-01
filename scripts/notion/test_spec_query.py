"""仕様書の窓口（search / get）を、小さな仕様書ミラーで確かめる。"""
import contextlib
import io
import json
import tempfile
import unittest
from pathlib import Path

from build_spec_catalog import main as build_catalog
from spec_query import main

PLAYER_ID = "3127c2c6cc028082acb4e96f7532687d"
DODGE_ID = "33a7c2c6cc028053a4fbf138ffac7b74"
DEV_ID = "3a37c2c6cc0280848b04d4d46b4f8fca"
STATUS_ID = "3e67c2c6cc028193aa6ed325642cef76"
TERM_ID = "3a37c2c6cc0280a39c08de577408df35"


def spec_page(title, page_id, category, summary="", parent="", body=""):
    return f"""<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->
# {title}

[Notionで開く](https://app.notion.com/p/{page_id})

最終更新: 2026-10-30T06:52:00.0000000+00:00

## プロパティ

| プロパティ | 値 |
|---|---|
| カテゴリー | {category} |
| サマリー | {summary} |
| 親仕様 | {parent} |
| 適用範囲 |  |

---

{body}"""


DODGE_BODY = f"""<table_of_contents color="gray"/>
# 仕様概要
敵の攻撃を避ける。
## 関連ページ
<mention-page url="https://app.notion.com/p/{PLAYER_ID}"/>
# 詳細説明
回避中は無敵になる。
## 経緯
| 日付 | 内容 |
## 演出
残像を出す。
```
# コード内の見出しは節にしない
```
# 仕様意図
爽快感を与える。
## 却下・保留した案
<table header-row="true">
<tr>
<td>日付</td>
<td>案</td>
</tr>
<tr>
<td>2026-10-30</td>
<td>キーコンフィグ</td>
</tr>
</table>
[実装状況](%E5%9B%9E%E9%81%BF/%E5%AE%9F%E8%A3%85.md)
"""

STATUS_PAGE = f"""<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->
# 回避アクション

[Notionで開く](https://app.notion.com/p/{STATUS_ID})

## プロパティ

| プロパティ | 値 |
|---|---|
| カテゴリー | 仕様 |
| ステータス | 進行中 |
| 仕様 | 33a7c2c6-cc02-8053-a4fb-f138ffac7b74 |

---

- キーコンフィグは未実装である
"""

TERM_PAGE = f"""<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->
# 回避アクション

[Notionで開く](https://app.notion.com/p/{TERM_ID})

---

# 定義
敵の攻撃を避ける行動。
"""


def database(title):
    return f"<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->\n# {title}\n"


class SpecQueryTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.root = Path(self.directory.name)
        specs = self.root / "SKC" / "仕様概要" / "仕様リスト"
        status = self.root / "SKC" / "ステータスリスト" / "実装リスト"
        terms = self.root / "SKC" / "用語" / "用語リスト"
        for folder in (specs / "回避", status, terms):
            folder.mkdir(parents=True)
        (specs / "_database.md").write_text(database("仕様リスト"), encoding="utf-8")
        (status / "_database.md").write_text(database("実装リスト"), encoding="utf-8")
        (terms / "_database.md").write_text(database("用語リスト"), encoding="utf-8")
        (specs / "プレイヤー.md").write_text(
            spec_page("プレイヤー", PLAYER_ID, "バトルパート", "操作するキャラクター", body="# 仕様概要\nプレイヤーの説明。\n"),
            encoding="utf-8")
        (specs / "回避.md").write_text(
            spec_page("回避アクション", DODGE_ID, "バトルパート", "", "3127c2c6-cc02-8082-acb4-e96f7532687d", DODGE_BODY),
            encoding="utf-8")
        (specs / "回避" / "実装.md").write_text(
            "# 回避アクション 実装状況\n\n[Notionで開く](https://app.notion.com/p/3fb7c2c6cc0281fba598f659c3a93542)\n\n---\n\n古い置き場\n",
            encoding="utf-8")
        (specs / "旧回避.md").write_text(spec_page("旧回避メモ", DEV_ID, "開発用", "回避の古い案"), encoding="utf-8")
        (status / "回避アクション.md").write_text(STATUS_PAGE, encoding="utf-8")
        (terms / "回避アクション.md").write_text(TERM_PAGE, encoding="utf-8")

    def tearDown(self):
        self.directory.cleanup()

    def run_cli(self, *args):
        out, err = io.StringIO(), io.StringIO()
        with contextlib.redirect_stdout(out), contextlib.redirect_stderr(err):
            code = main(["--source", str(self.root), *args])
        return code, out.getvalue(), err.getvalue()

    def search(self, *args):
        code, out, _ = self.run_cli("--json", "search", *args)
        return code, json.loads(out)

    def get(self, *args):
        code, out, err = self.run_cli("--json", "get", *args)
        self.assertEqual(code, 0, err)
        return json.loads(out)

    def test_search_ranks_spec_first_and_hides_development_pages(self):
        code, result = self.search("回避")
        self.assertEqual(code, 0)
        titles = [(item["title"], item["database"]) for item in result["results"]]
        self.assertEqual(titles[0], ("回避アクション", "仕様リスト"))
        self.assertNotIn("旧回避メモ", [item["title"] for item in result["results"]])
        _, with_dev = self.search("回避", "--include-dev")
        self.assertIn("旧回避メモ", [item["title"] for item in with_dev["results"]])

    def test_search_filters_by_parent_category_and_database(self):
        _, by_parent = self.search("--parent", "プレイヤー")
        self.assertEqual([item["page_id"] for item in by_parent["results"]], [DODGE_ID])
        _, by_parent_id = self.search("--parent", PLAYER_ID)
        self.assertEqual([item["page_id"] for item in by_parent_id["results"]], [DODGE_ID])
        _, by_category = self.search("--category", "バトルパート")
        self.assertEqual({item["title"] for item in by_category["results"]}, {"プレイヤー", "回避アクション"})
        _, by_db = self.search("回避", "--db", "用語")
        self.assertEqual([item["page_id"] for item in by_db["results"]], [TERM_ID])

    def test_search_falls_back_to_body_text(self):
        code, result = self.search("残像")
        self.assertEqual((code, result["mode"]), (0, "本文"))
        self.assertEqual([item["page_id"] for item in result["results"]], [DODGE_ID])
        code, result = self.search("残像", "--no-fallback")
        self.assertEqual((code, result["total"]), (2, 0))

    def test_get_returns_spec_sections_only_by_default(self):
        result = self.get("回避アクション")
        self.assertEqual(result["page"]["page_id"], DODGE_ID)
        self.assertEqual([item["title"] for item in result["parents"]], ["プレイヤー"])
        kinds = [(section["heading"], section["kind"]) for section in result["sections"]]
        self.assertEqual(kinds, [("仕様概要", "仕様"), ("詳細説明", "仕様"), ("詳細説明 > 演出", "仕様")])
        text = "\n".join(section["text"] for section in result["sections"])
        self.assertIn("[プレイヤー](<SKC/仕様概要/仕様リスト/プレイヤー.md>)", text)
        self.assertIn("# コード内の見出しは節にしない", text)
        self.assertNotIn("table_of_contents", text)
        self.assertNotIn("爽快感", text)
        self.assertEqual(result["omitted_sections"], ["詳細説明 > 経緯", "仕様意図"])

    def test_get_rationale_converts_tables_and_links(self):
        result = self.get(DODGE_ID, "--rationale")
        rationale = [section for section in result["sections"] if section["kind"] == "制作意図"]
        self.assertEqual([section["heading"] for section in rationale], ["詳細説明 > 経緯", "仕様意図"])
        text = rationale[-1]["text"]
        self.assertIn("| 日付 | 案 |\n|---|---|\n| 2026-10-30 | キーコンフィグ |", text)
        self.assertIn("](<SKC/仕様概要/仕様リスト/回避/実装.md>)", text)

    def test_get_status_reads_implementation_list(self):
        result = self.get("SKC/仕様概要/仕様リスト/回避.md", "--status")
        self.assertEqual([item["page_id"] for item in result["status"]], [STATUS_ID])
        self.assertEqual(result["status"][0]["properties"]["ステータス"], "進行中")
        self.assertIn("キーコンフィグは未実装", result["status"][0]["text"])

    def test_get_reports_ambiguous_and_missing_titles(self):
        (self.root / "SKC" / "仕様概要" / "仕様リスト" / "回避2.md").write_text(
            spec_page("回避アクション", "4097c2c6cc0281aab8fcdd2b5301b960", "バトルパート"), encoding="utf-8")
        code, _, err = self.run_cli("get", "回避アクション")
        self.assertEqual(code, 2)
        self.assertIn("1件に決められません", err)
        code, _, err = self.run_cli("get", "存在しないページ")
        self.assertEqual(code, 2)
        self.assertIn("search で探してください", err)

    def test_reads_catalog_file_when_present(self):
        _, _, err = self.run_cli("search", "回避")
        self.assertIn("その場で作りました", err)
        with contextlib.redirect_stdout(io.StringIO()):
            build_catalog(["--source", str(self.root)])
        code, out, err = self.run_cli("search", "回避")
        self.assertEqual((code, err), (0, ""))
        self.assertIn(DODGE_ID, out)


if __name__ == "__main__":
    unittest.main()
