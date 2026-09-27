import { spawn } from 'node:child_process';
import { mkdir, readFile, realpath, unlink, writeFile } from 'node:fs/promises';
import { dirname, join, resolve } from 'node:path';
import { randomUUID } from 'node:crypto';

/** 外部文字列をC#リテラルへ直接埋め込まない。 */
export function csharpString(value) {
  return `System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String("${Buffer.from(value, 'utf8').toString('base64')}"))`;
}

/** 終了コードとuloop・API双方の失敗を検査する。 */
export function decodeResponse(stdout) {
  let value = JSON.parse(stdout.trim());
  for (let depth = 0; depth < 6; depth++) {
    if (typeof value === 'string') { value = JSON.parse(value); continue; }
    if (!value || typeof value !== 'object') { break; }
    if (value.Success === false || value.success === false || value.isError === true) {
      const error = new Error(JSON.stringify(value));
      error.response = value;
      throw error;
    }
    if (Object.hasOwn(value, 'Result')) { value = value.Result; continue; }
    if (value.success === true && ('state' in value || 'schemaVersion' in value || value.result?.dispatched === true)) { return value; }
    if (value.result !== undefined) { value = value.result; continue; }
    if (value.data !== undefined) { value = value.data; continue; }
    if (Array.isArray(value.content) && value.content.length === 1 && value.content[0].type === 'text') {
      value = value.content[0].text; continue;
    }
    break;
  }
  throw new Error('Unknown uloop response; refusing to treat it as success');
}

/** シェルを介さずにCLIを呼び、時間と出力量を制限する。 */
function execute(file, args, cwd, timeoutMs = 15000) {
  return new Promise((resolveResult, reject) => {
    const child = spawn(file, args, { cwd, windowsHide: true, shell: false, stdio: ['ignore', 'pipe', 'pipe'] });
    let stdout = '', stderr = '', failure;
    const timer = setTimeout(() => { failure = new Error('uloop timed out; operation outcome is unknown'); child.kill(); }, timeoutMs);
    child.on('error', error => { clearTimeout(timer); reject(error); });
    for (const [stream, append] of [[child.stdout, text => { stdout += text; }], [child.stderr, text => { stderr += text; }]]) {
      stream.setEncoding('utf8');
      stream.on('data', text => {
        append(text);
        if (stdout.length + stderr.length > 4 * 1024 * 1024) {
          failure = new Error('uloop output exceeded 4 MiB'); child.kill();
        }
      });
    }
    child.on('close', code => {
      clearTimeout(timer);
      if (failure) { reject(failure); }
      else if (code !== 0) { reject(new Error(`uloop exit ${code}: ${stderr || stdout}`)); }
      else { resolveResult(stdout); }
    });
  });
}

/** npmのWindows shimもコードをシェルに渡さずNodeで起動する。 */
async function cliCommand(cwd) {
  if (process.env.ULOOP_CLI_JS) { return [process.execPath, resolve(process.env.ULOOP_CLI_JS)]; }
  if (process.env.ULOOP_EXECUTABLE) { return [resolve(process.env.ULOOP_EXECUTABLE)]; }
  if (process.platform !== 'win32') { return ['uloop']; }
  const candidates = (await execute('where.exe', ['uloop'], cwd)).trim().split(/\r?\n/);
  const executable = candidates.find(path => /\.exe$/i.test(path));
  if (executable) { return [executable]; }
  const shim = candidates.find(path => /\.cmd$/i.test(path));
  if (shim) {
    const source = await readFile(shim, 'utf8');
    const match = source.match(/"%dp0%\\([^"\r\n]+\.(?:js|mjs|cjs))"/i);
    if (match) { return [process.execPath, resolve(dirname(shim), match[1])]; }
  }
  throw new Error('Cannot resolve uloop. Set ULOOP_EXECUTABLE to its executable or ULOOP_CLI_JS to its Node entry point.');
}

/** 呼び出し先Editorのprojectを毎回照合する。起動・インストールは行わない。 */
export async function createTransport(project = process.cwd()) {
  const root = await realpath(project);
  await readFile(join(root, 'ProjectSettings/ProjectVersion.txt'), 'utf8');
  const command = await cliCommand(root);
  const inputDirectory = join(root, '.uloop/qa-input');
  await mkdir(inputDirectory, { recursive: true });
  return async expression => {
    const file = join(inputDirectory, `${randomUUID()}.csx`);
    const expected = csharpString(join(root, 'Assets').replaceAll('\\', '/'));
    const code = `using KillChord.Editor.AIDebugPlay;\nif (!string.Equals(UnityEngine.Application.dataPath.Replace('\\\\', '/'), ${expected}, System.StringComparison.OrdinalIgnoreCase)) { return "{\\"success\\":false,\\"message\\":\\"Wrong Unity project\\"}"; }\nreturn ${expression};\n`;
    await writeFile(file, code, { encoding: 'utf8', flag: 'wx' });
    try {
      return decodeResponse(await execute(command[0], [...command.slice(1), 'execute-dynamic-code', '--code-file', file], root));
    } finally { await unlink(file); }
  };
}
