using KillChord.Editor.SinfoniaOperator;
using System;
using System.IO;
using System.Linq;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     プロジェクト ドキュメントの写し（サブモジュール Docs/NotionSpecifications）の場所と状態。
    ///     取得はエディタ起動時の <see cref="NotionSpecificationsAutoPull"/> と同じ処理を使う。
    /// </summary>
    public sealed class SpecMirror
    {
        /// <summary>
        ///     写しの情報を生成する。
        /// </summary>
        /// <param name="repositoryRoot">本体リポジトリのルート。</param>
        public SpecMirror(string repositoryRoot)
        {
            _repositoryRoot = repositoryRoot;
            RootDirectory = Path.Combine(repositoryRoot, SPECIFICATIONS_RELATIVE_PATH);
        }

        /// <summary> 写しのルート。 </summary>
        public string RootDirectory { get; }

        /// <summary> 写しに Markdown が1つでもあるか。 </summary>
        public bool IsAvailable =>
            Directory.Exists(RootDirectory) &&
            Directory.EnumerateFiles(RootDirectory, "*.md", SearchOption.TopDirectoryOnly).Any();

        /// <summary> 写しが無いときの案内。 </summary>
        public string UnavailableMessage =>
            $"プロジェクト ドキュメントの写し（{SPECIFICATIONS_RELATIVE_PATH}）を取得できていません。" +
            "仕様書リポジトリ（SymphonyKillChord_Specifications）の閲覧権限が必要です。" +
            "権限があれば「更新」を押すか、リポジトリ直下のセットアップ（Setup.bat / Setup.command）を実行してください。" +
            "権限が無ければリードに GitHub の招待を頼んでください。";

        /// <summary>
        ///     写しを最新にする。時間がかかるので、メインスレッド以外から呼ぶ。
        /// </summary>
        /// <returns>失敗したときの説明。成功したら空文字。</returns>
        public string Pull()
        {
            return NotionSpecificationsAutoPull.PullNow(_repositoryRoot);
        }

        /// <summary>
        ///     写しの最新コミットの日時を、表示用の文字列で返す。
        /// </summary>
        /// <returns>日時。取得できなければ空文字。</returns>
        public string ReadLastUpdatedText()
        {
            string iso = NotionSpecificationsAutoPull.ReadLastCommitDate(_repositoryRoot);
            if (!DateTimeOffset.TryParse(iso, out DateTimeOffset time)) { return string.Empty; }

            return time.ToLocalTime().ToString(DISPLAY_TIME_FORMAT);
        }

        private const string SPECIFICATIONS_RELATIVE_PATH = "Docs/NotionSpecifications";
        private const string DISPLAY_TIME_FORMAT = "yyyy-MM-dd HH:mm";

        private readonly string _repositoryRoot;
    }
}
