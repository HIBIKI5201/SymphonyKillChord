"""タスク・議事録・仕様検討フォーラムの定型投稿を、JSON から Notion へ作成する。

JSON（1件または配列）を scripts/notion/templates/<type>.json で検証し、本文の Markdown と
`--set` を組み立てて NotionMarkdownWriter の create へ渡す。書き込み・許可リストの確認・
dry-run は NotionMarkdownWriter が行い、このスクリプトは変換と名前の解決だけを担う。

使い方（リポジトリのルートから）:
  python scripts/notion/apply_entry.py entry.json             # 変換結果を表示し、Writer で dry-run
  python scripts/notion/apply_entry.py entry.json --confirm   # Notion へ作成する
  python scripts/notion/apply_entry.py entry.json --offline   # Notion へ接続せず変換結果だけ表示
  python scripts/notion/apply_entry.py --check-templates      # テンプレートと Notion のスキーマを比べる
"""
import argparse
import datetime
import json
import os
import platform
import re
import subprocess
import sys
import tempfile
import urllib.error
import urllib.request
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
TEMPLATE_DIR = Path(__file__).resolve().parent / "templates"
OPERATOR_DIR = REPO_ROOT / "SinfoniaOperator"
API_BASE_URL = "https://api.notion.com/v1"
# 読み取りだけに使う。database の query と properties の options を素直に返す版。
NOTION_VERSION = "2022-06-28"
PLACEHOLDER = re.compile(r"\{\{([^{}]+)\}\}")
NOTION_ID = re.compile(r"[0-9a-fA-F]{32}")
DATE = re.compile(r"\d{4}-\d{2}-\d{2}")
EMPTY_BLOCK = "<empty-block/>"


class EntryError(Exception):
    """入力 JSON やテンプレートの誤り。送信前に止めるために使う。"""


def load_templates(directory=TEMPLATE_DIR):
    """種類名 → テンプレート定義の辞書を返す。"""
    templates = {}
    for path in sorted(Path(directory).glob("*.json")):
        template = json.loads(path.read_text(encoding="utf-8"))
        templates[template["type"]] = template
    return templates


def split_values(value):
    """配列またはカンマ区切りの文字列を、値のリストにする。"""
    items = value if isinstance(value, list) else str(value).split(",")
    return [str(item).strip() for item in items if str(item).strip()]


def normalize_value(name, definition, value, today):
    """プロパティ値を型に沿って検証し、--set に書く文字列（relation は名前のリスト）にする。"""
    kind = definition["type"]
    if kind in ("title", "rich_text"):
        return str(value)
    if kind in ("status", "select"):
        options = definition.get("options")
        if options is not None and str(value) not in options:
            raise EntryError(f"「{name}」に選択肢にない値です: {value}（選べる値: {'、'.join(options)}）")
        return str(value)
    if kind == "multi_select":
        values = split_values(value)
        options = definition.get("options")
        unknown = [item for item in values if options is not None and item not in options]
        if unknown:
            raise EntryError(f"「{name}」に選択肢にない値です: {'、'.join(unknown)}（選べる値: {'、'.join(options)}）")
        return ",".join(values)
    if kind == "number":
        if isinstance(value, bool) or not isinstance(value, (int, float)):
            try:
                value = float(str(value))
            except ValueError:
                raise EntryError(f"「{name}」は数値で指定してください: {value}") from None
        return str(int(value)) if float(value).is_integer() else str(value)
    if kind == "checkbox":
        if not isinstance(value, bool):
            raise EntryError(f"「{name}」は true か false で指定してください: {value}")
        return "true" if value else "false"
    if kind == "date":
        text = today.isoformat() if value == "@today" else str(value)
        if not DATE.fullmatch(text):
            raise EntryError(f"「{name}」は YYYY-MM-DD で指定してください: {value}")
        return text
    if kind == "people":
        return ",".join(split_values(value))
    if kind == "relation":
        return split_values(value)
    if kind in ("url", "email", "phone_number"):
        return str(value)
    raise EntryError(f"テンプレートの「{name}」の型 {kind} には対応していません")


def build_properties(template, entry, today):
    """入力のプロパティを検証し、プロパティ名 → 値の辞書（タイトルを含む）を返す。"""
    errors = []
    title = entry.get("title")
    if not isinstance(title, str) or not title.strip():
        errors.append("title（ページ名）は必須です")
    given = entry.get("properties") or {}
    if not isinstance(given, dict):
        raise EntryError("properties はオブジェクトで指定してください")
    definitions = template["properties"]
    unknown = [name for name in given if name not in definitions]
    if unknown:
        errors.append(f"未知のプロパティです: {'、'.join(unknown)}（使えるプロパティ: {'、'.join(definitions)}）")

    values = {template["title_property"]: title.strip() if isinstance(title, str) else ""}
    for name, definition in definitions.items():
        value = given.get(name, definition.get("default"))
        if value is None or value == "" or value == []:
            if definition.get("required"):
                errors.append(f"「{name}」は必須です")
            continue
        try:
            values[name] = normalize_value(name, definition, value, today)
        except EntryError as error:
            errors.append(str(error))
    if errors:
        raise EntryError("\n".join(errors))
    return values


def build_body(template, entry):
    """本文の Markdown を組み立てる。行全体が {{節}} の行は、その行の字下げで複数行を差し込む。"""
    sections = entry.get("sections") or {}
    if not isinstance(sections, dict):
        raise EntryError("sections はオブジェクトで指定してください")
    definitions = template["sections"]
    errors = []
    unknown = [name for name in sections if name not in definitions]
    if unknown:
        errors.append(f"未知の節です: {'、'.join(unknown)}（使える節: {'、'.join(definitions)}）")

    texts = {}
    for name, definition in definitions.items():
        value = sections.get(name, definition.get("default", ""))
        if isinstance(value, list):
            value = "\n".join(str(item) for item in value)
        text = str(value).strip("\n")
        if not text.strip() and definition.get("required"):
            errors.append(f"節「{name}」は必須です")
        if definition.get("inline") and "\n" in text:
            errors.append(f"節「{name}」は1行で書いてください")
        texts[name] = text
    if errors:
        raise EntryError("\n".join(errors))

    lines = []
    for line in template["body"]:
        indent = line[: len(line) - len(line.lstrip("\t "))]
        whole = PLACEHOLDER.fullmatch(line.strip())
        if whole:
            text = texts[whole.group(1)]
            # 空の節は空行のブロックにして、テンプレートと同じ見た目にする。
            block = text.split("\n") if text.strip() else [EMPTY_BLOCK]
            lines.extend(indent + item if item.strip() else item for item in block)
        else:
            lines.append(PLACEHOLDER.sub(lambda match: texts[match.group(1)], line))
    return "\n".join(lines) + "\n"


def build_entry(template, entry, today):
    """1件分の入力を、作成先・タイトル・プロパティ・本文へ変換する。"""
    # プロパティと本文の誤りは、まとめて1回で知らせる。
    errors, properties, body = [], None, None
    try:
        properties = build_properties(template, entry, today)
    except EntryError as error:
        errors.append(str(error))
    try:
        body = build_body(template, entry)
    except EntryError as error:
        errors.append(str(error))
    if errors:
        raise EntryError("\n".join(errors))
    return {
        "type": template["type"],
        "label": template["label"],
        "database": template["database"],
        "title": entry["title"].strip(),
        "properties": properties,
        "body": body,
    }


def parse_entries(data, templates, today):
    """JSON（1件または配列）を検証・変換する。どれか1件でも誤りがあれば全件を止める。"""
    entries = data if isinstance(data, list) else [data]
    built, errors = [], []
    for index, entry in enumerate(entries, start=1):
        prefix = f"{index}件目" + (f"（{entry.get('title')}）" if isinstance(entry, dict) and entry.get("title") else "")
        if not isinstance(entry, dict):
            errors.append(f"{prefix}: オブジェクトで指定してください")
            continue
        template = templates.get(entry.get("type"))
        if template is None:
            errors.append(f"{prefix}: type は {'・'.join(templates)} のどれかにしてください: {entry.get('type')}")
            continue
        try:
            built.append(build_entry(template, entry, today))
        except EntryError as error:
            errors.extend(f"{prefix}: {line}" for line in str(error).split("\n"))
    if errors:
        raise EntryError("\n".join(errors))
    return built


def resolve_relations(built, templates, resolver):
    """relation の名前・別名を、resolver でページ ID へ置き換える。"""
    errors = []
    for entry in built:
        definitions = templates[entry["type"]]["properties"]
        for name, value in list(entry["properties"].items()):
            definition = definitions.get(name)
            if not definition or definition["type"] != "relation":
                continue
            try:
                entry["properties"][name] = ",".join(resolver(name, definition, item) for item in value)
            except EntryError as error:
                errors.append(f"{entry['title']}: {error}")
    if errors:
        raise EntryError("\n".join(errors))


def set_arguments(entry):
    """NotionMarkdownWriter create に渡す --set の引数リストを返す。"""
    arguments = []
    for name, value in entry["properties"].items():
        arguments += ["--set", f"{name}={value}"]
    return arguments


class NotionReader:
    """relation の解決とスキーマ確認のための、読み取り専用の Notion API 呼び出し。"""

    def __init__(self):
        self._token = read_token()

    def request(self, method, path, body=None):
        headers = {"Notion-Version": NOTION_VERSION, "Content-Type": "application/json"}
        # クラウド環境ではプロキシが認証ヘッダーを付けるため、トークンが無ければ付けずに送る。
        if self._token:
            headers["Authorization"] = f"Bearer {self._token}"
        data = json.dumps(body).encode("utf-8") if body is not None else None
        request = urllib.request.Request(API_BASE_URL + path, data=data, headers=headers, method=method)
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                return json.loads(response.read().decode("utf-8"))
        except urllib.error.HTTPError as error:
            detail = error.read().decode("utf-8", "replace")[:300]
            raise EntryError(f"Notion API が {error.code} を返しました: {detail}") from None

    def query(self, database, filter_body):
        result = self.request("POST", f"/databases/{database}/query", {"filter": filter_body, "page_size": 10})
        return [page["id"] for page in result.get("results", [])]

    def resolve(self, name, definition, item):
        """relation の値 1 件を、ページ ID へ解決する。ID・URL はそのまま、名前は完全一致で探す。"""
        match = NOTION_ID.search(item.replace("-", ""))
        if match and (len(item.replace("-", "")) == 32 or "notion" in item):
            return match.group(0)
        alias = definition.get("aliases", {}).get(item)
        if alias:
            ids = self.query(definition["database"], {"property": alias["property"], "status": {"equals": alias["status"]}})
        else:
            ids = self.query(definition["database"], {"property": definition["title_property"], "title": {"equals": item}})
        if len(ids) != 1:
            state = "見つかりません" if not ids else f"{len(ids)}件あります（ID か URL で指定してください）"
            raise EntryError(f"「{name}」の「{item}」が{state}")
        return ids[0]


def read_token():
    """NotionMarkdownWriter と同じく、秘密設定ファイル → 環境変数の順で NOTION_TOKEN を読む。"""
    secrets = OPERATOR_DIR / "sinfonia-operator.secrets.json"
    if secrets.exists():
        token = json.loads(secrets.read_text(encoding="utf-8")).get("NOTION_TOKEN")
        if token:
            return token.strip()
    return os.environ.get("NOTION_TOKEN", "").strip()


def writer_command():
    """実行中の OS に合わせて NotionMarkdownWriter の起動コマンドを返す。"""
    if platform.system() == "Windows":
        return [str(OPERATOR_DIR / "NotionMarkdownWriter.exe")]
    return ["bash", str(OPERATOR_DIR / "run-tool.sh"), "NotionMarkdownWriter"]


def run_writer(entry, confirm):
    """1件分を NotionMarkdownWriter create へ渡す。終了コードを返す。"""
    with tempfile.TemporaryDirectory() as directory:
        body_path = Path(directory) / "body.md"
        body_path.write_text(entry["body"], encoding="utf-8", newline="\n")
        command = writer_command() + ["create", str(body_path), "--parent", entry["database"]] + set_arguments(entry)
        if confirm:
            command.append("--confirm")
        return subprocess.run(command, cwd=REPO_ROOT).returncode


def print_entry(index, entry):
    print(f"=== {index}. {entry['label']}: {entry['title']}")
    for name, value in entry["properties"].items():
        print(f"  {name}: {','.join(value) if isinstance(value, list) else value}")
    print("  本文:")
    for line in entry["body"].rstrip("\n").split("\n"):
        print(f"  | {line}")


def check_templates(templates, reader):
    """テンプレートのプロパティ・選択肢が、Notion の現在のスキーマと一致するか確かめる。"""
    problems = []
    for template in templates.values():
        schema = reader.request("GET", f"/databases/{template['database']}")["properties"]
        for name, definition in template["properties"].items():
            actual = schema.get(name)
            if actual is None:
                problems.append(f"{template['label']}: Notion に「{name}」がありません")
                continue
            if actual["type"] != definition["type"]:
                problems.append(f"{template['label']}: 「{name}」の型が {actual['type']} です（テンプレートは {definition['type']}）")
                continue
            if "options" in definition:
                live = [option["name"] for option in actual[actual["type"]]["options"]]
                if live != definition["options"]:
                    problems.append(f"{template['label']}: 「{name}」の選択肢が違います。Notion: {'、'.join(live)}")
        if schema.get(template["title_property"], {}).get("type") != "title":
            problems.append(f"{template['label']}: タイトルのプロパティ「{template['title_property']}」がありません")
    return problems


def main(argv=None):
    parser = argparse.ArgumentParser(description="JSON から Notion の定型投稿（タスク・議事録・フォーラム）を作成する")
    parser.add_argument("input", nargs="?", help="入力 JSON のパス（- で標準入力）")
    parser.add_argument("--confirm", action="store_true", help="Notion へ作成する（既定は dry-run）")
    parser.add_argument("--offline", action="store_true", help="Notion へ接続せず、変換結果だけを表示する")
    parser.add_argument("--check-templates", action="store_true", help="テンプレートと Notion のスキーマを比べる")
    args = parser.parse_args(argv)
    templates = load_templates()

    try:
        if args.check_templates:
            problems = check_templates(templates, NotionReader())
            print("\n".join(problems) if problems else "テンプレートは Notion のスキーマと一致しています。")
            return 1 if problems else 0
        if not args.input:
            parser.error("入力 JSON のパスを指定してください")
        if args.confirm and args.offline:
            parser.error("--confirm と --offline は同時に使えません")
        text = sys.stdin.read() if args.input == "-" else Path(args.input).read_text(encoding="utf-8")
        try:
            data = json.loads(text)
        except json.JSONDecodeError as error:
            raise EntryError(f"JSON として読めません: {error}") from None
        built = parse_entries(data, templates, datetime.date.today())
        if not args.offline:
            resolve_relations(built, templates, NotionReader().resolve)
    except EntryError as error:
        print("エラー（何も送信していません）:", file=sys.stderr)
        print(error, file=sys.stderr)
        return 1

    for index, entry in enumerate(built, start=1):
        print_entry(index, entry)
    if args.offline:
        print("\n--offline のため、Notion へは接続していません。")
        return 0

    for index, entry in enumerate(built, start=1):
        print(f"\n--- {index}. {entry['title']} を NotionMarkdownWriter へ渡します")
        if run_writer(entry, args.confirm) != 0:
            done = f"{index - 1}件は作成済みです。" if args.confirm and index > 1 else ""
            print(f"\n{index}件目で失敗したため止めました。{done}", file=sys.stderr)
            return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
