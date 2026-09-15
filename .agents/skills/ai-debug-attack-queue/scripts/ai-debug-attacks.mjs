#!/usr/bin/env node

import { spawnSync } from "node:child_process";

const [, , command, ...args] = process.argv;

if (!command || !["enqueue", "status", "cancel"].includes(command)) {
  fail("Usage: ai-debug-attacks.mjs <enqueue|status|cancel> [--queue green:4,orange:8] [--no-prime]");
}

let code;

if (command === "enqueue") {
  const queueIndex = args.indexOf("--queue");
  const specification = queueIndex >= 0 ? args[queueIndex + 1] : undefined;
  if (!specification) {
    fail("enqueue requires --queue, for example --queue green:4,orange:8");
  }

  const prime = !args.includes("--no-prime");
  const encodedSpecification = Buffer.from(specification, "utf8").toString("base64");
  code = [
    "using System;",
    "using System.Text;",
    "using KillChord.Editor.AIDebugPlay;",
    `string specification = Encoding.UTF8.GetString(Convert.FromBase64String(\"${encodedSpecification}\"));`,
    `return AIDebugAttackQueue.Enqueue(specification, ${prime ? "true" : "false"});`,
  ].join(" ");
} else if (command === "status") {
  code = "using KillChord.Editor.AIDebugPlay; return AIDebugAttackQueue.GetStatusJson();";
} else {
  code = "using KillChord.Editor.AIDebugPlay; return AIDebugAttackQueue.Cancel();";
}

const result = spawnSync("uloop", ["execute-dynamic-code", "--code", code], {
  cwd: process.cwd(),
  encoding: "utf8",
  stdio: ["ignore", "pipe", "pipe"],
});

if (result.error) {
  fail(result.error.message);
}

if (result.stdout) {
  process.stdout.write(result.stdout);
}

if (result.stderr) {
  process.stderr.write(result.stderr);
}

process.exit(result.status ?? 1);

function fail(message) {
  process.stderr.write(`${message}\n`);
  process.exit(1);
}
