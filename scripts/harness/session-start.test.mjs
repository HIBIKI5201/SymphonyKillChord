import assert from "node:assert/strict";
import test from "node:test";
import { startNotionSpecSync } from "./session-start.mjs";

test("SessionStart は git submodule update をバックグラウンド設定で起動する", () => {
    const calls = [];
    let unrefCalled = false;
    const fakeSpawn = (cmd, args, options) => {
        calls.push({ cmd, args, options });
        return {
            on() {},
            unref() { unrefCalled = true; },
        };
    };

    startNotionSpecSync("C:/repo", fakeSpawn);

    assert.equal(calls.length, 1);
    assert.equal(calls[0].cmd, "git");
    assert.deepEqual(calls[0].args, [
        "-C", "C:/repo", "submodule", "update", "--init", "--remote", "--depth", "1", "--", "Docs/NotionSpecifications",
    ]);
    assert.equal(calls[0].options.detached, true);
    assert.equal(calls[0].options.stdio, "ignore");
    assert.equal(calls[0].options.windowsHide, true);
    assert.equal(calls[0].options.env.GIT_TERMINAL_PROMPT, "0");
    assert.equal(unrefCalled, true);
});
