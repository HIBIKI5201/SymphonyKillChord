using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.MeshMerge
{
    /// <summary>
    ///     Projectウィンドウで選んだモデルから、SkinnedMeshRenderer統合済みのPrefab Variantを作るメニューです。
    /// </summary>
    public static class SkinnedMeshMergeMenu
    {
        private const string MENU_PATH = "Assets/KillChord/Create Merged Skinned Mesh Variant";
        private const string OUTPUT_FOLDER_NAME = "Merged";

        /// <summary>
        ///     選択中のモデルの隣に「Merged」フォルダを作り、統合済みVariantを出力します。
        ///     モデルと一緒に通常のPrefabも選んだ場合は、そのPrefabのマテリアルを参照して統合します。
        /// </summary>
        [MenuItem(MENU_PATH)]
        private static void CreateMergedVariant()
        {
            GameObject source = null;
            GameObject reference = null;
            foreach (GameObject selected in Selection.GetFiltered<GameObject>(SelectionMode.Assets))
            {
                if (PrefabUtility.GetPrefabAssetType(selected) == PrefabAssetType.Model)
                {
                    source = selected;
                }
                else
                {
                    reference = selected;
                }
            }
            if (source == null)
            {
                Debug.LogError($"[{nameof(SkinnedMeshMergeMenu)}] モデル(FBX)が選択されていません。");
                return;
            }

            string sourceFolder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(source)).Replace('\\', '/');
            SkinnedMeshMergeVariantBuilder.CreateVariant(source, $"{sourceFolder}/{OUTPUT_FOLDER_NAME}", reference);
        }

        /// <summary>
        ///     モデルまたはPrefabのアセットが選ばれているときだけメニューを有効にします。
        /// </summary>
        [MenuItem(MENU_PATH, true)]
        private static bool ValidateCreateMergedVariant()
        {
            return Selection.activeObject is GameObject && EditorUtility.IsPersistent(Selection.activeObject);
        }
    }
}
