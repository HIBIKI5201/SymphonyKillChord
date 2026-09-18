import { randomUUID } from 'node:crypto';
import { appendFile, mkdir, open, unlink, writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import { performance } from 'node:perf_hooks';
import { csharpString } from './qa-transport.mjs';

/** 有限の観測と攻撃列の反復を行い、項目IDに結び付いた証跡を追記保存する。 */
export async function runAging(call, { seconds, repeat, queue, qa, signal }, root = process.cwd()) {
  const runId = randomUUID();
  const directory = join(root, '.uloop/qa', runId);
  await mkdir(directory, { recursive: true });
  const lockPath = join(root, '.uloop/qa-session.lock');
  const lock = await open(lockPath, 'wx');
  try { await lock.writeFile(JSON.stringify({ runId, pid: process.pid, directory })); }
  finally { await lock.close(); }
  const journal = join(directory, 'observations.jsonl');
  const summary = { schemaVersion: 1, runId, qa, queue, seconds, repeat, environment: 'UnityEditor',
    state: 'Running', qaVerdict: 'Unverified', completedQueues: 0, cleanupErrors: [] };
  let attackId, monitorOwned = false, monitor, snapshot;
  const start = performance.now();
  let observationStart = start;
  const record = async data => appendFile(journal, JSON.stringify({ capturedAtUtc: new Date().toISOString(), ...data }) + '\n', 'utf8');
  try {
    await writeFile(join(directory, 'request.json'), JSON.stringify(summary, null, 2), 'utf8');
    if (signal.aborted) { throw new Error('Cancelled before observation'); }
    snapshot = await call('AIDebugQaApi.GetSnapshotJson()');
    await record({ snapshot });
    if (!snapshot.isPlaying || snapshot.isEditorPaused) { throw new Error('An unpaused PlayMode session is required'); }
    const activeQueue = await call('AIDebugAttackQueue.GetStatusJson()');
    if (activeQueue.state === 'Waiting') { throw new Error('An attack queue is already running'); }
    // 開始応答が失われても、finallyで自分のIDだけを停止する。
    monitorOwned = true;
    monitor = await call(`AIDebugQaMonitor.Start(${csharpString(runId)}, ${seconds}d)`);
    observationStart = performance.now();
    while (!signal.aborted && performance.now() - observationStart < seconds * 1000) {
      monitor = await call('AIDebugQaMonitor.GetStatusJson()');
      snapshot = await call('AIDebugQaApi.GetSnapshotJson(false)');
      await record({ monitor, snapshot });
      if (monitor.runId !== runId) { throw new Error('Monitor session changed'); }
      if (monitor.errors > 0) { throw new Error('Unity reported an error; see observations.jsonl'); }
      if (monitor.state === 'Completed') { summary.state = 'DurationReached'; break; }
      if (monitor.state !== 'Running' || !snapshot.isPlaying) { throw new Error('Observation interrupted'); }
      if (queue) {
        if (snapshot.sections.mission.available && snapshot.sections.mission.value.isFinished) {
          summary.state = 'ScenarioEnded'; break;
        }
        if (attackId) {
          const attack = await call('AIDebugAttackQueue.GetStatusJson()');
          await record({ attack });
          if (attack.runId !== attackId) { throw new Error('Attack queue session changed'); }
          if (attack.state === 'Completed') { summary.completedQueues++; attackId = undefined; }
          else if (attack.state !== 'Waiting') { throw new Error(`Attack queue ${attack.state}`); }
        }
        if (!attackId) {
          if (summary.completedQueues >= repeat) { summary.state = 'RepeatLimitReached'; break; }
          if (signal.aborted) { break; }
          attackId = randomUUID();
          const remaining = Math.max(1, Math.min(120, seconds - (performance.now() - observationStart) / 1000));
          const attack = await call(`AIDebugAttackQueue.Enqueue(${csharpString(queue)}, true, ${csharpString(attackId)}, ${remaining.toFixed(3)}d)`);
          await record({ attack });
        }
      }
      await new Promise(resolve => {
        const finish = () => { clearTimeout(timer); signal.removeEventListener('abort', finish); resolve(); };
        const timer = setTimeout(finish, 1000);
        signal.addEventListener('abort', finish, { once: true });
        if (signal.aborted) { finish(); }
      });
    }
    if (summary.state === 'Running') { summary.state = signal.aborted ? 'Cancelled' : 'DurationReached'; }
  } catch (error) {
    summary.state = signal.aborted ? 'Cancelled' : 'Failed'; summary.error = error.message;
    try { await record({ failure: error.response ?? error.message }); }
    catch (writeError) { summary.cleanupErrors.push(writeError.message); }
  } finally {
    for (const expression of [
      attackId && `AIDebugAttackQueue.Cancel(${csharpString(attackId)})`,
      monitorOwned && `AIDebugQaMonitor.Stop(${csharpString(runId)})`,
    ].filter(Boolean)) {
      try {
        const response = await call(expression);
        if (expression.startsWith('AIDebugQaMonitor.')) { monitor = response; }
        await record({ cleanup: response });
        if (response.cleanupPending) {
          const release = await call('AIDebugAttackQueue.GetStatusJson()');
          await record({ cleanup: release });
          if (release.runId !== attackId || release.cleanupPending) {
            throw new Error('Attack release not confirmed; resume gameplay and inspect queue status before retrying');
          }
        }
      }
      catch (error) { summary.cleanupErrors.push(error.message); }
    }
    if (monitorOwned) {
      try { snapshot = await call('AIDebugQaApi.GetSnapshotJson()'); await record({ finalSnapshot: snapshot }); }
      catch (error) { summary.cleanupErrors.push(error.message); }
    }
    if (monitor?.errors > 0) { summary.state = 'Failed'; summary.error ??= 'Unity reported an error'; }
    summary.elapsedSeconds = (performance.now() - start) / 1000;
    summary.lastMonitor = monitor;
    summary.lastSnapshot = snapshot;
    try { await writeFile(join(directory, 'summary.json'), JSON.stringify(summary, null, 2), 'utf8'); }
    finally { await unlink(lockPath); }
  }
  return { ...summary, directory };
}
