"""Tests for pairing Demo/Master assets by name and reporting differences as warnings."""
from pathlib import Path
import io
import tempfile
import unittest

import diff_demo_master as tool


def write(root, relative, text):
    path = root / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def asset(root, relative, guid, body):
    write(root, relative, body)
    write(root, relative + ".meta", f"fileFormatVersion: 2\nguid: {guid}\n")


class DiffDemoMasterTest(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.root = Path(self.tmp.name)
        base = "Assets/Level/Data"
        asset(self.root, f"{base}/Demo/Stage.asset", "d" * 32, "name: 1\n")
        asset(self.root, f"{base}/Master/Stage.asset", "e" * 32, "name: 1\n")
        # Each side references its own Stage: equal after mapping.
        asset(self.root, f"{base}/Demo/Tree.asset", "1" * 32, f"- {{guid: {'d' * 32}}}\n")
        asset(self.root, f"{base}/Master/Tree.asset", "2" * 32, f"- {{guid: {'e' * 32}}}\n")
        # Real value difference.
        asset(self.root, f"{base}/Demo/Mission.asset", "3" * 32, "time: 60\n")
        asset(self.root, f"{base}/Master/Mission.asset", "4" * 32, "time: 120\n")
        # Paired by the Demo -> Master token in the file name.
        asset(self.root, f"{base}/Demo/DemoPanel.asset", "5" * 32, "a\n")
        asset(self.root, f"{base}/Master/MasterPanel.asset", "6" * 32, "b\n")
        # Demo only: not a pair.
        asset(self.root, f"{base}/Demo/OnlyDemo.asset", "7" * 32, "x\n")

    def tearDown(self):
        self.tmp.cleanup()

    def run_report(self, changed=None):
        out = io.StringIO()
        differing = tool.report(self.root, changed, out=out)
        return {d for d, _, _ in differing}, out.getvalue()

    def test_reports_only_real_differences_as_warnings(self):
        differing, out = self.run_report()
        self.assertEqual(differing, {
            "Assets/Level/Data/Demo/Mission.asset",
            "Assets/Level/Data/Demo/DemoPanel.asset",
        })
        self.assertIn("::warning ", out)
        self.assertNotIn("::error", out)

    def test_changed_filter_and_one_sided_note(self):
        differing, out = self.run_report({"Assets/Level/Data/Master/Mission.asset"})
        self.assertEqual(differing, {"Assets/Level/Data/Demo/Mission.asset"})
        self.assertIn("file=Assets/Level/Data/Master/Mission.asset", out)
        self.assertIn("Master だけが変わっています", out)

    def test_main_exits_zero_even_with_differences(self):
        self.assertEqual(tool.main([]), 0)


if __name__ == "__main__":
    unittest.main()
