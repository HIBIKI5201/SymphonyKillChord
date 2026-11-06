"""Notion仕様書ミラーを、目録で探して（search）必要な節だけ読む（get）ための窓口。

仕様書改定方針（#2396）の段階2。AI・エージェントが仕様を読むとき、仕様書ミラーを全文検索して
ページを丸ごと読む代わりに、次の二段階で必要な範囲だけを取り出す。

  search  目録（_catalog.jsonl）をキーワードとプロパティで絞り込み、ページID・タイトル・サマリーを返す
  get     1ページの「仕様」の節（仕様概要・詳細説明）を返す。親仕様・子仕様・関連ページを添える
          --rationale で仕様意図（経緯・却下・保留した案）を、--status で実装リストの実装状態を足す

目録がない版の仕様書ミラーでも動くよう、そのときは build_spec_catalog.py と同じ処理で目録をその場で作る。
本文の <mention-page> はページ名とパスに置き換えて返す（書き出すとページ名が落ちる問題 #2483 D-14 を読む側で補う）。

使い方（リポジトリのルートから）:
  python scripts/notion/spec_query.py search 回避
  python scripts/notion/spec_query.py search --category バトルパート --parent プレイヤー
  python scripts/notion/spec_query.py get 回避アクション --rationale --status
  python scripts/notion/spec_query.py get 33a7c2c6cc028053a4fbf138ffac7b74 --json
"""
import argparse
import json
import re
import sys
import urllib.parse
from dataclasses import dataclass, field
from pathlib import Path

from build_spec_catalog import (
    CATALOG_JSONL,
    RELATIVE_LINK_RE,
    FENCE_RE,
    HEADING_RE,
    UUID_RE,
    build_jsonl,
    clean_text,
    collect,
    extract_page_id,
    normalize_path,
    parse_page,
    resolve_links,
)

DEFAULT_SOURCE = Path(__file__).resolve().parents[2] / "Docs" / "NotionSpecifications"

# 仕様リストのカテゴリー「開発用」は旧版・参考として、検索の既定から外す（#2396 段階0）。
DEVELOPMENT_CATEGORIES = {"開発用"}
CATEGORY_PROPERTIES = ("カテゴリー", "種別")
SUMMARY_PROPERTIES = ("サマリー", "概要")
SCOPE_PROPERTY = "適用範囲"
PARENT_PROPERTIES = ("親仕様", "親")
STATUS_DATABASE = "実装リスト"
STATUS_RELATION = "仕様"
STATUS_PAGE_SUFFIX = " 実装状況"

# W-09 のページ構成。H1の見出しで節の種別を決める（#2396 段階1の判定規則）。
SPEC_HEADINGS = ("仕様概要", "詳細説明")
RATIONALE_HEADINGS = ("仕様意図",)
# 仕様の節の中にあっても、制作意図として扱う見出し。
RATIONALE_SUBHEADINGS = ("経緯", "却下", "保留した案", "変更履歴")
KIND_SPEC = "仕様"
KIND_RATIONALE = "制作意図"

# search の採点。タイトル・サマリーに当たったページを上に出す。
SCORE_TITLE = 8
SCORE_SUMMARY = 4
SCORE_PROPERTY = 2
SCORE_HEADING = 1
# タイトルが語と完全に一致したときと、仕様の正本DBにあるときの加点。タスクやアセットの行より上に出す。
SCORE_EXACT_TITLE = 4
SCORE_PRIMARY_DATABASE = 8
PRIMARY_DATABASES = ("仕様リスト", "システムリスト", "用語リスト")
# get でタイトルが重なったとき、この順で仕様の正本を選ぶ（実装リストや用語リストに同名の行がある）。
SPEC_DATABASES = ("仕様リスト", "システムリスト")
DEFAULT_LIMIT = 20

MENTION_TAG_RE = re.compile(r'<mention-(?:page|database)\s+url="(?P<url>[^"]+)"\s*/?>(?:</mention-(?:page|database)>)?')
TABLE_OF_CONTENTS_RE = re.compile(r"^\s*<table_of_contents[^>]*/>\s*$")
TABLE_START_RE = re.compile(r"^\s*<table(?:\s[^>]*)?>\s*$")
TABLE_END_RE = re.compile(r"^\s*</table>\s*$")
TABLE_ROW_RE = re.compile(r"<tr[^>]*>(?P<row>.*?)</tr>", re.DOTALL)
TABLE_CELL_RE = re.compile(r"<td[^>]*>(?P<cell>.*?)</td>", re.DOTALL)


@dataclass
class Section:
    """本文のH1単位の節。種別は仕様か制作意図。"""

    heading: str
    kind: str
    lines: list = field(default_factory=list)

    @property
    def text(self):
        return "\n".join(self.lines).strip("\n")


class Catalog:
    """目録の行（1ページ1件）と、パス・ID・タイトルからの引き当て。"""

    def __init__(self, source, rows, from_file):
        self.source = source
        self.rows = rows
        self.from_file = from_file
        self.by_path = {row["path"]: row for row in rows}
        self.by_id = {row["page_id"]: row for row in rows if row.get("page_id")}

    def title(self, path):
        row = self.by_path.get(path)
        return row["title"] if row else path

    def summary(self, row):
        return first_property(row, SUMMARY_PROPERTIES)

    def children(self, row):
        """親仕様リレーションでこのページを指す行と、Notion上の子ページ（同名フォルダ内）を返す。"""
        result = []
        for other in self.rows:
            if any(row["path"] in other.get("relations", {}).get(name, []) for name in PARENT_PROPERTIES):
                result.append(other)
        folder = row["path"][: -len(".md")] + "/" if row["path"].endswith(".md") else None
        if folder:
            for other in self.rows:
                rest = other["path"][len(folder):] if other["path"].startswith(folder) else ""
                if rest and "/" not in rest and other not in result and not other["path"].endswith("/_database.md"):
                    result.append(other)
        return result

    def parents(self, row):
        paths = []
        for name in PARENT_PROPERTIES:
            paths.extend(path for path in row.get("relations", {}).get(name, []) if path not in paths)
        return [self.by_path[path] for path in paths if path in self.by_path]

    def status_rows(self, row):
        """実装リストの行のうち、仕様リレーションでこのページを指すもの。無ければ子ページ「〇〇 実装状況」。"""
        result = [
            other for other in self.rows
            if other.get("database") == STATUS_DATABASE and row["path"] in other.get("relations", {}).get(STATUS_RELATION, [])
        ]
        if result:
            return result
        return [child for child in self.children(row) if child["title"] == row["title"] + STATUS_PAGE_SUFFIX]


def load_catalog(source):
    """仕様書ルートの _catalog.jsonl を読む。無ければ目録をその場で作る（ファイルには書かない）。"""
    catalog_file = source / CATALOG_JSONL
    if catalog_file.is_file():
        text = catalog_file.read_text(encoding="utf-8")
        from_file = True
    else:
        pages, _ = collect(source)
        text = build_jsonl(pages, resolve_links(pages))
        from_file = False
    rows = [json.loads(line) for line in text.splitlines() if line.strip()]
    return Catalog(source, rows, from_file)


def first_property(row, names):
    properties = row.get("properties", {})
    for name in names:
        value = clean_text(properties.get(name, ""))
        if value:
            return value
    return ""


def property_values(catalog, row):
    """表示用のプロパティ。リレーションはタイトルに置き換え、ページIDだけの値は出さない。"""
    values = {}
    relations = row.get("relations", {})
    for name, value in row.get("properties", {}).items():
        if name in relations:
            value = "、".join(catalog.title(path) for path in relations[name])
        value = clean_text(value)
        if value and not UUID_RE.search(value):
            values[name] = value
    return values


def matches_text(value, wanted):
    return wanted.casefold() in value.casefold()


def resolve_parent_paths(catalog, parent):
    """--parent の値（ID・パス・タイトル）を、目録のパスの集合にする。"""
    page_id = extract_page_id(parent)
    if page_id and page_id in catalog.by_id:
        return {catalog.by_id[page_id]["path"]}
    if parent in catalog.by_path:
        return {parent}
    return {row["path"] for row in catalog.rows if row["title"] == parent}


def filter_rows(catalog, args):
    """プロパティ・所属DBで絞り込む。適用範囲が未設定のページは、既定値が決まるまで全範囲に含める。"""
    parent_paths = resolve_parent_paths(catalog, args.parent) if args.parent else None
    result = []
    for row in catalog.rows:
        category = first_property(row, CATEGORY_PROPERTIES)
        if not args.include_dev and category in DEVELOPMENT_CATEGORIES:
            continue
        if args.category and category != args.category:
            continue
        if args.db and not matches_text(row.get("database", ""), args.db):
            continue
        if args.scope:
            scope = clean_text(row.get("properties", {}).get(SCOPE_PROPERTY, ""))
            if scope and args.scope not in [item.strip() for item in re.split(r"[,、]", scope)]:
                continue
        if parent_paths is not None:
            parents = {path for name in PARENT_PROPERTIES for path in row.get("relations", {}).get(name, [])}
            if not parents & parent_paths:
                continue
        result.append(row)
    return result


def score_row(catalog, row, terms):
    """全語が当たったときだけ点を付ける（AND）。当たらなければ0。"""
    title = row["title"]
    summary = catalog.summary(row)
    properties = " ".join(property_values(catalog, row).values())
    headings = " ".join(item["text"] for item in row.get("headings", []))
    total = 0
    for term in terms:
        score = 0
        if matches_text(title, term):
            score += SCORE_TITLE
        if matches_text(summary, term):
            score += SCORE_SUMMARY
        if matches_text(properties, term):
            score += SCORE_PROPERTY
        if matches_text(headings, term):
            score += SCORE_HEADING
        if score == 0:
            return 0
        total += score
    if any(title.casefold() == term.casefold() for term in terms):
        total += SCORE_EXACT_TITLE
    if row.get("database") in PRIMARY_DATABASES:
        total += SCORE_PRIMARY_DATABASE
    return total


def body_contains(catalog, row, terms):
    """目録で見つからないときの全文検索。前置き（プロパティ表）を除いた本文で判定する。"""
    file = catalog.source / row["path"]
    if not file.is_file():
        return False
    _, body = read_body(file, row["path"])
    text = "\n".join(body).casefold()
    return all(term.casefold() in text for term in terms)


def search(catalog, args):
    """目録で絞り込み、点の高い順に返す。0件なら本文の全文検索に切り替える。"""
    rows = filter_rows(catalog, args)
    terms = args.query
    if not terms:
        hits = [(0, row) for row in rows]
        mode = "目録"
    else:
        hits = [(score_row(catalog, row, terms), row) for row in rows]
        hits = [hit for hit in hits if hit[0] > 0]
        mode = "目録"
        if not hits and not args.no_fallback:
            hits = [(0, row) for row in rows if body_contains(catalog, row, terms)]
            mode = "本文"
    hits.sort(key=lambda hit: (-hit[0], hit[1]["path"]))
    return mode, len(hits), [row for _, row in hits[: args.limit]]


def read_body(file, relative_path):
    text = file.read_text(encoding="utf-8")
    page = parse_page(text, relative_path)
    lines = text.replace("\r\n", "\n").split("\n")
    return page, lines[page.body_start:]


def split_sections(body):
    """本文をH1で区切り、W-09 の見出しで種別を付ける。W-09 でないページは全体を仕様として返す。"""
    sections = []
    current = Section("", KIND_SPEC)
    parent = current
    fence = ""
    for line in body:
        if TABLE_OF_CONTENTS_RE.match(line):
            continue
        fence_match = FENCE_RE.match(line)
        if fence_match:
            marker = fence_match.group("fence")
            if not fence:
                fence = marker
            elif marker[0] == fence[0] and len(marker) >= len(fence):
                fence = ""
        heading = None if fence or fence_match else HEADING_RE.match(line)
        if heading and len(heading.group("marks")) == 1:
            sections.append(current)
            text = clean_text(heading.group("text"))
            kind = KIND_RATIONALE if any(name in text for name in RATIONALE_HEADINGS) else KIND_SPEC
            current = Section(text, kind, [line])
            parent = current
            continue
        if heading and len(heading.group("marks")) == 2 and parent.kind == KIND_SPEC:
            # 仕様の節の中の「経緯」などは制作意図として切り出し、次のH2で仕様の節へ戻す。
            text = clean_text(heading.group("text"))
            is_rationale = any(name in text for name in RATIONALE_SUBHEADINGS)
            if is_rationale or current is not parent and current.kind == KIND_RATIONALE:
                sections.append(current)
                label = f"{parent.heading} > {text}" if parent.heading else text
                current = Section(label, KIND_RATIONALE if is_rationale else KIND_SPEC, [line])
                continue
        current.lines.append(line)
    sections.append(current)
    return [section for section in sections if section.text.strip()]


def replace_mentions(catalog, text, page_path=""):
    """<mention-page> を [タイトル](<パス>) に、ページ内の相対リンクを仕様書ルートからのパスに置き換える。
    目録に無いページはURLを残す。"""

    def replace_mention(match):
        url = match.group("url")
        row = catalog.by_id.get(extract_page_id(url))
        if row:
            return f"[{row['title']}](<{row['path']}>)"
        return f"[目録に無いページ]({url})"

    def replace_link(match):
        target = match.group("target")
        if "://" in target:
            return match.group(0)
        path = normalize_path((Path(page_path).parent / urllib.parse.unquote(target)).as_posix())
        return f"](<{path}>)" if path in catalog.by_path else match.group(0)

    text = MENTION_TAG_RE.sub(replace_mention, text)
    return convert_tables(RELATIVE_LINK_RE.sub(replace_link, text) if page_path else text)


def convert_tables(text):
    """Exporterが書き出すHTMLの表（経緯・実装状況など）を、トークンの少ないMarkdownの表にする。
    想定外の形の表はそのまま残す。"""
    lines = text.split("\n")
    result = []
    index = 0
    while index < len(lines):
        if not TABLE_START_RE.match(lines[index]):
            result.append(lines[index])
            index += 1
            continue
        end = next((i for i in range(index + 1, len(lines)) if TABLE_END_RE.match(lines[i])), None)
        if end is None:
            result.extend(lines[index:])
            break
        block = "\n".join(lines[index:end + 1])
        rows = [[clean_cell(cell) for cell in TABLE_CELL_RE.findall(row)] for row in TABLE_ROW_RE.findall(block)]
        width = max((len(row) for row in rows), default=0)
        if not rows or width == 0 or any(len(row) == 0 for row in rows):
            result.extend(lines[index:end + 1])
        else:
            rows = [row + [""] * (width - len(row)) for row in rows]
            result.append("| " + " | ".join(rows[0]) + " |")
            result.append("|" + "---|" * width)
            result.extend("| " + " | ".join(row) + " |" for row in rows[1:])
        index = end + 1
    return "\n".join(result)


def clean_cell(cell):
    return re.sub(r"\s*\n\s*", "<br>", cell.strip()).replace("|", "\\|")


def resolve_target(catalog, target):
    """ID・URL・パス・タイトルのどれかで1ページに決める。タイトルが重なれば候補を返す。"""
    page_id = extract_page_id(target)
    if page_id and page_id in catalog.by_id:
        return catalog.by_id[page_id], []
    normalized = target.replace("\\", "/")
    if normalized in catalog.by_path:
        return catalog.by_path[normalized], []
    candidates = [row for row in catalog.rows if row["title"] == target]
    if len(candidates) == 1:
        return candidates[0], []
    for database in SPEC_DATABASES:
        specs = [row for row in candidates if row.get("database") == database]
        if len(specs) == 1:
            return specs[0], []
    if not candidates:
        candidates = [row for row in catalog.rows if matches_text(row["title"], target)]
    return None, candidates


def brief(catalog, row):
    """search・get の一覧に出す1件分の情報。"""
    return {
        "page_id": row.get("page_id", ""),
        "title": row["title"],
        "path": row["path"],
        "database": row.get("database", ""),
        "category": first_property(row, CATEGORY_PROPERTIES),
        "summary": catalog.summary(row),
        "characters": row.get("characters", 0),
    }


def get(catalog, row, include_rationale, include_status):
    """1ページ分の取得結果を組み立てる。"""
    _, body = read_body(catalog.source / row["path"], row["path"])
    sections = split_sections(body)
    chosen = [section for section in sections if section.kind == KIND_SPEC or include_rationale]
    parents = catalog.parents(row)
    children = catalog.children(row)
    # 親・子として既に出したページは、関連ページに重ねて出さない。
    listed = {item["path"] for item in parents + children}
    result = {
        "page": brief(catalog, row) | {
            "url": row.get("url", ""),
            "last_edited": row.get("last_edited", ""),
            "properties": property_values(catalog, row),
        },
        "parents": [brief(catalog, parent) for parent in parents],
        "children": [brief(catalog, child) for child in children],
        "related": [
            brief(catalog, catalog.by_path[path]) for path in row.get("links", [])
            if path in catalog.by_path and path not in listed
        ],
        "sections": [
            {"heading": section.heading, "kind": section.kind, "text": replace_mentions(catalog, section.text, row["path"])}
            for section in chosen
        ],
        "omitted_sections": [section.heading for section in sections if section not in chosen],
    }
    if include_status:
        status = []
        for status_row in catalog.status_rows(row):
            _, status_body = read_body(catalog.source / status_row["path"], status_row["path"])
            status.append(brief(catalog, status_row) | {
                "properties": property_values(catalog, status_row),
                "text": replace_mentions(catalog, "\n".join(status_body).strip("\n"), status_row["path"]),
            })
        result["status"] = status
    result["returned_characters"] = sum(len(section["text"]) for section in result["sections"]) + sum(
        len(item["text"]) for item in result.get("status", []))
    return result


def format_brief(item):
    line = f"- {item['title']} ({item['page_id'] or 'IDなし'}) <{item['path']}>"
    labels = [label for label in (item["database"], item["category"]) if label]
    if labels:
        line += " 〔" + " / ".join(labels) + "〕"
    if item["summary"]:
        line += f" {item['summary']}"
    return line


def format_get(result):
    page = result["page"]
    lines = [f"# {page['title']}", "", f"- ページID: {page['page_id']}", f"- パス: <{page['path']}>"]
    if page["url"]:
        lines.append(f"- Notion: {page['url']}")
    if page["last_edited"]:
        lines.append(f"- 最終更新: {page['last_edited']}")
    for name, value in page["properties"].items():
        lines.append(f"- {name}: {value}")
    for label, key in (("親仕様", "parents"), ("子仕様・子ページ", "children"), ("関連ページ", "related")):
        if result[key]:
            lines.extend(["", f"## {label}", ""])
            lines.extend(format_brief(item) for item in result[key])
    for section in result["sections"]:
        lines.extend(["", f"<!-- 節: {section['heading'] or '（見出しなし）'} / 種別: {section['kind']} -->", section["text"]])
    if "status" in result:
        lines.extend(["", "## 実装状態（実装リスト）", ""])
        if not result["status"]:
            lines.append("実装リストに、このページを指す行がありません。")
        for item in result["status"]:
            lines.append(format_brief(item))
            lines.extend(f"  - {name}: {value}" for name, value in item["properties"].items())
            if item["text"]:
                lines.extend(["", item["text"], ""])
    if result["omitted_sections"]:
        lines.extend(["", "省いた節: " + "、".join(result["omitted_sections"]) + "（--rationale で読めます）"])
    lines.append(f"返した本文: {result['returned_characters']} 文字 / ページ全体: {page['characters']} 文字")
    return "\n".join(lines) + "\n"


def build_parser():
    parser = argparse.ArgumentParser(description="Notion仕様書ミラーを目録で探し、必要な節だけ読む。")
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE, help="仕様書ミラーのルート（既定: Docs/NotionSpecifications）")
    parser.add_argument("--json", action="store_true", help="結果をJSONで出す")
    commands = parser.add_subparsers(dest="command", required=True)

    search_parser = commands.add_parser("search", help="目録からページを探す")
    search_parser.add_argument("query", nargs="*", help="キーワード（複数はAND）。省くと絞り込みだけで一覧する")
    search_parser.add_argument("--category", help="カテゴリー（システムリストでは種別）の完全一致")
    search_parser.add_argument("--parent", help="親仕様（ID・パス・タイトル）")
    search_parser.add_argument("--scope", help="適用範囲（製品版・体験版など）。未設定のページは含める")
    search_parser.add_argument("--db", help="所属DB名の部分一致（例: 仕様リスト）")
    search_parser.add_argument("--include-dev", action="store_true", help="カテゴリー「開発用」も含める")
    search_parser.add_argument("--no-fallback", action="store_true", help="目録で0件のときに本文を検索しない")
    search_parser.add_argument("--limit", type=int, default=DEFAULT_LIMIT, help=f"表示件数（既定 {DEFAULT_LIMIT}）")

    get_parser = commands.add_parser("get", help="1ページの仕様の節を読む")
    get_parser.add_argument("target", help="ページID・Notion URL・パス・タイトル")
    get_parser.add_argument("--rationale", action="store_true", help="仕様意図（経緯・却下・保留した案）も返す")
    get_parser.add_argument("--status", action="store_true", help="実装リストの実装状態も返す")
    return parser


def main(argv=None):
    args = build_parser().parse_args(argv)
    source = args.source.resolve()
    if not source.is_dir():
        print(f"仕様書のディレクトリが見つかりません: {args.source}", file=sys.stderr)
        return 1
    catalog = load_catalog(source)
    if not catalog.from_file:
        print(f"{CATALOG_JSONL} が無いため、目録をその場で作りました。", file=sys.stderr)

    if args.command == "search":
        mode, total, rows = search(catalog, args)
        items = [brief(catalog, row) for row in rows]
        if args.json:
            print(json.dumps({"mode": mode, "total": total, "results": items}, ensure_ascii=False, indent=2))
        else:
            print(f"{total} 件（{mode}で一致）" + (f"、上位 {len(items)} 件を表示" if total > len(items) else ""))
            for item in items:
                print(format_brief(item))
        return 0 if total else 2

    row, candidates = resolve_target(catalog, args.target)
    if row is None:
        if candidates:
            print("ページを1件に決められません。IDかパスで指定してください:", file=sys.stderr)
            for item in candidates[:DEFAULT_LIMIT]:
                print(format_brief(brief(catalog, item)), file=sys.stderr)
        else:
            print("ページが見つかりません。search で探してください。", file=sys.stderr)
        return 2
    result = get(catalog, row, args.rationale, args.status)
    if args.json:
        print(json.dumps(result, ensure_ascii=False, indent=2))
    else:
        sys.stdout.write(format_get(result))
    return 0


if __name__ == "__main__":
    sys.exit(main())
