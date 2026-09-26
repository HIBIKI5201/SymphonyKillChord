using System;
using System.Collections.Generic;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     サブコマンドのコマンドライン引数を保持するクラス。
    /// </summary>
    internal sealed class CommandArguments
    {
        private readonly Dictionary<string, string?> _options;
        private readonly Dictionary<string, List<string>> _repeatedOptions;
        private readonly List<string> _operands;

        private CommandArguments(
            Dictionary<string, string?> options,
            Dictionary<string, List<string>> repeatedOptions,
            List<string> operands)
        {
            _options = options;
            _repeatedOptions = repeatedOptions;
            _operands = operands;
        }

        /// <summary> オプション以外の引数。 </summary>
        internal IReadOnlyList<string> Operands
        {
            get { return _operands; }
        }

        /// <summary>
        ///     コマンドライン引数を解析する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <param name="valueOptions">値を伴うオプション名。</param>
        /// <param name="flagOptions">値を伴わないオプション名。</param>
        /// <param name="repeatableOptions">複数回指定できる、値を伴うオプション名。</param>
        /// <returns>解析結果。</returns>
        internal static CommandArguments Parse(
            string[] args,
            IReadOnlyCollection<string> valueOptions,
            IReadOnlyCollection<string> flagOptions,
            IReadOnlyCollection<string>? repeatableOptions = null)
        {
            Dictionary<string, string?> options = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, List<string>> repeated = new(StringComparer.OrdinalIgnoreCase);
            List<string> operands = new();
            HashSet<string> values = new(valueOptions, StringComparer.OrdinalIgnoreCase);
            HashSet<string> flags = new(flagOptions, StringComparer.OrdinalIgnoreCase);
            HashSet<string> repeatable = new(
                repeatableOptions ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];
                if (!argument.StartsWith("--", StringComparison.Ordinal))
                {
                    operands.Add(argument);
                    continue;
                }

                string name = argument[2..];
                if (flags.Contains(name))
                {
                    options[name] = null;
                    continue;
                }

                bool isRepeatable = repeatable.Contains(name);
                if (!values.Contains(name) && !isRepeatable)
                {
                    throw new WriterException($"不明なオプションです: {argument}");
                }

                if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new WriterException($"{argument}の値が指定されていません。");
                }

                string value = args[++index];
                if (isRepeatable)
                {
                    if (!repeated.TryGetValue(name, out List<string>? list))
                    {
                        list = new List<string>();
                        repeated[name] = list;
                    }

                    list.Add(value);
                    continue;
                }

                options[name] = value;
            }

            return new CommandArguments(options, repeated, operands);
        }

        /// <summary>
        ///     複数回指定できるオプションの値をすべて取得する。
        /// </summary>
        /// <param name="name">オプション名。</param>
        /// <returns>指定された順の値。未指定の場合は空。</returns>
        internal IReadOnlyList<string> GetValues(string name)
        {
            return _repeatedOptions.TryGetValue(name, out List<string>? values)
                ? values
                : Array.Empty<string>();
        }

        /// <summary>
        ///     値を伴うオプションを取得する。
        /// </summary>
        /// <param name="name">オプション名。</param>
        /// <returns>指定されていればその値、それ以外はnull。</returns>
        internal string? GetValue(string name)
        {
            return _options.TryGetValue(name, out string? value) ? value : null;
        }

        /// <summary>
        ///     フラグが指定されているかを判定する。
        /// </summary>
        /// <param name="name">オプション名。</param>
        /// <returns>指定されていればtrue。</returns>
        internal bool HasFlag(string name)
        {
            return _options.ContainsKey(name);
        }

        /// <summary>
        ///     必須の位置引数をひとつ取得する。
        /// </summary>
        /// <param name="description">エラーメッセージに使う引数の説明。</param>
        /// <returns>位置引数。</returns>
        internal string GetRequiredOperand(string description)
        {
            if (_operands.Count == 0) { throw new WriterException($"{description}を指定してください。"); }
            if (_operands.Count > 1) { throw new WriterException($"引数が多すぎます: {_operands[1]}"); }

            return _operands[0];
        }
    }
}
