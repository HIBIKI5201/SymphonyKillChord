#!/usr/bin/env node

import { randomUUID } from 'node:crypto';
import { createTransport, csharpString } from './qa-transport.mjs';
import { parseOptions, numberOption } from './qa-options.mjs';

try {
  const [command, ...args] = process.argv.slice(2);
  const allowed = { enqueue: ['queue', 'run-id', 'timeout'], status: [], cancel: ['run-id'] };
  if (!Object.hasOwn(allowed, command)) { throw new Error('Expected enqueue, status or cancel'); }
  const options = parseOptions(args, allowed[command], command === 'enqueue' ? ['no-prime'] : []);
  let expression;
  if (command === 'enqueue' && options.queue) {
    const timeout = numberOption(options, 'timeout', 120, 1, 3600);
    expression = `AIDebugAttackQueue.Enqueue(${csharpString(options.queue)}, ${!options['no-prime']}, ${csharpString(options['run-id'] ?? randomUUID())}, ${timeout}d)`;
  } else if (command === 'status') { expression = 'AIDebugAttackQueue.GetStatusJson()'; }
  else if (command === 'cancel' && options['run-id']) { expression = `AIDebugAttackQueue.Cancel(${csharpString(options['run-id'])})`; }
  else { throw new Error('enqueue requires --queue; cancel requires --run-id from status'); }
  const response = await (await createTransport())(expression);
  console.log(JSON.stringify(response));
  if (command === 'cancel' && response.cleanupPending) { process.exitCode = 1; }
} catch (error) {
  console.error(error.message);
  if (error.response) { console.error(JSON.stringify(error.response)); }
  process.exitCode = 1;
}
