using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.Build
{
    /// <summary>
    ///     タイトルとアウトゲームのUI依存を再インポートし、欠落したUIのビルドを防ぎます。
    /// </summary>
    public sealed class UiDocumentBuildValidator : IPreprocessBuildWithReport
    {
        /// <summary> Addressablesとプレイヤーの生成より前にUIを検証する実行順です。 </summary>
        public int callbackOrder => VALIDATION_ORDER;

        /// <summary>
        ///     UIテンプレートを依存先から同期インポートし、必須コンテナを検証します。
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            HashSet<string> imported = new(StringComparer.Ordinal);
            HashSet<string> importing = new(StringComparer.Ordinal);
            ImportDependencyTree(TITLE_PATH, imported, importing);
            ImportDependencyTree(OUT_GAME_PATH, imported, importing);
            ValidateContainers(TITLE_PATH, TITLE_CONTAINERS);
            ValidateContainers(OUT_GAME_PATH, OUT_GAME_CONTAINERS);
            Debug.Log($"[{nameof(UiDocumentBuildValidator)}] UI依存{imported.Count}件と必須コンテナを検証しました。");
        }

        private const int VALIDATION_ORDER = -300;
        private const string PROJECT_URI_PREFIX = "project://database/";
        private const string TITLE_PATH =
            "Assets/Level/Scenes/Develop/OutGameTest/Title/UIToolkit/TitleScreen.uxml";
        private const string OUT_GAME_PATH =
            "Assets/Level/Scenes/Develop/OutGameTest/ScreenTransitionTest/UI Toolkit/OutGame.uxml";
        private const ImportAssetOptions IMPORT_OPTIONS = ImportAssetOptions.ForceUpdate
                                                         | ImportAssetOptions.ForceSynchronousImport
                                                         | ImportAssetOptions.DontDownloadFromCacheServer;

        private static readonly string[] TITLE_CONTAINERS =
        {
            "TitleContainer", "MenuContainer", "CreditContainer"
        };

        private static readonly string[] OUT_GAME_CONTAINERS =
        {
            "HomeContainer", "StageSelectContainer", "SkillTreeContainer", "SkillBuildContainer",
            "BattlePreparationContainer", "SettingContainer"
        };

        /// <summary>
        ///     欠落時のAssetDatabase依存一覧には頼らず、ソースに書かれた依存を先に復元します。
        /// </summary>
        private static void ImportDependencyTree(string path, HashSet<string> imported, HashSet<string> importing)
        {
            if (imported.Contains(path))
            {
                return;
            }

            if (!importing.Add(path))
            {
                throw CreateFailure(path, "UI依存が循環しています。");
            }

            if (!File.Exists(path) || !File.Exists(path + ".meta"))
            {
                throw CreateFailure(path, "UIソースまたは.metaが存在しません。CIのチェックアウトと素材展開を確認してください。");
            }

            XDocument document = null;
            if (path.EndsWith(".uxml", StringComparison.OrdinalIgnoreCase))
            {
                document = XDocument.Load(path);
                foreach (XElement element in document.Descendants())
                {
                    if (element.Name.LocalName != "Template" && element.Name.LocalName != "Style")
                    {
                        continue;
                    }

                    string source = (string)element.Attribute("src");
                    if (string.IsNullOrEmpty(source))
                    {
                        throw CreateFailure(path, "Template/Styleのsrcが未設定です。");
                    }

                    string dependencyPath = ResolveDependencyPath(path, source);
                    ImportDependencyTree(dependencyPath, imported, importing);
                    ValidateGuidReference(path, source, dependencyPath);
                }
            }

            // Libraryに欠落状態のインポート結果が残っていても、このUI依存だけを作り直します。
            AssetDatabase.ImportAsset(path, IMPORT_OPTIONS);
            ValidateImportedAsset(path, document);
            importing.Remove(path);
            imported.Add(path);
        }

        /// <summary>
        ///     現行UXMLのproject URIまたは相対srcから、プロジェクト内のUI依存パスを解決します。
        /// </summary>
        private static string ResolveDependencyPath(string ownerPath, string source)
        {
            string path = Uri.UnescapeDataString(source.Split('?', '#')[0]);
            if (path.StartsWith(PROJECT_URI_PREFIX, StringComparison.Ordinal))
            {
                path = path.Substring(PROJECT_URI_PREFIX.Length);
            }
            else if (!path.StartsWith("Assets/", StringComparison.Ordinal))
            {
                path = Path.Combine(Path.GetDirectoryName(ownerPath), path);
            }

            string projectRoot = Path.GetFullPath(".") + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(path);
            if (!fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw CreateFailure(ownerPath, $"UI依存がプロジェクト外を指しています。src: {source}");
            }

            path = fullPath.Substring(projectRoot.Length).Replace('\\', '/');
            if (!path.StartsWith("Assets/", StringComparison.Ordinal)
                || (!path.EndsWith(".uxml", StringComparison.OrdinalIgnoreCase)
                    && !path.EndsWith(".uss", StringComparison.OrdinalIgnoreCase)))
            {
                throw CreateFailure(ownerPath, $"未対応のUI依存です。src: {source}");
            }

            return path;
        }

        /// <summary>
        ///     パッケージ展開などで.metaが置換され、srcのGUIDと食い違っていないか検証します。
        /// </summary>
        private static void ValidateGuidReference(string ownerPath, string source, string dependencyPath)
        {
            foreach (string part in source.Split('?', '&', '#'))
            {
                if (!part.StartsWith("guid=", StringComparison.Ordinal))
                {
                    continue;
                }

                string expectedGuid = part.Substring("guid=".Length);
                string actualGuid = AssetDatabase.AssetPathToGUID(dependencyPath);
                if (!string.Equals(expectedGuid, actualGuid, StringComparison.OrdinalIgnoreCase))
                {
                    throw CreateFailure(ownerPath,
                        $"UI依存のGUIDが一致しません。Path: {dependencyPath}, Expected: {expectedGuid}, Actual: {actualGuid}");
                }
            }
        }

        /// <summary>
        ///     インポートエラーと、ソースにある名前付き要素の脱落を検証します。
        /// </summary>
        private static void ValidateImportedAsset(string path, XDocument document)
        {
            if (document == null)
            {
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (styleSheet == null || styleSheet.importedWithErrors)
                {
                    throw CreateFailure(path, "USSのインポートに失敗しました。");
                }

                return;
            }

            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            if (asset == null || asset.importedWithErrors)
            {
                throw CreateFailure(path, "UXMLのインポートに失敗しました。Semanticエラーを確認してください。");
            }

            TemplateContainer root = asset.CloneTree();
            foreach (XElement element in document.Descendants())
            {
                string name = (string)element.Attribute("name");
                if (string.IsNullOrEmpty(name) || element.Name.LocalName == "Template")
                {
                    continue;
                }

                if (root.Q<VisualElement>(name) == null)
                {
                    throw CreateFailure(path, $"ソースにあるUI要素がインポート結果から欠落しています。Name: {name}");
                }
            }
        }

        /// <summary>
        ///     実行時の画面初期化で必要なコンテナが展開結果に存在することを検証します。
        /// </summary>
        private static void ValidateContainers(string path, string[] names)
        {
            TemplateContainer root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).CloneTree();
            foreach (string name in names)
            {
                if (root.Q<VisualElement>(name) == null)
                {
                    throw CreateFailure(path, $"画面初期化に必要なコンテナがありません。Name: {name}");
                }
            }
        }

        /// <summary>
        ///     問題のあるアセットを特定できるビルド失敗を作成します。
        /// </summary>
        private static BuildFailedException CreateFailure(string path, string reason)
        {
            return new BuildFailedException($"[{nameof(UiDocumentBuildValidator)}] {reason} Path: {path}");
        }
    }
}
