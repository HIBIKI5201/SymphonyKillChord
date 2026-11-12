using System.Collections.Generic;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     プロジェクト ドキュメントの写しの1ブロック。子ブロック（字下げされた行や、タグで囲まれた中身）を持つ。
    /// </summary>
    public sealed class SpecBlock
    {
        /// <summary>
        ///     ブロックを生成する。
        /// </summary>
        /// <param name="kind">ブロックの種類。</param>
        /// <param name="text">本文（Markdownの文中の書式を含む）。</param>
        public SpecBlock(SpecBlockKind kind, string text)
        {
            Kind = kind;
            Text = text ?? string.Empty;
        }

        /// <summary> ブロックの種類。 </summary>
        public SpecBlockKind Kind { get; }

        /// <summary> 本文。見出し・段落・項目は文中の書式を含む。画像・動画はファイルのパス。 </summary>
        public string Text { get; }

        /// <summary> 見出しの段（1〜6）。番号付きリストでは番号。 </summary>
        public int Level { get; private set; }

        /// <summary> チェックボックスにチェックが入っているか。 </summary>
        public bool IsChecked { get; private set; }

        /// <summary> 開閉できるか（トグル見出し）。 </summary>
        public bool IsToggle { get; private set; }

        /// <summary> 表の1行目が見出し行か。 </summary>
        public bool HasHeaderRow { get; private set; }

        /// <summary> 補足（トグルの見出し、コールアウトの絵文字、画像の代替テキスト、コードの言語など）。 </summary>
        public string Caption { get; private set; } = string.Empty;

        /// <summary> 子ブロック。 </summary>
        public IReadOnlyList<SpecBlock> Children => _children;

        /// <summary> 表のセル（行ごと）。 </summary>
        public IReadOnlyList<IReadOnlyList<string>> Rows => _rows;

        /// <summary>
        ///     見出しの段や番号を設定する。
        /// </summary>
        /// <param name="level">段・番号。</param>
        /// <returns>このブロック。</returns>
        public SpecBlock SetLevel(int level)
        {
            Level = level;
            return this;
        }

        /// <summary>
        ///     チェックの状態を設定する。
        /// </summary>
        /// <param name="isChecked">チェックが入っているか。</param>
        /// <returns>このブロック。</returns>
        public SpecBlock SetChecked(bool isChecked)
        {
            IsChecked = isChecked;
            return this;
        }

        /// <summary>
        ///     開閉できる見出しにする。
        /// </summary>
        /// <returns>このブロック。</returns>
        public SpecBlock MarkToggle()
        {
            IsToggle = true;
            return this;
        }

        /// <summary>
        ///     表の1行目を見出し行にする。
        /// </summary>
        /// <returns>このブロック。</returns>
        public SpecBlock MarkHeaderRow()
        {
            HasHeaderRow = true;
            return this;
        }

        /// <summary>
        ///     補足を設定する。
        /// </summary>
        /// <param name="caption">補足。</param>
        /// <returns>このブロック。</returns>
        public SpecBlock SetCaption(string caption)
        {
            Caption = caption ?? string.Empty;
            return this;
        }

        /// <summary>
        ///     子ブロックを足す。
        /// </summary>
        /// <param name="children">足す子ブロック。</param>
        public void AddChildren(IEnumerable<SpecBlock> children)
        {
            _children.AddRange(children);
        }

        /// <summary>
        ///     表の行を足す。
        /// </summary>
        /// <param name="cells">行のセル。</param>
        public void AddRow(IReadOnlyList<string> cells)
        {
            _rows.Add(cells);
        }

        private readonly List<SpecBlock> _children = new();
        private readonly List<IReadOnlyList<string>> _rows = new();
    }
}
