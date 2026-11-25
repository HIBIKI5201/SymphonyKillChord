"""Validate the repository's lightweight agent entrypoints without external dependencies."""
from pathlib import Path
import json
import re
import sys
import unicodedata

ROOT = Path(__file__).resolve().parents[2]
LIMITS = {
    "AGENTS.md": 1600,
    "CLAUDE.md": 300,
    ".gemini/GEMINI.md": 300,
    "Assets/Scripts/DesignPhilosophy.md": 2000,
    ".agents/skills/skc-feature-flow/SKILL.md": 1400,
    ".claude/skills/skc-feature-flow/SKILL.md": 500,
    ".agents/skills/skc-code-guideline-check/SKILL.md": 2000,
    ".claude/skills/skc-code-guideline-check/SKILL.md": 500,
    "Docs/agent/README.md": 4500,
}
# Skill descriptions stay in every session's listing; bodies load only when used.
# Skills the Unity CLI installs (`unity skill install --local`) are not edited here.
DESCRIPTION_LIMIT = 300
DESCRIPTION_DIRS = [".claude/skills", ".agents/skills", ".codex/skills"]
SHARED_SKILL_DIRS = [".agents/skills", ".codex/skills", ".gemini/skills"]
VENDOR_PREFIXES = ("unity-", "new-unity-project")
LINKED = [
    "AGENTS.md", "Assets/Scripts/DesignPhilosophy.md",
    "Assets/Scripts/CodeGuidelines.md",
    "Assets/Scripts/DesignClassRoles.md", "Assets/Scripts/DesignExamples.md",
    ".agents/skills/skc-code-guideline-check/SKILL.md",
    ".claude/skills/skc-feature-flow/SKILL.md",
    ".claude/skills/skc-code-guideline-check/SKILL.md",
    "Docs/agent/README.md", "Docs/agent/index.md",
]

NOTION_MAP = "SinfoniaOperator/notion-cache-map.json"
UUID_RE = re.compile(r"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}")


def heading_anchor(heading):
    """Match ordinary GitHub Markdown heading anchors, including Japanese text."""
    heading = heading.lower().replace("`", "").replace(" ", "-")
    return "".join(c for c in heading if c in "-_" or
                   unicodedata.category(c)[0] in "LN")


def validate(root):
    """Return metrics and failures for budgets, local links and duplicate skill names."""
    errors, metrics = [], {}
    for relative, limit in LIMITS.items():
        path = root / relative
        if not path.is_file():
            errors.append(f"missing entrypoint: {relative}")
            continue
        size = len(path.read_text(encoding="utf-8-sig"))
        metrics[relative] = size
        if size > limit:
            errors.append(f"budget exceeded: {relative}: {size} > {limit} characters")
    claude = root / "CLAUDE.md"
    if claude.is_file() and not re.search(r"^@AGENTS\.md$", claude.read_text(encoding="utf-8-sig"), re.M):
        errors.append("CLAUDE.md must import AGENTS.md with a line '@AGENTS.md'")
    gemini_md = root / ".gemini/GEMINI.md"
    if gemini_md.is_file() and not re.search(r"^@\.\./AGENTS\.md$", gemini_md.read_text(encoding="utf-8-sig"), re.M):
        errors.append(".gemini/GEMINI.md must import ../AGENTS.md with a line '@../AGENTS.md'")
    linked = [root / p for p in LINKED]
    linked.extend((root / ".agents/skills/skc-feature-flow").rglob("*.md"))
    linked.extend(sorted((root / ".claude/skills").glob("*/SKILL.md")))
    seen_linked = set()
    for path in linked:
        resolved = path.resolve()
        if resolved in seen_linked:
            continue
        seen_linked.add(resolved)
        if not path.is_file():
            errors.append(f"missing document: {path.relative_to(root)}")
            continue
        text = path.read_text(encoding="utf-8-sig")
        for target in re.findall(r"\[[^\]]*\]\(([^)]+)\)", text):
            target, separator, fragment = target.partition("#")
            if "://" in target:
                continue
            destination = (path.parent / target).resolve() if target else path.resolve()
            if not destination.is_relative_to(root.resolve()) or not destination.exists():
                errors.append(f"broken local link: {path.relative_to(root)} -> {target}")
            elif separator and fragment and destination.suffix == ".md":
                headings = re.findall(r"^#{1,6} +(.+)$",
                                      destination.read_text(encoding="utf-8-sig"), re.M)
                if fragment not in {heading_anchor(h) for h in headings}:
                    errors.append(f"broken heading link: {path.relative_to(root)} -> {target}#{fragment}")
    errors.extend(validate_agent_configs(root))
    errors.extend(validate_notion_map(root))
    errors.extend(validate_descriptions(root, metrics))
    names = {}
    for directory in SHARED_SKILL_DIRS:
        skills_dir = root / directory
        if not skills_dir.is_dir():
            continue
        for path in sorted(skills_dir.glob("*/SKILL.md")):
            text = path.read_text(encoding="utf-8-sig")
            match = re.match(r"---\n(.*?)\n---", text, re.S)
            if not match:
                errors.append(f"invalid frontmatter: {path.relative_to(root)}")
                continue
            name = re.search(r"^name: *([^\n]+)", match.group(1), re.M)
            description = re.search(r"^description: *[^\n]+", match.group(1), re.M)
            if not name or not description:
                errors.append(f"missing skill metadata: {path.relative_to(root)}")
                continue
            key = name.group(1).strip().strip('"').strip("'")
            if key != path.parent.name:
                errors.append(f"skill name differs from folder: {path.relative_to(root)}")
            if key in names:
                errors.append(f"duplicate skill: {key}: {names[key]} and {path.relative_to(root)}")
            names[key] = path.relative_to(root)
    return metrics, errors


def read_json(path, errors, label):
    """Load JSON config or record an error when missing or malformed."""
    if not path.is_file():
        errors.append(f"missing config: {label}")
        return None
    try:
        data = json.loads(path.read_text(encoding="utf-8-sig"))
    except ValueError as error:
        errors.append(f"invalid json: {label}: {error}")
        return None
    if not isinstance(data, dict):
        errors.append(f"invalid json object: {label}")
        return None
    return data


def validate_agent_configs(root):
    """Validate Gemini CLI and Antigravity JSON configs stay wired to AGENTS.md, spec MCP, and hooks."""
    errors = []
    gemini_settings = read_json(root / ".gemini/settings.json", errors, ".gemini/settings.json")
    if gemini_settings is not None:
        file_name = gemini_settings.get("context", {}).get("fileName")
        names = file_name if isinstance(file_name, list) else [file_name]
        if "AGENTS.md" not in names:
            errors.append(".gemini/settings.json context.fileName must include AGENTS.md")
        if "spec" not in gemini_settings.get("mcpServers", {}):
            errors.append(".gemini/settings.json must register mcpServers.spec")
        hooks = gemini_settings.get("hooks", {})
        if not hooks.get("SessionStart") or not hooks.get("BeforeTool"):
            errors.append(".gemini/settings.json must define SessionStart and BeforeTool hooks")

    agy_hooks = read_json(root / ".agents/hooks.json", errors, ".agents/hooks.json")
    if agy_hooks is not None:
        events = {event for spec in agy_hooks.values() if isinstance(spec, dict) for event in spec}
        if "SessionStart" not in events or "PreToolUse" not in events:
            errors.append(".agents/hooks.json must define SessionStart and PreToolUse hooks")

    agy_mcp = read_json(root / ".agents/mcp_config.json", errors, ".agents/mcp_config.json")
    if agy_mcp is not None and "spec" not in agy_mcp.get("mcpServers", {}):
        errors.append(".agents/mcp_config.json must register mcpServers.spec")

    return errors


def skill_description(text):
    """Return the one-line description in a SKILL.md frontmatter, or None."""
    match = re.match(r"---\n(.*?)\n---", text, re.S)
    found = match and re.search(r"^description: *([^\n]+)", match.group(1), re.M)
    if not found:
        return None
    value = found.group(1).strip()
    if value.startswith('"'):
        try:
            return json.loads(value)
        except ValueError:
            pass
    return value.strip('"').strip("'")


def validate_descriptions(root, metrics):
    """Keep project skill descriptions short and record the listing size per tool."""
    errors = []
    for directory in DESCRIPTION_DIRS:
        total = 0
        skills_dir = root / directory
        if skills_dir.is_dir():
            for path in sorted(skills_dir.glob("*/SKILL.md")):
                description = skill_description(path.read_text(encoding="utf-8-sig"))
                if description is None:
                    continue
                total += len(description)
                if not path.parent.name.startswith(VENDOR_PREFIXES) and len(description) > DESCRIPTION_LIMIT:
                    errors.append(f"description too long: {path.relative_to(root)}: "
                                  f"{len(description)} > {DESCRIPTION_LIMIT} characters")
        metrics[f"{directory} descriptions (total)"] = total
    return errors


def validate_notion_map(root):
    """Notionページの参照表で、page idの形・呼び名の重複・readersのファイルにidが書かれているかを確かめる。"""
    path = root / NOTION_MAP
    if not path.is_file():
        return []
    try:
        pages = json.loads(path.read_text(encoding="utf-8-sig"))["pages"]
    except (ValueError, KeyError) as error:
        return [f"invalid notion map: {NOTION_MAP}: {error}"]
    errors, seen = [], set()
    for page in pages:
        page_id, name = page.get("id", ""), page.get("name", "")
        if not UUID_RE.fullmatch(page_id) or not name or not page.get("readers"):
            errors.append(f"invalid notion map entry: {name or page_id}")
            continue
        if page_id in seen or name in seen:
            errors.append(f"duplicate notion map entry: {name}")
        seen.update((page_id, name))
        for reader in page["readers"]:
            reader_path = root / reader
            if not reader_path.is_file():
                errors.append(f"missing notion map reader: {name} -> {reader}")
            elif page_id not in reader_path.read_text(encoding="utf-8-sig"):
                errors.append(f"notion map reader lacks page id: {name} -> {reader}")
    return errors


def main():
    metrics, errors = validate(ROOT)
    for path, size in metrics.items():
        limit = LIMITS.get(path)
        print(f"{path}: {size}/{limit} characters" if limit else f"{path}: {size} characters")
    for error in errors:
        print(error, file=sys.stderr)
    print(f"Harness check: {len(errors)} error(s)")
    return bool(errors)


if __name__ == "__main__":
    sys.exit(main())
