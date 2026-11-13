using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     写しのページのパスから、TreeView に渡す階層を作る。
    ///     Notion の書き出しは「ページ名.md」と、その子ページを入れる「ページ名/」フォルダの組になっているので、両者を1つの項目にまとめる。
    ///     データベースの一覧ページ（フォルダ/_database.md）は、そのフォルダの項目にする。
    /// </summary>
    public sealed class SpecTreeBuilder
    {
        /// <summary> 直前に作った階層での、ページの相対パスから項目IDへの対応。 </summary>
        public IReadOnlyDictionary<string, int> IdsByPath => _idsByPath;

        /// <summary>
        ///     ページの一覧から階層を作る。
        /// </summary>
        /// <param name="entries">ページ。</param>
        /// <returns>最上位の項目。</returns>
        public IList<TreeViewItemData<SpecTreeNode>> Build(IEnumerable<SpecCatalogEntry> entries)
        {
            _idsByPath.Clear();
            _nextId = 0;
            Dictionary<string, MutableNode> nodes = new(StringComparer.Ordinal);
            MutableNode root = new(string.Empty);

            foreach (SpecCatalogEntry entry in entries)
            {
                MutableNode node = GetOrCreate(nodes, root, ToNodeKey(entry.Path));
                node.SetEntry(entry);
            }

            List<TreeViewItemData<SpecTreeNode>> items = new();
            foreach (MutableNode child in root.GetSortedChildren())
            {
                items.Add(ToItem(child));
            }

            return items;
        }

        private const string DATABASE_SUFFIX = "/_database.md";
        private const string MARKDOWN_EXTENSION = ".md";

        private readonly Dictionary<string, int> _idsByPath = new(StringComparer.Ordinal);
        private int _nextId;

        /// <summary>
        ///     ページの相対パスを、項目のキー（拡張子を除いたパス）にする。
        /// </summary>
        /// <param name="path">ページの相対パス。</param>
        /// <returns>項目のキー。</returns>
        private static string ToNodeKey(string path)
        {
            if (path.EndsWith(DATABASE_SUFFIX, StringComparison.Ordinal))
            {
                return path.Substring(0, path.Length - DATABASE_SUFFIX.Length);
            }

            return path.EndsWith(MARKDOWN_EXTENSION, StringComparison.Ordinal)
                ? path.Substring(0, path.Length - MARKDOWN_EXTENSION.Length)
                : path;
        }

        /// <summary>
        ///     キーの項目を返す。無ければ親の項目も含めて作る。
        /// </summary>
        /// <param name="nodes">作った項目。</param>
        /// <param name="root">最上位の親。</param>
        /// <param name="key">項目のキー。</param>
        /// <returns>項目。</returns>
        private static MutableNode GetOrCreate(Dictionary<string, MutableNode> nodes, MutableNode root, string key)
        {
            if (nodes.TryGetValue(key, out MutableNode existing)) { return existing; }

            int slash = key.LastIndexOf('/');
            MutableNode parent = slash < 0 ? root : GetOrCreate(nodes, root, key.Substring(0, slash));
            MutableNode node = new(key.Substring(slash + 1));
            parent.Children.Add(node);
            nodes[key] = node;
            return node;
        }

        /// <summary>
        ///     作業用の項目を、TreeView の項目に変える。
        /// </summary>
        /// <param name="node">作業用の項目。</param>
        /// <returns>TreeView の項目。</returns>
        private TreeViewItemData<SpecTreeNode> ToItem(MutableNode node)
        {
            int id = _nextId++;
            if (node.Entry != null) { _idsByPath[node.Entry.Path] = id; }

            List<TreeViewItemData<SpecTreeNode>> children = new();
            foreach (MutableNode child in node.GetSortedChildren())
            {
                children.Add(ToItem(child));
            }

            SpecTreeNode data = new(node.Entry?.Title ?? node.Name, node.Entry);
            return new TreeViewItemData<SpecTreeNode>(id, data, children);
        }

        /// <summary>
        ///     階層を組み立てる途中の項目。
        /// </summary>
        private sealed class MutableNode
        {
            /// <summary>
            ///     項目を生成する。
            /// </summary>
            /// <param name="name">パスの最後の要素。</param>
            public MutableNode(string name)
            {
                Name = name;
            }

            /// <summary> パスの最後の要素。 </summary>
            public string Name { get; }

            /// <summary> 対応するページ。 </summary>
            public SpecCatalogEntry Entry { get; private set; }

            /// <summary> 子の項目。 </summary>
            public List<MutableNode> Children { get; } = new();

            /// <summary>
            ///     対応するページを設定する。
            /// </summary>
            /// <param name="entry">ページ。</param>
            public void SetEntry(SpecCatalogEntry entry)
            {
                Entry = entry;
            }

            /// <summary>
            ///     子の項目を表示名の順に並べて返す。
            /// </summary>
            /// <returns>並べた子の項目。</returns>
            public List<MutableNode> GetSortedChildren()
            {
                Children.Sort((left, right) => string.CompareOrdinal(left.DisplayName, right.DisplayName));
                return Children;
            }

            /// <summary> 表示名。 </summary>
            private string DisplayName => Entry?.Title ?? Name;
        }
    }
}
