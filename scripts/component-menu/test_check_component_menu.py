"""check_component_menu の判定と自動付与を確かめる。"""
from pathlib import Path
import subprocess
import tempfile
import unittest

import check_component_menu as tool


def write(root: Path, relative: str, text: str) -> None:
    path = root / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def script(root: Path, relative: str, guid: str, body: str) -> None:
    write(root, relative, body)
    write(root, relative + ".meta", f"fileFormatVersion: 2\nguid: {guid}\n")


def behaviour(name: str, attr: str = "", base: str = "MonoBehaviour", mods: str = "public sealed") -> str:
    return (
        "using UnityEngine;\n\nnamespace KillChord\n{\n"
        "    /// <summary>\n    ///     テスト用。\n    /// </summary>\n"
        f"{attr}    {mods} class {name} : {base}\n    {{\n    }}\n}}\n"
    )


class CheckComponentMenuTest(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.root = Path(self.tmp.name)
        view = "Assets/Scripts/Runtime/4.View/InGame/Enemy"
        script(self.root, f"{view}/EnemyView.cs", "a" * 32, behaviour("EnemyView"))
        script(
            self.root,
            f"{view}/EnemyHpView.cs",
            "b" * 32,
            behaviour("EnemyHpView", '    [AddComponentMenu("KillChord/View/Enemy/EnemyHpView")]\n'),
        )
        script(
            self.root,
            f"{view}/EnemyWrongView.cs",
            "c" * 32,
            behaviour("EnemyWrongView", '    [AddComponentMenu("KillChord/Debug/Enemy/Other")]\n'),
        )
        script(self.root, f"{view}/EnemyViewBase.cs", "d" * 32, behaviour("EnemyViewBase", mods="public abstract"))
        script(self.root, f"{view}/BossView.cs", "e" * 32, behaviour("BossView", base="EnemyViewBase"))
        # シーンに置かれていないコード専用のコンポーネント。
        script(self.root, f"{view}/EnemyRuntimeOnly.cs", "f" * 32, behaviour("EnemyRuntimeOnly"))
        script(
            self.root,
            "Assets/Scripts/Runtime/6.Composition/OutGame/OutGameSceneInitializer.cs",
            "1" * 32,
            behaviour("OutGameSceneInitializer"),
        )
        script(self.root, "Assets/Scripts/DebugWorld/Develop/View/DestroyOnAwake.cs", "2" * 32, behaviour("DestroyOnAwake"))
        refs = "".join(
            f"  m_Script: {{fileID: 11500000, guid: {g * 32}, type: 3}}\n" for g in "abcde12"
        )
        write(self.root, "Assets/Level/Scenes/Test.unity", refs)
        subprocess.run(["git", "init", "-q"], cwd=self.root, check=True)
        subprocess.run(["git", "add", "-A"], cwd=self.root, check=True)

    def tearDown(self):
        self.tmp.cleanup()

    def results(self):
        return {t.name: t for t in tool.check(self.root)}

    def test_only_placed_concrete_behaviours_are_checked(self):
        names = set(self.results())
        self.assertIn("BossView", names)
        self.assertNotIn("EnemyViewBase", names)
        self.assertNotIn("EnemyRuntimeOnly", names)

    def test_reports_missing_and_wrong_paths(self):
        results = self.results()
        self.assertEqual([], results["EnemyHpView"].issues)
        self.assertIn("ありません", results["EnemyView"].issues[0])
        wrong = " ".join(results["EnemyWrongView"].issues)
        self.assertIn("\"View\"", wrong)
        self.assertIn("EnemyWrongView", wrong)

    def test_expected_paths_follow_role_and_feature(self):
        results = self.results()
        self.assertEqual("KillChord/View/Enemy/EnemyView", tool.expected_menu(results["EnemyView"]))
        self.assertEqual(
            "KillChord/Composition/OutGame/OutGameSceneInitializer",
            tool.expected_menu(results["OutGameSceneInitializer"]),
        )
        self.assertEqual("KillChord/Debug/Common/DestroyOnAwake", tool.expected_menu(results["DestroyOnAwake"]))

    def test_fix_adds_attribute_above_class(self):
        tool.fix(self.root, list(self.results().values()))
        text = (self.root / "Assets/Scripts/Runtime/4.View/InGame/Enemy/EnemyView.cs").read_text(encoding="utf-8")
        self.assertIn('    /// </summary>\n    [AddComponentMenu("KillChord/View/Enemy/EnemyView")]\n    public sealed class', text)
        self.assertTrue(all(not t.issues for n, t in self.results().items() if n != "EnemyWrongView"))

    def test_strict_fails_when_violations_remain(self):
        self.assertEqual(1, tool.main(["--root", str(self.root), "--strict"]))
        self.assertEqual(0, tool.main(["--root", str(self.root)]))


if __name__ == "__main__":
    unittest.main()
