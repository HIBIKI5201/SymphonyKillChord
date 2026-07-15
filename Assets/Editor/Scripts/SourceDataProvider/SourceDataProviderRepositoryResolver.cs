using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProvider設定からAddressableリポジトリと登録済みDataIDを解決します。
    /// </summary>
    internal static class SourceDataProviderRepositoryResolver
    {
        /// <summary>
        ///     Addressableキーからリポジトリアセットを取得します。
        /// </summary>
        /// <param name="addressableKey"> リポジトリのAddressableキーです。 </param>
        /// <param name="repository"> 解決したリポジトリアセットです。 </param>
        /// <returns> リポジトリアセットを解決できた場合はtrueです。 </returns>
        public static bool TryResolveRepository(string addressableKey, out UnityEngine.Object repository)
        {
            repository = null;
            if (string.IsNullOrWhiteSpace(addressableKey))
            {
                return false;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return false;
            }

            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if (entry == null
                        || !string.Equals(entry.address, addressableKey, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    repository = AssetDatabase.LoadMainAssetAtPath(entry.AssetPath);
                    return repository != null;
                }
            }

            return false;
        }

        /// <summary>
        ///     リポジトリアセットが持つ配列プロパティのパス一覧を取得します。
        /// </summary>
        /// <param name="repository"> 対象リポジトリアセットです。 </param>
        /// <returns> 配列プロパティのパス一覧です。 </returns>
        public static string[] GetArrayPropertyPaths(UnityEngine.Object repository)
        {
            if (repository == null)
            {
                return Array.Empty<string>();
            }

            List<string> paths = new();
            SerializedObject serializedObject = new(repository);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = true;
                if (iterator.isArray && iterator.propertyType != SerializedPropertyType.String)
                {
                    paths.Add(iterator.propertyPath);
                }
            }

            return paths.ToArray();
        }

        /// <summary>
        ///     指定カテゴリへ登録済みのDataID一覧を取得します。
        /// </summary>
        /// <param name="category"> 取得するカテゴリ名です。 </param>
        /// <returns> 登録済みのDataID一覧です。 </returns>
        public static IReadOnlyList<SourceDataIDOption> GetOptions(string category)
        {
            List<SourceDataIDOption> options = new();
            SourceDataProviderSettings settings = SourceDataProviderSettings.instance;
            if (!settings.TryGetMapping(category, out SourceDataProviderSettings.RepositoryMapping mapping)
                || !TryResolveRepository(mapping.AddressableKey, out UnityEngine.Object repository))
            {
                return options;
            }

            HashSet<int> visitedInstanceIds = new();
            SerializedObject serializedObject = new(repository);
            SerializedProperty rootProperty = string.IsNullOrWhiteSpace(mapping.ArrayPropertyPath)
                ? null
                : serializedObject.FindProperty(mapping.ArrayPropertyPath);

            if (rootProperty == null)
            {
                CollectFromObject(repository, category, options, visitedInstanceIds);
            }
            else
            {
                CollectFromProperty(
                    serializedObject,
                    rootProperty,
                    category,
                    options,
                    visitedInstanceIds);
            }

            options.Sort((left, right) => string.Compare(left.Id, right.Id, StringComparison.Ordinal));
            return options;
        }

        /// <summary>
        ///     対象DataIDがリポジトリへ登録される生成側フィールドか判定します。
        /// </summary>
        /// <param name="category"> DataIDのカテゴリ名です。 </param>
        /// <param name="target"> DataIDを保持するUnityオブジェクトです。 </param>
        /// <param name="propertyPath"> DataIDのプロパティパスです。 </param>
        /// <returns> 生成側フィールドの場合はtrueです。 </returns>
        public static bool IsAuthoringProperty(
            string category,
            UnityEngine.Object target,
            string propertyPath)
        {
            SourceDataProviderSettings settings = SourceDataProviderSettings.instance;
            if (!settings.TryGetMapping(category, out SourceDataProviderSettings.RepositoryMapping mapping)
                || !TryResolveRepository(mapping.AddressableKey, out UnityEngine.Object repository))
            {
                return true;
            }

            if (target == repository)
            {
                return string.IsNullOrWhiteSpace(mapping.ArrayPropertyPath)
                    || propertyPath.StartsWith(mapping.ArrayPropertyPath, StringComparison.Ordinal);
            }

            if (string.IsNullOrWhiteSpace(mapping.ArrayPropertyPath))
            {
                return false;
            }

            SerializedObject serializedObject = new(repository);
            SerializedProperty arrayProperty = serializedObject.FindProperty(mapping.ArrayPropertyPath);
            if (arrayProperty == null || !arrayProperty.isArray)
            {
                return false;
            }

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);
                if (element.propertyType == SerializedPropertyType.ObjectReference
                    && element.objectReferenceValue == target)
                {
                    return !propertyPath.Contains(".Array.data[", StringComparison.Ordinal);
                }
            }

            return false;
        }

        /// <summary>
        ///     Unityオブジェクト内から指定カテゴリのDataIDを収集します。
        /// </summary>
        /// <param name="target"> 走査対象のUnityオブジェクトです。 </param>
        /// <param name="category"> 収集するカテゴリ名です。 </param>
        /// <param name="options"> 収集結果です。 </param>
        /// <param name="visitedInstanceIds"> 走査済みオブジェクトのInstanceID一覧です。 </param>
        private static void CollectFromObject(
            UnityEngine.Object target,
            string category,
            List<SourceDataIDOption> options,
            HashSet<int> visitedInstanceIds)
        {
            if (target == null || !visitedInstanceIds.Add(target.GetInstanceID()))
            {
                return;
            }

            SerializedObject serializedObject = new(target);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = true;
                CollectCurrentProperty(
                    serializedObject,
                    iterator,
                    category,
                    options,
                    visitedInstanceIds);
            }
        }

        /// <summary>
        ///     指定プロパティとその子要素からDataIDを収集します。
        /// </summary>
        /// <param name="serializedObject"> プロパティを所有するSerializedObjectです。 </param>
        /// <param name="rootProperty"> 走査起点のプロパティです。 </param>
        /// <param name="category"> 収集するカテゴリ名です。 </param>
        /// <param name="options"> 収集結果です。 </param>
        /// <param name="visitedInstanceIds"> 走査済みオブジェクトのInstanceID一覧です。 </param>
        private static void CollectFromProperty(
            SerializedObject serializedObject,
            SerializedProperty rootProperty,
            string category,
            List<SourceDataIDOption> options,
            HashSet<int> visitedInstanceIds)
        {
            SerializedProperty iterator = rootProperty.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            CollectCurrentProperty(serializedObject, iterator, category, options, visitedInstanceIds);
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren)
                && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = true;
                CollectCurrentProperty(
                    serializedObject,
                    iterator,
                    category,
                    options,
                    visitedInstanceIds);
            }
        }

        /// <summary>
        ///     現在のSerializedPropertyからDataIDまたは参照先オブジェクトを収集します。
        /// </summary>
        /// <param name="serializedObject"> プロパティを所有するSerializedObjectです。 </param>
        /// <param name="property"> 走査対象のプロパティです。 </param>
        /// <param name="category"> 収集するカテゴリ名です。 </param>
        /// <param name="options"> 収集結果です。 </param>
        /// <param name="visitedInstanceIds"> 走査済みオブジェクトのInstanceID一覧です。 </param>
        private static void CollectCurrentProperty(
            SerializedObject serializedObject,
            SerializedProperty property,
            string category,
            List<SourceDataIDOption> options,
            HashSet<int> visitedInstanceIds)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue is ScriptableObject scriptableObject)
                {
                    CollectFromObject(scriptableObject, category, options, visitedInstanceIds);
                }
                return;
            }

            if (!string.Equals(property.type, nameof(DataID), StringComparison.Ordinal)
                || !SerializedPropertyFieldResolver.TryResolve(
                    serializedObject.targetObject.GetType(),
                    property.propertyPath,
                    out FieldInfo fieldInfo))
            {
                return;
            }

            DataCategoryAttribute attribute = fieldInfo.GetCustomAttribute<DataCategoryAttribute>();
            if (attribute == null
                || !string.Equals(attribute.Category, category, StringComparison.Ordinal))
            {
                return;
            }

            SerializedProperty idProperty = property.FindPropertyRelative(ID_PROPERTY_NAME);
            SerializedProperty hashProperty = property.FindPropertyRelative(HASH_PROPERTY_NAME);
            if (idProperty == null
                || hashProperty == null
                || string.IsNullOrWhiteSpace(idProperty.stringValue))
            {
                return;
            }

            options.Add(new SourceDataIDOption(
                idProperty.stringValue,
                hashProperty.intValue,
                serializedObject.targetObject));
        }

        internal const string ID_PROPERTY_NAME = "_id";
        internal const string HASH_PROPERTY_NAME = "_hashId";
    }
}
