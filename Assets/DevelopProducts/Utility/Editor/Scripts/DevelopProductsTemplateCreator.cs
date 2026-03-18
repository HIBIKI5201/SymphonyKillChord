using UnityEngine;
using UnityEditor;
using System.IO;

namespace DevelopProducts.Utility.Editor
{
    public static class DevelopProductsTemplateCreator
    {
        private const string TEMPLATE_PATH = "Assets/DevelopProducts/Utility/Editor/DevelopProductsTemplateDirectory";

        [MenuItem("Assets/Create/" + DevelopProductsConst.DEVELOP_PRODUCTS_CREATE_ASSET_PATH + "Products Template (FolderPanel)", false, 210)]
        public static void CreateFeature()
        {
            string fullPath = EditorUtility.SaveFolderPanel(
                "Create Products Folder",
                Application.dataPath,
                "NewProducts"
            );

            if (string.IsNullOrEmpty(fullPath))
                return;

            string assetPath = ConvertToAssetPath(fullPath);

            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("Error", "Assetsフォルダ内を選択してください", "OK");
                return;
            }

            CopyDirectory(TEMPLATE_PATH, assetPath);

            AssetDatabase.Refresh();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        }

        private static string ConvertToAssetPath(string fullPath)
        {
            if (fullPath.StartsWith(Application.dataPath))
            {
                return "Assets" + fullPath.Substring(Application.dataPath.Length);
            }

            return null;
        }

        private static void CopyDirectory(string source, string destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (var file in Directory.GetFiles(source))
            {
                if (file.EndsWith(".meta")) continue;

                var dest = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                var dest = Path.Combine(destination, Path.GetFileName(dir));
                CopyDirectory(dir, dest);
            }
        }
    }
}