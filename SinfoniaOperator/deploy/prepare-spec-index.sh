#!/usr/bin/env bash
set -euo pipefail

readonly STAGING_DIRECTORY="${1:?staging directory is required}"
readonly RELEASE_DIRECTORY="${2:?final release directory is required}"
readonly DEPLOY_ROOT="${3:?deploy root is required}"
readonly REPOSITORY_DIRECTORY="$DEPLOY_ROOT/repository"
readonly MODEL_PATH="$DEPLOY_ROOT/models/multilingual-e5-small/model.onnx"

# 稼働中の索引を上書きせず、サーバー上の仕様キャッシュから新リリース用に生成する。
if [[ ! -d "$REPOSITORY_DIRECTORY" || ! -f "$MODEL_PATH" ]]; then
  echo "[Deploy] repository cache or embedding model is missing." >&2
  exit 1
fi

python3 - "$STAGING_DIRECTORY" "$RELEASE_DIRECTORY" "$REPOSITORY_DIRECTORY" "$MODEL_PATH" <<'PY'
import json
import pathlib
import sys

staging, release, repository, model = map(pathlib.Path, sys.argv[1:])
source = repository / "Library/NotionSpecifications"
if not source.is_dir():
    source = repository / "Docs/NotionSpecifications"
if not source.is_dir() or not any(source.rglob("*.md")):
    raise SystemExit("[Deploy] specification cache is empty or missing.")

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

(
  cd "$REPOSITORY_DIRECTORY"
  "$STAGING_DIRECTORY/SinfoniaOperator" index "$STAGING_DIRECTORY/spec-search.index.json"
)
test -s "$STAGING_DIRECTORY/spec-index.bin"
rm -- "$STAGING_DIRECTORY/spec-search.index.json"
echo "[Deploy] release specification index is prepared."
