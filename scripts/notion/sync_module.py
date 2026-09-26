"""ローカルのモジュール文書1本を、Notionのモジュールページへ反映する。

手順:
  1. pull して現在の本文を取得する
  2. 処理フローを子ページ用に切り出す
  3. まだ無いフローだけを子ページとして作成する（既存タイトルは作り直さない）
  4. 子ページ作成後の本文を取り直し、リンクを処理フロー節の位置へ置いた本文を push する

使い方:
  python sync_module.py <ローカル文書.md> <NotionページIDまたはエクスポート済み.mdパス> <作業ディレクトリ>
"""
import io
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, "..", ".."))
WRITER = os.path.join(REPO, "SinfoniaOperator", "NotionMarkdownWriter.exe")
SPLIT = os.path.join(HERE, "split_module_doc.py")
WORK_FILE_PATTERN = re.compile(r"作業ファイル: (.+)")
CHILD_TITLE_PATTERN = re.compile(r'^<page url="([^"]+)">(.*)</page>$')


def run(args, quiet=False):
    result = subprocess.run(args, cwd=REPO, capture_output=True, text=True, encoding="utf-8")
    output = (result.stdout or "") + (result.stderr or "")
    if not quiet:
        for line in output.splitlines():
            if "OperatorConfig" not in line and line.strip():
                print("   " + line)
    return result.returncode, output


def pull(target):
    code, output = run([WRITER, "pull", target], quiet=True)
    if code != 0:
        raise SystemExit("pullに失敗しました:\n" + output)

    match = WORK_FILE_PATTERN.search(output)
    if not match:
        raise SystemExit("作業ファイルのパスを取得できませんでした:\n" + output)

    return match.group(1).strip()


def existing_children(work_file):
    """作成済みの子ページを {タイトル: URL} で返す。"""
    text = io.open(work_file, encoding="utf-8").read().replace("\r\n", "\n")
    children = {}
    for line in text.split("\n"):
        match = CHILD_TITLE_PATTERN.match(line.strip())
        if match:
            children[match.group(2)] = match.group(1)
    return children


def update_child_page(url, flow_path, title):
    """既存の子ページ本文を、切り出したフロー内容で置き換える。"""
    child_work = pull(url)

    # 先頭のH1はページタイトルとして消費済みのため、本文からは外す。
    lines = io.open(flow_path, encoding="utf-8").read().replace("\r\n", "\n").split("\n")
    body = "\n".join(lines[1:]).strip("\n") + "\n"
    io.open(child_work, "w", encoding="utf-8", newline="").write(body)

    code, output = run([WRITER, "push", child_work, "--whole", "--confirm", "--quiet"], quiet=True)
    if code != 0:
        raise SystemExit(f"子ページの更新に失敗しました（{title}）:\n" + output)
    return "変更なし" not in output


def split(source, out_dir, baseline):
    code, _ = run([sys.executable, SPLIT, source, out_dir, baseline], quiet=True)
    if code != 0:
        raise SystemExit("分割に失敗しました")

    flows_path = os.path.join(out_dir, "flows.txt")
    text = io.open(flows_path, encoding="utf-8").read() if os.path.exists(flows_path) else ""
    return [title for title in text.split("\n") if title.strip()]


def main():
    source, target, out_dir = sys.argv[1], sys.argv[2], sys.argv[3]
    name = os.path.splitext(os.path.basename(source))[0]
    print(f"[{name}] 取得")
    work_file = pull(target)
    page_id = os.path.basename(work_file).split("-")[0]

    titles = split(source, out_dir, work_file)
    existing = existing_children(work_file)
    missing = [(index + 1, title) for index, title in enumerate(titles) if title not in existing]

    if missing:
        print(f"[{name}] 子ページを{len(missing)}件作成")
        for number, title in missing:
            flow_path = os.path.join(out_dir, f"flow_{number}.md")
            code, output = run([WRITER, "create", flow_path, "--parent", target, "--confirm"], quiet=True)
            if code != 0:
                raise SystemExit(f"子ページの作成に失敗しました（{title}）:\n" + output)
            print(f"   作成: {title}")

        work_file = pull(target)
        split(source, out_dir, work_file)
        existing = existing_children(work_file)

    # 既存の子ページも本文を追従させる。作成時のまま放置すると内容が古くなる。
    updated = 0
    for number, title in enumerate(titles, start=1):
        if title not in existing:
            continue
        if update_child_page(existing[title], os.path.join(out_dir, f"flow_{number}.md"), title):
            updated += 1
    if updated:
        print(f"[{name}] 子ページを{updated}件更新")

    body = io.open(os.path.join(out_dir, "body.md"), encoding="utf-8").read()
    io.open(work_file, "w", encoding="utf-8", newline="").write(body)

    print(f"[{name}] 本文を反映")
    code, output = run([WRITER, "push", work_file, "--whole", "--confirm", "--quiet"], quiet=True)
    if code != 0:
        raise SystemExit("pushに失敗しました:\n" + output)

    for line in output.splitlines():
        if "反映しました" in line or "一致しません" in line:
            print("   " + line.strip())

    print(f"[{name}] 完了 (page {page_id}, flows {len(titles)})")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
