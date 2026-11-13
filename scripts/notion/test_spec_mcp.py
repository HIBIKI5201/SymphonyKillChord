"""プロジェクト ドキュメントの窓口の MCP サーバー（spec_mcp.py）を、小さなプロジェクト ドキュメントのミラーで確かめる。"""
import io
import json
import unittest

from spec_mcp import Server, serve
from test_spec_query import DODGE_ID, SpecQueryTests


class SpecMcpTests(unittest.TestCase):
    def setUp(self):
        # spec_query のテストと同じ小さなミラーを作る。
        self.fixture = SpecQueryTests()
        self.fixture.setUp()
        self.server = Server(self.fixture.root)

    def tearDown(self):
        self.fixture.tearDown()

    def call(self, method, params=None, request_id=1):
        message = {"jsonrpc": "2.0", "id": request_id, "method": method}
        if params is not None:
            message["params"] = params
        return self.server.handle(message)

    def call_tool(self, name, arguments):
        response = self.call("tools/call", {"name": name, "arguments": arguments})
        return response["result"]

    def test_initialize_reports_tools_capability(self):
        response = self.call("initialize", {"protocolVersion": "2025-06-18", "capabilities": {}, "clientInfo": {"name": "test"}})
        self.assertEqual(response["id"], 1)
        self.assertIn("tools", response["result"]["capabilities"])
        self.assertEqual(response["result"]["serverInfo"]["name"], "skc-spec")

    def test_notifications_get_no_response(self):
        self.assertIsNone(self.server.handle({"jsonrpc": "2.0", "method": "notifications/initialized"}))

    def test_tools_list_names_both_tools(self):
        tools = self.call("tools/list")["result"]["tools"]
        self.assertEqual([tool["name"] for tool in tools], ["spec_search", "spec_get"])
        self.assertEqual(tools[1]["inputSchema"]["required"], ["target"])

    def test_search_returns_spec_first_and_hides_development_pages(self):
        result = self.call_tool("spec_search", {"query": "回避"})
        self.assertFalse(result["isError"])
        items = result["structuredContent"]["results"]
        self.assertEqual(items[0]["page_id"], DODGE_ID)
        self.assertNotIn("旧回避メモ", [item["title"] for item in items])
        self.assertIn("回避アクション", result["content"][0]["text"])

    def test_search_can_include_development_pages(self):
        result = self.call_tool("spec_search", {"query": "回避", "include_dev": True})
        self.assertIn("旧回避メモ", [item["title"] for item in result["structuredContent"]["results"]])

    def test_get_returns_spec_sections_and_status(self):
        result = self.call_tool("spec_get", {"target": DODGE_ID, "status": True})
        self.assertFalse(result["isError"])
        structured = result["structuredContent"]
        self.assertEqual({section["kind"] for section in structured["sections"]}, {"仕様"})
        self.assertEqual(structured["status"][0]["title"], "回避アクション")
        self.assertIn("実装状態（実装リスト）", result["content"][0]["text"])

    def test_get_with_rationale_adds_rationale_section(self):
        result = self.call_tool("spec_get", {"target": DODGE_ID, "rationale": True})
        kinds = [section["kind"] for section in result["structuredContent"]["sections"]]
        self.assertIn("制作意図", kinds)

    def test_get_unknown_page_is_tool_error(self):
        result = self.call_tool("spec_get", {"target": "存在しないページ"})
        self.assertTrue(result["isError"])
        self.assertNotIn("structuredContent", result)

    def test_unknown_tool_and_method_are_protocol_errors(self):
        self.assertEqual(self.call("tools/call", {"name": "nope", "arguments": {}})["error"]["code"], -32602)
        self.assertEqual(self.call("resources/list")["error"]["code"], -32601)

    def test_serve_reads_lines_and_answers_requests_only(self):
        lines = "\n".join([
            json.dumps({"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {}}),
            json.dumps({"jsonrpc": "2.0", "method": "notifications/initialized"}),
            "not json",
            json.dumps({"jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": {"name": "spec_get", "arguments": {"target": DODGE_ID}}}),
        ]) + "\n"
        out = io.StringIO()
        serve(self.server, io.StringIO(lines), out)
        responses = [json.loads(line) for line in out.getvalue().splitlines()]
        self.assertEqual([response.get("id") for response in responses], [1, None, 2])
        self.assertEqual(responses[1]["error"]["code"], -32700)
        self.assertFalse(responses[2]["result"]["isError"])


if __name__ == "__main__":
    unittest.main()
