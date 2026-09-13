using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     仕様書のソースファイルに対応する検索優先度を管理する。
    /// </summary>
    public sealed class SpecPriorityTable
    {
        /// <summary>
        ///     JSONファイルから検索優先度テーブルを読み込む。
        /// </summary>
        /// <param name="jsonFilePath">優先度ルール配列を格納したJSONファイルのパス。</param>
        /// <returns>読み込んだ検索優先度テーブル。</returns>
        public static SpecPriorityTable Load(string jsonFilePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jsonFilePath);
            string json = File.ReadAllText(jsonFilePath);
            SpecPriorityRule[] rules = JsonConvert.DeserializeObject<SpecPriorityRule[]>(json)
                ?? throw new InvalidDataException("検索優先度ルールのJSON配列が必要です。");
            return new SpecPriorityTable(rules);
        }

        /// <summary>
        ///     ソースファイルに最も具体的にマッチする検索優先度を取得する。
        /// </summary>
        /// <param name="sourceFile">検索対象レコードの相対パス。</param>
        /// <returns>ソースファイルへ適用する重み。</returns>
        public double GetWeight(string sourceFile)
        {
            ArgumentNullException.ThrowIfNull(sourceFile);
            string normalizedSourceFile = NormalizePath(sourceFile);
            foreach (SpecPriorityRule rule in _rules)
            {
                if (normalizedSourceFile.Contains(rule.PathContains, StringComparison.Ordinal))
                {
                    return rule.Weight;
                }
            }

            return DEFAULT_WEIGHT;
        }

        private const double DEFAULT_WEIGHT = 1.0D;

        private readonly SpecPriorityRule[] _rules;

        /// <summary>
        ///     パス条件を正規化し、具体的なルールから評価できるテーブルを生成する。
        /// </summary>
        /// <param name="rules">登録する検索優先度ルール。</param>
        private SpecPriorityTable(SpecPriorityRule[] rules)
        {
            ArgumentNullException.ThrowIfNull(rules);
            _rules = rules
                .Select(rule => rule ?? throw new InvalidDataException("検索優先度ルールにnullは指定できません。"))
                .Select(rule => new SpecPriorityRule(NormalizePath(rule.PathContains), rule.Weight))
                .OrderByDescending(rule => rule.PathContains.Length)
                .ToArray();
        }

        /// <summary>
        ///     パス区切り文字をスラッシュへ正規化する。
        /// </summary>
        /// <param name="path">正規化するパス。</param>
        /// <returns>正規化したパス。</returns>
        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
