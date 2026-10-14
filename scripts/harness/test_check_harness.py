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

    def test_malformed_skill_is_rejected(self):
        self.write(".agents/skills/shared/SKILL.md", "no metadata")
        self.assertTrue(any("invalid frontmatter" in e for e in validate(self.root)[1]))


if __name__ == "__main__":
    unittest.main()
