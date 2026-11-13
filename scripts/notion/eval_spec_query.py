"""プロジェクト ドキュメントの窓口（spec_query の search / get）と、現行のMarkdown全文検索を評価セットで比べる（#2396 段階3）。

測るもの（質問ごとに数え、全体を平均する）:
  再現率   上位1件・上位5件に、正解ページのどれかが入った割合
  読んだ量 エージェントが読む文字数。窓口は search の一覧＋1件目の get、全文検索は一致したパスの一覧＋1件目の全文
  時間     検索と取得にかかった時間
  版の一致 目録（_catalog.jsonl）が、今のページから作り直した目録と同じか

全文検索は、キーワードを全部含むファイルを、出現回数の多い順に並べる（rg -c で数えて並べるのと同じ）。

使い方（リポジトリのルートから）:
  python scripts/notion/eval_spec_query.py --source Docs/NotionSpecifications
  python scripts/notion/eval_spec_query.py --source Docs/NotionSpecifications --json
"""
import argparse
import json
import sys
import time
from pathlib import Path

from build_spec_catalog import CATALOG_JSONL, build_jsonl, collect, resolve_links
from spec_query import brief, format_brief, format_get, get, load_catalog, search

DEFAULT_SET = Path(__file__).with_name("eval") / "spec_eval_set.json"
TOP = 5


def fulltext_rank(files, keywords):
    """キーワードを全部含むファイルを、出現回数の合計が多い順に返す。"""
    scored = []
    for path, text in files:
        folded = text.casefold()
        counts = [folded.count(keyword.casefold()) for keyword in keywords]
        if all(counts):
            scored.append((-sum(counts), path))
    scored.sort()
    return [path for _, path in scored]


def load_files(source):
    """全文検索の対象（.git・assets・目録・DB索引を除いたMarkdown）を読む。"""
    files = []
    for file in sorted(source.rglob("*.md")):
        relative = file.relative_to(source).as_posix()
        parts = relative.split("/")
        if parts[0] == ".git" or "assets" in parts or file.name.startswith("_"):
            continue
        files.append((relative, file.read_text(encoding="utf-8")))
    return files


def hit_rank(ids, expected):
    for index, page_id in enumerate(ids):
        if page_id in expected:
            return index + 1
    return 0


def evaluate_window(catalog, item):
    started = time.perf_counter()
    args = argparse.Namespace(query=item["keywords"], category=None, parent=None, scope=None, db=None,
                              include_dev=False, no_fallback=False, limit=TOP)
    _, _, rows = search(catalog, args)
    listing = "\n".join(format_brief(brief(catalog, row)) for row in rows)
    read = len(listing)
    if rows:
        read += len(format_get(get(catalog, rows[0], False, False)))
    elapsed = time.perf_counter() - started
    return hit_rank([row.get("page_id", "") for row in rows], set(item["expected"])), read, elapsed


def evaluate_fulltext(files, id_by_path, item):
    started = time.perf_counter()
    paths = fulltext_rank(files, item["keywords"])
    text_by_path = dict(files)
    read = sum(len(path) + 1 for path in paths)  # rg -l の一覧はヒット全件が出る
    if paths:
        read += len(text_by_path[paths[0]])
    elapsed = time.perf_counter() - started
    return hit_rank([id_by_path.get(path, "") for path in paths[:TOP]], set(item["expected"])), read, elapsed, len(paths)


def summarize(results, key):
    count = len(results)
    ranks = [result[key]["rank"] for result in results]
    return {
        "recall_at_1": sum(1 for rank in ranks if rank == 1) / count,
        f"recall_at_{TOP}": sum(1 for rank in ranks if rank) / count,
        "average_characters": sum(result[key]["characters"] for result in results) / count,
        "average_seconds": sum(result[key]["seconds"] for result in results) / count,
    }


def main(argv=None):
    parser = argparse.ArgumentParser(description="プロジェクト ドキュメントの窓口と全文検索を評価セットで比べる。")
    parser.add_argument("--source", type=Path, default=Path("Docs/NotionSpecifications"), help="プロジェクト ドキュメントのミラーのルート")
    parser.add_argument("--set", type=Path, default=DEFAULT_SET, help="評価セットのJSON")
    parser.add_argument("--json", action="store_true", help="結果をJSONで出す")
    args = parser.parse_args(argv)
    source = args.source.resolve()
    items = json.loads(args.set.read_text(encoding="utf-8"))["items"]

    pages, _ = collect(source)
    catalog_file = source / CATALOG_JSONL
    same_version = catalog_file.is_file() and catalog_file.read_text(encoding="utf-8") == build_jsonl(pages, resolve_links(pages))
    catalog = load_catalog(source)
    id_by_path = {row["path"]: row.get("page_id", "") for row in catalog.rows}
    files = load_files(source)

    results = []
    for item in items:
        window_rank, window_read, window_time = evaluate_window(catalog, item)
        text_rank, text_read, text_time, text_hits = evaluate_fulltext(files, id_by_path, item)
        results.append({
            "id": item["id"], "keywords": item["keywords"],
            "window": {"rank": window_rank, "characters": window_read, "seconds": window_time},
            "fulltext": {"rank": text_rank, "characters": text_read, "seconds": text_time, "hits": text_hits},
        })
    report = {
        "items": len(items), "catalog_same_version": same_version,
        "window": summarize(results, "window"), "fulltext": summarize(results, "fulltext"), "results": results,
    }
    if args.json:
        print(json.dumps(report, ensure_ascii=False, indent=2))
        return 0
    print(f"評価セット {len(items)} 問 / 目録とページの版: {'一致' if same_version else '不一致'}")
    print(f"| 方式 | 上位1件 | 上位{TOP}件 | 読んだ文字数（平均） | 時間（平均） |")
    print("| --- | --- | --- | --- | --- |")
    for key, label in (("window", "窓口（search＋get）"), ("fulltext", "全文検索（rg相当）")):
        summary = report[key]
        print(f"| {label} | {summary['recall_at_1']:.0%} | {summary[f'recall_at_{TOP}']:.0%} | "
              f"{summary['average_characters']:,.0f} | {summary['average_seconds'] * 1000:.0f} ms |")
    misses = [result["id"] for result in results if not result["window"]["rank"]]
    print(f"窓口で上位{TOP}件に入らなかった問: {', '.join(misses) if misses else 'なし'}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
