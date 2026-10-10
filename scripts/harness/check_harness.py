"""Validate the repository's lightweight agent entrypoints without external dependencies."""
from pathlib import Path
import re
import sys
import unicodedata

ROOT = Path(__file__).resolve().parents[2]
LIMITS = {
    "AGENTS.md": 1600,
    "Assets/Scripts/DesignPhilosophy.md": 2000,
    ".agents/skills/skc-feature-flow/SKILL.md": 1400,
    ".claude/skills/skc-feature-flow/SKILL.md": 500,
    ".claude/skills/skc-code-guideline-check/SKILL.md": 2000,
}
LINKED = [
    "AGENTS.md", "Assets/Scripts/DesignPhilosophy.md",
    "Assets/Scripts/DesignClassRoles.md", "Assets/Scripts/DesignExamples.md",
    ".claude/skills/skc-feature-flow/SKILL.md",
    ".claude/skills/skc-code-guideline-check/SKILL.md",
]


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
    linked = [root / p for p in LINKED]
    linked.extend((root / ".agents/skills/skc-feature-flow").rglob("*.md"))
    for path in linked:
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
    names = {}
    for directory in [".agents/skills", ".codex/skills"]:
        for path in sorted((root / directory).glob("*/SKILL.md")):
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


def main():
    metrics, errors = validate(ROOT)
    for path, size in metrics.items():
        print(f"{path}: {size}/{LIMITS[path]} characters")
    for error in errors:
        print(error, file=sys.stderr)
    print(f"Harness check: {len(errors)} error(s)")
    return bool(errors)


if __name__ == "__main__":
    sys.exit(main())
