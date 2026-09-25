using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.ProjectWindow
{
    /// <summary>
    ///     Project WindowのアセットアイコンへSourceData関連状態のバッジを描画します。
    /// </summary>
    [InitializeOnLoad]
    internal static class SourceDataProjectWindowIcons
    {
        private const float LIST_VIEW_MAX_HEIGHT = 20f;
        private const float LIST_BADGE_SIZE = 7f;
        private const float GRID_BADGE_SIZE = 12f;
        private const float BADGE_GAP = 1f;

        private static readonly Color _addressableFallbackColor = new(0.2f, 0.6f, 1f, 0.9f);
        private static readonly Color _collectionItemColor = new(0.85f, 0.55f, 0.15f, 0.9f);
        private static readonly Color _buildDependencyColor = new(0.3f, 0.8f, 0.4f, 0.9f);
        private static readonly Texture _addressableIcon = ResolveIcon(
            EditorGUIUtility.isProSkin ? "d_Linked" : "Linked");

        /// <summary>
        ///     Project Windowの描画イベントを購読します。
        /// </summary>
        static SourceDataProjectWindowIcons()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        /// <summary>
        ///     対象アセットの状態に対応するバッジを描画します。
        /// </summary>
        /// <param name="guid"> 描画対象のアセットGUIDです。 </param>
        /// <param name="selectionRect"> Project Window上の描画領域です。 </param>
        private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            SourceDataAssetFlags flags = SourceDataAssetIndex.GetFlags(guid);
            if (flags == SourceDataAssetFlags.None)
            {
                return;
            }

            bool isListView = selectionRect.height <= LIST_VIEW_MAX_HEIGHT;
            float badgeSize = isListView ? LIST_BADGE_SIZE : GRID_BADGE_SIZE;
            float iconWidth = isListView ? selectionRect.height : selectionRect.width;
            float right = selectionRect.x + iconWidth;
            Rect badgeRect = new(
                right - badgeSize,
                selectionRect.y,
                badgeSize,
                badgeSize);

            if ((flags & SourceDataAssetFlags.Addressable) != 0)
            {
                DrawAddressableBadge(badgeRect);
                badgeRect.x -= badgeSize + BADGE_GAP;
            }

            if ((flags & SourceDataAssetFlags.CollectionItem) != 0)
            {
                EditorGUI.DrawRect(badgeRect, _collectionItemColor);
            }
            else if ((flags & SourceDataAssetFlags.BuildDependency) != 0)
            {
                // collection要素はほぼ全てビルドに含まれるため、重複表示を避けてcollection要素の表示を優先する。
                EditorGUI.DrawRect(badgeRect, _buildDependencyColor);
            }
        }

        /// <summary>
        ///     Addressables登録状態のバッジを描画します。
        /// </summary>
        /// <param name="badgeRect"> バッジの描画領域です。 </param>
        private static void DrawAddressableBadge(Rect badgeRect)
        {
            if (_addressableIcon != null)
            {
                GUI.DrawTexture(badgeRect, _addressableIcon);
                return;
            }

            EditorGUI.DrawRect(badgeRect, _addressableFallbackColor);
        }

        /// <summary>
        ///     Unity Editor組み込みアイコンを取得します。
        /// </summary>
        /// <param name="iconName"> 取得するアイコン名です。 </param>
        /// <returns> 見つかったアイコンです。 </returns>
        private static Texture ResolveIcon(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName)?.image;
        }
    }

    /// <summary>
    ///     アセット変更時にSourceDataアセットインデックスを無効化します。
    /// </summary>
    internal sealed class SourceDataAssetIndexPostprocessor : AssetPostprocessor
    {
        /// <summary>
        ///     アセットのインポート、削除、移動をSourceDataアセットインデックスへ反映します。
        /// </summary>
        /// <param name="importedAssets"> インポートされたアセットパスです。 </param>
        /// <param name="deletedAssets"> 削除されたアセットパスです。 </param>
        /// <param name="movedAssets"> 移動後のアセットパスです。 </param>
        /// <param name="movedFromAssetPaths"> 移動前のアセットパスです。 </param>
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            BuildDependencyAssetIndex.ScheduleRebuildIfRelevant(importedAssets);
            BuildDependencyAssetIndex.ScheduleRebuildIfRelevant(deletedAssets);
            BuildDependencyAssetIndex.ScheduleRebuildIfRelevant(movedAssets);

            if (importedAssets.Length > 0
                || deletedAssets.Length > 0
                || movedAssets.Length > 0
                || movedFromAssetPaths.Length > 0)
            {
                SourceDataAssetIndex.Invalidate();
            }
        }
    }
}
