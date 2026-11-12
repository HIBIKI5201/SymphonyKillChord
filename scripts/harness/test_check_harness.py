"""Exercise failures that would silently damage agent routing."""
import tempfile
import unittest
from pathlib import Path
from check_harness import LIMITS, LINKED, validate


class HarnessValidationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        for relative in set(LIMITS) | set(LINKED):
            self.write(relative, "short document")
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.skill(".agents", "shared")
        self.skill(".agents", "skc-feature-flow")

    def tearDown(self):
        self.temp.cleanup()

    def write(self, relative, text):
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")

    def skill(self, directory, name):
        self.write(f"{directory}/skills/{name}/SKILL.md",
                   f"---\nname: {name}\ndescription: Test capability.\n---\n")

    def test_valid_fixture(self):
        self.assertEqual([], validate(self.root)[1])

    def test_duplicate_skill_is_rejected(self):
        self.skill(".codex", "shared")
        self.assertTrue(any("duplicate skill" in e for e in validate(self.root)[1]))

    def test_broken_link_and_escape_are_rejected(self):
        self.write("AGENTS.md", "[missing](missing.md) [escape](../outside.md)")
        self.assertEqual(2, sum("broken local link" in e for e in validate(self.root)[1]))

    def test_relative_link_with_fragment_is_resolved(self):
        self.write("Assets/Scripts/DesignPhilosophy.md", "## Rules\n")
        self.write("AGENTS.md", "[rules](Assets/Scripts/DesignPhilosophy.md#rules)")
        self.assertEqual([], validate(self.root)[1])

    def test_missing_heading_is_rejected(self):
        self.write("AGENTS.md", "[missing](Assets/Scripts/DesignPhilosophy.md#missing)")
        self.assertTrue(any("broken heading link" in e for e in validate(self.root)[1]))

    def test_budget_growth_is_rejected(self):
        self.write("AGENTS.md", "あ" * (LIMITS["AGENTS.md"] + 1))
        self.assertTrue(any("budget exceeded" in e for e in validate(self.root)[1]))

    def test_missing_entrypoint_is_rejected(self):
        (self.root / "AGENTS.md").unlink()
        self.assertTrue(any("missing entrypoint" in e for e in validate(self.root)[1]))

    def test_long_project_description_is_rejected(self):
        self.write(".claude/skills/skc-long/SKILL.md",
                   f"---\nname: skc-long\ndescription: \"{'a' * 301}\"\n---\n")
        self.assertTrue(any("description too long" in e for e in validate(self.root)[1]))

    def test_vendor_description_is_not_limited(self):
        self.write(".claude/skills/unity-cli/SKILL.md",
                   f"---\nname: unity-cli\ndescription: {'a' * 400}\n---\n")
        self.assertEqual([], validate(self.root)[1])

    def test_claude_entry_without_agents_import_is_rejected(self):
        self.write("CLAUDE.md", "AGENTS.md を読む")
        self.assertTrue(any("must import AGENTS.md" in e for e in validate(self.root)[1]))

    def test_malformed_skill_is_rejected(self):
        self.write(".agents/skills/shared/SKILL.md", "no metadata")
        self.assertTrue(any("invalid frontmatter" in e for e in validate(self.root)[1]))

    def test_notion_map_reader_must_cite_page_id(self):
        page_id = "2a87c2c6-cc02-80f9-a10a-f0b72b95fbb3"
        self.write("SinfoniaOperator/notion-cache-map.json",
                   '{"pages": [{"id": "%s", "name": "rules/code", "readers": ["AGENTS.md"]}]}' % page_id)
        self.assertTrue(any("lacks page id" in e for e in validate(self.root)[1]))
        self.write("AGENTS.md", f"コード規定 `{page_id}`")
        self.assertEqual(validate(self.root)[1], [])

    def test_notion_map_rejects_bad_entries(self):
        self.write("SinfoniaOperator/notion-cache-map.json",
                   '{"pages": [{"id": "2a87c2c6", "name": "x", "readers": ["AGENTS.md"]},'
                   ' {"id": "31c7c2c6-cc02-80f3-92c0-e6929d962476", "name": "y", "readers": ["missing.md"]}]}')
        errors = validate(self.root)[1]
        self.assertTrue(any("invalid notion map entry" in e for e in errors))
        self.assertTrue(any("missing notion map reader" in e for e in errors))


if __name__ == "__main__":
    unittest.main()
