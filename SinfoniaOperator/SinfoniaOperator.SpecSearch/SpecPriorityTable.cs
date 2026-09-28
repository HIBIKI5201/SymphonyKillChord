using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     明示ページID、文書用途、具体的なパスの順で資料の検索方針を解決する。
    /// </summary>
    public sealed class SpecPriorityTable
    {
        /// <summary>
        ///     設定されたルールを識別条件の優先順に並べる。
        /// </summary>
        public SpecPriorityTable(IEnumerable<SpecPriorityRule> rules)
        {
            ArgumentNullException.ThrowIfNull(rules);
            _rules = rules.Select(rule => rule ?? throw new InvalidDataException("優先度ルールにnullは指定できません。"))
                .OrderByDescending(rule => rule.PageId.Length > 0)
                .ThenByDescending(rule => rule.DocumentKind.Length > 0)
                .ThenByDescending(rule => rule.PathContains.Length)
                .ToArray();
            string[] keys = _rules.Select(rule => $"{rule.PageId}|{rule.DocumentKind}|{rule.PathContains}").ToArray();
            if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
            {
                throw new InvalidDataException("同じ識別条件の優先度ルールが重複しています。");
            }
        }

        /// <summary>
        ///     ページIDと文書用途を優先し、移行前の資料はパス条件で照合する。
        /// </summary>
        public SpecPriorityRule? Resolve(SpecChunkRecord record)
        {
            string path = record.SourceFile.Replace('\\', '/');
            return _rules.FirstOrDefault(rule =>
                rule.PageId.Length > 0 ? rule.PageId == record.Metadata.PageId
                : rule.DocumentKind.Length > 0 ? rule.DocumentKind == record.Metadata.DocumentKind
                : path.Contains(rule.PathContains, StringComparison.Ordinal));
        }

        private readonly SpecPriorityRule[] _rules;
    }
}
