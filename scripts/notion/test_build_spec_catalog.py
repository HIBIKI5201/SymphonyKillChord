"""仕様書目録の生成を、小さな仕様書ミラーで確かめる。"""
import json
import tempfile
import unittest
from pathlib import Path

from build_spec_catalog import CATALOG_JSONL, CATALOG_MARKDOWN, main

SPEC_ID = "31f7c2c6cc02809da534ceee55d7b005"
TERM_ID = "3257c2c6cc028020ae8fc1304e2be69e"

SPEC_PAGE = f"""<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->
# スキル

[Notionで開く](https://app.notion.com/p/{SPEC_ID})

最終更新: 2026-10-30T06:52:00.0000000+00:00

## プロパティ

| プロパティ | 値 |
|---|---|
| カテゴリー | 全体 |
| サマリー | プレイヤーが発動するスキル |
| 親仕様 | 3257c2c6-cc02-8020-ae8f-c1304e2be69e |
| 適用範囲 |  |

---

# 仕様概要
## 関連用語
<mention-page url="https://app.notion.com/p/{TERM_ID}"/>
```
# コード内の見出しは拾わない
```
### **発動条件** {{toggle="true"}}
[用語へ](../../%E7%94%A8%E8%AA%9E/%E3%82%AD%E3%83%AB%E3%82%B3%E3%83%BC%E3%83%89.md)
"""

TERM_PAGE = f"""<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->
# キルコード

[Notionで開く](https://app.notion.com/p/Kill-{TERM_ID})

最終更新: 2026-10-01T00:00:00.0000000+00:00

---

# 定義
スキルのゲーム内呼称。
"""

DATABASE = """<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->
# 仕様リスト

[Notionで開く](https://app.notion.com/p/27d7c2c6cc028131aee3cf5d08b0b03a)
"""


class BuildSpecCatalogTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.write("README.md", "# README\n")
        self.write("SKC/仕様概要/仕様リスト/_database.md", DATABASE)
        self.write("SKC/仕様概要/仕様リスト/スキル.md", SPEC_PAGE)
        self.write("SKC/用語/キルコード.md", TERM_PAGE)
        self.write("assets/abc/ignored.md", "# 添付\n")

    def tearDown(self):
        self.temp.cleanup()

    def write(self, relative, text):
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")

    def build(self):
        self.assertEqual(0, main(["--source", str(self.root)]))
        rows = [json.loads(line) for line in (self.root / CATALOG_JSONL).read_text(encoding="utf-8").splitlines()]
        return {row["path"]: row for row in rows}, (self.root / CATALOG_MARKDOWN).read_text(encoding="utf-8")

    def test_excludes_database_index_assets_and_readme(self):
        rows, _ = self.build()
        self.assertEqual(["SKC/仕様概要/仕様リスト/スキル.md", "SKC/用語/キルコード.md"], sorted(rows))

    def test_reads_preamble_properties_and_headings(self):
        row = self.build()[0]["SKC/仕様概要/仕様リスト/スキル.md"]
        self.assertEqual("スキル", row["title"])
        self.assertEqual(SPEC_ID, row["page_id"])
        self.assertEqual("2026-10-30T06:52:00.0000000+00:00", row["last_edited"])
        self.assertEqual("仕様リスト", row["database"])
        self.assertNotIn("適用範囲", row["properties"])
        self.assertEqual("全体", row["properties"]["カテゴリー"])
        self.assertEqual(
            [[1, "仕様概要"], [2, "関連用語"], [3, "発動条件"]],
            [[heading["level"], heading["text"]] for heading in row["headings"]])

    def test_resolves_relations_mentions_and_relative_links(self):
        row = self.build()[0]["SKC/仕様概要/仕様リスト/スキル.md"]
        self.assertEqual({"親仕様": ["SKC/用語/キルコード.md"]}, row["relations"])
        self.assertEqual(["SKC/用語/キルコード.md"], row["links"])

    def test_markdown_line_shows_relation_title_and_h1_h2_only(self):
        _, markdown = self.build()
        line = next(line for line in markdown.splitlines() if "スキル.md>" in line)
        self.assertEqual(
            "- [スキル](<SKC/仕様概要/仕様リスト/スキル.md>) 〔仕様リスト〕 カテゴリー: 全体 ／ "
            "サマリー: プレイヤーが発動するスキル ／ 親仕様: キルコード § 仕様概要 / 関連用語",
            line)
        self.assertIn("- [仕様リスト](<SKC/仕様概要/仕様リスト/_database.md>) 1件", markdown)

    def test_output_is_stable_when_nothing_changes(self):
        _, first = self.build()
        stamp = (self.root / CATALOG_MARKDOWN).stat().st_mtime_ns
        _, second = self.build()
        self.assertEqual(first, second)
        self.assertEqual(stamp, (self.root / CATALOG_MARKDOWN).stat().st_mtime_ns)

    def test_catalog_does_not_list_itself(self):
        self.build()
        rows, _ = self.build()
        self.assertFalse(any(path.startswith("_catalog") for path in rows))


if __name__ == "__main__":
    unittest.main()
