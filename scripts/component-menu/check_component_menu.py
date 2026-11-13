"""シーン・プレハブに置くコンポーネントに AddComponentMenu が付いているかを調べる。

コード規約 CG-073 の検査。シーン (.unity) とプレハブ (.prefab) から参照されている
MonoBehaviour の具象クラスに、`[AddComponentMenu("KillChord/<役割>/<機能>/<クラス名>")]`
が付いていることを確かめる。Roslyn アナライザーからはシーンが見えないため、ここで調べる。

使い方:
    python scripts/component-menu/check_component_menu.py            # 警告を出す（終了コード 0）
    python scripts/component-menu/check_component_menu.py --strict   # 違反があれば終了コード 1
    python scripts/component-menu/check_component_menu.py --fix      # 付いていないものへ自動で付ける
"""
from __future__ import annotations

import argparse
import os
import re
import subprocess
import sys
from dataclasses import dataclass, field
from pathlib import Path

MENU_ROOT = "KillChord"

# 調べる対象のフォルダと、そこに置くクラスの役割。上から順に最初に一致したものを使う。
ROLE_ROOTS = [
    ("Assets/Scripts/DebugWorld/", "Debug"),
    ("Assets/Scripts/Demo/", "Demo"),
    ("Assets/Scripts/Shaders/", "Rendering"),
    ("Assets/Scripts/Runtime/4.View/", "View"),
    ("Assets/Scripts/Runtime/6.Composition/", "Composition"),
]

# 役割として使ってよい名前。Runtime の他の層に置いたものは、ここに無い役割になり違反になる。
ROLES = {role for _, role in ROLE_ROOTS}

# プロジェクト外で定義された MonoBehaviour の派生クラス（継承元として現れるもの）。
EXTERNAL_BEHAVIOURS = {
    "MonoBehaviour",
    "UIBehaviour",
    "Graphic",
    "MaskableGraphic",
    "Selectable",
    "BaseMeshEffect",
    "BaseInputModule",
    "PlayableDirector",
}

# 継承の判定から外すフォルダ（第三者のサンプルなど）。
SKIPPED_PARTS = ("/Editor/", "/Tests/", "Assets/AssetStoreTools/", "Assets/Plugins/")

AREAS = {"InGame", "OutGame", "Persistent"}
LAYER_FOLDERS = {"View", "Composition", "Develop"}

CLASS_DECL = re.compile(
    r"^(?P<indent>[ \t]*)(?P<mods>(?:(?:public|internal|private|protected|sealed|abstract|partial|static)\s+)*)"
    r"class\s+(?P<name>\w+)(?:<[^>]*>)?\s*:\s*(?P<bases>[\w.<>, ]+)",
    re.M,
)
MENU_ATTR = re.compile(r"\[\s*(?:UnityEngine\.)?AddComponentMenu(?:Attribute)?\s*\(\s*\"(?P<path>[^\"]*)\"")
SCRIPT_REF = re.compile(r"m_Script: \{fileID: 11500000, guid: (\w+)")


@dataclass
class ClassInfo:
    """1つのクラス宣言の情報。"""

    name: str
    path: str
    base: str
    is_abstract: bool
    start: int
    indent: str
    menu_path: str | None = None
    placed: bool = False
    issues: list[str] = field(default_factory=list)


def git_files(root: Path, *patterns: str) -> list[str]:
    """Git で管理されているファイルを列挙する。"""
    out = subprocess.check_output(["git", "ls-files", "-z", *patterns], cwd=root)
    # 衝突中は同じファイルが段ごとに並ぶため、重複を除く。
    return list(dict.fromkeys(p for p in out.decode("utf-8").split("\0") if p))


def read_text(path: Path) -> str:
    """BOM を除いて読み込む。"""
    return path.read_text(encoding="utf-8-sig", errors="ignore")


def role_of(path: str) -> str | None:
    """ファイルの場所から役割を決める。対象外なら None。"""
    if any(part in path for part in SKIPPED_PARTS):
        return None
    for prefix, role in ROLE_ROOTS:
        if path.startswith(prefix):
            return role
    if path.startswith("Assets/Scripts/Runtime/"):
        layer = path.split("/")[3]
        return re.sub(r"^\d+\.", "", layer)
    return None


def feature_of(path: str) -> str:
    """ファイルの場所から機能名（メニューの第2階層）を決める。"""
    for prefix, _ in ROLE_ROOTS:
        if path.startswith(prefix):
            folders = path[len(prefix):].split("/")[:-1]
            break
    else:
        folders = path.split("/")[4:-1]
    if not folders:
        return "Common"
    # 層・シーン区分のフォルダを読み飛ばし、最初の機能フォルダを使う。
    rest = [f for f in folders if f not in AREAS and f not in LAYER_FOLDERS and f != "Runtime"]
    if rest:
        return rest[0]
    # 機能フォルダが無いときは、シーン区分の直下ならその名前、それ以外は Common にする。
    areas = [f for f in folders if f in AREAS]
    return areas[-1] if areas else "Common"


def expected_menu(info: ClassInfo) -> str:
    """付けるべきメニューのパスを返す。"""
    return f"{MENU_ROOT}/{role_of(info.path)}/{feature_of(info.path)}/{info.name}"


def find_menu(text: str, start: int) -> str | None:
    """クラス宣言の直前の属性から AddComponentMenu のパスを探す。"""
    head = text[:start]
    lines = head.split("\n")
    collected: list[str] = []
    # 宣言の直前の属性・コメント行だけをさかのぼる。
    for line in reversed(lines[:-1]):
        stripped = line.strip()
        if stripped.startswith("[") or stripped.startswith("///") or stripped.startswith("//") or stripped == "":
            if stripped == "" and collected:
                break
            collected.append(stripped)
            continue
        break
    match = MENU_ATTR.search("\n".join(reversed(collected)))
    return match.group("path") if match else None


def collect_classes(root: Path, files: list[str]) -> dict[str, list[ClassInfo]]:
    """クラス宣言を名前ごとに集める。"""
    classes: dict[str, list[ClassInfo]] = {}
    for rel in files:
        if any(part in rel for part in SKIPPED_PARTS):
            continue
        text = read_text(root / rel)
        for m in CLASS_DECL.finditer(text):
            base = m.group("bases").split(",")[0].strip().split("<")[0].split(".")[-1]
            info = ClassInfo(
                name=m.group("name"),
                path=rel,
                base=base,
                is_abstract="abstract" in m.group("mods"),
                start=m.start(),
                indent=m.group("indent"),
                menu_path=find_menu(text, m.start()),
            )
            classes.setdefault(info.name, []).append(info)
    return classes


def behaviour_names(classes: dict[str, list[ClassInfo]]) -> set[str]:
    """MonoBehaviour を継承するクラス名を求める。"""
    names = set(EXTERNAL_BEHAVIOURS)
    changed = True
    while changed:
        changed = False
        for name, infos in classes.items():
            if name not in names and any(i.base in names for i in infos):
                names.add(name)
                changed = True
    return names


def placed_guids(root: Path) -> set[str]:
    """シーンとプレハブから参照されているスクリプトの GUID を集める。"""
    guids: set[str] = set()
    for rel in git_files(root, "*.unity", "*.prefab"):
        guids.update(SCRIPT_REF.findall((root / rel).read_text(encoding="utf-8", errors="ignore")))
    return guids


def guid_of(root: Path, rel: str) -> str | None:
    """スクリプトの .meta から GUID を読む。"""
    meta = root / (rel + ".meta")
    if not meta.exists():
        return None
    match = re.search(r"^guid:\s*(\w+)", meta.read_text(encoding="utf-8", errors="ignore"), re.M)
    return match.group(1) if match else None


def check(root: Path) -> list[ClassInfo]:
    """シーン・プレハブに置かれた対象クラスを調べ、問題を記録して返す。"""
    files = git_files(root, "Assets/*.cs")
    classes = collect_classes(root, files)
    behaviours = behaviour_names(classes)
    guids = placed_guids(root)
    targets: list[ClassInfo] = []
    for name, infos in sorted(classes.items()):
        if name not in behaviours:
            continue
        for info in infos:
            # Unity がスクリプトとして扱うのは、ファイル名と同じ名前のクラスだけ。
            if info.is_abstract or Path(info.path).stem != name or role_of(info.path) is None:
                continue
            if guid_of(root, info.path) not in guids:
                continue
            info.placed = True
            validate(info)
            targets.append(info)
    return targets


def validate(info: ClassInfo) -> None:
    """メニューのパスが規約の形かを調べる。"""
    role = role_of(info.path)
    if info.menu_path is None:
        info.issues.append(f"[AddComponentMenu] がありません。例: [AddComponentMenu(\"{expected_menu(info)}\")]")
        return
    parts = info.menu_path.split("/")
    if len(parts) != 4 or parts[0] != MENU_ROOT:
        info.issues.append(f"パスは \"{MENU_ROOT}/<役割>/<機能>/<クラス名>\" の4段にします（現在: \"{info.menu_path}\"）。")
        return
    if parts[1] not in ROLES or parts[1] != role:
        info.issues.append(f"役割は置き場所に合わせて \"{role}\" にします（現在: \"{parts[1]}\"）。")
    if parts[3] != info.name:
        info.issues.append(f"最後の段はクラス名 \"{info.name}\" にします（現在: \"{parts[3]}\"）。")


def line_of(root: Path, info: ClassInfo) -> int:
    """宣言の行番号を求める。"""
    return read_text(root / info.path)[: info.start].count("\n") + 1


def fix(root: Path, targets: list[ClassInfo]) -> int:
    """属性の無いクラスへ AddComponentMenu を付ける。付けた数を返す。"""
    by_file: dict[str, list[ClassInfo]] = {}
    for info in targets:
        if info.menu_path is None:
            by_file.setdefault(info.path, []).append(info)
    for rel, infos in by_file.items():
        path = root / rel
        raw = path.read_bytes()
        bom = raw.startswith(b"\xef\xbb\xbf")
        text = raw.decode("utf-8-sig")
        newline = "\r\n" if "\r\n" in text else "\n"
        for info in sorted(infos, key=lambda i: i.start, reverse=True):
            line = f"{info.indent}[AddComponentMenu(\"{expected_menu(info)}\")]{newline}"
            text = text[: info.start] + line + text[info.start:]
        if not re.search(r"^\s*using\s+UnityEngine\s*;", text, re.M):
            text = add_using(text, newline)
        path.write_bytes((b"\xef\xbb\xbf" if bom else b"") + text.encode("utf-8"))
    return sum(len(v) for v in by_file.values())


def add_using(text: str, newline: str) -> str:
    """`using UnityEngine;` を using のまとまりの末尾へ足す。"""
    usings = list(re.finditer(r"^using\s+[\w.]+\s*;[ \t]*\r?\n", text, re.M))
    if usings:
        end = usings[-1].end()
        return text[:end] + f"using UnityEngine;{newline}" + text[end:]
    return f"using UnityEngine;{newline}{newline}" + text


def main(argv: list[str] | None = None) -> int:
    """エントリポイント。"""
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--root", default=".", help="リポジトリのルート")
    parser.add_argument("--strict", action="store_true", help="違反があれば終了コード 1 にする")
    parser.add_argument("--fix", action="store_true", help="属性の無いクラスへ自動で付ける")
    args = parser.parse_args(argv)
    root = Path(args.root).resolve()

    targets = check(root)
    if args.fix:
        count = fix(root, targets)
        print(f"AddComponentMenu を {count} 件付けました。")
        targets = check(root)

    bad = [t for t in targets if t.issues]
    annotate = os.environ.get("GITHUB_ACTIONS") == "true"
    for info in bad:
        for issue in info.issues:
            if annotate:
                print(f"::warning file={info.path},line={line_of(root, info)},title=CG-073 AddComponentMenu::{issue}")
            else:
                print(f"{info.path}:{line_of(root, info)}: {info.name}: {issue}")
    print(f"シーン・プレハブに置かれた対象クラス {len(targets)} 件のうち、違反 {len(bad)} 件。")
    return 1 if args.strict and bad else 0


if __name__ == "__main__":
    sys.exit(main())
