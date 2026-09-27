/** 未知の引数、重複、値の欠落を実行前に拒否する。 */
export function parseOptions(args, values, flags = []) {
  const result = {};
  for (let index = 0; index < args.length; index++) {
    const name = args[index].startsWith('--') ? args[index].slice(2) : '';
    if ((!values.includes(name) && !flags.includes(name)) || Object.hasOwn(result, name)) {
      throw new Error(`Unknown or repeated argument: ${args[index]}`);
    }
    if (flags.includes(name)) { result[name] = true; continue; }
    const value = args[++index];
    if (value === undefined || value.startsWith('--') || value.trim() === '') { throw new Error(`Missing value: --${name}`); }
    result[name] = value;
  }
  return result;
}

/** 有限の数値だけを受け入れる。 */
export function numberOption(options, key, fallback, minimum, maximum, integer = false) {
  const value = options[key] === undefined ? fallback : Number(options[key]);
  if (!Number.isFinite(value) || value < minimum || value > maximum || (integer && !Number.isInteger(value))) {
    throw new Error(`--${key} must be ${integer ? 'an integer' : 'a number'} in ${minimum}..${maximum}`);
  }
  return value;
}
