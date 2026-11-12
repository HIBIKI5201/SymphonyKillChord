using System.Collections.Generic;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     仕様書ビューアで開いたページの履歴（戻る・進む）。
    /// </summary>
    public sealed class SpecPageHistory
    {
        /// <summary> 戻れるか。 </summary>
        public bool CanGoBack => _index > 0;

        /// <summary> 進めるか。 </summary>
        public bool CanGoForward => _index < _paths.Count - 1;

        /// <summary>
        ///     新しく開いたページを積む。今より先の履歴は捨てる。
        /// </summary>
        /// <param name="path">ページの相対パス。</param>
        public void Push(string path)
        {
            if (_index >= 0 && _paths[_index] == path) { return; }

            _paths.RemoveRange(_index + 1, _paths.Count - _index - 1);
            _paths.Add(path);
            _index = _paths.Count - 1;
        }

        /// <summary>
        ///     1つ戻る。
        /// </summary>
        /// <returns>戻った先のページ。戻れなければ null。</returns>
        public string GoBack()
        {
            if (!CanGoBack) { return null; }

            _index--;
            return _paths[_index];
        }

        /// <summary>
        ///     1つ進む。
        /// </summary>
        /// <returns>進んだ先のページ。進めなければ null。</returns>
        public string GoForward()
        {
            if (!CanGoForward) { return null; }

            _index++;
            return _paths[_index];
        }

        private readonly List<string> _paths = new();
        private int _index = -1;
    }
}
