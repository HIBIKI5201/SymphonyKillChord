#!/usr/bin/env python3
"""Codex CLI execution wrapper with rate-limit (capacity) protection.

Claude Code から Codex CLI へ実装を委任するためのラッパー。
実行前に Codex の残枠を確認し、残量が閾値未満なら **API を叩かずに** 終了する。

Exit codes
----------
0 : Codex の実行・生成に成功した
1 : 一般的なエラー（CLI 不在 / 実行失敗 / 生成物なし など）
2 : 残量不足によるスキップ（レートリミット保護）※呼び出し側はフォールバックへ

Usage
-----
    python scripts/codex_runner.py "<prompt>" "<output_path>"
    python scripts/codex_runner.py --check-only
    python scripts/codex_runner.py --check-only --json
    python scripts/codex_runner.py --prompt-file plan.md src/Foo.cs --threshold 15
"""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import time
from dataclasses import dataclass, asdict, field
from pathlib import Path
from typing import Any, Iterator, Optional

# --------------------------------------------------------------------------
# Exit codes
# --------------------------------------------------------------------------
EXIT_OK = 0
EXIT_ERROR = 1
EXIT_INSUFFICIENT_CAPACITY = 2

DEFAULT_THRESHOLD_PERCENT = 10.0
DEFAULT_TIMEOUT_SEC = 1800
TAIL_BYTES = 4 * 1024 * 1024  # rollout ログの末尾のみ読む上限
MAX_ROLLOUTS_TO_SCAN = 12     # 新しい順にこの数だけ rate_limits を探す


def log(msg: str) -> None:
    """人間向けログは stderr へ（stdout は --json 用に汚さない）。"""
    print(msg, file=sys.stderr, flush=True)


# --------------------------------------------------------------------------
# Codex home / binary resolution
# --------------------------------------------------------------------------
def codex_home() -> Path:
    return Path(os.environ.get("CODEX_HOME") or (Path.home() / ".codex"))


def resolve_codex_bin() -> Optional[str]:
    """codex 実行ファイルを解決する。

    Codex Desktop 同梱版は PATH に載らず、ハッシュ付きディレクトリに入るため
    次の優先順で探索する:
      1. 環境変数 CODEX_BIN
      2. PATH 上の codex
      3. ~/.codex/config.toml の CODEX_CLI_PATH
      4. %LOCALAPPDATA%/OpenAI/Codex/bin/<hash>/codex.exe のうち最新
    """
    env_bin = os.environ.get("CODEX_BIN")
    if env_bin and Path(env_bin).exists():
        return env_bin

    found = shutil.which("codex")
    if found:
        return found

    # config.toml に Desktop が書き込む実体パス
    cfg = codex_home() / "config.toml"
    if cfg.is_file():
        try:
            text = cfg.read_text(encoding="utf-8", errors="replace")
            m = re.search(r"""CODEX_CLI_PATH\s*=\s*['"](.+?)['"]""", text)
            if m and Path(m.group(1)).exists():
                return m.group(1)
        except OSError:
            pass

    # 同梱バイナリを直接探す（更新でハッシュが変わるので最新を採用）
    local = os.environ.get("LOCALAPPDATA")
    roots = [Path(local) / "OpenAI" / "Codex" / "bin"] if local else []
    roots.append(Path.home() / ".local" / "share" / "openai" / "codex" / "bin")
    candidates: list[Path] = []
    for root in roots:
        if root.is_dir():
            candidates.extend(root.glob("*/codex.exe"))
            candidates.extend(root.glob("*/codex"))
    candidates = [c for c in candidates if c.is_file()]
    if candidates:
        return str(max(candidates, key=lambda p: p.stat().st_mtime))

    return None


# --------------------------------------------------------------------------
# Capacity check
# --------------------------------------------------------------------------
@dataclass
class CapacityReport:
    allowed: bool
    remaining_percent: Optional[float]
    threshold_percent: float
    reason: str
    source: Optional[str] = None
    plan_type: Optional[str] = None
    windows: list[dict[str, Any]] = field(default_factory=list)
    snapshot_age_hours: Optional[float] = None
    degraded: bool = False  # 残量を確定できず fail-open した場合 True

    def to_json(self) -> str:
        return json.dumps(asdict(self), ensure_ascii=False, indent=2)


def _iter_recent_rollouts(sessions_dir: Path) -> Iterator[Path]:
    """rollout-*.jsonl を更新時刻の新しい順に返す。"""
    try:
        files = [p for p in sessions_dir.rglob("rollout-*.jsonl") if p.is_file()]
    except OSError:
        return
    for p in sorted(files, key=lambda x: x.stat().st_mtime, reverse=True):
        yield p


def _tail_text(path: Path, max_bytes: int = TAIL_BYTES) -> str:
    """巨大な rollout でも末尾だけ読む（先頭の欠けた行は捨てる）。"""
    size = path.stat().st_size
    with path.open("rb") as f:
        if size > max_bytes:
            f.seek(size - max_bytes)
            f.readline()
        data = f.read()
    return data.decode("utf-8", errors="replace")


def _find_rate_limits(obj: Any) -> Optional[dict]:
    """ネスト構造から "rate_limits" を再帰的に探す（スキーマ変更に強くする）。"""
    if isinstance(obj, dict):
        rl = obj.get("rate_limits")
        if isinstance(rl, dict):
            return rl
        for v in obj.values():
            hit = _find_rate_limits(v)
            if hit is not None:
                return hit
    elif isinstance(obj, list):
        for v in obj:
            hit = _find_rate_limits(v)
            if hit is not None:
                return hit
    return None


def _latest_rate_limits() -> tuple[Optional[dict], Optional[Path], Optional[float]]:
    """直近の rate_limits スナップショットと、その取得元・記録時刻を返す。"""
    sessions = codex_home() / "sessions"
    if not sessions.is_dir():
        return None, None, None

    for scanned, path in enumerate(_iter_recent_rollouts(sessions)):
        if scanned >= MAX_ROLLOUTS_TO_SCAN:
            break
        try:
            text = _tail_text(path)
        except OSError:
            continue
        # 末尾（=最新）から探す
        for line in reversed(text.splitlines()):
            if '"rate_limits"' not in line:
                continue
            try:
                obj = json.loads(line)
            except json.JSONDecodeError:
                continue
            rl = _find_rate_limits(obj)
            if rl:
                return rl, path, path.stat().st_mtime
    return None, None, None


def _window_remaining(win: Any, now: float) -> Optional[float]:
    """1 ウィンドウあたりの残量 %。リセット時刻を過ぎていれば満タン扱い。"""
    if not isinstance(win, dict):
        return None
    used = win.get("used_percent")
    if not isinstance(used, (int, float)):
        return None
    resets_at = win.get("resets_at")
    if isinstance(resets_at, (int, float)) and resets_at > 0 and now >= resets_at:
        return 100.0
    return max(0.0, 100.0 - float(used))


def check_codex_capacity(
    threshold_percent: float = DEFAULT_THRESHOLD_PERCENT,
    fail_closed: bool = False,
) -> CapacityReport:
    """Codex の残枠を判定する。

    ~/.codex/sessions/**/rollout-*.jsonl に記録された最新の rate_limits を読み、
    残量 (= 100 - used_percent) が threshold_percent 未満なら allowed=False。

    セッション情報が取れない場合は既定で **実行許可 (fail-open)** する。
    誤検知で作業を止めないためだが、--fail-closed で反転できる。
    """
    now = time.time()

    def fallback(reason: str) -> CapacityReport:
        return CapacityReport(
            allowed=not fail_closed,
            remaining_percent=None,
            threshold_percent=threshold_percent,
            reason=reason
            + ("（fail-closed によりスキップ）" if fail_closed else "（判定不能のため実行を許可）"),
            degraded=True,
        )

    try:
        rl, src, mtime = _latest_rate_limits()
    except Exception as exc:  # 想定外でも残量判定でクラッシュさせない
        return fallback(f"残量情報の読み取りに失敗しました: {exc!r}")

    if rl is None:
        return fallback("rate_limits を含むセッションログが見つかりませんでした")

    plan_type = rl.get("plan_type") if isinstance(rl.get("plan_type"), str) else None

    # 無制限クレジットがあるなら常に許可
    credits = rl.get("credits")
    if isinstance(credits, dict) and credits.get("unlimited") is True:
        return CapacityReport(
            allowed=True,
            remaining_percent=100.0,
            threshold_percent=threshold_percent,
            reason="credits.unlimited=true のため残量制限なし",
            source=str(src) if src else None,
            plan_type=plan_type,
        )

    windows: list[dict[str, Any]] = []
    remainings: list[float] = []
    for key in ("primary", "secondary"):
        win = rl.get(key)
        rem = _window_remaining(win, now)
        if rem is None:
            continue
        remainings.append(rem)
        windows.append(
            {
                "name": key,
                "remaining_percent": rem,
                "used_percent": win.get("used_percent"),
                "window_minutes": win.get("window_minutes"),
                "resets_at": win.get("resets_at"),
            }
        )

    if not remainings:
        return fallback("rate_limits に有効な used_percent がありませんでした")

    remaining = min(remainings)  # 最も逼迫したウィンドウで判断
    age_hours = round((now - mtime) / 3600.0, 2) if mtime else None
    allowed = remaining >= threshold_percent

    reason = (
        f"残量 {remaining:.1f}% ≧ しきい値 {threshold_percent:.1f}% のため実行可能"
        if allowed
        else f"残量 {remaining:.1f}% < しきい値 {threshold_percent:.1f}% のためスキップ"
    )

    return CapacityReport(
        allowed=allowed,
        remaining_percent=remaining,
        threshold_percent=threshold_percent,
        reason=reason,
        source=str(src) if src else None,
        plan_type=plan_type,
        windows=windows,
        snapshot_age_hours=age_hours,
    )


# --------------------------------------------------------------------------
# Codex execution
# --------------------------------------------------------------------------
CODE_FENCE_RE = re.compile(r"```[^\n]*\n(.*?)```", re.DOTALL)


def _extract_code(text: str) -> Optional[str]:
    """最終メッセージから最大のコードフェンスを取り出す（保険用）。"""
    blocks = CODE_FENCE_RE.findall(text)
    if not blocks:
        return None
    return max(blocks, key=len).strip() + "\n"


def build_prompt(user_prompt: str, output_path: Path, workdir: Path) -> str:
    """出力先を明示した最終プロンプトを組み立てる。"""
    try:
        rel = output_path.resolve().relative_to(workdir.resolve())
        shown = rel.as_posix()
    except ValueError:
        shown = str(output_path)

    return (
        f"{user_prompt}\n\n"
        "---\n"
        "## 出力に関する厳守事項\n"
        f"- 生成した実装は必ずファイル `{shown}` に書き込むこと。\n"
        "- 既存ファイルがある場合は内容を置き換えること。\n"
        "- 説明文だけで終わらせず、必ずファイルへの書き込みを完了させること。\n"
        "- 作業対象外のファイルは変更しないこと。\n"
    )


def run_codex(
    codex_bin: str,
    prompt: str,
    output_path: Path,
    workdir: Path,
    model: Optional[str],
    sandbox: str,
    timeout: int,
) -> int:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    before_mtime = output_path.stat().st_mtime if output_path.exists() else None

    last_msg = output_path.parent / f".{output_path.name}.codex-last-message.md"

    cmd = [
        codex_bin,
        "exec",
        "-",                       # プロンプトは stdin から（長文/日本語のコマンドライン長対策）
        "--cd", str(workdir),
        "--sandbox", sandbox,
        "--skip-git-repo-check",
        "--color", "never",
        "-o", str(last_msg),
    ]
    if model:
        cmd += ["--model", model]

    log(f"[codex_runner] 実行: {Path(codex_bin).name} exec --sandbox {sandbox} -> {output_path}")

    env = dict(os.environ, PYTHONIOENCODING="utf-8")
    try:
        proc = subprocess.run(
            cmd,
            input=prompt,
            text=True,
            encoding="utf-8",
            errors="replace",
            timeout=timeout,
            cwd=str(workdir),
            env=env,
        )
    except FileNotFoundError:
        log(f"[codex_runner] ERROR: codex を実行できません: {codex_bin}")
        return EXIT_ERROR
    except subprocess.TimeoutExpired:
        log(f"[codex_runner] ERROR: タイムアウト（{timeout}s）しました")
        return EXIT_ERROR

    if proc.returncode != 0:
        log(f"[codex_runner] ERROR: codex exec が終了コード {proc.returncode} で失敗しました")
        # レートリミットに実際に当たった場合も呼び出し側でフォールバックできるようにする
        if _looks_rate_limited(last_msg):
            log("[codex_runner] レートリミット到達を検出したため exit 2 を返します")
            return EXIT_INSUFFICIENT_CAPACITY
        return EXIT_ERROR

    # 生成結果の確認
    if output_path.exists() and output_path.stat().st_size > 0:
        after = output_path.stat().st_mtime
        if before_mtime is None or after > before_mtime:
            log(f"[codex_runner] OK: {output_path} を生成/更新しました")
            _cleanup(last_msg)
            return EXIT_OK
        log(f"[codex_runner] WARN: {output_path} が更新されていません")

    # 保険: Codex がファイルを書かず本文だけ返した場合、コードフェンスを救出する
    if last_msg.is_file():
        text = last_msg.read_text(encoding="utf-8", errors="replace")
        code = _extract_code(text)
        if code:
            output_path.write_text(code, encoding="utf-8")
            log(f"[codex_runner] OK: 最終メッセージのコードブロックから {output_path} を生成しました")
            _cleanup(last_msg)
            return EXIT_OK

    log("[codex_runner] ERROR: 生成物を確認できませんでした")
    return EXIT_ERROR


def _looks_rate_limited(last_msg: Path) -> bool:
    if not last_msg.is_file():
        return False
    try:
        text = last_msg.read_text(encoding="utf-8", errors="replace").lower()
    except OSError:
        return False
    return any(k in text for k in ("rate limit", "usage limit", "quota", "429"))


def _cleanup(path: Path) -> None:
    try:
        path.unlink(missing_ok=True)
    except OSError:
        pass


# --------------------------------------------------------------------------
# CLI
# --------------------------------------------------------------------------
def main(argv: Optional[list[str]] = None) -> int:
    parser = argparse.ArgumentParser(
        prog="codex_runner.py",
        description="残量チェック付きで Codex CLI に実装を委任する。",
    )
    parser.add_argument("prompt", nargs="?", help="Codex に渡すプロンプト")
    parser.add_argument("output", nargs="?", help="生成コードの出力先パス")
    parser.add_argument("--prompt-file", help="プロンプトをファイルから読む（prompt 引数の代わり）")
    parser.add_argument("--check-only", action="store_true", help="残量チェックのみ実行（Codex は呼ばない）")
    parser.add_argument("--json", action="store_true", help="残量レポートを JSON で stdout に出力")
    parser.add_argument(
        "--threshold", type=float, default=DEFAULT_THRESHOLD_PERCENT,
        help=f"残量しきい値パーセント（既定 {DEFAULT_THRESHOLD_PERCENT}）",
    )
    parser.add_argument(
        "--fail-closed", action="store_true",
        help="残量を判定できないときに実行を許可せずスキップ(exit 2)する",
    )
    parser.add_argument("--skip-capacity-check", action="store_true", help="残量チェックを行わない")
    parser.add_argument("--model", help="使用モデル（既定は config.toml の設定）")
    parser.add_argument(
        "--sandbox", default="workspace-write",
        choices=["read-only", "workspace-write", "danger-full-access"],
        help="Codex のサンドボックスポリシー（既定 workspace-write）",
    )
    parser.add_argument("--cd", dest="workdir", default=".", help="作業ルート（既定 カレント）")
    parser.add_argument("--timeout", type=int, default=DEFAULT_TIMEOUT_SEC, help="タイムアウト秒")
    args = parser.parse_args(argv)

    # Windows コンソールでも日本語を落とさない
    for stream in (sys.stdout, sys.stderr):
        try:
            stream.reconfigure(encoding="utf-8", errors="replace")  # type: ignore[attr-defined]
        except (AttributeError, ValueError):
            pass

    # ---- 1. 残量チェック -------------------------------------------------
    if not args.skip_capacity_check:
        report = check_codex_capacity(args.threshold, fail_closed=args.fail_closed)

        if args.json:
            print(report.to_json())
        else:
            head = "OK" if report.allowed else "SKIP"
            log(f"[codex_runner] 残量チェック {head}: {report.reason}")
            for w in report.windows:
                mins = w.get("window_minutes")
                span = f"{mins}分" if isinstance(mins, (int, float)) else "不明"
                log(f"  - {w['name']}: 残り {w['remaining_percent']:.1f}% (ウィンドウ {span})")

        if not report.allowed:
            log("[codex_runner] 残枠不足のため Codex 呼び出しをスキップします (exit 2)")
            return EXIT_INSUFFICIENT_CAPACITY

    if args.check_only:
        return EXIT_OK

    # ---- 2. 引数検証 -----------------------------------------------------
    if args.prompt_file:
        pf = Path(args.prompt_file)
        if not pf.is_file():
            log(f"[codex_runner] ERROR: プロンプトファイルが見つかりません: {pf}")
            return EXIT_ERROR
        prompt_text = pf.read_text(encoding="utf-8", errors="replace")
        output_arg = args.output or args.prompt
    else:
        prompt_text = args.prompt or ""
        output_arg = args.output

    if not prompt_text.strip() or not output_arg:
        log("[codex_runner] ERROR: プロンプトと出力先パスの両方が必要です")
        log('  例) python scripts/codex_runner.py "<prompt>" "<output_path>"')
        return EXIT_ERROR

    # ---- 3. Codex 実行 ---------------------------------------------------
    codex_bin = resolve_codex_bin()
    if not codex_bin:
        log("[codex_runner] ERROR: codex CLI が見つかりません。")
        log("  CODEX_BIN 環境変数で実行ファイルのパスを指定してください。")
        return EXIT_ERROR

    workdir = Path(args.workdir).resolve()
    output_path = Path(output_arg)
    if not output_path.is_absolute():
        output_path = workdir / output_path

    final_prompt = build_prompt(prompt_text, output_path, workdir)
    return run_codex(
        codex_bin=codex_bin,
        prompt=final_prompt,
        output_path=output_path,
        workdir=workdir,
        model=args.model,
        sandbox=args.sandbox,
        timeout=args.timeout,
    )


if __name__ == "__main__":
    sys.exit(main())
