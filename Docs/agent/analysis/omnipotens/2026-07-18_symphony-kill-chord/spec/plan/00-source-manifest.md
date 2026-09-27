# Symphony Kill Chord 最終解析の出典

**ステータス: complete（静的証拠について完了）。ランタイム検証は未実施。**

- Analysis profile: final eleven-stage Omnipotens analysis
- Analysis date: 2026-07-18 (Asia/Tokyo)
- Repository: https://github.com/HIBIKI5201/SymphonyKillChord
- Analysis branch: `analysis/omnipotens-core`
- Pinned source commit: `0cca5d536257fd5a2f7951f7d3a0756dfc0d5788`
- Unity editor declaration: `6000.3.10f1 (e35f0c77bd8e)`
- Runtime scope: `Assets/Scripts/Runtime` (799 C# files)

## 企画・仕様ソース

- Public root: https://lying-foxglove-81a.notion.site/27d7c2c6cc02819a9547f211f355dfec
- Root title: `仕様概要`
- Coverage: rootから発見した35ページを統合コーパスとして確認した。
- Final index: `spec/data/notion-full-spec-index.json`
- Source payloads: `spec/data/notion-full-pages/*.json` および本文補完用payload
- 公開ページはCanalis `NotionPublicSource`で取得した。
- 画像内だけに存在するレイアウト・数値はテキスト抽出で欠落しうる。原ページの `:未確定:`、空ページ、内部矛盾は確定要件へ補完していない。

## ツール・辞書の固定

- Anatomia: `0.1.0`, Unity対応 commit `d0cc4d6e77d1a10d1b4b1ca911026a1bec304cbb`
- Anatomia parser: tree-sitter WASM による決定的 C# 構文解析。LLM・clang・Unity Editor は不使用。
- Ludus public OKF: `b949cfa136fa27de101ace324f99a715f17e6846`; verifier result 907 typed docs / 906 nodes / 2,863 edges
- Vitia: `3fa33c9e5e7913c35a2a085dbbe49631cdee9eae`
- Vitia source manifest: `spec/data/vitia-ux-source-manifest.json`
- AI Format: `ff3edc1a927170d1728df10f635af28fd000dd7c`
- Canalis: `0.1.0`, `69b7fdaedc54233589d60c77b185ef195e78d5c8`

## 実施境界

- 実施: 全仕様の整理、Ludus対応付け、10ドメイン再定義、Unity-aware静的解析、仕様配線、mechanics/economy、AI Format、Vitia UX／市場性、Di合議、HTML統合。
- 未実施: Unity/ゲーム起動、ビルド、実機プレイ、Prefab/Sceneのシリアライズ参照解決、CRI実行、Profiler、端末性能、ユーザテスト、ストア・価格・販売データ検証。
- サービスの再起動・起動テストは行っていない。
