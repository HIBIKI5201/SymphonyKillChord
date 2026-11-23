"""check_launcher_scripts の判定を確かめる。"""
import json
import tempfile
import unittest
from pathlib import Path

import check_launcher_scripts as tool


def entry(**overrides):
    base = {"id": "setup", "name": "初期化", "category": "初期化", "windows": "Setup.bat", "mac": "Setup.command"}
    base.update(overrides)
    return base


class CheckLauncherScriptsTest(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.root = Path(self.tmp.name)
        for name in ("Setup.bat", "Setup.command"):
            (self.root / name).write_text("", encoding="utf-8")

    def tearDown(self):
        self.tmp.cleanup()

    def write_config(self, scripts, ignored=None, version=1):
        path = self.root / tool.CONFIG_PATH
        path.parent.mkdir(parents=True, exist_ok=True)
        config = {"version": version, "scripts": scripts}
        if ignored is not None:
            config["ignored"] = ignored
        path.write_text(json.dumps(config, ensure_ascii=False), encoding="utf-8")

    def test_registered_files_pass(self):
        self.write_config([entry()])
        errors, warnings = tool.check(self.root, ["Setup.bat", "Setup.command"])
        self.assertEqual([], errors)
        self.assertEqual([], warnings)

    def test_unregistered_entry_warns(self):
        self.write_config([entry()])
        errors, warnings = tool.check(self.root, ["Setup.bat", "Setup.command", "Tools/new.bat"])
        self.assertEqual([], errors)
        self.assertEqual(1, len(warnings))
        self.assertIn("Tools/new.bat", warnings[0])

    def test_ignored_entry_does_not_warn(self):
        self.write_config([entry()], ignored=[{"path": "Tools/new.bat", "reason": "直接動かさない"}])
        errors, warnings = tool.check(self.root, ["Setup.bat", "Setup.command", "Tools/new.bat"])
        self.assertEqual([], errors)
        self.assertEqual([], warnings)

    def test_ignored_without_reason_is_error(self):
        self.write_config([entry()], ignored=[{"path": "Tools/new.bat"}])
        errors, _ = tool.check(self.root, ["Setup.bat", "Setup.command"])
        self.assertEqual(1, len(errors))

    def test_missing_registered_file_warns_unless_local_only(self):
        self.write_config([entry(), entry(id="install", windows="Builds/Install_App.ps1", mac=None, localOnly=True),
                           entry(id="gone", windows="Gone.bat", mac=None)])
        errors, warnings = tool.check(self.root, ["Setup.bat", "Setup.command"])
        self.assertEqual([], errors)
        self.assertEqual(1, len(warnings))
        self.assertIn("Gone.bat", warnings[0])

    def test_invalid_configs_are_errors(self):
        cases = {
            "id の重複": [entry(), entry()],
            "name が無い": [{"id": "a", "category": "c", "windows": "Setup.bat"}],
            "拡張子": [entry(windows="Setup.sh")],
            "親フォルダ": [entry(windows="../Setup.bat")],
            "mode": [entry(mode="window")],
            "bat に &": [entry(presets=[{"label": "x", "args": ["a&b"]}])],
            "管理者で出力を受ける": [entry(elevate=True, presets=[{"label": "x", "mode": "capture"}])],
        }
        for name, scripts in cases.items():
            with self.subTest(name):
                self.write_config(scripts)
                errors, _ = tool.check(self.root, ["Setup.bat", "Setup.command"])
                self.assertTrue(errors, name)

    def test_unsupported_version_is_error(self):
        self.write_config([entry()], version=2)
        errors, _ = tool.check(self.root, ["Setup.bat", "Setup.command"])
        self.assertTrue(errors)

    def test_repository_config_is_valid(self):
        root = Path(__file__).resolve().parents[2]
        errors, warnings = tool.check(root)
        self.assertEqual([], errors)
        self.assertEqual([], warnings)


if __name__ == "__main__":
    unittest.main()
