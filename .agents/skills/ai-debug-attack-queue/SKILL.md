---
name: ai-debug-attack-queue
description: Inspect Symphony Kill Chord gameplay as JSON, record combat and stability evidence, operate UI buttons, and queue precise rhythm attacks for the repository QA sheets. Editor-only; distinguish assisted QA from physical device verification.
---

# AI debug attack queue

Use the bundled script to register an entire attack sequence in one call. Timing-critical work runs inside the Unity Editor; do not approximate just timing with repeated CLI sleeps or individual LLM tool calls.

This implementation is shared by Claude and Codex. Read `references/qa-coverage.md` before selecting a QA case and `references/json-api.md` for response semantics.

Run from the same project checkout opened in Unity. Every request checks `Application.dataPath`. Use an already installed uloop; these scripts never install it or launch Unity. Windows native executables and standard npm shims are supported without shell evaluation. If discovery fails, set `ULOOP_EXECUTABLE` to the native executable or `ULOOP_CLI_JS` to its Node entry point.

## JSON state and bounded observations

```powershell
node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-qa.mjs snapshot
node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-qa.mjs snapshot --no-ui
node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-qa.mjs run --qa CBT:12-3 --seconds 900
node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-qa.mjs run --qa CBT:4-3 --seconds 120 --repeat 2 --queue "purple:1,blue:1,cyan:1,green:1,yellow:1,orange:1"
```

Snapshots include scene/frame/time, HP, rhythm history, targets, mission progress, loaded save data and active UI. Uninitialized sections are `available: false`. Current timing windows are not past input judgments. Start a monitor to capture actual attack judgments, weapon hits, skill IDs and damage events.

`run` writes request.json, observations.jsonl and summary.json under `.uloop/qa/<runId>/`. Every attack queue is primed. It stops on duration, repeat limit, mission end, Unity error, interruption or Ctrl+C. Without --queue it only observes; ordinary gameplay/input must be provided separately. The --qa value labels evidence; it is not an executable assertion. qaVerdict remains Unverified. Inspect the evidence and fill the original QA sheet separately.

`monitor-start --run-id <UUID> --seconds 900`, `monitor-status`, and `monitor-stop --run-id <UUID>` expose observation without the JS loop. Export evidence before starting another monitor, which resets bounded history. The monitor and attack queue each enforce deadlines even if JS disconnects.

## UI operations

Get `sections.ui.value.buttons` from a fresh snapshot, then:

```powershell
node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-qa.mjs button --id 1234 --path "<exact path from snapshot>"
```

This dispatches Button.onClick. Poll a new snapshot to verify completion. It does not prove pointer hit testing, keyboard navigation, visibility or hardware input. Use screenshots and real input for those checks. Destructive UI operations such as save reset require authorization for that scenario.

## Workflow

1. Ensure Unity is in PlayMode and gameplay has started. Resume if PlayMode is paused.
2. Enqueue attacks:

   ```powershell
   node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-attacks.mjs enqueue --queue "green:4,orange:8"
   ```

3. Poll status without sending additional input:

   ```powershell
   node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-attacks.mjs status
   ```

4. Verify the gameplay objective or mission state after `state` becomes `Completed`. Read Unity errors when it becomes `Failed`.
5. Cancel a remaining queue when stopping or changing the scenario:

   ```powershell
   node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-attacks.mjs cancel --run-id "<runId from enqueue/status>"
   ```

The default primer attack establishes a fresh rhythm reference before the requested sequence. Pass `--no-prime` only when an extra attack would invalidate the scenario and a recent attack already provides the intended reference.

Accepted names are `purple/One/1`, `blue/Two/2`, `cyan/Three/3`, `green/Four/4`, `yellow/Six/6`, and `orange/Eight/8`; Japanese color names are also accepted.

enqueue also accepts --run-id UUID for idempotent retries and --timeout 120 (1–3600 seconds). Maximum total queue size is 1000. Active queues are rejected rather than replaced. After an unknown transport outcome, query status and reuse the same ID; never blindly resend with a new ID.

If a killed JS process leaves `.uloop/qa-session.lock`, read its runId/pid and inspect monitor/attack status. Stop only owned IDs. Remove that exact lock only after confirming the owner is gone; do not take over a live run.
