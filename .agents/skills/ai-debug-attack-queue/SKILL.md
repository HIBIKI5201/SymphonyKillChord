---
name: ai-debug-attack-queue
description: Queue color- or BeatType-specific just attacks for Symphony Kill Chord PlayMode AI debugging. Use when frame-accurate rhythm attacks are needed; do not use for ordinary human input verification.
---

# AI debug attack queue

Use the bundled script to register an entire attack sequence in one call. Timing-critical work runs inside the Unity Editor; do not approximate just timing with repeated CLI sleeps or individual LLM tool calls.

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
   node .agents/skills/ai-debug-attack-queue/scripts/ai-debug-attacks.mjs cancel
   ```

The default primer attack establishes a fresh rhythm reference before the requested sequence. Pass `--no-prime` only when an extra attack would invalidate the scenario and a recent attack already provides the intended reference.

Accepted names are `purple/One/1`, `blue/Two/2`, `cyan/Three/3`, `green/Four/4`, `yellow/Six/6`, and `orange/Eight/8`; Japanese color names are also accepted.
