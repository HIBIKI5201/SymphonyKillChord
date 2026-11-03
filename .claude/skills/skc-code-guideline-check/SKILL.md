---
name: skc-code-guideline-check
description: "Review C# files against this project's coding conventions (Assets/Scripts/CodeGuidelines.md) and Clean Architecture layer rules (Assets/Scripts/DesignPhilosophy.md). Use after writing or editing any C# file under Assets/Scripts or Assets/Editor, or whenever the user asks for a style review, architecture review, layer-dependency check, or general code review of C# changes. Report violations with file:line references, not just prose."
---

# Code Guideline Check

Review C# source against two project documents (Notion counterparts: コード規定 `2a87c2c6-cc02-80f9-a10a-f0b72b95fbb3` / 設計思想 `31c7c2c6-cc02-80f3-92c0-e6929d962476`):

- [Assets/Scripts/CodeGuidelines.md](../../../Assets/Scripts/CodeGuidelines.md) — naming, ordering, comments, formatting
- [Assets/Scripts/DesignPhilosophy.md](../../../Assets/Scripts/DesignPhilosophy.md) — Clean Architecture layers and class-role conventions

Read the two core documents and the relevant layer section of [DesignClassRoles.md](../../../Assets/Scripts/DesignClassRoles.md). Reuse material already read at the same revision in this session. Read examples only when needed.

## Scope

Only review files the user touched (recently edited/added), unless explicitly asked for a full sweep. Skip generated and third-party code (`*.designer.cs`, `Assets/Plugins`).

## Checks

Use CodeGuidelines.md for style and DesignPhilosophy.md / DesignClassRoles.md for layer dependencies and class responsibilities. Do not maintain a second copy of their rules here. Check the changed types, their callers and lifecycle; report conflicts against the canonical source.

## Output format

Report as a list of findings, each with `file:line`, the rule violated, and a one-line fix suggestion. Group by file. If a file is fully compliant, say so briefly so the user knows the check ran.

Don't rewrite files unless the user asks you to fix the issues — default to reporting.
