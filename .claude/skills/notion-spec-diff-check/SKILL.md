---
name: notion-spec-diff-check
description: "Find gaps and inconsistencies between the Notion game specification (mirrored at Docs/NotionSpecifications) and the actual implementation (Assets/Scripts/Runtime and related). Use whenever the user asks to check spec/implementation drift, find undocumented systems, find unimplemented spec items, refresh the Notion export, or audit whether a specific feature/system matches its written spec. Produces a categorized report (spec-but-no-impl / impl-but-no-spec / both-incomplete) with file:line evidence, following the precedent at Docs/仕様書と実装の差分分析_2026-07-27.md."
---

# Notion Spec ↔ Implementation Diff Check

This project mirrors its Notion specification into the repo as Markdown
(`Docs/NotionSpecifications/`) via a custom exporter, and there's already one
full-scale precedent report at
[Docs/仕様書と実装の差分分析_2026-07-27.md](../../../Docs/仕様書と実装の差分分析_2026-07-27.md).
Read that file first — it defines the categorization scheme this skill reuses and shows
what good evidence (file:line citations on both sides) looks like in practice.

## Step 1: Decide whether the Notion export needs refreshing

`Docs/NotionSpecifications/` is a point-in-time snapshot, not live data. Before trusting it:

- Check `Docs/NotionSpecifications/Symphony Kill Chord.md` (or any page) file timestamps / git log to gauge staleness.
- If the user wants current data, or the export looks old, refresh it with the exporter at
  `SinfoniaOperator/NotionMarkdownExporter/`.

**Refreshing is a bulk overwrite of ~1,800 files** (per the precedent report: 1,747 Notion pages).
Before running it:
1. `git status` on `Docs/NotionSpecifications/` — if there are uncommitted local edits under
   that path (this has happened before — see the precedent report's note about
   `Docs/Missionステップ用バフ・デバフ付与機構_計画書.md`), stash or commit them first, or warn the user they'll be overwritten.
2. Confirm with the user before running the exporter — it hits the live Notion API and takes time.
3. **Run it from the repository root**, not from `SinfoniaOperator/`. `NOTION_EXPORT_OUTPUT` in
   `SinfoniaOperator/sinfonia-operator.env.json` is the relative path `Docs/NotionSpecifications`,
   resolved against the process's current working directory — running from the wrong directory
   silently writes a second copy under `SinfoniaOperator/Docs/NotionSpecifications/` instead of
   updating the real one (this exact mistake is documented in the precedent report §0.2).
   ```bash
   # from repo root:
   ./SinfoniaOperator/NotionMarkdownExporter/bin/Release/net10.0/win-x64/publish/NotionMarkdownExporter.exe
   ```
   Or pass `--output "Docs/NotionSpecifications"` explicitly to be safe regardless of CWD.
4. Requires `NOTION_TOKEN` to be configured (gitignored `SinfoniaOperator/sinfonia-operator.settings.json`,
   or environment variable) — if missing, the exporter will say so; don't try to hardcode a token.
5. After it finishes, sanity-check the page/database counts it prints against the last known
   counts (the precedent report table in §0.3 is a reference point) — a sudden large jump or drop
   usually means something changed structurally in Notion and deserves a second look before
   treating the new export as ground truth.

If the user just wants a diff check and doesn't ask for fresh data, skip this step and use the
existing export as-is — say so in the report so the reader knows the data's age.

## Step 2: Scope the check

The full codebase is ~842 files / 60,000 lines against ~1,800 spec pages — too large to diff
exhaustively in one pass with reliable accuracy. Scope every run:

- **If the user names a system/feature** ("スキルツリー", "ミッションのクリア条件", "カメラ" etc.),
  scope to that. This is the common case and the one to default to when the request is specific.
- **If the user wants a broad/full sweep**, split by domain (e.g. one pass per top-level Runtime
  folder like `InGame/Mission`, `InGame/Buff`, `OutGame/StageSelect`, `OutGame/SkillTree`, ...,
  matched against the corresponding `システム概要/システムリスト/*.md` / `仕様概要/仕様リスト/*.md`
  pages) and spawn one Explore or general-purpose subagent per domain in parallel rather than doing
  it serially yourself — each domain is a self-contained research task. Merge their findings into
  one report afterward, keeping the categorization scheme consistent across domains.
- Don't silently narrow scope without saying so — if you decide to sample rather than cover
  everything the user asked for, tell them what you covered and what you didn't.

## Step 3: Compare spec to implementation

For the scoped domain:

1. Find the relevant Notion pages. Game-design-facing content lives under
   `Docs/NotionSpecifications/Symphony Kill Chord/仕様概要/仕様リスト/`; technical/system specs
   (often written from the actual code, sometimes citing class names and file paths directly)
   live under `.../システム概要/システムリスト/`. Grep for the feature name in both trees — Japanese
   terminology may differ from the code's English class names, so also check
   `Docs/NotionSpecifications/Symphony Kill Chord/用語.md` for the mapping.
2. Find the relevant implementation. Follow the layer structure from
   [Assets/Scripts/DesignPhilosophy.md](../../../Assets/Scripts/DesignPhilosophy.md) — a feature's
   logic is usually spread across `1.Domain/`, `2.Application/`, `3.Adaptor/` under a matching
   folder name (e.g. `InGame/Mission/ClearCondition/`). Master data instances often live under
   `Assets/Level/Data/Master/`.
3. For each spec statement, verify it against the code with a concrete citation (class/method
   exists at `path:line`, or doesn't). For each implemented behavior with no clear spec coverage,
   note where you looked for it and confirmed it's absent.
4. Distinguish "no traceable connection" (B: needs documenting) from "described but wrong/partial"
   (C: both need work) from "spec exists, code doesn't" (A: needs implementing) — don't lump
   partial matches into a binary present/absent.

## Output format

Follow the structure of the precedent report exactly enough that it reads as one continuous
series of audits, not a one-off format:

```
# 仕様書と実装の差分分析

作成日: <today's date>
対象ブランチ: `<current branch>`
対象コード: `<scoped path(s)>`
対象仕様書: <export freshness — refreshed today, or "既存エクスポート (日付不明/概算)">

## 0. 調査の前提
(scope covered, scope explicitly NOT covered, export freshness caveat)

| 記号 | 意味 |
| --- | --- |
| A | 仕様書にあるが実装が無い |
| B | 実装があるが仕様書に無い |
| C | 双方に穴がある |

## A. システム側に足りていないもの
### A-1. <title>
**仕様書の記述** — `<path>:<line>` + quote
**実装の状況** — `<path>:<line>` citations, or explicit "該当なし"
**優先度**: 最高 / 高 / 中 / 要確認

## B. 仕様書側に足りていないもの
(same shape, reversed)

## C. 双方に穴があるもの
(same shape)
```

Order findings by priority within each section, most severe first. Save the report as
`Docs/仕様書と実装の差分分析_<YYYY-MM-DD>.md` (matching the existing filename convention) rather
than overwriting the precedent report — each run is a dated snapshot, and older ones stay as
history. Don't create the file until you've actually gathered evidence; don't pad the report with
low-confidence guesses just to fill out a section — "要確認" is a legitimate priority when the
answer genuinely depends on information you don't have access to (e.g. a Notion page not yet
exported, or a design decision only the team can make).
