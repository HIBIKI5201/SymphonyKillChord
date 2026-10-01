#!/usr/bin/env node
import { createTransport, csharpString } from './qa-transport.mjs';
import { parseOptions, numberOption } from './qa-options.mjs';
import { runAging } from './qa-aging.mjs';

try {
  const [command, ...args] = process.argv.slice(2);
  const allowed = {
    snapshot: [], button: ['id', 'path'], run: ['seconds', 'repeat', 'queue', 'qa'],
    'monitor-start': ['run-id', 'seconds'], 'monitor-status': [], 'monitor-stop': ['run-id'],
  };
  if (!Object.hasOwn(allowed, command)) {
    throw new Error('Usage: ai-debug-qa.mjs snapshot [--no-ui] | button --id N --path PATH | run --qa CBT:12-3 [--seconds 900] [--queue green:4,orange:8] [--repeat 1000] | monitor-start --run-id UUID [--seconds 900] | monitor-status | monitor-stop --run-id UUID');
  }
  const options = parseOptions(args, allowed[command], command === 'snapshot' ? ['no-ui'] : []);
  let expression;
  if (command === 'snapshot') { expression = `AIDebugQaApi.GetSnapshotJson(${!options['no-ui']})`; }
  if (command === 'button') {
    const id = numberOption(options, 'id', NaN, -2147483648, 2147483647, true);
    if (!options.path) { throw new Error('button requires --path from a fresh snapshot'); }
    expression = `AIDebugQaApi.InvokeButton(${id}, ${csharpString(options.path)})`;
  }
  if (command === 'monitor-status') { expression = 'AIDebugQaMonitor.GetStatusJson()'; }
  if (command === 'monitor-start' || command === 'monitor-stop') {
    if (!options['run-id']) { throw new Error('--run-id UUID is required'); }
    expression = command === 'monitor-start'
      ? `AIDebugQaMonitor.Start(${csharpString(options['run-id'])}, ${numberOption(options, 'seconds', 900, 1, 3600)}d)`
      : `AIDebugQaMonitor.Stop(${csharpString(options['run-id'])})`;
  }
  let result;
  if (command === 'run') {
    if (!/^(?:CBT|PC):\d+-\d+$/.test(options.qa ?? '')) { throw new Error('--qa must identify a sheet item, e.g. CBT:12-3 or PC:6-3'); }
    const seconds = numberOption(options, 'seconds', 900, 1, 3600);
    const repeat = numberOption(options, 'repeat', 1000, 1, 1000, true);
    const controller = new AbortController();
    const stop = () => controller.abort();
    process.on('SIGINT', stop); process.on('SIGTERM', stop);
    try { result = await runAging(await createTransport(), { ...options, seconds, repeat, signal: controller.signal }); }
    finally { process.off('SIGINT', stop); process.off('SIGTERM', stop); }
    if (result.state === 'Failed' || result.state === 'Cancelled' || result.cleanupErrors.length) { process.exitCode = 1; }
  } else { result = await (await createTransport())(expression); }
  console.log(JSON.stringify(result));
} catch (error) { console.error(error.message); process.exitCode = 1; }
