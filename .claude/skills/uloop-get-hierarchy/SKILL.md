---
name: uloop-get-hierarchy
description: "Get the Unity scene hierarchy as a structured tree. Use for parent-child structure, descendants, roots, or subtrees under objects the user currently selected."
---

# uloop get-hierarchy

Get Unity Hierarchy structure from the whole scene, a root path, or selected Hierarchy objects.

Use this for hierarchy structure, especially descendants under the current selection. Use `find-game-objects --search-mode Selected` when you need selected object details or component properties.

## Usage

```bash
uloop get-hierarchy [options]
```

## Parameters

V3 boolean options take no value — presence means enabled, absence means the documented default. Run `uloop get-hierarchy --help` to confirm current defaults.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `--root-path` | string | - | Root GameObject path to start from |
| `--max-depth` | integer | `-1` | Maximum depth (-1 for unlimited) |
| `--no-include-components` | flag | (included by default) | Exclude component information |
| `--no-include-inactive` | flag | (included by default) | Exclude inactive GameObjects |
| `--include-paths` | flag | disabled | Include full path information |
| `--use-components-lut` | string | `auto` | Use LUT for components (`auto`, `true`, `false`) |
| `--use-selection` | flag | disabled | Use selected GameObject(s) as root(s). When set, `--root-path` is ignored. |

## Global Options

| Option | Description |
|--------|-------------|
| `--project-path <path>` | Optional. Use only when the target Unity project is not the current directory. |

## Examples

```bash
# Get entire hierarchy
uloop get-hierarchy

# Get hierarchy from specific root
uloop get-hierarchy --root-path "Canvas/UI"

# Limit depth
uloop get-hierarchy --max-depth 2

# Without components
uloop get-hierarchy --no-include-components

# Get hierarchy from currently selected GameObjects
uloop get-hierarchy --use-selection
```

## Output

Returns JSON with:
- `message` (string): Human-readable guidance pointing at the saved file
- `hierarchyFilePath` (string): Filesystem path to the JSON file that contains the actual hierarchy data

The hierarchy itself is **not** in the response — it is written to the file at `hierarchyFilePath`. Open that file to read the `Context` and `Hierarchy` payload (GameObject tree, components, etc.).
