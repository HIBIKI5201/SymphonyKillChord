using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     キャッシュを見出し・トグルの境界で分け、本文へページ属性を継承する。
    /// </summary>
    public sealed partial class MarkdownChunker
    {
        /// <summary>
        ///     対象キャッシュの場所とチャンク長を設定する。
        /// </summary>
        public MarkdownChunker(
            string repositoryRootPath, int chunkLength = DEFAULT_CHUNK_LENGTH,
            int overlapLength = DEFAULT_OVERLAP_LENGTH, string? specificationRootPath = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRootPath);
            if (chunkLength <= 0) { throw new ArgumentOutOfRangeException(nameof(chunkLength)); }
            if (overlapLength < 0 || overlapLength >= chunkLength) { throw new ArgumentOutOfRangeException(nameof(overlapLength)); }
            _repositoryRootPath = Path.GetFullPath(repositoryRootPath);
            _specificationRootPath = Path.GetFullPath(specificationRootPath ?? ResolveDefaultRoot(_repositoryRootPath), _repositoryRootPath);
            _chunkLength = chunkLength;
            _overlapLength = overlapLength;
        }

        /// <summary>
        ///     指定キャッシュ内のMarkdownだけを索引化する。
        /// </summary>
        public SpecChunkRecord[] ChunkAll()
        {
            if (!Directory.Exists(_specificationRootPath))
            {
                throw new DirectoryNotFoundException($"仕様書キャッシュが見つかりません: {_specificationRootPath}");
            }
            EnumerationOptions options = new()
            {
                RecurseSubdirectories = true,
                AttributesToSkip = FileAttributes.ReparsePoint,
                IgnoreInaccessible = false
            };
            return Directory.EnumerateFiles(_specificationRootPath, "*.md", options)
                .Where(IsSearchTarget).OrderBy(path => path, StringComparer.Ordinal)
                .SelectMany(ChunkFile).ToArray();
        }

        private const int DEFAULT_CHUNK_LENGTH = 800;
        private const int DEFAULT_OVERLAP_LENGTH = 100;
        private const int MAX_HEADING_LEVEL = 6;
        private const string BREADCRUMB_SEPARATOR = " > ";
        private readonly string _repositoryRootPath;
        private readonly string _specificationRootPath;
        private readonly int _chunkLength;
        private readonly int _overlapLength;

        /// <summary>
        ///     新キャッシュを優先し、移行前だけ旧パスを利用する。
        /// </summary>
        private static string ResolveDefaultRoot(string root)
        {
            string migrated = Path.Combine(root, "Library", "NotionSpecifications");
            return Directory.Exists(migrated) ? migrated : Path.Combine(root, "Docs", "NotionSpecifications");
        }

        /// <summary>
        ///     DB索引・添付・移行原稿・ログを検索本文に含めない。
        /// </summary>
        private bool IsSearchTarget(string path)
        {
            string name = Path.GetFileName(path);
            if (name.StartsWith("_", StringComparison.Ordinal)) { return false; }
            string[] parts = Path.GetRelativePath(_specificationRootPath, path).Replace('\\', '/').Split('/');
            return !parts.Any(part => part.Equals("assets", StringComparison.OrdinalIgnoreCase)
                || part.Equals("NotionMigration", StringComparison.OrdinalIgnoreCase)
                || part.Equals("agent", StringComparison.OrdinalIgnoreCase)
                || part.Equals(".git", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     コード内の見出しを無視し、トグルの開始・終了でも必ず本文を区切る。
        /// </summary>
        private IEnumerable<SpecChunkRecord> ChunkFile(string path)
        {
            string markdown = File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal);
            string notionUrl = SpecPageMetadataReader.ReadNotionUrl(markdown);
            SpecPageMetadata metadata = SpecPageMetadataReader.Read(markdown, notionUrl);
            string sourceFile = Path.GetRelativePath(_repositoryRootPath, path).Replace('\\', '/');
            string title = Path.GetFileNameWithoutExtension(path);
            string[] headings = new string[MAX_HEADING_LEVEL];
            List<(string Title, string[] Headings)> details = new();
            StringBuilder section = new();
            string breadcrumb = title;
            char fenceCharacter = '\0';
            int fenceLength = 0;
            bool hasTitle = false;

            foreach (string line in SpecPageMetadataReader.RemoveMetadata(markdown).Split('\n'))
            {
                string trimmed = line.TrimStart();
                Match fence = FenceRegex().Match(trimmed);
                if (fenceCharacter != '\0')
                {
                    section.AppendLine(line);
                    if (fence.Success && fence.Groups["marks"].Value[0] == fenceCharacter
                        && fence.Groups["marks"].Length >= fenceLength
                        && string.IsNullOrWhiteSpace(trimmed[fence.Groups["marks"].Length..]))
                    {
                        fenceCharacter = '\0';
                    }
                    continue;
                }
                if (fence.Success)
                {
                    fenceCharacter = fence.Groups["marks"].Value[0];
                    fenceLength = fence.Groups["marks"].Length;
                    section.AppendLine(line);
                    continue;
                }

                Match heading = HeadingRegex().Match(trimmed);
                Match summary = SummaryRegex().Match(trimmed);
                bool opensDetails = trimmed.StartsWith("<details", StringComparison.OrdinalIgnoreCase);
                bool closesDetails = trimmed.StartsWith("</details>", StringComparison.OrdinalIgnoreCase);
                if (heading.Success || summary.Success || opensDetails || closesDetails)
                {
                    foreach (SpecChunkRecord chunk in CreateSectionChunks(sourceFile, breadcrumb, notionUrl, section.ToString(), metadata))
                    {
                        yield return chunk;
                    }
                    section.Clear();

                    if (opensDetails) { details.Add(("補足", headings.ToArray())); }
                    else if (closesDetails && details.Count > 0)
                    {
                        headings = details[^1].Headings;
                        details.RemoveAt(details.Count - 1);
                    }
                    else if (summary.Success && details.Count > 0)
                    {
                        details[^1] = (CleanHeading(summary.Groups["text"].Value), details[^1].Headings);
                    }
                    else if (heading.Success)
                    {
                        int level = heading.Groups["marks"].Length;
                        string headingText = CleanHeading(heading.Groups["text"].Value);
                        if (!hasTitle)
                        {
                            title = headingText;
                            hasTitle = true;
                        }
                        headings[level - 1] = headingText;
                        Array.Clear(headings, level, headings.Length - level);
                    }
                    breadcrumb = string.Join(BREADCRUMB_SEPARATOR,
                        new[] { title }.Concat(headings.Where(value => !string.IsNullOrWhiteSpace(value)))
                            .Concat(details.Select(detail => detail.Title)).Distinct(StringComparer.Ordinal));
                    continue;
                }

                if (trimmed.Length == 0 || trimmed == "---" || trimmed.StartsWith("<table_of_contents", StringComparison.Ordinal)
                    || trimmed.StartsWith("<empty-block", StringComparison.Ordinal))
                {
                    section.AppendLine();
                    continue;
                }
                section.AppendLine(line);
            }

            foreach (SpecChunkRecord chunk in CreateSectionChunks(sourceFile, breadcrumb, notionUrl, section.ToString(), metadata))
            {
                yield return chunk;
            }
        }

        /// <summary>
        ///     1つの節だけを分割し、重複部分が別の意味を持つ節へ跨がないようにする。
        /// </summary>
        private IEnumerable<SpecChunkRecord> CreateSectionChunks(
            string sourceFile, string breadcrumb, string notionUrl, string sectionText, SpecPageMetadata metadata)
        {
            string text = sectionText.Trim();
            int stepLength = _chunkLength - _overlapLength;
            for (int startIndex = 0; startIndex < text.Length; startIndex += stepLength)
            {
                int length = Math.Min(_chunkLength, text.Length - startIndex);
                string chunkText = text.Substring(startIndex, length).Trim();
                if (chunkText.Length > 0)
                {
                    yield return new SpecChunkRecord(sourceFile, breadcrumb, notionUrl, chunkText, Array.Empty<float>(), metadata);
                }
                if (startIndex + length >= text.Length) { yield break; }
            }
        }

        /// <summary>
        ///     見出しの装飾を除き、節ルールと一致する表記へ揃える。
        /// </summary>
        private static string CleanHeading(string text)
        {
            return HeadingAttributesRegex().Replace(text, string.Empty).Replace("**", string.Empty, StringComparison.Ordinal)
                .Replace("__", string.Empty, StringComparison.Ordinal).Trim();
        }

        /// <summary>
        ///     Markdownの全見出し段を認識する。
        /// </summary>
        [GeneratedRegex(@"^(?<marks>#{1,6})\s+(?<text>.+?)\s*$")]
        private static partial Regex HeadingRegex();

        /// <summary>
        ///     キャッシュのトグル名を見出しとして認識する。
        /// </summary>
        [GeneratedRegex(@"^<summary[^>]*>(?<text>.*?)</summary>\s*$", RegexOptions.IgnoreCase)]
        private static partial Regex SummaryRegex();

        /// <summary>
        ///     コードフェンスの開始・終了を認識する。
        /// </summary>
        [GeneratedRegex(@"^(?<marks>```+|~~~+)")]
        private static partial Regex FenceRegex();

        /// <summary>
        ///     Notionの見出し装飾属性を取り除く。
        /// </summary>
        [GeneratedRegex(@"\s*\{[^{}]*\}\s*$")]
        private static partial Regex HeadingAttributesRegex();
    }
}
