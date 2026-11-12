// Unity の検証（recompile・テスト・Play の開始）の前に、人の作業を守るための確認。
// 未保存の Scene・Prefab Stage、または Play 中なら終了コード 2 で止まる。状態を確かめられないときも止まる。
// 開いている Editor が無いときは、守る作業も無いので通す。
//
//   node scripts/harness/unity-preflight.mjs          手動で確認する（Codex など）
//   node scripts/harness/unity-preflight.mjs --hook   Claude Code の PreToolUse hook として、標準入力の Bash コマンドを見て確認する
//
// ユーザーに確認したうえで続けるときは、コマンドの先頭に SKC_UNITY_PREFLIGHT=confirmed を付ける（hook のときだけ有効）。
import { spawnSync } from "node:child_process";
import { dirname, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const BLOCK = 2;
const UNITY_TIMEOUT_MS = 60000;
// Unity CLI の終了コード 12（EDITOR_UNREACHABLE）: 対象の Editor が動いていない。
const NO_EDITOR = 12;
const CONFIRMED = /\bSKC_UNITY_PREFLIGHT=confirmed\b/;
// コマンドの先頭（環境変数の代入の後を含む）に来た unity だけを対象にし、grep の引数などは除く。
const UNITY_CALL = /(?:^|[;&|(\n])\s*(?:[A-Za-z_][A-Za-z0-9_]*=\S*\s+)*(?:&\s*)?unity(?:\.exe)?\s+([^;&|\n]*)/g;
// Editor でコンパイル・テスト・Play を始める呼び出し。止める・一時停止・状態の確認は人の作業を壊さないので含めない。
const GUARDED = [
    [/^recompile\b/, "recompile"],
    [/^(?:watch\s+)?test\b/, "test"],
    [/^command\s+(?:test[ .]run|run_tests)\b/, "command test run"],
    [/^command\s+(?:editor[ .]playmode[ .]start|editor_play)\b/, "command editor.playmode start"],
    [/^command\s+script[ .]recompile\b(?!_status)/, "command script recompile"],
];

// Play 状態と未保存のシーンを 1 行ずつ返す。1 行目が playing=0/1、2 行目以降が未保存の項目。
// Editor 側の eval が using を受け付けるとは限らないので、型は完全な名前で書く。
export const PROBE = `var lines = new System.Collections.Generic.List<string>();
lines.Add(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ? "playing=1" : "playing=0");
for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
{
    var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
    if (scene.isDirty) { lines.Add("Scene: " + (string.IsNullOrEmpty(scene.path) ? "(Untitled)" : scene.path)); }
}
var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
if (stage != null && stage.scene.isDirty) { lines.Add("Prefab: " + stage.assetPath); }
return string.Join("\\n", lines);`;

/** Bash コマンドのうち、確認が要る unity の呼び出し名を返す。要らなければ null。 */
export function findGuardedCommand(command) {
    if (!command || CONFIRMED.test(command)) { return null; }
    for (const [, rest] of command.matchAll(UNITY_CALL)) {
        const args = rest.trim();
        for (const [pattern, name] of GUARDED) {
            if (pattern.test(args)) { return name; }
        }
    }
    return null;
}

/** `unity command script eval --result-only` の出力から、C# が返した文字列を取り出す。 */
export function extractEvalResult(stdout) {
    let value;
    try { value = JSON.parse(String(stdout ?? "").trim()); } catch { return null; }
    for (let depth = 0; depth < 6; depth++) {
        if (typeof value === "string") { return value; }
        if (!value || typeof value !== "object") { return null; }
        if (value.success === false || value.Success === false) { return null; }
        const next = ["result", "Result", "returnValue", "value", "data"].find((key) => value[key] !== undefined);
        if (!next) { return null; }
        value = value[next];
    }
    return null;
}

/** 調べた結果から、止まる理由を返す。問題がなければ空の配列。 */
export function parseProbe(result) {
    const lines = String(result ?? "").split(/\r?\n/).map((line) => line.trim()).filter(Boolean);
    if (!/^playing=[01]$/.test(lines[0] ?? "")) { return [`Unity の状態を読み取れなかった: ${result}`]; }
    const reasons = lines.slice(1).map((item) => `未保存: ${item}`);
    if (lines[0] === "playing=1") { reasons.unshift("Play 中"); }
    return reasons;
}

/** このリポジトリを開いている Editor の状態を Unity CLI で調べ、止まる理由を返す。 */
function probeUnity() {
    const unity = process.env.UNITY_CLI_EXECUTABLE || "unity";
    const run = spawnSync(unity, [
        "command", "script", "eval", "--result-only", "--project-path", ROOT, "--", "--code", PROBE, "--timeout", "15000",
    ], { cwd: ROOT, encoding: "utf8", shell: false, windowsHide: true, timeout: UNITY_TIMEOUT_MS });
    if (run.status === NO_EDITOR) { return []; }
    const result = run.status === 0 ? extractEvalResult(run.stdout) : null;
    if (result === null) {
        const detail = run.error?.message || run.stderr || run.stdout || `exit ${run.status}`;
        return [`Unity の状態を確かめられなかった（unity コマンドが無い、または Editor が応答しない）: ${String(detail).trim()}`];
    }
    return parseProbe(result);
}

function report(reasons, tool) {
    const target = tool ? `unity ${tool} の前の確認` : "Unity 検証の前の確認";
    console.error([
        `[unity-preflight] ${target}で止まった。`,
        ...reasons.map((reason) => `- ${reason}`),
        "人の作業かもしれないので、勝手に保存・破棄・停止しない。ユーザーに確認すること。",
        "自分が始めた Play なら `unity command editor.playmode stop` で止めてよい。",
        "ユーザーの了承を得て続けるときは、コマンドの先頭に SKC_UNITY_PREFLIGHT=confirmed を付ける。",
    ].join("\n"));
}

async function readStdin() {
    let text = "";
    for await (const chunk of process.stdin) { text += chunk; }
    return text;
}

async function main() {
    let tool = null;
    if (process.argv.includes("--hook")) {
        let input = {};
        try { input = JSON.parse(await readStdin()); } catch { return 0; }
        tool = findGuardedCommand(input?.tool_input?.command);
        if (!tool) { return 0; }
    }
    const reasons = probeUnity();
    if (reasons.length === 0) {
        if (!tool) { console.log("[unity-preflight] 未保存の Scene・Prefab なし、Play 中でない（または Editor が開いていない）。"); }
        return 0;
    }
    report(reasons, tool);
    return BLOCK;
}

if (import.meta.url === pathToFileURL(process.argv[1] ?? "").href) {
    process.exitCode = await main();
}
