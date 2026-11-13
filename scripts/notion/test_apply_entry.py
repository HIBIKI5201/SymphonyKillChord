"""JSON → 本文・--set の変換を確かめる。Notion へは接続しない。"""
import datetime
import unittest
from apply_entry import EntryError, load_templates, parse_entries, resolve_relations, set_arguments

TODAY = datetime.date(2026, 10, 30)


def forum(**overrides):
    entry = {
        "type": "forum",
        "title": "ロックオン時のカメラ補正",
        "properties": {"担当者": ["八幡", "東"], "優先度": 5},
        "sections": {"経緯": "- 課題", "初期案の要約": "補正を弱める", "初期案": "# 仕様意図\n理由"},
    }
    entry.update(overrides)
    return entry


class ApplyEntryTests(unittest.TestCase):
    def setUp(self):
        self.templates = load_templates()

    def parse(self, data):
        return parse_entries(data, self.templates, TODAY)

    def assert_error(self, data, text):
        with self.assertRaises(EntryError) as context:
            self.parse(data)
        self.assertIn(text, str(context.exception))

    def test_forum_body_follows_template(self):
        entry = self.parse(forum())[0]
        self.assertEqual(
            ["--set", "名前=ロックオン時のカメラ補正", "--set", "ステータス=作業中",
             "--set", "担当者=八幡,東", "--set", "優先度=5"],
            set_arguments(entry))
        body = entry["body"]
        self.assertTrue(body.startswith('# 結論 {toggle="true"}\n\t# 記述指針\n\t<empty-block/>\n'))
        self.assertIn("\t## 経緯\n\t- 課題\n", body)
        self.assertIn("\t<summary>初期案<br>補正を弱める</summary>\n\t\t# 仕様意図\n\t\t理由\n\t</details>\n", body)

    def test_minutes_defaults(self):
        entry = self.parse({"type": "minutes", "title": "定例", "properties": {"議題": "進捗"},
                            "sections": {"ログ": ["- 報告1", "- 報告2"]}})[0]
        self.assertEqual({"名前": "定例", "日付": "2026-10-30", "議題": "進捗"}, entry["properties"])
        self.assertEqual('<table_of_contents color="gray"/>\n# 決定\nなし\n# ログ\n- 報告1\n- 報告2\n', entry["body"])

    def test_task_relation_is_resolved(self):
        built = self.parse([{"type": "task", "title": "実装", "properties": {"親タスク": "親"},
                             "sections": {"内容": "やる"}}])
        calls = []

        def resolver(name, definition, item):
            calls.append((name, item))
            return item + "-id"

        resolve_relations(built, self.templates, resolver)
        self.assertEqual("現在-id", built[0]["properties"]["スプリント"])
        self.assertEqual("親-id", built[0]["properties"]["親タスク"])
        self.assertEqual([("スプリント", "現在"), ("親タスク", "親")], sorted(calls))

    def test_unknown_property_is_rejected(self):
        self.assert_error(forum(properties={"色": "赤"}), "未知のプロパティです: 色")

    def test_status_outside_options_is_rejected(self):
        self.assert_error(forum(properties={"ステータス": "作成中"}), "選択肢にない値です: 作成中")

    def test_multi_select_outside_options_is_rejected(self):
        self.assert_error(forum(properties={"担当者": "知らない人"}), "選択肢にない値です: 知らない人")

    def test_missing_required_section_is_rejected(self):
        self.assert_error(forum(sections={}), "節「経緯」は必須です")

    def test_unknown_section_is_rejected(self):
        self.assert_error(forum(sections={"結論": "x"}), "未知の節です: 結論")

    def test_inline_section_must_be_one_line(self):
        sections = {"経緯": "a", "初期案の要約": "1行目\n2行目", "初期案": "b"}
        self.assert_error(forum(sections=sections), "1行で書いてください")

    def test_bad_date_and_number_are_rejected(self):
        self.assert_error({"type": "minutes", "title": "x", "properties": {"日付": "10/30", "議題": "y"},
                           "sections": {"ログ": "z"}}, "YYYY-MM-DD")
        self.assert_error(forum(properties={"優先度": "高"}), "数値で指定してください")

    def test_missing_title_and_type(self):
        self.assert_error(forum(title=""), "title（ページ名）は必須です")
        self.assert_error({"type": "memo", "title": "x"}, "type は")

    def test_one_bad_entry_stops_all(self):
        self.assert_error([forum(), forum(properties={"色": 1})], "2件目")


if __name__ == "__main__":
    unittest.main()
