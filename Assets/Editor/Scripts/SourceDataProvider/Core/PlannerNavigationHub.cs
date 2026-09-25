using System;

namespace KillChord.Editor.SourceDataProvider.Core
{
    /// <summary>
    ///     Core側のPropertyDrawer/Menuから、Wiki側のPlanner Master Data windowへジャンプするための間接層です。
    ///     CoreはWikiの型を直接知らず、このHubへ委譲することでCore→Wikiの一方向依存を保ちます。
    ///     実体（Planner Master Data windowを開いて実際にナビゲートする処理）はWiki側が起動時に登録します。
    /// </summary>
    internal static class PlannerNavigationHub
    {
        /// <summary>
        ///     指定SourceAssetへジャンプする処理です。Wiki側が登録します。
        ///     戻り値はジャンプに成功したかどうかです。
        /// </summary>
        public static Func<string, bool> NavigateToSourceAsset { get; set; }

        /// <summary>
        ///     登録済みの場合は指定SourceAssetへジャンプします。未登録(Wiki側が未ロード等)の場合は何もしません。
        /// </summary>
        /// <param name="addressableKey"> 移動先SourceAssetのAddressableキーです。 </param>
        /// <returns> ジャンプ処理を実行できた場合はtrueです。 </returns>
        public static bool TryNavigateToSourceAsset(string addressableKey)
        {
            return NavigateToSourceAsset?.Invoke(addressableKey) ?? false;
        }
    }
}
