"""Report content differences between paired Demo and Master assets as warnings.

Pairs are found from names only: a directory named Demo with a sibling directory
named Master is a pair root, and a file under Demo/ pairs with the file at the same
relative path under Master/ (a "Demo" token in the file name may read "Master").
GUID references are shown as asset paths, and Demo paths are read as their Master
counterpart, so "Demo references Demo, Master references Master" is not a difference.
Differences are warnings, not failures: Demo and Master are allowed to differ.
"""
from pathlib import Path
import argparse
import difflib
import os
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
GUID_LINE = re.compile(r"^guid:\s*([0-9a-f]{32})\s*$", re.MULTILINE)
GUID_REF = re.compile(r"guid: ([0-9a-f]{32})")
MAX_DIFF_LINES = 40


def relative(path, root):
    return path.relative_to(root).as_posix()


def find_pair_roots(assets):
    """Return (demo_dir, master_dir) for every Demo directory with a sibling Master."""
    roots = []
    for current, dirs, _ in os.walk(assets):
        if "Demo" in dirs and "Master" in dirs:
            roots.append((Path(current) / "Demo", Path(current) / "Master"))
    return sorted(roots)


def counterpart_name(name):
    """Swap the Demo token in a file name for Master (DemoFoo.asset -> MasterFoo.asset)."""
    return name.replace("Demo", "Master")


def find_pairs(roots):
    """Return (demo_file, master_file) for files that exist on both sides."""
    pairs = []
    for demo_root, master_root in roots:
        for demo_file in sorted(demo_root.rglob("*")):
            if not demo_file.is_file() or demo_file.suffix == ".meta":
                continue
            relative = demo_file.relative_to(demo_root)
            for name in dict.fromkeys([relative.name, counterpart_name(relative.name)]):
                master_file = master_root / relative.parent / name
                if master_file.is_file():
                    pairs.append((demo_file, master_file))
                    break
    return pairs


def build_guid_paths(assets, root):
    """Map every GUID under Assets/ to its asset path (relative to root)."""
    paths = {}
    for meta in assets.rglob("*.meta"):
        match = GUID_LINE.search(meta.read_text(encoding="utf-8", errors="replace"))
        if match:
            paths[match.group(1)] = relative(meta.with_suffix(""), root)
    return paths


def to_master_path(path, roots, root):
    """Rewrite a path under a Demo pair root to the matching Master path."""
    for demo_root, master_root in roots:
        demo_rel, master_rel = relative(demo_root, root), relative(master_root, root)
        if path == demo_rel or path.startswith(demo_rel + "/"):
            rest = path[len(demo_rel):]
            head, _, name = rest.rpartition("/")
            return master_rel + head + "/" + counterpart_name(name) if name else master_rel
    return path


def read_lines(path):
    """Read text with line endings normalized; return None for binary files."""
    data = path.read_bytes()
    if b"\0" in data:
        return None
    return data.decode("utf-8", errors="replace").replace("\r\n", "\n").splitlines()


def resolve_refs(lines, guid_paths, rewrite=lambda path: path):
    """Replace GUID references with asset paths so the diff is readable and pair-aware."""
    def sub(match):
        path = guid_paths.get(match.group(1))
        return "guid: " + (rewrite(path) if path else match.group(1))
    return [GUID_REF.sub(sub, line) for line in lines]


def compare(demo, master, guid_paths, demo_rewrite):
    """Return unified diff lines, or [] when equal after mapping Demo references to Master."""
    demo_lines, master_lines = read_lines(demo), read_lines(master)
    if demo_lines is None or master_lines is None:
        return [] if demo.read_bytes() == master.read_bytes() else ["(バイナリが違います)"]
    demo_lines = resolve_refs(demo_lines, guid_paths, demo_rewrite)
    master_lines = resolve_refs(master_lines, guid_paths)
    return list(difflib.unified_diff(demo_lines, master_lines, "Demo", "Master", lineterm="", n=1))


def report(root, changed=None, out=sys.stdout, summary=None):
    """Print GitHub warning annotations for differing pairs; return the differing pairs.

    When changed is given, only pairs with at least one side in it are checked.
    """
    roots = find_pair_roots(root / "Assets")
    pairs = find_pairs(roots)
    guid_paths = build_guid_paths(root / "Assets", root)
    demo_rewrite = lambda path: to_master_path(path, roots, root)
    differing = []
    for demo, master in pairs:
        demo_rel, master_rel = relative(demo, root), relative(master, root)
        if changed is not None and demo_rel not in changed and master_rel not in changed:
            continue
        diff = compare(demo, master, guid_paths, demo_rewrite)
        if not diff:
            continue
        differing.append((demo_rel, master_rel, diff))
        if changed is not None and (demo_rel in changed) != (master_rel in changed):
            side = "Demo" if demo_rel in changed else "Master"
            note = f"このPRでは {side} だけが変わっています。"
        else:
            note = ""
        target = master_rel if changed is not None and master_rel in changed else demo_rel
        print(f"::warning file={target}::Demo と Master で内容が違います（{demo_rel} ↔ {master_rel}）。"
              f"{note}意図した違いなら、このままで構いません", file=out)
    if summary is not None:
        write_summary(summary, len(pairs), differing, changed is not None)
    return differing


def write_summary(summary, pair_count, differing, filtered):
    scope = "このPRで変わった対" if filtered else "全ての対"
    lines = [f"## Demo と Master の差分（{scope}）", "",
             f"対になるアセット {pair_count} 件のうち、違いがあるもの: {len(differing)} 件。"
             "差分は警告で、チェックは失敗しません。",
             "GUID はアセットのパスで表示し、Demo 側の参照は対になる Master のパスに読み替えています。", ""]
    for demo_rel, master_rel, diff in differing:
        shown = diff[:MAX_DIFF_LINES]
        more = len(diff) - len(shown)
        lines += [f"<details><summary><code>{demo_rel}</code> ↔ <code>{master_rel}</code></summary>",
                  "", "```diff", *shown, *([f"... ほか {more} 行"] if more else []), "```",
                  "", "</details>", ""]
    summary.write("\n".join(lines) + "\n")


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--changed", help="このPRで変わったファイルの一覧（1行1パス）。省略すると全ての対を見る")
    args = parser.parse_args(argv)
    changed = None
    if args.changed:
        text = Path(args.changed).read_text(encoding="utf-8")
        changed = {line.strip().removesuffix(".meta") for line in text.splitlines() if line.strip()}
    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary_path:
        with open(summary_path, "a", encoding="utf-8") as summary:
            differing = report(ROOT, changed, summary=summary)
    else:
        differing = report(ROOT, changed)
    print(f"Demo と Master で違いのある対: {len(differing)} 件（警告のみ）")
    return 0


if __name__ == "__main__":
    sys.exit(main())
