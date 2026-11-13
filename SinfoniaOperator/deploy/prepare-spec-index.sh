#!/usr/bin/env bash
set -euo pipefail

readonly STAGING_DIRECTORY="${1:?staging directory is required}"
readonly RELEASE_DIRECTORY="${2:?final release directory is required}"
readonly DEPLOY_ROOT="${3:?deploy root is required}"
# プロジェクト ドキュメントはデプロイのワークフローが仕様書リポジトリから取り、パッケージの specifications/ に同梱する（#2329）。
readonly SPECIFICATION_ROOT="$STAGING_DIRECTORY/specifications"
readonly MODEL_PATH="$DEPLOY_ROOT/models/multilingual-e5-small/model.onnx"

# 稼働中の索引を上書きせず、同梱したプロジェクト ドキュメントから新リリース用に生成する。
if [[ ! -d "$SPECIFICATION_ROOT/Docs/NotionSpecifications" || ! -f "$MODEL_PATH" ]]; then
  echo "[Deploy] bundled specifications or embedding model is missing." >&2
  exit 1
fi

python3 - "$STAGING_DIRECTORY" "$RELEASE_DIRECTORY" "$SPECIFICATION_ROOT" "$MODEL_PATH" <<'PY'
import json
import pathlib
import sys

staging, release, specification_root, model = map(pathlib.Path, sys.argv[1:])
source = specification_root / "Docs/NotionSpecifications"
if not any(source.rglob("*.md")):
    raise SystemExit("[Deploy] bundled specifications are empty.")

settings = json.loads((staging / "spec-search.release.json").read_text(encoding="utf-8"))
settings["SPEC_SEARCH_INDEX_PATH"] = str(release / "spec-index.bin")
settings["SPEC_SEARCH_EMBEDDING_MODEL_PATH"] = str(model)
(staging / "spec-search.release.json").write_text(
    json.dumps(settings, ensure_ascii=False, indent=2), encoding="utf-8")
index_settings = {
    "SPEC_SEARCH_INDEX_PATH": str(staging / "spec-index.bin"),
    "SPEC_SEARCH_EMBEDDING_MODEL_PATH": str(model),
    "NOTION_EXPORT_OUTPUT": str(source),
}
(staging / "spec-search.index.json").write_text(
    json.dumps(index_settings, ensure_ascii=False, indent=2), encoding="utf-8")
PY

# 索引の SourceFile が従来どおり Docs/NotionSpecifications/... になるよう、同梱したプロジェクト ドキュメントのルートで実行する。
(
  cd "$SPECIFICATION_ROOT"
  "$STAGING_DIRECTORY/SinfoniaOperator" index "$STAGING_DIRECTORY/spec-search.index.json"
)
test -s "$STAGING_DIRECTORY/spec-index.bin"
rm -- "$STAGING_DIRECTORY/spec-search.index.json"
# 索引を作った後はプロジェクト ドキュメントを残さない（稼働中のBotは索引だけを読む）。
rm -rf -- "$SPECIFICATION_ROOT"
echo "[Deploy] release specification index is prepared."
