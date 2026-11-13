// unity-preflight.mjs の判定部分のテスト。Unity は使わない。
import assert from "node:assert/strict";
import test from "node:test";
import { extractEvalResult, findGuardedCommand, parseProbe, readHookCommand } from "./unity-preflight.mjs";

test("Unity でコンパイル・テスト・Play を始める呼び出しを見つける", () => {
    assert.equal(findGuardedCommand("unity recompile --project-path C:/p --format json"), "recompile");
    assert.equal(findGuardedCommand("cd /repo && unity test . --mode PlayMode"), "test");
    assert.equal(findGuardedCommand("unity watch test ."), "test");
    assert.equal(findGuardedCommand("FOO=1 unity command test run --detach"), "command test run");
    assert.equal(findGuardedCommand("unity command test.run"), "command test run");
    assert.equal(findGuardedCommand("unity command editor.playmode start"), "command editor.playmode start");
    assert.equal(findGuardedCommand("unity command editor playmode start"), "command editor.playmode start");
    assert.equal(findGuardedCommand("unity command editor_play"), "command editor.playmode start");
    assert.equal(findGuardedCommand("unity.exe command script recompile"), "command script recompile");
    assert.equal(findGuardedCommand("& unity recompile"), "recompile");
});

test("Play を始めない操作・読み取り・了承済みは通す", () => {
    assert.equal(findGuardedCommand("unity command editor.playmode stop"), null);
    assert.equal(findGuardedCommand("unity command editor.playmode pause"), null);
    assert.equal(findGuardedCommand("unity command editor status"), null);
    assert.equal(findGuardedCommand("unity command console get"), null);
    assert.equal(findGuardedCommand("unity command script recompile_status"), null);
    assert.equal(findGuardedCommand("unity status --until-ready"), null);
    assert.equal(findGuardedCommand("unity open ."), null);
    assert.equal(findGuardedCommand("rg -n \"unity recompile\" AGENTS.md"), null);
    assert.equal(findGuardedCommand("cat Docs/unity test.md"), null);
    assert.equal(findGuardedCommand("SKC_UNITY_PREFLIGHT=confirmed unity recompile"), null);
    assert.equal(findGuardedCommand(undefined), null);
});

test("eval の出力から C# の戻り値を取り出す", () => {
    assert.equal(extractEvalResult(JSON.stringify("playing=0")), "playing=0");
    assert.equal(extractEvalResult(JSON.stringify({ result: "playing=1" })), "playing=1");
    assert.equal(extractEvalResult(JSON.stringify({ success: true, data: { result: "x" } })), "x");
    assert.equal(extractEvalResult(JSON.stringify({ success: false, result: "x" })), null);
    assert.equal(extractEvalResult("not json"), null);
});

test("Unity CLI 1.0.0-beta.17・Pipeline 1.0.0 の実際の eval 出力を読む", () => {
    // `unity command script eval --result-only -- --code 'return "playing=0";'` の標準出力そのまま。
    assert.equal(extractEvalResult("\"playing=0\"\n"), "playing=0");
    // 複数行の戻り値は JSON 文字列の \n で来る。
    assert.equal(extractEvalResult("\"playing=0\\nScene: Assets/A.unity\"\n"), "playing=0\nScene: Assets/A.unity");
});

test("hook の標準入力からコマンドを読む", () => {
    const input = JSON.stringify({ tool_input: { command: "unity recompile" } });
    assert.equal(readHookCommand(input), "unity recompile");
    // Windows PowerShell 5.1 のパイプは BOM と CRLF を付ける。
    assert.equal(readHookCommand(`\uFEFF${input}\r\n`), "unity recompile");
    assert.equal(readHookCommand("not json"), null);
    assert.equal(readHookCommand("{}"), null);
});

test("Unity の状態から止まる理由を作る", () => {
    assert.deepEqual(parseProbe("playing=0"), []);
    assert.deepEqual(parseProbe("playing=1"), ["Play 中"]);
    assert.deepEqual(parseProbe("playing=0\nScene: Assets/Scenes/Home.unity\nPrefab: Assets/P.prefab"),
        ["未保存: Scene: Assets/Scenes/Home.unity", "未保存: Prefab: Assets/P.prefab"]);
    assert.equal(parseProbe("").length, 1);
});
