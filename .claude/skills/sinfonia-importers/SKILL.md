---
name: sinfonia-importers
description: "Fetch project data into the repo with the SinfoniaOperator importers: NotionMarkdownExporter (Notion spec pages -> Docs/NotionSpecifications) and DiscordLogExporter (Discord channel/forum logs -> Docs/DiscordLog). Use when you need current Notion specification text or Discord discussion history and the local snapshot may be stale or missing — e.g. 'refresh the spec export', 'what did the team decide in Discord about X', 'check the design doc for Y', or any question whose answer lives in Notion or Discord rather than in code."
---

# SinfoniaOperator Importers

Two Windows CLI tools pull external project data into the repo as local files.
Both read config from `SinfoniaOperator/sinfonia-operator.env.json` (shared, in git) and
secrets from `SinfoniaOperator/sinfonia-operator.secrets.json` (gitignored), in that order.

| Tool | Source | Destination | Token |
|---|---|---|---|
| `NotionMarkdownExporter` | Notion root page + subtree | `Docs/NotionSpecifications/` | `NOTION_TOKEN` |
| `DiscordLogExporter` | channels in `DISCORD_LOG_CHANNEL_IDS` | `Docs/DiscordLog/` | `DISCORD_BOT_TOKEN` |

Both output directories are **gitignored snapshots**, not live data. Read them first; only
run an importer when the snapshot is missing, or is old enough to matter for the question.
Check staleness with file mtimes (`ls -l`), not git.

## Before running either

Both hit live APIs, take minutes, and bulk-overwrite their output directory.
**Confirm with the user before running**, and say which tool and roughly how much it will rewrite.
Never hardcode a token; if one is missing the tool says so — ask the user to fill in the secrets file.

## Notion → Docs/NotionSpecifications

~1,800 Markdown pages plus downloaded images/attachments.

```bash
./SinfoniaOperator/NotionMarkdownExporter.exe --output "Docs/NotionSpecifications"
```

- `NOTION_EXPORT_OUTPUT` is a **CWD-relative** path. Run from the repository root, or pass
  `--output` explicitly as above — otherwise it silently writes a second copy under
  `SinfoniaOperator/Docs/NotionSpecifications/`.
- Re-runs are incremental: it compares each page's Notion `last_edited_time` against
  `.notion-export-manifest.json` and only re-fetches changed/new pages. Manually added files
  under the output dir are left alone. A corrupted manifest aborts the export by design.
- `--root "<page URL or id>"` overrides `NOTION_EXPORT_ROOT_PAGE`; `--no-assets` skips
  image/attachment downloads (much faster when you only need text).
- If it appears to hang, see the memory note on the stall watchdog / environmental freezes
  before assuming the tool is broken.

For comparing this export against the implementation, use the `notion-spec-diff-check` skill —
it owns the categorization scheme and the precedent report.

## Discord → Docs/DiscordLog

One `.txt` per channel, and per forum thread for forum channels; messages oldest-first with
body, attachment URLs, and embed text.

```bash
./SinfoniaOperator/DiscordLogExporter.exe
```

- Output is fixed at `<repo root>/Docs/DiscordLog/`; the tool locates the git root itself,
  so CWD does not matter.
- It exports **only** the channels listed in `DISCORD_LOG_CHANNEL_IDS`. To pull a channel that
  isn't listed, add its id to `sinfonia-operator.env.json` first (that file is committed —
  mention the config change to the user).
- `--config <PATH>` points at an alternate settings JSON. `--help` for usage.
- If it errors about empty message bodies, the bot is missing `Message Content Intent` in the
  Discord Developer Portal — a setup problem for the user, not something to work around.
  The tool deliberately refuses to overwrite existing logs with empty ones.

## Reading the output

Both directories are large. Prefer `grep -ril` over the tree to find the relevant files, then
read only those. Filenames encode the source: `<channel>_<id>.txt`,
`<forum>_<thread>_<id>.txt`, and Notion pages keep their page titles.
