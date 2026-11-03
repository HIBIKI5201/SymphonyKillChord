"""評価スクリプトの並べ方と、評価セットの形を確かめる。"""
import json
import unittest
from pathlib import Path

from eval_spec_query import DEFAULT_SET, fulltext_rank, hit_rank

PAGE_ID_LENGTH = 32


class EvalSpecQueryTests(unittest.TestCase):
    def test_fulltext_needs_all_keywords_and_ranks_by_count(self):
        files = [("a.md", "回避 回避 回避"), ("b.md", "回避 無敵"), ("c.md", "回避 無敵 無敵 無敵"), ("d.md", "無敵")]
        self.assertEqual(fulltext_rank(files, ["回避", "無敵"]), ["c.md", "b.md"])

    def test_hit_rank_is_one_based_and_zero_when_missing(self):
        self.assertEqual(hit_rank(["x", "y", "z"], {"y", "z"}), 2)
        self.assertEqual(hit_rank(["x"], {"y"}), 0)

    def test_eval_set_items_are_well_formed(self):
        items = json.loads(Path(DEFAULT_SET).read_text(encoding="utf-8"))["items"]
        self.assertGreaterEqual(len(items), 30)
        self.assertEqual(len({item["id"] for item in items}), len(items))
        for item in items:
            self.assertTrue(item["question"] and item["keywords"], item["id"])
            for page_id in item["expected"]:
                self.assertEqual(len(page_id), PAGE_ID_LENGTH, item["id"])
                self.assertEqual(page_id, page_id.lower().replace("-", ""), item["id"])


if __name__ == "__main__":
    unittest.main()
