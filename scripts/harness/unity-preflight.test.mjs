// unity-preflight.mjs の判定部分のテスト。Unity は使わない。
import assert from "node:assert/strict";
import test from "node:test";
import { findGuardedCommand, parseProbe } from "./unity-preflight.mjs";

test("Unity を動かす uloop の呼び出しを見つける", () => {
    assert.equal(findGuardedCommand("uloop compile"), "compile");
    assert.equal(findGuardedCommand("cd /repo && npx uloop run-tests --test-mode PlayMode"), "run-tests");
    assert.equal(findGuardedCommand("FOO=1 uloop hot-reload"), "hot-reload");
    assert.equal(findGuardedCommand("uloop control-play-mode"), "control-play-mode");
    assert.equal(findGuardedCommand("uloop control-play-mode --action Play"), "control-play-mode");
});

test("Play を始めない操作・読み取り・了承済みは通す", () => {
    assert.equal(findGuardedCommand("uloop control-play-mode --action Stop"), null);
    assert.equal(findGuardedCommand("uloop control-play-mode --action Status"), null);
    assert.equal(findGuardedCommand("uloop get-logs"), null);
    assert.equal(findGuardedCommand("rg -n \"uloop compile\" AGENTS.md"), null);
    assert.equal(findGuardedCommand("cat .agents/skills/uloop-compile/SKILL.md"), null);
    assert.equal(findGuardedCommand("SKC_UNITY_PREFLIGHT=confirmed uloop compile"), null);
    assert.equal(findGuardedCommand(undefined), null);
});

test("Unity の状態から止まる理由を作る", () => {
    assert.deepEqual(parseProbe("playing=0"), []);
    assert.deepEqual(parseProbe("playing=1"), ["Play 中"]);
    assert.deepEqual(parseProbe("playing=0\nScene: Assets/Scenes/Home.unity\nPrefab: Assets/P.prefab"),
        ["未保存: Scene: Assets/Scenes/Home.unity", "未保存: Prefab: Assets/P.prefab"]);
    assert.equal(parseProbe("").length, 1);
});
