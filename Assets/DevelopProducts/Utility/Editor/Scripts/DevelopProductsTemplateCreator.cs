using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace DevelopProducts.Utility.Editor
{
    public static class DevelopProductsTemplateCreator
    {
        private const string TEMPLATE_PATH = "Assets/DevelopProducts/Utility/Editor/DevelopProductsTemplateDirectory";

        [MenuItem("Assets/Create/" + DevelopProductsConst.DEVELOP_PRODUCTS_CREATE_ASSET_PATH + "Products Template (Native)", false, 220)]
        public static void Create()
        {
            var action = ScriptableObject.CreateInstance<CreateFolderAction>();

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                action,
                "NewProducts", // 初期名
                null,
                null
            );
        }

        private class CreateFolderAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string folderPath = pathName;

                CopyDirectory(TEMPLATE_PATH, folderPath);
                string scriptsFolder = FindScriptsFolder(folderPath);
                if (!string.IsNullOrEmpty(scriptsFolder))
                {
                    CreateAssembly(scriptsFolder, Path.GetFileNameWithoutExtension(folderPath));
                }

                AssetDatabase.Refresh();
            }

            private void CopyDirectory(string source, string destination)
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

            private static string FindScriptsFolder(string rootPath)
            {
                var directories = Directory.GetDirectories(rootPath, "Scripts", SearchOption.AllDirectories);

                if (directories.Length > 0)
                {
                    return directories[0]; // 最初に見つかったもの。
                }

                return null;
            }

            private void CreateAssembly(string path, string name)
            {
                string asmdefPath = Path.Combine(path, name + ".asmdef");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var data = new AssemblyDefinitionData($"DevelopProducts-{name}");
                data.rootNamespace = $"DevelopProducts.{name}";

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(asmdefPath, json);

                AssetDatabase.ImportAsset(asmdefPath);
            }

            [Serializable]
            private class AssemblyDefinitionData
            {
                public string name = string.Empty;

                public string rootNamespace = string.Empty;

                public string[] references = new string[0];

                public string[] includePlatforms = new string[0];

                public string[] excludePlatforms = new string[0];

                public bool allowUnsafeCode;

                public bool overrideReferences;

                public string[] precompiledReferences = new string[0];

                public bool autoReferenced = true;

                public string[] defineConstraints = new string[0];

                public string[] versionDefines = new string[0];

                public bool noEngineReferences;

                public string[] platforms = new string[0];

                public AssemblyDefinitionData(string name)
                {
                    this.name = name;
                }
            }
        }
    }
}