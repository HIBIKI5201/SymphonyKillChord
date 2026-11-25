// セッション開始時に Docs/NotionSpecifications サブモジュールをバックグラウンドで更新する。
// Windows / macOS / Linux のいずれでも動くよう Node.js から非同期に起動し、即座に {} を返す。
import { spawn } from "node:child_process";
import { dirname, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "../..");

export function startNotionSpecSync(root = ROOT, spawnFn = spawn) {
    const child = spawnFn(
        "git",
        ["-C", root, "submodule", "update", "--init", "--remote", "--depth", "1", "--", "Docs/NotionSpecifications"],
        {
            cwd: root,
            detached: true,
            stdio: "ignore",
            windowsHide: true,
            env: { ...process.env, GIT_TERMINAL_PROMPT: "0" },
        },
    );
    child.on?.("error", () => {});
    child.unref?.();
    return child;
}

if (import.meta.url === pathToFileURL(process.argv[1] ?? "").href) {
    try {
        startNotionSpecSync();
    } catch {
        // 起動に失敗してもセッション開始は止めない。
    }
    console.log("{}");
}
