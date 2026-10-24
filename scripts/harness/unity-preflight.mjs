// Unity の検証（compile・run-tests・hot-reload・Play の開始）の前に、人の作業を守るための確認。
// 未保存の Scene・Prefab Stage、または Play 中なら終了コード 2 で止まる。状態を確かめられないときも止まる。
//
//   node scripts/harness/unity-preflight.mjs          手動で確認する（Codex など）
//   node scripts/harness/unity-preflight.mjs --hook   Claude Code の PreToolUse hook として、標準入力の Bash コマンドを見て確認する
//
// ユーザーに確認したうえで続けるときは、コマンドの先頭に SKC_UNITY_PREFLIGHT=confirmed を付ける（hook のときだけ有効）。
import { spawnSync } from "node:child_process";
import { mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const BLOCK = 2;
const UNITY_TIMEOUT_MS = 60000;
const CONFIRMED = /\bSKC_UNITY_PREFLIGHT=confirmed\b/;
// コマンドの先頭（環境変数の代入・npx の後を含む）に来た uloop だけを対象にし、grep の引数などは除く。
const GUARDED = /(?:^|[;&|(\n])\s*(?:[A-Za-z_][A-Za-z0-9_]*=\S*\s+)*(?:npx\s+)?uloop(?:\.cmd)?\s+(compile|run-tests|hot-reload|control-play-mode)\b([^;&|\n]*)/g;
const NON_STARTING_ACTION = /--action[\s=]+["']?(Stop|Pause|Step|Status)\b/i;

// Play 状態と未保存のシーンを 1 行ずつ返す。1 行目が playing=0/1、2 行目以降が未保存の項目。
const PROBE = `using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
var lines = new System.Collections.Generic.List<string>();
lines.Add(EditorApplication.isPlayingOrWillChangePlaymode ? "playing=1" : "playing=0");
for (int i = 0; i < SceneManager.sceneCount; i++)
{
    Scene scene = SceneManager.GetSceneAt(i);
    if (scene.isDirty) { lines.Add("Scene: " + (string.IsNullOrEmpty(scene.path) ? "(Untitled)" : scene.path)); }
}
PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
if (stage != null && stage.scene.isDirty) { lines.Add("Prefab: " + stage.assetPath); }
return string.Join("\\n", lines);`;

/** Bash コマンドのうち、確認が要る uloop の呼び出し名を返す。要らなければ null。 */
export function findGuardedCommand(command) {
    if (!command || CONFIRMED.test(command)) { return null; }
    for (const match of command.matchAll(GUARDED)) {
        const [, tool, rest] = match;
        // Play を始めない操作（止める・一時停止・状態の確認）は人の作業を壊さないので通す。
        if (tool === "control-play-mode" && NON_STARTING_ACTION.test(rest)) { continue; }
        return tool;
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

/** uloop で開いている Editor の状態を調べ、止まる理由を返す。 */
function probeUnity() {
    const dir = mkdtempSync(join(tmpdir(), "skc-preflight-"));
    try {
        const codeFile = join(dir, "probe.cs");
        writeFileSync(codeFile, PROBE, "utf8");
        const windows = process.platform === "win32";
        const run = spawnSync("uloop", ["execute-dynamic-code", "--code-file", windows ? `"${codeFile}"` : codeFile], {
            cwd: ROOT, encoding: "utf8", shell: windows, timeout: UNITY_TIMEOUT_MS,
        });
        const output = `${run.stdout ?? ""}`;
        const start = output.indexOf("{");
        let response = null;
        try { response = start >= 0 ? JSON.parse(output.slice(start)) : null; } catch { response = null; }
        if (!response?.Success) {
            const detail = response?.ErrorMessage || run.error?.message || run.stderr || output || `exit ${run.status}`;
            return [`Unity の状態を確かめられなかった（Unity が起動していない、または uloop が応答しない）: ${String(detail).trim()}`];
        }
        return parseProbe(response.Result);
    } finally {
        rmSync(dir, { recursive: true, force: true });
    }
}

function report(reasons, tool) {
    const target = tool ? `uloop ${tool} の前の確認` : "Unity 検証の前の確認";
    console.error([
        `[unity-preflight] ${target}で止まった。`,
        ...reasons.map((reason) => `- ${reason}`),
        "人の作業かもしれないので、勝手に保存・破棄・停止しない。ユーザーに確認すること。",
        "自分が始めた Play なら `uloop control-play-mode --action Stop` で止めてよい。",
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
        if (!tool) { console.log("[unity-preflight] 未保存の Scene・Prefab なし、Play 中でない。"); }
        return 0;
    }
    report(reasons, tool);
    return BLOCK;
}

if (import.meta.url === pathToFileURL(process.argv[1] ?? "").href) {
    process.exitCode = await main();
}
