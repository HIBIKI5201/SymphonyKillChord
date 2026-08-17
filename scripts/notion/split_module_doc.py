"""ローカルのモジュール文書を、Notionへ書き込む形へ変換して分割する。

出力:
  <out>/body.md      モジュールページ本文（クラス一覧はトグル見出し、処理フローは見出しだけ残す）
  <out>/flow_N.md    処理フローの各節。モジュールページの子ページとして作成する
  <out>/flows.txt    子ページのタイトル一覧（1行1件）

既存ページの末尾にある <page url="...">title</page>（作成済みの子ページ）は、
--baseline で渡した現在の本文から引き継いで本文末尾へ残す。引き継がないと
本文全体の置換時に子ページを消そうとして Notion 側が拒否する。
"""
import io
import os
import re
import sys

TOC_LINE = '<table_of_contents color="gray"/>'
DECORATED_HEADINGS = ("# 概要", "# 詳細")
TOGGLE_HEADINGS = ("## 🏗️ クラス",)
FLOW_HEADING = "## 🔄処理フロー"
CHILD_PAGE_PATTERN = re.compile(r'^<page url="[^"]+">.*</page>$')
HEADING_PATTERN = re.compile(r"^#{1,6} ")
H1_PATTERN = re.compile(r"^# ")
CHILD_PAGE_MARKER = "<!--child-pages-->"


def split_sections(lines):
    """処理フロー節の各フローを本文から切り出し、(本文行, [(見出し, 本文行)]) を返す。

    節の見出しと導入文は元の位置に残し、フロー本体があった場所へ
    子ページリンクの差し込み位置（CHILD_PAGE_MARKER）を置く。
    """
    body, flows = [], []
    index = 0
    while index < len(lines):
        line = lines[index]
        if line.rstrip() != FLOW_HEADING:
            body.append(line)
            index += 1
            continue

        body.append(line)
        index += 1
        while index < len(lines) and not HEADING_PATTERN.match(lines[index]):
            body.append(lines[index])
            index += 1

        while index < len(lines) and lines[index].startswith("### "):
            title = lines[index][4:].strip()
            index += 1
            content = []
            while index < len(lines) and not HEADING_PATTERN.match(lines[index]):
                content.append(lines[index])
                index += 1
            flows.append((title, content))

        body.append(CHILD_PAGE_MARKER)

    return body, flows


def decorate(lines):
    """大見出しへ背景色を付け、クラス一覧をトグル見出しへ変換する。"""
    out = []
    index = 0
    while index < len(lines):
        stripped = lines[index].rstrip()

        # ライティング規則: H1には背景色を設定する。
        if H1_PATTERN.match(stripped) and not stripped.endswith('}'):
            out.append(stripped + ' {color="gray_bg"}')
            index += 1
            continue

        if stripped in TOGGLE_HEADINGS:
            out.append(stripped + ' {toggle="true"}')
            index += 1
            # 次の見出しまでをトグルの中身としてタブで字下げする。
            # Composition初期化情報などの下位見出しは畳まず、外に出したままにする。
            while index < len(lines) and not HEADING_PATTERN.match(lines[index]) and lines[index].rstrip() != "---":
                out.append("\t" + lines[index] if lines[index].strip() else "")
                index += 1
            continue

        out.append(lines[index])
        index += 1

    return out


def read_child_pages(baseline_path):
    if not baseline_path or not os.path.exists(baseline_path):
        return []

    text = io.open(baseline_path, encoding="utf-8").read().replace("\r\n", "\n")
    return [line for line in text.split("\n") if CHILD_PAGE_PATTERN.match(line.strip())]


def main():
    source_path, out_dir = sys.argv[1], sys.argv[2]
    baseline_path = sys.argv[3] if len(sys.argv) > 3 else None
    os.makedirs(out_dir, exist_ok=True)

    text = io.open(source_path, encoding="utf-8").read().replace("\r\n", "\n").replace("\r", "\n")
    body_lines, flows = split_sections(text.split("\n"))
    body = "\n".join(decorate(body_lines)).strip("\n")

    # 既に作成済みの子ページは、処理フロー節の位置へ置き直す。
    # 末尾へ残したままだと「既知の課題」より後ろに並んでしまう。
    child_pages = read_child_pages(baseline_path)
    body = body.replace(CHILD_PAGE_MARKER, "\n".join(child_pages) if child_pages else "")
    body = TOC_LINE + "\n---\n" + body.strip("\n") + "\n"

    io.open(os.path.join(out_dir, "body.md"), "w", encoding="utf-8", newline="").write(body)

    titles = []
    for number, (title, content) in enumerate(flows, start=1):
        titles.append(title)
        page = "# " + title + "\n" + "\n".join(content).strip("\n") + "\n"
        io.open(os.path.join(out_dir, f"flow_{number}.md"), "w", encoding="utf-8", newline="").write(page)

    io.open(os.path.join(out_dir, "flows.txt"), "w", encoding="utf-8", newline="").write("\n".join(titles))
    print(f"body.md + {len(flows)} flows")
    for number, title in enumerate(titles, start=1):
        print(f"  flow_{number}.md  {title}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
