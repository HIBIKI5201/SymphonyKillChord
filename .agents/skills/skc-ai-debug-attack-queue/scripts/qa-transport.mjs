import { spawn } from 'node:child_process';
import { readFile, realpath } from 'node:fs/promises';
import { join, resolve } from 'node:path';

// Editor 側で C# を待つ上限（ミリ秒）。CLI 側の上限 execute() はこれより長くする。
const EVAL_TIMEOUT_MS = 10000;

/** 外部文字列をC#リテラルへ直接埋め込まない。 */
export function csharpString(value) {
  return `System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String("${Buffer.from(value, 'utf8').toString('base64')}"))`;
}

/** 終了コードとUnity CLI・API双方の失敗を検査する。 */
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
    if (value.returnValue !== undefined) { value = value.returnValue; continue; }
    if (value.value !== undefined) { value = value.value; continue; }
    if (Array.isArray(value.content) && value.content.length === 1 && value.content[0].type === 'text') {
      value = value.content[0].text; continue;
    }
    break;
  }
  throw new Error('Unknown Unity CLI response; refusing to treat it as success');
}

/** シェルを介さずにCLIを呼び、時間と出力量を制限する。 */
function execute(file, args, cwd, timeoutMs = 15000) {
  return new Promise((resolveResult, reject) => {
    const child = spawn(file, args, { cwd, windowsHide: true, shell: false, stdio: ['ignore', 'pipe', 'pipe'] });
    let stdout = '', stderr = '', failure;
    const timer = setTimeout(() => { failure = new Error('Unity CLI timed out; operation outcome is unknown'); child.kill(); }, timeoutMs);
    child.on('error', error => { clearTimeout(timer); reject(error); });
    for (const [stream, append] of [[child.stdout, text => { stdout += text; }], [child.stderr, text => { stderr += text; }]]) {
      stream.setEncoding('utf8');
      stream.on('data', text => {
        append(text);
        if (stdout.length + stderr.length > 4 * 1024 * 1024) {
          failure = new Error('Unity CLI output exceeded 4 MiB'); child.kill();
        }
      });
    }
    child.on('close', code => {
      clearTimeout(timer);
      if (failure) { reject(failure); }
      else if (code !== 0) { reject(new Error(`unity exit ${code}: ${stderr || stdout}`)); }
      else { resolveResult(stdout); }
    });
  });
}

/** Unity CLI の実行ファイル。公式のインストーラーは単体の実行ファイルを PATH に置く。 */
function cliCommand() {
  return process.env.UNITY_CLI_EXECUTABLE ? resolve(process.env.UNITY_CLI_EXECUTABLE) : 'unity';
}

/** Editor 側の eval が using を受け付けるとは限らないので、窓口の型を完全な名前にする。 */
export function qualifyApi(expression) {
  return expression.replace(/(?<![\w.])(AIDebugQaApi|AIDebugQaMonitor|AIDebugAttackQueue)\./g, 'KillChord.Editor.AIDebugPlay.$1.');
}

/** 呼び出し先Editorのprojectを毎回照合する。起動・インストールは行わない。 */
export async function createTransport(project = process.cwd()) {
  const root = await realpath(project);
  await readFile(join(root, 'ProjectSettings/ProjectVersion.txt'), 'utf8');
  const command = cliCommand();
  return async expression => {
    const expected = csharpString(join(root, 'Assets').replaceAll('\\', '/'));
    const code = `if (!string.Equals(UnityEngine.Application.dataPath.Replace('\\\\', '/'), ${expected}, System.StringComparison.OrdinalIgnoreCase)) { return "{\\"success\\":false,\\"message\\":\\"Wrong Unity project\\"}"; }\nreturn ${qualifyApi(expression)};\n`;
    return decodeResponse(await execute(command, [
      'command', 'script', 'eval', '--result-only', '--project-path', root, '--', '--code', code, '--timeout', String(EVAL_TIMEOUT_MS),
    ], root));
  };
}
