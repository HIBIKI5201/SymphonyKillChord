using KillChord.Editor.SourceDataProvider.Core;
using KillChord.Editor.Utility;
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.Build
{
    /// <summary>
    ///     ScriptableObjectとPrefabが持つDataIDの重複・ハッシュ衝突・焼き込み忘れ・参照切れを、
    ///     アセットを変更せずにビルド前に検証します。Masterビルドはエラーで止め、Devビルドは警告に留めます。
    /// </summary>
    public sealed class DataIDBuildValidator : IPreprocessBuildWithReport
    {
        /// <summary> フォント検証と同じく、他のビルド前処理より先に検証する実行順です。 </summary>
        public int callbackOrder => VALIDATION_ORDER;

        /// <summary>
        ///     ビルド開始前にDataIDの整合を検証します。
        /// </summary>
        /// <param name="report"> ビルドレポートです。 </param>
        /// <exception cref="BuildFailedException"> Masterビルドで不整合がある場合に送出されます。 </exception>
        public void OnPreprocessBuild(BuildReport report)
        {
            IReadOnlyList<string> issues = CollectIssues(out int checkedCount);
            if (issues.Count == 0)
            {
                Debug.Log($"[{nameof(DataIDBuildValidator)}] DataIDに問題はありません。検査: {checkedCount}件");
                return;
            }

            string message = CreateErrorMessage(issues);
            bool isDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;
            if (isDevelopmentBuild)
            {
                Debug.LogWarning(message);
                return;
            }

            throw new BuildFailedException(message);
        }

        private const int VALIDATION_ORDER = -200;
        private const int MAX_REPORTED_ISSUES = 50;
        /// <summary> 走査で子要素へ進む深さの上限です。DataIDはこれより浅い位置にあります。 </summary>
        private const int MAX_PROPERTY_DEPTH = 64;
        private const string EXCLUDED_FOLDER = "Assets/DevelopProducts/";

        private static readonly string[] SEARCH_FOLDERS = { "Assets" };
        private static readonly GameDataVariant[] VARIANTS = { GameDataVariant.Release, GameDataVariant.Demo };

        /// <summary>
        ///     メニュー操作からDataIDの整合を検証します。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Build/Validate DataIDs")]
        private static void ValidateFromMenu()
        {
            IReadOnlyList<string> issues = CollectIssues(out int checkedCount);
            if (issues.Count == 0)
            {
                string okMessage = $"DataIDに問題はありません。検査: {checkedCount}件";
                Debug.Log($"[{nameof(DataIDBuildValidator)}] {okMessage}");
                EditorUtility.DisplayDialog("DataID Build Validation", okMessage, "OK");
                return;
            }

            string errorMessage = CreateErrorMessage(issues);
            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog(
                "DataID Build Validation",
                $"DataIDの不整合が{issues.Count}件あります。詳細はConsoleを確認してください。",
                "OK");
        }

        /// <summary>
        ///     登録済みIDとアセット内のDataIDを集め、不整合を検出します。
        /// </summary>
        /// <param name="checkedCount"> 検査したDataIDの件数です。 </param>
        /// <returns> 検出した不整合の一覧です。 </returns>
        private static IReadOnlyList<string> CollectIssues(out int checkedCount)
        {
            DataIDIntegrityChecker checker = new();
            Dictionary<string, List<GameDataVariant>> resolvedVariants = CollectRegisteredIds(checker);

            string[] scriptableObjectGuids = AssetDatabase.FindAssets("t:ScriptableObject", SEARCH_FOLDERS);
            for (int i = 0; i < scriptableObjectGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(scriptableObjectGuids[i]);
                if (IsExcluded(path))
                {
                    continue;
                }

                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                for (int j = 0; j < assets.Length; j++)
                {
                    if (assets[j] != null)
                    {
                        CollectFromObject(assets[j], path, resolvedVariants, checker);
                    }
                }
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", SEARCH_FOLDERS);
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject prefab = IsExcluded(path) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                // Prefabを開かずにアセットのまま読むため、保存内容は変わらない。
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                for (int j = 0; j < components.Length; j++)
                {
                    if (components[j] != null)
                    {
                        CollectFromObject(components[j], path, resolvedVariants, checker);
                    }
                }
            }

            IReadOnlyList<string> issues = checker.Validate();
            checkedCount = checker.CheckedCount;
            return issues;
        }

        /// <summary>
        ///     SourceDataProviderに登録された全CollectionKeyの登録済みIDを、ゲームデータ種別ごとに記録します。
        /// </summary>
        /// <param name="checker"> 記録先です。 </param>
        /// <returns> CollectionKeyごとの、データ本体を解決できたゲームデータ種別です。 </returns>
        private static Dictionary<string, List<GameDataVariant>> CollectRegisteredIds(DataIDIntegrityChecker checker)
        {
            Dictionary<string, List<GameDataVariant>> resolvedVariants = new(StringComparer.Ordinal);
            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> mappings =
                SourceDataProviderSettings.instance.SourceCollectionMappings;

            for (int i = 0; i < mappings.Count; i++)
            {
                string collectionKey = mappings[i].CollectionKey;
                if (string.IsNullOrWhiteSpace(collectionKey) || resolvedVariants.ContainsKey(collectionKey))
                {
                    continue;
                }

                List<GameDataVariant> variants = new();
                resolvedVariants.Add(collectionKey, variants);
                for (int j = 0; j < VARIANTS.Length; j++)
                {
                    GameDataVariant variant = VARIANTS[j];
                    if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                            mappings[i].DataAssetAddressableKey,
                            variant,
                            out _,
                            out _,
                            out _))
                    {
                        continue;
                    }

                    variants.Add(variant);
                    IReadOnlyList<SourceDataIDOption> options =
                        SourceDataProviderRepositoryResolver.GetOptions(collectionKey, variant);
                    for (int k = 0; k < options.Count; k++)
                    {
                        SourceDataIDOption option = options[k];
                        checker.AddRegisteredId(
                            variant.ToString(),
                            collectionKey,
                            option.Id,
                            option.HashId,
                            GetLocation(option.Source));
                    }
                }
            }

            return resolvedVariants;
        }

        /// <summary>
        ///     Unityオブジェクト内のDataIDを、生成側か参照側かに分けて記録します。
        /// </summary>
        /// <param name="target"> 走査対象のUnityオブジェクトです。 </param>
        /// <param name="assetPath"> エラー表示用のアセットパスです。 </param>
        /// <param name="resolvedVariants"> CollectionKeyごとの、データ本体を解決できたゲームデータ種別です。 </param>
        /// <param name="checker"> 記録先です。 </param>
        private static void CollectFromObject(
            UnityEngine.Object target,
            string assetPath,
            Dictionary<string, List<GameDataVariant>> resolvedVariants,
            DataIDIntegrityChecker checker)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty iterator = serializedObject.GetIterator();
            HashSet<long> visitedReferenceIds = new();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = CanEnterChildren(iterator, visitedReferenceIds);
                if (!string.Equals(iterator.type, nameof(DataID), StringComparison.Ordinal)
                    || !SerializedPropertyFieldResolver.TryResolve(
                        target.GetType(),
                        iterator.propertyPath,
                        out FieldInfo fieldInfo))
                {
                    continue;
                }

                string location = $"{assetPath} : {target.name}.{iterator.propertyPath}";
                SerializedProperty idProperty = iterator.FindPropertyRelative(
                    SourceDataProviderRepositoryResolver.ID_PROPERTY_NAME);
                SerializedProperty hashProperty = iterator.FindPropertyRelative(
                    SourceDataProviderRepositoryResolver.HASH_PROPERTY_NAME);
                if (idProperty == null || hashProperty == null)
                {
                    continue;
                }

                string id = idProperty.stringValue;
                int hashId = hashProperty.intValue;
                SourceDataCollectionAttribute attribute = fieldInfo.GetCustomAttribute<SourceDataCollectionAttribute>();
                if (attribute == null)
                {
                    checker.AddIssue($"SourceDataCollection属性がありません。 ({location})");
                    continue;
                }

                string collectionKey = attribute.CollectionKey;
                if (attribute.IsSceneScoped)
                {
                    CollectSceneScoped(serializedObject, collectionKey, id, hashId, location, checker);
                    continue;
                }

                if (!resolvedVariants.TryGetValue(collectionKey, out List<GameDataVariant> variants))
                {
                    checker.AddIssue($"SourceDataProviderにCollectionKey「{collectionKey}」が登録されていません。 ({location})");
                    continue;
                }

                if (IsAuthoring(collectionKey, variants, target, iterator.propertyPath))
                {
                    checker.AddHashOnly(collectionKey, id, hashId, location);
                }
                else
                {
                    checker.AddReference(collectionKey, id, hashId, location);
                }
            }
        }

        /// <summary>
        ///     走査で子要素へ進んでよいか判定します。辿り済みのSerializeReferenceと深すぎる入れ子には進みません。
        /// </summary>
        /// <param name="property"> 判定するプロパティです。 </param>
        /// <param name="visitedReferenceIds"> このオブジェクトで辿り済みのSerializeReferenceのIDです。 </param>
        /// <returns> 子要素へ進む場合はtrueです。 </returns>
        private static bool CanEnterChildren(SerializedProperty property, HashSet<long> visitedReferenceIds)
        {
            if (property.depth >= MAX_PROPERTY_DEPTH)
            {
                return false;
            }

            // BehaviorGraphのノードのように互いを参照し合うSerializeReferenceは、同じ参照先を何度も辿ると
            // 走査が終わらずメモリを使い切る（自動ビルド run #901 で発生）。参照先ごとに一度だけ辿る。
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                return visitedReferenceIds.Add(property.managedReferenceId);
            }

            return true;
        }

        /// <summary>
        ///     シーン内で完結するDataIDを、対象シーンの候補と照合して記録します。
        /// </summary>
        /// <param name="serializedObject"> DataIDを保持するSerializedObjectです。 </param>
        /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
        /// <param name="id"> 人間可読な文字列IDです。 </param>
        /// <param name="hashId"> 焼き込み済みの数値IDです。 </param>
        /// <param name="location"> エラー表示用の保持元です。 </param>
        /// <param name="checker"> 記録先です。 </param>
        private static void CollectSceneScoped(
            SerializedObject serializedObject,
            string collectionKey,
            string id,
            int hashId,
            string location,
            DataIDIntegrityChecker checker)
        {
            checker.AddHashOnly(collectionKey, id, hashId, location);
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            // 対象シーンを解決できない場合は候補を列挙できないため、存在は検証しない（Inspectorの警告と同じ扱い）。
            IReadOnlyList<SourceDataIDOption> options =
                SceneScopedDataIDOptionResolver.GetOptions(collectionKey, serializedObject);
            if (options.Count == 0)
            {
                return;
            }

            for (int i = 0; i < options.Count; i++)
            {
                if (string.Equals(options[i].Id, id, StringComparison.Ordinal))
                {
                    return;
                }
            }

            checker.AddIssue($"参照切れ: {collectionKey} の「{id}」が対象シーン内にありません。 ({location})");
        }

        /// <summary>
        ///     データ本体を解決できたいずれかのゲームデータ種別で、生成側フィールドか判定します。
        /// </summary>
        /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
        /// <param name="variants"> データ本体を解決できたゲームデータ種別です。 </param>
        /// <param name="target"> DataIDを保持するUnityオブジェクトです。 </param>
        /// <param name="propertyPath"> DataIDのプロパティパスです。 </param>
        /// <returns> 生成側フィールドの場合はtrueです。 </returns>
        private static bool IsAuthoring(
            string collectionKey,
            List<GameDataVariant> variants,
            UnityEngine.Object target,
            string propertyPath)
        {
            // データ本体をどの種別でも解決できない場合は、登録の有無を判定できないため生成側として扱う。
            if (variants.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < variants.Count; i++)
            {
                if (SourceDataProviderRepositoryResolver.IsAuthoringProperty(
                        collectionKey,
                        variants[i],
                        target,
                        propertyPath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     検証から除外する研究・モック用のアセットか判定します。
        /// </summary>
        /// <param name="path"> アセットパスです。 </param>
        /// <returns> 除外する場合はtrueです。 </returns>
        private static bool IsExcluded(string path)
        {
            return path.StartsWith(EXCLUDED_FOLDER, StringComparison.Ordinal);
        }

        /// <summary>
        ///     エラー表示用に、オブジェクトのアセットパスと名前を取得します。
        /// </summary>
        /// <param name="source"> 対象のUnityオブジェクトです。 </param>
        /// <returns> 表示用の文字列です。 </returns>
        private static string GetLocation(UnityEngine.Object source)
        {
            if (source == null)
            {
                return "不明";
            }

            return $"{AssetDatabase.GetAssetPath(source)} : {source.name}";
        }

        /// <summary>
        ///     不整合の一覧をログ表示用メッセージに変換します。件数が多い場合は先頭だけ表示します。
        /// </summary>
        /// <param name="issues"> 検出した不整合の一覧です。 </param>
        /// <returns> ログ表示用メッセージです。 </returns>
        private static string CreateErrorMessage(IReadOnlyList<string> issues)
        {
            int count = Math.Min(issues.Count, MAX_REPORTED_ISSUES);
            List<string> lines = new(count);
            for (int i = 0; i < count; i++)
            {
                lines.Add(issues[i]);
            }

            string omitted = issues.Count > count ? $"\n- ほか{issues.Count - count}件" : string.Empty;
            return $"[{nameof(DataIDBuildValidator)}] DataIDの不整合が{issues.Count}件あります。"
                + "修正後に Tools/KillChord/Build/Validate DataIDs で再確認してください。\n- "
                + string.Join("\n- ", lines)
                + omitted;
        }
    }
}
