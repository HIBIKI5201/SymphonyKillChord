using KillChord.Editor.Utility;
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     プロジェクト内の全DataIDを走査してハッシュを再焼き込みします。
    /// </summary>
    public static class DataIDRebuildMenu
    {
        /// <summary>
        ///     プロジェクト内の全DataIDを再計算して保存します。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Source Data Provider/Rebuild All DataID Hashes")]
        public static void RebuildAllDataIds()
        {
            if (!Application.isBatchMode
                && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            SceneSetup[] originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();
            RebuildContext context = new();

            try
            {
                RebuildScriptableObjects(context);
                RebuildPrefabs(context);
                RebuildScenes(context);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (originalSceneSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
                }
            }

            string message =
                $"DataIDの再焼き込みが完了しました。更新: {context.UpdatedCount}, 検査: {context.ScannedCount}";
            Debug.Log($"[{nameof(DataIDRebuildMenu)}] {message}");

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Rebuild All DataID Hashes", message, "OK");
            }
        }

        /// <summary>
        ///     ScriptableObjectアセットを再焼き込みします。
        /// </summary>
        /// <param name="context"> 再焼き込み処理の共有状態です。 </param>
        private static void RebuildScriptableObjects(RebuildContext context)
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ShowProgress("ScriptableObject", path, i, guids.Length);
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                for (int j = 0; j < assets.Length; j++)
                {
                    if (assets[j] != null)
                    {
                        RebuildObject(assets[j], path, context);
                    }
                }
            }
        }

        /// <summary>
        ///     Prefabアセットを再焼き込みします。
        /// </summary>
        /// <param name="context"> 再焼き込み処理の共有状態です。 </param>
        private static void RebuildPrefabs(RebuildContext context)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ShowProgress("Prefab", path, i, guids.Length);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                bool changed = RebuildGameObject(root, path, context);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        ///     Sceneアセットを再焼き込みします。
        /// </summary>
        /// <param name="context"> 再焼き込み処理の共有状態です。 </param>
        private static void RebuildScenes(RebuildContext context)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ShowProgress("Scene", path, i, guids.Length);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                bool changed = false;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int j = 0; j < roots.Length; j++)
                {
                    changed |= RebuildGameObject(roots[j], path, context);
                }

                if (changed)
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        /// <summary>
        ///     GameObject階層内のComponentを再焼き込みします。
        /// </summary>
        /// <param name="root"> 走査するルートGameObjectです。 </param>
        /// <param name="assetPath"> ログ表示用のアセットパスです。 </param>
        /// <param name="context"> 再焼き込み処理の共有状態です。 </param>
        /// <returns> 変更があった場合はtrueです。 </returns>
        private static bool RebuildGameObject(
            GameObject root,
            string assetPath,
            RebuildContext context)
        {
            bool changed = false;
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                {
                    changed |= RebuildObject(components[i], assetPath, context);
                }
            }

            return changed;
        }

        /// <summary>
        ///     Unityオブジェクト内のDataIDを再焼き込みします。
        /// </summary>
        /// <param name="target"> 走査対象のUnityオブジェクトです。 </param>
        /// <param name="assetPath"> ログ表示用のアセットパスです。 </param>
        /// <param name="context"> 再焼き込み処理の共有状態です。 </param>
        /// <returns> 変更があった場合はtrueです。 </returns>
        private static bool RebuildObject(
            UnityEngine.Object target,
            string assetPath,
            RebuildContext context)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            bool changed = false;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = true;
                if (!string.Equals(iterator.type, nameof(DataID), StringComparison.Ordinal)
                    || !SerializedPropertyFieldResolver.TryResolve(
                        target.GetType(),
                        iterator.propertyPath,
                        out FieldInfo fieldInfo))
                {
                    continue;
                }

                DataCategoryAttribute categoryAttribute =
                    fieldInfo.GetCustomAttribute<DataCategoryAttribute>();
                if (categoryAttribute == null)
                {
                    Debug.LogError(
                        $"[{nameof(DataIDRebuildMenu)}] DataCategory属性がありません。"
                        + $" Asset: {assetPath}, Property: {iterator.propertyPath}",
                        target);
                    continue;
                }

                context.ScannedCount++;
                SerializedProperty idProperty = iterator.FindPropertyRelative(
                    SourceDataProviderRepositoryResolver.ID_PROPERTY_NAME);
                SerializedProperty hashProperty = iterator.FindPropertyRelative(
                    SourceDataProviderRepositoryResolver.HASH_PROPERTY_NAME);
                if (idProperty == null || hashProperty == null)
                {
                    continue;
                }

                int expectedHash = DataIDHasher.Compute(
                    categoryAttribute.Category,
                    idProperty.stringValue);
                context.Record(
                    categoryAttribute.Category,
                    idProperty.stringValue,
                    expectedHash,
                    assetPath,
                    target);

                if (hashProperty.intValue == expectedHash)
                {
                    continue;
                }

                hashProperty.intValue = expectedHash;
                context.UpdatedCount++;
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }

            return changed;
        }

        /// <summary>
        ///     一括処理の進捗を表示します。
        /// </summary>
        /// <param name="kind"> 走査対象の種類です。 </param>
        /// <param name="path"> 走査中のアセットパスです。 </param>
        /// <param name="index"> 現在位置です。 </param>
        /// <param name="count"> 全件数です。 </param>
        private static void ShowProgress(string kind, string path, int index, int count)
        {
            if (Application.isBatchMode)
            {
                return;
            }

            float progress = count > 0 ? (float)index / count : 0f;
            EditorUtility.DisplayProgressBar(
                "Rebuild All DataID Hashes",
                $"{kind}: {path}",
                progress);
        }

        /// <summary>
        ///     再焼き込み件数とハッシュ衝突検査状態を保持します。
        /// </summary>
        private sealed class RebuildContext
        {
            /// <summary> 検査したDataID数です。 </summary>
            public int ScannedCount { get; set; }

            /// <summary> 更新したDataID数です。 </summary>
            public int UpdatedCount { get; set; }

            /// <summary>
            ///     IDを記録して同一カテゴリ内の衝突を検査します。
            /// </summary>
            /// <param name="category"> IDのカテゴリ名です。 </param>
            /// <param name="id"> 人間可読な文字列IDです。 </param>
            /// <param name="hashId"> 数値IDです。 </param>
            /// <param name="assetPath"> IDを保持するアセットパスです。 </param>
            /// <param name="context"> IDを保持するUnityオブジェクトです。 </param>
            public void Record(
                string category,
                string id,
                int hashId,
                string assetPath,
                UnityEngine.Object context)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return;
                }

                if (!_idsByCategory.TryGetValue(category, out Dictionary<int, string> idsByHash))
                {
                    idsByHash = new Dictionary<int, string>();
                    _idsByCategory.Add(category, idsByHash);
                }

                if (idsByHash.TryGetValue(hashId, out string registeredId)
                    && !string.Equals(registeredId, id, StringComparison.Ordinal))
                {
                    Debug.LogError(
                        $"[{nameof(DataIDRebuildMenu)}] DataIDのハッシュ衝突を検出しました。"
                        + $" Category: {category}, Id: {registeredId}, Other: {id}, Asset: {assetPath}",
                        context);
                    return;
                }

                idsByHash[hashId] = id;
            }

            private readonly Dictionary<string, Dictionary<int, string>> _idsByCategory =
                new(StringComparer.Ordinal);
        }
    }
}
