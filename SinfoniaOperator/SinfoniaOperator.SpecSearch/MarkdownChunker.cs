using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     Markdown仕様書を見出し単位で分割する。
    /// </summary>
    public sealed partial class MarkdownChunker
    {
        /// <summary>
        ///     指定したリポジトリを読み取るチャンク生成器を初期化する。
        /// </summary>
        /// <param name="repositoryRootPath">リポジトリルートのパス。</param>
        /// <param name="chunkLength">チャンクの目標文字数。</param>
        /// <param name="overlapLength">隣接チャンク間の重複文字数。</param>
        public MarkdownChunker(
            string repositoryRootPath,
            int chunkLength = DEFAULT_CHUNK_LENGTH,
            int overlapLength = DEFAULT_OVERLAP_LENGTH)
        {
            if (string.IsNullOrWhiteSpace(repositoryRootPath))
            {
                throw new ArgumentException("リポジトリルートを指定してください。", nameof(repositoryRootPath));
            }

            if (chunkLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkLength));
            }

            if (overlapLength < 0 || overlapLength >= chunkLength)
            {
                throw new ArgumentOutOfRangeException(nameof(overlapLength));
            }

            _repositoryRootPath = Path.GetFullPath(repositoryRootPath);
            _specificationRootPath = Path.Combine(_repositoryRootPath, SPECIFICATION_RELATIVE_PATH);
            _chunkLength = chunkLength;
            _overlapLength = overlapLength;
        }

        /// <summary>
        ///     対象ディレクトリのMarkdown仕様書をすべてチャンクへ変換する。
        /// </summary>
        /// <returns>ベクトルが未設定の仕様書チャンク。</returns>
        public SpecChunkRecord[] ChunkAll()
        {
            if (!Directory.Exists(_specificationRootPath))
            {
                throw new DirectoryNotFoundException($"仕様書ディレクトリが見つかりません: {_specificationRootPath}");
            }

            return Directory
                .EnumerateFiles(_specificationRootPath, MARKDOWN_PATTERN, SearchOption.AllDirectories)
                .Where(IsSearchTarget)
                .OrderBy(path => path, StringComparer.Ordinal)
                .SelectMany(ChunkFile)
                .ToArray();
        }

        private const int DEFAULT_CHUNK_LENGTH = 800;
        private const int DEFAULT_OVERLAP_LENGTH = 100;
        private const int NOTION_LINK_SEARCH_LENGTH = 4096;
        private const string SPECIFICATION_RELATIVE_PATH = "Docs/NotionSpecifications";
        private const string MARKDOWN_PATTERN = "*.md";
        private const string DATABASE_FILE_NAME = "_database.md";
        private const string ASSETS_DIRECTORY_NAME = "assets";
        private const string BREADCRUMB_SEPARATOR = " > ";
        private const int MAX_HEADING_LEVEL = 3;

        private readonly string _repositoryRootPath;
        private readonly string _specificationRootPath;
        private readonly int _chunkLength;
        private readonly int _overlapLength;

        /// <summary>
        ///     指定したファイルが検索対象であるか判定する。
        /// </summary>
        /// <param name="path">判定するファイルのパス。</param>
        /// <returns>検索対象の場合はtrue。</returns>
        private bool IsSearchTarget(string path)
        {
            if (string.Equals(Path.GetFileName(path), DATABASE_FILE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string relativePath = Path.GetRelativePath(_specificationRootPath, path);
            return !relativePath
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => string.Equals(segment, ASSETS_DIRECTORY_NAME, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     1つのMarkdownファイルを見出し単位のチャンクへ変換する。
        /// </summary>
        /// <param name="path">Markdownファイルのパス。</param>
        /// <returns>生成した仕様書チャンク。</returns>
        private IEnumerable<SpecChunkRecord> ChunkFile(string path)
        {
            string markdown = File.ReadAllText(path, Encoding.UTF8);
            string notionUrl = ExtractNotionUrl(markdown);
            string sourceFile = Path.GetRelativePath(_repositoryRootPath, path).Replace(Path.DirectorySeparatorChar, '/');
            string fallbackHeading = Path.GetFileNameWithoutExtension(path);
            string[] headings = new string[MAX_HEADING_LEVEL];
            StringBuilder section = new();
            string breadcrumb = fallbackHeading;

            foreach (string line in markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
            {
                Match headingMatch = HeadingRegex().Match(line);
                if (headingMatch.Success)
                {
                    foreach (SpecChunkRecord chunk in CreateSectionChunks(sourceFile, breadcrumb, notionUrl, section.ToString()))
                    {
                        yield return chunk;
                    }

                    section.Clear();
                    int level = headingMatch.Groups["marks"].Value.Length;
                    headings[level - 1] = headingMatch.Groups["text"].Value.Trim();
                    Array.Clear(headings, level, headings.Length - level);
                    breadcrumb = string.Join(BREADCRUMB_SEPARATOR, headings.Where(value => !string.IsNullOrWhiteSpace(value)));
                    continue;
                }

                if (!NotionLinkRegex().IsMatch(line))
                {
                    section.AppendLine(line);
                }
            }

            foreach (SpecChunkRecord chunk in CreateSectionChunks(sourceFile, breadcrumb, notionUrl, section.ToString()))
            {
                yield return chunk;
            }
        }

        /// <summary>
        ///     セクション本文を指定文字数の重複付きチャンクへ変換する。
        /// </summary>
        /// <param name="sourceFile">リポジトリルートからの相対パス。</param>
        /// <param name="breadcrumb">見出しのパンくず。</param>
        /// <param name="notionUrl">対応するNotionページのURL。</param>
        /// <param name="sectionText">セクション本文。</param>
        /// <returns>生成した仕様書チャンク。</returns>
        private IEnumerable<SpecChunkRecord> CreateSectionChunks(
            string sourceFile,
            string breadcrumb,
            string notionUrl,
            string sectionText)
        {
            string text = sectionText.Trim();
            if (text.Length == 0)
            {
                yield break;
            }

            int stepLength = _chunkLength - _overlapLength;
            for (int startIndex = 0; startIndex < text.Length; startIndex += stepLength)
            {
                int length = Math.Min(_chunkLength, text.Length - startIndex);
                string chunkText = text.Substring(startIndex, length).Trim();
                if (chunkText.Length > 0)
                {
                    yield return new SpecChunkRecord(sourceFile, breadcrumb, notionUrl, chunkText, Array.Empty<float>());
                }

                if (startIndex + length >= text.Length)
                {
                    yield break;
                }
            }
        }

        /// <summary>
        ///     Markdown冒頭からNotionページのURLを抽出する。
        /// </summary>
        /// <param name="markdown">Markdown本文。</param>
        /// <returns>見つかったURL。見つからない場合は空文字。</returns>
        private static string ExtractNotionUrl(string markdown)
        {
            string beginning = markdown[..Math.Min(markdown.Length, NOTION_LINK_SEARCH_LENGTH)];
            Match match = NotionLinkRegex().Match(beginning);
            return match.Success ? match.Groups["url"].Value : string.Empty;
        }

        /// <summary>
        ///     対象見出しを抽出する正規表現を生成する。
        /// </summary>
        /// <returns>コンパイル済みの正規表現。</returns>
        [GeneratedRegex(@"^(?<marks>#{1,3})\s+(?<text>.+?)\s*$", RegexOptions.CultureInvariant)]
        private static partial Regex HeadingRegex();

        /// <summary>
        ///     Notionリンクを抽出する正規表現を生成する。
        /// </summary>
        /// <returns>コンパイル済みの正規表現。</returns>
        [GeneratedRegex(@"\[Notionで開く\]\((?<url>https://[^)\s]+)\)", RegexOptions.CultureInvariant)]
        private static partial Regex NotionLinkRegex();
    }
}
