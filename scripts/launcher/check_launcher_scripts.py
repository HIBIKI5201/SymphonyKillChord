"""スクリプト管理ツールの登録ファイル（ScriptLauncher/launcher-scripts.json）を調べる。

- ダブルクリックで動かす入口（.bat / .command）が、登録ファイルに無ければ警告する（登録漏れ）。
  わざと登録しないものは、登録ファイルの "ignored" に {"path": ..., "reason": ...} で書く。
- 登録したファイルがリポジトリに無ければ警告する（"localOnly": true のものは除く）。
- 登録ファイルの書き方の誤り（必須の欄・id の重複・mode・bat に渡せない引数）はエラーにする。
  同じ判定は C++ 側（ScriptLauncher/src/core/ScriptConfig.cpp）にもあり、ツールは誤りがあると一覧を出さない。

使い方:
    python scripts/launcher/check_launcher_scripts.py            # 警告を出す（書き方の誤りだけ終了コード 1）
    python scripts/launcher/check_launcher_scripts.py --strict   # 警告もあれば終了コード 1
"""
from __future__ import annotations

import argparse
import json
import os
import subprocess
import sys
from pathlib import Path, PurePosixPath

CONFIG_PATH = "ScriptLauncher/launcher-scripts.json"
SUPPORTED_VERSION = 1
ENTRY_PATTERNS = ("*.bat", "*.command")
WINDOWS_EXTENSIONS = (".bat", ".cmd", ".ps1", ".exe")
MODES = ("console", "capture")
BATCH_UNSAFE_CHARACTERS = set('"&|<>^%!()')


def tracked_entry_files(root: Path) -> list[str]:
    """git が管理している入口のファイルを返す（/ 区切り）。"""
    out = subprocess.check_output(["git", "ls-files", "-z", "--", *ENTRY_PATTERNS], cwd=root)
    return sorted(p for p in out.decode("utf-8").split("\0") if p)


def validate_path(path: str, where: str, errors: list[str]) -> None:
    if path.startswith(("/", "\\")) or ":" in path:
        errors.append(f"{where} のパス \"{path}\" は、リポジトリのルートからの相対パスで書いてください。")
    if "\\" in path:
        errors.append(f"{where} のパス \"{path}\" は / 区切りで書いてください。")
    if ".." in PurePosixPath(path).parts:
        errors.append(f"{where} のパス \"{path}\" に .. は使えません。")


def validate_entry(entry: object, index: int, ids: set[str], errors: list[str]) -> None:
    where = f"scripts[{index}]"
    if not isinstance(entry, dict):
        errors.append(f"{where} はオブジェクトで書いてください。")
        return

    for key in ("id", "name", "category", "windows"):
        if not isinstance(entry.get(key), str) or not entry.get(key):
            errors.append(f"{where} に \"{key}\"（空でない文字列）がありません。")
    if not isinstance(entry.get("id"), str):
        return

    entry_id = entry.get("id", "")
    where = f"{where}（{entry_id}）"
    if entry_id in ids:
        errors.append(f"id \"{entry_id}\" が2回以上使われています。")
    ids.add(entry_id)

    windows = entry.get("windows")
    if isinstance(windows, str) and windows:
        validate_path(windows, where, errors)
        if not windows.lower().endswith(WINDOWS_EXTENSIONS):
            errors.append(f"{where} の \"windows\" は .bat / .cmd / .ps1 / .exe のどれかにしてください。")
    mac = entry.get("mac")
    if mac is not None:
        if not isinstance(mac, str):
            errors.append(f"{where} の \"mac\" は文字列で書いてください。")
        elif mac:
            validate_path(mac, where, errors)

    for key in ("elevate", "localOnly"):
        if key in entry and not isinstance(entry[key], bool):
            errors.append(f"{where} の \"{key}\" は true か false で書いてください。")

    modes = []
    if entry.get("mode") is not None:
        if entry["mode"] not in MODES:
            errors.append(f"{where} の \"mode\" は \"console\" か \"capture\" です。")
        modes.append(entry["mode"])
    else:
        modes.append("console")

    is_batch = isinstance(windows, str) and windows.lower().endswith((".bat", ".cmd"))
    presets = entry.get("presets") or []
    if not isinstance(presets, list):
        errors.append(f"{where} の \"presets\" は配列で書いてください。")
        presets = []
    for p_index, preset in enumerate(presets):
        p_where = f"{where} の presets[{p_index}]"
        if not isinstance(preset, dict) or not isinstance(preset.get("label"), str) or not preset.get("label"):
            errors.append(f"{p_where} に \"label\"（空でない文字列）がありません。")
            continue
        args = preset.get("args") or []
        if not isinstance(args, list) or not all(isinstance(a, str) for a in args):
            errors.append(f"{p_where} の \"args\" は文字列の配列で書いてください。")
            args = []
        for arg in args:
            if any(ord(c) < 0x20 for c in arg):
                errors.append(f"{p_where} の引数に改行などの制御文字は使えません。")
            if is_batch and BATCH_UNSAFE_CHARACTERS & set(arg):
                errors.append(f"{p_where} の引数 \"{arg}\" に、bat へ渡せない文字（\" & | < > ^ % ! ( )）が入っています。")
        if preset.get("mode") is not None:
            if preset["mode"] not in MODES:
                errors.append(f"{p_where} の \"mode\" は \"console\" か \"capture\" です。")
            modes.append(preset["mode"])

    if entry.get("elevate") is True and "capture" in modes:
        errors.append(f"{where} は \"elevate\": true なので、\"mode\": \"capture\" にはできません。")


def check(root: Path, entry_files: list[str] | None = None) -> tuple[list[str], list[str]]:
    """登録ファイルを調べ、(エラー, 警告) を返す。"""
    errors: list[str] = []
    warnings: list[str] = []
    config_file = root / CONFIG_PATH
    try:
        config = json.loads(config_file.read_text(encoding="utf-8-sig"))
    except FileNotFoundError:
        return [f"{CONFIG_PATH} がありません。"], []
    except json.JSONDecodeError as error:
        return [f"{CONFIG_PATH} を JSON として読めません: {error}"], []

    if not isinstance(config, dict):
        return ["ファイルの一番外側はオブジェクトにしてください。"], []
    if config.get("version") != SUPPORTED_VERSION:
        errors.append(f"\"version\" は {SUPPORTED_VERSION} にしてください（ツールが読める版）。")
    scripts = config.get("scripts")
    if not isinstance(scripts, list):
        return errors + ["\"scripts\"（配列）がありません。"], []

    ids: set[str] = set()
    registered: set[str] = set()
    for index, entry in enumerate(scripts):
        validate_entry(entry, index, ids, errors)
        if not isinstance(entry, dict):
            continue
        for key in ("windows", "mac"):
            path = entry.get(key)
            if isinstance(path, str) and path:
                registered.add(path)
                if not entry.get("localOnly") and not (root / path).is_file():
                    warnings.append(f"登録した \"{path}\"（{entry.get('id')}）がリポジトリにありません。")

    ignored: set[str] = set()
    for index, item in enumerate(config.get("ignored") or []):
        if not isinstance(item, dict) or not isinstance(item.get("path"), str) or not isinstance(item.get("reason"), str) or not item.get("reason"):
            errors.append(f"ignored[{index}] は {{\"path\": ..., \"reason\": ...}} の形で、理由も書いてください。")
            continue
        ignored.add(item["path"])

    files = tracked_entry_files(root) if entry_files is None else entry_files
    for path in files:
        if path not in registered and path not in ignored:
            warnings.append(
                f"\"{path}\" がスクリプト管理ツールに登録されていません。{CONFIG_PATH} に足すか、"
                f"登録しない理由を \"ignored\" に書いてください。"
            )

    return errors, warnings


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--strict", action="store_true", help="警告もあれば終了コード 1 にする")
    parser.add_argument("--root", default=".", help="リポジトリのルート")
    options = parser.parse_args()

    root = Path(options.root).resolve()
    errors, warnings = check(root)
    in_actions = os.environ.get("GITHUB_ACTIONS") == "true"
    for message in errors:
        print(f"::error file={CONFIG_PATH}::{message}" if in_actions else f"エラー: {message}")
    for message in warnings:
        print(f"::warning file={CONFIG_PATH}::{message}" if in_actions else f"警告: {message}")
    if not errors and not warnings:
        print("スクリプト管理ツールの登録に問題はありません。")

    if errors or (options.strict and warnings):
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
