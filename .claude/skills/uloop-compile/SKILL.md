---
name: uloop-compile
description: "Compile the Unity project and report errors/warnings. Use after C# edits or when a full Domain Reload compile is needed."
---

# uloop compile

Execute Unity project compilation.

## Usage

```bash
uloop compile [--force-recompile] [--no-wait-for-domain-reload] [--stop-on-external-scene-changes]
```

## Parameters

V3 boolean options take no value — presence means enabled, absence means the documented default. Run `uloop compile --help` to confirm current defaults.

| Parameter | Default | Description |
|-----------|---------|-------------|
| `--force-recompile` | disabled | Force full recompilation (triggers Domain Reload). |
| `--no-wait-for-domain-reload` | (waits by default) | Return without waiting for Domain Reload to complete. |
| `--stop-on-external-scene-changes` | (reloads by default) | Stop instead of reloading when external scene changes are detected. |

## Global Options

| Option | Description |
|--------|-------------|
| `--project-path <path>` | Optional. Use only when the target Unity project is not the current directory. |

## Examples

```bash
# Check compilation
uloop compile

# Force full recompilation
uloop compile --force-recompile

# Force recompilation without waiting for Domain Reload completion
uloop compile --force-recompile --no-wait-for-domain-reload
```

## Output

Returns JSON:
- `Success`: boolean
- `ErrorCount`: number
- `WarningCount`: number

## Troubleshooting

Diagnose the failure mode before retrying.

**Stale lock files** (CLI hangs or shows "Unity is busy" while Unity Editor *is* running):

```bash
uloop fix
```

This removes any leftover lock files (`compiling.lock`, `domainreload.lock`, `serverstarting.lock`) from the Unity project's Temp directory. Then retry `uloop compile`.

**Unity Editor not running** (CLI returns a connection failure and no Unity process is alive):

```bash
uloop launch
```

`uloop launch` auto-detects the project at the current working directory and opens it in the matching Unity Editor version. After Unity finishes launching, retry `uloop compile`.
