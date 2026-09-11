#!/usr/bin/env bash
set -euo pipefail

readonly MODEL_REVISION="03415a4be176a1620747c692ed433219fabc3def"
readonly MODEL_BASE_URL="https://huggingface.co/intfloat/multilingual-e5-small/resolve/${MODEL_REVISION}/onnx"
readonly TARGET_DIRECTORY="${1:-/opt/sinfonia-specsearch/models/multilingual-e5-small}"

mkdir -p "${TARGET_DIRECTORY}"
curl --fail --location --retry 3 \
  --output "${TARGET_DIRECTORY}/model.onnx" \
  "${MODEL_BASE_URL}/model.onnx?download=true"
curl --fail --location --retry 3 \
  --output "${TARGET_DIRECTORY}/sentencepiece.bpe.model" \
  "${MODEL_BASE_URL}/sentencepiece.bpe.model?download=true"

echo "Downloaded multilingual-e5-small to ${TARGET_DIRECTORY}."
