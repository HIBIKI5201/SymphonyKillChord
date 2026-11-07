"""Notionのプロジェクト ドキュメントのミラーから、LLM・エージェントが最初に読む目録を生成する。

仕様書リポジトリ（Docs/NotionSpecifications）の全ページを走査し、次の2ファイルをプロジェクト ドキュメントの
ルートへ書き出す。どちらもページの内容が変わらなければ同じ出力になる（生成日時を含めない）。

  _catalog.md     1ページ1行。タイトル・パス・所属DB・プロパティ・見出しを並べる。rgで当たりを付ける用
  _catalog.jsonl  1ページ1行のJSON。プロパティ全件、リレーションとページ内リンクの解決先パスを含む

ファイル名を `_` で始めるのは、DB索引（_database.md）と同じく SpecSearch の検索本文から除くため。
ログにはページタイトルを出さない（同期ワークフローのログは公開される）。件数だけを表示する。

使い方（リポジトリのルートから）:
  python scripts/notion/build_spec_catalog.py --source Docs/NotionSpecifications
  python scripts/notion/build_spec_catalog.py --source Docs/NotionSpecifications --check

--check は書き込まずに、目録ファイルが今のページから作り直した目録と同じ版かを確かめる（#2396 段階1）。
目録の生成だけが失敗して古い目録が残った、手でページだけを直した、などのずれを見つける。

--lint は書き込まずに、プロジェクト ドキュメントの書き方の崩れを数える（#2359 P9）。
  python scripts/notion/build_spec_catalog.py --source Docs/NotionSpecifications --lint [--previous 前回の_catalog.jsonl] [--details]
  - 切れたリンク: ページ内の相対リンクの先が、目録に無い
  - 範囲外のメンション: メンションの先が、書き出した範囲に無い（Notion 上で消えたか、範囲の外のページ）
  - 必須属性の空き: 仕様リストの行で「カテゴリー」「サマリー」が空
  - 経緯の種別: 「経緯」の表の種別が、ライティング規則 W-09 の値（提案・変更・決定・却下）以外
  - ページの移動・消失: --previous の目録と比べて、同じ page id のパスが変わった・page id が消えた
ログには件数だけを出す。パスを見るときは --details を付けて手元で実行する（同期のログは公開されるため）。
--strict を付けると、切れたリンクがあるとき終了コード1にする。
"""
import argparse
import json
import re
import sys
import urllib.parse
from dataclasses import dataclass, field
from pathlib import Path

CATALOG_MARKDOWN = "_catalog.md"
CATALOG_JSONL = "_catalog.jsonl"
# 目録の形式を変えたら上げる。読み手が古い形式を見分けるため。
CATALOG_VERSION = 1

# 目録の1行に載せるプロパティの値の最大文字数と、見出しの最大件数。
MARKDOWN_VALUE_LIMIT = 80
MARKDOWN_HEADING_LIMIT = 12
MARKDOWN_HEADING_LEVEL = 2
JSONL_HEADING_LEVEL = 3

EXCLUDED_DIRECTORIES = {"assets", ".git"}
EXCLUDED_ROOT_FILES = {"README.md"}

PAGE_ID_RE = re.compile(r"([0-9a-f]{32})(?:[/?#]|$)", re.IGNORECASE)
UUID_RE = re.compile(r"\b[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\b", re.IGNORECASE)
NOTION_LINK_RE = re.compile(r"^\[Notionで開く\]\((?P<url>https://[^)\s]+)\)\s*$")
LAST_EDITED_RE = re.compile(r"^最終更新:\s*(?P<value>.*)$")
HEADING_RE = re.compile(r"^(?P<marks>#{1,6})[ \t]+(?P<text>.+?)[ \t]*$")
FENCE_RE = re.compile(r"^[ \t]*(?P<fence>`{3,}|~{3,})")
PROPERTY_ROW_RE = re.compile(r"^\|\s*(?P<name>[^|]+?)\s*\|\s*(?P<value>.*?)\s*\|\s*$")
MENTION_RE = re.compile(r'<mention-(?:page|database)\s+url="(?P<url>[^"]+)"')
RELATIVE_LINK_RE = re.compile(r"\]\((?P<target>[^)\s]+\.md)\)")
ATTRIBUTE_RE = re.compile(r"\s*\{[a-z_]+=\"[^\"]*\"\}\s*$")
TAG_RE = re.compile(r"<[^>]+>")

# 検査（--lint）で使う値。
REQUIRED_PROPERTIES = {"仕様リスト": ("カテゴリー", "サマリー")}
HISTORY_HEADING = "経緯"
HISTORY_KINDS = {"提案", "変更", "決定", "却下"}
TABLE_CELL_RE = re.compile(r"<td>(?P<value>.*?)</td>", re.DOTALL)


@dataclass
class Page:
    """目録の1行になるページ。"""

    path: str
    title: str
    page_id: str = ""
    url: str = ""
    last_edited: str = ""
    database: str = ""
    properties: dict = field(default_factory=dict)
    headings: list = field(default_factory=list)
    raw_links: list = field(default_factory=list)
    characters: int = 0
    # 本文が始まる行番号（前置きの直後）。spec_query.py が本文だけを読むときに使う。目録には出さない。
    body_start: int = 0


@dataclass
class Database:
    """DB索引（_database.md）。所属DB名と行数の集計に使う。"""

    path: str
    title: str
    directory: str
    rows: int = 0


def extract_page_id(value):
    """Notion URL・ハイフン付きUUIDから、ハイフンなしのページIDを取り出す。"""
    value = value.strip()
    uuid = UUID_RE.fullmatch(value)
    if uuid:
        return value.replace("-", "").lower()
    match = PAGE_ID_RE.search(urllib.parse.urlsplit(value).path + "?")
    return match.group(1).lower() if match else ""


def clean_text(text):
    """見出し・プロパティの値から装飾を取り除き、1行にする。"""
    text = ATTRIBUTE_RE.sub("", text)
    text = text.replace("<br>", " ").replace("<br/>", " ")
    text = TAG_RE.sub("", text)
    text = text.replace("**", "").replace("__", "")
    return re.sub(r"\s+", " ", text).strip()


def parse_page(text, relative_path):
    """Exporterの書式（自動生成コメント→タイトル→Notionリンク→最終更新→プロパティ表→本文）を読む。"""
    lines = text.replace("\r\n", "\n").split("\n")
    page = Page(path=relative_path, title=Path(relative_path).stem)
    index = 0
    has_title = False

    # 前置き: 本文最初の行までにタイトル・Notionリンク・最終更新・プロパティ表が並ぶ。
    while index < len(lines):
        line = lines[index]
        stripped = line.strip()
        if not stripped or (stripped.startswith("<!--") and stripped.endswith("-->")):
            index += 1
            continue
        if line.startswith("# ") and not has_title:
            page.title = clean_text(line[2:]) or page.title
            has_title = True
            index += 1
            continue
        link = NOTION_LINK_RE.match(stripped)
        if link:
            page.url = link.group("url")
            page.page_id = extract_page_id(page.url)
            index += 1
            continue
        edited = LAST_EDITED_RE.match(stripped)
        if edited:
            page.last_edited = edited.group("value").strip()
            index += 1
            continue
        if stripped == "## プロパティ":
            index = parse_properties(lines, index + 1, page.properties)
            continue
        if stripped == "---":
            index += 1
            break
        break

    page.body_start = index
    body = lines[index:]
    page.characters = sum(len(line.strip()) for line in body)
    page.headings = parse_headings(body)
    for line in body:
        page.raw_links.extend(("id", match.group("url")) for match in MENTION_RE.finditer(line))
        page.raw_links.extend(("path", match.group("target")) for match in RELATIVE_LINK_RE.finditer(line))
    return page


def parse_properties(lines, index, properties):
    """プロパティ表の行を読み、表の直後の行番号を返す。見出し行と区切り行は除く。"""
    while index < len(lines):
        stripped = lines[index].strip()
        if not stripped:
            index += 1
            continue
        if not stripped.startswith("|"):
            break
        row = PROPERTY_ROW_RE.match(stripped)
        if row and not set(row.group("name")) <= set("-: ") and row.group("name") != "プロパティ":
            properties[row.group("name").strip()] = row.group("value").strip()
        index += 1
    return index


def parse_headings(body):
    """コードブロック内を除いた見出しを (レベル, テキスト) で返す。"""
    headings = []
    fence = ""
    for line in body:
        fence_match = FENCE_RE.match(line)
        if fence_match:
            marker = fence_match.group("fence")
            if not fence:
                fence = marker
            elif marker[0] == fence[0] and len(marker) >= len(fence):
                fence = ""
            continue
        if fence:
            continue
        heading = HEADING_RE.match(line)
        if heading and len(heading.group("marks")) <= JSONL_HEADING_LEVEL:
            text = clean_text(heading.group("text"))
            if text:
                headings.append((len(heading.group("marks")), text))
    return headings


def is_catalog_target(relative):
    """検索本文と同じく、DB索引・添付・READMEを目録のページから除く。"""
    parts = relative.parts
    if any(part in EXCLUDED_DIRECTORIES for part in parts[:-1]):
        return False
    if len(parts) == 1 and parts[0] in EXCLUDED_ROOT_FILES:
        return False
    return not parts[-1].startswith("_")


def collect(source):
    """プロジェクト ドキュメントのルートからページとDB索引を集め、所属DBとリンク先を解決する。"""
    pages = []
    databases = []
    for file in sorted(source.rglob("*.md"), key=lambda path: path.relative_to(source).as_posix()):
        relative = file.relative_to(source)
        if any(part in EXCLUDED_DIRECTORIES for part in relative.parts[:-1]):
            continue
        text = file.read_text(encoding="utf-8")
        if relative.name == "_database.md":
            parsed = parse_page(text, relative.as_posix())
            databases.append(Database(relative.as_posix(), parsed.title, relative.parent.as_posix()))
            continue
        if is_catalog_target(relative):
            pages.append(parse_page(text, relative.as_posix()))

    database_by_directory = {database.directory: database for database in databases}
    for page in pages:
        database = database_by_directory.get(Path(page.path).parent.as_posix())
        if database:
            page.database = database.title
            database.rows += 1
    return pages, databases


def resolve_links(pages):
    """リレーションのID・メンション・相対リンクを、目録内のページのパスへ置き換える。"""
    path_by_id = {page.page_id: page.path for page in pages if page.page_id}
    known_paths = {page.path for page in pages}
    resolved = {}
    for page in pages:
        links = []
        for kind, value in page.raw_links:
            if kind == "id":
                target = path_by_id.get(extract_page_id(value), "")
            else:
                joined = (Path(page.path).parent / urllib.parse.unquote(value)).as_posix()
                target = normalize_path(joined)
            if target and target != page.path and target in known_paths and target not in links:
                links.append(target)
        relations = {}
        for name, value in page.properties.items():
            ids = UUID_RE.findall(value)
            targets = [path_by_id[extract_page_id(item)] for item in ids if extract_page_id(item) in path_by_id]
            if targets:
                relations[name] = targets
        resolved[page.path] = (links, relations)
    return resolved


def normalize_path(path):
    """`..` を含む相対パスをプロジェクト ドキュメントのルートからのパスへ畳む。"""
    parts = []
    for part in path.split("/"):
        if part in ("", "."):
            continue
        if part == "..":
            if parts:
                parts.pop()
            continue
        parts.append(part)
    return "/".join(parts)


def property_display(page, relations, title_by_path):
    """目録の1行に載せるプロパティ。空の値を除き、リレーションはタイトルに置き換える。"""
    items = []
    for name, value in page.properties.items():
        if name in relations:
            value = "、".join(title_by_path[path] for path in relations[name])
        value = clean_text(value)
        if not value or UUID_RE.search(value):
            continue
        if len(value) > MARKDOWN_VALUE_LIMIT:
            value = value[:MARKDOWN_VALUE_LIMIT] + "…"
        items.append(f"{name}: {value}")
    return items


def escape_inline(text):
    """表示文字列が目録の書式を壊さないよう、リンク記号と改行を逃がす。"""
    return text.replace("[", "［").replace("]", "］").replace("\n", " ")


def build_markdown(pages, databases, resolved):
    """セクション（プロジェクト ドキュメントのルート直下の2階層目）ごとに、1ページ1行の目録を作る。"""
    title_by_path = {page.path: page.title for page in pages}
    lines = [
        "<!-- build_spec_catalog.py による自動生成ファイルです。手で編集しないでください。 -->",
        "# プロジェクト ドキュメントの目録",
        "",
        f"形式バージョン: {CATALOG_VERSION} / ページ数: {len(pages)} / DB数: {len(databases)}",
        "",
        "Notionのプロジェクト ドキュメントの全ページを1行ずつ並べた目録です。`rg` でキーワード・プロパティ値を検索して当たりを付け、"
        "該当ページだけを開いて読んでください。",
        "",
        "- 1行の形式: `- [タイトル](<パス>) 〔所属DB〕 プロパティ名: 値 ／ … § 見出し / 見出し`",
        f"- 見出しは本文のH1〜H{MARKDOWN_HEADING_LEVEL}を最大{MARKDOWN_HEADING_LIMIT}件、プロパティ値は{MARKDOWN_VALUE_LIMIT}文字までです。",
        "- プロパティ全件・H3までの見出し・リンク先のパスは `_catalog.jsonl`（1ページ1行のJSON）にあります。",
        "- 仕様の正本はNotionです。ページの「最終更新」が古いものは現在の仕様と違う可能性があります。",
        "",
        "## DB一覧",
        "",
    ]
    for database in sorted(databases, key=lambda item: item.path):
        if database.rows:
            lines.append(f"- [{escape_inline(database.title)}](<{database.path}>) {database.rows}件")
    sections = {}
    for page in pages:
        parts = page.path.split("/")
        section = "/".join(parts[:2]) if len(parts) > 2 else (parts[0] if len(parts) > 1 else "(ルート)")
        sections.setdefault(section, []).append(page)
    for section in sorted(sections):
        lines.extend(["", f"## {section}", ""])
        for page in sections[section]:
            links, relations = resolved[page.path]
            line = f"- [{escape_inline(page.title)}](<{page.path}>)"
            if page.database:
                line += f" 〔{escape_inline(page.database)}〕"
            properties = property_display(page, relations, title_by_path)
            if properties:
                line += " " + " ／ ".join(escape_inline(item) for item in properties)
            headings = [text for level, text in page.headings if level <= MARKDOWN_HEADING_LEVEL]
            if headings:
                shown = headings[:MARKDOWN_HEADING_LIMIT]
                suffix = " …" if len(headings) > len(shown) else ""
                line += " § " + " / ".join(escape_inline(text) for text in shown) + suffix
            lines.append(line)
    return "\n".join(lines) + "\n"


def build_jsonl(pages, resolved):
    """1ページ1行のJSONを作る。キーの順序を固定し、差分が内容の変化だけになるようにする。"""
    rows = []
    for page in pages:
        links, relations = resolved[page.path]
        row = {
            "path": page.path,
            "title": page.title,
            "page_id": page.page_id,
            "url": page.url,
            "last_edited": page.last_edited,
            "database": page.database,
            "properties": {name: value for name, value in page.properties.items() if value},
            "relations": relations,
            "headings": [{"level": level, "text": text} for level, text in page.headings],
            "links": links,
            "characters": page.characters,
        }
        rows.append(json.dumps(row, ensure_ascii=False, separators=(",", ":")))
    return "\n".join(rows) + "\n"


def write_if_changed(path, content):
    """内容が同じなら書き込まない。改行はLFに固定する。"""
    if path.exists() and path.read_text(encoding="utf-8") == content:
        return False
    with open(path, "w", encoding="utf-8", newline="\n") as file:
        file.write(content)
    return True


def compare_catalog(source, markdown, jsonl):
    """目録ファイルが今のページから作り直した目録と同じ版かを比べ、ずれの件数を返す。

    ページタイトルは返さない（同期のログは公開リポジトリで誰でも読めるため、件数だけを出す）。
    """
    expected = {json.loads(line)["path"]: line for line in jsonl.splitlines() if line.strip()}
    jsonl_file = source / CATALOG_JSONL
    actual = {}
    if jsonl_file.is_file():
        for line in jsonl_file.read_text(encoding="utf-8").splitlines():
            if line.strip():
                actual[json.loads(line)["path"]] = line
    markdown_file = source / CATALOG_MARKDOWN
    return {
        "missing": len(expected.keys() - actual.keys()),
        "extra": len(actual.keys() - expected.keys()),
        "changed": sum(1 for path in expected.keys() & actual.keys() if expected[path] != actual[path]),
        "markdown": not markdown_file.is_file() or markdown_file.read_text(encoding="utf-8") != markdown,
        "no_file": not jsonl_file.is_file(),
    }


def history_kinds(text):
    """「経緯」見出しの直後の表から、種別の列（2列目）の値を見出し行を除いて返す。"""
    kinds = []
    lines = text.replace("\r\n", "\n").split("\n")
    index = 0
    while index < len(lines):
        heading = HEADING_RE.match(lines[index])
        index += 1
        if not heading or clean_text(heading.group("text")) != HISTORY_HEADING:
            continue
        table = []
        while index < len(lines) and not HEADING_RE.match(lines[index]):
            table.append(lines[index])
            index += 1
        rows = re.findall(r"<tr>(.*?)</tr>", "\n".join(table), re.DOTALL)
        for row in rows[1:]:
            cells = [clean_text(cell) for cell in TABLE_CELL_RE.findall(row)]
            if len(cells) >= 2:
                kinds.append(cells[1])
    return kinds


def lint_catalog(source, pages, databases, previous_rows=None):
    """プロジェクト ドキュメントの書き方の崩れを数え、件数と該当パスを返す（#2359 P9）。"""
    known_paths = {page.path for page in pages} | {database.path for database in databases}
    known_ids = {page.page_id for page in pages if page.page_id}
    found = {"broken_links": [], "outside_mentions": [], "missing_properties": [], "history_kinds": [],
             "moved_pages": [], "removed_pages": []}
    for page in pages:
        for kind, value in page.raw_links:
            if kind == "id":
                page_id = extract_page_id(value)
                if page_id and page_id not in known_ids:
                    found["outside_mentions"].append(page.path)
                continue
            if "://" in value or value.startswith("mailto:"):
                continue
            target = normalize_path((Path(page.path).parent / urllib.parse.unquote(value)).as_posix())
            if target not in known_paths:
                found["broken_links"].append(f"{page.path} -> {target}")
        for name in REQUIRED_PROPERTIES.get(page.database, ()):
            if not page.properties.get(name, "").strip():
                found["missing_properties"].append(f"{page.path} ({name})")
        text = (source / page.path).read_text(encoding="utf-8")
        for value in history_kinds(text):
            if value not in HISTORY_KINDS:
                found["history_kinds"].append(f"{page.path} ({value or '空'})")
    if previous_rows is not None:
        current = {page.page_id: page.path for page in pages if page.page_id}
        for row in previous_rows:
            page_id, path = row.get("page_id", ""), row.get("path", "")
            if not page_id:
                continue
            if page_id not in current:
                found["removed_pages"].append(path)
            elif current[page_id] != path:
                found["moved_pages"].append(f"{path} -> {current[page_id]}")
    return found


LINT_LABELS = {
    "broken_links": "切れたリンク",
    "outside_mentions": "範囲外のメンション",
    "missing_properties": "必須属性の空き",
    "history_kinds": "経緯の種別がW-09以外",
    "moved_pages": "移動したページ",
    "removed_pages": "消えたページ",
}


def read_jsonl(path):
    """目録のJSONLを読む。ファイルが無いか空ならNone。"""
    if not path or not path.is_file():
        return None
    rows = [json.loads(line) for line in path.read_text(encoding="utf-8").splitlines() if line.strip()]
    return rows or None


def main(argv=None):
    parser = argparse.ArgumentParser(description="Notionのプロジェクト ドキュメントのミラーの目録を生成する。")
    parser.add_argument("--source", required=True, type=Path, help="仕様書リポジトリのルート")
    parser.add_argument("--check", action="store_true",
                        help="書き込まず、目録ファイルが今のページと同じ版かを確かめる（ずれていれば終了コード1）")
    parser.add_argument("--lint", action="store_true", help="書き込まず、プロジェクト ドキュメントの書き方の崩れを数える（#2359 P9）")
    parser.add_argument("--previous", type=Path, help="--lint で比べる前回の _catalog.jsonl（ページの移動・消失を数える）")
    parser.add_argument("--details", action="store_true", help="--lint で該当パスも表示する（公開ログでは付けない）")
    parser.add_argument("--strict", action="store_true", help="--lint で切れたリンクがあれば終了コード1にする")
    args = parser.parse_args(argv)
    source = args.source.resolve()
    if not source.is_dir():
        print(f"プロジェクト ドキュメントのディレクトリが見つかりません: {args.source}", file=sys.stderr)
        return 1

    pages, databases = collect(source)
    resolved = resolve_links(pages)
    markdown, jsonl = build_markdown(pages, databases, resolved), build_jsonl(pages, resolved)
    if args.lint:
        found = lint_catalog(source, pages, databases, read_jsonl(args.previous))
        summary = "、".join(f"{LINT_LABELS[key]} {len(items)} 件" for key, items in found.items()
                           if args.previous or key not in ("moved_pages", "removed_pages"))
        print(f"プロジェクト ドキュメントの検査（ページ {len(pages)} 件）: {summary}。")
        if args.details:
            for key, items in found.items():
                for item in items:
                    print(f"  {LINT_LABELS[key]}: {item}")
        return 1 if args.strict and found["broken_links"] else 0
    if args.check:
        diff = compare_catalog(source, markdown, jsonl)
        if diff["no_file"]:
            print(f"{CATALOG_JSONL} がありません。", file=sys.stderr)
            return 1
        if diff["missing"] or diff["extra"] or diff["changed"] or diff["markdown"]:
            print(f"目録がページと別の版です（目録に無いページ {diff['missing']} 件、消えたページ {diff['extra']} 件、"
                  f"内容が違う行 {diff['changed']} 件、{CATALOG_MARKDOWN} の不一致 {'あり' if diff['markdown'] else 'なし'}）。"
                  "build_spec_catalog.py を --check なしで実行して作り直す。", file=sys.stderr)
            return 1
        print(f"目録はページと同じ版です（ページ {len(pages)} 件）。")
        return 0
    changed = [
        write_if_changed(source / CATALOG_MARKDOWN, markdown),
        write_if_changed(source / CATALOG_JSONL, jsonl),
    ]
    status = "目録を更新しました" if any(changed) else "目録に変更はありませんでした"
    print(f"{status}（ページ {len(pages)} 件、DB {len(databases)} 件）。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
