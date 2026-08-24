---
task: formalize-program-domains
project: SymphonyKillChord
kind: 実装
created: 2026-08-24
memory_links:
  - spec/tasks/2026-08-24-anatomia-layers-json-setup.md
  - Assets/Scripts/DesignPhilosophy.md
---

# プログラムドメイン候補の正式定義（spec/domains/*.md 化）

## 目的

2026-08-24 のプログラムドメイン解析で、`anatomia domains suggest` の出力（2回実行し実シンボル数で
裏取り済み）から13件のドメイン候補を抽出した:
combat-core / enemy / mission / skill-runtime / skill-progression / rhythm-music /
stage-barrage / presentation-fx / scenario-narrative / sortie-stageselect / ui-screens /
platform-services / spec-tooling。

これらは `anatomia domains program`（構造的なlayer分類。`anatomia-layers-json-setup` taskで対応）とは
別軸の「ビジネスドメイン」（意味的・spec紐付け）であり、`anatomia domains draft` /
`spec/domains/*.md` で正式に人間レビュー・確定させる必要がある。

## 完了条件

- `spec/domains/` 配下に各候補（または人間レビューで統合/分割調整した結果）のドメイン定義が
  コミットされている。各定義には根拠パス（pathPatterns）とrationaleを残す。
- `anatomia domains list` で確定したドメイン一覧が確認できる。
- 13候補のうち `presentation-fx`（Camera/Sequence/Animation/PostEffect/Voice、241 symbols/12 modules）は
  どのLLM提案にも含まれず今回の解析で独自追加したものなので、正式化時に妥当性を再確認する。
- `spec-tooling`（SinfoniaOperator一式 + Editor/Scripts/SourceDataProvider、523 symbols/6 modules）は
  ゲームプレイでなく開発ツールチェーンのドメインである点を定義に明記する。

## スコープ (編集可ディレクトリ)

- `spec/domains/**`（新規作成）

Runtimeソースコードの変更はこのtaskに含めない。`anatomia-layers-json-setup` taskの
`.anatomia/layers.json`（構造レイヤー）とは独立して進めてよい。
