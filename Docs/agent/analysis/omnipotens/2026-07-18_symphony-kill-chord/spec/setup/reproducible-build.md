# 再現可能ビルドに関する記録

ステータス: partial（部分完了）。

- 解析対象revision: `0cca5d536257fd5a2f7951f7d3a0756dfc0d5788`。
- Unity Editor: `6000.3.10f1`。
- build workflowは `actions/checkout` をcommitで固定し、`develop` へのpull-request mergeまたは手動dispatchでbuildする。
- Unity build前にGoogle Drive folderを照会し、最新ZIPをdownloadして、その中の全 `.unitypackage` をimportする（`.github/workflows/BuildAndRelease.yml:168`）。選択されるpackageはimmutable IDで固定されず、workflow内でchecksum検証もされない。
- build workflowに自動test実行stepは確認できない。

したがって再現にはGit revisionだけでなく、変更可能なGoogle Drive内容とsecretも影響する。release manifestでは、外部archive IDとSHA-256、Unity version、target platform、build script revisionを固定する必要がある。
