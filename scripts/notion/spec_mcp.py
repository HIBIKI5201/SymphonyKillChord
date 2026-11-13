"""プロジェクト ドキュメントの窓口（spec_query.py の search / get）を MCP のツールとして公開する stdio サーバー。

仕様書改定方針（#2396）の段階2の「MCP で公開する」。Claude Code と Codex から、プロジェクト ドキュメントのミラーを
全文検索せずに、目録で探して（spec_search）必要な節だけ読む（spec_get）ために使う。

外部のライブラリを使わず、MCP の stdio トランスポート（1行1メッセージの JSON-RPC 2.0）を直接話す。
扱うメソッドは initialize・ping・tools/list・tools/call だけで、通知（id の無いメッセージ）には返事をしない。
目録は最初の呼び出しで読み、プロジェクト ドキュメントのミラーの _catalog.jsonl が更新されたら読み直す。

登録の例（リポジトリのルートから）:
  Claude Code: .mcp.json の "spec" サーバー
  Codex: .codex/config.toml の [mcp_servers.spec]
手で確かめるとき:
  echo '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | python scripts/notion/spec_mcp.py
"""
import argparse
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

from build_spec_catalog import CATALOG_JSONL  # noqa: E402
from spec_query import (  # noqa: E402
    DEFAULT_LIMIT,
    DEFAULT_SOURCE,
    brief,
    format_brief,
    format_get,
    get,
    load_catalog,
    resolve_target,
    search,
)

SERVER_NAME = "skc-spec"
SERVER_VERSION = "1.0.0"
# 対応する MCP の版。クライアントが別の版を求めたら、こちらの版を返す（クライアントが判断する）。
PROTOCOL_VERSION = "2025-06-18"

TOOLS = [
    {
        "name": "spec_search",
        "description": (
            "Symphony Kill Chord の Notion のプロジェクト ドキュメントのミラーを目録で探し、ページID・タイトル・サマリーを返す。"
            "仕様を読むときは、まずこれで候補を絞り、spec_get で必要なページだけ読む。"
            "カテゴリー「開発用（旧版・参考）」は既定で外す。"
        ),
        "inputSchema": {
            "type": "object",
            "properties": {
                "query": {"type": "string", "description": "キーワード。空白で区切ると AND。省くと絞り込みだけで一覧する"},
                "category": {"type": "string", "description": "カテゴリー（システムリストでは種別）の完全一致"},
                "parent": {"type": "string", "description": "親仕様（ID・パス・タイトル）"},
                "scope": {"type": "string", "description": "適用範囲（体験版など）。未設定のページは含める"},
                "db": {"type": "string", "description": "所属DB名の部分一致（例: 仕様リスト、用語リスト）"},
                "include_dev": {"type": "boolean", "description": "カテゴリー「開発用（旧版・参考）」も含める"},
                "limit": {"type": "integer", "description": f"返す件数（既定 {DEFAULT_LIMIT}）", "minimum": 1},
            },
        },
    },
    {
        "name": "spec_get",
        "description": (
            "プロジェクト ドキュメントの1ページから、仕様の節（仕様概要・詳細説明）と、親仕様・子仕様・関連ページを返す。"
            "rationale で仕様意図と、仕様検討フォーラムの決定の記録を、status で実装リストの実装状態を足す。"
        ),
        "inputSchema": {
            "type": "object",
            "properties": {
                "target": {"type": "string", "description": "ページID・Notion URL・パス・タイトル"},
                "rationale": {"type": "boolean", "description": "仕様意図と、仕様検討フォーラムの決定の記録（反映先がこのページの行）も返す"},
                "status": {"type": "boolean", "description": "実装リストの実装状態も返す"},
            },
            "required": ["target"],
        },
    },
]

# JSON-RPC のエラーコード。
PARSE_ERROR = -32700
INVALID_REQUEST = -32600
METHOD_NOT_FOUND = -32601
INVALID_PARAMS = -32602


class CatalogCache:
    """目録を一度だけ読み、_catalog.jsonl の更新時刻が変わったら読み直す。"""

    def __init__(self, source):
        self.source = source
        self.catalog = None
        self.stamp = None

    def _stamp(self):
        catalog_file = self.source / CATALOG_JSONL
        return catalog_file.stat().st_mtime_ns if catalog_file.is_file() else None

    def get(self):
        stamp = self._stamp()
        if self.catalog is None or stamp != self.stamp:
            self.catalog = load_catalog(self.source)
            self.stamp = stamp
        return self.catalog


def tool_search(catalog, arguments):
    limit = arguments.get("limit") or DEFAULT_LIMIT
    args = argparse.Namespace(
        query=str(arguments.get("query") or "").split(),
        category=arguments.get("category"),
        parent=arguments.get("parent"),
        scope=arguments.get("scope"),
        db=arguments.get("db"),
        include_dev=bool(arguments.get("include_dev")),
        no_fallback=False,
        limit=int(limit),
    )
    mode, total, rows = search(catalog, args)
    items = [brief(catalog, row) for row in rows]
    header = f"{total} 件（{mode}で一致）" + (f"、上位 {len(items)} 件を表示" if total > len(items) else "")
    text = "\n".join([header] + [format_brief(item) for item in items])
    return text, {"mode": mode, "total": total, "results": items}, False


def tool_get(catalog, arguments):
    target = str(arguments.get("target") or "").strip()
    if not target:
        return "target を指定してください。", None, True
    row, candidates = resolve_target(catalog, target)
    if row is None:
        if candidates:
            lines = ["ページを1件に決められません。IDかパスで指定してください:"]
            lines += [format_brief(brief(catalog, item)) for item in candidates[:DEFAULT_LIMIT]]
            return "\n".join(lines), None, True
        return "ページが見つかりません。spec_search で探してください。", None, True
    result = get(catalog, row, bool(arguments.get("rationale")), bool(arguments.get("status")))
    return format_get(result), result, False


HANDLERS = {"spec_search": tool_search, "spec_get": tool_get}


class Server:
    def __init__(self, source):
        self.source = source
        self.cache = CatalogCache(source)

    def handle(self, message):
        """1件のメッセージを処理し、返すべき応答（通知なら None）を返す。"""
        if not isinstance(message, dict) or message.get("jsonrpc") != "2.0" or "method" not in message:
            return error(message.get("id") if isinstance(message, dict) else None, INVALID_REQUEST, "Invalid Request")
        method = message["method"]
        if "id" not in message:
            return None
        request_id = message["id"]
        params = message.get("params") or {}
        if method == "initialize":
            return result(request_id, {
                "protocolVersion": PROTOCOL_VERSION,
                "capabilities": {"tools": {"listChanged": False}},
                "serverInfo": {"name": SERVER_NAME, "version": SERVER_VERSION},
                "instructions": "仕様を読むときは spec_search で候補を絞り、spec_get で必要なページだけ読む。",
            })
        if method == "ping":
            return result(request_id, {})
        if method == "tools/list":
            return result(request_id, {"tools": TOOLS})
        if method == "tools/call":
            name = params.get("name")
            handler = HANDLERS.get(name)
            if handler is None:
                return error(request_id, INVALID_PARAMS, f"Unknown tool: {name}")
            if not self.source.is_dir():
                return result(request_id, tool_result(f"プロジェクト ドキュメントのディレクトリが見つかりません: {self.source}", None, True))
            try:
                text, structured, is_error = handler(self.cache.get(), params.get("arguments") or {})
            except Exception as exception:  # ツールの失敗は、プロトコルのエラーではなくツールの結果として返す。
                return result(request_id, tool_result(f"{type(exception).__name__}: {exception}", None, True))
            return result(request_id, tool_result(text, structured, is_error))
        return error(request_id, METHOD_NOT_FOUND, f"Method not found: {method}")


def tool_result(text, structured, is_error):
    body = {"content": [{"type": "text", "text": text}], "isError": is_error}
    if structured is not None:
        body["structuredContent"] = structured
    return body


def result(request_id, value):
    return {"jsonrpc": "2.0", "id": request_id, "result": value}


def error(request_id, code, message):
    return {"jsonrpc": "2.0", "id": request_id, "error": {"code": code, "message": message}}


def serve(server, reader, writer):
    for line in reader:
        line = line.strip()
        if not line:
            continue
        try:
            message = json.loads(line)
        except json.JSONDecodeError:
            response = error(None, PARSE_ERROR, "Parse error")
        else:
            response = server.handle(message)
        if response is not None:
            writer.write(json.dumps(response, ensure_ascii=False) + "\n")
            writer.flush()


def main(argv=None):
    parser = argparse.ArgumentParser(description="プロジェクト ドキュメントの窓口を MCP（stdio）で公開する。")
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE, help="プロジェクト ドキュメントのミラーのルート（既定: Docs/NotionSpecifications）")
    args = parser.parse_args(argv)
    # Windows でも UTF-8 で読み書きする（既定のコードページでは日本語が化ける）。
    sys.stdin.reconfigure(encoding="utf-8")
    sys.stdout.reconfigure(encoding="utf-8", newline="\n")
    serve(Server(args.source.resolve()), sys.stdin, sys.stdout)
    return 0


if __name__ == "__main__":
    sys.exit(main())
