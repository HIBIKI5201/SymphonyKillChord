---
task: anatomia-layers-json-setup
project: SymphonyKillChord
kind: 実装
created: 2026-08-24
memory_links:
  - Assets/Scripts/DesignPhilosophy.md
  - spec/tasks/2026-08-24-formalize-program-domains.md
---

# .anatomia/layers.json の新設 — プログラムドメインゲートの未分類解消

## 目的

`anatomia domains program --project symphony-kill-chord` は現状 `.anatomia/layers.json` が
存在しないため、311モジュール / 5372シンボル全件が `no-layer-rule` で未分類になっている
（2026-08-24 のプログラムドメイン解析で確認）。`DesignPhilosophy.md` が既に定義している
クリーンアーキテクチャ6層（Domain/Application/Adaptor/View/InfraStructure/Composition）と、
Runtime外の区分（Develop/Editor/DevelopProducts/外部ツール/ベンダー資産）をそのまま
`layers.json` のglobルールへ落とし込み、Revisorの二層ゲート（program-domain gate）が
将来 enforced 化されても本番アーキテクチャと非本番コードを混同しないようにする。

## 完了条件

- `.anatomia/layers.json` が追加されている。ルールは概ね以下の対応:
  - `Assets/Scripts/Runtime/{0.Utility,1.Domain,2.Application,3.Adaptor,4.View,5.InfraStructure,6.Composition}/**`
    → `shared`/`domain`/`application`/`adaptor`/`view`/`infrastructure`/`composition`
  - `Assets/Scripts/Shaders/**`, `Assets/Scripts/SymphonyFrameWork/**`, `Assets/Settings/**` → `view`/`shared`/`shared`
  - `Assets/Scripts/Develop/**`, `Assets/Level/**` → `develop-test`
  - `Assets/Editor/**`, `Assets/DevelopProducts/{TicketSystem,Utility}/**` → `editor-tooling`
  - `Assets/DevelopProducts/Mocks/**` → `mock-prototype`
  - `Assets/DevelopProducts/{Research,Design}/**` → `research-prototype`
  - `Assets/Arts/**` → `vendor`
  - `SinfoniaOperator/**` → `external-tooling`
- `anatomia domains program --project symphony-kill-chord` の実行結果が
  `unclassified: 0 module(s), 0 symbol(s)` になる。
- Runtime（本番）と Develop/Editor/DevelopProducts/外部ツール/ベンダーが異なる `layer` 値で
  区別されている（本番と非本番が同じ値に丸められていない）。

## スコープ (編集可ディレクトリ)

- `.anatomia/layers.json`（新規追加のみ）

Runtime配下の既存ソースコード・アセットの変更はこのtaskに含めない。
