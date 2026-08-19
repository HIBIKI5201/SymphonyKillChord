using System;
using System.Collections.Generic;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     エクスポート対象ページと保存先を保持するクラス。
    /// </summary>
    internal sealed class PageExportNode
    {
        /// <summary>
        ///     ページのエクスポート情報を生成する。
        /// </summary>
        /// <param name="metadata">ページメタデータ。</param>
        /// <param name="propertiesMarkdown">ページプロパティのMarkdown。</param>
        /// <param name="stagingFilePath">取得直後のMarkdownを退避する一時ファイル。</param>
        /// <param name="filePath">保存先ファイル。</param>
        /// <param name="childDirectory">子要素の保存先。</param>
        /// <param name="isUpdated">今回のエクスポートでページを更新するかどうか。</param>
        /// <param name="pageReferences">ページ本文に含まれる子ページ参照。</param>
        /// <param name="databaseReferences">ページ本文に含まれる子データベース参照。</param>
        internal PageExportNode(
            PageMetadata metadata,
            string propertiesMarkdown,
            string stagingFilePath,
            string filePath,
            string childDirectory,
            bool isUpdated,
            IReadOnlyList<MarkdownReference> pageReferences,
            IReadOnlyList<MarkdownReference> databaseReferences)
        {
            Id = metadata.Id;
            Title = metadata.Title;
            Url = metadata.Url;
            LastEditedTime = metadata.LastEditedTime;
            PropertiesMarkdown = propertiesMarkdown;
            StagingFilePath = stagingFilePath;
            FilePath = filePath;
            ChildDirectory = childDirectory;
            IsUpdated = isUpdated;
            PageReferences = pageReferences;
            DatabaseReferences = databaseReferences;
            GeneratedFiles = new List<string>();
        }

        internal string Id { get; }
        internal string Title { get; }
        internal string Url { get; }
        internal DateTimeOffset? LastEditedTime { get; }
        internal string PropertiesMarkdown { get; }
        internal string StagingFilePath { get; }
        internal string FilePath { get; }
        internal string ChildDirectory { get; }
        internal bool IsUpdated { get; }
        internal IReadOnlyList<MarkdownReference> PageReferences { get; }
        internal IReadOnlyList<MarkdownReference> DatabaseReferences { get; }
        internal List<string> GeneratedFiles { get; }
    }

    /// <summary>
    ///     エクスポート対象データベースと保存先を保持するクラス。
    /// </summary>
    internal sealed class DatabaseExportNode
    {
        /// <summary>
        ///     データベースのエクスポート情報を生成する。
        /// </summary>
        /// <param name="metadata">データベースメタデータ。</param>
        /// <param name="filePath">索引Markdownの保存先。</param>
        /// <param name="directoryPath">データベース配下の保存先。</param>
        internal DatabaseExportNode(DatabaseMetadata metadata, string filePath, string directoryPath)
        {
            Metadata = metadata;
            FilePath = filePath;
            DirectoryPath = directoryPath;
            DataSources = new List<DataSourceExportNode>();
        }

        internal DatabaseMetadata Metadata { get; }
        internal string FilePath { get; }
        internal string DirectoryPath { get; }
        internal List<DataSourceExportNode> DataSources { get; }
    }

    /// <summary>
    ///     データソースのスキーマとページ一覧を保持するクラス。
    /// </summary>
    internal sealed class DataSourceExportNode
    {
        /// <summary>
        ///     データソースのエクスポート情報を生成する。
        /// </summary>
        /// <param name="reference">データソース参照。</param>
        /// <param name="schema">データソーススキーマ。</param>
        internal DataSourceExportNode(DataSourceReference reference, DataSourceSchema schema)
        {
            Reference = reference;
            Schema = schema;
            Pages = new List<PageExportNode>();
        }

        internal DataSourceReference Reference { get; }
        internal DataSourceSchema Schema { get; }
        internal List<PageExportNode> Pages { get; }
    }

    /// <summary>
    ///     エクスポート完了時の件数を保持するクラス。
    /// </summary>
    internal sealed class ExportSummary
    {
        /// <summary>
        ///     エクスポート結果を生成する。
        /// </summary>
        /// <param name="pageCount">ページ数。</param>
        /// <param name="databaseCount">データベース数。</param>
        /// <param name="assetCount">添付ファイル数。</param>
        /// <param name="updatedPageCount">更新したページ数。</param>
        /// <param name="skippedPageCount">更新を省略したページ数。</param>
        /// <param name="warningCount">警告数。</param>
        /// <param name="outputDirectory">出力先。</param>
        internal ExportSummary(
            int pageCount,
            int databaseCount,
            int assetCount,
            int updatedPageCount,
            int skippedPageCount,
            int warningCount,
            string outputDirectory)
        {
            PageCount = pageCount;
            DatabaseCount = databaseCount;
            AssetCount = assetCount;
            UpdatedPageCount = updatedPageCount;
            SkippedPageCount = skippedPageCount;
            WarningCount = warningCount;
            OutputDirectory = outputDirectory;
        }

        internal int PageCount { get; }
        internal int DatabaseCount { get; }
        internal int AssetCount { get; }
        internal int UpdatedPageCount { get; }
        internal int SkippedPageCount { get; }
        internal int WarningCount { get; }
        internal string OutputDirectory { get; }
    }
}
