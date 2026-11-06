using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     仕様書ミラーの目録（_catalog.jsonl）から、ページ本文に無い属性を補う。
    ///     仕様書の窓口（scripts/notion/spec_query.py）と同じ目録を読み、所属DB・実装状態の解釈を揃える（#2396）。
    /// </summary>
    internal sealed class SpecCatalog
    {
        /// <summary>
        ///     目録の行から、パスごとの属性と実装リストの状態を組み立てる。
        /// </summary>
        private SpecCatalog(IReadOnlyDictionary<string, Entry> entries, IReadOnlyDictionary<string, string> implementations)
        {
            _entries = entries;
            _implementations = implementations;
        }

        /// <summary>
        ///     仕様書ルートの目録を読む。目録が無い版では何も補わない。
        /// </summary>
        internal static SpecCatalog Load(string specificationRootPath)
        {
            string path = Path.Combine(specificationRootPath, CATALOG_FILE_NAME);
            if (!File.Exists(path)) { return Empty; }

            Dictionary<string, Entry> entries = new(StringComparer.Ordinal);
            Dictionary<string, List<string>> statuses = new(StringComparer.Ordinal);
            foreach (string line in File.ReadLines(path, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) { continue; }
                JObject row = JObject.Parse(line);
                string rowPath = row.Value<string>("path") ?? string.Empty;
                string database = row.Value<string>("database") ?? string.Empty;
                JObject properties = row["properties"] as JObject ?? new JObject();
                entries[rowPath] = new Entry(ReadDocumentKind(database, properties), ReadString(properties, APPLIES_TO_PROPERTY));
                if (database != IMPLEMENTATION_DATABASE) { continue; }

                string status = ReadString(properties, STATUS_PROPERTY);
                if (row["relations"]?[SPECIFICATION_RELATION] is not JArray targets) { continue; }
                foreach (string target in targets.Values<string>().OfType<string>())
                {
                    if (!statuses.TryGetValue(target, out List<string>? list))
                    {
                        list = new List<string>();
                        statuses[target] = list;
                    }
                    list.Add(status);
                }
            }

            Dictionary<string, string> implementations = statuses.ToDictionary(
                pair => pair.Key, pair => SummarizeImplementation(pair.Value), StringComparer.Ordinal);
            return new SpecCatalog(entries, implementations);
        }

        /// <summary>
        ///     ページ自身のプロパティ表に無い値だけを、目録の値で補う。
        /// </summary>
        /// <param name="relativePath">仕様書ルートからの相対パス（区切りは / ）。</param>
        /// <param name="metadata">ページ本文から読んだ属性。</param>
        internal SpecPageMetadata Complete(string relativePath, SpecPageMetadata metadata)
        {
            _entries.TryGetValue(relativePath, out Entry? entry);
            _implementations.TryGetValue(relativePath, out string? implementation);
            if (entry == null && implementation == null) { return metadata; }

            return new SpecPageMetadata(
                metadata.PageId,
                Prefer(metadata.DocumentKind, entry?.DocumentKind),
                metadata.SpecificationStatus,
                Prefer(metadata.ImplementationStatus, implementation),
                Prefer(metadata.AppliesTo, entry?.AppliesTo),
                metadata.LastEdited);
        }

        private const string CATALOG_FILE_NAME = "_catalog.jsonl";
        private const string IMPLEMENTATION_DATABASE = "実装リスト";
        private const string SPECIFICATION_DATABASE = "仕様リスト";
        private const string SYSTEM_DATABASE = "システムリスト";
        private const string SPECIFICATION_RELATION = "仕様";
        private const string STATUS_PROPERTY = "ステータス";
        private const string CATEGORY_PROPERTY = "カテゴリー";
        private const string KIND_PROPERTY = "種別";
        private const string APPLIES_TO_PROPERTY = "適用範囲";
        private const string DONE = "完了";
        private const string NOT_STARTED = "未着手";
        private const string OUT_OF_SCOPE = "対象外";
        private const string IMPLEMENTED = "実装済";
        private const string PARTIALLY_IMPLEMENTED = "一部実装";
        private const string NOT_IMPLEMENTED = "未実装";

        private static readonly SpecCatalog Empty = new(
            new Dictionary<string, Entry>(StringComparer.Ordinal), new Dictionary<string, string>(StringComparer.Ordinal));

        private readonly IReadOnlyDictionary<string, Entry> _entries;
        private readonly IReadOnlyDictionary<string, string> _implementations;

        /// <summary>
        ///     資料の種類は、仕様リストではカテゴリー、システムリストでは種別とする。ミラーに「文書用途」の列が無いため（#2396 段階1）。
        ///     ほかのDBでは補わない。DB名を種類にすると SourceRules の DocumentKind（議事録など）に当たり、今の順位が変わるため。
        /// </summary>
        private static string ReadDocumentKind(string database, JObject properties)
        {
            return database switch
            {
                SPECIFICATION_DATABASE => ReadString(properties, CATEGORY_PROPERTY),
                SYSTEM_DATABASE => ReadString(properties, KIND_PROPERTY),
                _ => string.Empty
            };
        }

        /// <summary>
        ///     実装リストの項目の状態を、ページ単位の実装状態にまとめる。対象外の項目は数えない。
        /// </summary>
        private static string SummarizeImplementation(IReadOnlyCollection<string> statuses)
        {
            string[] counted = statuses.Where(status => status.Length > 0 && status != OUT_OF_SCOPE).ToArray();
            if (counted.Length == 0) { return string.Empty; }
            if (counted.All(status => status == DONE)) { return IMPLEMENTED; }
            if (counted.All(status => status == NOT_STARTED)) { return NOT_IMPLEMENTED; }
            return PARTIALLY_IMPLEMENTED;
        }

        /// <summary>
        ///     プロパティの値を文字列で読む。値が無ければ空文字を返す。
        /// </summary>
        private static string ReadString(JObject properties, string name)
        {
            return properties[name]?.Type == JTokenType.String ? properties.Value<string>(name)?.Trim() ?? string.Empty : string.Empty;
        }

        /// <summary>
        ///     ページ本文の値を優先し、空のときだけ目録の値を使う。
        /// </summary>
        private static string Prefer(string own, string? fromCatalog)
        {
            return own.Length > 0 ? own : fromCatalog ?? string.Empty;
        }

        /// <summary>
        ///     目録の1行から読む、ページの属性。
        /// </summary>
        private sealed record Entry(string DocumentKind, string AppliesTo);
    }
}
