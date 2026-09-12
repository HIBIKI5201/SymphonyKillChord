using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     書き込み対象が許可ルートの配下にあるかを検証するクラス。
    ///     許可ルートは公開設定のNOTION_WRITE_ALLOWED_ROOTSで列挙し、ページIDでもデータベースIDでもよい。
    /// </summary>
    internal sealed class WriteScopeGuard
    {
        /// <summary> 祖先を辿る上限。循環や想定外の深さで無限に問い合わせないための保険。 </summary>
        private const int MAX_ANCESTOR_DEPTH = 16;

        private readonly HashSet<string> _allowedRootPageIds;
        private readonly NotionWriteClient _client;

        /// <summary>
        ///     スコープ検証を生成する。
        /// </summary>
        /// <param name="allowedRootPageIds">書き込みを許可するルートのページIDまたはデータベースID。</param>
        /// <param name="client">祖先取得に使うAPIクライアント。</param>
        internal WriteScopeGuard(IReadOnlyList<string> allowedRootPageIds, NotionWriteClient client)
        {
            _allowedRootPageIds = new HashSet<string>(allowedRootPageIds, StringComparer.OrdinalIgnoreCase);
            _client = client;
        }

        /// <summary>
        ///     ローカルのエクスポート構造から得た祖先IDで、明らかに範囲外のものを事前に弾く。
        ///     エクスポートは時点のコピーなので、これは高速な却下専用でありAPI検証の代わりにはならない。
        /// </summary>
        /// <param name="pageId">対象ページID。</param>
        /// <param name="localAncestorPageIds">ローカル構造から得た祖先ページID。</param>
        internal void RejectByLocalMirror(string pageId, IReadOnlyList<string> localAncestorPageIds)
        {
            if (localAncestorPageIds.Count == 0) { return; }

            bool isInsideAllowedRoot = _allowedRootPageIds.Contains(pageId) ||
                                       localAncestorPageIds.Any(id => _allowedRootPageIds.Contains(id));
            if (isInsideAllowedRoot) { return; }

            throw new WriterException(
                "エクスポート済みの仕様書ツリー上、対象ページは書き込み許可ページの配下にありません。" +
                $"許可ページ: {string.Join(", ", _allowedRootPageIds)}");
        }

        /// <summary>
        ///     編集対象が許可ルートの子孫であることをAPIで確認する。許可ルート自身は編集対象にしない。
        /// </summary>
        /// <param name="page">編集対象ページ。</param>
        /// <returns>一致した許可ルートID。</returns>
        internal async Task<string> AuthorizeEditAsync(NotionPageInfo page)
        {
            if (_allowedRootPageIds.Contains(page.Id))
            {
                throw new WriterException(
                    "書き込み許可ページ自身は編集できません。編集できるのはその子孫ページだけです。");
            }

            string? allowedRootId = await FindAllowedAncestorAsync(page.Parent);
            if (allowedRootId == null)
            {
                throw new WriterException(
                    $"対象ページ {page.Id} は書き込み許可ページの配下にありません。" +
                    $"許可: {string.Join(", ", _allowedRootPageIds)}");
            }

            return allowedRootId;
        }

        /// <summary>
        ///     作成先が許可ルート自身、またはその子孫であることをAPIで確認する。
        ///     作成先はページでもデータベースでもよい。
        /// </summary>
        /// <param name="parentId">作成先のページIDまたはデータベースID。</param>
        /// <param name="parent">作成先自身の親への参照。</param>
        /// <returns>一致した許可ルートID。</returns>
        internal async Task<string> AuthorizeCreateAsync(string parentId, NotionParentReference parent)
        {
            if (_allowedRootPageIds.Contains(parentId)) { return parentId; }

            string? allowedRootId = await FindAllowedAncestorAsync(parent);
            if (allowedRootId == null)
            {
                throw new WriterException(
                    $"作成先 {parentId} は書き込み許可ページとその配下のどちらでもありません。" +
                    $"許可: {string.Join(", ", _allowedRootPageIds)}");
            }

            return allowedRootId;
        }

        /// <summary>
        ///     親を順に辿り、許可ルートに到達するかを調べる。
        ///     データベース行はデータソースとデータベースを経由し、トグル内のページはブロックを経由する。
        /// </summary>
        /// <param name="parent">起点となる親への参照。</param>
        /// <returns>到達した許可ルートID。到達しない場合はnull。</returns>
        private async Task<string?> FindAllowedAncestorAsync(NotionParentReference parent)
        {
            NotionParentReference current = parent;
            for (int depth = 0; depth < MAX_ANCESTOR_DEPTH; depth++)
            {
                // 許可ルートにはページIDだけでなくデータベースIDも書けるため、種別を問わず先に照合する。
                if (_allowedRootPageIds.Contains(current.Id)) { return current.Id; }

                current = current.Type switch
                {
                    "page_id" => await _client.GetParentAsync(NotionObjectKind.Page, current.Id),
                    "block_id" => await _client.GetParentAsync(NotionObjectKind.Block, current.Id),
                    "data_source_id" => await _client.GetParentAsync(NotionObjectKind.DataSource, current.Id),
                    "database_id" => await _client.GetParentAsync(NotionObjectKind.Database, current.Id),

                    // workspaceやagent_idまで到達した時点で、許可ルートの配下ではない。
                    _ => null!
                };

                if (current == null) { return null; }
            }

            throw new WriterException($"親を{MAX_ANCESTOR_DEPTH}段辿っても許可ルートに到達しませんでした。");
        }
    }
}
